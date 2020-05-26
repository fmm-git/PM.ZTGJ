using Dos.Common;
using Dos.ORM;
using PM.Common;
using PM.Common.Helper;
using PM.DataAccess;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Business
{
    public class TbUserRoleLogic
    {

        #region 新增数据

        /// <summary>
        /// 新增数据(单条)
        /// </summary>
        public AjaxResult Insert(List<TbUserRole> userList)
        {
            if (userList == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                var model = MapperHelper.Map<TbUserRole, TbUserRole>(userList);
                var count = TbUserRoleRepository.Insert(userList);
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

        /// <summary>
        /// 新增数据(单条)
        /// </summary>
        public AjaxResult InsertNew(List<TbUserRole> userroleList, bool isApi = false)
        {
            if (userroleList == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                List<TbUserRole> data = Db.Context.From<TbUserRole>().Select(TbUserRole._.All).ToList();
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //先删除原来的表
                    Db.Context.FromSql("truncate table TbUserRole").SetDbTransaction(trans).ExecuteNonQuery();
                    Repository<TbUserRole>.Insert(trans, userroleList, isApi);
                    trans.Commit();//提交事务
                    return AjaxResult.Success();
                }
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
        public AjaxResult Update(TbUserRoleRequset request)
        {
            if (request == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                var model = MapperHelper.Map<TbUserRoleRequset, TbUserRole>(request);
                if (model == null)
                    return AjaxResult.Warning("信息不存在");
                var count = TbUserRoleRepository.Update(model);
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
        public AjaxResult Delete(string roleCode, string userCode, bool isApi = false)
        {
            try
            {
                var count = TbUserRoleRepository.Delete(t => t.RoleCode == roleCode && t.UserCode == userCode, isApi);
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

        #region 获取数据

        public TbUserRole FindEntity(string RoleCode)
        {
            var model = TbUserRoleRepository.First(d => d.RoleCode == RoleCode);
            return model;
        }

        ///// <summary>
        ///// 获取数据列表(分页)
        ///// </summary>
        //public List<TbUserRoleUserRequset> GetDataListForPage(TbUserRoleRequset param, string RoleCode, string keyword)
        //{
        //    try
        //    {
        //        return TbUserRoleRepository.GetDataListForPage(param, RoleCode, keyword);
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}
        /// <summary>
        /// 获取数据列表(分页)
        /// </summary>
        public PageModel GetDataListForPage(TbUserRoleRequset request)
        {
            try
            {
                //参数化
                List<Parameter> parameter = new List<Parameter>();
                string where = " where 1=1 and u.UserName is not null ";
                if (!string.IsNullOrWhiteSpace(request.ProjectId))
                {
                    where += " and ur.ProjectId=@ProjectId";
                    parameter.Add(new Parameter("@ProjectId", request.ProjectId, DbType.String, null));
                }
                if (!string.IsNullOrWhiteSpace(request.CompanyId))
                {
                    where += " and ur.OrgId=@OrgId";
                    parameter.Add(new Parameter("@OrgId", request.CompanyId, DbType.String, null));
                }
                if (!string.IsNullOrWhiteSpace(request.DeptId))
                {
                    where += " and ur.DeptId=@DeptId";
                    parameter.Add(new Parameter("@DeptId", request.DeptId, DbType.String, null));
                }
                if (!string.IsNullOrWhiteSpace(request.RoleCode))
                {
                    where += " and ur.RoleCode=@RoleCode";
                    parameter.Add(new Parameter("@RoleCode", request.RoleCode, DbType.String, null));
                }
                string sql = @"select ur.*,u.UserName,r.RoleName,cp.CompanyFullName,dp.DepartmentName,case when ur.OrgType=1 then '' else pro.ProjectName end as ProjectName from TbUserRole ur
left join TbUser u on ur.UserCode=u.UserId
left join TbRole r on ur.RoleCode=r.RoleId
left join TbCompany cp on ur.OrgId=cp.CompanyCode
left join TbDepartment dp on ur.DeptId=dp.DepartmentId
left join TbProjectInfo pro on pro.ProjectId=ur.ProjectId ";
                var model = Repository<TbUserRole>.FromSqlToPageTable(sql + where, parameter, request.rows, request.page, "ID", "asc");
                return model;
            }
            catch (Exception)
            {

                throw;
            }
            ////组装查询语句
            //#region 模糊搜索条件

            //var where = new Where<TbUserRole>();

            //if (!string.IsNullOrWhiteSpace(request.ProjectId))
            //{
            //    where.And(p => p.ProjectId == request.ProjectId);
            //}
            //if (!string.IsNullOrWhiteSpace(request.CompanyId))
            //{
            //    where.And(p => p.OrgId == request.CompanyId);
            //}
            //if (!string.IsNullOrWhiteSpace(request.DeptId))
            //{
            //    where.And(p => p.DeptId == request.DeptId);
            //} 
            //if (!string.IsNullOrWhiteSpace(request.RoleCode))
            //{
            //    where.And(p => p.RoleCode == request.RoleCode);
            //}

            //#endregion

            //try
            //{
            //    var ret = Db.Context.From<TbUserRole>()
            //      .Select(TbUserRole._.All,
            //      TbUser._.UserName,
            //      TbRole._.RoleName,
            //      TbCompany._.CompanyFullName,
            //      TbDepartment._.DepartmentName,
            //      TbProjectInfo._.ProjectName)
            //      .LeftJoin<TbUser>((a, c) => a.UserCode == c.UserId)
            //      .LeftJoin<TbRole>((a, c) => a.RoleCode == c.RoleId)
            //      .LeftJoin<TbCompany>((a, c) => a.OrgId == c.CompanyCode)
            //      .LeftJoin<TbDepartment>((a, c) => a.DeptId == c.DepartmentId)
            //      .LeftJoin<TbProjectInfo>((a, c) => a.ProjectId == c.ProjectId)
            //          .Where(where).OrderByDescending(d => d.UserCode).ToPageList(request.rows, request.page);
            //    return ret;
            //}
            //catch (Exception)
            //{
            //    throw;
            //}
        }
        /// <summary>
        /// 获取用户不分页数据列表
        /// </summary>
        /// <param name="code">编码</param>
        /// <param name="type">1：角色 2：岗位</param>
        public List<TbUser> GetUserGridList(UserListRequset param)
        {
            try
            {
                return TbUserRoleRepository.GetUserGridList(param);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #endregion

        /// <summary>
        /// 获取登录用户信息
        /// </summary>
        /// <param name="menuCode"></param>
        /// <returns></returns>
        public CurrentUserInfo FindUserInfo(string userCode)
        {
            return TbUserRoleRepository.FindUserInfo(userCode);
        }
    }
}
