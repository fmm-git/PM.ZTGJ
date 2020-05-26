using Dos.ORM;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Business.RawMaterial
{
    /// <summary>
    /// 供应商管理
    /// </summary>
    public class SupplierLogic
    {

        #region 新增数据

        /// <summary>
        /// 新增数据
        /// </summary>
        public AjaxResult Insert(TbSupplier model)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            model.InsertUserCode = OperatorProvider.Provider.CurrentUser.UserCode;
            try
            {
                var count = Repository<TbSupplier>.Insert(model);
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
        public AjaxResult Update(TbSupplier model)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            //验证是否可操作
            var anyRet = AnyInfo(model.ID);
            if (anyRet.state != ResultType.success.ToString())
                return anyRet;
            try
            {
                var count = Repository<TbSupplier>.Update(model, p => p.ID == model.ID);
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
                var count = Repository<TbSupplier>.Delete(p => p.ID == keyValue);
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
            var ret = Db.Context.From<TbSupplier>()
                   .Select(
                       TbSupplier._.All
                       , TbUser._.UserName.As("InsertUserName"))
                    .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                    .Where(p => p.ID == keyValue).ToDataTable();
            return ret;
        }

        /// <summary>
        /// 获取数据列表(分页)
        /// </summary>
        public PageModel GetDataListForPage(SupplierRequest request)
        {

            #region 模糊搜索条件
            var where = new Where<TbSupplier>();
            if (!string.IsNullOrWhiteSpace(request.SupplierName))
            {
                where.And(d => d.SupplierName.Like(request.SupplierName));
            }
            #endregion

            try
            {
                var data = Db.Context.From<TbSupplier>()
                    .Select(
                      TbSupplier._.All
                    , TbUser._.UserName.As("InsertUserName"))
                    .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
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
            var supplier = Repository<TbSupplier>.First(p => p.ID == keyValue);
            if (supplier == null)
                return AjaxResult.Warning("信息不存在");

            return AjaxResult.Success(supplier);
        }

        #endregion
    }
}
