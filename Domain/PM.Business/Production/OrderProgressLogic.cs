using Dos.ORM;
using PM.Common;
using System;
using System.Collections.Generic;
using PM.DataEntity.Production.ViewModel;
using PM.DataEntity;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PM.DataAccess.DbContext;

namespace PM.Business.Production
{
    public class OrderProgressLogic
    {
        private readonly TbWorkOrderLogic orderProLogic = new TbWorkOrderLogic();

        string ProjectId = GetProjectId();

        public static string GetProjectId()
        {
            string ProjectId = "";
            bool Flog = OperatorProvider.Provider.CurrentUser == null;
            if (!Flog)
            {
                ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
            }
            return ProjectId;
        }

        #region 获取数据
        /// <summary>
        /// 获取编辑数据
        /// </summary>
        /// <param name="dataID">数据Id</param>
        /// <returns></returns>
        public Tuple<DataTable, DataTable> FindEntity(int dataID)
        {
            var ret = Db.Context.From<TbOrderProgress>()
            .Select(
                    TbOrderProgress._.All
                    , TbCompany._.CompanyFullName.As("SiteName")
                    , TbSysDictionaryData._.DictionaryText.As("ProcessingStateNew")
                    , TbUser._.UserName)
                  .LeftJoin<TbCompany>((a, c) => a.SiteCode == c.CompanyCode)
                  .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                  .Where(TbCompany._.CompanyCode == TbOrderProgress._.ProcessFactoryCode), "ProcessFactoryName")
                  .AddSelect(Db.Context.From<TbSysDictionaryData>().Select(p => p.DictionaryText)
                  .Where(TbSysDictionaryData._.DictionaryCode == TbOrderProgress._.UrgentDegree && TbSysDictionaryData._.FDictionaryCode == "UrgentDegree"), "UrgentDegreeNew")
                  .LeftJoin<TbSysDictionaryData>((a, c) => a.ProcessingState == c.DictionaryCode && c.FDictionaryCode == "ProcessingState")
                  .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                    .Where(p => p.ID == dataID).ToDataTable();
            //查找明细信息
           // var items = Db.Context.From<TbOrderProgressDetail>().Select(
           //    TbOrderProgressDetail._.All,
           //    TbSysDictionaryData._.DictionaryText.As("MeasurementUnitText"))
           //.LeftJoin<TbSysDictionaryData>((a, c) => a.MeasurementUnit == c.DictionaryCode)
           //.AddSelect(Db.Context.From<TbSysDictionaryData>().Select(p => p.DictionaryText)
           //.Where(TbSysDictionaryData._.DictionaryCode == TbOrderProgressDetail._.DaetailWorkStrat && TbSysDictionaryData._.FDictionaryCode == "DaetailWorkStrat"), "DaetailWorkStratNew")
           //.Where(p => p.OrderCode == ret.Rows[0]["OrderCode"].ToString() && p.RevokeStart == "正常").ToDataTable();
            string sql = @"select a.ID,a.OrderCode,a.ComponentName,a.LargePattern,a.MaterialCode,a.MaterialName,a.SpecificationModel,a.MeasurementUnit,a.MeasurementUnitZl,a.ItemUseNum,a.Number,a.WeightSmallPlan,a.DaetailWorkStrat,a.PackNumber,a.Manufactor,a.HeatNo,a.TestReportNo,a.QRCode,0 as TodayCompleted,a.AlreadyCompleted,a.WorkorderdetailId,a.NoCompleted,a.RevokeStart,b.DictionaryText as MeasurementUnitText,c.DictionaryText as DaetailWorkStratNew from TbOrderProgressDetail a
left join TbSysDictionaryData b on a.MeasurementUnit=b.DictionaryCode
left join TbSysDictionaryData c on a.DaetailWorkStrat=c.DictionaryCode and c.FDictionaryCode='DaetailWorkStrat'
where a.OrderCode=@OrderCode and a.RevokeStart='正常'";
            DataTable items = Db.Context.FromSql(sql)
                             .AddInParameter("@OrderCode", DbType.String, ret.Rows[0]["OrderCode"].ToString()).ToDataTable();
            return new Tuple<DataTable, DataTable>(ret, items);
        }

