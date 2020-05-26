using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using Dos.ORM;
using Dos.Common;
using PM.Common;
using PM.Common.Helper;
using PM.DataEntity;
using PM.DataEntity.Production.ViewModel;
using PM.Business.Production;
using PM.DataAccess.DbContext;

namespace PM.Business.RawMaterial
{
    /// <summary>
    /// 原材料生产领料
    /// </summary>
    public class RMProductionMaterialMark
    {
        private readonly RawMaterialStockRecordLogic _rawMaterialStockRecordLogic = new RawMaterialStockRecordLogic();
        private readonly TbWorkOrderLogic _workOrderLogic = new TbWorkOrderLogic();

        #region 新增数据

        /// <summary>
        /// 新增数据
        /// </summary>
        public AjaxResult Insert(TbRMProductionMaterial model, List<TbRMProductionMaterialDetail> items, List<RMProductionMaterialItmeBackRequest> itemBack)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            model.Examinestatus = "审核完成";
            //查询余料库存信息
            var data = GetCloutStock(model, items, itemBack);
            //获取库存记录
            var recordList = GetStockRecord(items, itemBack, model.CollarCode, model.WorkAreaCode);
            if (recordList.Item1.state.ToString() != ResultType.success.ToString())
                return recordList.Item1;
            //修改动态库存
            var stockRecordList = UpdateStockRecordUseCount(recordList.Item2);
            var itemPlan = new List<TbRMProductionMaterialPlan>();
            if (itemBack.Any())
            {
                //领料方案信息
                itemPlan = MapperHelper.Map<RMProductionMaterialItmeBackRequest, TbRMProductionMaterialPlan>(itemBack);
            }
            //判断是否领料完成
            var anyRet = IsFinish(model.OrderCode, items, 1);
            if (anyRet.Item1.state.ToString() != ResultType.success.ToString())
                return anyRet.Item1;
            model.CollarState = (bool)anyRet.Item1.data ? "领料完成" : "部分领料";
            ////反写加工订单中订单状态字段  注释原因：与数据库审核完成触发器修改状态冲突，这里修改了，触发器里不执行 fmm20191016
            //var modelOrder = Repository<TbWorkOrder>.First(d => d.OrderCode == model.OrderCode && d.ProcessingState == "Received");
            //if (modelOrder != null)
            //    modelOrder.ProcessingState = "AlreadyReceived";//已领料
            //反写加工订单中领料状态字段
            var modelOrder1 = Repository<TbWorkOrder>.First(d => d.OrderCode == model.OrderCode);
            modelOrder1.PickingState = (bool)anyRet.Item1.data ? "领料完成" : "部分领料";
            //反写加工订单炉批号
            var workOrderDetails = GetWorkOrderDetail(items);
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //添加信息
                    var id = Repository<TbRMProductionMaterial>.Insert(trans, model);
                    //添加明细信息
                    Repository<TbRMProductionMaterialDetail>.Insert(trans, items);
                    if (itemBack.Any())
                    {
                        //领料方案信息
                        Repository<TbRMProductionMaterialPlan>.Insert(trans, itemPlan);
                        //新增余料库存信息
                        if (data.Item1.Any())
                            Repository<TbCloutStock>.Insert(trans, data.Item1);
                        //新增余料流向信息
                        if (data.Item2.Any())
                            Repository<TbCloutStockPlace>.Insert(trans, data.Item2);
                        //新增废料信息
                        if (data.Item3.Any())
                            Repository<TbRubbishStock>.Insert(trans, data.Item3);
                    }
                    //操作库存记录
                    if (recordList.Item2.Any())
                        Repository<TbRMProductionMaterialLockCount>.Insert(trans, recordList.Item2);
                    //修改动态库存
                    if (stockRecordList.Any())
                        Repository<TbRawMaterialStockRecord>.Update(trans, stockRecordList);
                    //修改领料单领用状态
                    if (anyRet.Item2 != null)
                        Repository<TbRMProductionMaterial>.Update(trans, anyRet.Item2);
                    ////反写加工订单中加工状态
                    //if (modelOrder != null)
                    //    Repository<TbWorkOrder>.Update(trans, modelOrder);
                    //反写加工订单中领料状态
                    if (modelOrder1 != null)
                        Repository<TbWorkOrder>.Update(trans, modelOrder1);
                    //反写加工订单炉批号
                    if (workOrderDetails.Any())
                        Repository<TbWorkOrderDetail>.Update(trans, workOrderDetails);
                    trans.Commit();
                    return AjaxResult.Success(id);
                }
            }
            catch (Exception ex)
            {
                return AjaxResult.Error();
            }
        }

        #endregion

        #region 修改数据

        /// <summary>
        /// 修改数据
        /// </summary>
        public AjaxResult Update(TbRMProductionMaterial model, List<TbRMProductionMaterialDetail> items, List<RMProductionMaterialItmeBackRequest> itemBack)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            model.Examinestatus = "审核完成";
            var anyRet = AnyInfo(model.ID);
            if (anyRet.state != ResultType.success.ToString())
                return anyRet;
            //查询余料库存信息
            var data = GetCloutStock(model, items, itemBack);
            //判断是否领料完成
            var anyRetf = IsFinish(model.OrderCode, items, 2);
            if (anyRetf.Item1.state != ResultType.success.ToString())
                return anyRetf.Item1;
            model.CollarState = (bool)anyRetf.Item1.data ? "领料完成" : "部分领料";
            //反写加工订单中领料状态字段
            var modelOrder1 = Repository<TbWorkOrder>.First(d => d.OrderCode == model.OrderCode);
            modelOrder1.PickingState = (bool)anyRetf.Item1.data ? "领料完成" : "部分领料";
            //反写加工订单炉批号
            var workOrderDetails = GetWorkOrderDetail(items);
            //获取库存记录
            var recordList = GetStockRecord(items, itemBack, model.CollarCode, model.WorkAreaCode);
            if (recordList.Item1.state != ResultType.success.ToString())
                return recordList.Item1;
            //修改动态库存
            var stockRecordList = UpdateStockRecordUseCount(recordList.Item2, model.CollarCode);
            //领料方案信息
            var itemPlan = new List<TbRMProductionMaterialPlan>();
            if (itemBack.Any())
            {
                //领料方案信息
                itemPlan = MapperHelper.Map<RMProductionMaterialItmeBackRequest, TbRMProductionMaterialPlan>(itemBack);
            }
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //修改信息
                    Repository<TbRMProductionMaterial>.Update(trans, model, p => p.ID == model.ID);
                    if (items.Count > 0)
                    {
                        //删除历史明细信息
                        Repository<TbRMProductionMaterialDetail>.Delete(trans, p => p.CollarCode == model.CollarCode);
                        //添加明细信息
                        Repository<TbRMProductionMaterialDetail>.Insert(trans, items);
                        //删除领料方案信息
                        Repository<TbRMProductionMaterialPlan>.Delete(trans, p => p.CollarCode == model.CollarCode);
                        //删除余料库存信息
                        Repository<TbCloutStock>.Delete(trans, p => p.CollarCode == model.CollarCode);
                        //删除余料流向信息
                        Repository<TbCloutStockPlace>.Delete(trans, p => p.CollarCode == model.CollarCode);
                        //删除废料信息
                        Repository<TbRubbishStock>.Delete(trans, p => p.CollarCode == model.CollarCode);
                        if (itemBack.Any())
                        {
                            //添加领料方案信息
                            Repository<TbRMProductionMaterialPlan>.Insert(trans, itemPlan);
                            //新增余料库存信息
                            if (data.Item1.Any())
                                Repository<TbCloutStock>.Insert(trans, data.Item1);
                            //新增余料流向信息
                            if (data.Item2.Any())
                                Repository<TbCloutStockPlace>.Insert(trans, data.Item2);
                            //新增废料信息
                            if (data.Item3.Any())
                                Repository<TbRubbishStock>.Insert(trans, data.Item3);
                        }
                        //删除操作库存记录
                        Repository<TbRMProductionMaterialLockCount>.Delete(trans, p => p.CollarCode == model.CollarCode);
                        //操作库存记录
                        if (recordList.Item2.Any())
                            Repository<TbRMProductionMaterialLockCount>.Insert(trans, recordList.Item2);
                        //修改动态库存
                        if (stockRecordList.Any())
                            Repository<TbRawMaterialStockRecord>.Update(trans, stockRecordList);
                        //修改领料单领用状态
                        if (anyRetf.Item2 != null)
                            Repository<TbRMProductionMaterial>.Update(trans, anyRetf.Item2);
                        //反写加工订单中的领料状态
                        if (modelOrder1 != null)
                            Repository<TbWorkOrder>.Update(trans, modelOrder1);
                        //反写加工订单炉批号
                        if (workOrderDetails.Any())
                            Repository<TbWorkOrderDetail>.Update(trans, workOrderDetails);
                    }
                    trans.Commit();//提交事务

                    return AjaxResult.Success(model.ID);
                }
            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.ToString());
            }
        }

        #endregion

        #region 删除数据

        public AjaxResult DeleteForm(int keyValue)
        {
            try
            {
                //判断信息是否存在
                var anyRet = AnyInfo(keyValue);
                if (anyRet.state != ResultType.success.ToString())
                    return anyRet;
                var collarCode = ((TbRMProductionMaterial)anyRet.data).CollarCode;
                //取消修改动态库存
                var stockRecordList = cancelUseCount(collarCode);
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //删除信息
                    var count = Repository<TbRMProductionMaterial>.Delete(trans, p => p.ID == keyValue);
                    //删除明细信息
                    Repository<TbRMProductionMaterialDetail>.Delete(trans, p => p.CollarCode == collarCode);
                    //删除余料库存信息
                    Repository<TbCloutStock>.Delete(trans, p => p.CollarCode == collarCode);
                    //删除余料流向信息
                    Repository<TbCloutStockPlace>.Delete(trans, p => p.CollarCode == collarCode);
                    //删除操作库存记录
                    Repository<TbRMProductionMaterialLockCount>.Delete(trans, p => p.CollarCode == collarCode);
                    //删除领料方案信息
                    Repository<TbRMProductionMaterialPlan>.Delete(trans, p => p.CollarCode == collarCode);
                    //删除废料信息
                    Repository<TbRubbishStock>.Delete(trans, p => p.CollarCode == collarCode);
                    //修改动态库存
                    if (stockRecordList != null)
                        Repository<TbRawMaterialStockRecord>.Update(trans, stockRecordList);
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

        #region 查询数据

        /// <summary>
        /// 当月领料量分析
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public List<ReportPieResponse> PieSelect(TbRMProductionMaterialRequest request)
        {
            #region 搜索条件

            List<Parameter> parameter = new List<Parameter>();
            string where = " ";
            if (!string.IsNullOrWhiteSpace(request.CollarState))
            {
                where += " and a.CollarState=@CollarState";
                parameter.Add(new Parameter("@CollarState", request.CollarState, DbType.String, null));
            }
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where += " and a.ProjectId=@ProjectId";
                parameter.Add(new Parameter("@ProjectId", request.ProjectId, DbType.String, null));
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);
                string siteCodeStr = string.Join("','", SiteList);
                where += " and a.SiteCode in('" + siteCodeStr + "')";
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where += " and a.ProcessFactoryCode=@ProcessFactoryCode";
                parameter.Add(new Parameter("@ProcessFactoryCode", request.ProcessFactoryCode, DbType.String, null));
            }
            if (request.HistoryMonth.HasValue==true)
            {
                string CollarDate = Convert.ToDateTime(request.HistoryMonth).ToString("yyyy-MM");
                where += " and CONVERT(VARCHAR(7),a.CollarDate,120)='" + CollarDate + "'";
            }
            if (request.HistoryMonth.HasValue==false)
            {
                where += " and CONVERT(VARCHAR(7),a.CollarDate,120)=CONVERT(VARCHAR(7),GETDATE(),120)";
            }
            #endregion

            string sqlStr = @"select 
                              sum(ISNULL(b.yl,0)) as yl,--余料
                              sum(ISNULL(c.ycl,0)) as ycl--原材料
                              from TbRMProductionMaterial a
                              left join (
                              select CollarCode,SUM(WeightSmallPlanN) yl from TbRMProductionMaterialPlan
                              where RMTypeName='余料'
                              group by CollarCode
                              ) b on a.CollarCode=b.CollarCode
                              left join (
                              select CollarCode,SUM(WeightSmallPlanN) ycl from TbRMProductionMaterialPlan
                              where RMTypeName='原材料'
                              group by CollarCode
                              ) c on a.CollarCode=c.CollarCode
                              where 1=1 ";
            var ret = Repositorys<ReportPieResponse>.FromSql(sqlStr + where, parameter);
            var dataList = new List<ReportPieResponse>();
            if (ret.Any())
            {
                var data = new ReportPieResponse()
                {
                    text = "原材料领用量",
                    value = ret[0].ycl
                };
                dataList.Add(data);
                data = new ReportPieResponse()
                {
                    text = "余料领用量",
                    value = ret[0].yl
                };
                dataList.Add(data);
            }

            return dataList;
        }

        /// <summary>
        /// 各站点领料量分析
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public DataTable SitaDataReport(TbRMProductionMaterialRequest request)
        {
            #region 搜索条件

            List<Parameter> parameter = new List<Parameter>();
            string where = " ";
            if (!string.IsNullOrWhiteSpace(request.CollarStateSelected))
            {
                where += " and Tb.CollarState='" + request.CollarState + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where += " and Tb.ProjectId='" + request.ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);
                string siteCodeStr = string.Join("','", SiteList);
                where += " and Tb.SiteCode in('" + siteCodeStr + "')";
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where += " and Tb.ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
            }
            if (request.HistoryMonth.HasValue)
            {
                string CollarDate = Convert.ToDateTime(request.HistoryMonth).ToString("yyyy-MM");
                where += " and CONVERT(VARCHAR(7),Tb.CollarDate,120)='" + CollarDate + "'";
            }

            #endregion

            string sql = @"select  Tb.SiteCode,Tb.SiteName,SUM(Tb.ycl)+SUM(Tb.yl) as Total,SUM(Tb.ycl) as ycl,SUM(Tb.yl) as yl from ( select 
                             a.CollarState,
                             a.CollarDate,
                             a.SiteCode,
                             a.ProjectId,
                             a.ProcessFactoryCode,
                             d.CompanyFullName as SiteName,
                             isnull(b.yl,0) yl,
                             isnull(c.ycl,0) ycl
                            from TbRMProductionMaterial a
                            left join (
                               select CollarCode,SUM(WeightSmallPlanN) yl from TbRMProductionMaterialPlan
                               where RMTypeName='余料'
                               group by CollarCode
                            ) b on a.CollarCode=b.CollarCode
                            left join (
                               select tbycl.CollarCode,sum(tbycl.ycl) ycl from(select CollarCode,SUM(WeightSmallPlanN) ycl from TbRMProductionMaterialPlan
                               where RMTypeName='原材料'
                               group by CollarCode
                               union all
                               select CollarCode,SUM(WeightSmallPlanN) ycl from TbRMProductionMaterialDetail
                               where RootNumber=0
                               group by CollarCode) tbycl group by tbycl.CollarCode 
                            ) c on a.CollarCode=c.CollarCode 
                            left join TbCompany d on a.SiteCode=d.CompanyCode) Tb
                            where 1=1 " +where+@"
                            group by Tb.SiteCode,Tb.SiteName";
            var ret = Db.Context.FromSql(sql).ToDataTable();
            return ret;
        }

        /// <summary>
        /// 列表数据查询
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageModel GetDataListForPage(TbRMProductionMaterialRequest request)
        {
            #region 模糊搜索条件

            var where = new Where<TbRMProductionMaterial>();
            //if (!string.IsNullOrWhiteSpace(request.CollarStateSelected))
            //{
            //    where.And(p => p.CollarState == request.CollarStateSelected);
            //}
            if (!string.IsNullOrWhiteSpace(request.CollarState))
            {
                where.And(p => p.CollarState == request.CollarState);
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);
                where.And(p => p.SiteCode.In(SiteList));
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where.And(p => p.ProcessFactoryCode == request.ProcessFactoryCode);
            }
            if (!string.IsNullOrEmpty(request.ProjectId))
                where.And(p => p.ProjectId == request.ProjectId);
            if (!string.IsNullOrWhiteSpace(request.OrderCode))
            {
                where.And(p => p.OrderCode == request.OrderCode);
            }
            if (!string.IsNullOrWhiteSpace(request.CollarCode))
            {
                where.And(p => p.CollarCode == request.CollarCode);
            }
            if (request.HistoryMonth.HasValue)
            {
                var historyMonth = new WhereClip("YEAR(TbRMProductionMaterial.InsertTime)=" + request.HistoryMonth.Value.Year + " and MONTH(TbRMProductionMaterial.InsertTime)=" + request.HistoryMonth.Value.Month);
                where.And(historyMonth);
            }
            #endregion

            try
            {
                var data = Db.Context.From<TbRMProductionMaterial>()
                    .Select(
                      TbRMProductionMaterial._.ID
                    , TbRMProductionMaterial._.Examinestatus
                    , TbRMProductionMaterial._.CollarCode
                    , TbRMProductionMaterial._.CollarDate
                    , TbRMProductionMaterial._.OrderCode
                    , TbRMProductionMaterial._.CollarState
                    , TbRMProductionMaterial._.CollarPosition
                    , TbRMProductionMaterial._.InsertUserCode
                    , TbRMProductionMaterial._.InsertTime
                    , TbRMProductionMaterial._.ProjectId
                    , TbRMProductionMaterial._.TypeCode
                    , TbRMProductionMaterial._.Total
                    ,TbRMProductionMaterial._.WeightSum
                    , TbWorkOrder._.TypeName
                    , TbUser._.UserName
                    , TbCompany._.CompanyFullName.As("SiteName"))
                    .AddSelect(Db.Context.From<TbUser>()
                      .Select(p => p.UserName)
                      .Where(TbUser._.UserCode == TbRMProductionMaterial._.UserCode), "DeliverUser")
                    .AddSelect(Db.Context.From<TbCompany>()
                      .Select(p => p.CompanyFullName)
                      .Where(TbCompany._.CompanyCode == TbRMProductionMaterial._.BranchCode), "BranchName")
                    .AddSelect(Db.Context.From<TbCompany>()
                      .Select(p => p.CompanyFullName)
                      .Where(TbCompany._.CompanyCode == TbRMProductionMaterial._.WorkAreaCode), "WorkAreaName")
                    .AddSelect(Db.Context.From<TbCompany>()
                      .Select(p => p.CompanyFullName)
                      .Where(TbCompany._.CompanyCode == TbRMProductionMaterial._.ProcessFactoryCode), "ProcessFactoryName")
                  .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                  .LeftJoin<TbWorkOrder>((a, c) => a.OrderCode == c.OrderCode)
                  .LeftJoin<TbCompany>((a, c) => a.SiteCode == c.CompanyCode)
                  .Where(where)
                  .OrderByDescending(p => p.ID)
                  .ToPageList(request.rows, request.page);
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 根据ID绑定信息
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public Tuple<DataTable, DataTable, DataTable> GetFormJson(int keyValue)
        {
            //查询原材料生产领料信息
            var RM = Db.Context.From<TbRMProductionMaterial>()
             .Select(
                 TbRMProductionMaterial._.ID,
                 TbRMProductionMaterial._.ProjectId,
                 TbRMProductionMaterial._.CollarCode,
                 TbRMProductionMaterial._.CollarDate,
                 TbRMProductionMaterial._.OrderCode,
                 TbWorkOrder._.TypeName,
                 TbRMProductionMaterial._.CollarPosition,
                 TbRMProductionMaterial._.UserCode,
                 TbRMProductionMaterial._.TypeCode,
                 TbUser._.UserName,
                 TbRMProductionMaterial._.CollarState,
                 TbRMProductionMaterial._.BranchCode,
                 TbRMProductionMaterial._.WorkAreaCode,
                 TbRMProductionMaterial._.SiteCode,
                 TbRMProductionMaterial._.ProcessFactoryCode,
                 TbRMProductionMaterial._.InsertUserCode,
                 TbRMProductionMaterial._.InsertTime,
                 TbRMProductionMaterial._.Remark,
                 TbRMProductionMaterial._.Total,
                 TbRMProductionMaterial._.Enclosure
                ).LeftJoin<TbWorkOrder>((a, c) => a.OrderCode == c.OrderCode)
                .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                .Where(TbCompany._.CompanyCode == TbRMProductionMaterial._.BranchCode), "BranchName")
                .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                .Where(TbCompany._.CompanyCode == TbRMProductionMaterial._.WorkAreaCode), "WorkAreaName")
                .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                .Where(TbCompany._.CompanyCode == TbRMProductionMaterial._.SiteCode), "SiteName")
                .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                .Where(TbCompany._.CompanyCode == TbRMProductionMaterial._.ProcessFactoryCode), "ProcessFactoryName")
                .AddSelect(Db.Context.From<TbUser>().Select(p => p.UserName)
                .Where(TbUser._.UserCode == TbRMProductionMaterial._.InsertUserCode), "InsertUserName")
                .LeftJoin<TbUser>((a, c) => a.UserCode == c.UserCode)
                .Where(p => p.ID == keyValue).ToDataTable();
            //查找明细信息
            string sql = @"select a.* ,
                            isnull((a.WeightSmallPlan-isnull(b.WeightSmallPlanN,0)),0) as WeightSmallPlanNH
                            from TbRMProductionMaterialDetail a
                            left join (
                                   select 
                                   OrderCode,
                                   WorkOrderItemId,
                                   sum(WeightSmallPlanN) as WeightSmallPlanN
                                   from TbRMProductionMaterialDetail
                                   where CollarCode!=@CollarCode
                                   group by OrderCode,WorkOrderItemId) b
                             on a.OrderCode=b.OrderCode 
                            and a.WorkOrderItemId=b.WorkOrderItemId
                            where a.CollarCode=@CollarCode";
            var items = Db.Context.FromSql(sql)
                                  .AddInParameter("CollarCode", DbType.String, "" + RM.Rows[0]["CollarCode"].ToString() + "")
                                  .ToDataTable();
            //查找领料计划明细
            var itemsPlan = Db.Context.From<TbRMProductionMaterialPlan>()
                   .Select(TbRMProductionMaterialPlan._.All)
                   .Where(p => p.CollarCode == RM.Rows[0]["CollarCode"].ToString())
                   .ToDataTable();
            return new Tuple<DataTable, DataTable, DataTable>(RM, items, itemsPlan);
        }

        /// <summary>
        /// 打印
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public Tuple<DataTable, List<WorkOrderDetailDetailResponse>> GetPrinJson(int keyValue)
        {
            //查询原材料生产领料信息
            var RM = Db.Context.From<TbRMProductionMaterial>()
             .Select(
                 TbRMProductionMaterial._.CollarCode,
                 TbRMProductionMaterial._.CollarDate,
                 TbRMProductionMaterial._.OrderCode,
                 TbWorkOrder._.TypeName,
                 TbRMProductionMaterial._.CollarPosition,
                 TbRMProductionMaterial._.CollarState,
                 TbRMProductionMaterial._.Remark
                ).LeftJoin<TbWorkOrder>((a, c) => a.OrderCode == c.OrderCode)
                .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                .Where(TbCompany._.CompanyCode == TbRMProductionMaterial._.BranchCode), "BranchName")
                .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                .Where(TbCompany._.CompanyCode == TbRMProductionMaterial._.WorkAreaCode), "WorkAreaName")
                .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                .Where(TbCompany._.CompanyCode == TbRMProductionMaterial._.SiteCode), "SiteName")
                .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                .Where(TbCompany._.CompanyCode == TbRMProductionMaterial._.ProcessFactoryCode), "ProcessFactoryName")
                .Where(p => p.ID == keyValue).ToDataTable();

            //查找明细信息
            var items = Db.Context.From<TbRMProductionMaterialDetail>()
                   .Select(TbRMProductionMaterialDetail._.All)
                   .Where(p => p.CollarCode == RM.Rows[0]["CollarCode"].ToString())
                   .ToList<WorkOrderDetailDetailResponse>();
            //查找领料计划明细
            var itemsPlan = Db.Context.From<TbRMProductionMaterialPlan>()
                   .Select(TbRMProductionMaterialPlan._.All)
                   .Where(p => p.CollarCode == RM.Rows[0]["CollarCode"].ToString())
                   .ToList<RMProductionMaterialItmeBackRequest>();

            foreach (var i in items)
            {
                var plan = itemsPlan.Where(p => p.WorkOrderItemId == i.WorkOrderItemId).ToList();
                i.plan = plan;
            }
            return new Tuple<DataTable, List<WorkOrderDetailDetailResponse>>(RM, items);
        }

        /// <summary>
        /// 判断信息是否可操作
        /// </summary>
        /// <returns></returns>
        public AjaxResult AnyInfo(int keyValue)
        {
            var CollarCode = Repository<TbRMProductionMaterial>.First(p => p.ID == keyValue);
            if (CollarCode == null)
                return AjaxResult.Warning("信息不存在");
            //if (CollarCode.Examinestatus != "未发起" && CollarCode.Examinestatus != "已退回")
            //    return AjaxResult.Warning("信息正在审核中或已审核完成,不能进行此操作");

            return AjaxResult.Success(CollarCode);
        }

        #endregion

        #region Private

        /// <summary>
        /// 获取余料库存
        /// </summary>
        /// <param name="model">领料数据</param>
        /// <param name="items">领料明细数据</param>
        /// <returns>
        /// item1:余料库存(新增)
        /// item2:余料流向
        /// item3:废料
        /// </returns>
        private Tuple<List<TbCloutStock>, List<TbCloutStockPlace>, List<TbRubbishStock>> GetCloutStock(TbRMProductionMaterial model, List<TbRMProductionMaterialDetail> items, List<RMProductionMaterialItmeBackRequest> itemBack)
        {
            var list = new List<TbCloutStock>();
            var stockPlaceList = new List<TbCloutStockPlace>();
            var rubbishList = new List<TbRubbishStock>();
            if (!itemBack.Any())
            {
                return new Tuple<List<TbCloutStock>, List<TbCloutStockPlace>, List<TbRubbishStock>>(list, stockPlaceList, rubbishList);
            }
            //查询余料库存信息
            var cloutStockIDs = itemBack.Where(p => p.RMTypeName == "余料").Select(p => p.StockID).ToList();
            var cloutStockS = Repository<TbCloutStock>.Query(p => p.ID.In(cloutStockIDs)).ToList();
            if (cloutStockS.Any())
            {
                cloutStockS.ForEach(x =>
                {
                    var stock = itemBack.Where(p => p.StockID == x.ID).FirstOrDefault();
                    if (stock != null)
                    {
                        //添加余料流向数据
                        var stockPlace = MapperHelper.Map<TbCloutStock, TbCloutStockPlace>(x);
                        stockPlace.CollarCode = model.CollarCode;
                        stockPlace.EndSiteCode = model.SiteCode;
                        stockPlace.StartSiteCode = x.SiteCode;
                        stockPlace.EndProjectId = model.ProjectId;
                        stockPlace.ProjectId = x.ProjectId;
                        stockPlace.Size = x.Size;
                        stockPlace.Number = stock.RootNumber;
                        stockPlace.Weight = x.Size * stock.RootNumber * x.MeasurementUnitZl;
                        stockPlace.PlaceTime = DateTime.Now;
                        stockPlace.State = 0;
                        stockPlaceList.Add(stockPlace);
                    }
                });
            }
            //生成余料库存
            var ret = AddYL(model, items, itemBack);
            if (ret.Item1.Any())
                list.AddRange(ret.Item1);
            if (ret.Item2.Any())
                rubbishList.AddRange(ret.Item2);
            return new Tuple<List<TbCloutStock>, List<TbCloutStockPlace>, List<TbRubbishStock>>(list, stockPlaceList, rubbishList);
        }

        /// <summary>
        /// 判断是否领料完成
        /// </summary>
        /// <param name="orderCode">订单编号</param>
        /// <param name="items">领料明细数据</param>
        /// <param name="type">1添加2修改</param>
        /// <returns></returns>
        private Tuple<AjaxResult, TbRMProductionMaterial> IsFinish(string orderCode, List<TbRMProductionMaterialDetail> items, int type)
        {
            //判断其他领料单是否领料完成
            var RMProduct = Repository<TbRMProductionMaterial>.First(p => p.OrderCode == orderCode && p.CollarState == "领料完成");
            if (RMProduct != null)
            {
                //if (RMProduct.Examinestatus != "未发起")
                //    return new Tuple<AjaxResult, TbRMProductionMaterial>(AjaxResult.Error("该加工订单的领料信息已完成且正在审核中"), null);
            }
            //订单明细
            var OrderDetail = Repository<TbWorkOrderDetail>.Query(p => p.OrderCode == orderCode).ToList();
            var WeightSmallPlan = OrderDetail.Sum(p => p.WeightSmallPlan);
            //领料单明细
            var RMProducDetail = Repository<TbRMProductionMaterialDetail>.Query(p => p.OrderCode == orderCode).ToList();
            var WeightSmallPlanN = RMProducDetail.Sum(p => p.WeightSmallPlanN);
            var count = RMProducDetail.Count;
            if (type == 2)
            {
                WeightSmallPlanN = RMProducDetail.Where(p => p.CollarCode != items[0].CollarCode).Sum(p => p.WeightSmallPlanN);
                count = RMProducDetail.Where(p => p.CollarCode != items[0].CollarCode).ToList().Count;
                if (RMProduct != null && RMProduct.CollarCode == items[0].CollarCode)
                    RMProduct = null;
            }
            //领料单明细(页面)
            WeightSmallPlanN += items.Sum(p => p.WeightSmallPlanN);
            count += items.Count;
            //领料完成： 领料单重量小计>=加工订单重量小计 && 领料单明细数量= 加工订单明细数量
            if (WeightSmallPlanN >= WeightSmallPlan && OrderDetail.Count == count)
            {
                return new Tuple<AjaxResult, TbRMProductionMaterial>(AjaxResult.Success(true), null);
            }
            else
            {
                //领料单已领料完成但未进入审核中，相关领料单改变领料信息,调整为部分领料
                if (RMProduct != null)
                {
                    RMProduct.CollarState = "部分领料";
                    return new Tuple<AjaxResult, TbRMProductionMaterial>(AjaxResult.Success(false), RMProduct);
                }
            }

            return new Tuple<AjaxResult, TbRMProductionMaterial>(AjaxResult.Success(false), null);
        }

        /// <summary>
        /// 计算余料数据
        /// </summary>
        /// <param name="item"></param>
        /// <param name="size">余料尺寸</param>
        /// <param name="number">余料数量</param>
        private void GetSizeNumber(TbRMProductionMaterial model, RMProductionMaterialItmeBackRequest item, List<TbCloutStock> yl, List<TbRubbishStock> fl)
        {
            //计算正常需要的根数
            CloutStockResponse re = new CloutStockResponse();
            re.SizeSelection = item.SizeSelection;
            re.ItemUseNum = item.ItemUseNum;
            re.UseNumber = item.UseNumber;
            re.Number = item.NumberH;
            re.IsPJOk = CloutStockLogic.ispjok(item.SpecificationModel, item.ItemUseNum);
            bool isyl = item.RMTypeName == "余料";
            //判断是否圆钢
            if (item.MaterialName == "圆钢" && isyl)
            {
                CloutStockLogic.GetRootNumberForYG(re);
            }
            else
            {
                CloutStockLogic.GetRootNumber2(re, isyl);
            }
            var rootNumber = 0;
            var numberH = 0;
            decimal size = 0;
            int number = 0;
            if (item.NumberH > item.Number || item.RootNumber > re.RootNumber || re.NumberH > item.NumberH)//多领
            {
                //多领的根数
                int a = item.RootNumber - re.RootNumber;
                if (a > 0)
                {
                    if (isyl)
                    {
                        addStockInfo(model, item, yl, fl, item.SizeSelection, a);
                    }
                    else
                    {
                        item.RootNumberCL = a;
                    }
                }
                //多领的件数
                int b = re.NumberH - item.NumberH;
                if (b > 0)
                {
                    //判断拼接还是剪切
                    if (re.pjorjq)
                    {
                        //拼接
                        //判断一个需要多少材料
                        var num = (int)Math.Ceiling(item.ItemUseNum / item.SizeSelection);
                        item.RootNumberCL = num * b;
                    }
                    else
                    {
                        //剪切
                        addStockInfo(model, item, yl, fl, b * item.ItemUseNum, 1);
                    }
                }

                rootNumber = re.RootNumber;
                numberH = re.NumberH;
            }
            else
            {
                rootNumber = item.RootNumber;
                numberH = item.NumberH;
            }
            if (re.hxcd > 0)
            {
                if (re.pjorjq)
                {
                    //拼接
                    if (re.ispj)
                    {
                        //使用的材料多余的部分
                        if (re.hxcdLast > 0)
                        {
                            number = (int)Math.Floor(numberH / re.CountLast);
                            size = re.hxcdLast;
                            addStockInfo(model, item, yl, fl, size, number);
                        }
                        number = 1;
                        size = re.hxcd;
                    }
                    else
                    {   //使用的材料所产生的余料
                        number = numberH;
                        size = re.hxcd;
                    }
                    addStockInfo(model, item, yl, fl, size, number);
                }
                else
                {
                    //剪切
                    if (re.ispj)
                    {
                        //使用的材料多余的部分
                        if (re.hxcdLast > 0)
                        {
                            number = (int)Math.Floor(numberH / re.CountLast);
                            size = re.hxcdLast;
                            addStockInfo(model, item, yl, fl, size, number);
                        }
                        number = 1;
                        size = re.hxcd;
                    }
                    else
                    {   //使用的材料所产生的余料
                        number = rootNumber;
                        size = re.hxcd;
                    }
                    addStockInfo(model, item, yl, fl, size, number);
                }
            }
        }

        /// <summary>
        /// 添加库存信息
        /// </summary>
        /// <param name="model"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        private Tuple<List<TbCloutStock>, List<TbRubbishStock>> AddYL(TbRMProductionMaterial model, List<TbRMProductionMaterialDetail> items, List<RMProductionMaterialItmeBackRequest> itemsBack)
        {
            var list = new List<TbCloutStock>();
            var fllist = new List<TbRubbishStock>();
            //筛选出圆钢，螺纹钢
            var itemList = items.Where(p => p.MaterialName == "圆钢" || p.MaterialName == "螺纹钢").ToList();
            if (itemList.Any())
            {
                itemList.ForEach(x =>
                {
                    //查找同一条明细的材料信息
                    var backMaterials = itemsBack.Where(p => p.WorkOrderItemId == x.WorkOrderItemId).ToList();
                    List<int> ids = new List<int>();
                    foreach (var item in backMaterials)
                    {
                        item.MaterialName = x.MaterialName;
                        item.SpecificationModel = x.SpecificationModel;
                        if (string.IsNullOrEmpty(item.Factory))
                            item.Factory = x.Manufactor.Split(',')[0];     //？？？这样取值待考虑
                        if (string.IsNullOrEmpty(item.BatchNumber))
                            item.BatchNumber = x.HeatNo.Split(',')[0];
                        if (item.PlanIndex == 3)//不同尺寸
                        {
                            //查找同一组数据
                            var tyd = backMaterials.FirstOrDefault(p => p.LableStr == item.LableStr && p.StockID != item.StockID && !ids.Contains(p.StockID));
                            if (tyd != null)
                            {
                                tyd.ItemUseNum = x.ItemUseNum - item.SizeSelection;
                                tyd.MaterialName = x.MaterialName;
                                GetSizeNumber(model, tyd, list, fllist);
                                ids.Add(tyd.StockID);
                                ids.Add(item.StockID);
                            }
                        }
                        else if (item.PlanIndex == 4)//拼接原材料
                        {
                            if (item.RMTypeName == "余料")
                            {
                                //查找同组原材料
                                var yclpj = backMaterials.FirstOrDefault(p => p.PlanIndex == 4 && p.RMTypeName == "原材料");
                                if (yclpj != null)
                                {
                                    var l = Math.Floor(x.ItemUseNum / yclpj.SizeSelection) * yclpj.SizeSelection;
                                    item.ItemUseNum = x.ItemUseNum - l;
                                    GetSizeNumber(model, item, list, fllist);
                                }
                            }
                        }
                        else
                        {
                            GetSizeNumber(model, item, list, fllist);
                        }
                    }
                });
            }
            return new Tuple<List<TbCloutStock>, List<TbRubbishStock>>(list, fllist);
        }

        private void addStockInfo(TbRMProductionMaterial model, RMProductionMaterialItmeBackRequest x, List<TbCloutStock> yl, List<TbRubbishStock> fl, decimal size, int number)
        {
            decimal rubbish = 0.5M;//废料标准
            if (size > 0 && number > 0)
            {
                if (size >= rubbish)
                {
                    //判断是否已有记录
                    var ylrecode = yl.FirstOrDefault(p => p.MaterialCode == x.MaterialCode && p.Factory == x.Factory && p.BatchNumber == x.BatchNumber && p.Size == size);
                    if (ylrecode != null)
                    {
                        ylrecode.Number += number;
                        ylrecode.Weight += size * number * x.MeasurementUnitZl;
                    }
                    else
                    {
                        //进入余料库
                        var cloutStock = new TbCloutStock()
                        {
                            CollarCode = model.CollarCode,
                            SiteCode = model.SiteCode,
                            ProcessFactoryCode = model.ProcessFactoryCode,
                            ProjectId = model.ProjectId,
                            WorkOrderCode = model.OrderCode,
                            MaterialCode = x.MaterialCode,
                            MeasurementUnitZl = x.MeasurementUnitZl,
                            Number = number,
                            State = 0,
                            SourceType = 2,
                            Size = size,
                            Weight = size * number * x.MeasurementUnitZl,
                            SpecificationModel = x.SpecificationModel,
                            MaterialName = x.MaterialName,
                            Factory = x.Factory,
                            BatchNumber = x.BatchNumber,
                            TestReportNo = x.TestReportNo,
                            InsertTime = DateTime.Now
                        };
                        yl.Add(cloutStock);
                    }
                }
                else
                {
                    //进入废料库
                    var rubbishStock = new TbRubbishStock()
                    {
                        CollarCode = model.CollarCode,
                        SiteCode = model.SiteCode,
                        ProjectId = model.ProjectId,
                        MaterialCode = x.MaterialCode,
                        Number = number,
                        State = 0,
                        Size = size,
                        WeightNot = size * number * x.MeasurementUnitZl,
                        WeightYes = 0,
                        Factory = x.Factory,
                        BatchNumber = x.BatchNumber
                    };
                    fl.Add(rubbishStock);
                }
            }
        }

        /// <summary>
        /// 反写加工订单炉批号
        /// </summary>
        /// <returns></returns>
        private List<TbWorkOrderDetail> GetWorkOrderDetail(List<TbRMProductionMaterialDetail> items)
        {
            var workOrderItemIds = items.Select(p => p.WorkOrderItemId).ToList();
            //查找加工订单明细
            var workOrderDetails = Repository<TbWorkOrderDetail>.Query(p => p.ID.In(workOrderItemIds)).ToList();
            if (workOrderDetails.Any())
            {
                workOrderDetails.ForEach(x =>
                {
                    var item = items.FirstOrDefault(p => p.WorkOrderItemId == x.ID);
                    x.Manufactor = item.Manufactor;
                    x.HeatNo = item.HeatNo;
                    x.TestReportNo = item.TestReportNo;
                });
            }
            return workOrderDetails;
        }

        /// <summary>
        /// 获取库存记录（原材料）
        /// </summary>
        /// <param name="Backtems"></param>
        /// <param name="collarCode"></param>
        /// <returns></returns>
        private Tuple<AjaxResult, List<TbRMProductionMaterialLockCount>> GetStockRecord(List<TbRMProductionMaterialDetail> items, List<RMProductionMaterialItmeBackRequest> Backtems, string collarCode, string workAreaCode)
        {
            List<TbRMProductionMaterialLockCount> recordList = new List<TbRMProductionMaterialLockCount>();
            var stockRecords = new List<TbRawMaterialStockRecord>();
            var stockRecordsyl = new List<TbCloutStock>();
            var stockIDListStr = items.Select(p => p.InOrderItemId).ToList();
            var stockIDList = string.Join(",", stockIDListStr).Split(',');
            stockRecords = Repository<TbRawMaterialStockRecord>.Query(p => p.ID.In(stockIDList)).ToList();
            if (Backtems.Any())
            {
                var dataItemsyl = Backtems.Where(p => p.RMTypeName == "余料").ToList();
                if (dataItemsyl.Any())
                {
                    //查找余料库存记录
                    var stockID = dataItemsyl.Select(p => p.StockID).ToList();
                    stockRecordsyl = Repository<TbCloutStock>.Query(p => p.ID.In(stockID)).ToList();
                }
                foreach (var x in Backtems)
                {
                    if (x.RMTypeName == "原材料")
                    {
                        //查找原材料库存记录
                        var ids = items.Where(p => p.WorkOrderItemId == x.WorkOrderItemId).First().InOrderItemId.Split(',');
                        var stockRecordsList = stockRecords.Where(p => ids.Contains(p.ID + "")).OrderBy(p => p.InsertTime).ToList();
                        //计算重量
                        decimal count = x.SizeSelection * x.MeasurementUnitZl * (x.RootNumber - x.RootNumberCL);
                        ReduceYCL(count, recordList, stockRecordsList, collarCode, x.WorkOrderItemId);
                    }
                    else
                    {
                        var lockCountData = new TbRMProductionMaterialLockCount();
                        lockCountData.CollarCode = collarCode;
                        lockCountData.StockRecordId = x.StockID;
                        var stockRecordyl = stockRecordsyl.First(p => p.ID == x.StockID);
                        if (x.RootNumber > stockRecordyl.Number)
                            return new Tuple<AjaxResult, List<TbRMProductionMaterialLockCount>>(AjaxResult.Error("余料库存数量不足"), recordList);
                        lockCountData.LockCount = x.RootNumber;
                        lockCountData.SourceType = 2;
                        recordList.Add(lockCountData);
                    }
                }
            }
            //非圆钢螺纹钢扣减
            var aList = items.Where(p => p.MaterialName != "圆钢" && p.MaterialName != "螺纹钢").ToList();
            aList.ForEach(x =>
            {
                //查找原材料库存记录
                var ids = items.Where(p => p.WorkOrderItemId == x.WorkOrderItemId).First().InOrderItemId.Split(',');
                var stockRecordsList = stockRecords.Where(p => ids.Contains(p.ID + "")).OrderBy(p => p.InsertTime).ToList();
                ReduceYCL(x.WeightSmallPlanN, recordList, stockRecordsList, collarCode, x.WorkOrderItemId.Value);
            });
            return new Tuple<AjaxResult, List<TbRMProductionMaterialLockCount>>(AjaxResult.Success(), recordList);
        }

        /// <summary>
        /// 原材料库存减扣
        /// </summary>
        /// <param name="weight"></param>
        /// <param name="recordList"></param>
        /// <param name="stockRecord"></param>
        private void ReduceYCL(decimal weight, List<TbRMProductionMaterialLockCount> recordList, List<TbRawMaterialStockRecord> stockRecord, string collarCode, int woid)
        {
            //计算重量
            decimal count = weight;
            decimal countp = weight;
            decimal lockCount = 0.00M;
            decimal tCount = 0.00M;
            for (int i = 0; i < stockRecord.Count; i++)
            {
                if (tCount == countp)
                    break;
                if (stockRecord[i].Count - count > 0)
                {
                    lockCount = count;
                }
                else
                {
                    lockCount = stockRecord[i].Count;
                }
                tCount += lockCount;
                count = countp - tCount;
                if (i == stockRecord.Count - 1)
                {
                    if (count > 0)
                        lockCount += count;
                }
                var lockCountData = new TbRMProductionMaterialLockCount();
                lockCountData.CollarCode = collarCode;
                lockCountData.WorkOrderItemId = woid;
                lockCountData.StockRecordId = stockRecord[i].ID;
                lockCountData.LockCount = lockCount;
                lockCountData.SourceType = 1;
                recordList.Add(lockCountData);
            }
        }

        /// <summary>
        /// 修改动态库存
        /// </summary>
        /// <param name="recordList"></param>
        /// <returns></returns>
        private List<TbRawMaterialStockRecord> UpdateStockRecordUseCount(List<TbRMProductionMaterialLockCount> recordList, string collarCode = "")
        {
            var stockRecordList = new List<TbRawMaterialStockRecord>();
            var lockCountNew = new List<TbRMProductionMaterialLockCount>();
            var lockCountOld = new List<TbRMProductionMaterialLockCount>();

            if (!recordList.Any())
                return stockRecordList;
            recordList = recordList.Where(p => p.SourceType == 1).ToList();
            // 取消修改动态库存
            var cancelRecord = cancelUseCount(collarCode);
            if (cancelRecord != null)
            {
                if (recordList.Any())
                {
                    var cid = cancelRecord.Select(p => p.ID).ToList();
                    lockCountNew = recordList.Where(p => !cid.Contains(p.StockRecordId.Value)).ToList();
                    lockCountOld = recordList.Where(p => cid.Contains(p.StockRecordId.Value)).ToList();
                }
            }
            else
            {
                lockCountNew = recordList;
            }
            //新增动态库存
            if (lockCountNew.Any())
            {
                var stockRecordId = lockCountNew.Select(p => p.StockRecordId).Distinct().ToList();
                var stockRecord = Repository<TbRawMaterialStockRecord>.Query(p => p.ID.In(stockRecordId)).ToList();
                if (stockRecord.Any())
                {
                    stockRecord.ForEach(x =>
                    {
                        var lockCount = lockCountNew.Where(p => p.StockRecordId == x.ID).Sum(p => p.LockCount.Value);
                        x.UseCount -= lockCount;
                    });
                    stockRecordList.AddRange(stockRecord);
                }
            }
            //历史动态库存
            if (cancelRecord != null)
            {
                cancelRecord.ForEach(x =>
                {
                    if (lockCountOld.Any())
                    {
                        var lockCount = lockCountOld.Where(p => p.StockRecordId == x.ID).Sum(p => p.LockCount.Value);
                        x.UseCount -= lockCount;
                    }
                });
                stockRecordList.AddRange(cancelRecord);
            }
            return stockRecordList;
        }

        /// <summary>
        /// 取消修改动态库存
        /// </summary>
        /// <param name="collarCode"></param>
        /// <returns></returns>
        private List<TbRawMaterialStockRecord> cancelUseCount(string collarCode)
        {
            if (string.IsNullOrEmpty(collarCode))
                return null;
            var lockCountList = Repository<TbRMProductionMaterialLockCount>.Query(p => p.CollarCode == collarCode && p.SourceType == 1).ToList();
            if (!lockCountList.Any())
                return null;
            var stockRecordId = lockCountList.Select(p => p.StockRecordId).Distinct().ToList();
            var stockRecord = Repository<TbRawMaterialStockRecord>.Query(p => p.ID.In(stockRecordId)).ToList();
            if (!stockRecord.Any())
                return null;
            stockRecord.ForEach(x =>
            {
                var lockCount = lockCountList.Where(p => p.StockRecordId == x.ID).Sum(p => p.LockCount.Value);
                x.UseCount += lockCount;
            });
            return stockRecord;
        }

        #endregion

        #region 弹窗

        #region 原材料生产领料添加/修改界面-加工订单信息查询
        public PageModel TypeNameSelect(RMProductionMaterialRequest request)
        {
            string where = "";
            if (!string.IsNullOrWhiteSpace(request.keyword))
            {
                where += " and TypeName like '%" + request.keyword + "%' or UsePart='" + request.keyword + "' or O.OrderCode like '%" + request.keyword + "%' ";
            }
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where += " and O.ProjectId='" + request.ProjectId + "' ";
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where += " and O.ProcessFactoryCode='" + request.ProcessFactoryCode + "' ";
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);//站点
                List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 4);//工区
                string siteStr = string.Join("','", SiteList);
                string workAreaStr = string.Join("','", WorkAreaList);
                where += " and (CC.CompanyCode in('" + siteStr + "') or A.CompanyCode in('" + workAreaStr + "'))";
            }
            //加工订单信息查询
            string sql = @"--*********************加工订单信息查询***********************
                            SELECT O.ID,O.OrderCode,
                                O.ProjectId,                
		                        TypeName,
		                        TypeCode,
		                        UsePart CollarPosition,
		                        O.ProcessFactoryCode ProcessFactoryCode,
		                        C.CompanyFullName ProcessFactoryName,
		                        CC.CompanyFullName SiteName,
		                        CC.CompanyCode SiteCode,
		                        A.CompanyFullName WorkAreaName,
		                        A.CompanyCode WorkAreaCode,
		                        B.CompanyCode BranchCode,
		                        B.CompanyFullName BranchName
		                        FROM TbWorkOrder O
		                        LEFT JOIN TbCompany C ON O.ProcessFactoryCode=C.CompanyCode
		                        LEFT JOIN TbCompany CC ON CC.CompanyCode=O.SiteCode
		                        LEFT JOIN View_CompanyParentCodeName A ON A.OrderCode=O.OrderCode
		                        LEFT JOIN (
									 SELECT A.OrderCode,C.CompanyFullName,C.CompanyCode FROM TbCompany C
											INNER JOIN (
											SELECT C.ParentCompanyCode,V.OrderCode FROM View_CompanyParentCodeName V
											INNER JOIN TbCompany C ON V.CompanyCode=C.CompanyCode) A 
											ON A.ParentCompanyCode=C.CompanyCode
		                        ) B ON B.OrderCode=O.OrderCode
		                        WHERE O.OrderCode not in(
		                                       SELECT DISTINCT OrderCode
		                                       FROM TbRMProductionMaterial
		                                       WHERE CollarState='领料完成'
		                                      ) 
                                and O.Examinestatus='审核完成' and O.ProcessingState !='Finishing' and O.OrderStart !='全部撤销' ";
            try
            {
                List<Parameter> parameter = new List<Parameter>();
                var data = Repository<TbRMProductionMaterial>.FromSqlToPageTable(sql + where, parameter, request.rows, request.page, "ID", "desc");
                return data;
            }
            catch (Exception)
            {
                throw;
            }

        }
        #endregion

        #region 原材料生产领料添加/修改界面-领用人信息查询
        public PageModel InsertUserNameSelect(RMProductionMaterialRequest request)
        {
            var where = new Where<TbUser>();
            if (!string.IsNullOrWhiteSpace(request.keyword))
            {
                where.And(p => p.UserName.Like(request.keyword) ||
                               p.UserClosed.Like(request.keyword) ||
                               p.UserSex.Like(request.keyword));
            }
            var data = Db.Context.From<TbUser>().Select(
                TbUser._.ID,
                TbUser._.UserId,
                TbUser._.UserCode,
                TbUser._.UserName,
                TbUser._.UserClosed.As("InsertUserCode"),
                TbUser._.UserSex.As("CollarState"),
                TbUser._.MobilePhone,
                TbUser._.IDNumber)
                .Where(where)
                .OrderByDescending(p => p.ID)
                .ToPageList(request.rows, request.page);
            return data;
        }
        #endregion

        #region 原材料生产领料明细信息查询操作,根据OrderCode查询对应的加工订单明细信息
        public PageModel RMProductionMDetailSelect(RMProductionMaterialRequest request)
        {
            try
            {
                string sql = @"SELECT
                                D.ID as WorkOrderItemId,
                                D.OrderCode,
                                D.ComponentName,
                                D.MaterialCode,
                                D.MaterialName,
                                D.SpecificationModel,
                                D.MeasurementUnitZl,
                                D.ItemUseNum,
                                D.Number,
                                D.Manufactor,
                                D.HeatNo,
                                D.TestReportNo,
                                D.WeightSmallPlan,
                               (d.WeightSmallPlan-isnull(b.WeightSmallPlanN,0)) as WeightSmallPlanNH
                               FROM TbWorkOrderDetail D
                               left join (
                                   select 
                                   OrderCode,
                                   WorkOrderItemId,
                                   sum(WeightSmallPlanN) as WeightSmallPlanN
                                   from TbRMProductionMaterialDetail
                                   group by OrderCode,WorkOrderItemId) b
                               on d.OrderCode=b.OrderCode
                               and d.ID=b.WorkOrderItemId
                               WHERE ((ISNULL(@keyword,'')='' OR D.ComponentName LIKE @keyword)
                               OR (ISNULL(@keyword,'')='' OR D.MaterialName LIKE @keyword))
                               AND D.OrderCode=@OrderCode and (d.WeightSmallPlan-isnull(b.WeightSmallPlanN,0))>0 and D.RevokeStart='正常'";

                List<Parameter> parameter = new List<Parameter>();
                parameter.Add(new Parameter("@keyword", '%' + request.keyword + '%', DbType.String, null));
                parameter.Add(new Parameter("@OrderCode", request.OrderCode, DbType.String, null));
                var data = Repository<TbRMProductionMaterial>.FromSqlToPageTable(sql, parameter, request.rows, request.page, "WorkOrderItemId", "desc");
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable RMProductionMDetailSelectAll(RMProductionMaterialRequest request)
        {
            try
            {
                string sql = @"SELECT
                                D.ID as WorkOrderItemId,
                                D.OrderCode,
                                D.ComponentName,
                                D.MaterialCode,
                                D.MaterialName,
                                D.SpecificationModel,
                                D.MeasurementUnitZl,
                                D.ItemUseNum,
                                D.Number,
                                D.Manufactor,
                                D.HeatNo,
                                D.TestReportNo,
                                D.WeightSmallPlan,
                               (d.WeightSmallPlan-isnull(b.WeightSmallPlanN,0)) as WeightSmallPlanNH
                               FROM TbWorkOrderDetail D
                               left join (
                                   select 
                                   OrderCode,
                                   WorkOrderItemId,
                                   sum(WeightSmallPlanN) as WeightSmallPlanN
                                   from TbRMProductionMaterialDetail
                                   group by OrderCode,WorkOrderItemId) b
                               on d.OrderCode=b.OrderCode
                               and d.ID=b.WorkOrderItemId
                               WHERE ((ISNULL(@keyword,'')='' OR D.ComponentName LIKE @keyword)
                               OR (ISNULL(@keyword,'')='' OR D.MaterialName LIKE @keyword))
                               AND D.OrderCode=@OrderCode and (d.WeightSmallPlan-isnull(b.WeightSmallPlanN,0))>0 and D.RevokeStart='正常'";

                var ret = Db.Context.FromSql(sql)
                    .AddInParameter("@keyword", DbType.String, '%' + request.keyword + '%')
                    .AddInParameter("@OrderCode", DbType.String, request.OrderCode)
                                 .ToDataTable();
                return ret;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        /// <summary>
        /// 获取数据列表(厂家，炉批号，质检报告)
        /// </summary>
        public PageModel GetTestReportList(InOrderRequest request)
        {
            var ret = _rawMaterialStockRecordLogic.GetDataListForPage(request);
            return ret;
        }

        #endregion

        #region 导出

        /// <summary>
        /// 获取导出数据列表
        /// </summary>
        public DataTable GetExportList(TbRMProductionMaterialRequest request)
        {
            #region 模糊搜索条件

            var where = new Where<TbRMProductionMaterial>();
            if (!string.IsNullOrWhiteSpace(request.CollarState))
            {
                where.And(p => p.CollarState == request.CollarState);
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);
                where.And(p => p.SiteCode.In(SiteList));
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where.And(p => p.ProcessFactoryCode == request.ProcessFactoryCode);
            }
            if (!string.IsNullOrEmpty(request.ProjectId))
                where.And(p => p.ProjectId == request.ProjectId);
            if (!string.IsNullOrWhiteSpace(request.OrderCode))
            {
                where.And(p => p.OrderCode == request.OrderCode);
            }
            if (!string.IsNullOrWhiteSpace(request.CollarCode))
            {
                where.And(p => p.CollarCode == request.CollarCode);
            }
            if (request.HistoryMonth.HasValue)
            {
                var historyMonth = new WhereClip("YEAR(TbRMProductionMaterial.InsertTime)=" + request.HistoryMonth.Value.Year + " and MONTH(TbRMProductionMaterial.InsertTime)=" + request.HistoryMonth.Value.Month);
                where.And(historyMonth);
            }
            #endregion

            try
            {
                var data = Db.Context.From<TbRMProductionMaterial>()
                    .Select(
                      TbRMProductionMaterial._.ID
                    , TbRMProductionMaterial._.Examinestatus
                    , TbRMProductionMaterial._.CollarCode
                    , TbRMProductionMaterial._.CollarDate
                    , TbRMProductionMaterial._.OrderCode
                    , TbRMProductionMaterial._.CollarState
                    , TbRMProductionMaterial._.CollarPosition
                    , TbRMProductionMaterial._.InsertUserCode
                    , TbRMProductionMaterial._.InsertTime
                    , TbRMProductionMaterial._.TypeCode
                    , TbRMProductionMaterial._.Total
                    , TbRMProductionMaterial._.WeightSum
                    , TbWorkOrder._.TypeName
                    , TbUser._.UserName
                    , TbCompany._.CompanyFullName.As("SiteName"))
                    .AddSelect(Db.Context.From<TbUser>()
                      .Select(p => p.UserName)
                      .Where(TbUser._.UserCode == TbRMProductionMaterial._.UserCode), "DeliverUser")
                    .AddSelect(Db.Context.From<TbCompany>()
                      .Select(p => p.CompanyFullName)
                      .Where(TbCompany._.CompanyCode == TbRMProductionMaterial._.BranchCode), "BranchName")
                    .AddSelect(Db.Context.From<TbCompany>()
                      .Select(p => p.CompanyFullName)
                      .Where(TbCompany._.CompanyCode == TbRMProductionMaterial._.WorkAreaCode), "WorkAreaName")
                    .AddSelect(Db.Context.From<TbCompany>()
                      .Select(p => p.CompanyFullName)
                      .Where(TbCompany._.CompanyCode == TbRMProductionMaterial._.ProcessFactoryCode), "ProcessFactoryName")
                  .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                  .LeftJoin<TbWorkOrder>((a, c) => a.OrderCode == c.OrderCode)
                  .LeftJoin<TbCompany>((a, c) => a.SiteCode == c.CompanyCode)
                  .Where(where)
                  .OrderByDescending(p => p.ID).ToDataTable();
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion
    }
}
