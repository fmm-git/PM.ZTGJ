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

namespace PM.Business
{
    public class TbPositionMenuLogic
    {
        #region 新增数据

        /// <summary>
        /// 新增数据(多条)
        /// </summary>
        public AjaxResult Insert(List<TbPositionMenu> userList, string PositionCode)
        {
            if (userList == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                //删除角色菜单
                var Rolecount = Repository<TbPositionMenu>.Delete(t => t.PositionCode == PositionCode);
                if (userList.Count>0)
                {
                    var count = Repository<TbPositionMenu>.Insert(userList);
                }
                //var model = MapperHelper.Map<TbPositionMenu, TbPositionMenu>(userList);
                
                return AjaxResult.Success();
               
            }

            catch (Exception)
            {
                return AjaxResult.Error();
            }

        }

        #endregion

        #region 查询
        public List<TbPositionMenu> GetList(string PositionCode)
        {
            #region 模糊搜索条件

            var where = new Where<TbPositionMenu>();
            if (!string.IsNullOrWhiteSpace(PositionCode))
            {
                where.And(d => d.PositionCode == PositionCode);
            }

            #endregion
            try
            {
                return Repository<TbPositionMenu>.Query(where, d => d.ID, "asc").ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
    }
}
