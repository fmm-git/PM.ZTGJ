using Dos.ORM;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataAccess.Production;
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
    /// 逻辑处理层
    /// 人员工日消耗
    /// </summary>
    public class PersonnelWorkDayConsumeLogic
    {
        //配送装车数据访问处理层类
        public readonly PersonnelWorkDayConsumeDA _pwdcDA = new PersonnelWorkDayConsumeDA();

        #region 查询数据

        /// <summary>
        /// 首页查询
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public PageModel GetAllOrBySearch(FPiCiXQPlan entity)
        {
            //组装查询语句
            #region 搜索条件

            var where = new Where<TbPersonnelWorkDayConsume>();
            if (!string.IsNullOrWhiteSpace(entity.ProcessFactoryCode))
            {
                where.And(p => p.ProcessFactoryCode == entity.ProcessFactoryCode);
            }
            if (!string.IsNullOrWhiteSpace(entity.OrderCode))
            {
                where.And(p => p.OrderCode.Like(entity.OrderCode));
            }
            if (!string.IsNullOrEmpty(entity.ProjectId)) 
            {
                where.And(p => p.ProjectId == entity.ProjectId);
            }
            #endregion

            //数据权限
            //var authorizaModel = new AuthorizeLogic().CheckAuthoriza(new AuthorizationParameterModel("PersonnelWorkDayConsume"));
            //if (authorizaModel.IsAuthorize)
            //{
            //    if (authorizaModel.Ids.Count > 0 && authorizaModel.UserCodes.Count > 0)
            //        where.And(d => d.InsertUserCode.In(authorizaModel.UserCodes) || d.ID.In(authorizaModel.Ids));
            //    else if (authorizaModel.Ids.Count > 0)
            //        where.Or(d => d.ID.In(authorizaModel.Ids));
            //    else if (authorizaModel.UserCodes.Count > 0)
            //        where.And(d => d.InsertUserCode.In(authorizaModel.UserCodes));
            //}

            try
            {
                var ret = Db.Context.From<TbPersonnelWorkDayConsume>()
              .Select(
                      TbPersonnelWorkDayConsume._.All
                      , TbCompany._.CompanyFullName.As("SiteName")
                      , TbUser._.UserName)
                    .LeftJoin<TbCompany>((a, c) => a.SiteCode == c.CompanyCode)
                    .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                    .Where(TbCompany._.CompanyCode == TbPersonnelWorkDayConsume._.ProcessFactoryCode), "ProcessFactoryName")
                    .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                      .Where(where).OrderBy(d => d.ID).ToPageList(entity.rows, entity.page);
                return ret;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 以ID查询人员工日消耗
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public Tuple<DataTable, DataTable> GetFormJson(int keyValue)
        {
            var ret = Db.Context.From<TbPersonnelWorkDayConsume>()
            .Select(
                    TbPersonnelWorkDayConsume._.All
                    , TbCompany._.CompanyFullName.As("SiteName")
                    , TbUser._.UserName)
                  .LeftJoin<TbCompany>((a, c) => a.SiteCode == c.CompanyCode)
                  .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                  .Where(TbCompany._.CompanyCode == TbPersonnelWorkDayConsume._.ProcessFactoryCode), "ProcessFactoryName")
                  .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                    .Where(p => p.ID == keyValue).ToDataTable();
            //查找明细信息
            var items = Db.Context.From<TbPersonnelWorkDayConsumeItem>().Where(p => p.ConsumeCode ==Convert.ToString(ret.Rows[0]["ConsumeCode"])).ToDataTable();
            return new Tuple<DataTable, DataTable>(ret, items);
        }

        /// <summary>
        /// 加工订单弹窗
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageModel GetJGDDGridJson(WorkOrderRequest request)
        {
            var where = new Where<TbWorkOrder>();
            where.And(d => d.ProcessingState == "Processing");
            if (!string.IsNullOrEmpty(request.ProjectId))
            {
                where.And(d => d.ProjectId == request.ProjectId);
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where.And(d => d.ProcessFactoryCode == request.ProcessFactoryCode);
            }
            if (!string.IsNullOrWhiteSpace(request.keyword))
            {
                where.And<TbCompany>((a, c) => a.TypeName.Like(request.keyword));
            }

            try
            {
                var data = Db.Context.From<TbWorkOrder>()
                    .Select(
                      TbWorkOrder._.All
                    , TbCompany._.CompanyFullName.As("ProcessFactoryName"))
                  .LeftJoin<TbCompany>((a, b) => a.ProcessFactoryCode == b.CompanyCode)
                  .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                  .Where(TbCompany._.CompanyCode == TbWorkOrder._.SiteCode), "SiteName")
                  .Where(where)
                  .ToPageList(request.rows, request.page);
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 新增明细弹窗
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public PageModel GetOrderItemGridJson(WorkOrderRequest request,string keyValue,string OrderCode)
        {
            var where = new Where<TbWorkOrderDetail>();
            where.And(d => d.OrderCode == OrderCode);
            if (!string.IsNullOrWhiteSpace(keyValue))
            {
                where.And(d => d.ComponentName.Like(keyValue) || d.MaterialName.Like(keyValue));
            }
            try
            {
                var data = Db.Context.From<TbWorkOrderDetail>().Where(where).ToPageList(request.rows, request.page);
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region （新增、编辑）数据

        /// <summary>
        /// 新增数据
        /// </summary>
        public AjaxResult Insert(TbPersonnelWorkDayConsume model, List<TbPersonnelWorkDayConsumeItem> items)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            model.Examinestatus = "未发起";
            model.InsertUserCode = OperatorProvider.Provider.CurrentUser.UserCode;
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //添加信息
                    Repository<TbPersonnelWorkDayConsume>.Insert(trans, model);
                    //添加明细信息
                    Repository<TbPersonnelWorkDayConsumeItem>.Insert(trans, items);
                    trans.Commit();
                    return AjaxResult.Success();
                }
            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 修改数据
        /// </summary>
        public AjaxResult Update(TbPersonnelWorkDayConsume model, List<TbPersonnelWorkDayConsumeItem> items)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            var anyRet = AnyInfo(model.ID);
            if (anyRet.state.ToString() != ResultType.success.ToString())
                return anyRet;
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())//使用事务
                {
                    //修改信息
                    Repository<TbPersonnelWorkDayConsume>.Update(trans, model, p => p.ID == model.ID);
                    if (items.Count > 0)
                    {
                        //删除历史明细信息
                        Repository<TbPersonnelWorkDayConsumeItem>.Delete(p => p.ConsumeCode == model.ConsumeCode);
                        //添加明细信息
                        Repository<TbPersonnelWorkDayConsumeItem>.Insert(trans, items);
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

        /// <summary>
        /// 判断信息是否可操作
        /// </summary>
        /// <returns></returns>
        public AjaxResult AnyInfo(int keyValue)
        {
            var personnelWorkDay = Repository<TbPersonnelWorkDayConsume>.First(p => p.ID == keyValue);
            if (personnelWorkDay == null)
                return AjaxResult.Warning("信息不存在");
            if (personnelWorkDay.Examinestatus != "未发起" && personnelWorkDay.Examinestatus != "已退回")
                return AjaxResult.Warning("信息正在审核中或已审核完成,不能进行此操作");

            return AjaxResult.Success(personnelWorkDay);
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
                    //删除信息
                    var count = Repository<TbPersonnelWorkDayConsume>.Delete(trans, p => p.ID == keyValue);
                    //删除明细信息
                    Repository<TbPersonnelWorkDayConsumeItem>.Delete(trans, p => p.ConsumeCode == ((TbPersonnelWorkDayConsume)anyRet.data).ConsumeCode);
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
    }
}
