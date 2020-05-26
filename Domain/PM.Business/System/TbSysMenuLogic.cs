using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dos.Common;
using Dos.ORM;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using System.Data;
using PM.Domain.WebBase;

namespace PM.Business
{
    public class TbSysMenuLogic
    {
        #region 新增数据

        /// <summary>
        /// 新增数据(单条)
        /// </summary>
        public AjaxResult Insert(TbSysMenuRequset request)
        {
            if (request == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                var model = MapperHelper.Map<TbSysMenuRequset, TbSysMenu>(request);

                #region 将按钮的bool值转换为int值

                if (!string.IsNullOrWhiteSpace(model.OperationAdd))
                {
                    if (model.OperationAdd == "true")
                    {
                        model.OperationAdd = "1";
                    }
                    else
                    {
                        model.OperationAdd = "0";
                    }
                }
                if (!string.IsNullOrWhiteSpace(model.OperationEdit))
                {
                    if (model.OperationEdit == "true")
                    {
                        model.OperationEdit = "1";
                    }
                    else
                    {
                        model.OperationEdit = "0";
                    }
                }
                if (!string.IsNullOrWhiteSpace(model.OperationDel))
                {
                    if (model.OperationDel == "true")
                    {
                        model.OperationDel = "1";
                    }
                    else
                    {
                        model.OperationDel = "0";
                    }
                }
                if (!string.IsNullOrWhiteSpace(model.OperationView))
                {
                    if (model.OperationView == "true")
                    {
                        model.OperationView = "1";
                    }
                    else
                    {
                        model.OperationView = "0";
                    }
                }
                if (!string.IsNullOrWhiteSpace(model.OperationOutput))
                {
                    if (model.OperationOutput == "true")
                    {
                        model.OperationOutput = "1";
                    }
                    else
                    {
                        model.OperationOutput = "0";
                    }
                }
                if (!string.IsNullOrWhiteSpace(model.OperationExamination))
                {
                    if (model.OperationExamination == "true")
                    {
                        model.OperationExamination = "1";
                    }
                    else
                    {
                        model.OperationExamination = "0";
                    }
                }
                if (!string.IsNullOrWhiteSpace(model.OperationOther1))
                {
                    if (model.OperationOther1 == "true")
                    {
                        model.OperationOther1 = "1";
                    }
                    else
                    {
                        model.OperationOther1 = "0";
                    }
                }
                if (!string.IsNullOrWhiteSpace(model.OperationOther2))
                {
                    if (model.OperationOther2 == "true")
                    {
                        model.OperationOther2 = "1";
                    }
                    else
                    {
                        model.OperationOther2 = "0";
                    }
                }
                if (!string.IsNullOrWhiteSpace(model.OperationOther3))
                {
                    if (model.OperationOther3 == "true")
                    {
                        model.OperationOther3 = "1";
                    }
                    else
                    {
                        model.OperationOther3 = "0";
                    }
                }
                if (!string.IsNullOrWhiteSpace(model.OperationOther4))
                {
                    if (model.OperationOther4 == "true")
                    {
                        model.OperationOther4 = "1";
                    }
                    else
                    {
                        model.OperationOther4 = "0";
                    }
                }
                if (!string.IsNullOrWhiteSpace(model.OperationOther5))
                {
                    if (model.OperationOther5 == "true")
                    {
                        model.OperationOther5 = "1";
                    }
                    else
                    {
                        model.OperationOther5 = "0";
                    }
                }
                #endregion
             
                var count = Repository<TbSysMenu>.Insert(model);
                if (count > 0)
                {
                    return AjaxResult.Success();
                }
                return AjaxResult.Error("操作失败");
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败");
            }

        }

        #endregion

        #region 修改数据

