using Dos.ORM;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using PM.Domain.WebBase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Business.RawMaterial
{
    /// <summary>
    /// 仓库管理
    /// </summary>
    public class StorageLogic
    {

        #region 新增数据

        /// <summary>
        /// 新增数据
        /// </summary>
        public AjaxResult Insert(TbStorage model)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            model.StorageCode = CreateCode.GetTableMaxCode("CK", "StorageCode", "TbStorage");
            model.InsertUserCode = OperatorProvider.Provider.CurrentUser.UserCode;
            try
            {
                var count = Repository<TbStorage>.Insert(model);
                if (count > 0)
                {
                    return AjaxResult.Success();
                }
                return AjaxResult.Error("操作失败");
            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.ToString());
            }
        }

        #endregion

        #region 修改数据

        /// <summary>
        /// 修改数据
        /// </summary>
        public AjaxResult Update(TbStorage model)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            //验证是否可操作
            var anyRet = AnyInfo(model.ID);
            if (anyRet.state != ResultType.success.ToString())
                return anyRet;
            try
            {
                var count = Repository<TbStorage>.Update(model, p => p.ID == model.ID);
                if (count > 0)
                    return AjaxResult.Success();
                return AjaxResult.Error("操作失败");
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

            var anyRet = AnyInfo(keyValue);
            if (anyRet.state != ResultType.success.ToString())
                return anyRet;
            try
            {
                var count = Repository<TbStorage>.Delete(p => p.ID == keyValue);
                if (count > 0)
                    return AjaxResult.Success();
                return AjaxResult.Error("操作失败");
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
        public DataTable FindEntity(int keyValue)
        {
            var ret = Db.Context.From<TbStorage>()
                   .Select(
                       TbStorage._.All
                       , TbCompany._.CompanyFullName.As("ProcessFactoryName")
                       , TbUser._.UserName.As("InsertUserName")
                       , TbSysDictionaryData._.DictionaryText.As("StorageAttributeText"))
                    .AddSelect(Db.Context.From<TbUser>()
                    .Select(p => p.UserName)
                    .Where(TbUser._.UserCode == TbStorage._.UserCode), "UserName")
                    .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                    .LeftJoin<TbSysDictionaryData>((a, c) => a.StorageAttribute == c.DictionaryCode)
                    .LeftJoin<TbCompany>((a, c) => a.ProcessFactoryCode == c.CompanyCode)
                    .Where(p => p.ID == keyValue).ToDataTable();
            return ret;
        }

        /// <summary>
        /// 获取数据列表(分页)
        /// </summary>
        public PageModel GetDataListForPage(StorageRequest request)
        {

            #region 模糊搜索条件
            var where = new Where<TbStorage>();
            if (!string.IsNullOrWhiteSpace(request.StorageName))
            {
                where.And(d => d.StorageName.Like(request.StorageName));
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where.And(d => d.ProcessFactoryCode==request.ProcessFactoryCode);
            }
            #endregion

            try
            {
                var data = Db.Context.From<TbStorage>()
                    .Select(
                      TbStorage._.ID
                    , TbStorage._.StorageName
                    , TbStorage._.ProcessFactoryCode
                    , TbStorage._.StorageAttribute
                    , TbStorage._.AreaCode
                    , TbStorage._.StorageAdd
                    , TbStorage._.Tel
                    , TbStorage._.InsertUserCode
                    , TbStorage._.InsertTime
                    , TbUser._.UserName.As("InsertUserName")
                    , TbCompany._.CompanyFullName.As("ProcessFactoryName")
                    , TbSysDictionaryData._.DictionaryText.As("StorageAttributeText"))
                    .AddSelect(Db.Context.From<TbUser>()
                    .Select(p => p.UserName)
                    .Where(TbUser._.UserCode == TbStorage._.UserCode), "UserName")
                    .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                    .LeftJoin<TbSysDictionaryData>((a, c) => a.StorageAttribute == c.DictionaryCode)
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
        /// 判断信息是否可操作
        /// </summary>
        /// <returns></returns>
        public AjaxResult AnyInfo(int keyValue)
        {
            var storage = Repository<TbStorage>.First(p => p.ID == keyValue);
            if (storage == null)
                return AjaxResult.Warning("信息不存在");

            return AjaxResult.Success(storage);
        }
        #endregion

        #region 导出
        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public DataTable GetExportList(StorageRequest request)
        {

            #region 模糊搜索条件
            var where = new Where<TbStorage>();
            if (!string.IsNullOrWhiteSpace(request.StorageName))
            {
                where.And(d => d.StorageName.Like(request.StorageName));
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where.And(d => d.ProcessFactoryCode == request.ProcessFactoryCode);
            }
            #endregion

            try
            {
                var data = Db.Context.From<TbStorage>()
                    .Select(
                      TbStorage._.ID
                    , TbStorage._.StorageName
                    , TbStorage._.ProcessFactoryCode
                    , TbStorage._.StorageAttribute
                    , TbStorage._.AreaCode
                    , TbStorage._.StorageAdd
                    , TbStorage._.Tel
                    , TbStorage._.InsertUserCode
                    , TbStorage._.InsertTime
                    , TbUser._.UserName.As("InsertUserName")
                    , TbCompany._.CompanyFullName.As("ProcessFactoryName")
                    , TbSysDictionaryData._.DictionaryText.As("StorageAttributeText"))
                    .AddSelect(Db.Context.From<TbUser>()
                    .Select(p => p.UserName)
                    .Where(TbUser._.UserCode == TbStorage._.UserCode), "UserName")
                    .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                    .LeftJoin<TbSysDictionaryData>((a, c) => a.StorageAttribute == c.DictionaryCode)
                    .LeftJoin<TbCompany>((a, c) => a.ProcessFactoryCode == c.CompanyCode)
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
