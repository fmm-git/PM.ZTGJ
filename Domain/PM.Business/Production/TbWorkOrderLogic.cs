using Dos.Common;
using Dos.ORM;
using PM.Common;
using PM.Common.Helper;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using PM.DataEntity.Production.ViewModel;
using PM.DataEntity.System.ViewModel;
using PM.Domain.WebBase;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PM.Business.Production
{
    public class TbWorkOrderLogic
    {

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

        #region 新增数据
        /// <summary>
        /// 新增数据
        /// </summary>
        public AjaxResult Insert(TbWorkOrder model, List<TbWorkOrderDetail> items)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            model.InsertUserCode = OperatorProvider.Provider.CurrentUser.UserCode;
            model.IsOffline = 0;
            //for (int i = 0; i < items.Count; i++)
            //{
            //    var ret = Db.Context.From<TbProcessingTechnology>().Select(TbProcessingTechnology._.All).Where(d => d.PID != 0 && d.ProcessingTechnologyName == Convert.ToString(items[i].ProcessingTechnology)).First();
            //    if (ret != null)
            //    {
            //        items[i].ProcessingTechnology = Convert.ToString(ret.ID);
            //    }
            //}
            using (DbTrans trans = Db.Context.BeginTransaction())
            {
                try
                {
                    //添加信息及明细信息
                    Repository<TbWorkOrder>.Insert(trans, model);
                    Repository<TbWorkOrderDetail>.Insert(trans, items);
                    trans.Commit();
                    return AjaxResult.Success();
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
            }
        }

        #endregion

        #region 修改数据

        /// <summary>
        /// 修改数据
        /// </summary>
        public AjaxResult Update(TbWorkOrder model, List<TbWorkOrderDetail> items)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            var anyRet = AnyInfo(model.ID);
            if (anyRet.state.ToString() != ResultType.success.ToString())
                return anyRet;
            //for (int i = 0; i < items.Count; i++)
            //{
            //    var ret = Db.Context.From<TbProcessingTechnology>().Select(TbProcessingTechnology._.All).Where(d => d.PID != 0 && d.ProcessingTechnologyName == Convert.ToString(items[i].ProcessingTechnology)).First();
            //    if (ret != null)
            //    {
            //        items[i].ProcessingTechnology = Convert.ToString(ret.ID);
            //    }
            //}
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())//使用事务
                {
                    //修改信息
                    Repository<TbWorkOrder>.Update(trans, model, p => p.ID == model.ID);

                    if (items.Count > 0)
                    {
                        //删除历史明细信息,添加明细信息
                        Repository<TbWorkOrderDetail>.Delete(trans, p => p.OrderCode == model.OrderCode);
                        Repository<TbWorkOrderDetail>.Insert(trans, items);
                    }
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

        #region 删除数据

        /// <summary>
        /// 删除数据
        /// </summary>
        public AjaxResult Delete(int keyValue)
        {
            try
            {
                //判断信息是否存在
                var anyRet = AnyInfo(keyValue);
                if (anyRet.state.ToString() != ResultType.success.ToString())
                    return anyRet;
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //删除信息，删除明细信息
                    var count = Repository<TbWorkOrder>.Delete(trans, p => p.ID == keyValue);
                    Repository<TbWorkOrderDetail>.Delete(trans, p => p.OrderCode == ((TbWorkOrder)anyRet.data).OrderCode);

                    //新增配送计划，删除信息
                    //var count1 = Repository<TbDistributionPlanInfo>.Delete(trans, p => p.ID == keyValue);
                    //Repository<TbDistributionPlanDetailInfo>.Delete(trans, p => p.OrderCode == ((TbWorkOrder)anyRet.data).OrderCode);

                    //订单进度及明细
                    var count2 = Repository<TbOrderProgress>.Delete(trans, p => p.ID == keyValue);
                    Repository<TbOrderProgressDetail>.Delete(trans, p => p.OrderCode == ((TbWorkOrder)anyRet.data).OrderCode);

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

        #region 获取数据

        /// <summary>
        /// 获取编辑数据
        /// </summary>
        /// <param name="dataID">数据Id</param>
        /// <returns></returns>
        public Tuple<DataTable, DataTable> FindEntity(int dataID)
        {
            var ret = Db.Context.From<TbWorkOrder>()
            .Select(
                    TbWorkOrder._.All
                    , TbCompany._.CompanyFullName.As("SiteName")
                    , TbSysDictionaryData._.DictionaryText.As("ProcessingStateNew")
                    , TbUser._.UserName
                    , TbDistributionPlanInfo._.DistributionPlanCode
                    , TbDistributionPlanInfo._.ID.As("PID"))
                  .LeftJoin<TbCompany>((a, c) => a.SiteCode == c.CompanyCode)
                  .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                  .Where(TbCompany._.CompanyCode == TbWorkOrder._.ProcessFactoryCode), "ProcessFactoryName")
                  .AddSelect(Db.Context.From<TbSysDictionaryData>().Select(p => p.DictionaryText)
                  .Where(TbSysDictionaryData._.DictionaryCode == TbWorkOrder._.UrgentDegree && TbSysDictionaryData._.FDictionaryCode == "UrgentDegree"), "UrgentDegreeNew")
                  .LeftJoin<TbSysDictionaryData>((a, c) => a.ProcessingState == c.DictionaryCode && c.FDictionaryCode == "ProcessingState")
                  .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                  .LeftJoin<TbDistributionPlanInfo>((a, c) => a.OrderCode == c.OrderCode)
                    .Where(p => p.ID == dataID).ToDataTable();
            //查找明细信息
            var items = Db.Context.From<TbWorkOrderDetail>().Select(
               TbWorkOrderDetail._.All,
               TbSysDictionaryData._.DictionaryText.As("MeasurementUnitText"),
               TbProcessingTechnology._.ProcessingTechnologyName)
           .LeftJoin<TbSysDictionaryData>((a, c) => a.MeasurementUnit == c.DictionaryCode && c.FDictionaryCode == "Unit")
           .LeftJoin<TbProcessingTechnology>((a, c) => a.ProcessingTechnology == c.ID)
           .Where(p => p.OrderCode == ret.Rows[0]["OrderCode"].ToString()).ToDataTable();
            return new Tuple<DataTable, DataTable>(ret, items);
        }

        /// <summary>
        /// 获取数据列表(分页)
        /// </summary>
        public PageModel GetDataListForPage(WorkOrderRequest request)
        {
            //组装查询语句
            #region 模糊搜索条件

            var where = new Where<TbWorkOrder>();

            if (!string.IsNullOrWhiteSpace(request.ProcessingState))
            {
                if (request.ProcessingState == "已退回")
                {
                    where.And(p => p.Examinestatus == request.ProcessingState);
                }
                else
                {
                    //订单状态
                    where.And(p => p.ProcessingState == request.ProcessingState);
                }
            }
            if (!string.IsNullOrWhiteSpace(request.OrderProcessingState))
            {
                if (request.OrderProcessingState == "进度正常")
                {
                    var processingState = new WhereClip("((TbOrderProgress.FinishProcessingDateTime is not null and CONVERT(varchar(100),TbOrderProgress.FinishProcessingDateTime,23)=CONVERT(varchar(100),TbWorkOrder.DistributionTime,23) and TbWorkOrder.Examinestatus='审核完成') or (TbOrderProgress.FinishProcessingDateTime is null and CONVERT(varchar(100),TbWorkOrder.DistributionTime,23)>=CONVERT(varchar(100),GETDATE(),23) and TbWorkOrder.Examinestatus='审核完成'))");
                    where.And(processingState);
                }
                else if (request.OrderProcessingState == "进度超前")
                {
                    var processingState = new WhereClip("(TbOrderProgress.FinishProcessingDateTime is not null and CONVERT(varchar(100),TbOrderProgress.FinishProcessingDateTime,23)<CONVERT(varchar(100),TbWorkOrder.DistributionTime,23) and TbWorkOrder.Examinestatus='审核完成')");
                    where.And(processingState);
                }
                else if (request.OrderProcessingState == "已完成（滞后）")
                {
                    var processingState = new WhereClip("(TbOrderProgress.FinishProcessingDateTime is not null and CONVERT(varchar(100),TbOrderProgress.FinishProcessingDateTime,23)>CONVERT(varchar(100),TbWorkOrder.DistributionTime,23) and TbWorkOrder.Examinestatus='审核完成')");
                    where.And(processingState);
                }
                else if (request.OrderProcessingState == "未完成（滞后）")
                {
                    var processingState = new WhereClip("(CONVERT(varchar(100),TbWorkOrder.DistributionTime,23)<CONVERT(varchar(100),GETDATE(),23) and TbOrderProgress.FinishProcessingDateTime is null and TbWorkOrder.Examinestatus='审核完成')");
                    where.And(processingState);
                }
                else if (request.OrderProcessingState == "滞后（全部订单）")
                {
                    var processingState = new WhereClip("((TbOrderProgress.FinishProcessingDateTime is not null and CONVERT(varchar(100),TbOrderProgress.FinishProcessingDateTime,23)>CONVERT(varchar(100),TbWorkOrder.DistributionTime,23) and TbWorkOrder.Examinestatus='审核完成') or (CONVERT(varchar(100),TbWorkOrder.DistributionTime,23)<CONVERT(varchar(100),GETDATE(),23) and TbOrderProgress.FinishProcessingDateTime is null and TbWorkOrder.Examinestatus='审核完成'))");
                    where.And(processingState);
                }
                else if (request.OrderProcessingState=="已完成量")
                {
                    var processingState = new WhereClip("( TbWorkOrder.Examinestatus='审核完成' and TbWorkOrder.ProcessingState='Finishing')");
                    where.And(processingState);
                }
                else if (request.OrderProcessingState=="未完成量")
                {
                    var processingState = new WhereClip("( TbWorkOrder.Examinestatus='审核完成' and (TbWorkOrder.ProcessingState='Received' or TbWorkOrder.ProcessingState='AlreadyReceived' or TbWorkOrder.ProcessingState='Processing'))");
                    where.And(processingState);
                }
            }
            if (request.IsNotOver)//未完成
            {
                where.And(p => p.Examinestatus == "审核完成" && p.ProcessingState.In("Processing", "Received", "AlreadyReceived")); //加工中,已接收,已领料
            }
            if (!string.IsNullOrWhiteSpace(request.TypeCode))
            {
                where.And(p => p.TypeCode.Like(request.TypeCode));
            }
            if (!string.IsNullOrWhiteSpace(request.UrgentDegree))
            {
                where.And(p => p.UrgentDegree == request.UrgentDegree);
            }
            if (!string.IsNullOrWhiteSpace(request.PickingState))
            {
                if (request.PickingState == "未领料")
                {
                    where.And(p => p.PickingState == request.PickingState && p.ProcessingState == "Received");
                }
                else
                {
                    where.And(p => p.PickingState == request.PickingState && (p.ProcessingState == "AlreadyReceived" || p.ProcessingState == "Processing"));
                }

            }
            if (!string.IsNullOrWhiteSpace(request.Examinestatus))
            {
                where.And(p => p.Examinestatus == "已退回");
            }
            if (!string.IsNullOrWhiteSpace(request.BegTime.ToString()))
            {
                where.And(p => p.DistributionTime >= request.BegTime);
            }
            if (!string.IsNullOrWhiteSpace(request.EndTiem.ToString()))
            {
                where.And(p => p.DistributionTime <= request.EndTiem);
            }
            if (!string.IsNullOrWhiteSpace(request.OrderType))
            {
                if (request.OrderType == "订单总量")//订单总量
                {
                    where.And(p => p.ProcessingState != "ConfirmWork");
                }
                else if (request.OrderType == "已接收订单量")//已接收
                {
                    where.And(p => p.ProcessingState == "Received");
                }
                else if (request.OrderType == "已领料订单量")//已领料
                {
                    where.And(p => p.ProcessingState == "AlreadyReceived");
                }
                else if (request.OrderType == "加工中订单量")//加工中
                {
                    where.And(p => p.ProcessingState == "Processing");
                }
                else if (request.OrderType == "未配送订单量")//未配送
                {
                    List<string> OrderList = GetWpsOrderList(request, request.OrderType);
                    if (OrderList.Count > 0)
                    {
                        where.And(p => p.OrderCode.In(OrderList));
                    }
                }
                else if (request.OrderType == "已配送订单量")//已配送
                {
                    List<string> OrderList = GetWpsOrderList(request, request.OrderType);
                    if (OrderList.Count > 0)
                    {
                        where.And(p => p.OrderCode.In(OrderList));
                    }
                }
            }
            if (!string.IsNullOrWhiteSpace(request.OrderCode))
            {
                where.And(p => p.OrderCode.Like(request.OrderCode));
            }
            if (!string.IsNullOrEmpty(request.SignState))
            {
                List<string> OrderList = GetSignStateBySearch(request.SignState);
                if (OrderList.Any())
                {
                    where.And(p => p.OrderCode.In(OrderList));
                }
            }
            if (!string.IsNullOrEmpty(request.DistributionStart))
            {
                where.And<TbDistributionPlanInfo>((a, c) => c.DistributionStart == request.DistributionStart);
            }
            if (!string.IsNullOrEmpty(request.OrderStart))
            {
                if (request.IsQB == "是")
                {
                    if (!string.IsNullOrWhiteSpace(request.CxType))
                    {
                        if (request.OrderStart == "退回订单")
                        {
                            where.And(p => p.Examinestatus == "已退回");
                        }
                        else if (request.OrderStart == "变更订单")
                        {
                            where.And(p => p.OrderStart.Like("变更"));
                        }
                        else if (request.OrderStart == "正常订单")
                        {
                            where.And(p => p.UrgentDegree == "Commonly");
                        }
                        else if (request.OrderStart == "加急订单")
                        {
                            where.And(p => p.UrgentDegree == "Urgent");
                        }
                    }
                    else
                    {
                        where.And(p => p.OrderStart == request.OrderStart);
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(request.CxType))
                    {
                        if (request.OrderStart == "退回订单")
                        {
                            where.And(p => p.Examinestatus == "已退回");
                        }
                        else if (request.OrderStart == "变更订单")
                        {
                            where.And(p => p.OrderStart.Like("变更"));
                        }
                        else if (request.OrderStart == "正常订单")
                        {
                            where.And(p => p.UrgentDegree == "Commonly" && p.Examinestatus == "审核完成");
                        }
                        else if (request.OrderStart == "加急订单")
                        {
                            where.And(p => p.UrgentDegree == "Urgent" && p.Examinestatus == "审核完成");
                        }
                    }
                    else
                    {
                        where.And(p => p.OrderStart == request.OrderStart);
                    }
                }
            }
            if (request.HistoryMonth.HasValue)
            {
                string HistoryMonth = Convert.ToDateTime(request.HistoryMonth).ToString("yyyy-MM");
                if (request.MonthType == "加工月份")
                {
                    if (request.OrderStart == "退回订单" || request.OrderStart == "变更订单")
                    {
                        //var historyMonth = new WhereClip("((CONVERT(varchar(7), TbWorkOrder.DistributionTime, 120)='" + HistoryMonth + "' or (CONVERT(varchar(7), TbWorkOrder.DistributionTime, 120)<'" + HistoryMonth + "')))");
                        var historyMonth = new WhereClip("((CONVERT(varchar(7), TbWorkOrder.DistributionTime, 120)='" + HistoryMonth + "'))");
                        where.And(historyMonth);
                    }
                    else
                    {
                        var historyMonth = new WhereClip("((CONVERT(varchar(7), TbWorkOrder.DistributionTime, 120)='" + HistoryMonth + "' or (CONVERT(varchar(7), TbWorkOrder.DistributionTime, 120)<'" + HistoryMonth + "' and TbWorkOrder.ProcessingState!='Finishing')))");
                        where.And(historyMonth);
                        //if (!string.IsNullOrWhiteSpace(request.CxType))
                        //{
                        where.And(p => p.Examinestatus == "审核完成");
                        //}
                    }
                }
                else
                {
                    var historyMonth = new WhereClip("(CONVERT(varchar(7), TbWorkOrder.InsertTime, 120)='" + HistoryMonth + "')");
                    if (request.IsLeft == "是" && request.OrderStart != "退回订单")
                    {
                        where.And(historyMonth);
                        where.And(p => p.Examinestatus == "审核完成");
                    }
                    else
                    {
                        where.And(historyMonth);
                        if (!string.IsNullOrWhiteSpace(request.CxType))
                        {
                            if (request.OrderStart != "退回订单")
                            {
                                where.And(p => p.Examinestatus == "审核完成");
                            }
                        }
                    }
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
                List<string> SiteList = GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);
                if (SiteList.Count > 0)
                {
                    where.And(p => p.SiteCode.In(SiteList));
                }
            }

            #endregion

            try
            {
                var sql = Db.Context.From<TbWorkOrder>()
              .Select(
                      TbWorkOrder._.All
                      , TbCompany._.CompanyFullName.As("SiteName")
                      , TbSysDictionaryData._.DictionaryText.As("ProcessingStateNew")
                      , TbUser._.UserName
                      , TbDistributionPlanInfo._.DistributionStart.IsNull("")
                      , TbDistributionPlanInfo._.DeliveryCompleteTime
                      , TbWorkOrder._.Examinestatus.As("ExaminestatusNew")
                      , TbSemiFinishedSign._.ID.As("SemiFinishedSignId")
                      , TbSemiFinishedSign._.Enclosure.As("EnclosureSF")
                      , TbOrderProgress._.FinishProcessingDateTime)
                    .LeftJoin<TbCompany>((a, c) => a.SiteCode == c.CompanyCode)
                    .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                    .Where(TbCompany._.CompanyCode == TbWorkOrder._.ProcessFactoryCode), "ProcessFactoryName")
                    .AddSelect(Db.Context.From<TbSysDictionaryData>().Select(p => p.DictionaryText)
                    .Where(TbSysDictionaryData._.DictionaryCode == TbWorkOrder._.UrgentDegree && TbSysDictionaryData._.FDictionaryCode == "UrgentDegree"), "UrgentDegreeNew")
                    .LeftJoin<TbSysDictionaryData>((a, c) => a.ProcessingState == c.DictionaryCode && c.FDictionaryCode == "ProcessingState")
                    .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                    .LeftJoin<TbDistributionPlanInfo>((a, c) => a.OrderCode == c.OrderCode)
                    .LeftJoin<TbSemiFinishedSign>((a, c) => a.OrderCode == c.OrderCodeH)
                    .LeftJoin<TbOrderProgress>((a, c) => a.OrderCode == c.OrderCode)
                      .Where(where).OrderByDescending(d => d.ID);

                var ret = new PageModel();
                if (request.IsOutPut)
                    ret.rows = sql.ToList<WorkOrderListModel>();
                else
                    ret = sql.ToPageList<WorkOrderListModel>(request.rows, request.page);
                //获取加工订单配送完成时间
                var dataList = (List<WorkOrderListModel>)ret.rows;
                if (dataList.Count > 0)
                {
                    string orderStr = string.Join("','", dataList.Select(p => p.OrderCode).ToList());
                    var completeTimeData = GetCompleteTimeByOrder(orderStr);
                    var signStateData = GetSignStateByOrder(orderStr);
                    dataList.ForEach(x =>
                    {
                        var completeTime = completeTimeData.FirstOrDefault(p => p.OrderCode == x.OrderCode);
                        if (string.IsNullOrEmpty(completeTime.LoadCompleteTime))
                            completeTime.LoadCompleteTime = "";
                        else
                            completeTime.LoadCompleteTime = DateTime.Parse(completeTime.LoadCompleteTime).ToString("yyyy-MM-dd");
                        x.LoadCompleteTime = completeTime.LoadCompleteTime;
                        var signStated = signStateData.FirstOrDefault(p => p.OrderCode == x.OrderCode);
                        if (signStated.total == 0 || x.DistributionStart == "" || x.DistributionStart == "未配送")
                            x.SignState = "";
                        else if (signStated.number > 0)
                            x.SignState = "未签收";
                        else
                            x.SignState = "已签收";
                    });
                    ret.rows = dataList;
                }
                return ret;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 获取未配送、已配送订单
        /// </summary>
        /// <param name="request"></param>
        /// <param name="OrderType"></param>
        /// <returns></returns>
        public List<string> GetWpsOrderList(WorkOrderRequest request, string OrderType)
        {
            //获取所有的站点
            StringBuilder sbSiteCode = new StringBuilder();
            string sqlwhere = "";
            if (!string.IsNullOrWhiteSpace(request.ProcessingState))
            {
                sqlwhere += " and ProcessingState='" + request.ProcessingState + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.TypeCode))
            {
                sqlwhere += " and TypeCode like '%" + request.TypeCode + "%'";
            }
            if (!string.IsNullOrWhiteSpace(request.UrgentDegree))
            {
                sqlwhere += " and UrgentDegree='" + request.UrgentDegree + "'";
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
                List<string> SiteList = GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);
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

            string sql = "";
            if (OrderType == "未配送订单量")
            {
                sql = @"select Tb.OrderCode  from TbDistributionPlanInfo Tb 
                        left join TbDistributionPlanDetailInfo dpdi on Tb.OrderCode=dpdi.OrderCode
                        where dpdi.PSAmount>0 and dpdi.RevokeStart='正常' " + sqlwhere + " group by Tb.OrderCode";
            }
            else
            {
                sql = @"select Tb.OrderCode  from TbDistributionPlanInfo Tb 
                        left join TbDistributionPlanDetailInfo dpdi on Tb.OrderCode=dpdi.OrderCode
                        where dpdi.PSAmount!=dpdi.Number and dpdi.RevokeStart='正常' " + sqlwhere + " group by Tb.OrderCode";
            }
            var dt = Db.Context.FromSql(sql).ToList<string>();
            return dt;
        }
        /// <summary>
        /// 获取该组织机构下的所有站点
        /// </summary>
        /// <param name="parentCode"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<string> GetCompanyWorkAreaOrSiteList(string parentCode, int type)
        {
            string sql = @"WITH TREE AS(SELECT * FROM TbCompany WHERE CompanyCode =@parentCode UNION ALL SELECT TbCompany.* FROM TbCompany, TREE WHERE TbCompany.ParentCompanyCode = TREE.CompanyCode) SELECT CompanyCode FROM TREE where TREE.OrgType=@type";
            var dt = Db.Context.FromSql(sql)
                .AddInParameter("@parentCode", DbType.String, parentCode)
                .AddInParameter("@type", DbType.Int32, type).ToList<string>();
            if (!dt.Any())
                dt.Add(parentCode);
            return dt;
        }
        /// <summary>
        /// 获取组织机构下的所有下级
        /// </summary>
        /// <param name="CompanyId"></param>
        /// <returns></returns>
        public List<string> GetCompanyAllChild(string CompanyId)
        {
            string sqlChilde = @"with t as (select CompanyCode from TbCompany where ParentCompanyCode='" + CompanyId + "' union all select a.CompanyCode from TbCompany a join t b on a.ParentCompanyCode=b.CompanyCode) select * from t";
            var dt = Db.Context.FromSql(sqlChilde).ToList<string>();
            return dt;
        }

        public DataTable GetWorkOrderReportForm(WorkOrderRequest request)
        {
            //获取所有的站点
            StringBuilder sbSiteCode = new StringBuilder();
            string sqlChilde = "";
            string sqlwhere = "";
            if (!string.IsNullOrWhiteSpace(request.ProcessingState))
            {
                sqlwhere += " and ProcessingState='" + request.ProcessingState + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.TypeCode))
            {
                sqlwhere += " and TypeCode like '%" + request.TypeCode + "%'";
            }
            if (!string.IsNullOrWhiteSpace(request.UrgentDegree))
            {
                sqlwhere += " and UrgentDegree='" + request.UrgentDegree + "'";
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
                List<string> SiteList = GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);
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

            sqlChilde = @"--订单总量(订单状态除未接收外的订单)
                        select isnull(Convert(decimal(18,5),SUM(wod.MeasurementUnitZl*wod.ItemUseNum*wod.Number)),0) as SumWeightTotal,'1' as OrderType from TbWorkOrder Tb
                        left join TbWorkOrderDetail wod on Tb.OrderCode=wod.OrderCode
                        where Tb.ProcessingState!='ConfirmWork' and wod.RevokeStart='正常' " + sqlwhere + @"
                        union all
                        --已接收总量
                        select  isnull(Convert(decimal(18,5),SUM(wod.MeasurementUnitZl*wod.ItemUseNum*wod.Number)),0) as SumWeightTotal,'2' as OrderType from TbWorkOrder Tb
                        left join TbWorkOrderDetail wod on Tb.OrderCode=wod.OrderCode
                        where Tb.ProcessingState='Received'  and wod.RevokeStart='正常' " + sqlwhere + @"
                        union all
                        --已领料总量
                        select  isnull(Convert(decimal(18,5),SUM(wod.MeasurementUnitZl*wod.ItemUseNum*wod.Number)),0) as SumWeightTotal,'3' as OrderType from TbWorkOrder Tb
                        left join TbWorkOrderDetail wod on Tb.OrderCode=wod.OrderCode
                        where Tb.ProcessingState='AlreadyReceived'  and wod.RevokeStart='正常' " + sqlwhere + @"
                        union all
                        --加工中总量
                        select  isnull(Convert(decimal(18,5),SUM(wod.MeasurementUnitZl*wod.ItemUseNum*wod.Number)),0) as SumWeightTotal,'4' as OrderType from TbWorkOrder Tb
                        left join TbWorkOrderDetail wod on Tb.OrderCode=wod.OrderCode
                        where Tb.ProcessingState='Processing'  and wod.RevokeStart='正常' " + sqlwhere + @"
                        union all
                        select isnull(Convert(decimal(18,5),sum(dpdi.MeasurementUnitZl*dpdi.ItemUseNum*dpdi.PSAmount)),0) as SumWeightTotal,'5' as OrderType  from TbDistributionPlanInfo Tb 
                        left join TbDistributionPlanDetailInfo dpdi on Tb.OrderCode=dpdi.OrderCode
                        where dpdi.PSAmount>0 and dpdi.RevokeStart='正常' " + sqlwhere + @"
                        union all
                        select isnull(Convert(decimal(18,5),sum(dpdi.MeasurementUnitZl*dpdi.ItemUseNum*(dpdi.Number-dpdi.PSAmount))),0) as SumWeightTotal,'6' as OrderType  from TbDistributionPlanInfo Tb 
                        left join TbDistributionPlanDetailInfo dpdi on Tb.OrderCode=dpdi.OrderCode
                        where dpdi.PSAmount!=dpdi.Number and dpdi.RevokeStart='正常' " + sqlwhere + @"";

            var dt = Db.Context.FromSql(sqlChilde).ToDataTable();
            return dt;
        }

        /// <summary>
        /// 获取加工订单配送完成时间
        /// </summary>
        /// <returns></returns>
        public List<SignStateListModel> GetCompleteTimeByOrder(string orders)
        {
            string sql = @"SELECT two.OrderCode,(
            	case 
            				when two.IsOffline=0
            				THEN(
            SELECT top 1 de.LoadCompleteTime FROM TbDistributionEntOrder deo
            LEFT JOIN TbDistributionEnt de ON deo.DistributionCode=de.DistributionCode
            WHERE deo.OrderCode LIKE '%'+two.OrderCode+'%'
            ORDER BY de.LoadCompleteTime desc
            )ELSE
            	(
            		SELECT DeliveryCompleteTime
            		FROM TbDistributionPlanInfo
            		WHERE OrderCode=two.OrderCode
            	)
            	END
            ) AS LoadCompleteTime 
            FROM TbWorkOrder two
            WHERE two.OrderCode IN('" + orders + "')";
            var dt = Db.Context.FromSql(sql).ToList<SignStateListModel>();
            return dt;
        }

        /// <summary>
        /// 获取加工订单签收状态
        /// </summary>
        /// <returns></returns>
        public List<SignStateListModel> GetSignStateByOrder(string orders)
        {
            string sql = @"SELECT two.OrderCode,(case 
            				when two.IsOffline=0
            				THEN(
                            SELECT COUNT(1) AS number FROM TbDistributionEntOrder 
                            WHERE OrderCode LIKE '%'+two.OrderCode+'%')
                            ELSE
            	(
            		SELECT COUNT(1) FROM TbSemiFinishedSign WHERE OrderCodeH=two.OrderCode
            		)
            	END) total,(
            		case 
            				when two.IsOffline=0
            				THEN(
                            SELECT COUNT(1) AS number FROM TbDistributionEntOrder 
                            WHERE OrderCode LIKE '%'+two.OrderCode+'%'AND SignState='未签收')  ELSE
            	(
            		SELECT COUNT(1) FROM TbSemiFinishedSign WHERE OrderCodeH=two.OrderCode AND OperateState='未签收'
            		)
            	END) number
                            FROM TbWorkOrder two
                            WHERE two.OrderCode IN('" + orders + "')";
            var dt = Db.Context.FromSql(sql).ToList<SignStateListModel>();
            return dt;
        }
        /// <summary>
        /// 获取加工订单签收状态
        /// </summary>
        /// <returns></returns>
        public List<string> GetSignStateBySearch(string signState)
        {
            //signState = "已签收";
            //string text = "已签收";
            //if (signState == "已签收")
            //    text = "未签收";
            var dataS = Repository<TbDistributionEntOrder>.Query(p => p.SignState == signState).ToList();
            if (dataS.Count == 0)
                return new List<string>();
            List<string> listH = new List<string>();
            if (signState == "已签收")
            {
                var dataH = Repository<TbDistributionEntOrder>.Query(p => p.SignState == "未签收")
                                                          .Select(p => p.OrderCode).Distinct().ToList();
                if (dataH.Any())
                {
                    string order = string.Join(",", dataH);
                    listH = order.Split(',').Distinct().ToList();
                }
            }
            else
            {
                listH = Repository<TbDistributionPlanInfo>.Query(p => p.DistributionStart == "未配送")
                                                          .Select(p => p.OrderCode).Distinct().ToList();
            }
            dataS = dataS.Where(p => !listH.Contains(p.OrderCode)).ToList();
            if (dataS.Count == 0)
                return new List<string>();
            var dt = dataS.Select(p => p.OrderCode).ToList();
            string ret = string.Join(",", dt);
            var d = ret.Split(',').Distinct().ToList();
            if (listH.Any())
                d = d.Where(p => !listH.Contains(p)).ToList();
            //加入线下订单
            var so = Repository<TbSemiFinishedSign>.Query(p => p.OperateState == signState && p.OrderCodeH != null)
                                                   .Select(p => p.OrderCodeH).ToList();
            d.AddRange(so);
            return d;
        }

        #endregion

        #region 判断
        /// <summary>
        /// 判断信息是否可操作
        /// </summary>
        /// <returns></returns>
        public AjaxResult AnyInfo(int keyValue)
        {
            var monthDemandPlan = Repository<TbWorkOrder>.First(p => p.ID == keyValue);
            if (monthDemandPlan == null)
                return AjaxResult.Warning("信息不存在");
            if (monthDemandPlan.Examinestatus != "未发起" && monthDemandPlan.Examinestatus != "已退回")
                return AjaxResult.Warning("信息正在审核中或已审核完成,不能进行此操作");

            return AjaxResult.Success(monthDemandPlan);
        }
        /// <summary>
        /// 判断信息是否线上订单
        /// </summary>
        /// <returns></returns>
        public AjaxResult IsOffline(int keyValue)
        {
            var workOrder = Repository<TbWorkOrder>.First(p => p.ID == keyValue);
            if (workOrder == null)
                return AjaxResult.Warning("信息不存在");
            if (workOrder.Examinestatus != "审核完成")
                return AjaxResult.Warning("订单还未审核通过,不能进行此操作");
            //查找订单进度是否可进行此操作
            var isAnyData = Repository<TbOrderProgressDetail>.Any(p => p.OrderCode == workOrder.OrderCode
                                                     && p.RevokeStart == "正常" && p.DaetailWorkStrat == "加工完成");
            if (!isAnyData)
                return AjaxResult.Warning("订单还未加工完成,不能进行此操作");
            //判断是否已配送完成
            var plan = Repository<TbDistributionPlanInfo>.First(p => p.OrderCode == workOrder.OrderCode);
            if (plan.DistributionStart == "配送完成")
                return AjaxResult.Warning("订单已配送完成");
            //判断订单是否有数据正在运输途中
            var orderAny = Repository<TbDistributionEntOrder>.Any(p => p.OrderCode.Like(workOrder.OrderCode) && p.SignState == "未签收");
            if (orderAny)
                return AjaxResult.Warning("订单正在配送中,不能进行此操作");
            return AjaxResult.Success(workOrder);
        }
        #endregion

        #region 获取当前登录人下的所有工区\站点
        public DataTable GetCompanyCompanyAllSiteList()
        {
            string sql = "";
            var orgType = OperatorProvider.Provider.CurrentUser.OrgType;
            if (orgType == "0" || orgType == "1" || orgType == "2")
            {
                sql = @"select cp.id,cp.CompanyCode,cp.CompanyFullName,cp.ParentCompanyCode,cp.Address,cp.OrgType from  TbCompany cp
left join TbProjectCompany pc on cp.CompanyCode=pc.CompanyCode where pc.ProjectId=@ProjectId and cp.OrgType=5 order by cp.id asc";
            }
            else
            {
                sql = @"WITH TREE AS(SELECT * FROM TbCompany WHERE CompanyCode =@parentCode UNION ALL SELECT TbCompany.* FROM TbCompany, TREE WHERE TbCompany.ParentCompanyCode = TREE.CompanyCode) SELECT CompanyCode,ParentCompanyCode,CompanyFullName,CompanyShortName,OrgType,Address FROM TREE where TREE.OrgType=5";
            }
            DataTable dt = Db.Context.FromSql(sql)
                .AddInParameter("@ProjectId", DbType.String, ProjectId)
                .AddInParameter("@parentCode", DbType.String, OperatorProvider.Provider.CurrentUser.CompanyId).ToDataTable();
            return dt;
        }

        public PageModel GetCompanyCompanyAllSiteList(TbCompanyRequest request)
        {
            string sql = "";
            var orgType = OperatorProvider.Provider.CurrentUser.OrgType;
            var parentCode = OperatorProvider.Provider.CurrentUser.CompanyId;
            string where = "";
            //参数化
            List<Parameter> parameter = new List<Parameter>();
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where += " and ProjectId=@ProjectId";
                parameter.Add(new Parameter("@ProjectId", request.ProjectId, DbType.String, null));
            }
            if (!string.IsNullOrWhiteSpace(request.keyword))
            {
                where += " and CompanyFullName like @CompanyFullName";
                parameter.Add(new Parameter("@CompanyFullName", '%' + request.keyword + '%', DbType.String, null));
            }
            if (orgType == "0" || orgType == "1" || orgType == "2")
            {
                sql = @"select cm.id,cm.CompanyCode,cm.CompanyFullName,cm.OrgType,cm.Address,pc.ProjectId from TbCompany cm
left join TbProjectCompany pc on cm.CompanyCode=pc.CompanyCode where 1=1 and cm.OrgType=5 ";

            }
            else
            {
                sql = @"select * from GetCompanyChild_fun(@parentCode) as cp where 1=1 and cp.OrgType=5 ";
                parameter.Add(new Parameter("@parentCode", parentCode, DbType.String, null));
            }
            var model = Repository<TbCompany>.FromSqlToPageTable(sql + where, parameter, request.rows, request.page, "ID", "asc");
            return model;

        }

        #endregion

        #region 选择加工订单
        public PageModel GetWorkOrderList(WorkOrderRequest request)
        {
            try
            {
                //参数化
                List<Parameter> parameter = new List<Parameter>();
                string where = " where 1=1 and wo.Examinestatus='审核完成' and (wo.ProcessingState='Processing' or wo.ProcessingState='Finishing') and wo.ProjectId=@ProjectId ";
                parameter.Add(new Parameter("@ProjectId", ProjectId, DbType.String, null));
                string sql = @"select OrderCode,TypeCode,TypeName,ProcessFactoryCode,c1.CompanyFullName as ProcessFactoryName,SiteCode,c2.CompanyFullName as SiteName,UsePart,DistributionTime from TbWorkOrder wo
left join TbCompany c1 on wo.ProcessFactoryCode=c1.CompanyCode
left join TbCompany c2 on wo.SiteCode=c2.CompanyCode";
                if (!string.IsNullOrWhiteSpace(request.keyword))
                {
                    where += " and OrderCode like @keyword or TypeCode like @keyword or TypeName like @keyword or c1.CompanyFullName like @keyword or c2.CompanyFullName like @keyword";
                    parameter.Add(new Parameter("@keyword", '%' + request.keyword + '%', DbType.String, null));
                }
                if (!string.IsNullOrEmpty(CompanyList()))
                {
                    where += " and wo.SiteCode in(" + CompanyList() + ")";
                }
                var model = Repository<TbWorkOrder>.FromSqlToPageTable(sql + where, parameter, request.rows, request.page, "OrderCode", "desc");
                return model;
                //                string sql = "";
                //                if (!string.IsNullOrEmpty(CompanyList()))
                //                {
                //                    sql = @"select OrderCode,TypeCode,TypeName,ProcessFactoryCode,c1.CompanyFullName as ProcessFactoryName,SiteCode,c2.CompanyFullName as SiteName,UsePart,DistributionTime from TbWorkOrder wo
                //left join TbCompany c1 on wo.ProcessFactoryCode=c1.CompanyCode
                //left join TbCompany c2 on wo.SiteCode=c2.CompanyCode where 1=1 and wo.SiteCode in(" + CompanyList() + ")";
                //                }
                //                else
                //                {
                //                    sql = @"select OrderCode,TypeCode,TypeName,ProcessFactoryCode,c1.CompanyFullName as ProcessFactoryName,SiteCode,c2.CompanyFullName as SiteName,UsePart,DistributionTime from TbWorkOrder wo
                //left join TbCompany c1 on wo.ProcessFactoryCode=c1.CompanyCode
                //left join TbCompany c2 on wo.SiteCode=c2.CompanyCode where 1=1 ";
                //                }
                //                DataTable dt = Db.Context.FromSql(sql).ToDataTable();
                //                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region 加工订单打包
        public PageModel GetNowSiteWorkOrderDetailList(WorkOrderPackResponse request)
        {
            StringBuilder sbSiteCode = new StringBuilder();
            #region 模糊搜索条件

            string where = "";
            if (!string.IsNullOrWhiteSpace(request.OrderCode))
            {
                where += " and wo.OrderCode='" + request.OrderCode + "'";
            }
            if (request.DistributionTime != null)
            {
                DateTime date = Convert.ToDateTime(request.DistributionTime);
                string date1 = date.ToString("yyyy/MM/dd");

                where += " and CONVERT(varchar(100),wo.DistributionTime, 111)='" + date1 + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.DistributionStart))
            {
                where += " and tdpi.DistributionStart='" + request.DistributionStart + "'";
            }
            if (!string.IsNullOrEmpty(request.TypeCode))
                where += " and wo.TypeCode='" + request.TypeCode + "'";
            #endregion

            #region 数据权限

            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where += " and wo.ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where += " and wo.ProjectId='" + request.ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);
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
                    where += " and wo.SiteCode in(" + sbSiteCode + ")";
                }
            }

            #endregion

            string sql = @"select 
                            tdpi.DistributionStart,wod.ID,wo.ProjectId,wo.ProcessFactoryCode,wo.OrderCode,
                            wo.UsePart,
                            wo.TypeCode,wo.TypeName,wo.SiteCode,co.CompanyFullName as SiteName,
                            wod.ComponentName,wod.Number,wod.PackNumber,
                            wod.WeightSmallPlan,
                            wod.SpecificationModel,
                            wod.ItemUseNum,
                            tpi.ProjectName+dbo.GetCompanyParentName_fun(wo.SiteCode) AS OrgName,
                            op.FinishProcessingDateTime,
                            tdpi.DistributionTime,
                            GETDATE() AS PageTime,
                            case when wod.PackNumber=0 then 0 
                            else FLOOR(wod.Number/wod.PackNumber) 
                            end as lldyNum,
                            case when wod.PackNumber=0 then Number-0 
                            else Number-PackNumber*FLOOR(wod.Number/wod.PackNumber) 
                            end as syNum 
                            from TbWorkOrder wo
                            left join TbWorkOrderDetail wod on wo.OrderCode=wod.OrderCode
                            left join TbCompany co on wo.SiteCode=co.CompanyCode
                            LEFT JOIN TbDistributionPlanInfo tdpi ON wo.OrderCode=tdpi.OrderCode
                            left JOIN TbOrderProgress op on wo.OrderCode=op.OrderCode
                            LEFT JOIN TbProjectInfo tpi ON wo.ProjectId=tpi.ProjectId 
                            where 1=1 and wo.ProcessingState='Finishing' and wod.RevokeStart='正常' ";
            //参数化
            List<Parameter> parameter = new List<Parameter>();
            var model = Repository<TbWorkOrderDetail>.FromSqlToPageTable(sql + where, parameter, request.rows, request.page, "ID", "desc");
            return model;
        }

        #endregion

        #region 获取当前组织机构已经子级组织机构
        public string CompanyList()
        {
            string sqlCompany = "";
            string compCode = "";
            var orgType = OperatorProvider.Provider.CurrentUser.OrgType;
            if (orgType == "0" || orgType == "1" || orgType == "2")
            {
                sqlCompany = @"SELECT cp.CompanyCode FROM TbCompany cp 
left join TbProjectCompany pcp on cp.CompanyCode=pcp.CompanyCode where cp.OrgType=5 and ProjectId=@ProjectId";
                DataTable dtCompany = Db.Context.FromSql(sqlCompany).AddInParameter("@ProjectId", DbType.String, OperatorProvider.Provider.CurrentUser.ProjectId).ToDataTable();
                if (dtCompany != null && dtCompany.Rows.Count > 0)
                {
                    for (int i = 0; i < dtCompany.Rows.Count; i++)
                    {
                        if (i == dtCompany.Rows.Count - 1)
                        {
                            compCode += "'" + dtCompany.Rows[i][0] + "'";
                        }
                        else
                        {
                            compCode += "'" + dtCompany.Rows[i][0] + "',";
                        }
                    }
                }
            }
            else
            {
                sqlCompany = @"select * from GetCompanyChild_fun(@parentCode) as TREE where TREE.OrgType=5";
                DataTable dtCompany = Db.Context.FromSql(sqlCompany)
               .AddInParameter("@parentCode", DbType.String, OperatorProvider.Provider.CurrentUser.CompanyId).ToDataTable();
                if (dtCompany != null && dtCompany.Rows.Count > 0)
                {
                    for (int i = 0; i < dtCompany.Rows.Count; i++)
                    {
                        if (i == dtCompany.Rows.Count - 1)
                        {
                            compCode += "'" + dtCompany.Rows[i][0] + "'";
                        }
                        else
                        {
                            compCode += "'" + dtCompany.Rows[i][0] + "',";
                        }
                    }
                }
            }

            return compCode;
        }

        public DataTable GetCompany(string CompanyId)
        {
            var ret = Db.Context.From<TbCompany>()
             .Select(
                     TbCompany._.CompanyCode
                     , TbCompany._.CompanyFullName
                     , TbCompany._.Address)
                     .Where(p => p.CompanyCode == CompanyId).ToDataTable();
            return ret;

        }
        #endregion

        #region App
        /// <summary>
        /// 加急订单App首页展示更多
        /// </summary>
        /// <returns></returns>
        public DataTable GetUrgentWorkOrderSumWeight(string ProjectId, string ProcessFactoryCode, string orgId, string orgType)
        {
            string where = "";
            if (!string.IsNullOrWhiteSpace(orgId))
            {
                if (!string.IsNullOrWhiteSpace(orgType) && orgType != "1")
                {
                    List<string> SiteList = GetCompanyWorkAreaOrSiteList(orgId, 5);//站点
                    string siteStr = string.Join("','", SiteList);
                    where += "and wo.SiteCode in('" + siteStr + "')";
                }
            }
            string sql = @"select COUNT(1) as SumNum,sum(Tb.WeightSmallPlan) as WeightSmallPlan from (select Convert(decimal(18,2),SUM(wod.WeightSmallPlan)) as WeightSmallPlan from TbWorkOrder wo left join TbWorkOrderDetail wod on wo.OrderCode=wod.OrderCode left join TbCompany cm on wo.SiteCode=cm.CompanyCode left join TbCompany cm1 on wo.ProcessFactoryCode = cm1.CompanyCode 
                           where OrderStart='加急订单' and (isnull(@ProjectId,'')='' or wo.ProjectId=@ProjectId) 
                           and (ISNULL(@ProcessFactoryCode,'')='' or wo.ProcessFactoryCode=@ProcessFactoryCode) " + where + @"
                           group by wo.OrderCode,cm1.CompanyFullName,wo.TypeName,cm.CompanyFullName,wo.TypeCode,wo.UsePart,DistributionTime) Tb";
            DataTable dt = Db.Context.FromSql(sql)
                .AddInParameter("@ProjectId", DbType.String, ProjectId)
                .AddInParameter("@ProcessFactoryCode", DbType.String, ProcessFactoryCode)
                .ToDataTable();

            return dt;
        }

        /// <summary>
        /// 加急订单App首页展示更多
        /// </summary>
        /// <returns></returns>
        public DataTable GetUrgentWorkOrderList(string ProjectId, string ProcessFactoryCode, string orgId, string orgType)
        {
            string where = "";
            if (!string.IsNullOrWhiteSpace(orgId))
            {
                if (!string.IsNullOrWhiteSpace(orgType) && orgType != "1")
                {
                    List<string> SiteList = GetCompanyWorkAreaOrSiteList(orgId, 5);//站点
                    string siteStr = string.Join("','", SiteList);
                    where += "and wo.SiteCode in('" + siteStr + "')";
                }
            }
            string sql = @"select wo.OrderCode,cm1.CompanyFullName as ProcessFactoryName,wo.TypeName,cm.CompanyFullName as SiteName,wo.TypeCode,wo.UsePart,DistributionTime,Convert(decimal(18,2),SUM(wod.WeightSmallPlan)) as WeightSmallPlan from TbWorkOrder wo left join TbWorkOrderDetail wod on wo.OrderCode=wod.OrderCode left join TbCompany cm on wo.SiteCode=cm.CompanyCode left join TbCompany cm1 on wo.ProcessFactoryCode = cm1.CompanyCode 
                           where OrderStart='加急订单' and (isnull(@ProjectId,'')='' or wo.ProjectId=@ProjectId) 
                           and (ISNULL(@ProcessFactoryCode,'')='' or wo.ProcessFactoryCode=@ProcessFactoryCode) " + where + @"
                           group by wo.OrderCode,cm1.CompanyFullName,wo.TypeName,cm.CompanyFullName,wo.TypeCode,wo.UsePart,DistributionTime";
            DataTable dt = Db.Context.FromSql(sql)
                .AddInParameter("@ProjectId", DbType.String, ProjectId)
                .AddInParameter("@ProcessFactoryCode", DbType.String, ProcessFactoryCode)
                .ToDataTable();

            return dt;
        }

        /// <summary>
        /// 库存数量、配送数量、签收数量、订单加工完成总量
        /// </summary>
        /// <returns></returns>
        public DataTable GetTotelNum(string ProjectId, string ProcessFactoryCode, string orgId, string orgType, int Year, int Month)
        {
            string where1 = "";
            string where2 = "";
            if (!string.IsNullOrWhiteSpace(orgId))
            {
                if (!string.IsNullOrWhiteSpace(orgType) && orgType != "1")
                {
                    List<string> SiteList = GetCompanyWorkAreaOrSiteList(orgId, 5);//站点
                    List<string> WorkAreaList = GetCompanyWorkAreaOrSiteList(orgId, 4);//工区
                    string siteStr = string.Join("','", SiteList);
                    string workAreaStr = string.Join("','", WorkAreaList);
                    where1 += "and (rsr.SiteCode in('" + siteStr + "') or rsr.WorkAreaCode in('" + workAreaStr + "'))";
                    where2 += "and SiteCode in('" + siteStr + "')";
                }
            }
            string sql = @"--库存量
                           select isnull(sum(rsr.Count),0) as TotelNum from TbRawMaterialStockRecord rsr
                           left join TbStorage s on rsr.StorageCode=s.StorageCode
                           where (ISNULL(@ProjectId,'')='' or ProjectId=@ProjectId) and rsr.ChackState=1
                           and (ISNULL(@ProcessFactoryCode,'')='' or s.ProcessFactoryCode=@ProcessFactoryCode) " + where1 + @"
                           union all
                           --配送量
                           select isnull(SUM(WeightTotal),0.0000) DistributionTotal 
                           from TbDistributionPlanInfo 
                           where (ISNULL(@ProjectId,'')='' or ProjectId=@ProjectId) 
                           --and datediff(month,InsertTime,getdate())=0
                           and YEAR(InsertTime)=@Year and MONTH(InsertTime)=@Month and DistributionStart='配送完成'
                           and (ISNULL(@ProcessFactoryCode,'')='' or ProcessFactoryCode=@ProcessFactoryCode) " + where2 + @"
                           union all
                           --签收量
                           select isnull(Sum(SumTotal),0.0000) SumTotal 
                           from TbSemiFinishedSign 
                           where (ISNULL(@ProjectId,'')='' or ProjectId=@ProjectId) 
                           --and datediff(month,SigninTime,getdate())=0
                           and YEAR(SigninTime)=@Year and MONTH(SigninTime)=@Month
                           and (ISNULL(@ProcessFactoryCode,'')='' or ProcessFactoryCode=@ProcessFactoryCode) " + where2 + @"
                           union all
                           --加工完成量
                           select isnull(sum(opd.AlreadyCompleted),0) as AlreadyCompleted 
                           from TbOrderProgress op left join TbOrderProgressDetail opd on op.OrderCode=opd.OrderCode  
                           --where MONTH(op.InsertTime)=DATENAME(MONTH,GETDATE()) 
                           and YEAR(op.InsertTime)=@Year and MONTH(op.InsertTime)=@Month
                           and WeightSmallPlan=AlreadyCompleted 
                           and (ISNULL(@ProjectId,'')='' or op.ProjectId=@ProjectId)
                           and (ISNULL(@ProcessFactoryCode,'')='' or ProcessFactoryCode=@ProcessFactoryCode)  " + where2 + @"";
            DataTable dt = Db.Context.FromSql(sql)
                           .AddInParameter("@ProjectId", DbType.String, ProjectId)
                           .AddInParameter("@ProcessFactoryCode", DbType.String, ProcessFactoryCode)
                           .AddInParameter("@Year", DbType.Int32, Year)
                           .AddInParameter("@Month", DbType.Int32, Month).ToDataTable();
            return dt;
        }

        public DataTable GetCapacityNum(string ProcessFactoryCode, string CapacityMonth)
        {
            string sql = @"select isnull(sum(TbCF.Capacity),0) as Capacity,isnull(sum(TbCF.WeightSmallPlan),0) as WeightSmallPlan,isnull(cast(sum(TbCF.WeightSmallPlan)/sum(TbCF.Capacity)*100 as decimal(18,2)),0) as ActualLoadNew from (select cf.Capacity,Tb1.WeightSmallPlan,cf.ProcessFactoryCode from TbCapacityFilling cf  
left join(
select SUM(Tb.WeightSmallPlan) as WeightSmallPlan,Tb.ProcessFactoryCode from (
                            select isnull(SUM(wod.WeightSmallPlan),0) as WeightSmallPlan,wo.ProcessFactoryCode from TbWorkOrder wo
                            left join TbWorkOrderDetail wod on wo.OrderCode=wod.OrderCode
                            where (ISNULL(@ProcessFactoryCode,'')='' or wo.ProcessFactoryCode=@ProcessFactoryCode)
                            and CONVERT(varchar(7), wo.DistributionTime, 120)=@month
                            and wod.RevokeStart='正常' and wo.ProcessingState!='ConfirmWork' and wo.ProcessingState!='Finishing'  group by wo.ProcessFactoryCode
                            union all
                            select isnull(sum(opd.NoCompleted),0) as NoCompleted,op.ProcessFactoryCode from TbOrderProgress op
                            left join TbOrderProgressDetail opd on op.OrderCode=opd.OrderCode 
                            where (ISNULL(@ProcessFactoryCode,'')='' or op.ProcessFactoryCode=@ProcessFactoryCode)
                            and CONVERT(varchar(7), op.DistributionTime, 120)=CONVERT(varchar(4),YEAR(DATEADD(MONTH,-1,@month+'-01')))+'-'+Right(100+Month(DATEADD(MONTH,-1,@month+'-01')),2)
                            and opd.RevokeStart='正常'  group by op.ProcessFactoryCode)Tb  group by Tb.ProcessFactoryCode
) Tb1 on cf.ProcessFactoryCode=Tb1.ProcessFactoryCode 
where cf.CapacityMonth=@month
and (ISNULL(@ProcessFactoryCode,'')='' or cf.ProcessFactoryCode=@ProcessFactoryCode)) TbCF where (ISNULL(@ProcessFactoryCode,'')='' or TbCF.ProcessFactoryCode=@ProcessFactoryCode)";
            DataTable dt = Db.Context.FromSql(sql)
                           .AddInParameter("@ProcessFactoryCode", DbType.String, ProcessFactoryCode)
                           .AddInParameter("@month", DbType.String, CapacityMonth).ToDataTable();
            return dt;
        }

        /// <summary>
        /// 供货延时App首页展示更多
        /// </summary>
        /// <returns></returns>
        public DataTable GetDelayedSupplyList(string ProjectId, string ProcessFactoryCode, string orgId, string orgType)
        {
            string where = "";
            if (!string.IsNullOrWhiteSpace(orgId))
            {
                if (!string.IsNullOrWhiteSpace(orgType) && orgType != "1")
                {
                    List<string> SiteList = GetCompanyWorkAreaOrSiteList(orgId, 5);//站点
                    string siteStr = string.Join("','", SiteList);
                    where += "and sl.SiteCode in('" + siteStr + "')";
                }
            }
            string sql = @"select sl.BatchPlanNum,sl.BranchCode,c1.CompanyFullName as BranchName,sl.WorkAreaCode,c4.CompanyFullName as WorkAreaName,sl.SiteCode,c2.CompanyFullName as SiteName,sl.ProcessFactoryCode,c3.CompanyFullName as ProcessFactoryName,sl.SupplyDate,isnull(sl.BatchPlanTotal,0) as BatchPlanTotal,isnull(sl.HasSupplierTotal,0) as HasSupplierTotal,(isnull(sl.BatchPlanTotal,0)-isnull(sl.HasSupplierTotal,0)) as NoSupplierTotal from TbSupplyList sl
                           left join TbCompany c1 on c1.CompanyCode=sl.BranchCode
                           left join TbCompany c2 on c2.CompanyCode=sl.SiteCode
						   left join TbCompany c4 on c4.CompanyCode=sl.WorkAreaCode
                           left join TbCompany c3 on c3.CompanyCode=sl.ProcessFactoryCode 
                           where StateCode='未供货'  
                           and GETDATE()>SupplyDate 
                           and (isnull(@ProjectId,'')='' or sl.ProjectId=@ProjectId)
                           and (ISNULL(@ProcessFactoryCode,'')='' or sl.ProcessFactoryCode=@ProcessFactoryCode) " + where + "";
            DataTable dt = Db.Context.FromSql(sql)
                           .AddInParameter("@ProjectId", DbType.String, ProjectId)
                           .AddInParameter("@ProcessFactoryCode", DbType.String, ProcessFactoryCode).ToDataTable();

            return dt;
        }
        /// <summary>
        /// 供货延时App首页展示
        /// </summary>
        /// <param name="ProjectId"></param>
        /// <param name="ProcessFactoryCode"></param>
        /// <param name="orgId"></param>
        /// <param name="orgType"></param>
        /// <returns></returns>
        public DataTable GetDelayedSupplySumWeight(string ProjectId, string ProcessFactoryCode, string orgId, string orgType)
        {
            string where = "";
            if (!string.IsNullOrWhiteSpace(orgId))
            {
                if (!string.IsNullOrWhiteSpace(orgType) && orgType != "1")
                {
                    List<string> SiteList = GetCompanyWorkAreaOrSiteList(orgId, 5);//站点
                    string siteStr = string.Join("','", SiteList);
                    where += "and sl.SiteCode in('" + siteStr + "')";
                }
            }
            string sql = @"select Count(1) as SumNum,sum(isnull(sl.BatchPlanTotal,0)-isnull(sl.HasSupplierTotal,0)) as SumNoSupplierTotal from TbSupplyList sl
                           left join TbCompany c1 on c1.CompanyCode=sl.BranchCode
                           left join TbCompany c2 on c2.CompanyCode=sl.SiteCode
						   left join TbCompany c4 on c4.CompanyCode=sl.WorkAreaCode
                           left join TbCompany c3 on c3.CompanyCode=sl.ProcessFactoryCode 
                           where StateCode='未供货' 
                           and GETDATE()>SupplyDate 
                           and (isnull(@ProjectId,'')='' or sl.ProjectId=@ProjectId)
                           and (ISNULL(@ProcessFactoryCode,'')='' or sl.ProcessFactoryCode=@ProcessFactoryCode) " + where + "";
            DataTable dt = Db.Context.FromSql(sql)
                           .AddInParameter("@ProjectId", DbType.String, ProjectId)
                           .AddInParameter("@ProcessFactoryCode", DbType.String, ProcessFactoryCode).ToDataTable();

            return dt;
        }

        /// <summary>
        /// 订单报警（加工状态为已接收、加工中）
        /// </summary>
        /// <returns></returns>
        public DataTable GetWorkOrderStartList(string ProjectId)
        {
            string sql = @"select OrderCode,Examinestatus,OrderStart,ProcessingState,TypeCode,TypeName,SiteCode,cp.CompanyFullName as SiteName,DistributionTime from TbWorkOrder
left join TbCompany cp on TbWorkOrder.SiteCode=cp.CompanyCode
 where (isnull(@ProjectId,'')='' or ProjectId=@ProjectId) and OrderStart='加急订单' and (ProcessingState='Received' or ProcessingState='Processing')";
            DataTable dt = Db.Context.FromSql(sql).AddInParameter("@ProjectId", DbType.String, ProjectId).ToDataTable();
            return dt;
        }

        /// <summary>
        ///获取订单一级查询
        /// </summary>
        /// <returns></returns>
        public DataTable GetWorkOrderOneList(string ProjectId)
        {
            string sql = @"select OrderCode,Examinestatus,OrderStart,ProcessingState,TypeCode,TypeName,SiteCode,cp.CompanyFullName as SiteName,DistributionTime from TbWorkOrder
left join TbCompany cp on TbWorkOrder.SiteCode=cp.CompanyCode
 where (isnull(@ProjectId,'')='' or ProjectId=@ProjectId) and (OrderStart='加急订单' or OrderStart='问题订单') and (ProcessingState='Received' or ProcessingState='Processing' or ProcessingState='Finishing')";
            DataTable dt = Db.Context.FromSql(sql).AddInParameter("@ProjectId", DbType.String, ProjectId).ToDataTable();
            return dt;
        }

        /// <summary>
        /// 获取二级订单
        /// </summary>
        /// <param name="OrderCode"></param>
        /// <returns></returns>
        public DataTable GetWorkOrderTwoList(string OrderCode)
        {
            string sql = @" select ComponentName,MaterialCode,MaterialName,SpecificationModel,MeasurementUnit,MeasurementUnitZl,ItemUseNum,Number,WeightSmallPlan,DaetailWorkStrat,RevokeStart,PackNumber,Manufactor,HeatNo,TestReportNo from TbWorkOrderDetail where OrderCode=@OrderCode";
            DataTable dt = Db.Context.FromSql(sql).AddInParameter("@OrderCode", DbType.String, OrderCode).ToDataTable();
            return dt;
        }

        #endregion

        #region 填写打包数量

        /// <summary>
        /// 获取编辑数据
        /// </summary>
        /// <param name="dataID">数据Id</param>
        /// <returns></returns>
        public Tuple<DataTable> FindEntity1(int dataID)
        {
            //获取加工订单主表信息
            var model = Repository<TbWorkOrder>.First(d => d.ID == dataID);
            //查找明细信息
            var sql = @"select distinct wod.ID,wod.OrderCode,wod.ComponentName,wod.LargePattern,wod.MaterialCode,wod.MaterialName,wod.SpecificationModel,wod.MeasurementUnit,wod.MeasurementUnitZl,wod.ItemUseNum,wod.Number,wod.WeightSmallPlan,wod.DaetailWorkStrat,case when wod.PackNumber=0 and isnull(rmd.WorkOrderItemId,0)!=0 then wod.Number else wod.PackNumber end PackNumber,wod.Manufactor,wod.HeatNo,wod.TestReportNo,wod.QRCode,wod.RevokeStart,wod.SkillRequirement,wod.ProcessingTechnology,wod.Remark,case when wod.PackNumber=0 and isnull(rmd.WorkOrderItemId,0)!=0 then 1 else wod.PackageNumber end PackageNumber,wod.FinishTime,wod.ID as ItemID, sd.DictionaryText as MeasurementUnitText,pt.ProcessingTechnologyName,isnull(rmd.WorkOrderItemId,0) as WorkOrderItemId  from TbWorkOrderDetail wod 
left join TbSysDictionaryData sd on wod.MeasurementUnit=sd.DictionaryCode
left join TbProcessingTechnology pt on wod.ProcessingTechnology=pt.ID
left join (
select r.Examinestatus,rd.* from TbRMProductionMaterial r 
left join TbRMProductionMaterialDetail rd on r.CollarCode=rd.CollarCode and r.Examinestatus='审核完成') rmd on wod.ID=rmd.WorkOrderItemId
where wod.OrderCode=@OrderCode and wod.ID not in(select poi.WorkOrderDetailId from TbProblemOrder  po 
left join  TbProblemOrderItem poi on po.ProblemOrderCode=poi.ProblemOrderCode where po.OrderCode=@OrderCode)";
            DataTable items = Db.Context.FromSql(sql)
                             .AddInParameter("@OrderCode", DbType.String, model.OrderCode).ToDataTable();
            return new Tuple<DataTable>(items);
        }

        /// <summary>
        /// 填写打包数量
        /// </summary>
        public AjaxResult SavePackNum(int ID, List<TbWorkOrderDetail> items)
        {
            if (items == null)
                return AjaxResult.Warning("参数错误");
            //筛选已填写打包数量的数据
            var orderIds = items.Where(p => p.PackNumber > 0).Select(p => p.ID).ToList();
            if (!orderIds.Any())
                return AjaxResult.Success();
            //获取加工订单主表信息
            var model = Repository<TbWorkOrder>.First(d => d.ID == ID);
            model.ProcessingState = "Processing";
            //获取加工订单进度主表信息
            var model1 = Repository<TbOrderProgress>.First(d => d.OrderCode == model.OrderCode);
            model1.ProcessingState = "Processing";
            //获取配送计划主表信息
            var model2 = Repository<TbDistributionPlanInfo>.First(d => d.OrderCode == model.OrderCode);
            model2.ProcessingState = "Processing";
            //获取该加工订单下所有的明细
            var workOrderList = Repository<TbWorkOrderDetail>.Query(p => p.OrderCode == model.OrderCode && p.ID.In(orderIds)).ToList();
            workOrderList.ForEach(x =>
            {
                var itemO = items.FirstOrDefault(p => p.ID == x.ID);
                x.PackNumber = itemO.PackNumber;
                x.PackageNumber = itemO.PackageNumber.Value;
                x.DaetailWorkStrat = "加工中";
            });
            //获取该加工订单进度下所有的明细
            var orderProgressList = Repository<TbOrderProgressDetail>.Query(p => p.OrderCode == model.OrderCode && p.WorkorderdetailId.In(orderIds)).ToList();
            orderProgressList.ForEach(x =>
            {
                var itemPr = items.FirstOrDefault(p => p.ID == x.WorkorderdetailId);
                x.PackNumber = itemPr.PackNumber;
                x.DaetailWorkStrat = "加工中";
            });
            //获取该配送计划下所有的明细
            var planList = Repository<TbDistributionPlanDetailInfo>.Query(p => p.OrderCode == model.OrderCode && p.WorkorderdetailId.In(orderIds)).ToList();
            planList.ForEach(x =>
            {
                var itemP = items.FirstOrDefault(p => p.ID == x.WorkorderdetailId);
                x.PackNumber = itemP.PackNumber;
                x.DaetailWorkStrat = "加工中";
            });

            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())//使用事务
                {
                    //修改加工订单主表加工状态
                    Repository<TbWorkOrder>.Update(trans, model);
                    //修改加工订单明细中的加工状态
                    Repository<TbWorkOrderDetail>.Update(trans, workOrderList);
                    //修改加工订单进度主表加工状态
                    Repository<TbOrderProgress>.Update(trans, model1, p => p.OrderCode == model.OrderCode);
                    //修改加工订单进度明细表加工状态
                    Repository<TbOrderProgressDetail>.Update(trans, orderProgressList);
                    //修改配送计划主表加工状态
                    Repository<TbDistributionPlanInfo>.Update(trans, model2, p => p.OrderCode == model.OrderCode);
                    //修改配送计划明细表加工状态
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

        #region App订单重量查询
        public DataTable GetWorkOrderWeight(WorkOrderRequest request)
        {
            string sql = "";
            #region 模糊搜索条件

            var where = "";

            if (!string.IsNullOrWhiteSpace(request.ProcessingState))
            {
                where += (" and wo.ProcessingState ='" + request.ProcessingState + "'");
            }
            if (!string.IsNullOrWhiteSpace(request.UrgentDegree))
            {
                where += (" and wo.UrgentDegree='" + request.UrgentDegree + "'");
            }

            #endregion

            #region 数据权限新

            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where += (" and wo.ProcessFactoryCode='" + request.ProcessFactoryCode + "'");
            }
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where += (" and wo.ProjectId='" + request.ProjectId + "'");
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                StringBuilder sb = new StringBuilder();
                List<string> SiteList = GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);
                for (int i = 0; i < SiteList.Count; i++)
                {
                    if (i == (SiteList.Count - 1))
                    {
                        sb.Append("'" + SiteList[i] + "'");
                    }
                    else
                    {
                        sb.Append("'" + SiteList[i] + "',");
                    }
                }
                if (SiteList.Count > 0)
                {
                    where += " and wo.SiteCode in(" + sb + ")";
                }
            }

            #endregion

            try
            {
                sql = @"--订单总量(订单状态除未接收外的订单)
                          select  isnull(SUM(wo.WeightTotal),0) as SumWeightTotal from TbWorkOrder wo where wo.ProcessingState!='ConfirmWork' " + where + @"
                          union all
                          select isnull(sum(wod.WeightSmallPlan),0) as SumWeightTotal from TbWorkOrder wo
                          left join TbWorkOrderDetail wod on wo.OrderCode=wod.OrderCode
                          where wod.DaetailWorkStrat='加工完成' " + where + @"";

                var dt = Db.Context.FromSql(sql).ToDataTable();
                DataColumn BFB = new DataColumn("BFB", typeof(decimal));   //数据类型为 整形
                dt.Columns.Add(BFB);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        decimal dtValue = Convert.ToDecimal(((Convert.ToDecimal(dt.Rows[i]["SumWeightTotal"]) / Convert.ToDecimal(dt.Rows[0]["SumWeightTotal"])) * 100).ToString("f2"));
                        dt.Rows[i]["BFB"] = dtValue;
                    }
                }
                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region App订单进度查询

        public DataTable GetOrderProgress(string OrderCode)
        {
            string sql = "";
            try
            {
                sql = @"select WeightTotal,case when WeightTotal=0 then 0 else cast(WeightTotal/WeightTotal*100 as  decimal(18,2)) end as SumBFB,AccumulativeQuantity,case when WeightTotal=0 then 0 else cast(AccumulativeQuantity/WeightTotal*100 as  decimal(18,2)) end KlBFB,ISNULL(WeightTotal,0)-ISNULL(AccumulativeQuantity,0) as Wwc,case when WeightTotal=0 then 0 else  cast(((ISNULL(WeightTotal,0)-ISNULL(AccumulativeQuantity,0))/WeightTotal*100) as  decimal(18,2)) end WwcBFB from TbOrderProgress 
                        where OrderCode=@OrderCode";

                var dt = Db.Context.FromSql(sql)
                           .AddInParameter("@OrderCode", DbType.String, OrderCode).ToDataTable();
                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region  订单进度图
        public DataTable GetOrderProgess(string OrderCode)
        {
            string sql = @"select TbType.name,CAST(TbType.progressGd as decimal(18,2)) as progressGd,CAST(Tbprogress.progressnew as decimal(18,2)) as progressnew,CAST(Tbprogress.progress as decimal(18,2)) as progress,CONVERT(varchar(100),Tbprogress.progressDate, 120) as progressDate from (
                           select '订单接收' as name,0 as progressGd
                           union all
                           select '领料完成' as name,25 as progressGd
                           union all
                           select '加工完成' as name,50 as progressGd
                           union all
                           select '配送完成' as name,75 as progressGd
                           union all
                           select '签收完成' as name,100 as progressGd) TbType
                           left join (
                           --订单接收时间
                           select '订单接收' as name,(case when CONVERT(varchar(100),fpo.PerformDate, 120)!='' then 100 else 0.00 end)/100*0 as progressnew,case when CONVERT(varchar(100),fpo.PerformDate, 120)!='' then 100 else 0.00 end as progress,CONVERT(varchar(100),fpo.PerformDate, 120) as progressDate  from TbFlowPerformOpinions fpo where fpo.FlowPerformID in(select fpf.FlowPerformID from TbFlowPerform fpf  where fpf.FormCode='WorkOrder' and FormDataCode=(select ID from TbWorkOrder where OrderCode=@OrderCode))
                           and fpo.FlowNodeCode=2 and fpo.PerformState=1
                           union all
                           --订单领料百分比\领料完成时间
                           select '领料完成' as name,(case when Tb.LlTotal>=Tb.OrderTotal then 100/100*25*1.0/2+12.5 else Tb.LlTotal/Tb.OrderTotal*100/100*25*1.0/2+12.5 end) as progressnew,case when Tb.LlTotal>=Tb.OrderTotal then 100 else Tb.LlTotal/Tb.OrderTotal*100 end as progress,Tb.OrderlyDate from (select SUM(TbLld.WeightSmallPlan) as OrderTotal,SUM(TbLld.WeightSmallPlanN) as LlTotal,TbLl.OrderlyDate from TbWorkOrder wo 
                           left join (select TbLl.OrderCode,CONVERT(varchar(100),InsertTime, 120) as  OrderlyDate from TbRMProductionMaterial TbLl where TbLl.CollarState='领料完成') TbLl on wo.OrderCode=TbLl.OrderCode
                           left join TbRMProductionMaterialDetail TbLld on wo.OrderCode=TbLld.OrderCode 
                           where wo.OrderCode=@OrderCode and wo.ProcessingState!='Received'
                           group by TbLl.OrderlyDate) Tb
                           union all
                           --订单加工百分比\加工完成时间
                           select '加工完成' as name,(case when op.AccumulativeQuantity=SUM(wod.WeightSmallPlan) then 100/100*50*1.0/2+25 else op.AccumulativeQuantity/SUM(wod.WeightSmallPlan)*100/100*50*1.0/2+25 end) as progressnew,case when op.AccumulativeQuantity=SUM(wod.WeightSmallPlan) then 100 else (op.AccumulativeQuantity/SUM(wod.WeightSmallPlan)*100) end as progress,op.FinishProcessingDateTime from TbWorkOrder wo
                           left join TbWorkOrderDetail wod on wo.OrderCode=wod.OrderCode
                           left join TbOrderProgress op on wo.OrderCode=op.OrderCode
                           where wo.OrderCode=@OrderCode and (wo.ProcessingState='Processing' or wo.ProcessingState='Finishing') and  wod.RevokeStart='正常'
                           group by op.AccumulativeQuantity,op.FinishProcessingDateTime
                           union all
                           --配送完成百分比、配送完成时间
                           select '配送完成' as name,(case when SUM(wod.Number)=SUM(disPlanItem.PSAmount) then 0 when SUM(disPlanItem.PSAmount)=0 then 100/100*75*1.0/2+37.5 else SUM(disPlanItem.PSAmount)/SUM(wod.Number)*100/100*75*1.0/2+37.5 end)  as progressnew,case when SUM(wod.Number)=SUM(disPlanItem.PSAmount) then 0 when SUM(disPlanItem.PSAmount)=0 then 100 else SUM(disPlanItem.PSAmount)/SUM(wod.Number)*100 end as progress,disPlan.DeliveryCompleteTime from TbWorkOrder wo
                           left join TbWorkOrderDetail wod on wo.OrderCode=wod.OrderCode
                           left join TbDistributionPlanInfo disPlan on wo.OrderCode=disPlan.OrderCode
                           left join TbDistributionPlanDetailInfo disPlanItem on disPlan.OrderCode=disPlanItem.OrderCode  
                           where wo.OrderCode=@OrderCode and wo.ProcessingState='Finishing' and wod.RevokeStart='正常'
                           group by disPlan.DeliveryCompleteTime
                           union all
                           --签收完成百分比、配送完成时间
                           select '签收完成' as name,
                           case when Tb.Number=0 then null 
                           else Tb.QsNum*1.0/Tb.Number*100/100*100*1.0/2+50 end progressNew,
                           case when Tb.Number=0 then null 
                           else Tb.QsNum*1.0/Tb.Number*100 end as progress,
                           Tb.SignStateTime 
                           from 
                           (
                           SELECT
							(
								select SUM(wod.Number) as Number from TbWorkOrderDetail wod 
								where wod.OrderCode=@OrderCode and RevokeStart='正常'
							) Number , 
							SUM(qsdata.QsNum) AS QsNum,
							MAX(qsdata.SignStateTime) AS SignStateTime FROM 
							(
							 select 
                           	isnull(sum(disEntOrderItem.Number),0) as QsNum,
                           	MAX(disEntOrder.SignStateTime) as SignStateTime 
                           	from TbDistributionEntOrder disEntOrder
                           left join TbDistributionEntItem disEntOrderItem on disEntOrder.DisEntOrderIdentity=disEntOrderItem.DisEntOrderIdentity and disEntOrder.DistributionCode=disEntOrderItem.DistributionCode
                           where disEntOrder.OrderCode like '%'+@OrderCode+'%' 
                           and disEntOrderItem.OrderCode=@OrderCode 
                           and disEntOrder.SignState='已签收'
                           UNION ALL			
                           SELECT SUM(Number) AS QsNum,tsfs.SigninTime AS SignStateTime
                           FROM TbSemiFinishedSign tsfs
                           LEFT JOIN TbWorkOrderDeliveryOverItem twodoi ON twodoi.OrderCode=tsfs.OrderCodeH
                           WHERE tsfs.OrderCodeH=@OrderCode AND tsfs.OperateState='已签收'
                           GROUP BY tsfs.SigninTime
								) qsdata
                           ) Tb
                           ) Tbprogress on TbType.name=Tbprogress.name";

            var dt = Db.Context.FromSql(sql).AddInParameter("@OrderCode", DbType.String, OrderCode).ToDataTable();
            return dt;
        }
        #endregion

        #region 产能填报

        #region 加工产能信息获取
        /// <summary>
        /// 加工产能信息获取
        /// </summary>
        /// <param name="keyValue">加工厂编号</param>
        /// <param name="month">产能月份</param>
        /// <returns></returns>
        public Tuple<DataTable> GetWorkCapacityFormJson(string keyValue, string month)
        {
            string sql = @"select cf.*,Tb1.WeightSmallPlan as ActualOutputValueNew,cast(Tb1.WeightSmallPlan/cf.Capacity*100 as decimal(18,2)) as ActualLoadNew from TbCapacityFilling cf
                            left join(
                            select SUM(Tb.WeightSmallPlan) as WeightSmallPlan from (
                            select isnull(SUM(wod.WeightSmallPlan),0) as WeightSmallPlan from TbWorkOrder wo
                            left join TbWorkOrderDetail wod on wo.OrderCode=wod.OrderCode
                            where (ISNULL(@ProcessFactoryCode,'')='' or wo.ProcessFactoryCode=@ProcessFactoryCode)
                            and CONVERT(varchar(7), wo.DistributionTime, 120)=@month
                            and wod.RevokeStart='正常' and wo.ProcessingState!='ConfirmWork' and wo.ProcessingState!='Finishing'
                            union all
                            select isnull(sum(opd.NoCompleted),0) as NoCompleted from TbOrderProgress op
                            left join TbOrderProgressDetail opd on op.OrderCode=opd.OrderCode 
                            where (ISNULL(@ProcessFactoryCode,'')='' or op.ProcessFactoryCode=@ProcessFactoryCode)
                            and CONVERT(varchar(7), op.DistributionTime, 120)=CONVERT(varchar(4),YEAR(DATEADD(MONTH,-1,@month+'-01')))+'-'+Right(100+Month(DATEADD(MONTH,-1,@month+'-01')),2)
                            and opd.RevokeStart='正常')Tb) Tb1 on cf.CapacityMonth=@month 
                            where cf.CapacityMonth=@month
                            and (ISNULL(@ProcessFactoryCode,'')='' or cf.ProcessFactoryCode=@ProcessFactoryCode)";
            var ret = Db.Context.FromSql(sql)
                .AddInParameter("@ProcessFactoryCode", DbType.String, keyValue)
                .AddInParameter("@month", DbType.String, month).ToDataTable();
            return new Tuple<DataTable>(ret);
        }
        /// <summary>
        /// 获取产能填报的默认值
        /// </summary>
        /// <param name="keyValue">加工厂编号</param>
        /// <returns></returns>
        public DataTable GetWorkCapacityIsDefault(string keyValue)
        {
            string sql = @"select Capacity from TbCapacityFilling where ProcessFactoryCode=@ProcessFactoryCode and IsDefault=1";
            var ret = Db.Context.FromSql(sql)
                .AddInParameter("@ProcessFactoryCode", DbType.String, keyValue).ToDataTable();
            return ret;
        }

        #endregion

        #region 保存产能信息

        /// <summary>
        /// 新增产能信息
        /// </summary>
        public AjaxResult InsertCapacity(TbCapacityFilling model)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            model.InsertUserCode = OperatorProvider.Provider.CurrentUser.UserCode;
            model.InsertTiem = DateTime.Now;
            TbCapacityFilling cfModel = null;
            if (model.IsDefault == 1)
            {
                cfModel = Repository<TbCapacityFilling>.First(p => p.ProcessFactoryCode == model.ProcessFactoryCode && p.IsDefault == 1);
                if (cfModel != null)
                {
                    cfModel.IsDefault = 0;
                }
            }
            using (DbTrans trans = Db.Context.BeginTransaction())
            {
                try
                {
                    //将之前设置为默认的数据修改为非默认
                    if (cfModel != null)
                    {
                        Repository<TbCapacityFilling>.Update(trans, cfModel);
                    }
                    //添加信息及明细信息
                    Repository<TbCapacityFilling>.Insert(trans, model);
                    trans.Commit();
                    return AjaxResult.Success();
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
            }
        }

        /// <summary>
        /// 修改产能信息
        /// </summary>
        public AjaxResult UpdateCapacity(TbCapacityFilling model)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            TbCapacityFilling cfModel = null;
            if (model.IsDefault == 1)
            {
                cfModel = Repository<TbCapacityFilling>.First(p => p.ProcessFactoryCode == model.ProcessFactoryCode && p.IsDefault == 1 && p.ID != model.ID);
                if (cfModel != null)
                {
                    cfModel.IsDefault = 0;
                }
            }
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())//使用事务
                {
                    //将之前设置为默认的数据修改为非默认
                    if (cfModel != null)
                    {
                        Repository<TbCapacityFilling>.Update(trans, cfModel);
                    }
                    //修改信息
                    Repository<TbCapacityFilling>.Update(trans, model, p => p.ID == model.ID);
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

        #region 获取当月加工完成量
        /// <summary>
        /// 加工产能信息获取
        /// </summary>
        /// <param name="keyValue">加工厂编号</param>
        /// <param name="month">产能月份</param>
        /// <returns></returns>
        public Tuple<DataTable> GetWorkJgwcl(string keyValue, string month)
        {
            string sql = @"select SUM(Tb.WeightSmallPlan) as Jgwcl from (
                           select isnull(SUM(wod.WeightSmallPlan),0) as WeightSmallPlan from TbWorkOrder wo
                           left join TbWorkOrderDetail wod on wo.OrderCode=wod.OrderCode
                           where (ISNULL(@ProcessFactoryCode,'')='' or wo.ProcessFactoryCode=@ProcessFactoryCode) and CONVERT(varchar(7), wo.DistributionTime, 120)=@month
                           and wod.RevokeStart='正常' and wo.ProcessingState!='ConfirmWork'
                           union all
                           select isnull(sum(opd.NoCompleted),0) as NoCompleted from TbOrderProgress op
                           left join TbOrderProgressDetail opd on op.OrderCode=opd.OrderCode 
                           where (ISNULL(@ProcessFactoryCode,'')='' or op.ProcessFactoryCode=@ProcessFactoryCode) and CONVERT(varchar(7), op.DistributionTime, 120)=CONVERT(varchar(5),YEAR(DATEADD(MONTH,-1,@month+'-01')))+'-'+Right(100+Month(DATEADD(MONTH,-1,@month+'-01')),2)
                           and opd.RevokeStart='正常') Tb";
            DataTable dt = Db.Context.FromSql(sql)
                .AddInParameter("@ProcessFactoryCode", DbType.String, keyValue)
                .AddInParameter("@month", DbType.String, month).ToDataTable();
            return new Tuple<DataTable>(dt);
        }

        #endregion

        #endregion

        #region 订单的节点图形
        /// <summary>
        /// 获取领料信息
        /// </summary>
        /// <param name="OrderCode"></param>
        /// <returns></returns>
        public DataTable GetLlInfo(string OrderCode)
        {
            string sql = @"select top 1 ID from TbRMProductionMaterial where OrderCode=@OrderCode order by ID desc";
            var dt = Db.Context.FromSql(sql).AddInParameter("@OrderCode", DbType.String, OrderCode).ToDataTable();
            return dt;

        }
        /// <summary>
        /// 获取加工进度
        /// </summary>
        /// <returns></returns>
        public DataTable GetJgJdInfo(string OrderCode)
        {
            string sql = @"select ID from TbOrderProgress where OrderCode=@OrderCode";
            var dt = Db.Context.FromSql(sql).AddInParameter("@OrderCode", DbType.String, OrderCode).ToDataTable();
            return dt;
        }
        /// <summary>
        /// 获取运输卸货统计
        /// </summary>
        /// <returns></returns>
        public DataTable GetYsXhTjInfo(string OrderCode)
        {
            string sql = @"select top 1 ID from TbTransportCarReport where OrderCode like '%'+@OrderCode+'%' order by ID desc";
            var dt = Db.Context.FromSql(sql).AddInParameter("@OrderCode", DbType.String, OrderCode).ToDataTable();
            return dt;
        }
        /// <summary>
        /// 获取签收信息
        /// </summary>
        /// <param name="OrderCode"></param>
        /// <returns></returns>
        public DataTable GetQsWcInfo(string OrderCode)
        {
            string sql = @"select top 1 sfs.ID from TbSemiFinishedSign sfs
left join TbSiteDischargeCargo sdc on sfs.DischargeCargoCode=sdc.DischargeCargoCode
where sdc.OrderCode  like '%'+@OrderCode+'%' and sfs.SigninTime is not null order by sfs.ID desc";
            var dt = Db.Context.FromSql(sql).AddInParameter("@OrderCode", DbType.String, OrderCode).ToDataTable();
            return dt;
        }
        #endregion

        #region 配送确认

        /// <summary>
        /// 配送确认
        /// </summary>
        public AjaxResult DeliveryConfirm(int keyValue, DateTime deliveryCompleteTime, string Enclosure, string Remark)
        {
            try
            {
                //判断是否可操作
                var anyRet = IsOffline(keyValue);
                if (anyRet.state.ToString() != ResultType.success.ToString())
                    return anyRet;
                var workOrder = (TbWorkOrder)anyRet.data;
                //查找配送计划单
                var plan = Repository<TbDistributionPlanInfo>.First(p => p.OrderCode == workOrder.OrderCode);
                if (plan == null)
                    return AjaxResult.Warning("订单信息不存在");
                workOrder.IsOffline = 1;
                plan.DistributionStart = "配送完成";
                plan.DeliveryCompleteTime = deliveryCompleteTime;
                if (!string.IsNullOrWhiteSpace(Remark))
                    plan.Remark = Remark;
                //查找配送计划明细,更改未配送数量
                var planDetailList = Repository<TbDistributionPlanDetailInfo>.Query(p => p.OrderCode == workOrder.OrderCode && p.PSAmount > 0);
                if (planDetailList.Any())
                {
                    planDetailList.ForEach(x =>
                    {
                        x.PSAmount = 0;
                    });
                }
                //查找加工订单未在配送流程中的数据 
                //记录配送完成明细
                List<TbWorkOrderDeliveryOverItem> overItemList = new List<TbWorkOrderDeliveryOverItem>();
                var orderItemIdList = Repository<TbDistributionEntItem>.Query(p => p.OrderCode == workOrder.OrderCode).Select(p => p.WorkorderdetailId).ToList();
                var orderItemList = Repository<TbWorkOrderDetail>.Query(p => p.OrderCode == workOrder.OrderCode && p.RevokeStart == "正常" && p.ID.NotIn(orderItemIdList)).ToList();
                if (orderItemList.Any())
                {
                    orderItemList.ForEach(x =>
                    {
                        var data = new TbWorkOrderDeliveryOverItem()
                        {
                            OrderCode = x.OrderCode,
                            OrderDetailId = x.ID,
                            ProcessingTechnology = x.ProcessingTechnology,
                            WeightSmallPlan = x.WeightSmallPlan,
                            Number = x.Number
                        };
                        overItemList.Add(data);
                    });
                }
                else
                {
                    return AjaxResult.Warning("没有构件信息可以配送");
                }
                //创建签收单信息
                var signInfo = MapperHelper.Map<TbDistributionPlanInfo, TbSemiFinishedSign>(plan);
                var number = CreateCode.GetTableMaxCode("BCPQS", "SigninNuber", "TbSemiFinishedSign");
                string fc = "BCPQS" + DateTime.Now.Year;
                var ec = number.Replace(fc, "").Substring(1);
                signInfo.SigninNuber = fc + ec;
                signInfo.OrderCodeH = plan.OrderCode;
                signInfo.DistributionAddress = plan.DistributionAdd;
                signInfo.PlanDistributionTime = plan.DistributionTime.Value.ToString("yyyy-MM-dd");
                signInfo.SumTotal = plan.WeightTotal;
                signInfo.InsertTime = DateTime.Now;
                signInfo.DistributionTime = null;
                signInfo.OperateState = "未签收";
                signInfo.Enclosure = Enclosure;
                signInfo.Remark = "";
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //修改加工订单
                    Repository<TbWorkOrder>.Update(trans, workOrder, p => p.ID == workOrder.ID);
                    //修改配送计划
                    Repository<TbDistributionPlanInfo>.Update(trans, plan, p => p.ID == plan.ID);
                    //配送计划明细
                    if (planDetailList.Any())
                        Repository<TbDistributionPlanDetailInfo>.Update(trans, planDetailList);
                    //添加签收单信息
                    Repository<TbSemiFinishedSign>.Insert(trans, signInfo);
                    //记录配送完成明细
                    if (overItemList.Any())
                        Repository<TbWorkOrderDeliveryOverItem>.Insert(trans, overItemList);
                    trans.Commit();
                    return AjaxResult.Success();
                }
            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.ToString());
            }
        }

        #endregion

        #region 统计报表

        public DataTable Img2(WorkOrderRequest request)
        {
            try
            {
                string where = "";

                #region 查询条件

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
                    List<string> SiteList = GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);
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
                    if (request.MonthType == "加工月份")
                    {
                        where += " and (CONVERT(varchar(7),a.DistributionTime, 120)='" + InsertTimeMonth + "' or (CONVERT(varchar(7),a.DistributionTime, 120)<'" + InsertTimeMonth + "' and a.ProcessingState!='Finishing')) and a.Examinestatus='审核完成'";
                    }
                    else if (request.MonthType == "录入月份")
                    {
                        where += " and CONVERT(varchar(7),a.InsertTime, 120)='" + InsertTimeMonth + "' and a.Examinestatus='审核完成'";
                    }
                }

                #endregion

                string sql = @"select isnull(SUM(Tb.AlreadyCompleted),0) as AlreadyCompleted,isnull(SUM(Tb.NoCompleted),0) as NoCompleted from (
                              select case when c.ID is null then b.WeightSmallPlan else c.NoCompleted end as NoCompleted,c.AlreadyCompleted from TbWorkOrder a 
                              left join TbWorkOrderDetail b on a.OrderCode=b.OrderCode
                              left join TbOrderProgressDetail c on c.WorkorderdetailId=b.ID where 1=1 " + where + @") Tb ";
                DataTable dt = Db.Context.FromSql(sql).ToDataTable();
                return dt;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public DataTable Img3(WorkOrderRequest request)
        {
            try
            {
                string where = "";

                #region 查询条件

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
                    List<string> SiteList = GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);
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
                    if (request.MonthType == "加工月份")
                    {
                        where += " and (CONVERT(varchar(7),a.DistributionTime, 120)='" + InsertTimeMonth + "' or (CONVERT(varchar(7),a.DistributionTime, 120)<'" + InsertTimeMonth + "' and a.ProcessingState!='Finishing')) and a.Examinestatus='审核完成'";
                    }
                    else if (request.MonthType == "录入月份")
                    {
                        where += " and CONVERT(varchar(7),a.InsertTime, 120)='" + InsertTimeMonth + "' and a.Examinestatus='审核完成'";
                    }
                }

                #endregion

                //                string sql = @"select TbProgressType.ProgressType,ISNULL(TbProgress.WeightTotal, 0) as WeightTotal,isnull(TbProgress.AlreadyCompleted,0) as AlreadyCompleted ,isnull(TbProgress.NoCompleted,0) as NoCompleted from (select '进度正常' as ProgressType
                //union all
                //select '进度超前' as ProgressType
                //union all
                //select '进度滞后' as ProgressType) TbProgressType
                //left join (select sum(a.WeightTotal) as WeightTotal,'进度超前' as  ProgressType,0 as AlreadyCompleted,0 as NoCompleted from TbWorkOrder a
                //left join TbOrderProgress b on a.OrderCode=b.OrderCode
                //where 1=1 " + where + @"  and (b.FinishProcessingDateTime is not null and CONVERT(varchar(100),b.FinishProcessingDateTime,23)<CONVERT(varchar(100),a.DistributionTime,23)) 
                //union all
                //select sum(a.WeightTotal) as WeightTotal,'进度正常' as  ProgressType,0 as AlreadyCompleted,0 as NoCompleted from TbWorkOrder a
                //left join TbOrderProgress b on a.OrderCode=b.OrderCode
                //where 1=1 " + where + @" and (b.FinishProcessingDateTime is not null and (CONVERT(varchar(100),b.FinishProcessingDateTime,23)=CONVERT(varchar(100),a.DistributionTime,23)) 
                //or (b.FinishProcessingDateTime is null and CONVERT(varchar(100),a.DistributionTime,23)>=CONVERT(varchar(100),GETDATE(),23)))
                //union all
                //select sum(Tb.AlreadyCompleted)+sum(Tb.NoCompleted) as WeightTotal,'进度滞后' as  ProgressType,sum(Tb.AlreadyCompleted) as AlreadyCompleted,sum(Tb.NoCompleted) as NoCompleted from (select a.WeightTotal,b.AccumulativeQuantity as AlreadyCompleted ,case when b.ID is null then a.WeightTotal else a.WeightTotal-b.AccumulativeQuantity end as NoCompleted from TbWorkOrder a
                //left join TbOrderProgress b on a.OrderCode=b.OrderCode
                //where 1=1 " + where + @" and (b.FinishProcessingDateTime is not null and (CONVERT(varchar(100),b.FinishProcessingDateTime,23)>CONVERT(varchar(100),a.DistributionTime,23)) 
                //or (b.FinishProcessingDateTime is null and CONVERT(varchar(100),a.DistributionTime,23)<CONVERT(varchar(100),GETDATE(),23)))) Tb) TbProgress on TbProgressType.ProgressType=TbProgress.ProgressType ";
                string sql = @"select TbProgressType.ProgressType,isnull(TbProgress.WeightTotal,0) as WeightTotal from (select '进度正常' as ProgressType
