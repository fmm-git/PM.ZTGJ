using Dos.ORM;
using PM.Common;
using PM.Common.Helper;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace PM.DataAccess
{
    /// <summary>
    /// 数据库处理层。多表联查、复杂的Dos.ORM写法都可以丢到这层来写。
    /// </summary>
    public class TbUserRoleRepository : Repository<TbUserRole>
    {

        /// <summary>
        /// 获取登录用户信息
        /// </summary>
        /// <param name="menuCode"></param>
        /// <returns></returns>
        public static CurrentUserInfo FindUserInfo(string userCode)
        {
            //            string sql = @"select 
            //                        	tu.UserCode,
            //                        	tu.UserName,
            //                        	ur.RoleCode,
            //                            tu.CompanyCode as CompanyId,
            //                            tc.OrgType,
            //                        	tpu.PositionCode  
            //                        from TbUser tu
            //                        left join TbUserRole ur on tu.UserCode=ur.UserCode
            //                        left join TbPositionUser tpu on tu.UserCode=tpu.UserCode
            //                        left join TbCompany tc on tc.CompanyCode=tu.CompanyCode
            //                        where tu.UserCode=@userCode";
            //            var model = Db.Context.FromSql(sql)
            //                .AddInParameter("@userCode", DbType.String, userCode)
            //                .ToFirst<CurrentUserInfo>();
            //            return model;

            CurrentUserInfo cui = new CurrentUserInfo();
            string sql = @"select top 1 ur.ProjectId,pro.ProjectName,ur.OrgId,cm.CompanyFullName,cm.ParentCompanyCode,ur.OrgType,ur.UserCode as UserId,u.UserCode,u.UserName from TbUserRole ur 
left join TbCompany cm on ur.OrgId=cm.CompanyCode
left join TbProjectInfo pro on ur.ProjectId=pro.ProjectId
left join TbUser u on ur.UserCode=u.UserId
where u.UserCode=@UserCode and ur.Flag=0 group by ur.ProjectId,pro.ProjectName,ur.OrgId,cm.CompanyFullName,cm.ParentCompanyCode,ur.OrgType,ur.UserCode,u.UserCode,u.UserName order by OrgId asc";
            DataTable dt = Db.Context.FromSql(sql).AddInParameter("@UserCode", DbType.String, userCode).ToDataTable();
            if (dt != null&&dt.Rows.Count>0)
            {
                string sqlRole = @"select distinct ur.RoleCode as RoleId,r.RoleCode,r.RoleName from TbUserRole ur
left join TbUser u on ur.UserCode=u.UserId
left join TbRole r on ur.RoleCode=r.RoleId
where u.UserCode=@UserCode and ProjectId=@ProjectId and OrgId=@OrgId and ur.Flag=0";
                string strRole = "";
                DataTable dtRole = Db.Context.FromSql(sqlRole).AddInParameter("@UserCode", DbType.String, userCode).AddInParameter("@ProjectId", DbType.String, dt.Rows[0]["ProjectId"]).AddInParameter("@OrgId", DbType.String, dt.Rows[0]["OrgId"]).ToDataTable();
                if (dtRole != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dtRole.Rows.Count; i++)
                    {
                        if (i == dtRole.Rows.Count - 1)
                        {
                            strRole += "'" + dtRole.Rows[i]["RoleId"] + "'";
                        }
                        else
                        {
                            strRole += "'" + dtRole.Rows[i]["RoleId"] + "',";
                        }
                    }
                }
                cui.ProjectId = Convert.ToString(dt.Rows[0]["ProjectId"]);
                cui.CompanyId = Convert.ToString(dt.Rows[0]["OrgId"]);
                cui.OrgType = Convert.ToString(dt.Rows[0]["OrgType"]);
                cui.ComPanyName = Convert.ToString(dt.Rows[0]["CompanyFullName"]);
                if (cui.OrgType == "3" || cui.OrgType == "4" || cui.OrgType == "5")
                {
                    string ProjectOrgAllId = "";
                    string ProjectOrgAllName = "";
                    string sql1 = @"with tab as
                                    (
                                     select CompanyCode,ParentCompanyCode,CompanyFullName,OrgType from TbCompany where CompanyCode=@CompanyCode
                                     union all
                                     select b.CompanyCode,b.ParentCompanyCode,b.CompanyFullName,b.OrgType
                                     from
                                      tab a,
                                      TbCompany b 
                                      where a.ParentCompanyCode=b.CompanyCode
                                    )
                                    select * from tab order by OrgType asc;";
                    var ret1 = Db.Context.FromSql(sql1).AddInParameter("@CompanyCode", DbType.String, dt.Rows[0]["OrgId"]).ToDataTable();
                    if (ret1 != null && ret1.Rows.Count > 0)
                    {
                        for (int j = 0; j < ret1.Rows.Count; j++)
                        {
                            if (ret1.Rows.Count >1&& ret1.Rows.Count != j + 1)
                            {
                                ProjectOrgAllId += ret1.Rows[j]["CompanyCode"] + "/";
                                ProjectOrgAllName += ret1.Rows[j]["CompanyFullName"] + "/";
                            }
                            else
                            {
                                ProjectOrgAllId += ret1.Rows[j]["CompanyCode"];
                                ProjectOrgAllName += ret1.Rows[j]["CompanyFullName"];
                            }
                        }
                    }
                    cui.ProjectOrgAllId = ProjectOrgAllId;
                    cui.ProjectOrgAllName = ProjectOrgAllName;
                    //cui.ProcessFactoryCode = "";
                    //cui.ProcessFactoryName = "所有加工厂";
                    cui.ProcessFactoryCode = "6386683729561128960";
                    cui.ProcessFactoryName = "二号加工厂";
                }
                else
                {
                    cui.ProjectOrgAllId = Convert.ToString(dt.Rows[0]["OrgId"]);
                    cui.ProjectOrgAllName = Convert.ToString(dt.Rows[0]["CompanyFullName"]);
                    if (cui.OrgType=="1")//当前登录人是加工厂
                    {
                        cui.ProcessFactoryCode = Convert.ToString(dt.Rows[0]["OrgId"]);
                        cui.ProcessFactoryName = Convert.ToString(dt.Rows[0]["CompanyFullName"]);
                        cui.ProjectId = "";
                    }
                    else//经理部默认所有
                    {
                        //cui.ProcessFactoryCode = "";
                        //cui.ProcessFactoryName = "所有加工厂";
                        cui.ProcessFactoryCode = "6386683729561128960";
                        cui.ProcessFactoryName = "二号加工厂";
                    }
                }
                cui.RoleCode = strRole;
                cui.UserId = Convert.ToString(dt.Rows[0]["UserId"]);
                cui.UserCode = Convert.ToString(dt.Rows[0]["UserCode"]);
                cui.UserName = Convert.ToString(dt.Rows[0]["UserName"]);
            }
            return cui;
        }


        /// <summary>
        /// 获取数据列表(分页)
        /// </summary>
        public static List<TbUserRoleUserRequset> GetDataListForPage(TbUserRoleRequset param, string RoleCode, string keyword)
        {
            //组装查询语句
            #region 模糊搜索条件

            var where = new Where<TbUserRole>();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                where.Or<TbUser>((a, b) => a.UserCode.Like(keyword) || b.UserName.Like(keyword));
                where.Or<TbRole>((a, b) => b.RoleName.Like(keyword) || b.State.Like(keyword));
            }
            if (!string.IsNullOrEmpty(RoleCode))
            {
                where.And(d => d.RoleCode.Like(RoleCode));
            }
            #endregion
            try
            {
                var data = Db.Context.From<TbUserRole>()
               .Select(TbUserRole._.All,
                       TbUser._.UserName,
                       TbRole._.RoleName,
                       TbRole._.State)
               .LeftJoin<TbUser>((a, c) => a.UserCode == c.UserCode)
               .LeftJoin<TbRole>((a, c) => a.RoleCode == c.RoleCode).Where(where).OrderBy(d => d.UserCode);

                var dt = data
                .Page(param.rows, param.page)
                .ToDataTable();
                //var dateCount = data.Count();
                //取总数，以计算共多少页。
                var dateCount = Db.Context.From<TbUserRole>()
                   .LeftJoin<TbUser>((a, c) => a.UserCode == c.UserCode)
                   .LeftJoin<TbRole>((a, c) => a.RoleCode == c.RoleCode)
                   .Where(where).Count();
                param.records = dateCount;
                var list = ModelConvertHelper<TbUserRoleUserRequset>.ToList(dt);
                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 获取用户不分页数据列表
        /// </summary>
        /// <param name="code">编码</param>
        /// <param name="type">1：角色 2：岗位</param>
        public static List<TbUser> GetUserGridList(UserListRequset param)
        {
            var where = new Where<TbUser>();
            var userCode = new List<string>();
            if (param.type == 1)
            {
                userCode = TbUserRoleRepository.Query(p => p.RoleCode == param.code).Select(p => p.UserCode).Distinct().ToList();
            }
            else
            {
                userCode = Repository<TbPositionUser>.Query(p => p.PositionCode == param.code).Select(p => p.UserCode).Distinct().ToList();
            }
            if (userCode.Count > 0)
                where.And(d => d.UserCode.NotIn(userCode));

            if (!string.IsNullOrEmpty(param.keyword))
            {
                where.And(d => d.UserCode.Like(param.keyword));
                where.Or(d => d.UserName.Like(param.keyword));
            }


            param.records = Repository<TbUser>.Count(where);
            try
            {
                var orderBy = OrderByOperater.ASC;
                if (param.sord.Equals("desc"))
                    orderBy = OrderByOperater.DESC;
                var orderByClip = new OrderByClip(param.sidx, orderBy);//排序字段
                return Repository<TbUser>.Query(where, orderByClip, param.sord, param.rows, param.page).ToList();
            }
            catch (Exception ex)
            {

                throw;
            }

        }
    }
}
