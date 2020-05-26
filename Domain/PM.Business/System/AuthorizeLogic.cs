using PM.Common.DataCache;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using PM.Common;

namespace PM.Business
{
    /// <summary>
    /// 获取权限菜单数据
    /// </summary>
    public class AuthorizeLogic
    {
        public bool ActionValidate(string roleId, string moduleId, string action)
        {
            var authorizeurldata = new List<AuthorizeActionModel>();
            var cachedata = CacheFactory.Cache().GetCache<List<AuthorizeActionModel>>("authorizeurldata_" + roleId);
            if (cachedata == null)
            {
                //var moduledata = moduleApp.GetList();
                //var buttondata = moduleButtonApp.GetList();
                //var authorizedata = service.IQueryable(t => t.F_ObjectId == roleId).ToList();
                //foreach (var item in authorizedata)
                //{
                //    if (item.F_ItemType == 1)
                //    {
                //        ModuleEntity moduleEntity = moduledata.Find(t => t.F_Id == item.F_ItemId);
                //        authorizeurldata.Add(new AuthorizeActionModel { F_Id = moduleEntity.F_Id, F_UrlAddress = moduleEntity.F_UrlAddress });
                //    }
                //    else if (item.F_ItemType == 2)
                //    {
                //        ModuleButtonEntity moduleButtonEntity = buttondata.Find(t => t.F_Id == item.F_ItemId);
                //        authorizeurldata.Add(new AuthorizeActionModel { F_Id = moduleButtonEntity.F_ModuleId, F_UrlAddress = moduleButtonEntity.F_UrlAddress });
                //    }
                //}
                CacheFactory.Cache().WriteCache(authorizeurldata, "authorizeurldata_" + roleId, DateTime.Now.AddMinutes(5));
            }
            else
            {
                authorizeurldata = cachedata;
            }
            authorizeurldata = authorizeurldata.FindAll(t => t.F_Id.Equals(moduleId));
            foreach (var item in authorizeurldata)
            {
                if (!string.IsNullOrEmpty(item.F_UrlAddress))
                {
                    string[] url = item.F_UrlAddress.Split('?');
                    if (item.F_Id == moduleId && url[0] == action)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 数据权限筛选
        /// </summary>
        /// <returns></returns>
        public AuthorizationCodeIdModel CheckAuthoriza(AuthorizationParameterModel par)
        {
            var ret = new AuthorizationCodeIdModel();
            if (string.IsNullOrEmpty(par.UserCode))
            par.UserCode = OperatorProvider.Provider.CurrentUser.UserCode;//当前登录人code
            if (string.IsNullOrEmpty(par.UserCode) || string.IsNullOrEmpty(par.FormCode))
                return ret;
            try
            {
                //判断菜单是否开启数据权限
                var AnySysMenu = Repository<TbSysMenu>.Any(p => p.MenuCode == par.FormCode && p.DataAuthority == "0");
                if (!AnySysMenu)
                    return ret;
                //判断团队数据权限
                var sql = "select * from [dbo].[fun_PermissionsUser](@FormCode,@UserCode)";
                ret.UserCodes = Db.Context.FromSql(sql)
                    .AddInParameter("@FormCode", DbType.String, par.FormCode)
                    .AddInParameter("@UserCode", DbType.String, par.UserCode)
                    .ToList<string>();
                //判断单条数据权限
                sql = "select * from [dbo].[fun_PermissionsDataID](@FormCode,@UserCode)";
                ret.Ids = Db.Context.FromSql(sql)
                    .AddInParameter("@FormCode", DbType.String, par.FormCode)
                    .AddInParameter("@UserCode", DbType.String, par.UserCode)
                    .ToList<int>();
                ret.IsAuthorize = true;
            }
            catch (Exception e)
            {
                return ret;
            }
            return ret;
        }
    }
}
