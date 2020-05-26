using Dos.ORM;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using PM.DataEntity.Safe.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Business.Safe
{
    /// <summary>
    /// 安全检查
    /// </summary>
    public class SafeCheckLogic
    {

        #region 新增数据

        /// <summary>
        /// 新增数据
        /// </summary>
        public AjaxResult Insert(TbSafeCheck model, List<TbSafeCheckItem> items,bool isApi=false)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            model.InsertUserCode = model.InsertUserCode;
            model.Examinestatus = "未发起";

            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //添加信息
                    Repository<TbSafeCheck>.Insert(trans, model, isApi);
                    //添加明细信息
                    Repository<TbSafeCheckItem>.Insert(trans, items, isApi);
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
        public AjaxResult Update(TbSafeCheck model, List<TbSafeCheckItem> items, bool isApi = false)
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
                    Repository<TbSafeCheck>.Update(trans, model, p => p.ID == model.ID, isApi);
                    if (items.Count > 0)
                    {
                        //删除历史明细信息
                        Repository<TbSafeCheckItem>.Delete(p => p.SafeCheckCode == model.SafeCheckCode, isApi);
                        //添加明细信息
                        Repository<TbSafeCheckItem>.Insert(trans, items, isApi);
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
                    var count = Repository<TbSafeCheck>.Delete(trans, p => p.ID == keyValue);
                    //删除明细信息
                    Repository<TbSafeCheckItem>.Delete(trans, p => p.SafeCheckCode == ((TbSafeCheck)anyRet.data).SafeCheckCode);
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
            var ret = Db.Context.From<TbSafeCheck>()
                .Select(
                       TbSafeCheck._.All
                    , TbUser._.UserName
                    , TbCompany._.CompanyFullName.As("ProcessFactoryName"))
                    .AddSelect(Db.Context.From<TbUser>()
                      .Select(p => p.UserName)
                      .Where(TbUser._.UserCode == TbSafeCheck._.CheckUser), "CheckUserName")
                  .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                  .LeftJoin<TbCompany>((a, c) => a.ProcessFactoryCode == c.CompanyCode)
                  .Where(p => p.ID == keyValue).ToDataTable();
            if (ret == null || ret.Rows.Count == 0)
                return new Tuple<object, object>(null, null);
            //查找明细信息
            var items = Db.Context.From<TbSafeCheckItem>().Select(
                TbSafeCheckItem._.All)
            .Where(p => p.SafeCheckCode == ret.Rows[0]["SafeCheckCode"])
            .ToDataTable();
            return new Tuple<object, object>(ret, items);
        }

        /// <summary>
        /// 获取数据列表(分页)
        /// </summary>
        public PageModel GetDataListForPage(SafeCheckRequest request)
        {

            #region 模糊搜索条件
            var where = new Where<TbSafeCheck>();
            if (!string.IsNullOrWhiteSpace(request.CheckType))
            {
                where.And(d => d.CheckType == request.CheckType);
            }
            if (!string.IsNullOrWhiteSpace(request.SafeCheckCode))
            {
                where.And(d => d.SafeCheckCode.Like(request.SafeCheckCode));
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where.And(d => d.ProcessFactoryCode == request.ProcessFactoryCode);
            }
            if (request.CheckTimeS.HasValue)
            {
                where.And(d => d.CheckTime >= request.CheckTimeS);
            }
            if (request.CheckTimeE.HasValue)
            {
                where.And(d => d.CheckTime <= request.CheckTimeE);
            }
            #endregion

            //#region 数据权限

            ////数据权限
            //var authorizaModel = new AuthorizeLogic().CheckAuthoriza(new AuthorizationParameterModel() { FormCode = "SafeCheck",UserCode=request.UserCode });
            //if (authorizaModel.IsAuthorize)
            //{
            //    if (authorizaModel.Ids.Count > 0 && authorizaModel.UserCodes.Count > 0)
            //        where.And(d => d.InsertUserCode.In(authorizaModel.UserCodes) || d.ID.In(authorizaModel.Ids));
            //    else if (authorizaModel.Ids.Count > 0)
            //        where.Or(d => d.ID.In(authorizaModel.Ids));
            //    else if (authorizaModel.UserCodes.Count > 0)
            //        where.And(d => d.InsertUserCode.In(authorizaModel.UserCodes));
            //}
            //if (!string.IsNullOrEmpty(request.ProjectId))
            //    where.And(p => p.ProjectId == request.ProjectId);
            //#endregion

            try
            {
                var data = Db.Context.From<TbSafeCheck>()
                    .Select(
                      TbSafeCheck._.ID
                    , TbSafeCheck._.Examinestatus
                    , TbSafeCheck._.SafeCheckCode
                    , TbSafeCheck._.PartInUsers
                    , TbSafeCheck._.CheckTime
                    , TbSafeCheck._.Remark
                    , TbSafeCheck._.InsertUserCode
                    , TbSafeCheck._.InsertTime
                    , TbUser._.UserName
                    , TbCompany._.CompanyFullName.As("ProcessFactoryName")
                    , TbSysDictionaryData._.DictionaryText.As("CheckTypeName"))
                    .AddSelect(Db.Context.From<TbUser>()
                      .Select(p => p.UserName)
                      .Where(TbUser._.UserCode == TbSafeCheck._.CheckUser), "CheckUserName")
                  .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                  .LeftJoin<TbCompany>((a, c) => a.ProcessFactoryCode == c.CompanyCode)
                  .LeftJoin<TbSysDictionaryData>((a, c) => a.CheckType == c.DictionaryCode)
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
        public DataTable GetSafeCheckItemDataList(SafeCheckRequest request)
        {
            var where = new Where<TbSysDictionaryData>();
            if (!string.IsNullOrWhiteSpace(request.keyword))
            {
                where.And(d => d.DictionaryText.Like(request.keyword) || d.DictionaryOrder.Like(request.keyword));
            }
            where.And(p => p.FDictionaryCode == "SafeCheckItem");
            try
            {
                var data = Db.Context.From<TbSysDictionaryData>()
                     .Select(
                       TbSysDictionaryData._.id
                     , TbSysDictionaryData._.DictionaryCode.As("CheckItem")
                     , TbSysDictionaryData._.DictionaryText.As("Content"))
                     .Where(where)
                     .ToDataTable();
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
            var safeCheck = Repository<TbSafeCheck>.First(p => p.ID == keyValue);
            if (safeCheck == null)
                return AjaxResult.Warning("信息不存在");
            if (safeCheck.Examinestatus != "未发起" && safeCheck.Examinestatus != "已退回")
                return AjaxResult.Warning("信息正在审核中或已审核完成,不能进行此操作");

            return AjaxResult.Success(safeCheck);
        }
        #endregion

        #region 导出
        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public DataTable GetExportList(SafeCheckRequest request)
        {

            #region 模糊搜索条件

            var where = new Where<TbSafeCheck>();
            if (!string.IsNullOrWhiteSpace(request.CheckType))
            {
                where.And(d => d.CheckType == request.CheckType);
            }
            if (!string.IsNullOrWhiteSpace(request.SafeCheckCode))
            {
                where.And(d => d.SafeCheckCode.Like(request.SafeCheckCode));
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where.And(d => d.ProcessFactoryCode == request.ProcessFactoryCode);
            }
            if (request.CheckTimeS.HasValue)
            {
                where.And(d => d.CheckTime >= request.CheckTimeS);
            }
            if (request.CheckTimeE.HasValue)
            {
                where.And(d => d.CheckTime <= request.CheckTimeE);
            }

            #endregion

            try
            {
                var data = Db.Context.From<TbSafeCheck>()
                    .Select(
                      TbSafeCheck._.ID
                    , TbSafeCheck._.Examinestatus
                    , TbSafeCheck._.SafeCheckCode
                    , TbSafeCheck._.PartInUsers
                    ,TbSafeCheck._.CheckTime
                    , TbSafeCheck._.Remark
                    , TbSafeCheck._.InsertUserCode
                    , TbSafeCheck._.InsertTime
                    , TbUser._.UserName
                    , TbCompany._.CompanyFullName.As("ProcessFactoryName")
                    , TbSysDictionaryData._.DictionaryText.As("CheckTypeName"))
                    .AddSelect(Db.Context.From<TbUser>()
                      .Select(p => p.UserName)
                      .Where(TbUser._.UserCode == TbSafeCheck._.CheckUser), "CheckUserName")
                  .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                  .LeftJoin<TbCompany>((a, c) => a.ProcessFactoryCode == c.CompanyCode)
                  .LeftJoin<TbSysDictionaryData>((a, c) => a.CheckType == c.DictionaryCode)
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
