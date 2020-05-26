using Dos.Common;
using Dos.ORM;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace PM.Business
{
    public class TbRoleMenuLogic
    {
        #region 新增数据

        /// <summary>
        /// 新增数据(多条)
        /// </summary>
        public AjaxResult Insert(List<TbRoleMenu> userList, string RoleCode)
        {
            if (userList == null)
                 return AjaxResult.Warning("参数错误");
           
            try
            {
                //删除角色菜单
                var Rolecount = Repository<TbRoleMenu>.Delete(t => t.RoleCode == RoleCode);
                if (userList.Count>0)
                {
                    var count = Repository<TbRoleMenu>.Insert(userList);
                }
                //var model = MapperHelper.Map<TbRoleMenu, TbRoleMenu>(userList);
                //var count = Repository<TbRoleMenu>.Insert(userList);
                return AjaxResult.Success();
            }

            catch (Exception)
            {
                return AjaxResult.Error("操作失败");
            }
            //}

        }

        #endregion

        #region 查询
        public List<TbRoleMenu> GetList(string RoleCode)
        {
            #region 模糊搜索条件

            var where = new Where<TbRoleMenu>();
            if (!string.IsNullOrWhiteSpace(RoleCode))
            {
                where.And(d => d.RoleCode==RoleCode);
            }

            #endregion
            try
            {
                return Repository<TbRoleMenu>.Query(where, d => d.ID, "asc").ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
    }
}
