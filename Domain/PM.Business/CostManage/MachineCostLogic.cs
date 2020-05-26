using Dos.ORM;
using PM.Business.Production;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using PM.DataEntity.CostManage.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Business.CostManage
{
    /// <summary>
    /// 机械费用核算
    /// </summary>
    public class MachineCostLogic
    {

        private readonly TbWorkOrderLogic _workOrderLogic = new TbWorkOrderLogic();

        #region 新增数据

        /// <summary>
        /// 新增数据
        /// </summary>
        public AjaxResult Insert(TbMachineCost model, List<TbMachineCostItem> items)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            model.InsertUserCode = OperatorProvider.Provider.CurrentUser.UserCode;
            model.Examinestatus = "未发起";

            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //添加信息
                    Repository<TbMachineCost>.Insert(trans, model);
                    //添加明细信息
                    Repository<TbMachineCostItem>.Insert(trans, items);
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
        public AjaxResult Update(TbMachineCost model, List<TbMachineCostItem> items)
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
                    Repository<TbMachineCost>.Update(trans, model, p => p.ID == model.ID);
                    if (items.Count > 0)
                    {
                        //删除历史明细信息
                        Repository<TbMachineCostItem>.Delete(trans, p => p.CheckCode == model.CheckCode);
                        //添加明细信息
                        Repository<TbMachineCostItem>.Insert(trans, items);
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
                    var count = Repository<TbMachineCost>.Delete(trans, p => p.ID == keyValue);
                    //删除明细信息
                    Repository<TbMachineCostItem>.Delete(trans, p => p.CheckCode == ((TbMachineCost)anyRet.data).CheckCode);
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
            var ret = Db.Context.From<TbMachineCost>()
                .Select(
                       TbMachineCost._.All
                    , TbUser._.UserName
                    , TbCompany._.CompanyFullName.As("ProcessFactoryName"))
                    .AddSelect(Db.Context.From<TbCompany>()
                      .Select(p => p.CompanyFullName)
                      .Where(TbCompany._.CompanyCode == TbMachineCost._.SiteCode), "SiteName")
                  .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                  .LeftJoin<TbCompany>((a, c) => a.ProcessFactoryCode == c.CompanyCode)
                  .Where(p => p.ID == keyValue).ToDataTable();
            if (ret == null || ret.Rows.Count == 0)
                return new Tuple<object, object>(null, null);
            //查找明细信息
            var items = Db.Context.From<TbMachineCostItem>().Select(
                TbMachineCostItem._.All)
            .Where(p => p.CheckCode == ret.Rows[0]["CheckCode"])
            .ToDataTable();
            return new Tuple<object, object>(ret, items);
        }

        /// <summary>
        /// 获取数据列表(分页)
        /// </summary>
        public PageModel GetDataListForPage(MachineCostRequest request)
        {

            #region 模糊搜索条件
            var where = new Where<TbMachineCost>();
            if (!string.IsNullOrWhiteSpace(request.CheckCode))
            {
                where.And(d => d.CheckCode == request.CheckCode);
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where.And(d => d.ProcessFactoryCode == request.ProcessFactoryCode);
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);
                where.And(p => p.SiteCode.In(SiteList));
            }
            #endregion

            #region 数据权限

            //数据权限
            var authorizaModel = new AuthorizeLogic().CheckAuthoriza(new AuthorizationParameterModel("MachineCost"));
            if (authorizaModel.IsAuthorize)
            {
                if (authorizaModel.Ids.Count > 0 && authorizaModel.UserCodes.Count > 0)
                    where.And(d => d.InsertUserCode.In(authorizaModel.UserCodes) || d.ID.In(authorizaModel.Ids));
                else if (authorizaModel.Ids.Count > 0)
                    where.Or(d => d.ID.In(authorizaModel.Ids));
                else if (authorizaModel.UserCodes.Count > 0)
                    where.And(d => d.InsertUserCode.In(authorizaModel.UserCodes));
            }
            if (!string.IsNullOrEmpty(request.ProjectId))
                where.And(p => p.ProjectId == request.ProjectId);
            #endregion

            try
            {
                var data = Db.Context.From<TbMachineCost>()
                    .Select(
                      TbMachineCost._.ID
                    , TbMachineCost._.Examinestatus
                    , TbMachineCost._.CheckCode
                    , TbMachineCost._.OrderCode
                    , TbMachineCost._.TypeCode
                    , TbMachineCost._.TypeName
                    , TbMachineCost._.UsePart
                    , TbMachineCost._.TotalAmount
                    , TbMachineCost._.InsertUserCode
                    , TbMachineCost._.InsertTime
                    , TbMachineCost._.ProjectId
                    , TbUser._.UserName
                    , TbCompany._.CompanyFullName.As("ProcessFactoryName"))
                    .AddSelect(Db.Context.From<TbCompany>()
                      .Select(p => p.CompanyFullName)
                      .Where(TbCompany._.CompanyCode == TbMachineCost._.SiteCode), "SiteName")
                  .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                  .LeftJoin<TbCompany>((a, c) => a.ProcessFactoryCode == c.CompanyCode)
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
        /// 获取数据列表(明细)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public DataTable GetItemGridJson(MachineCostRequest request)
        {
            var data = Db.Context.From<TbMachineCostItem>().Select(
               TbMachineCostItem._.All)
           .Where(p => p.CheckCode == request.CheckCode)
           .ToDataTable();
            return data;
        }

        /// <summary>
        /// 判断信息是否可操作
        /// </summary>
        /// <returns></returns>
        public AjaxResult AnyInfo(int keyValue)
        {
            var machineCost = Repository<TbMachineCost>.First(p => p.ID == keyValue);
            if (machineCost == null)
                return AjaxResult.Warning("信息不存在");
            if (machineCost.Examinestatus != "未发起" && machineCost.Examinestatus != "已退回")
                return AjaxResult.Warning("信息正在审核中或已审核完成,不能进行此操作");

            return AjaxResult.Success(machineCost);
        }
        #endregion


        /// <summary>
        /// 获取数据列表(设备信息)
        /// </summary>
        public PageModel GetEquipmentRegisterList(MachineCostRequest request)
        {

            #region 模糊搜索条件

            var where = new Where<TbEquipmentRegister>();
            if (!string.IsNullOrWhiteSpace(request.keyword))
            {
                where.And(d => d.EquimentName.Like(request.CheckCode));
            }
            where.And(d => d.TypeCode == "09");
            #endregion
            try
            {
                var data = Db.ContextGJ.From<TbEquipmentRegister>()
                    .Select(
                      TbEquipmentRegister._.EquimentCode
                    , TbEquipmentRegister._.EquimentName
                    , TbEquipmentRegister._.ManageLevel
                    , TbEquipmentRegister._.EquipmentCategory
                    , TbEquipmentRegister._.IsTzEquiment
                    , TbEquipmentRegister._.MeasurementUnit
                    , EquipmentType._.Type_Name)
                  .LeftJoin<EquipmentType>((a, c) => a.TypeCode == c.TypeCode)
                  .Where(where)
                  .OrderByDescending(p => p.id)
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