        /// <summary>
        /// 修改数据(单条)
        /// </summary>
        public AjaxResult Update(TbSysMenuRequset request)
        {
            if (request == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                var model = MapperHelper.Map<TbSysMenuRequset, TbSysMenu>(request);
                if (model == null)
                    return AjaxResult.Error("信息不存在");

                #region 将按钮的bool值转换为int值

                if (!string.IsNullOrWhiteSpace(model.OperationAdd))
                {
                    if (model.OperationAdd == "true")
                    {
                        model.OperationAdd = "1";
                    }
                    else
                    {
                        model.OperationAdd = "0";
                    }
                }
                if (!string.IsNullOrWhiteSpace(model.OperationEdit))
                {
                    if (model.OperationEdit == "true")
                    {
                        model.OperationEdit = "1";
                    }
                    else
                    {
                        model.OperationEdit = "0";
                    }
                }
                if (!string.IsNullOrWhiteSpace(model.OperationDel))
                {
                    if (model.OperationDel == "true")
                    {
                        model.OperationDel = "1";
                    }
                    else
                    {
                        model.OperationDel = "0";
                    }
                }
                if (!string.IsNullOrWhiteSpace(model.OperationView))
                {
                    if (model.OperationView == "true")
                    {
                        model.OperationView = "1";
                    }
                    else
                    {
                        model.OperationView = "0";
                    }
                }
                if (!string.IsNullOrWhiteSpace(model.OperationOutput))
                {
                    if (model.OperationOutput == "true")
                    {
                        model.OperationOutput = "1";
                    }
                    else
                    {
                        model.OperationOutput = "0";
                    }
                }
                if (!string.IsNullOrWhiteSpace(model.OperationExamination))
                {
                    if (model.OperationExamination == "true")
                    {
                        model.OperationExamination = "1";
                    }
                    else
                    {
                        model.OperationExamination = "0";
                    }
                }
                if (!string.IsNullOrWhiteSpace(model.OperationOther1))
                {
                    if (model.OperationOther1 == "true")
                    {
                        model.OperationOther1 = "1";
                    }
                    else
                    {
                        model.OperationOther1 = "0";
                    }
                }
                if (!string.IsNullOrWhiteSpace(model.OperationOther2))
                {
                    if (model.OperationOther2 == "true")
                    {
                        model.OperationOther2 = "1";
                    }
                    else
                    {
                        model.OperationOther2 = "0";
                    }
                }
                if (!string.IsNullOrWhiteSpace(model.OperationOther3))
                {
                    if (model.OperationOther3 == "true")
                    {
                        model.OperationOther3 = "1";
                    }
                    else
                    {
                        model.OperationOther3 = "0";
                    }
                }
                if (!string.IsNullOrWhiteSpace(model.OperationOther4))
                {
                    if (model.OperationOther4 == "true")
                    {
                        model.OperationOther4 = "1";
                    }
                    else
                    {
                        model.OperationOther4 = "0";
                    }
                }
                if (!string.IsNullOrWhiteSpace(model.OperationOther5))
                {
                    if (model.OperationOther5 == "true")
                    {
                        model.OperationOther5 = "1";
                    }
                    else
                    {
                        model.OperationOther5 = "0";
                    }
                }
                #endregion

                var count = Repository<TbSysMenu>.Update(model);
                if (count > 0)
                {
                    return AjaxResult.Success();
                }
                return AjaxResult.Error("操作失败");
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败");
            }
        }

        #endregion

        #region 删除数据

        /// <summary>
        /// 删除数据
        /// </summary>
        public AjaxResult Delete(string MenuCode)
        {
            try
            {
                var model1 = Repository<TbSysMenu>.First(t => t.MenuPCode == MenuCode);
                if (model1 != null)
                {
                    return AjaxResult.Error("该菜单下有子菜单不能删除");
                }
                var count = Repository<TbSysMenu>.Delete(t => t.MenuCode == MenuCode);
                if (count > 0)
                {
                    return AjaxResult.Success();
                }
                return AjaxResult.Error();
            }
            catch (Exception)
            {
                return AjaxResult.Error();
            }

        }


        /// <summary>
        /// 删除数据(单条)
        /// </summary>
        public BaseResult Delete(TbSysMenu model)
        {
            if (model == null)
                return new BaseResult(false, null, "参数不能为空");
            try
            {
                var count = Repository<TbSysMenu>.Delete(model);
                return new BaseResult(count > 0, count, count > 0 ? "" : "操作失败");
            }
            catch (Exception)
            {
                return new BaseResult(false, "操作失败");
            }
        }

