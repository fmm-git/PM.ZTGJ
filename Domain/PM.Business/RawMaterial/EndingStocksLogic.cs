using Dos.ORM;
using PM.Business.Production;
using PM.Business.ShortMessage;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using PM.DataEntity.RawMaterial.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Business.RawMaterial
{
    /// <summary>
    /// 期末库存 逻辑处理 
    /// </summary>
    public class EndingStocksLogic
    {
        private readonly TbWorkOrderLogic workOrderLogic = new TbWorkOrderLogic();
        //发送短信
        CensusdemoTask ct = new CensusdemoTask();
        string IsFormalSystem = ConfigurationManager.AppSettings["IsFormalSystem"];
        public PageModel GetMaterials(FPiCiXQPlan ent)
        {
            string whereTb = " where 1=1 ";
            string where = " ";
            string where1 = " ";
            if (!string.IsNullOrWhiteSpace(ent.SiteCode))
            {
                List<string> SiteList = workOrderLogic.GetCompanyWorkAreaOrSiteList(ent.SiteCode, 5);//站点
                List<string> WorkAreaList = workOrderLogic.GetCompanyWorkAreaOrSiteList(ent.SiteCode, 4);//工区
                string siteStr = string.Join("','", SiteList);
                string workAreaStr = string.Join("','", WorkAreaList);
                where += (" and (tb.SiteCode in('" + siteStr + "') or tb.WorkAreaCode in('" + workAreaStr + "'))");
                where1 += (" and tb.SiteCode in('" + siteStr + "') ");
            }

            if (!string.IsNullOrWhiteSpace(ent.MaterialCode))
            {
                whereTb += " and Tb.MaterialCode='" + ent.MaterialCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(ent.ProjectId))
            {
                whereTb += " and Tb.ProjectId='" + ent.ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(ent.ProcessFactoryCode))
            {
                whereTb += " and Tb.ProcessFactoryCode='" + ent.ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(ent.SpecificationModel1))
            {
                whereTb += " and Tb.SpecificationModel='" + ent.SpecificationModel1 + "'";
            }
            if (!string.IsNullOrWhiteSpace(ent.MaterialNameSelect1))
            {
                whereTb += " and Tb.MaterialName='" + ent.MaterialNameSelect1 + "'";
            }
            string sql = @"select * from (select Tb1.*,isnull(ydxqjh.LjDemandNum,0) as LjDemandNum,isnull(LjBatchPlanQuantity,0) as LjBatchPlanQuantity,isnull(LjHasSupplier,0) as LjHasSupplier,isnull(jgdd.HistoryMonthCount,0) as HistoryMonthCount,isnull(dhrk.PassCount,0) as AcceptanceQuantity,isnull(yclll.WeightSmallPlanN,0) as WeightSmallPlan,(isnull(yclll.WeightSmallPlanN,0)-ISNULL(yclll.WeightSmallPlan,0)-isnull(ylk.yll,0)) as Loss,case when isnull(yclll.WeightSmallPlanN,0)<=0 or isnull(flk.StartAddUpHandleQuantity,0)+isnull(flk.UntreatedQuantity,0)<=0 then Convert(decimal(18,2),0) else Convert(decimal(18,2),((isnull(flk.StartAddUpHandleQuantity,0)+isnull(flk.UntreatedQuantity,0))*1.0/isnull(yclll.WeightSmallPlanN,0))*100) end as LossPC,(isnull(Tb1.InitialCount,0)+isnull(dhrk.PassCount,0)-isnull(yclll.WeightSmallPlanN,0)) as RawMaterialStockQuantity,isnull(yclll.WeightSmallPlan,0) as ComponentStockQuantity,isnull(flk.StartAddUpHandleQuantity,0) as StartAddUpHandleQuantity,isnull(flk.UntreatedQuantity,0) as UntreatedQuantity from (
select tb.MaterialCode,tb.MaterialName,tb.SpecificationModel,sdd.DictionaryText as MeasurementUnitName,sum(tb.Count) as InitialCount,tb.ProjectId,s.ProcessFactoryCode from TbRawMaterialStockRecord tb
left join TbRawMaterialArchives rma on tb.MaterialCode=rma.MaterialCode
left join TbSysDictionaryData sdd on rma.MeasurementUnit=sdd.DictionaryCode
left join TbStorage s on s.StorageCode=tb.StorageCode
where 1=1 and tb.ChackState=1 " + where + @"
group by tb.MaterialCode,tb.MaterialName,tb.SpecificationModel,sdd.DictionaryText,tb.ProjectId,s.ProcessFactoryCode) Tb1
left join(select sum(TbJh.LjDemandNum) as LjDemandNum,TbJh.MaterialCode,TbJh.ProjectId,TbJh.ProcessFactoryCode from(
         select isnull(sum(DemandNum),0) as LjDemandNum,MaterialCode,tb.ProjectId,tb.ProcessFactoryCode from TbRawMaterialMonthDemandPlanDetail a
         left join TbRawMaterialMonthDemandPlan tb on a.DemandPlanCode=tb.DemandPlanCode where tb.Examinestatus='审核完成' " + where + @" 
         group by MaterialCode,tb.ProjectId,tb.ProcessFactoryCode
         union all 
         select isnull(sum(SupplyNum),0) as LjDemandNum,MaterialCode ,tb.ProjectId,tb.ProcessFactoryCode from TbRawMaterialMonthDemandSupplyPlanDetail a
         left join TbRawMaterialMonthDemandSupplyPlan tb on a.SupplyPlanCode=tb.SupplyPlanCode where tb.Examinestatus='审核完成' " + where + @"
         group by MaterialCode,tb.ProjectId,tb.ProcessFactoryCode) TbJh GROUP BY TbJh.MaterialCode,TbJh.ProjectId,TbJh.ProcessFactoryCode) ydxqjh on Tb1.MaterialCode=ydxqjh.MaterialCode and Tb1.ProjectId=ydxqjh.ProjectId and Tb1.ProcessFactoryCode=ydxqjh.ProcessFactoryCode
left join(select sum(BatchPlanQuantity) as LjBatchPlanQuantity,RawMaterialNum,tb.ProjectId,tb.ProcessFactoryCode from TbFactoryBatchNeedPlanItem a
         left join TbFactoryBatchNeedPlan tb on a.BatchPlanNum=tb.BatchPlanNum where tb.Examinestatus='审核完成' " + where + @"
         group by RawMaterialNum,tb.ProjectId,tb.ProcessFactoryCode) pcjh on pcjh.RawMaterialNum=Tb1.MaterialCode and pcjh.ProjectId=Tb1.ProjectId and pcjh.ProcessFactoryCode=Tb1.ProcessFactoryCodeleft join(select sum(HasSupplier) as LjHasSupplier,RawMaterialNum,tb.ProjectId,tb.ProcessFactoryCode from TbSupplyListDetail a         left join TbSupplyList tb on a.BatchPlanNum=tb.BatchPlanNum where 1=1 " + where + @"
         group by RawMaterialNum,tb.ProjectId,tb.ProcessFactoryCode) gh on gh.RawMaterialNum=Tb1.MaterialCode and gh.ProjectId=Tb1.ProjectId and gh.ProcessFactoryCode=Tb1.ProcessFactoryCode
left join(select sum(WeightSmallPlan) HistoryMonthCount,MaterialCode,tb.ProjectId,tb.ProcessFactoryCode from TbWorkOrderDetail a
         left join TbWorkOrder tb on a.OrderCode=tb.OrderCode
         where DaetailWorkStrat='加工完成' " + where1 + @"  
         group by MaterialCode,tb.ProjectId,tb.ProcessFactoryCode) jgdd on jgdd.MaterialCode=Tb1.MaterialCode and jgdd.ProjectId=Tb1.ProjectId and jgdd.ProcessFactoryCode=Tb1.ProcessFactoryCode
left join (select inoi.MaterialCode,isnull(sum(inoi.PassCount),0) as PassCount,tb.ProjectId,fbnp.ProcessFactoryCode from TbInOrder tb
left join TbInOrderItem inoi on tb.InOrderCode=inoi.InOrderCode 
left join TbFactoryBatchNeedPlan fbnp on tb.BatchPlanCode=fbnp.BatchPlanNum
where 1=1 and tb.BatchPlanCode!='' and inoi.MaterialCode!='' and inoi.MaterialCode is not null " + where + @"
group by inoi.MaterialCode,tb.ProjectId,fbnp.ProcessFactoryCode) as dhrk on Tb1.MaterialCode=dhrk.MaterialCode and dhrk.ProjectId=Tb1.ProjectId and dhrk.ProcessFactoryCode=Tb1.ProcessFactoryCode
left join(select rpmd.MaterialCode,SUM(rpmd.WeightSmallPlan) as WeightSmallPlan,sum(rpmd.WeightSmallPlanN) as WeightSmallPlanN,tb.ProjectId,tb.ProcessFactoryCode from TbRMProductionMaterial tb
left join TbRMProductionMaterialDetail rpmd on tb.CollarCode=rpmd.CollarCode
where 1=1 " + where + @"
group by rpmd.MaterialCode,tb.ProjectId,tb.ProcessFactoryCode) as yclll on Tb1.MaterialCode=yclll.MaterialCode   and yclll.ProjectId=Tb1.ProjectId and yclll.ProcessFactoryCode=Tb1.ProcessFactoryCode
left join (select tb.MaterialCode,SUM(tb.WeightYes) as StartAddUpHandleQuantity,SUM(tb.WeightNot) as UntreatedQuantity,tb.ProjectId,rm.ProcessFactoryCode from TbRubbishStock tb 
left join TbRMProductionMaterial rm on tb.CollarCode=rm.CollarCode
where 1=1 " + where1 + @"
group by tb.MaterialCode,tb.ProjectId,rm.ProcessFactoryCode) as flk on flk.MaterialCode=Tb1.MaterialCode and flk.ProjectId=Tb1.ProjectId and flk.ProcessFactoryCode=Tb1.ProcessFactoryCode
left join (select tb.MaterialCode,SUM(tb.Weight) as yll,tb.ProjectId,tb.ProcessFactoryCode from TbCloutStock tb 
left join TbRMProductionMaterial rm on tb.CollarCode=rm.CollarCode
where 1=1 and tb.SiteCode!='-1' " + where1 + @"
group by tb.MaterialCode,tb.ProjectId,tb.ProcessFactoryCode) as ylk on ylk.MaterialCode=Tb1.MaterialCode and ylk.ProjectId=Tb1.ProjectId and ylk.ProcessFactoryCode=Tb1.ProcessFactoryCode) Tb";
            //参数化
            List<Dos.ORM.Parameter> para = new List<Dos.ORM.Parameter>();
            var data = Repository<TbRawMaterialStock>.FromSqlToPageTable(sql + whereTb, para, ent.rows, ent.page, "MaterialCode", "desc");
            return data;

        }

        /// <summary>
        /// 原材料总库存用量统计
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public MaterialTotalStockReportRetModel GetMaterialTotalStockReport(EndingStocksRequest request)
        {
            SqlParameter[] par = new SqlParameter[] 
            {
                    new SqlParameter("@ProcessFactoryCode",request.ProcessFactoryCode),
                    new SqlParameter("@MaterialNames",request.MaterialNameSelect),  
                    new SqlParameter("@SpecificationModel",request.SpecificationModel),
            };
            var ret = new MaterialTotalStockReportRetModel();
            try
            {
                var list = Db.Context.FromProc("MaterialTotalStockReport_Proc").AddParameter(par).ToList<MaterialTotalStockReportModel>();
                if (list.Count > 0)
                {
                    if (string.IsNullOrEmpty(request.ProcessFactoryCode))
                    {
                        list = list.GroupBy(p => new { p.MaterialCode, p.MaterialName, p.SpecificationModel })
                            .Select(p => new MaterialTotalStockReportModel
                            {
                                MaterialCode = p.Key.MaterialCode,
                                MaterialName = p.Key.MaterialName,
                                SpecificationModel = p.Key.SpecificationModel,
                                WeightSmallPlan = p.Sum(y => y.WeightSmallPlan),
                                Count = p.Sum(y => y.Count)
                            }).ToList();
                    }
                    ret.Name = list.Select(p => p.MaterialName + " " + p.SpecificationModel).ToList();
                    var dataList1 = new List<DataModel>();
                    var dataList2 = new List<DataModel>();
                    list.ForEach(x =>
                    {
                        var item = new DataModel()
                        {
                            y = x.Count,
                            code = x.MaterialCode,
                            point = -1,
                            RebarType = x.RebarType
                        };
                        dataList1.Add(item);
                        var item2 = new DataModel()
                        {
                            y = x.WeightSmallPlan,
                            code = x.MaterialCode,
                            point = x.Point,
                            RebarType = x.RebarType
                        };
                        dataList2.Add(item2);
                    });
                    decimal xg1 = 0;
                    decimal jzgj1 = 0;
                    if (dataList1.Count > 0)
                    {
                        xg1 = dataList1.Where(p => p.RebarType == "SectionSteel").Sum(p => p.y);
                        jzgj1 = dataList1.Where(p => p.RebarType == "BuildingSteel").Sum(p => p.y);
                    }
                    decimal xg2 = 0;
                    decimal jzgj2 = 0;
                    if (dataList2.Count > 0)
                    {
                        xg2 = dataList2.Where(p => p.RebarType == "SectionSteel").Sum(p => p.y);
                        jzgj2 = dataList2.Where(p => p.RebarType == "BuildingSteel").Sum(p => p.y);
                    }

                    var sumRebarType = new RebarTypeDataModel()
                   {
                       xg1 = xg1,
                       xg2 = xg2,
                       jzgj1 = jzgj1,
                       jzgj2 = jzgj2
                   };
                    ret.SumRebarTypeData = sumRebarType;
                    ret.Data1 = dataList1;
                    ret.Data2 = dataList2;
                }
                return ret;
            }
            catch (Exception)
            {
                return ret;
            }
        }

        /// <summary>
        /// 原材料总库存及订单需求量历史分析
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public MaterialTotalHistoryStockRetModel GetMaterialTotalHistoryStockReport(EndingStocksRequest request)
        {
            SqlParameter[] par = new SqlParameter[] 
            {
                    new SqlParameter("@ProcessFactoryCode",request.ProcessFactoryCode),
                    new SqlParameter("@MaterialCode",request.MaterialCode),  
                    new SqlParameter("@MaterialNames",request.MaterialNameSelect),  
                    new SqlParameter("@SpecificationModel",request.SpecificationModel),
                    new SqlParameter("@Year",request.Year),     
                    new SqlParameter("@Month",request.Month),
            };
            var ret = new MaterialTotalHistoryStockRetModel();
            try
            {
                var list = Db.Context.FromProc("MaterialTotalStockHistoryReport_Proc").AddParameter(par).ToList<MaterialTotalHistoryStockReportModel>();
                if (list.Count > 0)
                {
                    if (string.IsNullOrEmpty(request.MaterialCode))
                    {
                        list = list.GroupBy(p => new { p.Yar, p.Mth, p.Dayi })
                            .Select(p => new MaterialTotalHistoryStockReportModel
                            {
                                Yar = p.Key.Yar,
                                Mth = p.Key.Mth,
                                Dayi = p.Key.Dayi,
                                WeightSmallPlan = p.Sum(y => y.WeightSmallPlan),
                                Count = p.Sum(y => y.Count)
                            }).ToList();
                    }

                    if (request.Month == 0)
                        ret.Name = list.Select(p => p.Mth).ToList();
                    else
                        ret.Name = list.Select(p => p.Dayi).ToList();
                    ret.Data1 = list.Select(p => p.Count).ToList();
                    ret.Data2 = list.Select(p => p.WeightSmallPlan).ToList();
                }
                return ret;
            }
            catch (Exception)
            {
                return ret;
            }
        }

        /// <summary>
        /// 原材料用量总排行
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public MaterialRankingListReportRetModel GetMaterialRankingListReport(EndingStocksRequest request)
        {
            SqlParameter[] par = new SqlParameter[] 
            {
                    new SqlParameter("@ProcessFactoryCode",request.ProcessFactoryCode), 
                    new SqlParameter("@MaterialNames",request.MaterialNameSelect),  
                    new SqlParameter("@SpecificationModel",request.SpecificationModel),
                    new SqlParameter("@Year",request.Year),     
                    new SqlParameter("@Month",request.Month),
            };
            var ret = new MaterialRankingListReportRetModel();
            try
            {
                var list = Db.Context.FromProc("MaterialRankingListReport_Proc").AddParameter(par).ToList<MaterialRankingListReportModel>();
                if (list.Count > 0)
                {
                    ret.Name = list.Select(p => p.MaterialName + " " + p.SpecificationModel).ToList();
                    var dataList = new List<DataModel>();
                    list.ForEach(x =>
                    {
                        var item = new DataModel();
                        item.y = x.WeightSmallPlan;
                        item.code = x.MaterialCode;
                        dataList.Add(item);
                    });
                    ret.Data = dataList;
                }
                return ret;
            }
            catch (Exception)
            {
                return ret;
            }
        }

        /// <summary>
        /// 加工工艺用料总排行
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ProcessingRankingRetModel GetProcessingRankingListReport(EndingStocksRequest request)
        {
            SqlParameter[] par = new SqlParameter[] 
            {
                    new SqlParameter("@ProcessFactoryCode",request.ProcessFactoryCode),
                    new SqlParameter("@MaterialCode",request.MaterialCode),
                    new SqlParameter("@MaterialNames",request.MaterialNameSelect),  
                    new SqlParameter("@SpecificationModel",request.SpecificationModel),
                    new SqlParameter("@Year",request.Year),     
                    new SqlParameter("@Month",request.Month),
            };
            var ret = new ProcessingRankingRetModel();
            try
            {
                var list = Db.Context.FromProc("ProcessingRankingListReport_Proc").AddParameter(par).ToList<ProcessingRankingReportModel>();
                if (list.Count > 0)
                {
                    ret.Name = list.Select(p => p.ProcessingTechnologyName).ToList();
                    ret.Data = list.Select(p => p.WeightSmallPlan).ToList();
                }
                return ret;
            }
            catch (Exception)
            {
                return ret;
            }
        }

        /// <summary>
        ///材料月度计划量及批次计划量统计
        /// </summary>
        /// <returns></returns>
        public DataTable GetMonthBatchPlanReport(EndingStocksRequest request)
        {
            string where = " WHERE a.Examinestatus='审核完成' ";
            if (request.Year > 0)
            {
                where += " and YEAR(a.insertTime)=" + request.Year;
                if (request.Month > 0)
                    where += " and MONTH(a.insertTime)=" + request.Month;
            }
            string sql = @"SELECT
                        	ISNULL(dbo.GetCompanyParentName_fun(tc.CompanyCode),tc.CompanyFullName) AS CompanyName, 
                        	 (
                        	 	SELECT ISNULL(SUM(a.PlanTotal+ISNULL(b.SupplyPlanNum,0)),0) AS PlanTotal 
								FROM TbRawMaterialMonthDemandPlan a
								LEFT JOIN TbRawMaterialMonthDemandSupplyPlan b ON a.DemandPlanCode=b.DemandPlanCode AND b.IsSupply=0 AND b.Examinestatus='审核完成'
								@WHERE
                        		AND a.WorkAreaCode IN
                        		(
                        		  SELECT CompanyCode from dbo.GetCompanyChild_fun(tc.CompanyCode) WHERE OrgType=4
                        		)
                        	 ) PlanTotal, --月度计划量
                        	 (
                        		SELECT ISNULL(sum(BatchPlanTotal),0) AS BatchPlanTotal 
								FROM TbFactoryBatchNeedPlan a
								@WHERE
                        		AND 
                        		a.WorkAreaCode IN
                        		(
                        		  SELECT CompanyCode from dbo.GetCompanyChild_fun(tc.CompanyCode) WHERE OrgType=4
                        		) 
                        	 )BatchPlanTotal --批次计划量
                        	FROM TbCompany tc
                        	LEFT JOIN TbProjectCompany b ON tc.CompanyCode=b.CompanyCode
                        	WHERE 
                        	tc.OrgType=@OrgType 
                        	AND b.ProjectId=@ProjectId
                        	ORDER BY PlanTotal desc";
            sql = sql.Replace("@WHERE", where);
            sql = sql.Replace("@ProjectId", request.ProjectId);
            sql = sql.Replace("@OrgType", request.OrgType);
            var retData = Db.Context.FromSql(sql).ToDataTable();
            return retData;
        }


        /// <summary>
        /// 已接收（还未领料）订单中的某种材料汇总重量≥当前库存重量的80%
        /// （即这些订单领料后，此种材料库存量会小于等于现有库存量的80%），系统发送预警信息给加工厂材料负责人，并以短信通知。
        /// </summary>
        public void SendMsgForMaterialStockByOrder(int orderId)
        {
            //查找订单信息
            var order = Repository<TbWorkOrder>.First(p => p.ID == orderId);
            if (order != null)
            {
                //查找订单明细信息
                var materialCodeList = Repository<TbWorkOrderDetail>.Query(p => p.OrderCode == order.OrderCode).Select(p => p.MaterialCode).ToList();
                string MaterialCodes = string.Join("','", materialCodeList);
                SqlParameter[] par = new SqlParameter[] 
            {
                    new SqlParameter("@MaterialCodeS",MaterialCodes), 
                    new SqlParameter("@ProcessFactoryCode",order.ProcessFactoryCode), 
            };
                try
                {
                    var list = Db.Context.FromProc("MaterialTotalStockReport_Proc").AddParameter(par).ToList<MaterialTotalStockReportModel>();
                    if (list.Count > 0)
                    {
                        //筛选大于80%的数据,发送预警信息给加工厂材料负责人，并以短信通知
                        var dataList = list.Where(p => p.Point >= 80).ToList();
                        if (dataList.Count > 0)
                        {
                            //查找出材料负责人
                            var storageCodeList = dataList.Select(p => p.StorageCode).Distinct().ToList();
                            var storageInfo = Repository<TbStorage>.Query(p => p.StorageCode.In(storageCodeList)).ToList();
                            List<TbFlowPerformMessage> myMsgList = new List<TbFlowPerformMessage>();//'我的消息'推送
                            //List<TbSMSAlert> listsms = new List<TbSMSAlert>();  //短信信息
                            //查找短信模板信息
                            var smsTemp =
                                Repository<TbShortMessageTemplate>.First(p => p.TemplateCode == "MB0010");
                            dataList.ForEach(x =>
                            {
                                //短信内容
                                string content = smsTemp.TemplateContent;
                                var s = content.Replace("变量：材料名称，规格", x.MaterialName + "," + x.SpecificationModel);
                                string sql = @"select tb1.ProcessFactoryCode,tb2.PersonnelSource,tb2.PersonnelCode from TbWorkOrder tb1
left join (select tb1.ProcessFactoryCode,tb3.PersonnelSource,tb3.PersonnelCode from TbFormEarlyWarningBegTime tb1
left join TbFormEarlyWarningNode tb2 on tb1.EarlyWarningCode=tb2.EarlyWarningCode
left join TbFormEarlyWarningNodePersonnel tb3 on tb2.EarlyWarningCode=tb3.EarlyWarningCode and tb2.EWNodeCode=tb3.EWNodeCode
where tb1.MenuCode='WorkOrder') tb2 on tb1.ProcessFactoryCode=tb2.ProcessFactoryCode
where tb1.OrderCode='" + order.OrderCode + "'";
                                DataTable dt = Db.Context.FromSql(sql).ToDataTable();
                                if (dt.Rows.Count > 0)
                                {
                                    for (int i = 0; i < dt.Rows.Count; i++)
                                    {
                                        var myMsg = new TbFlowPerformMessage()
                                        {
                                            messageID = Guid.NewGuid().ToString(),
                                            messageCreateTime = DateTime.Now,
                                            messageType = 11,
                                            messageTitle = "【" + x.MaterialName + "、" + x.SpecificationModel + "】原材料库存预警提醒",
                                            messageContent = s,
                                            IsRead = -1,
                                            UserCode = dt.Rows[i]["PersonnelCode"].ToString(),
                                            MsgType = "2"
                                        };
                                        myMsgList.Add(myMsg);
                                    }
                                }
                                //var storage = storageInfo.First(p => p.StorageCode == x.StorageCode);
                                //'我的消息'推送

                                //if (IsFormalSystem == "true")
                                //{
                                //    //发送短信信息
                                //    TbSMSAlert tbsms = new TbSMSAlert()
                                //    {
                                //        InsertTime = DateTime.Now,
                                //        ManagerDepartment = storage.ProcessFactoryCode,
                                //        UserCode = storage.UserCode,
                                //        UserTel = storage.Tel,
                                //        BusinessCode = smsTemp.TemplateCode,
                                //        DataCode = x.MaterialCode,
                                //        FromCode = "WorkOrder"
                                //    };
                                //    //短信内容
                                //    string content = smsTemp.TemplateContent;
                                //    tbsms.ShortContent = content.Replace("变量：材料名称/规格", x.MaterialName + "/" + x.SpecificationModel);
                                //    listsms.Add(tbsms);
                                //}
                            });
                            ////调用短信接口发送短信
                            //for (int m = 0; m < listsms.Count; m++)
                            //{
                            //    var dx = ct.ShortMessagePC(listsms[m].UserTel, listsms[m].ShortContent);
                            //    var jObject = Newtonsoft.Json.Linq.JObject.Parse(dx);
                            //    listsms[m].DXType = jObject["data"][0]["code"].ToString();
                            //}
                            using (DbTrans trans = Db.Context.BeginTransaction())
                            {
                                if (myMsgList.Any())
                                {
                                    //'我的消息'推送
                                    Repository<TbFlowPerformMessage>.Insert(trans, myMsgList);
                                }
                                ////向短信信息表中插入数据
                                //if (listsms.Any())
                                //    Repository<TbSMSAlert>.Insert(trans, listsms);
                                trans.Commit();
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        /// <summary>
        /// 已接收（还未领料）订单中的某种材料汇总重量≥当前库存重量的80%
        /// （即这些订单领料后，此种材料库存量会小于等于现有库存量的80%），系统发送预警信息给加工厂材料负责人，并以短信通知。
        /// </summary>
        public bool SendMsgForMaterialStockByOrderNew(int orderId)
        {
            try
            {

                List<TbFormEarlyWarningNodeInfo> myMsgList = new List<TbFormEarlyWarningNodeInfo>();//Pc端推送
                List<TbSMSAlert> myDxList = new List<TbSMSAlert>();//App端
                //查找订单信息
                var order = Repository<TbWorkOrder>.First(p => p.ID == orderId);
                if (order != null)
                {
                    //查找订单明细信息
                    var materialCodeList = Repository<TbWorkOrderDetail>.Query(p => p.OrderCode == order.OrderCode).Select(p => p.MaterialCode).ToList();
                    string MaterialCodes = string.Join("','", materialCodeList);
                    SqlParameter[] par = new SqlParameter[] 
                    {
                            new SqlParameter("@MaterialCodeS",MaterialCodes), 
                            new SqlParameter("@ProcessFactoryCode",order.ProcessFactoryCode), 
                    };
                    var list = Db.Context.FromProc("MaterialTotalStockReport_Proc").AddParameter(par).ToList<MaterialTotalStockReportModel>();
                    if (list.Count > 0)
                    {
                        //筛选大于80%的数据,发送预警信息给加工厂材料负责人，并以短信通知
                        var dataList = list.Where(p => p.Point >= 80).ToList();
                        if (dataList.Count > 0)
                        {
                            //预警设置
                            var ewSstUp = Repository<TbEarlyWarningSetUp>.First(p => p.MenuCode == "RawMaterialStock" && p.IsStart == 1);
                            if (ewSstUp != null)
                            {
                                //查找短信模板信息
                                var smsTemp =
                                    Repository<TbShortMessageTemplate>.First(p => p.TemplateCode == "MB0010");
                                dataList.ForEach(x =>
                                {
                                    //短信内容
                                    string content = smsTemp.TemplateContent;
                                    var ShortContent = content.Replace("变量：材料名称，规格", x.MaterialName + "," + x.SpecificationModel);
                                    //获取要发送短信通知消息的用户
                                    DataTable dt = ct.GetParentCompany(order.SiteCode);
                                    if (dt.Rows.Count > 0)
                                    {
                                        string ManagerDepartmentCode = "";
                                        string BranchCode = "";
                                        string BranchName = "";
                                        string WorkAreaCode = "";
                                        string WorkAreaName = "";
                                        string SiteName = "";
                                        for (int i = 0; i < dt.Rows.Count; i++)
                                        {
                                            if (dt.Rows[i]["OrgType"].ToString() == "2")
                                            {
                                                ManagerDepartmentCode = dt.Rows[i]["CompanyCode"].ToString();
                                            }
                                            else if (dt.Rows[i]["OrgType"].ToString() == "3")
                                            {
                                                BranchCode = dt.Rows[i]["CompanyCode"].ToString();
                                                BranchName = dt.Rows[i]["CompanyFullName"].ToString();
                                            }
                                            else if (dt.Rows[i]["OrgType"].ToString() == "4")
                                            {
                                                WorkAreaCode = dt.Rows[i]["CompanyCode"].ToString();
                                                WorkAreaName = dt.Rows[i]["CompanyFullName"].ToString();
                                            }
                                            else if (dt.Rows[i]["OrgType"].ToString() == "5")
                                            {
                                                SiteName = dt.Rows[i]["CompanyFullName"].ToString();
                                            }
                                        }
                                        List<CensusdemoTask.NotiecUser> listUser = ct.GetSendUser("WorkOrder", ewSstUp.EarlyWarningNewsCode, orderId);
                                        if (listUser.Any())
                                        {
                                            for (int i = 0; i < listUser.Count; i++)
                                            {
                                                if (ewSstUp.App == 1)
                                                {
                                                    //调用BIM获取人员电话或者身份证号码的的接口
                                                    string userInfo = ct.up(listUser[i].PersonnelCode);
                                                    var jObject = Newtonsoft.Json.Linq.JObject.Parse(userInfo);
                                                    string tel = jObject["data"][0]["MobilePhone"].ToString();
                                                    if (!string.IsNullOrWhiteSpace(tel))
                                                    {
                                                        var myDxMsg = new TbSMSAlert()
                                                        {
                                                            InsertTime = DateTime.Now,
                                                            ManagerDepartment = ManagerDepartmentCode,
                                                            Branch = BranchCode,
                                                            WorkArea = WorkAreaCode,
                                                            Site = "",
                                                            UserCode = listUser[i].PersonnelCode,
                                                            UserTel = tel,
                                                            DXType = "",
                                                            BusinessCode = smsTemp.TemplateCode,
                                                            DataCode = order.OrderCode,
                                                            ShortContent = ShortContent,
                                                            FromCode = "WorkOrder",
                                                            MsgType = "2"

                                                        };
                                                        myDxList.Add(myDxMsg);
                                                    }
                                                }
                                                if (ewSstUp.Pc == 1)
                                                {
                                                    var myMsg = new TbFormEarlyWarningNodeInfo()
                                                    {
                                                        MenuCode = "WorkOrder",
                                                        EarlyWarningCode = ewSstUp.EarlyWarningNewsCode,
                                                        EWNodeCode = ewSstUp.ID,
                                                        EWContent = ShortContent,
                                                        EWUserCode = listUser[i].PersonnelCode,
                                                        EWFormDataCode = orderId,
                                                        EWStart = 0,
                                                        WorkArea = WorkAreaCode,
                                                        CompanyCode = BranchCode,
                                                        SiteCode = order.SiteCode,
                                                        EWTime = DateTime.Now,
                                                        ProjectId = order.ProjectId,
                                                        MsgType = "2"
                                                    };
                                                    myMsgList.Add(myMsg);
                                                }
                                            }
                                        }
                                    }

                                });
                                for (int i = 0; i < myDxList.Count; i++)
                                {
                                    //调用短信发送接口
                                    //string dx = ct.ShortMessagePC(myDxList[i].UserTel, myDxList[i].ShortContent);
                                    string dx = ct.ShortMessagePC("15756321745", myDxList[i].ShortContent);
                                    var jObject1 = Newtonsoft.Json.Linq.JObject.Parse(dx);
                                    var logmsg = jObject1["data"][0]["code"].ToString();
                                    myDxList[i].DXType = logmsg;
                                }
                            }

                        }
                    }
                }
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    if (myMsgList.Any())
                        Repository<TbFormEarlyWarningNodeInfo>.Insert(trans, myMsgList, true);
                    if (myDxList.Any())
                        Repository<TbSMSAlert>.Insert(trans, myDxList, true);
                    trans.Commit();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

        }
    }
}
