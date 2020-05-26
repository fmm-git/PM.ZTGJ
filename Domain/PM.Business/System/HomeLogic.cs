using Dos.Common;
using Dos.ORM;
using PM.Business.Production;
using PM.Business.RawMaterial;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using PM.DataEntity.System.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace PM.Business.System
{
    /// <summary>
    /// 首页
    /// </summary>
    public class HomeLogic
    {
        TbWorkOrderLogic _WorkOrderLogic = new TbWorkOrderLogic();
        OrderProgressLogic _OrderProgressLogic = new OrderProgressLogic();
        TbRawMaterialMonthDemandPlanLogic _RawMaterialMonthDemandPlanLogic = new TbRawMaterialMonthDemandPlanLogic();

        #region 首页

        /// <summary>
        /// 获取年月日数据
        /// </summary>
        /// <returns></returns>
        public DMYDataModel GetDMYData(HomeRequest request)
        {
            request.DayMonth = DateTime.Now;
            //查找订单信息
            var orderList = GetOverReceiveData(request);
            //已接单,加工完成
            var receiveData = orderList.Item1;
            var overData = orderList.Item2;
            var retData = new DMYDataModel()
            {
                YearData = new DataModel(1, receiveData, overData),
                MonthData = new DataModel(2, receiveData, overData),
                DayData = new DataModel(3, receiveData, overData)
            };
            return retData;
        }

        /// <summary>
        /// 订单信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public OrderDataModel GetOrderData(HomeRequest request)
        {
            #region 查询条件

            var wherePS = new Where<TbDistributionPlanInfo>();
            var whereQS = new Where<TbDistributionEntOrder>();

            #endregion

            var retData = new OrderDataModel();
            //订单信息
            var orderInfo = CreatOrderInfo(request);
            if (!orderInfo.Item2.Any()) return retData;
            retData = orderInfo.Item1;
            //配送
            wherePS.And(p => p.OrderCode.In(orderInfo.Item2));
            var psList = Db.Context.From<TbDistributionPlanInfo>().Select(TbDistributionPlanInfo._.DistributionStart).Where(wherePS).ToList();
            retData.Distribution = psList.Where(p => p.DistributionStart == "配送完成").Count();
            retData.NotDistribution = psList.Where(p => p.DistributionStart == "未配送").Count();
            //签收
            whereQS.And(p => p.OrderCode.In(orderInfo.Item2));
            var qsList = Db.Context.From<TbDistributionEntOrder>().Select(TbDistributionEntOrder._.All)
                .LeftJoin<TbDistributionEnt>((a, c) => a.DistributionCode == c.DistributionCode)
                .Where(whereQS).ToList();
            retData.Sign = qsList.Where(p => p.SignState == "已签收").Count();
            retData.NotSign = qsList.Where(p => p.SignState == "未签收").Count();
            return retData;
        }

        /// <summary>
        /// 获取库存信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public RawMaterialStockModel GetRawMaterialStockData(HomeRequest request)
        {
            //建筑钢筋
            var buildingData = GetStockRecordByRebarType(1, request);
            //型钢
            var sectionData = GetStockRecordByRebarType(2, request);
            var retData = new RawMaterialStockModel()
            {
                BuildingSteel = new RebarTypeRetModel(buildingData),
                SectionSteel = new RebarTypeRetModel(sectionData)
            };
            return retData;
        }

        /// <summary>
        /// 获取生产计划阶段数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PlanRawMaterialStockModel GetRawMaterialStockPlanData(HomeRequest request)
        {
            #region 查询条件
            var whereSupply = new Where<TbSupplyList>();
            whereSupply.And(p => p.StateCode != "未供货");
            #endregion

            var retData = new PlanRawMaterialStockModel();
            //月度计划
            var monthList = GetPlanDataList(request);
            if (!monthList.Any()) return retData;
            var monthPlan = new RawMaterialStockModel(monthList);
            retData.MonthPlan = monthPlan;
            //批次计划
            List<string> demandNumList = monthList.Select(p => p.DemandPlanCode).ToList();
            var bpn = monthList.Where(p => p.BatchPlanNum != null).Select(p => p.BatchPlanNum).Distinct().ToList();
            demandNumList.AddRange(bpn);
            var batchList = GetBatchNeedList(request, demandNumList);
            if (!batchList.Any()) return retData;
            var batchPlan = new RawMaterialStockModel(batchList);
            retData.BatchPlan = batchPlan;
            //供应
            var batchPlanNumList = batchList.Select(p => p.BatchPlanNum).ToList();
            whereSupply.And(p => p.BatchPlanNum.In(batchPlanNumList));
            var supplyList = Db.Context.From<TbSupplyList>().Select(
                           TbSupplyList._.SteelsTypeCode.As("RebarType"),
                           TbSupplyList._.HasSupplierTotal.As("TotalWeight"))
                          .Where(whereSupply).ToList<PlanDataModel>();
            var supplyData = new RawMaterialStockModel(supplyList);
            retData.SupplyData = supplyData;
            retData.LabShow();
            return retData;
        }

        /// <summary>
        /// 月度计划统计图
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public MonthPlanReportModel GetMonthPlanReport(HomeRequest request)
        {
            var retData = new MonthPlanReportModel();
            var monthList = GetPlanDataList(request);
            if (!monthList.Any()) return retData;
            List<string> demandNumList = monthList.Select(p => p.DemandPlanCode).ToList();
            var bpn = monthList.Where(p => p.BatchPlanNum != null).Select(p => p.BatchPlanNum).Distinct().ToList();
            demandNumList.AddRange(bpn);
            var batchList = GetBatchNeedList(request, demandNumList);
            if (batchList.Any())
            {
                monthList.ForEach(x =>
                {
                    var batchWeight = batchList.Where(p => p.DemandPlanCode == x.DemandPlanCode || p.DemandPlanCode == x.BatchPlanNum).Sum(p => p.TotalWeight);
                    x.BatchWeight = batchWeight;
                });
            }
            retData = GetMonthPlanLab(monthList);
            return retData;
        }

        /// <summary>
        /// 加工订单统计图
        /// </summary>
        /// <returns></returns>
        public OrderDataModel GetOrderInfoReport(HomeRequest request)
        {
            var retData = new OrderDataModel();
            //订单信息
            var orderInfo = CreatOrderInfo(request);
            if (!orderInfo.Item2.Any()) return retData;
            retData = orderInfo.Item1;
            //配送超期
            var where = new Where<TbDistributionPlanInfo>();
            where.And(p => p.OrderCode.In(orderInfo.Item2));
            where.And(new WhereClip("(DistributionStart='配送完成' and DATEPART(day,DeliveryCompleteTime)>DATEPART(day,DistributionTime)) or (DistributionStart='未配送' and DistributionTime>GETDATE())"));
            var data = Db.Context.From<TbDistributionPlanInfo>()
                .Select(TbDistributionPlanInfo._.ID, TbCompany._.ParentCompanyCode)
                .LeftJoin<TbCompany>((a, c) => a.SiteCode == c.CompanyCode)
                .Where(where).ToList();
            retData.Distribution = data.GroupBy(p => p.ParentCompanyCode).Count();
            return retData;
        }

        /// <summary>
        /// 供货单统计图
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public SupplyDataReportModel GetSupplyDataReport(HomeRequest request)
        {
            var retData = new SupplyDataReportModel();
            //查找月度计划数据
            var monthList = GetPlanDataList(request);
            if (!monthList.Any())
                return retData;
            var supplyList = GetSupplyData(monthList);
            if (!supplyList.Any()) return retData;
            var section = supplyList.Where(p => p.SteelsTypeCode == "SectionSteel").ToList();
            if (section.Any())
                retData.SectionSteel = CreatSupplyRetData(section);
            var building = supplyList.Where(p => p.SteelsTypeCode == "BuildingSteel").ToList();
            if (building.Any())
                retData.BuildingSteel = CreatSupplyRetData(building);
            return retData;
        }

        /// <summary>
        /// 组织机构标签(加工厂)
        /// </summary>
        /// <returns></returns>
        public List<OrgLabData> GetOrgLabDataForFactory(HomeRequest request)
        {
            #region 查询条件

            var where = new Where<TbOrderProgress>();
            ////组织机构
            //if (!string.IsNullOrEmpty(request.CodeList))
            //{
            //    var list = request.CodeList.Split(',').ToList();
            //    if (list.Count < 500)
            //        where.And(p => p.SiteCode.In(list));
            //}
            //组织机构
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> siteCodeList = _WorkOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);
                if (siteCodeList.Count > 0)
                {
                    where.And(p => p.SiteCode.In(siteCodeList));
                }
            }
            if (!string.IsNullOrEmpty(request.ProcessFactoryCode))
                where.And(p => p.ProcessFactoryCode == request.ProcessFactoryCode);

            where.And(new WhereClip("(YEAR(DistributionTime)=" + request.DayMonth.Year + " and MONTH(DistributionTime)=" + request.DayMonth.Month + ") or (DistributionTime<'" + request.DayMonth + "' and (ProcessingState!='Finishing'))"));

            #endregion

            var retData = new List<OrgLabData>();
            var orderList = Db.Context.From<TbOrderProgress>()
               .Select(TbOrderProgress._.ProcessingState,
               TbOrderProgress._.FinishProcessingDateTime,
               TbOrderProgress._.DistributionTime,
               TbOrderProgress._.SiteCode,
               TbCompany._.ParentCompanyCode)
               .LeftJoin<TbCompany>((a, c) => a.SiteCode == c.CompanyCode)
               .Where(where).ToList();
            if (!orderList.Any()) return retData;
            //订单,未完成,滞后
            var orgList = orderList.Select(p => p.SiteCode).Distinct().ToList();
            orgList.ForEach(x =>
            {
                var oList = orderList.Where(p => p.SiteCode == x).ToList();
                var labList = new List<OrgLab>();
                labList.Add(new OrgLab("订单", oList.Count, "blue"));
                labList.Add(new OrgLab("未完成", oList.Count(p => p.ProcessingState != "Finishing"), "#FFB90F"));
                var zh = oList.Count(p => p.ProcessingState != "Finishing" && ((p.FinishProcessingDateTime != null && p.DistributionTime < p.FinishProcessingDateTime)
                                         || (p.FinishProcessingDateTime == null && p.DistributionTime < DateTime.Now.Date)));
                labList.Add(new OrgLab("滞后", zh, "red"));
                var item = new OrgLabData()
                {
                    OrgCode = x,
                    LeveL = 5,
                    LabList = labList
                };
                if (zh > 0)
                    item.pCode = oList[0].ParentCompanyCode;
                retData.Add(item);
            });
            return retData;
        }

        /// <summary>
        /// 组织机构标签(经理部)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public List<OrgLabData> GetOrgLabDataForJLB(HomeRequest request)
        {
            var retData = new List<OrgLabData>();
            //月度计划
            var monthList = GetPlanDataList(request);
            if (!monthList.Any()) return retData;
            //供应清单
            var supplyList = GetSupplyData(monthList);
            //分部
            var branchCode = monthList.Select(p => p.BranchCode).Distinct().ToList();
            //工区
            var workAreaCode = monthList.Select(p => p.WorkAreaCode).Distinct().ToList();
            //站点标签
            var sLab = GetOrgLabDataForFactory(request);
            retData.AddRange(sLab);
            var branchLab = new List<OrgLabData>();
            workAreaCode.ForEach(x =>
            {
                var labList = new List<OrgLab>();
                var blabList = new List<OrgLab>();
                var item = new OrgLabData()
                {
                    OrgCode = x
                };
                var wdata = GetMonthPlanLab(monthList.Where(p => p.WorkAreaCode == x).ToList());
                bool isw = false;
                if (wdata.TooFewCount > 0)
                {
                    isw = true;
                    labList.Add(new OrgLab("计划不足!", 0, "blue"));
                }
                else if (wdata.TooMuchCount > 0)
                {
                    isw = true;
                    labList.Add(new OrgLab("计划过多!", 0, "blue"));
                }
                else if (wdata.OverTime > 0)
                {
                    isw = true;
                    labList.Add(new OrgLab("超时未提交!", 0, "blue"));
                }
                if (isw)
                    blabList.Add(new OrgLab("月度计划!", 0, "blue", 1));
                var iszh = retData.Count(p => p.pCode == x);
                if (iszh > 0)
                {
                    labList.Add(new OrgLab("进度滞后！", 0, "red"));
                    blabList.Add(new OrgLab("进度滞后！", 0, "red", 2));
                    item.pCode = wdata.pCode;
                }

                var sdata = CreatSupplyRetData(supplyList.Where(p => p.WorkAreaCode == x).ToList());
                if (sdata.OverTime.Count > 0)
                {
                    labList.Add(new OrgLab("延期供货！", 0, "#FF60AF"));
                    blabList.Add(new OrgLab("材料供应！", 0, "#FF60AF"));
                }
                else if (sdata.OverTimeNoSupply.Count > 0)
                {
                    labList.Add(new OrgLab("超期未供货！", 0, "#FF60AF"));
                    blabList.Add(new OrgLab("材料供应！", 0, "#FF60AF", 3));
                }
                item.LabList = labList;
                item.LeveL = 4;
                retData.Add(item);
                //查找分部lab
                var bl = branchLab.FirstOrDefault(p => p.OrgCode == wdata.pCode);
                if (bl != null)
                {
                    if (bl.LabList.Count < 3)
                    {
                        blabList.ForEach(j =>
                        {
                            var a = bl.LabList.FirstOrDefault(p => p.Name == j.Name);
                            if (a == null)
                                bl.LabList.Add(j);
                        });
                        bl.LabList = bl.LabList.OrderBy(p => p.Order).ToList();
                    }
                }
                else
                {
                    var bitem = new OrgLabData()
                    {
                        OrgCode = wdata.pCode,
                        LeveL = 3,
                        LabList = blabList
                    };
                    branchLab.Add(bitem);
                }
            });
            retData.AddRange(branchLab);
            return retData;
        }

        #region 二级窗口 更多

        /// <summary>
        /// 历史接单量统计
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public List<MoreReportModel> GetOrderMore(HomeRequest request)
        {
            var where = new Where<TbWorkOrder>();
            //组织机构
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _WorkOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);
                if (SiteList.Count > 0)
                {
                    where.And(p => p.SiteCode.In(SiteList));
                }
            }
            if (!string.IsNullOrEmpty(request.ProcessFactoryCode))
                where.And(p => p.ProcessFactoryCode == request.ProcessFactoryCode);
            where.And(new WhereClip("YEAR(DistributionTime)=" + request.DayMonth.Year));
            var group = new GroupByClip("month(DistributionTime)");
            var select = new Field("convert(varchar,month(DistributionTime))+'月' name,SUM(WeightTotal) as y");
            var orderList = Db.Context.From<TbWorkOrder>().Select(select)
                .Where(where)
                .GroupBy(group).ToList<MoreReportModel>();
            CreatLast(orderList);
            return orderList;
        }
        /// <summary>
        /// 供应量需求量统计
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Tuple<List<MoreReportModel>, List<MoreReportModel>> GetSupplyrMore(HomeRequest request)
        {
            var where = new Where<TbFactoryBatchNeedPlan>();
            var where2 = new Where<TbSupplyList>();
            //组织机构
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _WorkOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 4);
                if (SiteList.Count > 0)
                {
                    where.And(p => p.WorkAreaCode.In(SiteList));
                    where2.And(p => p.WorkAreaCode.In(SiteList));
                }
            }
            if (!string.IsNullOrEmpty(request.ProcessFactoryCode))
            {
                where.And(p => p.ProcessFactoryCode == request.ProcessFactoryCode);
                where2.And(p => p.ProcessFactoryCode == request.ProcessFactoryCode);
            }
            //需求量
            where.And(new WhereClip("YEAR(InsertTime)=" + request.DayMonth.Year));
            var group = new GroupByClip("month(InsertTime)");
            var select = new Field("convert(varchar,month(InsertTime))+'月' name,SUM(BatchPlanTotal) as y");
            var batchList = Db.Context.From<TbFactoryBatchNeedPlan>().Select(select)
                .Where(where)
                .GroupBy(group).ToList<MoreReportModel>();
            CreatLast(batchList);
            //供应量
            where2.And(new WhereClip("YEAR(SupplyCompleteTime)=" + request.DayMonth.Year));
            var group2 = new GroupByClip("month(SupplyCompleteTime)");
            var select2 = new Field("convert(varchar,month(SupplyCompleteTime))+'月' name,SUM(HasSupplierTotal) as y");
            var supplyList = Db.Context.From<TbSupplyList>().Select(select2)
                .Where(where2)
                .GroupBy(group2).ToList<MoreReportModel>();
            CreatLast(supplyList);
            return new Tuple<List<MoreReportModel>, List<MoreReportModel>>(batchList, supplyList);
        }

        #endregion

        #endregion

        #region 加工厂概况页

        public GuideData GetFactoryInfo(HomeRequest request)
        {
            var where = new Where<TbDistributionPlanInfo>();
            where.And(p => p.Examinestatus == "审核完成");
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
                where.And(p => p.ProcessFactoryCode == request.ProcessFactoryCode);
            //订单信息
            var orderList = Db.Context.From<TbDistributionPlanInfo>()
                .Select(TbDistributionPlanInfo._.All, TbCompany._.ParentCompanyCode, TbOrderProgress._.FinishProcessingDateTime)
                .LeftJoin<TbCompany>((a, c) => a.SiteCode == c.CompanyCode)
                .LeftJoin<TbOrderProgress>((a, c) => a.OrderCode == c.OrderCode)
                .Where(where).ToList();
            //本月
            var orderMonth = orderList.Where(p => (p.InsertTime.Value.Year == DateTime.Now.Year && p.InsertTime.Value.Month == DateTime.Now.Month)
                                                  || (p.DistributionTime.Value.Year == DateTime.Now.Year && p.DistributionTime.Value.Month == DateTime.Now.Month)).ToList();
            var monthInfo = CreatGuideData(orderMonth, 1);
            monthInfo.Site = orderMonth.GroupBy(p => p.SiteCode).Count();
            monthInfo.WorkArea = orderMonth.GroupBy(p => p.ParentCompanyCode).Count();
            //综合
            var TotalInfo = CreatGuideData(orderList, 2);
            //月均数,历史月份的总和
            if (orderList.Any())
            {
                var tlist = orderList.OrderBy(p => p.InsertTime).ToList();
                DateTime dt1 = tlist[0].InsertTime.Value;
                DateTime dt2 = tlist[(orderList.Count - 1)].InsertTime.Value;
                int Year = dt2.Year - dt1.Year;
                int Month = ((dt2.Year - dt1.Year) * 12 + (dt2.Month - dt1.Month)) + 1;
                TotalInfo.Site = orderList.Count / Month;
                TotalInfo.TotalWeight = (orderList.Sum(p => p.WeightTotal) / Month).Value;
            }
            //报表
            var reportInfo = new FactoryItmeData();
            string CapacityMonth = DateTime.Now.Year + "-";
            int month = DateTime.Now.Month;
            if (month < 10)
                CapacityMonth += "0" + DateTime.Now.Month;
            else
                CapacityMonth += DateTime.Now.Month;
            var data = _WorkOrderLogic.GetCapacityNum(request.ProcessFactoryCode, CapacityMonth);
            if (data != null && data.Rows.Count > 0)
            {
                reportInfo.TotalRate = (decimal)data.Rows[0]["Capacity"];
                reportInfo.OrderRate = (decimal)data.Rows[0]["WeightSmallPlan"];
                reportInfo.OtherRate = (decimal)data.Rows[0]["ActualLoadNew"];
            }
            //联系人
            var storage = Db.Context.From<TbStorage>()
                .Select(TbStorage._.StorageAdd, TbStorage._.Tel, TbUser._.UserName)
                .LeftJoin<TbUser>((a, c) => a.UserCode == c.UserCode)
                .Where(p => p.ProcessFactoryCode == request.ProcessFactoryCode).First();
            var retData = new GuideData()
            {
                MonthInfo = monthInfo,
                TotalInfo = TotalInfo,
                ReportInfo = reportInfo,
                Tel = storage.Tel,
                Address = storage.StorageAdd,
                Contacts = storage.UserName
            };
            return retData;
        }

        #endregion

        #region Private

        /// <summary>
        /// 获取接单,加工完成
        /// </summary>
        /// <param name="request"></param>
        /// <returns>已接单,加工完成</returns>
        private Tuple<List<TbWorkOrder>, List<TbWorkOrderDetail>> GetOverReceiveData(HomeRequest request)
        {
            var where = new Where<TbWorkOrder>();
            var whereOver = new Where<TbWorkOrderDetail>();
            if (!string.IsNullOrEmpty(request.ProcessFactoryCode))
                where.And(p => p.ProcessFactoryCode == request.ProcessFactoryCode);
            //组织机构
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _WorkOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);
                if (SiteList.Count > 0)
                {
                    where.And(p => p.SiteCode.In(SiteList));
                }
            }
            where.And(new WhereClip("YEAR(TbWorkOrder.InsertTime)=" + request.DayMonth.Year));
            where.And(p => p.ProcessingState != "ConfirmWork");
            //已接单
            var orderList = Db.Context.From<TbWorkOrder>()
                .Select(TbWorkOrder._.WeightTotal, TbWorkOrder._.OrderCode, TbCompany._.ParentCompanyCode, TbWorkOrder._.DistributionTime, TbWorkOrder._.InsertTime)
                .LeftJoin<TbCompany>((a, c) => a.SiteCode == c.CompanyCode)
                .Where(where).ToList();
            var receiveData = orderList.ToList();
            //加工完成
            var oderCodeList = orderList.Select(p => p.OrderCode).ToList();
            whereOver.And(p => p.OrderCode.In(oderCodeList));
            whereOver.And(new WhereClip("YEAR(FinishTime)=" + request.DayMonth.Year));
            var overData = Db.Context.From<TbWorkOrderDetail>().Select(TbWorkOrderDetail._.WeightSmallPlan, TbWorkOrderDetail._.FinishTime, TbWorkOrderDetail._.OrderCode).Where(whereOver).ToList();
            return new Tuple<List<TbWorkOrder>, List<TbWorkOrderDetail>>(receiveData, overData);
        }

        /// <summary>
        /// 接单信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private Tuple<OrderDataModel, List<string>> CreatOrderInfo(HomeRequest request)
        {
            #region 查询条件
            var where = new Where<TbOrderProgress>();
            //组织机构
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _WorkOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);
                if (SiteList.Count > 0)
                {
                    where.And(p => p.SiteCode.In(SiteList));
                }
            }
            if (!string.IsNullOrEmpty(request.ProcessFactoryCode))
                where.And(p => p.ProcessFactoryCode == request.ProcessFactoryCode);

            where.And(new WhereClip("(YEAR(DistributionTime)=" + request.DayMonth.Year + " and MONTH(DistributionTime)=" + request.DayMonth.Month + ") or (DistributionTime<'" + request.DayMonth + "' and (ProcessingState!='Finishing'))"));

            #endregion

            var retData = new OrderDataModel();
            var codeList = new List<string>();
            //订单信息
            var orderList = GetOverReceiveData(request);
            if (orderList.Item1.Any())
            {
                var receiveList = orderList.Item1.Where(p => p.InsertTime.Month == request.DayMonth.Month).ToList();
                retData.OrderCount = receiveList.Count;
                retData.OrderTotal = receiveList.Sum(p => p.WeightTotal);
                retData.TotalCount = orderList.Item1.GroupBy(p => p.ParentCompanyCode).Count();
                codeList.AddRange(receiveList.Select(p => p.OrderCode).ToList());
            }
            if (orderList.Item2.Any())
            {
                var overList = orderList.Item2.Where(p => p.FinishTime.Value.Month == request.DayMonth.Month).ToList();
                retData.OverTotal = overList.Sum(p => p.WeightSmallPlan);
                retData.OverCount = overList.Select(p => p.OrderCode).Distinct().Count();
                codeList.AddRange(overList.Select(p => p.OrderCode).ToList());
            }
            //计划完成
            var planOrderList = Db.Context.From<TbOrderProgress>()
               .Select(TbOrderProgress._.ProcessingState, TbOrderProgress._.OrderCode, TbOrderProgress._.FinishProcessingDateTime, TbOrderProgress._.DistributionTime, TbOrderProgress._.SiteCode)
               .Where(where).ToList();
            var orderCodeList = planOrderList.Select(p => p.OrderCode).ToList();
            codeList.AddRange(orderCodeList);
            var orderDetailList = Repository<TbWorkOrderDetail>.Query(p => p.OrderCode.In(orderCodeList) && p.DaetailWorkStrat == "未加工").ToList();
            if (orderDetailList.Any())
            {
                retData.PlanCount = planOrderList.Count;
                retData.PlanTotal = orderDetailList.Sum(p => p.WeightSmallPlan);
            }
            //未完成
            retData.NotOver = planOrderList.Count(p => p.ProcessingState != "Finishing");
            //滞后
            codeList = codeList.Distinct().ToList();
            var lag = GetLagData(orderCodeList);
            retData.Lag = lag.Number;
            retData.LagCount = lag.TotalCount;
            retData.LagWeight = lag.TotalWeight;
            return new Tuple<OrderDataModel, List<string>>(retData, codeList);
        }

        /// <summary>
        /// 根据不同类型获取库存
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        private List<RebarTypeModel> GetStockRecordByRebarType(int type, HomeRequest request)
        {
            #region 查询条件
            var where = new Where<TbRawMaterialStockRecord>();
            var where2 = new Where<TbWorkOrderDetail>();
            where2.And<TbWorkOrder>((a, c) => c.Examinestatus == "审核完成" && c.ProcessingState == "Received");
            if (!string.IsNullOrEmpty(request.ProcessFactoryCode))
            {
                where.And<TbStorage>((a, c) => c.ProcessFactoryCode == request.ProcessFactoryCode);
                where2.And<TbWorkOrder>((a, c) => c.ProcessFactoryCode == request.ProcessFactoryCode);
            }
            //组织机构
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> workAreaList = _WorkOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 4);
                if (workAreaList.Count > 0)
                    where.And(p => p.WorkAreaCode.In(workAreaList));
                List<string> siteCodeList = _WorkOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);
                if (siteCodeList.Count > 0)
                    where2.And<TbWorkOrder>((a, c) => c.SiteCode.In(siteCodeList));
            }
            if (type == 1)
            {
                where.And<TbRawMaterialArchives>((a, c) => c.RebarType == "BuildingSteel");
                where2.And<TbRawMaterialArchives>((a, c) => c.RebarType == "BuildingSteel");
            }
            else
            {
                where.And<TbRawMaterialArchives>((a, c) => c.RebarType == "SectionSteel");
                where2.And<TbRawMaterialArchives>((a, c) => c.RebarType == "SectionSteel");
            }
            #endregion

            var retList = Db.Context.From<TbRawMaterialStockRecord>().Select(
                          TbRawMaterialStockRecord._.MaterialCode,
                          TbRawMaterialStockRecord._.UseCount.Sum().As("UseCount"),
                          TbRawMaterialStockRecord._.LockCount.Sum().As("LockCount"))
                          .LeftJoin<TbStorage>((a, c) => a.StorageCode == c.StorageCode)
                          .LeftJoin<TbRawMaterialArchives>((a, c) => a.MaterialCode == c.MaterialCode)
                         .Where(where)
                         .GroupBy(p => p.MaterialCode)
                         .ToList<RebarTypeModel>();
            if (retList.Any())
            {
                //查找加工订单使用量
                var orderList = Db.Context.From<TbWorkOrderDetail>().Select(
                           TbWorkOrderDetail._.MaterialCode,
                           TbWorkOrderDetail._.WeightSmallPlan.Sum().As("UseCount"))
                           .LeftJoin<TbWorkOrder>((a, c) => a.OrderCode == c.OrderCode)
                           .LeftJoin<TbRawMaterialArchives>((a, c) => a.MaterialCode == c.MaterialCode)
                          .Where(where2)
                          .GroupBy(p => p.MaterialCode)
                          .ToList<RebarTypeModel>();
                if (orderList.Any())
                {
                    retList.ForEach(x =>
                    {
                        var material = orderList.FirstOrDefault(p => p.MaterialCode == x.MaterialCode);
                        if (material != null)
                            x.UseCount -= material.UseCount;
                    });
                }
            }
            return retList;
        }

        /// <summary>
        /// 获取月度计划数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private List<PlanDataModel> GetPlanDataList(HomeRequest request)
        {
            #region 查询条件
            var whereMonth = new Where<TbRawMaterialMonthDemandPlan>();
            if (!string.IsNullOrEmpty(request.ProcessFactoryCode))
                whereMonth.And(p => p.ProcessFactoryCode == request.ProcessFactoryCode);
            //组织机构
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> workAreaList = _WorkOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 4);
                if (workAreaList.Count > 0)
                {
                    whereMonth.And(p => p.WorkAreaCode.In(workAreaList));
                }
            }
            string demandMonth = request.DayMonth.ToString("yyyy-MM");
            whereMonth.And(p => p.Examinestatus == "审核完成" && p.DemandMonth == demandMonth);
            #endregion

            var monthList = Db.Context.From<TbRawMaterialMonthDemandPlan>().Select(
                           TbRawMaterialMonthDemandPlan._.BranchCode,
                           TbRawMaterialMonthDemandPlan._.WorkAreaCode,
                           TbRawMaterialMonthDemandPlan._.DemandPlanCode,
                           TbRawMaterialMonthDemandPlan._.RebarType,
                           TbRawMaterialMonthDemandPlan._.InsertTime,
                           TbRawMaterialMonthDemandPlan._.PlanTotal.As("TotalWeight"))
                          .Where(whereMonth).ToList<PlanDataModel>();

            if (monthList.Any())
            {
                var codeList = monthList.Select(p => p.DemandPlanCode).ToList();
                var supplyPlanList = Repository<TbRawMaterialMonthDemandSupplyPlan>.Query(p => p.DemandPlanCode.In(codeList)).ToList();
                if (supplyPlanList.Any())
                {
                    supplyPlanList.ForEach(x =>
                    {
                        var item = monthList.First(p => p.DemandPlanCode == x.DemandPlanCode);
                        item.TotalWeight += x.SupplyPlanNum.Value;
                        item.BatchPlanNum = x.SupplyPlanCode;
                    });
                }
            }
            return monthList;
        }
        private MonthPlanReportModel GetMonthPlanLab(List<PlanDataModel> monthList)
        {
            var retData = new MonthPlanReportModel();
            if (!monthList.Any()) return retData;
            retData.TotalCount = monthList.GroupBy(p => p.WorkAreaCode).Count();
            //计划不足
            retData.TooFewCount = monthList.Where(p => p.RetWeight < 0 && (p.RetWeight < -(p.TotalWeight * ((decimal)0.1)))).GroupBy(p => p.WorkAreaCode).Count();
            //计划过多
            retData.TooMuchCount = monthList.Where(p => p.BatchWeight > 0 && p.RetWeight > 0 && (p.RetWeight > (p.TotalWeight * ((decimal)0.1)))).GroupBy(p => p.WorkAreaCode).Count();
            //计划合理
            //retData.EqualCount = monthList.Count(p => p.RetWeight <= (p.TotalWeight * ((decimal)0.1)) || p.RetWeight >= -(p.TotalWeight * ((decimal)0.1)));
            //超时未提交
            retData.OverTime = monthList.Where(p => p.InsertTime.Day > 19).GroupBy(p => p.WorkAreaCode).Count();
            retData.pCode = monthList[0].BranchCode;
            return retData;
        }

        /// <summary>
        /// 获取批次计划数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private List<PlanDataModel> GetBatchNeedList(HomeRequest request, List<string> demandNumList)
        {
            #region 查询条件
            var whereBatch = new Where<TbFactoryBatchNeedPlan>();
            whereBatch.And(p => p.Examinestatus == "审核完成");
            whereBatch.And(p => p.RawMaterialDemandNum.In(demandNumList));
            #endregion

            var batchList = Db.Context.From<TbFactoryBatchNeedPlan>().Select(
                           TbFactoryBatchNeedPlan._.RawMaterialDemandNum.As("DemandPlanCode"),
                           TbFactoryBatchNeedPlan._.BatchPlanNum,
                           TbFactoryBatchNeedPlan._.SteelsTypeCode.As("RebarType"),
                           TbFactoryBatchNeedPlan._.BatchPlanTotal.As("TotalWeight"))
                          .Where(whereBatch).ToList<PlanDataModel>();
            return batchList;
        }

        /// <summary>
        /// 供应单统计图
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type">1按时供货 2延期供货 3超期未供货 4未供货</param>
        /// <returns></returns>
        private RebarTypeRetModel CreatSupplyData(List<TbSupplyList> data, int type)
        {
            var retData = new RebarTypeRetModel();
            //已供货
            if (type == 1 || type == 2)
                data = data.Where(p => p.StateCode != "未供货").ToList();
            //未供货
            if (type == 3 || type == 4)
                data = data.Where(p => p.StateCode == "未供货").ToList();
            //按时
            if (type == 1 || type == 4)
                data = data.Where(p => (p.SupplyCompleteTime != null && p.SupplyDate.Value.Day >= p.SupplyCompleteTime.Value.Day) || p.SupplyDate.Value.Day >= DateTime.Now.Day).ToList();
            //超期
            if (type == 2 || type == 3)
                data = data.Where(p => (p.SupplyCompleteTime != null && p.SupplyDate.Value.Day < p.SupplyCompleteTime.Value.Day) || p.SupplyDate.Value.Day < DateTime.Now.Day).ToList();
            if (type < 3)
                retData.TotalWeight = data.Sum(p => p.HasSupplierTotal.Value);
            if (type >= 3)
                retData.TotalWeight = data.Sum(p => p.BatchPlanTotal.Value);
            retData.Count = data.Count;
            return retData;
        }
        private SupplyDataModel CreatSupplyRetData(List<TbSupplyList> data)
        {
            if (!data.Any()) return new SupplyDataModel();
            var retData = new SupplyDataModel()
            {
                OnTime = CreatSupplyData(data, 1),
                OverTime = CreatSupplyData(data, 2),
                OverTimeNoSupply = CreatSupplyData(data, 3),
                NoSupply = CreatSupplyData(data, 4),
                pCode = data[0].WorkAreaCode
            };
            return retData;
        }
        private List<TbSupplyList> GetSupplyData(List<PlanDataModel> monthList)
        {
            List<string> demandNumList = monthList.Select(p => p.DemandPlanCode).ToList();
            var bpn = monthList.Where(p => p.BatchPlanNum != null).Select(p => p.BatchPlanNum).Distinct().ToList();
            demandNumList.AddRange(bpn);
            var where = new Where<TbSupplyList>();
            where.And(p => p.RawMaterialDemandNum.In(demandNumList));
            var supplyList = Db.Context.From<TbSupplyList>().Select(
                TbSupplyList._.BatchPlanTotal,
                TbSupplyList._.BranchCode,
                TbSupplyList._.WorkAreaCode,
                TbSupplyList._.HasSupplierTotal,
                TbSupplyList._.StateCode,
                TbSupplyList._.SteelsTypeCode,
                TbSupplyList._.SupplyCompleteTime,
                TbSupplyList._.SupplyDate)
                .Where(where).ToList();
            return supplyList;
        }

        /// <summary>
        /// 加工厂概况信息
        /// </summary>
        /// <param name="list"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private FactoryData CreatGuideData(List<TbDistributionPlanInfo> orderList, int type)
        {
            var production = new FactoryItmeData();
            var delivery = new FactoryItmeData();
            var ps = orderList;
            if (type == 1)//本月
            {
                var o = orderList.Where(p => p.InsertTime.Value.Month == DateTime.Now.Month).ToList();
                production.OrderCount = o.Count;
                //加急,滞后
                var zh = o.Where(p => (p.FinishProcessingDateTime.HasValue && p.DistributionTime < p.FinishProcessingDateTime)
                                           || (p.FinishProcessingDateTime == null && p.DistributionTime < DateTime.Now.Date)).ToList();
                production.OrderRate = GetJJData(o);
                production.OtherRate = GetJJData(zh);
                ps = orderList.Where(p => p.DistributionTime.Value.Month == DateTime.Now.Month).ToList();
                //超期配送
                var cqps = ps.Where(p => p.DistributionStart == "配送完成" && p.DistributionTime < p.DeliveryCompleteTime).ToList();
                delivery.OrderCount = cqps.Count;
                delivery.OrderRate = GetJJData(cqps);
                //超期未配送
                var cqwps = ps.Where(p => p.DistributionStart == "未配送" && p.DistributionTime < DateTime.Now.Date).ToList();
                delivery.OtherCount = cqwps.Count;
                delivery.OtherRate = GetJJData(cqwps);
                //生产及时率=按时完成数/应加工完成数
                production.TotalRate = GetjslData(ps);
            }
            else
            {
                delivery.OrderCount = orderList.Count(p => p.DistributionStart == "配送完成");
                production.OrderCount = orderList.Count(p => p.ProcessingState == "Finishing");
                production.OrderRate = orderList.Where(p => p.ProcessingState == "Finishing").Sum(p => p.WeightTotal).Value;

                //生产及时率=按时完成数/应加工完成数
                var tps = orderList.Where(p => p.DistributionTime.Value < DateTime.Now.Date).ToList();
                production.TotalRate = GetjslData(tps);
            }

            //准时配送率=按期配送数 /（累计配送数+超期未配送数）
            decimal asps = ps.Where(p => p.DistributionStart == "配送完成" && p.DistributionTime >= p.DeliveryCompleteTime).Count();
            decimal ljps = ps.Where(p => p.DistributionStart == "配送完成").Count();
            decimal cqwpsc = ps.Where(p => p.DistributionStart == "未配送" && p.DistributionTime < DateTime.Now.Date).Count();
            if (delivery.OrderCount > 0)
                delivery.TotalRate = decimal.Round((asps / (ljps + cqwpsc)) * 100, 2);//配送准时率

            var retData = new FactoryData()
            {
                Production = production,
                Delivery = delivery
            };
            return retData;
        }
        /// <summary>
        /// 加急数据
        /// </summary>
        private decimal GetJJData(List<TbDistributionPlanInfo> list)
        {
            if (!list.Any()) return 0;
            decimal jj = list.Where(p => p.UrgentDegree == "Urgent").Count();
            return decimal.Round((jj / list.Count) * 100, 0);
        }
        private void CreatLast(List<MoreReportModel> list)
        {
            var c = list.Count(p => p.name.Contains("12"));
            if (c == 0)
            {
                var item = new MoreReportModel()
                {
                    name = "12月",
                    y = 0
                };
                list.Add(item);
            }
        }
        /// <summary>
        /// 生产及时率
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private decimal GetjslData(List<TbDistributionPlanInfo> list)
        {
            if (!list.Any()) return 0;
            decimal pCunt = list.Count;
            var fc = list.Count(p => p.ProcessingState == "Finishing" && p.DistributionTime >= p.FinishProcessingDateTime);
            return decimal.Round((fc / pCunt) * 100, 2);
        }

        /// <summary>
        /// 订单滞后数据
        /// </summary>
        /// <param name="codeList"></param>
        /// <returns></returns>
        private LagDataModel GetLagData(List<string> codeList)
        {
            var where = new Where<TbOrderProgress>();
            where.And(p => p.OrderCode.In(codeList));
            where.And(p => p.ProcessingState != "Finishing" );
            where.And(p => (p.FinishProcessingDateTime.IsNotNull() && p.DistributionTime < p.FinishProcessingDateTime)
                         || (p.FinishProcessingDateTime.IsNull() && p.DistributionTime < DateTime.Now.Date));
            var list = Db.Context.From<TbOrderProgress>().Select(
                           TbOrderProgress._.WeightTotal.As("TotalWeight"),
                           TbCompany._.ParentCompanyCode)
                          .LeftJoin<TbCompany>((a, c) => a.SiteCode == c.CompanyCode)
                          .Where(where).ToList<LagDataModel>();
            var retData = new LagDataModel()
            {
                Number = list.Count(),
                TotalCount = list.GroupBy(p => p.ParentCompanyCode).Count(),
                TotalWeight = list.Sum(p => p.TotalWeight)
            };
            return retData;
        }
        #endregion
    }
}