        /// <summary>
        /// 获取数据列表(分页)
        /// </summary>
        public PageModel GetDataListForPage(OrderProgressRequest request)
        {
            //组装查询语句
            #region 模糊搜索条件

            var where = new Where<TbOrderProgress>();

            if (!string.IsNullOrWhiteSpace(request.TypeName))
            {
                where.And(p => p.TypeName.Like(request.TypeName));
            }
            if (!string.IsNullOrWhiteSpace(request.OrderCode))
            {
                where.And(p => p.OrderCode.Like(request.OrderCode));
            }
            if (!string.IsNullOrWhiteSpace(request.ReportedStatus))
            {
                where.And(p => p.ReportedStatus == request.ReportedStatus);
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessingState))
            {
                where.And(p => p.ProcessingState == request.ProcessingState);
            }
            if (request.HistoryMonth.HasValue)
            {
                var historyMonth = new WhereClip("YEAR(TbOrderProgress.InsertTime)=" + request.HistoryMonth.Value.Year + " and MONTH(TbOrderProgress.InsertTime)=" + request.HistoryMonth.Value.Month);
                where.And(historyMonth);
            }
            if (!string.IsNullOrWhiteSpace(request.ProgressType))
            {
                if (request.ProgressType=="进度正常")
                {
                    var progressType = new WhereClip("(TbOrderProgress.FinishProcessingDateTime is not null and CONVERT(varchar(100),TbOrderProgress.FinishProcessingDateTime,23)=CONVERT(varchar(100),TbOrderProgress.DistributionTime,23)) or (TbOrderProgress.FinishProcessingDateTime is null and CONVERT(varchar(100),TbOrderProgress.DistributionTime,23)>=CONVERT(varchar(100),GETDATE(),23))");
                    where.And(progressType);
                }
                else if (request.ProgressType=="进度超前")
                {
                    var progressType = new WhereClip("(TbOrderProgress.FinishProcessingDateTime is not null and CONVERT(varchar(100),TbOrderProgress.FinishProcessingDateTime,23)<CONVERT(varchar(100),TbOrderProgress.DistributionTime,23))");
                    where.And(progressType);
                }
                else if (request.ProgressType=="进度滞后")
                {
                    var progressType = new WhereClip("(TbOrderProgress.FinishProcessingDateTime is not null and (CONVERT(varchar(100),TbOrderProgress.FinishProcessingDateTime,23)>CONVERT(varchar(100),TbOrderProgress.DistributionTime,23)) or (TbOrderProgress.FinishProcessingDateTime is null and CONVERT(varchar(100),TbOrderProgress.DistributionTime,23)<CONVERT(varchar(100),GETDATE(),23)))");
                    where.And(progressType);
                }
            }
            #endregion

