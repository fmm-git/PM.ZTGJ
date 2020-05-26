using Dos.Common;
using Dos.ORM;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using PM.Common;

namespace PM.Business
{
    public class TbUserMenuLogic
    {
        #region 新增数据

        /// <summary>
        /// 新增数据(多条)
        /// </summary>
        public AjaxResult Insert(List<TbUserMenu> userList, string UserCode)
        {
            if (userList == null)
                return AjaxResult.Warning("参数错误");
           
            try
            {
                //删除角色菜单
                var Rolecount = Repository<TbUserMenu>.Delete(t => t.UserCode == UserCode);
                //var model = MapperHelper.Map<TbUserMenu, TbPositionMenu>(userList);
                if (userList.Count>0)
                {
                    var count = Repository<TbUserMenu>.Insert(userList);
                }
                return AjaxResult.Success();
               
            }

            catch (Exception)
            {
                return AjaxResult.Error();
            }
        }

        #endregion

        #region 查询
        public List<TbUserMenu> GetList(string UserCode)
        {
            #region 模糊搜索条件

            var where = new Where<TbUserMenu>();
            if (!string.IsNullOrWhiteSpace(UserCode))
            {
                where.And(d => d.UserCode == UserCode);
            }

            #endregion
            try
            {
                return Repository<TbUserMenu>.Query(where, d => d.ID, "asc").ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 获取用户不分页数据列表
        /// </summary>
        public List<TbUser> GetUserGridList()
        {
            var where = new Where<TbUser>();
            return Repository<TbUser>.Query(where, d => d.UserCode, "asc").ToList();
        }


        #endregion

        #region 个人信息查询操作
        public List<TbUser> GetUserJson(string UserCode)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("select a.UserName,a.UserClosed,a.UserSex,Convert(varchar,Birthday,102) Birthday,a.PoliticalLandscape,a.MobilePhone,a.Tell,a.Email,a.QQ,a.WeChat,a.RecruitmentSource,a.PurchaseSocialSecurity,a.TheLaborContract,a.ConfidentialityContract,a.CardBankAdd,Convert(varchar,a.CreateTime,102) CreateTime,b.CompanyFullName as CompanyCode,d.DepartmentName as DepartmentCode from TbUser a left join TbCompany b on a.CompanyCode=b.CompanyCode left join TbUserAndDepRelevance c on a.UserCode=c.UserCode left join TbDepartment d on c.DepartmentCode=d.DepartmentCode where 1=1 AND a.UserClosed='在职'");
            //判断条件查询是否为空，不为空，条件添加进行查询
            if (!string.IsNullOrEmpty(UserCode))
            {
                sb.Append(" and a.UserCode=@UserCode");
            }
            sb.Append(" order by d.DepartmentName;");
            return Db.Context.FromSql(sb.ToString()).AddInParameter(
                "@UserCode", DbType.String, ""+UserCode+"").ToList<TbUser>();
        }

        //个人信息页面查询岗位信息
        public List<PositionUserResponse> GetPosition_UserCode(string UserCode)
        {
            string sql = "";
            try
            {
                if (!string.IsNullOrWhiteSpace(UserCode))
                {
                    sql = @"SELECT * FROM Fun_Position_UserCode(@UserCode)";
                }
                return Db.Context.FromSql(sql).AddInParameter("@UserCode", DbType.String, "" + UserCode + "").ToList<PositionUserResponse>();
            }
            catch (Exception)
            {

                throw;
            }
        }

        //个人信息页面查询角色信息
        public List<TbUserRoleUserRequset> GetRole_UserCode(string UserCode)
        {
            string sql = "";
            try
            {
                if (!string.IsNullOrWhiteSpace(UserCode))
                {
                    sql = @"SELECT *,RoleDetail UserName FROM TbRole A LEFT JOIN TbUserRole B ON A.RoleCode=B.RoleCode WHERE UserCode=@UserCode";
                }
                return Db.Context.FromSql(sql).AddInParameter("@UserCode", DbType.String, "" + UserCode + "").ToList<TbUserRoleUserRequset>();
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        
    }
}
