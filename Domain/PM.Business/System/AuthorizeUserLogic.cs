using Dos.Common;
using Dos.ORM;
using PM.Common;
using PM.Common.Helper;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Business
{
    public class AuthorizeUserLogic
    {

        #region 新增数据

        /// <summary>
        /// 新增数据
        /// </summary>
        public AjaxResult Insert(AuthorizeUserRequest request)
        {
            if (request == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                var model = MapperHelper.Map<AuthorizeUser, TbAuthorization>(request.AuthorizeUser).ToList();
                var count = Repository<TbAuthorization>.Insert(model);
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
        public AjaxResult Update(AuthorizeUserRequest request)
        {
            if (request == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                var modelNew = MapperHelper.Map<AuthorizeUser, TbAuthorization>(request.AuthorizeUser).ToList();
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    if (modelNew.Count > 0)
                    {
                        //删除历史数据
                        Repository<TbAuthorization>.Delete(p => p.MenuCode == request.MenuCode && p.DataID == request.DataID);
                        //添加数据
                        Repository<TbAuthorization>.Insert(trans, modelNew);
                    }
                    trans.Commit();
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

        #region 删除数据

        /// <summary>
        /// 删除数据
        /// </summary>
        public AjaxResult Delete(string menuCode, int dataID)
        {
            try
            {
                var count = Repository<TbAuthorization>.Delete(p => p.MenuCode == menuCode && p.DataID == dataID);
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
        /// <param name="menuCode">菜单编号</param>
        /// <param name="dataID">数据Id</param>
        /// <returns></returns>
        public AuthorizeUserResponse FindEntity(string menuCode, int dataID)
        {
            var ret = new AuthorizeUserResponse();
            var dt = Db.Context.From<TbAuthorization>()
               .Select(
                       TbAuthorization._.All
                       ,TbUser._.UserName)
                       .LeftJoin<TbUser>((a, c) => a.BSQR == c.UserCode)
                       .Where(p => p.MenuCode == menuCode && p.DataID == dataID).ToDataTable();
            var list = ModelConvertHelper<AuthorizeUser>.ToList(dt);
            if (list.Count > 0)
            {
                ret.AuthorizeUser = list;
                ret.DataID = list[0].DataID;
                ret.MenuCode = list[0].MenuCode;
            }
            return ret;
        }

        #endregion
    }
}
