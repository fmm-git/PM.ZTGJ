using Dos.Common;
using Dos.ORM;
using PM.Business.Production;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using PM.DataEntity.System.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace PM.Business
{
    /// <summary>
    /// 用户员工数据处理
    /// </summary>
    public class UserLogic
    {
        private readonly TbWorkOrderLogic _workOrderLogic = new TbWorkOrderLogic();

        #region 员工查询处理

        /// <summary>
        /// 查询全部公司左侧分类
        /// </summary>
        /// <returns></returns>
        public List<TbCompany> GetAllCompany()
        {
            return Db.Context.From<TbCompany>().ToList();
        }

        /// <summary>
        /// 根据左侧公司Code查询员工信息
        /// </summary>
        /// <param name="ComCode"></param>
        /// <returns></returns>
        public List<TbUser> GetUserByCompanyCode(string ComCode)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("select ");
            sb.Append("a.ID,a.UserCode,a.UserName,a.UserPwd,a.UserClosed,a.UserSex,");
            sb.Append("a.Birthday,a.IDNumber,a.PoliticalLandscape,a.MobilePhone,a.Tell,a.Email,");
            sb.Append("a.QQ,a.WeChat,a.RecruitmentSource,a.PurchaseSocialSecurity,a.TheLaborContract,");
            sb.Append("a.ConfidentialityContract,a.CardNumber,a.CardBankName,a.CardBankAdd,a.CreateTime,a.CreateUser,f.UserName as CreateUserName,");
            sb.Append("b.CompanyFullName as CompanyCode,d.DepartmentName as DepartmentCode ");
            sb.Append("from TbUser a left join TbCompany b on a.CompanyCode=b.CompanyCode ");
            sb.Append("left join TbUserAndDepRelevance c on a.UserCode=c.UserCode ");
            sb.Append("left join TbDepartment d on c.DepartmentCode=d.DepartmentCode left join TbUser f on a.CreateUser=f.UserCode where 1=1");
            sb.Append(" and a.CompanyCode = @name");
            sb.Append(" order by a.CompanyCode,d.DepartmentCode;");
            return Db.Context.FromSql(sb.ToString()).AddInParameter(
                "@name", DbType.String, ComCode).ToList<TbUser>();
        }

        /// <summary>
        /// 查询全部或者根据条件查询员工信息
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public List<TbUser> GetAllOrBySearchUser(string keyValue)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("select ");
            sb.Append("a.ID,a.UserCode,a.UserName,a.UserPwd,a.UserClosed,a.UserSex,");
            sb.Append("a.Birthday,a.IDNumber,a.PoliticalLandscape,a.MobilePhone,a.Tell,a.Email,");
            sb.Append("a.QQ,a.WeChat,a.RecruitmentSource,a.PurchaseSocialSecurity,a.TheLaborContract,");
            sb.Append("a.ConfidentialityContract,a.CardNumber,a.CardBankName,a.CardBankAdd,a.CreateTime,a.CreateUser,f.UserName as CreateUserName,");
            sb.Append("b.CompanyFullName as CompanyCode,d.DepartmentName as DepartmentCode ");
            sb.Append("from TbUser a left join TbCompany b on a.CompanyCode=b.CompanyCode ");
            sb.Append("left join TbUserAndDepRelevance c on a.UserCode=c.UserCode ");
            sb.Append("left join TbDepartment d on c.DepartmentCode=d.DepartmentCode left join TbUser f on a.CreateUser=f.UserCode where 1=1");
            //判断条件查询是否为空，不为空，条件添加进行查询
            if (!string.IsNullOrEmpty(keyValue))
            {
                sb.Append(" and a.UserName like @name");
            }
            sb.Append(" order by a.CompanyCode,d.DepartmentCode;");
            return Db.Context.FromSql(sb.ToString()).AddInParameter(
                "@name", DbType.String, "%" + keyValue + "%").ToList<TbUser>();
        }

        /// <summary>
        /// 获取数据列表(分页)
        /// </summary>
        public PageModel GetDataListForPage(TbUserRequest request)
        {
            //组装查询语句
            #region 模糊搜索条件

            var where = new Where<TbUser>();
            if (!string.IsNullOrWhiteSpace(request.UserName))
            {
                where.And(p => p.UserName.Contains(request.UserName));
            }

            if (!string.IsNullOrWhiteSpace(request.keyword))
            {

                where.And(p => p.UserName.Like(request.keyword) ||
                               p.UserClosed.Like(request.keyword) ||
                               p.UserSex.Like(request.keyword));

            }

            #endregion

            try
            {
                var ret = Db.Context.From<TbUser>()
              .Select(
                      TbUser._.ID
                      , TbUser._.UserCode
                      , TbUser._.UserId
                      , TbUser._.UserName
                      , TbUser._.UserClosed
                      , TbUser._.UserSex
                      , TbUser._.MobilePhone
                      , TbUser._.IDNumber)
                    .Where(where).OrderByDescending(d => d.UserId).ToPageList(request.rows, request.page);
                return ret;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public PageModel GetCompanyUser(TbUserRequest request, string UserName, string CompanyCode, string DepartmentProjectId)
        {
            //组装查询语句
            //参数化
            List<Parameter> parameter = new List<Parameter>();
            string where = " where 1=1 and ur.OrgId=@OrgId and ur.ProjectId=@ProjectId and ur.Flag=0 ";
            parameter.Add(new Parameter("@OrgId", CompanyCode, DbType.String, null));
            parameter.Add(new Parameter("@ProjectId", DepartmentProjectId, DbType.String, null));
            if (!string.IsNullOrWhiteSpace(UserName))
            {
                where += " and u.UserName like @UserName";
                parameter.Add(new Parameter("@UserName", '%' + UserName + '%', DbType.String, null));
            }
            string sql = @"select distinct u.ID,u.UserId,u.UserCode,u.UserName,u.UserClosed,u.UserSex,u.IDNumber,u.MobilePhone from TbUser u
left join TbUserRole ur on ur.UserCode=u.UserId ";
            var model = Repository<TbCompany>.FromSqlToPageTable(sql + where, parameter, request.rows, request.page, "ID", "asc");
            return model;
        }
        /// <summary>
        /// 查询全部或者根据条件查询在职员工信息
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public List<TbUser> GetIncumbencyUser(string keyValue)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("select ");
            sb.Append("a.ID,a.UserCode,a.UserName,a.UserPwd,a.UserClosed,a.UserSex,");
            sb.Append("a.Birthday,a.IDNumber,a.PoliticalLandscape,a.MobilePhone,a.Tell,a.Email,");
            sb.Append("a.QQ,a.WeChat,a.RecruitmentSource,a.PurchaseSocialSecurity,a.TheLaborContract,");
            sb.Append("a.ConfidentialityContract,a.CardNumber,a.CardBankName,a.CardBankAdd,a.CreateTime,a.CreateUser,f.UserName as CreateUserName,");
            sb.Append("b.CompanyFullName as CompanyCode,d.DepartmentName as DepartmentCode ");
            sb.Append("from TbUser a left join TbCompany b on a.CompanyCode=b.CompanyCode ");
            sb.Append("left join TbUserAndDepRelevance c on a.UserCode=c.UserCode ");
            sb.Append("left join TbDepartment d on c.DepartmentCode=d.DepartmentCode left join TbUser f on a.CreateUser=f.UserCode where 1=1 and a.UserClosed='在职'");
            //判断条件查询是否为空，不为空，条件添加进行查询
            if (!string.IsNullOrEmpty(keyValue))
            {
                sb.Append(" and a.UserName like @name");
            }
            sb.Append(" order by a.CompanyCode,d.DepartmentCode;");
            return Db.Context.FromSql(sb.ToString()).AddInParameter(
                "@name", DbType.String, "%" + keyValue + "%").ToList<TbUser>();
        }

        public TbUser GetFormJson(string keyValue)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("select ");
            sb.Append("a.ID,a.UserCode,a.UserName,a.UserPwd,a.UserClosed,a.UserSex,");
            sb.Append("a.Birthday,a.IDNumber,a.PoliticalLandscape,a.MobilePhone,a.Tell,a.Email,");
            sb.Append("a.QQ,a.WeChat,a.RecruitmentSource,a.PurchaseSocialSecurity,a.TheLaborContract,");
            sb.Append("a.ConfidentialityContract,a.CardNumber,a.CardBankName,a.CardBankAdd,a.CreateTime,a.CreateUser,f.UserName as CreateUserName,");
            sb.Append("b.CompanyFullName as CompanyCodeName,a.CompanyCode,d.DepartmentName as DepartmentName,");
            sb.Append("d.DepartmentCode as DepartmentCode ");
            sb.Append("from TbUser a left join TbCompany b on a.CompanyCode=b.CompanyCode ");
            sb.Append("left join TbUserAndDepRelevance c on a.UserCode=c.UserCode ");
            sb.Append("left join TbDepartment d on c.DepartmentCode=d.DepartmentCode left join TbUser f on a.CreateUser=f.UserCode where");
            sb.Append(" a.UserCode = @name");
            return Db.Context.FromSql(sb.ToString()).AddInParameter(
                "@name", DbType.String, keyValue).ToFirstDefault<TbUser>();
        }

        /// <summary>
        /// select 绑定
        /// </summary>
        /// <param name="dataCode"></param>
        /// <returns></returns>
        public List<TbSysDictionaryData> GetList(string dataCode)
        {
            var where = new Where<TbSysDictionaryData>();
            where.And(d => d.FDictionaryCode == dataCode);
            return Repository<TbSysDictionaryData>.Query(where, d => d.DictionaryOrder, "asc").ToList();
        }
        /// <summary>
        /// 查询用户所有存在的岗位
        /// </summary>
        /// <param name="usercode"></param>
        /// <returns></returns>
        public List<TbPosition> GetUserPosition(string usercode)
        {
            return Db.Context.FromSql("select a.id,a.PositionName,d.DepartmentName,c.CompanyFullName from TbPosition a left join TbPositionUser b on a.PositionCode=b.PositionCode left join TbCompany c on a.CompanyCode=c.CompanyCode left join TbDepartment d on a.DepartmentCode=d.DepartmentCode where b.UserCode=@ucode").AddInParameter("@ucode", DbType.String, usercode).ToList<TbPosition>();
        }
        /// <summary>
        /// 查询用户所有权限
        /// </summary>
        /// <param name="usercode"></param>
        /// <returns></returns>
        public List<TbUserRoleUserRequset> GetUserRole(string usercode)
        {
            return Db.Context.FromSql("select A.*,B.UserName from (select a.RoleCode,a.RoleName,a.State,b.UserCode from TbRole a left join TbUserRole b on a.RoleCode=b.RoleCode) A left join TbUser B on A.UserCode=B.UserCode where B.UserCode=@ucode").AddInParameter("@ucode", DbType.String, usercode).ToList<TbUserRoleUserRequset>();
        }

        #endregion

        #region 员工插入、修改处理

        /// <summary>
        /// 新增公司信息数据
        /// </summary>
        public AjaxResult Insert(TbUser user, bool isApi = false)
        {
            if (user == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                //var any = Repository<TbUser>.Any(p=>p.UserName==user.UserName);
                //if(any)
                //{
                //    return AjaxResult.Warning("员工姓名重复！");
                //}
                //user.UserPwd = "6sV+kBAMUw4=";
                //var model = MapperHelper.Map<TbUser, TbUser>(user);
                //var count = Repository<TbUser>.Insert(model);
                //if (count <= 0)
                //    return AjaxResult.Error();
                Repository<TbUser>.Insert(user, isApi);
                var _id = Convert.ToInt32(Db.Context.FromSql("select Max(id) from TbUser").ToScalar());
                if (_id > 0)
                {
                    //查询最新插入员信息
                    var userEntity = Repository<TbUser>.First(d => d.ID == _id);
                    //创建用户和部门关联对象
                    TbUserAndDepRelevance td = new TbUserAndDepRelevance();
                    //赋值给用户和部门关联对象
                    td.DepartmentCode = user.DepartmentCode;
                    td.UserCode = userEntity.UserCode;
                    td.ComCode = userEntity.CompanyCode;
                    ////插入关联对象数据
                    //var userAndRel = MapperHelper.Map<TbUserAndDepRelevance, TbUserAndDepRelevance>(td);
                    //count = Repository<TbUserAndDepRelevance>.Insert(userAndRel);
                    //if (count <= 0)
                    //    return AjaxResult.Error();
                    Repository<TbUserAndDepRelevance>.Insert(td, isApi);
                }
                else
                {
                    return AjaxResult.Error();
                }
                return AjaxResult.Success();
            }
            catch (Exception e)
            {
                var err = e.ToString();
                return AjaxResult.Error(err);
            }
        }

        public AjaxResult InsertNew(List<TbUser> user, bool isApi = false)
        {
            if (user == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                List<TbUser> data = Db.Context.From<TbUser>().Select(TbUser._.All).ToList();
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //先删除原来的表
                    Db.Context.FromSql("truncate table TbUser").SetDbTransaction(trans).ExecuteNonQuery();
                    //插入从BM那边取过来的数据
                    Repository<TbUser>.Insert(trans, user, isApi);
                    trans.Commit();//提交事务
                    return AjaxResult.Success();
                }
            }
            catch (Exception e)
            {
                var err = e.ToString();
                return AjaxResult.Error(err);
            }
        }

        /// <summary>
        /// 修改数据
        /// </summary>
        public AjaxResult Update(TbUser user, bool isApi = false)
        {
            if (user == null)
                return AjaxResult.Warning("参数错误！");
            try
            {
                //    var any = Repository<TbUser>.Any(p => p.UserName == user.UserName && p.ID != user.ID);
                //    if (any)
                //    {
                //        return AjaxResult.Warning("员工姓名重复！");
                //    }
                //var model = MapperHelper.Map<TbUser, TbUser>(user);
                //var count = Repository<TbUser>.Update(model);
                //if (count <= 0)
                //{
                //    return AjaxResult.Error("修改员工信息失败！");
                //}
                Repository<TbUser>.Update(user, isApi);
                //var uptModel = Db.Context.FromSql("select * from TbUserAndDepRelevance where UserCode='" + user.UserCode + "'").First<TbUserAndDepRelevance>();
                var uptModel = Db.Context.From<TbUserAndDepRelevance>().Where(d => d.UserCode == user.UserCode).First<TbUserAndDepRelevance>();
                if (uptModel == null)
                {
                    TbUserAndDepRelevance td = new TbUserAndDepRelevance();
                    td.DepartmentCode = user.DepartmentCode;
                    td.ComCode = user.CompanyCode;
                    td.UserCode = user.UserCode;
                    //插入关联对象数据
                    var userAndRel = MapperHelper.Map<TbUserAndDepRelevance, TbUserAndDepRelevance>(td);
                    Repository<TbUserAndDepRelevance>.Insert(userAndRel);
                }
                else
                {
                    uptModel.DepartmentCode = user.DepartmentCode;
                    uptModel.ComCode = user.CompanyCode;
                    Repository<TbUserAndDepRelevance>.Update(uptModel, d => d.UserCode == user.UserCode, isApi);
                    //count = Db.Context.Update<TbUserAndDepRelevance>(uptModel, d => d.UserCode == user.UserCode);
                    //if (count <= 0)
                    //{
                    //    return AjaxResult.Error("修改员工关联信息失败！");
                    //}
                }
                return AjaxResult.Success();
            }
            catch (Exception e)
            {
                var err = e.ToString();
                return AjaxResult.Error(err);
            }
        }

        #endregion

        #region 删除处理

        /// <summary>
        /// 删除数据
        /// </summary>
        public AjaxResult Delete(string userCode)
        {
            try
            {
                var user = Db.Context.FromSql("select * from TbPositionUser where UserCode='" + userCode + "'").ToList<TbPositionUser>();
                if (user != null && user.Count > 0)
                {
                    return AjaxResult.Warning("员工已设置岗位信息，请处理后再删除！");
                }
                var role = Db.Context.FromSql("select * from TbUserRole where UserCode='" + userCode + "'").ToList<TbUserRole>();
                if (role != null && role.Count > 0)
                {
                    return AjaxResult.Warning("员工已分配角色，请处理后再删除！");
                }
                var count = Repository<TbUser>.Delete(t => t.UserCode == userCode);
                if (count <= 0)
                    return AjaxResult.Error();
                count = Repository<TbUserAndDepRelevance>.Delete(t => t.UserCode == userCode);
                if (count <= 0)
                    return AjaxResult.Error();
                return AjaxResult.Success(); ;
            }
            catch (Exception e)
            {
                var err = e.ToString();
                return AjaxResult.Error(err);
            }
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        public AjaxResult DeleteNew(string userCode, bool isApi = false)
        {
            try
            {
                var user = Db.Context.FromSql("select * from TbPositionUser where UserCode='" + userCode + "'").ToList<TbPositionUser>();
                if (user != null && user.Count > 0)
                {
                    return AjaxResult.Warning("员工已设置岗位信息，请处理后再删除！");
                }
                var role = Db.Context.FromSql("select * from TbUserRole where UserCode='" + userCode + "'").ToList<TbUserRole>();
                if (role != null && role.Count > 0)
                {
                    return AjaxResult.Warning("员工已分配角色，请处理后再删除！");
                }
                //var count = Repository<TbUser>.Delete(t => t.UserCode == userCode);
                //if (count <= 0)
                //    return AjaxResult.Error();
                //count = Repository<TbUserAndDepRelevance>.Delete(t => t.UserCode == userCode);
                //if (count <= 0)
                //    return AjaxResult.Error();
                Repository<TbUser>.Delete(t => t.UserCode == userCode, isApi);
                Repository<TbUserAndDepRelevance>.Delete(t => t.UserCode == userCode, isApi);
                return AjaxResult.Success(); ;
            }
            catch (Exception e)
            {
                var err = e.ToString();
                return AjaxResult.Error(err);
            }
        }

        #endregion

        #region 根据用户编码查询出对应的个人信息
        public TbUser UserFormSelectList(string UserCode)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("select ");
                sb.Append("a.UserCode,a.UserName,a.UserClosed,");
                sb.Append("a.Birthday,a.IDNumber,a.PoliticalLandscape,a.MobilePhone,a.Tell,a.Email,");
                sb.Append("a.QQ,a.WeChat,a.RecruitmentSource,a.PurchaseSocialSecurity,a.TheLaborContract,");
                sb.Append("a.ConfidentialityContract,a.CardNumber,a.CardBankName,a.CardBankAdd,a.CreateTime,a.CreateUser,");
                sb.Append("b.CompanyFullName as CompanyCodeName,a.CompanyCode,d.DepartmentName as DepartmentName,");
                sb.Append("d.DepartmentCode as DepartmentCode ");
                sb.Append("from TbUser a left join TbCompany b on a.CompanyCode=b.CompanyCode ");
                sb.Append("left join TbUserAndDepRelevance c on a.UserCode=c.UserCode ");
                sb.Append("left join TbDepartment d on c.DepartmentCode=d.DepartmentCode where");
                sb.Append(" a.UserCode = @name");
                return Db.Context.FromSql(sb.ToString()).AddInParameter(
                    "@name", DbType.String, UserCode).ToFirstDefault<TbUser>();
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region 根据用户编码修改个人信息
        public AjaxResult UserUpdateSubmitForm(TbUser user)
        {
            try
            {
                var data = Repository<TbUser>.First(m => m.UserCode == user.UserCode);
                data.UserName = user.UserName;
                data.Birthday = user.Birthday;
                data.UserClosed = user.UserClosed;
                data.PoliticalLandscape = user.PoliticalLandscape;
                data.MobilePhone = user.MobilePhone;
                data.Tell = user.Tell;
                data.Email = user.Email;
                data.QQ = user.QQ;
                data.WeChat = user.WeChat;
                data.RecruitmentSource = user.RecruitmentSource;
                data.PurchaseSocialSecurity = user.PurchaseSocialSecurity;
                data.TheLaborContract = user.TheLaborContract;
                data.ConfidentialityContract = user.ConfidentialityContract;
                data.CreateTime = user.CreateTime;
                data.CompanyCode = user.CompanyCode;
                data.DepartmentCode = user.DepartmentCode;
                var count = Repository<TbUser>.Update(data);
                if (count <= 0)
                {
                    return AjaxResult.Success("个人信息修改失败，请重试！");
                }
                return AjaxResult.Success("个人信息修改成功！");

            }
            catch (Exception)
            {
                return AjaxResult.Error("个人信息修改失败，请重试！");
            }
        }
        #endregion

        #region 确认原密码，如果原密码输入错误，禁止用户操作
        public TbUser SelectPwd(string UserCode)
        {
            var model = Repository<TbUser>.First(m => m.UserCode == UserCode);
            var password = PM.Common.Encryption.EncryptionFactory.DecryptDES(model.UserPwd, "QWERTYUIOP");
            model.UserPwd = password;
            return model;
        }
        #endregion

        #region 修改密码按钮事件
        public AjaxResult UpDatePwd(UserRequest result, string UserCode)
        {
            var pwd = PM.Common.Encryption.EncryptionFactory.EncryptDES(result.UserPwd, "QWERTYUIOP");
            try
            {
                var model = Repository<TbUser>.First(m => m.UserCode == UserCode);
                model.UserPwd = pwd;
                var data = Repository<TbUser>.Update(model);
                if (data > 0)
                {
                    return AjaxResult.Success("密码修改成功，请重新进行登陆！");
                }
                return AjaxResult.Error("密码修改失败，请重试！");
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region 查询此用户是否是离职人员
        public string UserClosedSelect(string userName)
        {
            try
            {
                //判断用户名是否存在
                if (!string.IsNullOrWhiteSpace(userName))
                {
                    string sql = @"SELECT UserClosed FROM TbUser WHERE UserCode=@UserName";
                    var data = Db.Context.FromSql(sql).AddInParameter("@UserName", DbType.String, userName).ToList<TbUser>();
                    var Closed = data[0].UserClosed;
                    if (Closed == "在职")
                    {
                        return "1";
                    }
                }
                return "-1";
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region 获取该用户下所有的项目组织机构

        public List<ProjectOrg> GetAllProjectOrg(string UserCode)
        {
            if (!string.IsNullOrWhiteSpace(UserCode))
            {
                List<ProjectOrg> list = new List<ProjectOrg>();
                string psql = @"select ur.ProjectId,p.ProjectName from TbUserRole ur left join TbProjectInfo p on ur.ProjectId=p.ProjectId where UserCode=@UserCode and Flag=0 group by ur.ProjectId,p.ProjectName";
                var pret = Db.Context.FromSql(psql).AddInParameter("@UserCode", DbType.String, UserCode).ToDataTable();
                if (pret != null && pret.Rows.Count > 0)
                {
                    for (int p = 0; p < pret.Rows.Count; p++)
                    {
                        ProjectOrg model = new ProjectOrg();
                        model.OrgId = pret.Rows[p]["ProjectId"].ToString();//id
                        model.OrgName = pret.Rows[p]["ProjectName"].ToString();
                        model.ProjectId = "0";//pid
                        model.ProjectName = "";
                        model.ProjectOrgAllId = pret.Rows[p]["ProjectId"].ToString();
                        model.ProjectOrgAllName = pret.Rows[p]["ProjectName"].ToString();
                        model.OrgType = "-1";
                        list.Add(model);
                    }
                }
                string sql = @"select ur.ProjectId,pro.ProjectName,ur.OrgId,cp.CompanyFullName as OrgName,cp.ParentCompanyCode,ur.OrgType from TbUserRole ur
left join TbCompany cp on ur.OrgId=cp.CompanyCode and ur.OrgType=cp.OrgType
left join TbProjectInfo pro on ur.ProjectId=pro.ProjectId
where UserCode=@UserCode and Flag=0 group by ur.ProjectId,pro.ProjectName,ur.OrgId,cp.CompanyFullName,cp.ParentCompanyCode,ur.OrgType;";
                var ret = Db.Context.FromSql(sql).AddInParameter("@UserCode", DbType.String, UserCode).ToDataTable();
                if (ret != null && ret.Rows.Count > 0)
                {
                    for (int i = 0; i < ret.Rows.Count; i++)
                    {
                        ProjectOrg model = new ProjectOrg();
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
                        var ret1 = Db.Context.FromSql(sql1).AddInParameter("@CompanyCode", DbType.String, ret.Rows[i]["OrgId"]).ToDataTable();
                        if (ret1 != null && ret1.Rows.Count > 0)
                        {
                            for (int j = 0; j < ret1.Rows.Count; j++)
                            {
                                if (ret1.Rows.Count == 1)
                                {
                                    ProjectOrgAllId += ret1.Rows[j]["CompanyCode"];
                                    ProjectOrgAllName += ret1.Rows[j]["CompanyFullName"];
                                }
                                else
                                {
                                    if (ret1.Rows.Count != j + 1)
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
                        }
                        model.OrgId = ret.Rows[i]["OrgId"].ToString();//id
                        model.OrgName = ret.Rows[i]["OrgName"].ToString();
                        model.ProjectId = ret.Rows[i]["ProjectId"].ToString();//pid
                        model.ProjectName = ret.Rows[i]["ProjectName"].ToString();
                        model.ProjectOrgAllId = ProjectOrgAllId;
                        model.ProjectOrgAllName = ProjectOrgAllName;
                        model.OrgType = ret.Rows[i]["OrgType"].ToString();
                        list.Add(model);
                    }
                }
                return list;
            }
            else
            {
                return null;
            }

        }
        /// <summary>
        /// 切换项目组织机构重新保存切换后的组织机构信息到登陆信息中
        /// </summary>
        /// <param name="UserCode"></param>
        /// <param name="OrgId"></param>
        /// <param name="OrgName"></param>
        /// <param name="ProjectId"></param>
        /// <param name="OrgType"></param>
        /// <param name="ProjectOrgAllId"></param>
        /// <param name="ProjectOrgAllName"></param>
        /// <returns></returns>
        public CurrentUserInfo SaveProjectOrg(string UserCode, string OrgId, string OrgName, string ProjectId, string OrgType, string ProjectOrgAllId, string ProjectOrgAllName)
        {
            try
            {
                string sqlRole = @"select distinct ur.RoleCode as RoleId,r.RoleCode,r.RoleName from TbUserRole ur
left join TbUser u on ur.UserCode=u.UserId
left join TbRole r on ur.RoleCode=r.RoleId
where u.UserCode=@UserCode and ProjectId=@ProjectId and OrgId=@OrgId and ur.Flag=0";
                string strRole = "";
                DataTable dtRole = Db.Context.FromSql(sqlRole).AddInParameter("@UserCode", DbType.String, UserCode).AddInParameter("@ProjectId", DbType.String, ProjectId).AddInParameter("@OrgId", DbType.String, OrgId).ToDataTable();
                if (dtRole != null && dtRole.Rows.Count > 0)
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
                CurrentUserInfo cui = new CurrentUserInfo();
                cui.OrgType = OrgType;
                cui.CompanyId = OrgId;
                cui.ComPanyName = OrgName;
                cui.ProjectOrgAllId = ProjectOrgAllId;
                cui.ProjectOrgAllName = ProjectOrgAllName;
                if (OrgType == "1")
                {
                    cui.ProjectId = "";
                    cui.ProcessFactoryCode = OrgId;
                    cui.ProcessFactoryName = OrgName;
                }
                else
                {
                    cui.ProjectId = ProjectId;
                    //cui.ProcessFactoryCode = "";
                    //cui.ProcessFactoryName = "所有加工厂";
                    cui.ProcessFactoryCode = OperatorProvider.Provider.CurrentUser.ProcessFactoryCode;
                    cui.ProcessFactoryName = OperatorProvider.Provider.CurrentUser.ProcessFactoryName;
                }
                cui.RoleCode = strRole;
                cui.UserCode = OperatorProvider.Provider.CurrentUser.UserCode;
                cui.UserId = OperatorProvider.Provider.CurrentUser.UserId;
                cui.UserName = OperatorProvider.Provider.CurrentUser.UserName;
                cui.LoginTime = OperatorProvider.Provider.CurrentUser.LoginTime;
                cui.LoginToken = OperatorProvider.Provider.CurrentUser.LoginToken;
                cui.IsSystem = OperatorProvider.Provider.CurrentUser.IsSystem;
                cui.IsDriver = OperatorProvider.Provider.CurrentUser.IsDriver;
                return cui;
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        /// <summary>
        /// 切换项目组织机构重新保存切换后的组织机构信息到登陆信息中
        /// </summary>
        /// <param name="UserCode"></param>
        /// <param name="OrgId"></param>
        /// <param name="OrgName"></param>
        /// <param name="ProjectId"></param>
        /// <param name="OrgType"></param>
        /// <param name="ProjectOrgAllId"></param>
        /// <param name="ProjectOrgAllName"></param>
        /// <returns></returns>
        public CurrentUserInfo AppSaveProjectOrg(CurrentUserInfo model)
        {
            try
            {
                string sqlRole = @"select distinct ur.RoleCode as RoleId,r.RoleCode,r.RoleName from TbUserRole ur
left join TbUser u on ur.UserCode=u.UserId
left join TbRole r on ur.RoleCode=r.RoleId
where u.UserCode=@UserCode and ProjectId=@ProjectId and OrgId=@OrgId and ur.Flag=0";
                string strRole = "";
                DataTable dtRole = Db.Context.FromSql(sqlRole).AddInParameter("@UserCode", DbType.String, model.UserCode)
                    .AddInParameter("@ProjectId", DbType.String, model.ProjectId)
                    .AddInParameter("@OrgId", DbType.String, model.CompanyId).ToDataTable();
                if (dtRole != null && dtRole.Rows.Count > 0)
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
                CurrentUserInfo cui = new CurrentUserInfo();
                cui.OrgType = model.OrgType;
                cui.CompanyId = model.CompanyId;
                cui.ComPanyName = model.CompanyId;
                cui.ProjectOrgAllId = model.ProjectOrgAllId;
                cui.ProjectOrgAllName = model.ProjectOrgAllName;
                if (model.OrgType == "1")
                {
                    cui.ProjectId = "";
                    cui.ProcessFactoryCode = model.CompanyId;
                    cui.ProcessFactoryName = model.CompanyId;
                }
                else
                {
                    cui.ProjectId = model.ProjectId;
                    //cui.ProcessFactoryCode = "";
                    //cui.ProcessFactoryName = "所有加工厂";
                    cui.ProcessFactoryCode = model.CompanyId;
                    cui.ProcessFactoryName = model.CompanyId;
                }
                cui.RoleCode = strRole;
                cui.UserCode = model.UserCode;
                cui.UserId = model.UserId;
                cui.UserName = model.UserName;
                cui.LoginTime = model.LoginTime;
                cui.LoginToken = model.LoginToken;
                cui.IsSystem = model.IsSystem;
                cui.IsDriver = model.IsDriver;
                return cui;
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public PageModel GetAllProjectJgc(PageSearchRequest request)
        {
            //参数化
            List<Parameter> parameter = new List<Parameter>();
//            string sql = @" select 1 as ID,'' as ProcessFactoryCode,'所有加工厂' as ProcessFactoryName 
//                            union all
//                            select id,CompanyCode,CompanyFullName from TbCompany where OrgType=1 ";
            string sql = @"select id as ID,CompanyCode as ProcessFactoryCode,CompanyFullName as ProcessFactoryName from TbCompany where OrgType=1 ";
            var model = Repository<TbCompany>.FromSqlToPageTable(sql, parameter, request.rows, request.page, "ID", "asc");
            return model;
        }

        /// <summary>
        /// 切换加工厂重新保存切换后的加工厂信息到登陆信息中
        /// </summary>
        /// <param name="ProcessFactoryCode"></param>
        /// <returns></returns>
        public CurrentUserInfo SaveProcessFactoryCode(string ProcessFactoryCode, string ProcessFactoryName)
        {
            try
            {
                CurrentUserInfo cui = new CurrentUserInfo();
                cui.OrgType = OperatorProvider.Provider.CurrentUser.OrgType;
                cui.CompanyId = OperatorProvider.Provider.CurrentUser.CompanyId;
                cui.ComPanyName = OperatorProvider.Provider.CurrentUser.ComPanyName;
                cui.ProjectOrgAllId = OperatorProvider.Provider.CurrentUser.ProjectOrgAllId;
                cui.ProjectOrgAllName = OperatorProvider.Provider.CurrentUser.ProjectOrgAllName;
                cui.ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
                cui.RoleCode = OperatorProvider.Provider.CurrentUser.RoleCode;
                cui.UserCode = OperatorProvider.Provider.CurrentUser.UserCode;
                cui.UserId = OperatorProvider.Provider.CurrentUser.UserId;
                cui.UserName = OperatorProvider.Provider.CurrentUser.UserName;
                cui.LoginTime = OperatorProvider.Provider.CurrentUser.LoginTime;
                cui.LoginToken = OperatorProvider.Provider.CurrentUser.LoginToken;
                cui.IsSystem = OperatorProvider.Provider.CurrentUser.IsSystem;
                cui.IsDriver = OperatorProvider.Provider.CurrentUser.IsDriver;
                cui.ProcessFactoryCode = ProcessFactoryCode;
                cui.ProcessFactoryName = ProcessFactoryName;
                return cui;
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        /// <summary>
        /// 切换加工厂重新保存切换后的加工厂信息到登陆信息中
        /// </summary>
        /// <param name="ProcessFactoryCode"></param>
        /// <returns></returns>
        public CurrentUserInfo AppSaveProcessFactoryCode(CurrentUserInfo model)
        {
            try
            {
                CurrentUserInfo cui = new CurrentUserInfo();
                cui = model;
                return cui;
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        #endregion

        #region 首页饼图预警信息
        /// <summary>
        /// 首页报警信息分析
        /// </summary>
        /// <returns></returns>
        public DataTable GetAlarmMessage(HomeRequest request)
        {

            #region 审批预警条件

            string where1 = "";
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where1 += " and a.ProjectId='" + request.ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where1 += " and a.ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);//站点
                List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 4);//工区
                string siteStr = string.Join("','", SiteList);
                string workAreaStr = string.Join("','", WorkAreaList);
                where1 += (" and (a.SiteCode in('" + siteStr + "') or a.WorkAreaCode in('" + workAreaStr + "'))");
            }

            #endregion

            #region 未报月度需求计划预警

            string where2 = "";
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where2 += " and fwqi.ProjectId='" + request.ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);//站点
                List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 4);//工区
                string siteStr = string.Join("','", SiteList);
                string workAreaStr = string.Join("','", WorkAreaList);
                where2 += (" and (fwqi.SiteCode in('" + siteStr + "') or fwqi.WorkArea in('" + workAreaStr + "'))");
            }

            #endregion

            #region 原材料供货超时

            string where3 = "";
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where3 += " and sup.ProjectId='" + request.ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where3 += " and sup.ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);//站点
                List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 4);//工区
                string siteStr = string.Join("','", SiteList);
                string workAreaStr = string.Join("','", WorkAreaList);
                where3 += (" and (sup.SiteCode in('" + siteStr + "') or sup.WorkAreaCode in('" + workAreaStr + "'))");
            }

            #endregion

            #region 加急订单

            string where4 = "";
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where4 += " and wo.ProjectId='" + request.ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where4 += " and wo.ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);//站点
                string siteStr = string.Join("','", SiteList);
                where4 += (" and wo.SiteCode in('" + siteStr + "') ");
            }

            #endregion

            #region 加工进度滞后

            string where5 = "";
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where5 += " and op.ProjectId='" + request.ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where5 += " and op.ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);//站点
                string siteStr = string.Join("','", SiteList);
                where5 += (" and op.SiteCode in('" + siteStr + "') ");
            }

            #endregion

            #region 配送超期

            string where6 = "";
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where6 += " and dpi.ProjectId='" + request.ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where6 += " and dpi.ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);//站点
                string siteStr = string.Join("','", SiteList);
                where6 += (" and dpi.SiteCode in('" + siteStr + "') ");
            }

            #endregion

            #region 卸货超时

            string where7 = "";
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where7 += " and TbYj.ProjectId='" + request.ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where7 += " and disEnt.ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);//站点
                string siteStr = string.Join("','", SiteList);
                where7 += (" and TbYj.SiteCode in('" + siteStr + "') ");
            }

            #endregion

            #region 签收超时

            string where8 = "";
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where8 += " and TbYj.ProjectId='" + request.ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where8 += " and sfs.ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);//站点
                string siteStr = string.Join("','", SiteList);
                where8 += (" and TbYj.SiteCode in('" + siteStr + "') ");
            }

            #endregion

            string sql = @"SELECT 
                            '审批超时' as YjTypeName,COUNT(1) as YjNum
                            FROM (
									SELECT a.*, row_number()over(partition BY a.FlowCode ORDER BY a.FlowNodeCode,a.EarlyWarningTime desc) as group_idx 
									FROM TbFlowEarlyWarningOtherInfo  a
									INNER JOIN  TbFlowEarlyWarningInfo b ON a.FlowCode=b.FlowCode 
									AND a.EWFormDataCode=b.EWFormDataCode AND a.FlowNodeCode=b.FlowNodeCode 
									WHERE  b.EarlyWarningStart=0 AND YEAR(a.EarlyWarningTime)= @Year and MONTH(a.EarlyWarningTime)= @Month " + where1 + @"
							   ) Tb1
                            WHERE Tb1.group_idx =1
                           union all
                            select '未报月度需求计划' as YjTypeName,COUNT(1)as YjNum
                             FROM ( 
                                  SELECT *, row_number()over(partition BY EarlyWarningCode,EWFormDataCode ORDER BY EWNodeCode,EWTime desc) as group_idx 
                                  FROM TbFormEarlyWarningNodeInfo fwqi
                                  WHERE fwqi.MenuCode='RawMonthDemandPlan' AND fwqi.EWStart=0 AND YEAR(fwqi.EWTime)= @Year and MONTH(fwqi.EWTime)= @Month " + where2 + @"  
                             ) Tb2
                             WHERE Tb2.group_idx =1
                           union all
                           select '原材料供货超时' as YjTypeName,COUNT(1) as YjNum from (
                           select BatchPlanNum,cp1.CompanyFullName as BranchName,cp2.CompanyFullName as WorkAreaName,cp3.CompanyFullName as SiteName,cp4.CompanyFullName as ProcessFactoryName,sup.SupplyDate,sup.SupplyCompleteTime,sup.ProjectId from TbSupplyList sup
                           left join TbCompany cp1 on cp1.CompanyCode=sup.BranchCode
                           left join TbCompany cp2 on cp2.CompanyCode=sup.WorkAreaCode
                           left join TbCompany cp3 on cp3.CompanyCode=sup.SiteCode
                           left join TbCompany cp4 on cp4.CompanyCode=sup.ProcessFactoryCode
                           where (sup.SupplyCompleteTime>sup.SupplyDate or (GETDATE()>sup.SupplyDate and sup.SupplyCompleteTime is null))
						   and YEAR(sup.SupplyDate)= @Year and MONTH(sup.SupplyDate)= @Month " + where3 + @") Tb3
                           union all
                           select '加急订单' as YjTypeName,COUNT(1) as YjNum from (--加急订单个数
                           select wo.OrderCode,wo.DistributionTime,cp1.CompanyFullName as SiteName,cp2.CompanyFullName as ProcessFactoryName from TbWorkOrder wo
                           left join TbCompany cp1 on wo.SiteCode=cp1.CompanyCode
                           left join TbCompany cp2 on wo.ProcessFactoryCode=cp2.CompanyCode
                           where UrgentDegree='Urgent' 
						   and YEAR(wo.DistributionTime)= @Year and MONTH(wo.DistributionTime)= @Month " + where4 + @") Tb4
                           union all
                           select '加工进度滞后' as YjTypeName,COUNT(1) as YjNum from (--加工进度滞后
                           select op.OrderCode,cp1.CompanyFullName as SiteName,cp2.CompanyFullName as ProcessFactoryName,op.DistributionTime,op.FinishProcessingDateTime from TbOrderProgress op 
                           left join TbCompany cp1 on op.SiteCode=cp1.CompanyCode
                           left join TbCompany cp2 on op.ProcessFactoryCode=cp2.CompanyCode
                           where YEAR(op.DistributionTime)= @Year and MONTH(op.DistributionTime)= @Month
                           and (op.FinishProcessingDateTime>op.DistributionTime or (GETDATE()>op.DistributionTime and op.FinishProcessingDateTime is null)) " + where5 + @") Tb5
                           union all
                           select '接收配送超期' as YjTypeName,COUNT(1) as YjNum from (--配送超期
                           select dpi.OrderCode,cp1.CompanyFullName as SIteName,cp2.CompanyFullName as ProcessFactoryName,dpi.DistributionTime,dpi.DeliveryCompleteTime from TbDistributionPlanInfo dpi
                           left join TbCompany cp1 on dpi.SiteCode=cp1.CompanyCode
                           left join TbCompany cp2 on dpi.ProcessFactoryCode=cp2.CompanyCode
                           where YEAR(dpi.DistributionTime)= @Year and MONTH(dpi.DistributionTime)= @Month
                           and (dpi.DeliveryCompleteTime>dpi.DistributionTime or (GETDATE()>dpi.DistributionTime and dpi.DeliveryCompleteTime is null)) " + where6 + @") Tb6 
                           union all
                           select '卸货超时' as YjTypeName,COUNT(1) as YjNum from(--卸货超时
                           select TbYj.*,cp1.CompanyFullName as SiteName,tcr.EnterSpaceTime,tcr.StartDischargeTime,tcr.EndDischargeTime,disEnt.ProcessFactoryCode,cp.CompanyFullName as ProcessFactoryName from (select min(EwTime) as EwTime,MenuCode,EWFormDataCode,CompanyCode,WorkArea,SiteCode,ProjectId from TbFormEarlyWarningNodeInfo where  MenuCode='SiteDischargeCargo' 
						   group by MenuCode,EWFormDataCode,CompanyCode,WorkArea,SiteCode,ProjectId) TbYj
						   left join TbTransportCarReport tcr on TbYj.EWFormDataCode=tcr.DisEntOrderId
                           left join TbDistributionEnt disEnt on tcr.DistributionCode=disEnt.DistributionCode
                           left join TbCompany cp on disEnt.ProcessFactoryCode=cp.CompanyCode
                           left join TbCompany cp1 on TbYj.SiteCode=cp1.CompanyCode
						   where YEAR(TbYj.EWTime)= @Year and MONTH(TbYj.EWTime)= @Month " + where7 + @") Tb7
                           union all
                           select '签收超时' as YjTypeName,COUNT(1) as YjNum from (select TbYj.*,cp1.CompanyFullName as SiteName,sfs.DistributionCode,sdc.DistributionTime,sfs.SigninTime,sfs.ProcessFactoryCode,cp.CompanyFullName as ProcessFactoryName from (select min(EwTime) as EwTime,MenuCode,EWFormDataCode,CompanyCode,WorkArea,SiteCode,ProjectId from TbFormEarlyWarningNodeInfo where  MenuCode='SemiFinishedSign' 
						   group by MenuCode,EWFormDataCode,CompanyCode,WorkArea,SiteCode,ProjectId) TbYj
						   left join TbSemiFinishedSign sfs on TbYj.EWFormDataCode=sfs.ID
                           left join TbSiteDischargeCargo sdc on sfs.DistributionCode=sdc.DistributionCode and sfs.DischargeCargoCode=sdc.DischargeCargoCode
                           left join TbCompany cp on sfs.ProcessFactoryCode=cp.CompanyCode
                           left join TbCompany cp1 on TbYj.SiteCode=cp1.CompanyCode
						   where YEAR(TbYj.EWTime)= @Year and MONTH(TbYj.EWTime)= @Month " + where8 + @") Tb8";
            var ret = Db.Context.FromSql(sql)
                .AddInParameter("@Year", DbType.String, request.Year)
                .AddInParameter("@Month", DbType.String, request.Month).ToDataTable();
            return ret;
        }

        /// <summary>
        /// 审批超时
        /// </summary>
        /// <param name="Month">月份</param>
        /// <returns></returns>
        public DataTable GetSpCsYjList(HomeRequest request)
        {
            string where = " where 1=1";
            string sql = @"
                        select fewoi.DataCode as OddNumbers,
                         fewoi.EarlyWarningContent,
                         fewoi.EWFormDataCode,
                         CONVERT(varchar(100), fewoi.EarlyWarningTime, 120) as EarlyWarningTime,
                         EWFormCode,sm.MenuName,fn.FlowNodeName,cp1.CompanyFullName as BranchName,
                         cp2.CompanyFullName as WorkAreaName,cp3.CompanyFullName as SiteName,cp4.CompanyFullName as ProcessFactoryName,
                         u1.UserName as FlowSpUserName,u2.UserName as FlowYjUserName 
                        FROM (
                        select*
                           FROM ( 
                        		SELECT a.*, row_number()over(partition BY a.FlowCode ORDER BY a.FlowNodeCode,a.EarlyWarningTime desc) as group_idx 
                        		FROM TbFlowEarlyWarningOtherInfo  a
                        		INNER JOIN  TbFlowEarlyWarningInfo b ON a.FlowCode=b.FlowCode 
                        		AND a.EWFormDataCode=b.EWFormDataCode AND a.FlowNodeCode=b.FlowNodeCode 
                        		WHERE YEAR(a.EarlyWarningTime)= @Year and MONTH(a.EarlyWarningTime)= @Month AND b.EarlyWarningStart=0
                           ) ret
                        WHERE ret.group_idx =1
                        ) fewoi
                        left join TbFlowNode fn on fewoi.FlowCode=fn.FlowCode and fewoi.FlowNodeCode=fn.FlowNodeCode
                        left join TbSysMenu sm on fewoi.EWFormCode=sm.MenuCode
                        left join TbCompany cp1 on fewoi.BranchCode=cp1.CompanyCode
                        left join TbCompany cp2 on fewoi.WorkAreaCode=cp2.CompanyCode
                        left join TbCompany cp3 on fewoi.SiteCode=cp3.CompanyCode
                        left join TbCompany cp4 on fewoi.ProcessFactoryCode=cp4.CompanyCode
                        left join TbUser u1 on fewoi.FlowSpUserCode=u1.UserId
                        left join TbUser u2 on fewoi.FlowYjUserCode=u2.UserId";
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where += " and fewoi.ProjectId='" + request.ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where += " and fewoi.ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);//站点
                List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 4);//工区
                string siteStr = string.Join("','", SiteList);
                string workAreaStr = string.Join("','", WorkAreaList);
                where += (" and (fewoi.SiteCode in('" + siteStr + "') or fewoi.WorkAreaCode in('" + workAreaStr + "'))");
            }
            var ret = Db.Context.FromSql(sql + where + " ORDER BY fewoi.EarlyWarningTime desc")
                .AddInParameter("@Year", DbType.String, request.Year)
                .AddInParameter("@Month", DbType.String, request.Month).ToDataTable();
            return ret;
        }

        /// <summary>
        /// 未按时提报月度需求计划
        /// </summary>
        /// <param name="Year"></param>
        /// <param name="Month"></param>
        /// <returns></returns>
        public DataTable GetWbydxqjhYjList(HomeRequest request)
        {
            string where = " where 1=1";
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where += " and fwqi.ProjectId='" + request.ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);//站点
                List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 4);//工区
                string siteStr = string.Join("','", SiteList);
                string workAreaStr = string.Join("','", WorkAreaList);
                where += (" and (fwqi.SiteCode in('" + siteStr + "') or fwqi.WorkArea in('" + workAreaStr + "'))");
            }

            string sql = @"SELECT sm.MenuName,fwqi.EWContent,fwqi.EWUserCode,u.UserName,fwqi.EWFormDataCode,cp1.CompanyFullName as  FbName,cp2.CompanyFullName as GqName,fwqi.EWTime,sdd.DictionaryText as RebarType  
                   FROM (  
                    select ret.*
                    FROM ( 
                         SELECT *, row_number()over(partition BY EarlyWarningCode,EWFormDataCode ORDER BY EWNodeCode,EWTime desc) as group_idx 
                         FROM TbFormEarlyWarningNodeInfo fwqi
                         WHERE YEAR(fwqi.EWTime)= @Year and MONTH(fwqi.EWTime)= @Month AND fwqi.MenuCode='RawMonthDemandPlan' AND fwqi.EWStart=0
                    ) ret
                    WHERE ret.group_idx = 1
                   ) fwqi
                    left join TbSysMenu sm on fwqi.MenuCode=sm.MenuCode
                            left join TbCompany cp1 on fwqi.CompanyCode=cp1.CompanyCode
                            left join TbCompany cp2 on fwqi.WorkArea=cp2.CompanyCode
                            left join TbCompany cp3 on fwqi.SiteCode=cp3.CompanyCode
                            left join TbUser u on fwqi.EWUserCode=u.UserId
                            left join TbRawMaterialMonthDemandPlan rmp on fwqi.EWFormDataCode=rmp.ID
                            left join TbSysDictionaryData sdd on sdd.DictionaryCode=rmp.RebarType
                   ";
            var ret = Db.Context.FromSql(sql + where + " ORDER BY fwqi.EWTime DESC")
                .AddInParameter("@Year", DbType.String, request.Year)
                .AddInParameter("@Month", DbType.String, request.Month).ToDataTable();
            return ret;
        }

        /// <summary>
        /// 原材料供货超时
        /// </summary>
        /// <param name="Year"></param>
        /// <param name="Month"></param>
        /// <returns></returns>
        public DataTable GetYclghCsYjList(HomeRequest request)
        {
            string where = "";
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where += " and sup.ProjectId='" + request.ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where += " and sup.ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);//站点
                List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 4);//工区
                string siteStr = string.Join("','", SiteList);
                string workAreaStr = string.Join("','", WorkAreaList);
                where += (" and (sup.SiteCode in('" + siteStr + "') or sup.WorkAreaCode in('" + workAreaStr + "'))");
            }
            string sql = @"--原材料供货超时
                           select BatchPlanNum,cp1.CompanyFullName as BranchName,cp2.CompanyFullName as WorkAreaName,cp3.CompanyFullName as SiteName,cp4.CompanyFullName as ProcessFactoryName,sup.SupplyDate,sup.SupplyCompleteTime,sup.ProjectId,sup.Remarks from TbSupplyList sup
                           left join TbCompany cp1 on cp1.CompanyCode=sup.BranchCode
                           left join TbCompany cp2 on cp2.CompanyCode=sup.WorkAreaCode
                           left join TbCompany cp3 on cp3.CompanyCode=sup.SiteCode
                           left join TbCompany cp4 on cp4.CompanyCode=sup.ProcessFactoryCode
                           where YEAR(sup.SupplyDate)= @Year and MONTH(sup.SupplyDate)= @Month
                           and (sup.SupplyCompleteTime>sup.SupplyDate or (GETDATE()>sup.SupplyDate and sup.SupplyCompleteTime is null)) ";
            var ret = Db.Context.FromSql(sql+where)
                .AddInParameter("@Year", DbType.String, request.Year)
                .AddInParameter("@Month", DbType.String, request.Month).ToDataTable();
            return ret;
        }

        /// <summary>
        /// 加急订单个数
        /// </summary>
        /// <param name="Year"></param>
        /// <param name="Month"></param>
        /// <returns></returns>
        public DataTable GetJjOrderYjList(HomeRequest request)
        {
            string where = "";
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where += " and wo.ProjectId='" + request.ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where += " and wo.ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);//站点
                string siteStr = string.Join("','", SiteList);
                where += (" and wo.SiteCode in('" + siteStr + "') ");
            }
            string orderBy = " ORDER BY wo.DistributionTime desc";
            string sql = @"--加急订单个数
                           select wo.OrderCode,wo.DistributionTime,cp1.CompanyFullName as SiteName,cp2.CompanyFullName as ProcessFactoryName from TbWorkOrder wo
                           left join TbCompany cp1 on wo.SiteCode=cp1.CompanyCode
                           left join TbCompany cp2 on wo.ProcessFactoryCode=cp2.CompanyCode
                           where UrgentDegree='Urgent'
                           and YEAR(wo.DistributionTime)= @Year and MONTH(wo.DistributionTime)= @Month ";
            var ret = Db.Context.FromSql(sql + where + orderBy)
                .AddInParameter("@Year", DbType.String, request.Year)
                .AddInParameter("@Month", DbType.String, request.Month).ToDataTable();
            return ret;
        }

        /// <summary>
        /// 加工进度滞后
        /// </summary>
        /// <param name="Year"></param>
        /// <param name="Month"></param>
        /// <returns></returns>
        public DataTable GetJgOrderZhYjList(HomeRequest request)
        {
            string where = "";
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where += " and op.ProjectId='" + request.ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where += " and op.ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);//站点
                string siteStr = string.Join("','", SiteList);
                where += (" and op.SiteCode in('" + siteStr + "')");
            }
            string orderBy = " ORDER BY op.DistributionTime desc";
            string sql = @"--加工进度滞后
                           select op.OrderCode,cp1.CompanyFullName as SiteName,cp2.CompanyFullName as ProcessFactoryName,op.DistributionTime,op.FinishProcessingDateTime from TbOrderProgress op 
                           left join TbCompany cp1 on op.SiteCode=cp1.CompanyCode
                           left join TbCompany cp2 on op.ProcessFactoryCode=cp2.CompanyCode
                           where YEAR(op.DistributionTime)= @Year and MONTH(op.DistributionTime)= @Month
                           and (op.FinishProcessingDateTime>op.DistributionTime or (GETDATE()>op.DistributionTime and op.FinishProcessingDateTime is null)) ";
            var ret = Db.Context.FromSql(sql + where + orderBy)
                .AddInParameter("@Year", DbType.String, request.Year)
                .AddInParameter("@Month", DbType.String, request.Month).ToDataTable();
            return ret;
        }

        /// <summary>
        /// 配送超期
        /// </summary>
        /// <param name="Year"></param>
        /// <param name="Month"></param>
        /// <returns></returns>
        public DataTable GetPsCsYjList(HomeRequest request)
        {
            string where = "";
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where += " and dpi.ProjectId='" + request.ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where += " and dpi.ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);//站点
                string siteStr = string.Join("','", SiteList);
                where += (" and dpi.SiteCode in('" + siteStr + "')");
            }
            string orderBy = " ORDER BY dpi.DistributionTime desc";
            string sql = @"--配送超期
                           select dpi.OrderCode,cp1.CompanyFullName as SiteName,cp2.CompanyFullName as ProcessFactoryName,dpi.DistributionTime,dpi.DeliveryCompleteTime,dpi.Remark from TbDistributionPlanInfo dpi
                           left join TbCompany cp1 on dpi.SiteCode=cp1.CompanyCode
                           left join TbCompany cp2 on dpi.ProcessFactoryCode=cp2.CompanyCode
                           where YEAR(dpi.DistributionTime)= @Year and MONTH(dpi.DistributionTime)= @Month
                           and (dpi.DeliveryCompleteTime>dpi.DistributionTime or (GETDATE()>dpi.DistributionTime and dpi.DeliveryCompleteTime is null)) ";
            var ret = Db.Context.FromSql(sql + where + orderBy)
                .AddInParameter("@Year", DbType.String, request.Year)
                .AddInParameter("@Month", DbType.String, request.Month).ToDataTable();
            return ret;
        }

        /// <summary>
        /// 卸货超期
        /// </summary>
        /// <param name="Year"></param>
        /// <param name="Month"></param>
        /// <returns></returns>
        public DataTable GetXhCsYjList(HomeRequest request)
        {
            string where = "";
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where += " and TbYj.ProjectId='" + request.ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where += " and disEnt.ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);//站点
                string siteStr = string.Join("','", SiteList);
                where += (" and TbYj.SiteCode in('" + siteStr + "')");
            }
            string orderBy = " ORDER BY tcr.EnterSpaceTime desc";
            string sql = @"select TbYj.*,cp1.CompanyFullName as SiteName,tcr.EnterSpaceTime,tcr.StartDischargeTime,tcr.EndDischargeTime,disEnt.ProcessFactoryCode,cp.CompanyFullName as ProcessFactoryName from (select min(EwTime) as EwTime,MenuCode,EWFormDataCode,CompanyCode,WorkArea,SiteCode,ProjectId from TbFormEarlyWarningNodeInfo where  MenuCode='SiteDischargeCargo' 
						   group by MenuCode,EWFormDataCode,CompanyCode,WorkArea,SiteCode,ProjectId) TbYj
						   left join TbTransportCarReport tcr on TbYj.EWFormDataCode=tcr.DisEntOrderId
                           left join TbDistributionEnt disEnt on tcr.DistributionCode=disEnt.DistributionCode
                           left join TbCompany cp on disEnt.ProcessFactoryCode=cp.CompanyCode
                           left join TbCompany cp1 on TbYj.SiteCode=cp1.CompanyCode
						   where YEAR(TbYj.EWTime)= @Year and MONTH(TbYj.EWTime)= @Month ";
            var ret = Db.Context.FromSql(sql + where + orderBy)
                .AddInParameter("@Year", DbType.String, request.Year)
                .AddInParameter("@Month", DbType.String, request.Month).ToDataTable();
            return ret;
        }

        /// <summary>
        /// 签收超期
        /// </summary>
        /// <param name="Year"></param>
        /// <param name="Month"></param>
        /// <returns></returns>
        public DataTable GetQsCsYjList(HomeRequest request)
        {
            string where = "";
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where += " and TbYj.ProjectId='" + request.ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where += " and sfs.ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);//站点
                string siteStr = string.Join("','", SiteList);
                where += (" and TbYj.SiteCode in('" + siteStr + "')");
            }
            string sql = @"select TbYj.*,cp1.CompanyFullName as SiteName,sfs.DistributionCode,sdc.DistributionTime,sfs.SigninTime,sfs.ProcessFactoryCode,cp.CompanyFullName as ProcessFactoryName from (select min(EwTime) as EwTime,MenuCode,EWFormDataCode,CompanyCode,WorkArea,SiteCode,ProjectId from TbFormEarlyWarningNodeInfo where  MenuCode='SemiFinishedSign' 
						   group by MenuCode,EWFormDataCode,CompanyCode,WorkArea,SiteCode,ProjectId) TbYj
						   left join TbSemiFinishedSign sfs on TbYj.EWFormDataCode=sfs.ID
                           left join TbSiteDischargeCargo sdc on sfs.DistributionCode=sdc.DistributionCode and sfs.DischargeCargoCode=sdc.DischargeCargoCode
                           left join TbCompany cp on sfs.ProcessFactoryCode=cp.CompanyCode
                           left join TbCompany cp1 on TbYj.SiteCode=cp1.CompanyCode
						   where YEAR(TbYj.EWTime)= @Year and MONTH(TbYj.EWTime)= @Month";
            var ret = Db.Context.FromSql(sql+ where)
                .AddInParameter("@Year", DbType.String, request.Year)
                .AddInParameter("@Month", DbType.String, request.Month).ToDataTable();
            return ret;
        }

        #endregion

        #region 得到本周第一天、最后一天
        /// <summary>  
        /// 得到本周第一天(以星期一为第一天)  
        /// </summary>  
        /// <param name="datetime"></param>  
        /// <returns></returns>  
        public DateTime GetWeekFirstDayMon(DateTime datetime)
        {
            //星期一为第一天  
            int weeknow = Convert.ToInt32(datetime.DayOfWeek);

            //因为是以星期一为第一天，所以要判断weeknow等于0时，要向前推6天。  
            weeknow = (weeknow == 0 ? (7 - 1) : (weeknow - 1));
            int daydiff = (-1) * weeknow;

            //本周第一天  
            string FirstDay = datetime.AddDays(daydiff).ToString("yyyy-MM-dd");
            return Convert.ToDateTime(FirstDay);
        }
        /// <summary>  
        /// 得到本周最后一天(以星期天为最后一天)  
        /// </summary>  
        /// <param name="datetime"></param>  
        /// <returns></returns>  
        public DateTime GetWeekLastDaySun(DateTime datetime)
        {
            //星期天为最后一天  
            int weeknow = Convert.ToInt32(datetime.DayOfWeek);
            weeknow = (weeknow == 0 ? 7 : weeknow);
            int daydiff = (7 - weeknow);

            //本周最后一天  
            string LastDay = datetime.AddDays(daydiff).ToString("yyyy-MM-dd");
            return Convert.ToDateTime(LastDay);
        }

        #endregion

        #region 首页卸货问题时间展示

        public DataTable GetCarReportGridJson(TransportProcessRequest request, string Year, string Month)
        {
            var where = "";
            string orgId = OperatorProvider.Provider.CurrentUser.CompanyId;
            string orgType = OperatorProvider.Provider.CurrentUser.OrgType;
            if (!string.IsNullOrWhiteSpace(orgId))
            {
                if (!string.IsNullOrWhiteSpace(orgType) && orgType != "1")
                {
                    List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(orgId, 5);//站点
                    string siteStr = string.Join("','", SiteList);
                    where += "and tcr.SiteCode in('" + siteStr + "')";
                }
            }
            string sql = @"select tcr.*,cp.CompanyFullName as SiteName,ci.CarCph+tu.UserName AS CarCph,de.Enclosure from TbTransportCarReport  tcr
                           left join TbCompany cp on tcr.SiteCode=cp.CompanyCode
                           left join TbCarInfo ci on tcr.VehicleCode=ci.CarCode
                           left join TbDistributionEnt de on tcr.DistributionCode=de.DistributionCode
                           LEFT JOIN TbUser tu ON tu.UserCode=de.Driver
                           where (ISNULL(@ProjectId,'')='' or tcr.ProjectId=@ProjectId)
                           and (ISNULL(@ProcessFactoryCode,'')='' or de.ProcessFactoryCode=@ProcessFactoryCode)
                           and (tcr.IsProblem='是' or tcr.FlowState=2) 
                           and YEAR(tcr.EndDischargeTime)= @Year and MONTH(tcr.EndDischargeTime)= @Month " + where + @"
                           order by id desc";
            DataTable dt = Db.Context.FromSql(sql)
               .AddInParameter("@ProjectId", DbType.String, request.ProjectId)
               .AddInParameter("@ProcessFactoryCode", DbType.String, request.ProcessFactoryCode)
               .AddInParameter("@Year", DbType.String, Year)
               .AddInParameter("@Month", DbType.String, Month)
               .ToDataTable();
            return dt;
        }

        #endregion 

        public List<TbArticle> GetJiaoLiu(PageSearchRequest psr)
        {
            try
            {
                var data = Db.Context.From<TbArticle>().Where(p => p.Type == 1)
                  .OrderByDescending(TbArticle._.ID)
                  .ToList();
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }

    public class ProjectOrg
    {
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string ProjectOrgAllId { get; set; }
        public string ProjectOrgAllName { get; set; }
        public string OrgId { get; set; }
        public string OrgName { get; set; }
        public string OrgType { get; set; }
    }
}