            #region 数据权限新

            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where.And(p => p.ProcessFactoryCode == request.ProcessFactoryCode);
            }
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where.And(p => p.ProjectId == request.ProjectId);
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = orderProLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);
                if (SiteList.Count > 0)
                {
                    where.And(p => p.SiteCode.In(SiteList));
                }
            }

            #endregion

            try
            {
                var ret = Db.Context.From<TbOrderProgress>()
              .Select(
                      TbOrderProgress._.All
                      , TbCompany._.CompanyFullName.As("SiteName")
                      , TbSysDictionaryData._.DictionaryText.As("ProcessingStateNew")
                      , TbUser._.UserName)
                    .LeftJoin<TbCompany>((a, c) => a.SiteCode == c.CompanyCode)
                    .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                    .Where(TbCompany._.CompanyCode == TbOrderProgress._.ProcessFactoryCode), "ProcessFactoryName")
                    .AddSelect(Db.Context.From<TbSysDictionaryData>().Select(p => p.DictionaryText)
                    .Where(TbSysDictionaryData._.DictionaryCode == TbOrderProgress._.UrgentDegree && TbSysDictionaryData._.FDictionaryCode == "UrgentDegree"), "UrgentDegreeNew")
                    .LeftJoin<TbSysDictionaryData>((a, c) => a.ProcessingState == c.DictionaryCode && c.FDictionaryCode == "ProcessingState")
                    .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                      .Where(where).OrderByDescending(d => d.OrderCode).ToPageList(request.rows, request.page);
                return ret;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Tuple<DataTable, DataTable> GetProgressForm(OrderProgressRequest request)
        {
            DataTable dt1 = GetScProgress(request);
            DataTable dt2 = GetDdProgress(request);
            return new Tuple<DataTable, DataTable>(dt1, dt2);
        }
        /// <summary>
        /// 获取生产进度展示报表数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public DataTable GetScProgress(OrderProgressRequest request)
        {
            StringBuilder sbSiteCode = new StringBuilder();
            string sqlChilde = "";
            string sqlwhere = "";
            if (!string.IsNullOrWhiteSpace(request.TypeName))
            {
                sqlwhere += " and op.TypeName like '%" + request.TypeName + "%' ";
            }
            if (!string.IsNullOrWhiteSpace(request.OrderCode))
            {
                sqlwhere += " and op.OrderCode like '%" + request.OrderCode + "%' ";
            }
            if (request.HistoryMonth.HasValue)
            {
                string InsertTime = Convert.ToDateTime(request.HistoryMonth).ToString("yyyy-MM");
                sqlwhere += " and CONVERT(VARCHAR(7),op.InsertTime,120)='" + InsertTime + "'";
            }
            #region 数据权限

            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                sqlwhere += "and ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                sqlwhere += "and ProjectId='" + request.ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = orderProLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);
                for (int i = 0; i < SiteList.Count; i++)
                {
                    if (i == (SiteList.Count - 1))
                    {
                        sbSiteCode.Append("'" + SiteList[i] + "'");
                    }
                    else
                    {
                        sbSiteCode.Append("'" + SiteList[i] + "',");
                    }
                }
                if (SiteList.Count > 0)
                {
                    sqlwhere += " and SiteCode in(" + sbSiteCode + ")";
                }
            }

            #endregion

            sqlChilde = @"select op.SiteCode,cp.CompanyFullName as SiteName,op.ProcessFactoryCode,SUM(op.WeightTotal) as SumWeightTotal,SUM(AccumulativeQuantity) as JgWcWeightTotal,(SUM(op.WeightTotal)-SUM(AccumulativeQuantity)) as JgWwcWeightTotal from TbOrderProgress op
                              left join TbCompany cp on op.SiteCode=cp.CompanyCode
                              where 1=1 " + sqlwhere + @"
                              group by op.SiteCode,cp.CompanyFullName,op.ProcessFactoryCode order by op.SiteCode asc";
            var dt = Db.Context.FromSql(sqlChilde).ToDataTable();
            return dt;
        }

        public DataTable GetDdProgress(OrderProgressRequest request)
        {
            try
            {
                string where = "";

                #region 查询条件

                if (!string.IsNullOrWhiteSpace(request.TypeName))
                {
                    where += " and a.TypeName like '%" + request.TypeName + "%' ";
                }
                if (!string.IsNullOrWhiteSpace(request.OrderCode))
                {
                    where += " and a.OrderCode like '%" + request.OrderCode + "%' ";
                }

                if (!string.IsNullOrWhiteSpace(request.ProjectId))
                {
                    where += " and a.ProjectId='" + request.ProjectId + "'";
                }
                if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
                {
                    where += " and a.ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
                }
                if (!string.IsNullOrWhiteSpace(request.SiteCode))
                {
                    List<string> SiteList = orderProLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);
                    StringBuilder sbSiteCode = new StringBuilder();
                    for (int i = 0; i < SiteList.Count; i++)
                    {
                        if (i == (SiteList.Count - 1))
                        {
                            sbSiteCode.Append("'" + SiteList[i] + "'");
                        }
                        else
                        {
                            sbSiteCode.Append("'" + SiteList[i] + "',");
                        }
                    }
                    if (SiteList.Count > 0)
                    {
                        where += " and a.SiteCode in(" + sbSiteCode + ")";
                    }
                }
                if (request.HistoryMonth.HasValue)
                {
                    string InsertTimeMonth = Convert.ToDateTime(request.HistoryMonth).ToString("yyyy-MM");
                    where += " and CONVERT(VARCHAR(7),a.InsertTime,120)='" + InsertTimeMonth + "'";
                }

                #endregion

                string sql = @"select TbProgressType.ProgressType,ISNULL(TbProgress.WeightTotal, 0) as JdCount from (select '进度正常' as ProgressType
union all
select '进度超前' as ProgressType
union all
select '进度滞后' as ProgressType) TbProgressType
left join (select sum(a.WeightTotal) as WeightTotal,'进度超前' as  ProgressType from TbOrderProgress a
where 1=1 "+where+@" and (a.FinishProcessingDateTime is not null and CONVERT(varchar(100),a.FinishProcessingDateTime,23)<CONVERT(varchar(100),a.DistributionTime,23)) 
union all
select sum(a.WeightTotal) as WeightTotal,'进度正常' as  ProgressType from TbOrderProgress a
where 1=1 " + where + @" and (a.FinishProcessingDateTime is not null and CONVERT(varchar(100),a.FinishProcessingDateTime,23)=CONVERT(varchar(100),a.DistributionTime,23)) 
or (a.FinishProcessingDateTime is null and CONVERT(varchar(100),a.DistributionTime,23)>=CONVERT(varchar(100),GETDATE(),23))
union all
select sum(b.WeightSmallPlan) as WeightTotal,'进度滞后' as  ProgressType from TbOrderProgress a
left join TbOrderProgressDetail b on a.OrderCode=b.OrderCode
where 1=1 " + where + @" and (a.FinishProcessingDateTime is not null and (CONVERT(varchar(100),a.FinishProcessingDateTime,23)>CONVERT(varchar(100),a.DistributionTime,23)) 
or (a.FinishProcessingDateTime is null and CONVERT(varchar(100),a.DistributionTime,23)<CONVERT(varchar(100),GETDATE(),23)))) TbProgress on TbProgressType.ProgressType=TbProgress.ProgressType ";
                DataTable dt = Db.Context.FromSql(sql).ToDataTable();
                return dt;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        /// <summary>
        /// 获取进度滞后订单
        /// </summary>
        /// <returns></returns>
        public string GetLagOrderStr()
        {
            string str = @" select COUNT(1) as JdCount,'进度延后订单' as ProgressType from (select dp.SiteCode,cp.CompanyFullName as SiteName,dp.ProcessFactoryCode,dp.TypeName,dp.OrderCode,dp.ProjectId,dp.DistributionTime,du.LoadCompleteTime from TbDistributionPlanInfo dp
                          left join (select * from TbDistributionEnt as a
                          where a.DistributionCode in (select top 1 DistributionCode from TbDistributionEnt where DistributionPlanCode = a.DistributionPlanCode order by ID asc)) as du on dp.DistributionPlanCode=du.DistributionPlanCode
                          left join TbCompany cp on dp.SiteCode=cp.CompanyCode
                          where dp.OrderCode in(select OrderCode from TbDistributionEnt group by OrderCode) and LoadCompleteTime<GETDATE()) Tb 
                          where CONVERT(varchar(100), Tb.DistributionTime, 23)<CONVERT(varchar(100), Tb.LoadCompleteTime, 23) ";
            return str;
        }
        /// <summary>
        /// 获取当前登录人的组织机构下所有的下级站点/
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public List<string> GetSiteCodList(OrderProgressRequest request)
        {
            List<string> SiteList = new List<string>();

            #region 数据权限新

            bool Flog = OperatorProvider.Provider.CurrentUser == null;
            if (!Flog)
            {
                int orgType = Convert.ToInt32(OperatorProvider.Provider.CurrentUser.OrgType);
                string CompanyId = OperatorProvider.Provider.CurrentUser.CompanyId;
                SiteList = orderProLogic.GetCompanyWorkAreaOrSiteList(CompanyId, 5);//加载当前登录的组织机构下所有的站点
                if (!string.IsNullOrWhiteSpace(request.OrgType) && !string.IsNullOrWhiteSpace(request.CompanyCode))//选择左侧组织机构
                {
                    SiteList.Clear();//清空SiteList的集合
                    //第一步判断当前登录账号的，下级组织机构
                    if (orgType == 2)//经理部
                    {
                        //第二步判断当前点击的组织机构类型
                        if (request.OrgType == "3" || request.OrgType == "4" || request.OrgType == "5")//这个人就可以加载任意分部工区下的站点数据
                        {
                            SiteList = orderProLogic.GetCompanyWorkAreaOrSiteList(request.CompanyCode, 5);
                        }
                        else
                        {
                            SiteList = orderProLogic.GetCompanyWorkAreaOrSiteList(CompanyId, 5);
                        }
                    }
                    else if (orgType == 3)//分部
                    {
                        if (request.OrgType == "4" || request.OrgType == "5")//这个人只能加载该分部下的所有所以工区下的所有站点数据
                        {
                            //第三步判断当前点击的组织机构类型是不是属于当前登录账号的下级
                            List<string> a = orderProLogic.GetCompanyAllChild(CompanyId);
                            if (a.Contains(request.CompanyCode))
                            {
                                SiteList = orderProLogic.GetCompanyWorkAreaOrSiteList(request.CompanyCode, 5);
                            }
                            else
                            {
                                SiteList = orderProLogic.GetCompanyWorkAreaOrSiteList(CompanyId, 5);
                            }
                        }
                        else
                        {
                            SiteList = orderProLogic.GetCompanyWorkAreaOrSiteList(CompanyId, 5);
                        }
                    }
                    else if (orgType == 4)//工区
                    {
                        if (request.OrgType == "5")//这个人只能加载这个工区下的站点数据
                        {
                            //第三步判断当前点击的组织机构类型是不是属于当前登录账号的下级
                            List<string> a = orderProLogic.GetCompanyAllChild(CompanyId);
                            if (a.Contains(request.CompanyCode))
                            {
                                SiteList = orderProLogic.GetCompanyWorkAreaOrSiteList(request.CompanyCode, 5);
                            }
                            else
                            {
                                SiteList = orderProLogic.GetCompanyWorkAreaOrSiteList(CompanyId, 5);
                            }
                        }
                        else
                        {
                            SiteList = orderProLogic.GetCompanyWorkAreaOrSiteList(CompanyId, 5);
                        }
                    }
                    else if (orgType == 5)//站点
                    {
                        SiteList = orderProLogic.GetCompanyWorkAreaOrSiteList(CompanyId, 5);//这个人只能加载该站点下的数据
                    }
                    else if (orgType == 1)//加工厂
                    {

                    }
                }
            }

            #endregion

            return SiteList;
        }

        /// <summary>
        /// 获取加工信息
        /// </summary>
        /// <returns></returns>
        public List<string> GetProcessFactoryCode(string orgType, string CompanyCode)
        {
            List<string> ProcessFactoryCodeList = new List<string>();
            //判断当前登录是属于加工厂，还是经理部
            bool Flog = OperatorProvider.Provider.CurrentUser == null;
            if (!Flog)
            {
                int orgTypeLogin = Convert.ToInt32(OperatorProvider.Provider.CurrentUser.OrgType);
                string CompanyId = OperatorProvider.Provider.CurrentUser.CompanyId;
                if (orgTypeLogin == 2)//登录人是经理部的人员(经理部看所有加工厂的数据)
                {
                    if (!string.IsNullOrEmpty(orgType) && orgType == "1")//登录人是经理部里面的人员，同时选择了左边组织机构为加工厂的组织机构
                    {
                        ProcessFactoryCodeList.Add(CompanyCode);
                    }
                    else
                    {
                        string sql = @"select CompanyCode,CompanyFullName from TbCompany where OrgType=1";
                        ProcessFactoryCodeList = Db.Context.FromSql(sql).ToList<string>();
                    }
                }
                else//当前登录人不是经理部
                {
                    if (orgType == "1")
                    {
                        ProcessFactoryCodeList.Add(CompanyCode);
                    }
                }
            }
            return ProcessFactoryCodeList;
        }

        #endregion

        #region 修改：明细表数据

        /// <summary>
        /// 修改数据
        /// </summary>
        public AjaxResult Update(TbOrderProgress modelo, List<TbOrderProgressDetail> items)
        {
            if (modelo == null)
                return AjaxResult.Warning("参数错误");
            //获取加工订单主表信息
            var model = Repository<TbOrderProgress>.First(d => d.OrderCode == modelo.OrderCode);
            model.AccumulativeQuantity = modelo.AccumulativeQuantity;
            //获取加工订单主表信息
            var model1 = Repository<TbWorkOrder>.First(d => d.OrderCode == model.OrderCode);
            //获取配送计划主表信息
            var model2 = Repository<TbDistributionPlanInfo>.First(d => d.OrderCode == model.OrderCode);
            //获取该加工订单进度下所有的明细
            var ret = Db.Context.From<TbOrderProgressDetail>()
             .Select(
                     TbOrderProgressDetail._.All)
                     .Where(p => p.OrderCode == model.OrderCode).ToList();
            //定义一个list集合存放修改后的加工订单进度明细
            var proItemList = new List<TbOrderProgressDetail>();

            //查询该加工订单的所有明细
            var retorder = Db.Context.From<TbWorkOrderDetail>()
                .Select(TbWorkOrderDetail._.All)
                .Where(p => p.OrderCode == model.OrderCode).ToList();
            //定义一个list集合存放修改后的加工订单进度明细
            var orderList = new List<TbWorkOrderDetail>();

            //查询该配送计划的所有明细
            var displan = Db.Context.From<TbDistributionPlanDetailInfo>()
                .Select(TbDistributionPlanDetailInfo._.All)
                .Where(p => p.OrderCode == model.OrderCode).ToList();
            //定义一个list集合存放修改后的配送计划明细
            var planList = new List<TbDistributionPlanDetailInfo>();

            for (int i = 0; i < items.Count; i++)
            {
                var pItem = ret.Where(d => d.WorkorderdetailId == items[i].WorkorderdetailId).FirstOrDefault();
                //修改订单明细中的加工状态
                var orderItem = retorder.Where(d => d.ID == pItem.WorkorderdetailId).FirstOrDefault();
                //修改配送计划明细中的加工状态
                var planItem = displan.Where(d => d.WorkorderdetailId == pItem.WorkorderdetailId).FirstOrDefault();
                if (pItem != null && orderItem != null)
                {
                    if (pItem.WeightSmallPlan != 0)
                    {
                        //修改订单进度明细中的加工状态跟填报数量
                        pItem.TodayCompleted = items[i].TodayCompleted;//今日完成量
                        pItem.AlreadyCompleted = items[i].AlreadyCompleted;//开累完成量
                        pItem.NoCompleted = pItem.WeightSmallPlan - items[i].AlreadyCompleted;//未完成量
                        if (pItem.NoCompleted == 0 && items[i].DaetailWorkStrat != "加工完成")
                        {
                            pItem.DaetailWorkStrat = "加工完成";
                            orderItem.DaetailWorkStrat = "加工完成";
                            orderItem.FinishTime = DateTime.Now;
                            if (planItem != null)
                            {
                                planItem.DaetailWorkStrat = "加工完成";
                                planList.Add(planItem);
                            }
                        }
                        proItemList.Add(pItem);
                        orderList.Add(orderItem);
                    }
                    else
                    {
                        pItem.DaetailWorkStrat = "加工完成";
                        proItemList.Add(pItem);
                        orderItem.DaetailWorkStrat = "加工完成";
                        orderList.Add(orderItem);
                        if (planItem != null)
                        {
                            planItem.DaetailWorkStrat = "加工完成";
                            planList.Add(planItem);
                        }
                    }
                }
            }
            var a = proItemList.Where(p => p.DaetailWorkStrat != "加工完成");
            if (a.Count() == 0)
            {
                model.ReportedStatus = "已填报";
                model.FinishProcessingDateTime = DateTime.Now;
                model.ProcessingState = "Finishing";
                model1.ProcessingState = "Finishing";
                model2.ProcessingState = "Finishing";
            }
            else
            {
                model.ReportedStatus = "填报中";
            }
            using (DbTrans trans = Db.Context.BeginTransaction())//使用事务
            {
                try
                {
                    //修改主表中填报状态
                    Repository<TbOrderProgress>.Update(trans, model, p => p.ID == model.ID);
                    //修改加工订单主表加工状态
                    Repository<TbWorkOrder>.Update(trans, model1, p => p.OrderCode == model.OrderCode);
                    //修改配送计划主表加工状态
                    Repository<TbDistributionPlanInfo>.Update(trans, model2, p => p.OrderCode == model.OrderCode);
                    if (items.Count > 0)
                    {
                        if (proItemList.Count > 0)
                        {
                            //修改加工进度明细信息
                            Repository<TbOrderProgressDetail>.Update(trans, proItemList);
                        }
                        if (orderList.Count > 0)
                        {
                            //修改加工订单明细中的加工状态
                            Repository<TbWorkOrderDetail>.Update(trans, orderList);
                        }
                        if (planList.Count > 0)
                        {
                            //修改加工订单明细中的加工状态
                            Repository<TbDistributionPlanDetailInfo>.Update(trans, planList);
                        }

                    }
                    trans.Commit();//提交事务
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return AjaxResult.Error(ex.ToString());
                }
                finally
                {
                    trans.Close();
                }

                return AjaxResult.Success();
            }

        }

        #endregion

        #region 判断
        /// <summary>
        /// 判断信息是否可操作
        /// </summary>
        /// <returns></returns>
        public AjaxResult AnyInfo(int keyValue)
        {
            var monthDemandPlan = Repository<TbOrderProgress>.First(p => p.ID == keyValue);
            if (monthDemandPlan == null)
                return AjaxResult.Warning("信息不存在");
            if (monthDemandPlan.Examinestatus != "未发起")
                return AjaxResult.Warning("信息正在审核中或已审核完成,不能进行此操作");

            return AjaxResult.Success(monthDemandPlan);
        }

        #endregion

        #region 一键填报

        public AjaxResult UpdateOneKey(int Id)
        {
            try
            {
                //获取加工订单进度填报主表信息
                var model = Repository<TbOrderProgress>.First(d => d.ID == Id);
                model.ReportedStatus = "已填报";
                model.FinishProcessingDateTime = DateTime.Now;
                model.ProcessingState = "Finishing";
                //获取加工订单主表信息
                var model1 = Repository<TbWorkOrder>.First(d => d.OrderCode == model.OrderCode);
                model1.ProcessingState = "Finishing";
                //获取配送计划主表信息
                var model2 = Repository<TbDistributionPlanInfo>.First(d => d.OrderCode == model.OrderCode);
                model2.ProcessingState = "Finishing";
                //获取订单进度明细
                var proItemList = Repository<TbOrderProgressDetail>.Query(p => p.OrderCode == model.OrderCode && p.DaetailWorkStrat != "加工完成" && p.RevokeStart == "正常").ToList();
                if (proItemList.Any())
                {
                    decimal? AccumulativeQuantity = model.AccumulativeQuantity;
                    proItemList.ForEach(x =>
                    {
                        x.TodayCompleted = x.NoCompleted;
                        x.AlreadyCompleted = x.AlreadyCompleted + x.NoCompleted;
                        x.NoCompleted = 0;
                        if (x.DaetailWorkStrat == "未加工")
                            x.PackNumber = x.Number;
                        x.DaetailWorkStrat = "加工完成";
                        AccumulativeQuantity += x.TodayCompleted;//主表开累加工量
                    });
                    model.AccumulativeQuantity = AccumulativeQuantity;
                }

                //查询该加工订单的所有明细
                var orderList = Repository<TbWorkOrderDetail>.Query(p => p.OrderCode == model.OrderCode && p.DaetailWorkStrat != "加工完成" && p.RevokeStart == "正常").ToList();
                if (orderList.Any())
                {
                    orderList.ForEach(x =>
                    {
                        if (x.DaetailWorkStrat == "未加工")
                        {
                            x.PackageNumber = 1;
                            x.PackNumber = x.Number;
                        }
                        x.DaetailWorkStrat = "加工完成";
                        x.FinishTime = model.FinishProcessingDateTime;
                    });
                }
                //查询该配送计划的所有明细
                var planList = Repository<TbDistributionPlanDetailInfo>.Query(p => p.OrderCode == model.OrderCode && p.DaetailWorkStrat != "加工完成" && p.RevokeStart == "正常").ToList();
                planList.ForEach(x =>
                {
                    if (x.DaetailWorkStrat == "未加工")
                        x.PackNumber = x.Number;
                    x.DaetailWorkStrat = "加工完成";
                });

                using (DbTrans trans = Db.Context.BeginTransaction())//使用事务
                {
                    //修改加工订单进度填报主表中填报状态
                    Repository<TbOrderProgress>.Update(trans, model, p => p.ID == model.ID);
                    //修改加工订单主表加工状态
                    Repository<TbWorkOrder>.Update(trans, model1, p => p.OrderCode == model.OrderCode);
                    //修改配送计划主表加工状态
                    Repository<TbDistributionPlanInfo>.Update(trans, model2, p => p.OrderCode == model.OrderCode);
                    //修改加工进度明细信息
                    if (proItemList.Any())
                        Repository<TbOrderProgressDetail>.Update(trans, proItemList);
                    //修改加工订单明细中的加工状态
                    if (orderList.Any())
                        Repository<TbWorkOrderDetail>.Update(trans, orderList);
                    //修改配送计划明细中的加工状态
                    if (planList.Any())
                        Repository<TbDistributionPlanDetailInfo>.Update(trans, planList);
                    trans.Commit();//提交事务

                    return AjaxResult.Success();
                }
            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.ToString());
            }

        }

        #endregion

        #region 导出
        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public DataTable GetExportList(OrderProgressRequest request)
        {
            //组装查询语句
            #region 模糊搜索条件

            var where = new Where<TbOrderProgress>();

            if (!string.IsNullOrWhiteSpace(request.TypeName))
            {
                where.And(p => p.TypeName.Like(request.TypeName));
            }
            if (!string.IsNullOrWhiteSpace(request.OrderCode))
            {
                where.And(p => p.OrderCode.Like(request.OrderCode));
            }
            if (!string.IsNullOrWhiteSpace(request.ReportedStatus))
            {
                where.And(p => p.ReportedStatus == request.ReportedStatus);
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessingState))
            {
                where.And(p => p.ProcessingState == request.ProcessingState);
            }
            if (request.HistoryMonth.HasValue)
            {
                var historyMonth = new WhereClip("YEAR(TbOrderProgress.InsertTime)=" + request.HistoryMonth.Value.Year + " and MONTH(TbOrderProgress.InsertTime)=" + request.HistoryMonth.Value.Month);
                where.And(historyMonth);
            }
            #endregion

            #region 数据权限新

            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where.And(p => p.ProcessFactoryCode == request.ProcessFactoryCode);
            }
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where.And(p => p.ProjectId == request.ProjectId);
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = orderProLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);
                if (SiteList.Count > 0)
                {
                    where.And(p => p.SiteCode.In(SiteList));
                }
            }

            #endregion

            try
            {
                var ret = Db.Context.From<TbOrderProgress>()
              .Select(
                      TbOrderProgress._.All
                      , TbCompany._.CompanyFullName.As("SiteName")
                      , TbSysDictionaryData._.DictionaryText.As("ProcessingStateNew")
                      , TbUser._.UserName)
                    .LeftJoin<TbCompany>((a, c) => a.SiteCode == c.CompanyCode)
                    .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                    .Where(TbCompany._.CompanyCode == TbOrderProgress._.ProcessFactoryCode), "ProcessFactoryName")
                    .AddSelect(Db.Context.From<TbSysDictionaryData>().Select(p => p.DictionaryText)
                    .Where(TbSysDictionaryData._.DictionaryCode == TbOrderProgress._.UrgentDegree && TbSysDictionaryData._.FDictionaryCode == "UrgentDegree"), "UrgentDegreeNew")
                    .LeftJoin<TbSysDictionaryData>((a, c) => a.ProcessingState == c.DictionaryCode && c.FDictionaryCode == "ProcessingState")
                    .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                      .Where(where).OrderByDescending(d => d.OrderCode).ToDataTable();
                return ret;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion
    }
}