union all
select '进度超前' as ProgressType
union all
select '已完成（滞后）' as ProgressType
union all
select '未完成（滞后）' as ProgressType
) TbProgressType
left join(
select sum(a.WeightTotal) as WeightTotal,'进度正常' as  ProgressType from  TbWorkOrder a
left join TbOrderProgress b on a.OrderCode=b.OrderCode 
where 1=1 " + where + @" and ((CONVERT(varchar(100),b.FinishProcessingDateTime,23)=CONVERT(varchar(100),a.DistributionTime,23) or (b.FinishProcessingDateTime is null and a.Examinestatus='审核完成' and CONVERT(varchar(100),a.DistributionTime,23)>=CONVERT(varchar(100),GETDATE(),23))))
union all
select sum(a.WeightTotal) as WeightTotal,'进度超前' as  ProgressType from  TbWorkOrder a
left join TbOrderProgress b on a.OrderCode=b.OrderCode 
where 1=1 " + where + @" and CONVERT(varchar(100),b.FinishProcessingDateTime,23)<CONVERT(varchar(100),a.DistributionTime,23) and a.Examinestatus='审核完成'
union all
select sum(a.WeightTotal) as WeightTotal,'已完成（滞后）' as  ProgressType from  TbWorkOrder a
left join TbOrderProgress b on a.OrderCode=b.OrderCode 
where 1=1 " + where + @" and CONVERT(varchar(100),b.FinishProcessingDateTime,23)>CONVERT(varchar(100),a.DistributionTime,23) and a.Examinestatus='审核完成'
union all
select sum(a.WeightTotal) as WeightTotal,'未完成（滞后）' as  ProgressType from  TbWorkOrder a
left join TbOrderProgress b on a.OrderCode=b.OrderCode 
where 1=1 " + where + @" and CONVERT(varchar(100),a.DistributionTime,23)<CONVERT(varchar(100),GETDATE(),23) and b.FinishProcessingDateTime is null and a.Examinestatus='审核完成') TbProgress on TbProgressType.ProgressType=TbProgress.ProgressType";
                DataTable dt = Db.Context.FromSql(sql).ToDataTable();
                return dt;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public DataTable Img4(WorkOrderRequest request)
        {
            try
            {
                string where = "";
                string where1 = "";

                #region 查询条件

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
                    List<string> SiteList = GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);
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
                where1 += where;
                if (request.HistoryMonth.HasValue)
                {
                    string InsertTimeMonth = Convert.ToDateTime(request.HistoryMonth).ToString("yyyy-MM");
                    if (request.MonthType == "加工月份")
                    {
                        where += " and a.Examinestatus='审核完成' and (CONVERT(varchar(7),a.DistributionTime, 120)='" + InsertTimeMonth + "' or (CONVERT(varchar(7),a.DistributionTime, 120)<'" + InsertTimeMonth + "' and a.ProcessingState!='Finishing' and a.Examinestatus='审核完成'))";
                        where1 += "and (CONVERT(varchar(7),a.DistributionTime, 120)='" + InsertTimeMonth + "') ";
                    }
                    else if (request.MonthType == "录入月份")
                    {
                        where += " and a.Examinestatus='审核完成' and CONVERT(varchar(7),a.InsertTime, 120)='" + InsertTimeMonth + "'";
                        where1 += "and CONVERT(varchar(7),a.InsertTime, 120)='" + InsertTimeMonth + "'";
                    }
                }

                #endregion

                string sql = @"select TbOrderStart.*,isnull(TbOrderCount.WeightTotal,0) as WeightTotal,isnull(TbOrderCount.OrderCount,0) as OrderCount from(select '正常订单' as OrderStart
                               union all
                               select '加急订单' as OrderStart
                               union all
                               select '变更订单' as OrderStart
                               union all
                               select '退回订单' as OrderStart) TbOrderStart
                               left join(select count(1) as OrderCount,isnull(sum(a.WeightTotal),0) as WeightTotal,'正常订单' as OrderStart from TbWorkOrder a where 1=1 " + where + @" and a.UrgentDegree='Commonly' 
                               union all
                               select count(1) as OrderCount,isnull(sum(a.WeightTotal),0) as WeightTotal,'加急订单' as OrderStart from TbWorkOrder a where 1=1 " + where + @" and a.UrgentDegree='Urgent'
                               union all
                               select count(1) as OrderCount,isnull(sum(a.WeightTotal),0) as WeightTotal,'变更订单' as OrderStart from TbWorkOrder a where 1=1 " + where1 + @" and (a.OrderStart='部分变更' or a.OrderStart='全部变更')
                               union all
                               select count(1) as OrderCount,isnull(sum(a.WeightTotal),0) as WeightTotal,'退回订单' as OrderStart from TbWorkOrder a where 1=1 " + where1 + @" and a.Examinestatus='已退回') TbOrderCount on TbOrderStart.OrderStart=TbOrderCount.OrderStart ";
                DataTable dt = Db.Context.FromSql(sql).ToDataTable();
                return dt;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion

        #region 加工订单组织机构

        public List<TbCompanyNew1> GetLoginUserAllCompanyNew(bool isNoSite = true, string formMenuCode = "", string HistoryMonth = "", string MonthType = "")
        {
            string CompanyCode = OperatorProvider.Provider.CurrentUser.CompanyId;
            string OrgType = OperatorProvider.Provider.CurrentUser.OrgType;
            string ProcessFactoryCode = OperatorProvider.Provider.CurrentUser.ProcessFactoryCode;
            var listAll = new List<TbCompanyNew1>();
            string noSiteWhere = "";
            if (!isNoSite)
            {
                noSiteWhere = " and T.OrgType!=5";
            }
            if (OrgType == "1")//当前登录人是加工厂人员（加载所以线的组织机构数据）
            {
                string sqlJgc = @"select T.id,T.CompanyCode,T.ParentCompanyCode,T.CompanyFullName,T.Address,T.OrgType,pc.ProjectId from  TbCompany T
                           left join TbProjectCompany pc on T.CompanyCode=pc.CompanyCode where T.OrgType!=1" + noSiteWhere + " order by T.id asc";
                DataTable retjgc = Db.Context.FromSql(sqlJgc).ToDataTable();
                List<TbCompanyNew1> jgcList = ModelConvertHelper<TbCompanyNew1>.ToList(retjgc);
                listAll.AddRange(jgcList);
            }
            else// 当前登录人是经理部、分部、工区、站点人员（加载所属跟下级）
            {
                //获取当前登录人的所有上级除本身
                string sqlParentCompany = @"WITH T
                                        AS( 
                                            SELECT id,CompanyCode,ParentCompanyCode,CompanyFullName,Address,OrgType FROM TbCompany WHERE CompanyCode=@CompanyCode 
                                            UNION ALL 
                                            SELECT a.id,a.CompanyCode,a.ParentCompanyCode,a.CompanyFullName,a.Address,a.OrgType 
                                            FROM TbCompany a INNER JOIN T ON a.CompanyCode=T.ParentCompanyCode  
                                        ) 
                                        SELECT * FROM T 
                                        left join TbProjectCompany tpc on T.CompanyCode=tpc.CompanyCode where tpc.ProjectId=@ProjectId and T.CompanyCode!=@CompanyCode and T.OrgType!=1 " + noSiteWhere + " order by T.id asc;";
                DataTable retParent = Db.Context.FromSql(sqlParentCompany).AddInParameter("@CompanyCode", DbType.String, CompanyCode).AddInParameter("@ProjectId", DbType.String, ProjectId).ToDataTable();
                List<TbCompanyNew1> parentList = ModelConvertHelper<TbCompanyNew1>.ToList(retParent);
                listAll.AddRange(parentList);
                //获取当前登录人的所有下级包括本身
                string sqlSonCompany = @"WITH T
                                     AS( 
                                         SELECT id,CompanyCode,ParentCompanyCode,CompanyFullName,Address,OrgType FROM TbCompany WHERE CompanyCode=@CompanyCode 
                                         UNION ALL 
                                         SELECT a.id,a.CompanyCode,a.ParentCompanyCode,a.CompanyFullName,a.Address,a.OrgType  FROM TbCompany a INNER JOIN T ON a.ParentCompanyCode=T.CompanyCode  
                                     ) 
                                     SELECT T.*,tpc.ProjectId FROM T 
                                     left join TbProjectCompany tpc on T.CompanyCode=tpc.CompanyCode where tpc.ProjectId=@ProjectId and T.OrgType!=1" + noSiteWhere + " order by T.id asc;";
                DataTable retSon = Db.Context.FromSql(sqlSonCompany).AddInParameter("@CompanyCode", DbType.String, CompanyCode).AddInParameter("@ProjectId", DbType.String, ProjectId).ToDataTable();
                List<TbCompanyNew1> sonList = ModelConvertHelper<TbCompanyNew1>.ToList(retSon);
                listAll.AddRange(sonList);
            }
            if (!string.IsNullOrWhiteSpace(formMenuCode))
            {
                DataTable dtSite = GetAllSiteCode(HistoryMonth, MonthType, ProjectId, ProcessFactoryCode);
                if (dtSite.Rows.Count > 0)
                {
                    List<string> BarndCode = new List<string>();//获取所有分部
                    for (int i = 0; i < dtSite.Rows.Count; i++)
                    {
                        TbCompanyNew1 compModel = listAll.Find(a => a.CompanyCode == dtSite.Rows[i]["SiteCode"].ToString());
                        if (compModel != null)
                        {
                            //查找这个站点对应的订单信息
                            DataTable dt = GetSiteOrderInfo(HistoryMonth, MonthType, compModel.CompanyCode, ProcessFactoryCode);
                            if (dt.Rows.Count > 0)
                            {
                                int ddzs = 0;//订单总数
                                int jjdd = 0;//加急订单数
                                int zhdd = 0;//滞后订单数
                                for (int j = 0; j < dt.Rows.Count; j++)
                                {
                                    if (!BarndCode.Contains(dt.Rows[j]["BarndCode"].ToString()))
                                    {
                                        BarndCode.Add(dt.Rows[j]["BarndCode"].ToString());
                                    }
                                    ddzs++;
                                    if (dt.Rows[j]["OrderStart"].ToString() == "加急订单")
                                    {
                                        jjdd++;
                                    }
                                    if (dt.Rows[j]["DdType"].ToString() == "未完成滞后")
                                    {
                                        zhdd++;
                                    }
                                }
                                compModel.SumOrderCount = ddzs;
                                compModel.SumJjCount = jjdd;
                                compModel.SumZhCount = zhdd;
                            }
                        }

                    }
                    for (int b = 0; b < BarndCode.Count; b++)
                    {
                        List<TbCompanyNew1> compList1 = listAll.FindAll(a => a.ParentCompanyCode == BarndCode[b]);//获取分部下所有的工区
                        if (compList1.Count > 0)
                        {
                            int fbddCount = 0;
                            int fbjjddCount = 0;
                            int fbzhddCount = 0;
                            for (int w = 0; w < compList1.Count; w++)
                            {
                                int gqddCount = 0;
                                int gqjjddCount = 0;
                                int gqzhddCount = 0;
                                List<TbCompanyNew1> compList2 = listAll.FindAll(a => a.ParentCompanyCode == compList1[w].CompanyCode);//获取所有工区下的站点
                                for (int s = 0; s < compList2.Count; s++)
                                {
                                    if (compList2[s].SumOrderCount > 0)
                                    {
                                        gqddCount += compList2[s].SumOrderCount;
                                        if (compList2[s].SumJjCount > 0)
                                        {
                                            gqjjddCount += compList2[s].SumJjCount;
                                        }
                                        if (compList2[s].SumZhCount > 0)
                                        {
                                            gqzhddCount += compList2[s].SumZhCount;
                                        }
                                    }
                                }
                                TbCompanyNew1 gqModel = listAll.Find(a => a.CompanyCode == compList1[w].CompanyCode);
                                gqModel.SumOrderCount = gqddCount;
                                gqModel.SumJjCount = gqjjddCount;
                                gqModel.SumZhCount = gqzhddCount;
                                fbddCount += gqModel.SumOrderCount;
                                fbjjddCount += gqModel.SumJjCount;
                                fbzhddCount += gqModel.SumZhCount;
                            }
                            TbCompanyNew1 fbModel = listAll.Find(a => a.CompanyCode == BarndCode[b]);
                            fbModel.SumOrderCount = fbddCount;
                            fbModel.SumJjCount = fbjjddCount;
                            fbModel.SumZhCount = fbzhddCount;
                        }

                    }
                    //获取经理部的信息
                    List<TbCompany> JlbList = Repository<TbCompany>.Query(d => d.OrgType == 2);
                    if (JlbList.Count > 0)
                    {
                        for (int j = 0; j < JlbList.Count; j++)
                        {
                            int jlbdds = 0;
                            int jlbjjs = 0;
                            int jlbzhs = 0;
                            List<TbCompanyNew1> compList1 = listAll.FindAll(a => a.ParentCompanyCode == JlbList[j].CompanyCode);//获取经理部下所有的工分部
                            for (int i = 0; i < compList1.Count; i++)
                            {
                                jlbdds += compList1[i].SumOrderCount;
                                jlbjjs += compList1[i].SumJjCount;
                                jlbzhs += compList1[i].SumZhCount;
                            }
                            TbCompanyNew1 jlbModel = listAll.Find(a => a.CompanyCode == JlbList[j].CompanyCode);
                            if (jlbModel != null)
                            {
                                jlbModel.SumOrderCount = jlbdds;
                                jlbModel.SumJjCount = jlbjjs;
                                jlbModel.SumZhCount = jlbzhs;
                            }
                        }
                    }
                }
            }
            return listAll;
        }

        public DataTable GetAllSiteCode(string HistoryMonth, string MonthType, string ProjectId, string ProcessFactoryCode)
        {
            try
            {
                string sql = "";
                if (!string.IsNullOrWhiteSpace(HistoryMonth))
                {
                    if (MonthType == "加工月份")
                    {
                        sql = @"select a.SiteCode from TbWorkOrder a
where a.Examinestatus='审核完成' 
and (CONVERT(varchar(7), a.DistributionTime, 120)='" + HistoryMonth + "' or (CONVERT(varchar(7), a.DistributionTime, 120)<='" + HistoryMonth + @"' and a.ProcessingState!='Finishing')) 
and a.ProcessFactoryCode='" + ProcessFactoryCode + @"' 
and (ISNULL(@ProjectId,'')='' or a.ProjectId=@ProjectId)
group by a.SiteCode";
                    }
                    else
                    {
                        sql = @"select a.SiteCode from TbWorkOrder a
where a.Examinestatus='审核完成' and CONVERT(varchar(7), a.InsertTime, 120)='" + HistoryMonth + @"' 
and (ISNULL(@ProjectId,'')='' or a.ProjectId=@ProjectId)
and a.ProcessFactoryCode='" + ProcessFactoryCode + @"' 
group by a.SiteCode";
                    }
                }
                else
                {
                    sql = @"select a.SiteCode from TbWorkOrder a
where  (ISNULL(@ProjectId,'')='' or a.ProjectId=@ProjectId)
and a.ProcessFactoryCode='" + ProcessFactoryCode + @"' 
group by a.SiteCode";
                }
                DataTable dt = Db.Context.FromSql(sql)
                .AddInParameter("@ProjectId", DbType.String, ProjectId).ToDataTable();
                return dt;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public DataTable GetSiteOrderInfo(string HistoryMonth, string MonthType, string SiteCode, string ProcessFactoryCode)
        {
            try
            {
                string sql = "";
                if (!string.IsNullOrWhiteSpace(HistoryMonth))
                {
                    if (MonthType == "加工月份")
                    {
                        sql = @"select a.SiteCode,d.CompanyCode as WorkAreaCode,e.CompanyCode as BarndCode,a.OrderStart,CONVERT(varchar(100), a.DistributionTime, 23) as DistributionTime,CONVERT(varchar(100), b.FinishProcessingDateTime, 23) as FinishProcessingDateTime,case when CONVERT(varchar(100), b.FinishProcessingDateTime, 23) is not null and CONVERT(varchar(100), b.FinishProcessingDateTime, 23)>CONVERT(varchar(100), a.DistributionTime, 23) and a.Examinestatus='审核完成' then '已完成滞后' when CONVERT(varchar(100), b.FinishProcessingDateTime, 23) is null and CONVERT(varchar(100),GETDATE(),23)>CONVERT(varchar(100), a.DistributionTime, 23) and a.Examinestatus='审核完成' then '未完成滞后' end as DdType from TbWorkOrder a
left join TbOrderProgress b on a.OrderCode=b.OrderCode
left join TbCompany c on a.SiteCode=c.CompanyCode
left join TbCompany d on d.CompanyCode=c.ParentCompanyCode
left join TbCompany e on e.CompanyCode=d.ParentCompanyCode
where 1=1 and a.Examinestatus='审核完成' and (CONVERT(varchar(7), a.DistributionTime, 120)='" + HistoryMonth + "' or (CONVERT(varchar(7), a.DistributionTime, 120)<'" + HistoryMonth + "' and a.ProcessingState!='Finishing')) and a.SiteCode='" + SiteCode + "' and a.ProcessFactoryCode='" + ProcessFactoryCode + "'";
                    }
                    else
                    {
                        sql = @"select a.SiteCode,d.CompanyCode as WorkAreaCode,e.CompanyCode as BarndCode,a.OrderStart,CONVERT(varchar(100), a.DistributionTime, 23) as DistributionTime,CONVERT(varchar(100), b.FinishProcessingDateTime, 23) as FinishProcessingDateTime,case when CONVERT(varchar(100), b.FinishProcessingDateTime, 23) is not null and CONVERT(varchar(100), b.FinishProcessingDateTime, 23)>CONVERT(varchar(100), a.DistributionTime, 23) and a.Examinestatus='审核完成' then '已完成滞后' when CONVERT(varchar(100), b.FinishProcessingDateTime, 23) is null and CONVERT(varchar(100),GETDATE(),23)>CONVERT(varchar(100), a.DistributionTime, 23) and a.Examinestatus='审核完成' then '未完成滞后' end as DdType from TbWorkOrder a
left join TbOrderProgress b on a.OrderCode=b.OrderCode
left join TbCompany c on a.SiteCode=c.CompanyCode
left join TbCompany d on d.CompanyCode=c.ParentCompanyCode
left join TbCompany e on e.CompanyCode=d.ParentCompanyCode
where 1=1 and a.Examinestatus='审核完成' and CONVERT(varchar(7), a.InsertTime, 120)='" + HistoryMonth + "'  and a.SiteCode='" + SiteCode + "' and a.ProcessFactoryCode='" + ProcessFactoryCode + "'";
                    }
                }
                else
                {
                    sql = @"select a.SiteCode,d.CompanyCode as WorkAreaCode,e.CompanyCode as BarndCode,a.OrderStart,CONVERT(varchar(100), a.DistributionTime, 23) as DistributionTime,CONVERT(varchar(100), b.FinishProcessingDateTime, 23) as FinishProcessingDateTime,case when CONVERT(varchar(100), b.FinishProcessingDateTime, 23) is not null and CONVERT(varchar(100), b.FinishProcessingDateTime, 23)>CONVERT(varchar(100), a.DistributionTime, 23) and a.Examinestatus='审核完成' then '已完成滞后' when CONVERT(varchar(100), b.FinishProcessingDateTime, 23) is null and CONVERT(varchar(100),GETDATE(),23)>CONVERT(varchar(100), a.DistributionTime, 23) and a.Examinestatus='审核完成' then '未完成滞后' end as DdType from TbWorkOrder a
left join TbOrderProgress b on a.OrderCode=b.OrderCode
left join TbCompany c on a.SiteCode=c.CompanyCode
left join TbCompany d on d.CompanyCode=c.ParentCompanyCode
left join TbCompany e on e.CompanyCode=d.ParentCompanyCode
where 1=1  and a.SiteCode='" + SiteCode + "' and a.ProcessFactoryCode='" + ProcessFactoryCode + "'";
                }
                DataTable dt = Db.Context.FromSql(sql).ToDataTable();
                return dt;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion

    }
}
