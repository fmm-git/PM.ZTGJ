using Dos.ORM;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using PM.DataEntity.Production.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Business.Production
{
    /// <summary>
    /// 订单变更
    /// </summary>
    public class ProblemOrderLogic
    {
        private readonly TbWorkOrderLogic _workOrderLogic = new TbWorkOrderLogic();

        #region 新增数据

        /// <summary>
        /// 新增数据
        /// </summary>
        public AjaxResult Insert(TbProblemOrder model, List<TbProblemOrderItem> items)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            model.InsertUserCode = OperatorProvider.Provider.CurrentUser.UserCode;
            model.Examinestatus = "未发起";
            model.RevokeStatus = "未撤销";
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //添加信息
                    Repository<TbProblemOrder>.Insert(trans, model);
                    //添加明细信息
                    Repository<TbProblemOrderItem>.Insert(trans, items);
                    trans.Commit();
                    return AjaxResult.Success();
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
        public AjaxResult Update(TbProblemOrder model, List<TbProblemOrderItem> items)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            var anyRet = AnyInfo(model.ID);
            if (anyRet.state != ResultType.success.ToString())
                return anyRet;
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())//使用事务
                {
                    //修改信息
                    Repository<TbProblemOrder>.Update(trans, model, p => p.ID == model.ID);
                    if (items.Count > 0)
                    {
                        //删除历史明细信息
                        Repository<TbProblemOrderItem>.Delete(trans, p => p.ProblemOrderCode == model.ProblemOrderCode);
                        //添加明细信息
                        Repository<TbProblemOrderItem>.Insert(trans, items);
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
                if (anyRet.state != ResultType.success.ToString())
                    return anyRet;
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //删除信息
                    var count = Repository<TbProblemOrder>.Delete(trans, p => p.ID == keyValue);
                    //删除明细信息
                    Repository<TbProblemOrderItem>.Delete(trans, p => p.ProblemOrderCode == ((TbProblemOrder)anyRet.data).ProblemOrderCode);
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

        #region 获取数据

        /// <summary>
        /// 获取编辑数据
        /// </summary>
        /// <param name="keyValue">数据Id</param>
        /// <returns></returns>
        public Tuple<object, object> FindEntity(int keyValue)
        {
            var ret = Db.Context.From<TbProblemOrder>()
                .Select(
                       TbProblemOrder._.All
                    , TbUser._.UserName
                    , TbCompany._.CompanyFullName.As("SiteName")
                    , TbSysDictionaryData._.DictionaryText.As("ProcessingStateName"))
                    .AddSelect(Db.Context.From<TbCompany>()
                      .Select(p => p.CompanyFullName)
                      .Where(TbCompany._.CompanyCode == TbProblemOrder._.ProcessFactoryCode), "ProcessFactoryName")
                  .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                  .LeftJoin<TbCompany>((a, c) => a.SiteCode == c.CompanyCode)
                  .LeftJoin<TbSysDictionaryData>((a, c) => a.ProcessingState == c.DictionaryCode)
                  .Where(p => p.ID == keyValue).ToDataTable();
            if (ret == null || ret.Rows.Count == 0)
                return new Tuple<object, object>(null, null);
            //查找明细信息
            //var items = Db.Context.From<TbProblemOrderItem>().Select(
            //    TbProblemOrderItem._.All)
            //.Where(p => p.ProblemOrderCode == ret.Rows[0]["ProblemOrderCode"].ToString())
            //.ToDataTable();
            string sql = @"select poi.*,1 as OrderType from TbProblemOrderItem poi where poi.ProblemOrderCode=@ProblemOrderCode
                           union all
                           select  0 as ID,'' as ProblemOrderCode,wod.ComponentName,wod.LargePattern,wod.MaterialCode,wod.MaterialName,wod.SpecificationModel,wod.MeasurementUnit,wod.MeasurementUnitZl as Weight,wod.ItemUseNum as UseCount,wod.Number as Count,wod.WeightSmallPlan as WeightTotal,wod.DaetailWorkStrat as ProcessingState,wod.PackNumber as PackCount,wod.Manufactor as Factory,wod.HeatNo as BatchNumber,wod.TestReportNo as TestReportCode,wod.ID as WorkOrderDetailId,2 as OrderType from TbWorkOrderDetail wod where wod.ID in(select wod.ID from TbWorkOrderDetail wod where wod.ID not in(select poi.WorkOrderDetailId from TbProblemOrder po
                           left join TbProblemOrderItem poi on po.ProblemOrderCode=poi.ProblemOrderCode
                           where poi.ProblemOrderCode=@ProblemOrderCode) and wod.OrderCode=@OrderCode)";
            var items = Db.Context.FromSql(sql)
               .AddInParameter("@ProblemOrderCode", DbType.String, ret.Rows[0]["ProblemOrderCode"].ToString())
               .AddInParameter("@OrderCode", DbType.String, ret.Rows[0]["OrderCode"].ToString()).ToDataTable();
            return new Tuple<object, object>(ret, items);
        }

        /// <summary>
        /// 获取数据列表(分页)
        /// </summary>
        public PageModel GetDataListForPage(ProblemOrderRequest request)
        {

            #region 模糊搜索条件
            var where = new Where<TbProblemOrder>();
            if (!string.IsNullOrWhiteSpace(request.OrderCode))
            {
                where.And(d => d.OrderCode.Like(request.OrderCode));
            }
            if (!string.IsNullOrWhiteSpace(request.TypeCode))
            {
                where.And(d => d.TypeCode == request.TypeCode);
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
            if (!string.IsNullOrWhiteSpace(request.RevokeStatus))
            {
                where.And(d => d.RevokeStatus == request.RevokeStatus);
            }
            if (!string.IsNullOrWhiteSpace(request.BegTime.ToString()))
            {
                where.And(d => d.DistributionTime >= request.BegTime);
            }
            if (!string.IsNullOrWhiteSpace(request.EndTiem.ToString()))
            {
                where.And(d => d.DistributionTime <= request.EndTiem);
            }
            #endregion

            #region 数据权限

            ////数据权限
            //var authorizaModel = new AuthorizeLogic().CheckAuthoriza(new AuthorizationParameterModel("ProblemOrder"));
            //if (authorizaModel.IsAuthorize)
            //{
            //    if (authorizaModel.Ids.Count > 0 && authorizaModel.UserCodes.Count > 0)
            //        where.And(d => d.InsertUserCode.In(authorizaModel.UserCodes) || d.ID.In(authorizaModel.Ids));
            //    else if (authorizaModel.Ids.Count > 0)
            //        where.Or(d => d.ID.In(authorizaModel.Ids));
            //    else if (authorizaModel.UserCodes.Count > 0)
            //        where.And(d => d.InsertUserCode.In(authorizaModel.UserCodes));
            //}
            if (!string.IsNullOrEmpty(request.ProjectId))
                where.And(p => p.ProjectId == request.ProjectId);
            #endregion

            try
            {
                var sql = Db.Context.From<TbProblemOrder>()
                    .Select(
                      TbProblemOrder._.ID
                    , TbProblemOrder._.Examinestatus
                    , TbProblemOrder._.Examinestatus.As("ExaminestatusNew")
                    , TbProblemOrder._.ProblemOrderCode
                    , TbProblemOrder._.OrderCode
                    , TbProblemOrder._.TypeCode
                    , TbProblemOrder._.TypeName
                    , TbProblemOrder._.UsePart
                    , TbProblemOrder._.RevokeStatus
                    , TbProblemOrder._.DistributionTime
                    , TbProblemOrder._.DistributionAddress
                    , TbProblemOrder._.Total
                    , TbProblemOrder._.OldTotal
                    , TbProblemOrder._.InsertUserCode
                    , TbProblemOrder._.InsertTime
                    , TbProblemOrder._.ProjectId
                    , TbUser._.UserName
                    , TbCompany._.CompanyFullName.As("SiteName")
                    , TbWorkOrder._.ProcessingState.As("OldProcessingState"))
                  .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                  .LeftJoin<TbCompany>((a, c) => a.SiteCode == c.CompanyCode)
                  .LeftJoin<TbWorkOrder>((a, c) => a.OrderCode == c.OrderCode)
                  .Where(where)
                  .OrderByDescending(p => p.ID);
                var data = new PageModel();
                if (request.IsOutPut)
                {
                    data.rows = sql.ToDataTable();
                }
                else {
                    data = sql.ToPageList(request.rows, request.page);
                }
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 判断信息是否可操作
        /// </summary>
        /// <returns></returns>
        public AjaxResult AnyInfo(int keyValue)
        {
            var problemOrder = Repository<TbProblemOrder>.First(p => p.ID == keyValue);
            if (problemOrder == null)
                return AjaxResult.Warning("信息不存在");
            if (problemOrder.Examinestatus != "未发起" && problemOrder.Examinestatus != "已退回")
                return AjaxResult.Warning("信息正在审核中或已审核完成,不能进行此操作");

            return AjaxResult.Success(problemOrder);
        }
        #endregion


        /// <summary>
        /// 获取数据列表(原订单)
        /// </summary>
        public PageModel GetOrderDataList(ProblemOrderRequest request)
        {
            #region 模糊搜索条件

            var where = new Where<TbWorkOrder>();
            if (!string.IsNullOrWhiteSpace(request.keyword))
            {
                where.And(p => p.OrderCode.Like(request.keyword));
            }
            if (!string.IsNullOrEmpty(request.ProcessFactoryCode))
            {
                where.And(p => p.ProcessFactoryCode == request.ProcessFactoryCode);
            }
            if (!request.IsCost)
            {
                where.And(p => p.Examinestatus == "审核完成" && (p.ProcessingState != "Processing" && p.ProcessingState != "Finishing"));
                where.And(TbWorkOrder._.OrderCode
                    .SubQueryNotIn(Db.Context.From<TbProblemOrder>().Select(p => p.OrderCode)));
            }
            else
            {
                where.And(p => p.Examinestatus == "审核完成");
            }
            if (!string.IsNullOrEmpty(request.ProjectId))
                where.And(p => p.ProjectId == request.ProjectId);

            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where.And(p => p.ProcessFactoryCode == request.ProcessFactoryCode);
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);//站点
                where.And(p => p.SiteCode.In(SiteList));
            }
            #endregion
            try
            {
                var data = Db.Context.From<TbWorkOrder>().Select(
                    TbWorkOrder._.OrderCode,
                    TbWorkOrder._.SiteCode,
                    TbWorkOrder._.TypeCode,
                    TbWorkOrder._.TypeName,
                    TbWorkOrder._.SiteCode,
                    TbWorkOrder._.ProcessFactoryCode,
                    TbWorkOrder._.UsePart,
                    TbWorkOrder._.WeightTotal,
                    TbWorkOrder._.ProcessingState,
                    TbWorkOrder._.DistributionTime,
                    TbWorkOrder._.DistributionAdd.As("DistributionAddress"),
                    TbWorkOrder._.ProjectId,
                    TbCompany._.CompanyFullName.As("SiteName"),
                    TbSysDictionaryData._.DictionaryText.As("ProcessingStateName"))
                    .AddSelect(Db.Context.From<TbCompany>()
                      .Select(p => p.CompanyFullName)
                      .Where(TbCompany._.CompanyCode == TbWorkOrder._.ProcessFactoryCode), "ProcessFactoryName")
                    .LeftJoin<TbCompany>((a, c) => a.SiteCode == c.CompanyCode)
                    .LeftJoin<TbSysDictionaryData>((a, c) => a.ProcessingState == c.DictionaryCode)
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
        /// 获取数据列表(原订单明细)
        /// </summary>
        public PageModel GetOrderItemDataList(ProblemOrderRequest request)
        {
            #region 模糊搜索条件

            var where = new Where<TbWorkOrderDetail>();
            if (!string.IsNullOrWhiteSpace(request.keyword))
            {
                where.And(p => p.MaterialCode.Like(request.keyword));
            }
            if (!string.IsNullOrWhiteSpace(request.OrderCode))
            {
                where.And(p => p.OrderCode == request.OrderCode);
            }

            #endregion
            try
            {
                var data = Db.Context.From<TbWorkOrderDetail>().Select(
                    TbWorkOrderDetail._.ID.As("WorkOrderDetailId"),
                    TbWorkOrderDetail._.SpecificationModel,
                    TbWorkOrderDetail._.MaterialCode,
                    TbWorkOrderDetail._.MaterialName,
                    TbWorkOrderDetail._.ComponentName,
                    TbWorkOrderDetail._.LargePattern,
                    TbWorkOrderDetail._.MeasurementUnitZl.As("Weight"),
                    TbWorkOrderDetail._.ItemUseNum.As("UseCount"),
                    TbWorkOrderDetail._.Number.As("Count"),
                    TbWorkOrderDetail._.WeightSmallPlan.As("WeightTotal"),
                    TbWorkOrderDetail._.DaetailWorkStrat.As("ProcessingStateItem"),
                    TbWorkOrderDetail._.PackNumber.As("PackCount"),
                    TbWorkOrderDetail._.Manufactor.As("Factory"),
                    TbWorkOrderDetail._.HeatNo.As("BatchNumber"),
                    TbWorkOrderDetail._.TestReportNo.As("TestReportCode"),
                    TbSysDictionaryData._.DictionaryText.As("ProcessingStateNameItem"))
                    .AddSelect(Db.Context.From<TbSysDictionaryData>()
                      .Select(p => p.DictionaryText)
                      .Where(TbSysDictionaryData._.DictionaryCode == TbWorkOrderDetail._.MeasurementUnit), "MeasurementUnit")
                    .LeftJoin<TbSysDictionaryData>((a, c) => a.DaetailWorkStrat == c.DictionaryCode)
                    .Where(where)
                    .ToPageList(request.rows, request.page);
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