        #endregion

        #region 获取数据

        /// <summary>
        /// 获取菜单列表数据
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public List<TbSysMenu> GetList(string keyword)
        {
            #region 模糊搜索条件

            var where = new Where<TbSysMenu>();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                where.And(d => d.MenuCode.Like(keyword));
                where.Or(d => d.MenuName.Like(keyword));
            }

            #endregion
            try
            {
                return Repository<TbSysMenu>.Query(where, d => d.Sort, "asc").ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// 获取菜单列表数据
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public List<TbSysMenu> GetAuthorityMenuList(string keyword)
        {
            #region 模糊搜索条件

            var where = new Where<TbSysMenu>();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                where.And(d => d.MenuCode.Like(keyword));
                where.Or(d => d.MenuName.Like(keyword));
            }
            where.And(d => d.IsShow == "0");

            #endregion
            try
            {
                return Repository<TbSysMenu>.Query(where, d => d.Sort, "asc").ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// 获取单条数据
        /// </summary>
        /// <param name="menuCode"></param>
        /// <returns></returns>
        public TbSysMenu FindEntity(string menuCode)
        {
            var model = Repository<TbSysMenu>.First(d => d.MenuCode == menuCode);

            #region 将按钮的bool值转换为int值

            if (!string.IsNullOrWhiteSpace(model.OperationAdd))
            {
                if (model.OperationAdd == "1")
                {
                    model.OperationAdd = "true";
                }
                else
                {
                    model.OperationAdd = "false";
                }
            }
            if (!string.IsNullOrWhiteSpace(model.OperationEdit))
            {
                if (model.OperationEdit == "1")
                {
                    model.OperationEdit = "true";
                }
                else
                {
                    model.OperationEdit = "false";
                }
            }
            if (!string.IsNullOrWhiteSpace(model.OperationDel))
            {
                if (model.OperationDel == "1")
                {
                    model.OperationDel = "true";
                }
                else
                {
                    model.OperationDel = "false";
                }
            }
            if (!string.IsNullOrWhiteSpace(model.OperationView))
            {
                if (model.OperationView == "1")
                {
                    model.OperationView = "true";
                }
                else
                {
                    model.OperationView = "false";
                }
            }
            if (!string.IsNullOrWhiteSpace(model.OperationOutput))
            {
                if (model.OperationOutput == "1")
                {
                    model.OperationOutput = "true";
                }
                else
                {
                    model.OperationOutput = "false";
                }
            }
            if (!string.IsNullOrWhiteSpace(model.OperationExamination))
            {
                if (model.OperationExamination == "1")
                {
                    model.OperationExamination = "true";
                }
                else
                {
                    model.OperationExamination = "false";
                }
            }
            if (!string.IsNullOrWhiteSpace(model.OperationOther1))
            {
                if (model.OperationOther1 == "1")
                {
                    model.OperationOther1 = "true";
                }
                else
                {
                    model.OperationOther1 = "false";
                }
            }
            if (!string.IsNullOrWhiteSpace(model.OperationOther2))
            {
                if (model.OperationOther2 == "1")
                {
                    model.OperationOther2 = "true";
                }
                else
                {
                    model.OperationOther2 = "false";
                }
            }
            if (!string.IsNullOrWhiteSpace(model.OperationOther3))
            {
                if (model.OperationOther3 == "1")
                {
                    model.OperationOther3 = "true";
                }
                else
                {
                    model.OperationOther3 = "false";
                }
            }
            if (!string.IsNullOrWhiteSpace(model.OperationOther4))
            {
                if (model.OperationOther4 == "1")
                {
                    model.OperationOther4 = "true";
                }
                else
                {
                    model.OperationOther4 = "false";
                }
            }
            if (!string.IsNullOrWhiteSpace(model.OperationOther5))
            {
                if (model.OperationOther5 == "1")
                {
                    model.OperationOther5 = "true";
                }
                else
                {
                    model.OperationOther5 = "false";
                }
            }
            #endregion
            return model;
        }

        /// <summary>
        /// 获取左边导航菜单列表数据
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public List<TbSysMenu> GetNavMenuList()
        {
            #region 模糊搜索条件

            var where = new Where<TbSysMenu>();
            where.And(d => d.IsShow == "0");
            where.And(d => d.MenuType == "PC");

            #endregion

            return Repository<TbSysMenu>.Query(where, d => d.Sort, "asc").ToList();
        }


        /// <summary>
        /// 根据登录人角色获取菜单数据
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public List<TbSysMenu> GetCurrentMenuListByRoleCodePC(string roleCode)
        {
            string sql = @"select tsm.* from TbRoleMenu tum
                           left join TbSysMenu tsm on tum.MenuCode=tsm.MenuCode
                           where tsm.IsShow=0 and tsm.MenuType='PC' and tum.RoleCode in(" + roleCode + ") order by tsm.Sort asc";
            var model = Db.Context.FromSql(sql).ToList<TbSysMenu>();
            return model;
        }

        /// <summary>
        /// 根据登录人编号获取菜单数据
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public List<TbSysMenu> GetCurrentMenuListByUserCodePC(string userCode)
        {
            string sql = @"select 
                        	tsm.*
                        from TbUserMenu tum
                        left join TbSysMenu tsm on tum.MenuCode=tsm.MenuCode
                        where tsm.IsShow=0 and tsm.MenuType='PC' and tum.UserCode=@userCode order by tsm.Sort asc";
            var model = Db.Context.FromSql(sql)
                .AddInParameter("@userCode", DbType.String, userCode)
                .ToList<TbSysMenu>();
            return model;
        }
        /// <summary>
        /// 根据登录人角色获取菜单数据
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public List<TbSysMenuApp> GetCurrentMenuListByRoleCodeApp(string roleCode)
        {
            string sql = @"select tsm.* from TbRoleMenu tum
                           left join TbSysMenu tsm on tum.MenuCode=tsm.MenuCode
                           where tsm.IsShow=0 and tsm.MenuType='App' and tum.RoleCode in(" + roleCode + ") order by tsm.Sort asc";
            var model = Db.Context.FromSql(sql).ToList<TbSysMenuApp>();
            return model;
        }

        /// <summary>
        /// 根据登录人编号获取菜单数据
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public List<TbSysMenuApp> GetCurrentMenuListByUserCodeApp(string userCode)
        {
            string sql = @"select 
                        	tsm.*
                        from TbUserMenu tum
                        left join TbSysMenu tsm on tum.MenuCode=tsm.MenuCode
                        where tsm.IsShow=0 and tsm.MenuType='App' and tum.UserCode=@userCode order by tsm.Sort asc";
            var model = Db.Context.FromSql(sql)
                .AddInParameter("@userCode", DbType.String, userCode)
                .ToList<TbSysMenuApp>();
            return model;
        }

        /// <summary>
        /// 根据登录人信息获取菜单数据
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public List<TbSysMenu> GetCurrentMenuListByUser(string userCode, string roleCode, string positionCode)
        {
            string sql = @"select tsm.* from TbSysMenu tsm ";
            string where = " where 1=1 ";
            if (!string.IsNullOrEmpty(userCode))
            {
                sql += " left join TbUserMenu tum on tum.MenuCode=tsm.MenuCode";
                where += " and tum.UserCode=@userCode";
            }
            if (!string.IsNullOrEmpty(userCode))
            {
                sql += " left join TbRoleMenu trm on trm.MenuCode=tsm.MenuCode";
                where += " and trm.RoleCode=@roleCode";
            }
            if (!string.IsNullOrEmpty(userCode))
            {
                sql += " left join TbPositionMenu tpm on tpm.MenuCode=tsm.MenuCode";
                where += " and tpm.PositionCode=@positionCode";
            }
            var model = Db.Context.FromSql(sql + where)
                .AddInParameter("@userCode", DbType.String, userCode)
                .AddInParameter("@roleCode", DbType.String, roleCode)
                .AddInParameter("@positionCode", DbType.String, positionCode)
                .ToList<TbSysMenu>();
            return model;
        }

        #endregion
    }
}
