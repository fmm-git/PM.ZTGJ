using Dos.ORM;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Business.Flow
{
    public class TbFlowEarlyWarningConditionLogic
    {
        /// <summary>
        /// 新增、修改预警条件
        /// </summary>
        /// <param name="EarylWarningInfo"></param>
        /// <param name="FlowCode"></param>
        /// <param name="FlowNodeCode"></param>
        /// <returns></returns>
        public AjaxResult AddEarylWarning(string EarylWarningInfo, string FlowCode, string FlowNodeCode)
        {
            DbTrans trans = null;
            try
            {
                using (trans = Db.Context.BeginTransaction())
                {
                    //添加预警信息
                    JObject json = (JObject)JsonConvert.DeserializeObject(EarylWarningInfo);
                    JArray array = (JArray)json["jclist"];
                    if (!array.ToString().Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "").Equals("[[]]"))
                    {
                        List<TbFlowEarlyWarningCondition> list = new List<TbFlowEarlyWarningCondition>();
                        List<TbFlowNodeEarlyWarningPersonnel> listuser = new List<TbFlowNodeEarlyWarningPersonnel>();
                        for (int i = 0; i < array.Count; i++)
                        {
                            JObject obj = (JObject)array[i];
                            TbFlowEarlyWarningCondition model = new TbFlowEarlyWarningCondition();
                            model.EarlyWarningCode = Convert.ToString(obj["EarlyWarningCode"]);
                            model.FlowCode = Convert.ToString(obj["FlowCode"]);
                            model.FlowNodeCode = Convert.ToString(obj["FlowNodeCode"]);
                            model.EarlyWarningName = Convert.ToString(obj["EarlyWarningName"]);
                            model.EarlyWarningTime = Convert.ToInt32(obj["EarlyWarningTime"]);
                            model.EarlyWarningTimeType = Convert.ToString(obj["EarlyWarningTimeType"]);
                            model.EarlyWarningOrder = Convert.ToInt32(obj["EarlyWarningOrder"]);
                            model.Remark = Convert.ToString(obj["Remark"]);
                            list.Add(model);
                            if (obj["EarlyWarningUser"].ToString() == "[[]]")
                            {
                                return AjaxResult.Error("保存预警条件失败,请先添加预警人员");
                            }
                            //添加预警人员
                            var userjson = JsonConvert.DeserializeObject(obj["EarlyWarningUser"].ToString());
                            JArray arrayuser = (JArray)userjson;
                            for (int j = 0; j < arrayuser.Count; j++)
                            {
                                JObject objuser = (JObject)arrayuser[j];
                                TbFlowNodeEarlyWarningPersonnel modeluser = new TbFlowNodeEarlyWarningPersonnel();
                                modeluser.FlowCode = Convert.ToString(obj["FlowCode"]);
                                modeluser.FlowNodeCode = Convert.ToString(obj["FlowNodeCode"]);
                                modeluser.ActionType = 6;
                                string PersonnelCode = "";
                                if (objuser["UserTypeId"] != null)
                                {
                                    PersonnelCode = Convert.ToString(objuser["UserTypeId"]);
                                }
                                else
                                {
                                    PersonnelCode = Convert.ToString(objuser["PersonnelCode"]);
                                }
                                modeluser.PersonnelCode = PersonnelCode;
                                string PersonnelSource = "";
                                if (objuser["TypeName"] != null)
                                {
                                    switch (objuser["TypeName"].ToString())//用户输入的值,和下面的case匹配
                                    {
                                        case "部门":
                                            PersonnelSource = "Department";
                                            break;
                                        case "角色":
                                            PersonnelSource = "Role";
                                            break;
                                        case "操作员":
                                            PersonnelSource = "Personnel";
                                            break;
                                        default: //如果匹配全不成功则执行下面的代码
                                            break;
                                    }
                                }
                                else
                                {
                                    PersonnelSource = objuser["PersonnelSource"].ToString();
                                }
                                modeluser.PersonnelSource = PersonnelSource;
                                modeluser.EarlyWarningCode = Convert.ToString(obj["EarlyWarningCode"]);
                                modeluser.DeptId = Convert.ToString(objuser["DeptId"]);
                                modeluser.RoleId = Convert.ToString(objuser["RoleId"]);
                                modeluser.ProjectId = Convert.ToString(objuser["ProjectId"]);
                                modeluser.OrgId = Convert.ToString(objuser["OrgId"]);
                                listuser.Add(modeluser);
                            }
                        }
                        //先删除改预警信息下的预警人员
                        Repository<TbFlowNodeEarlyWarningPersonnel>.Delete(trans, p => p.FlowCode == FlowCode && p.FlowNodeCode == FlowNodeCode && p.ActionType == 6);
                        //添加预警人员
                        Repository<TbFlowNodeEarlyWarningPersonnel>.Insert(trans, listuser);
                        //先删除预警条件
                        Repository<TbFlowEarlyWarningCondition>.Delete(trans, p => p.FlowCode == FlowCode && p.FlowNodeCode == FlowNodeCode);
                        //添加预警条件
                        Repository<TbFlowEarlyWarningCondition>.Insert(trans, list);
                    }
                    else
                    {
                        //先删除改预警信息下的预警人员
                        Repository<TbFlowNodeEarlyWarningPersonnel>.Delete(trans, p => p.FlowCode == FlowCode && p.FlowNodeCode == FlowNodeCode && p.ActionType == 6);
                        //先删除预警条件
                        Repository<TbFlowEarlyWarningCondition>.Delete(trans, p => p.FlowCode == FlowCode && p.FlowNodeCode == FlowNodeCode);
                    }
                    trans.Commit();//提交事务
                }
                return AjaxResult.Success("预警条件添加成功");
            }
            catch (Exception)
            {
                if (trans != null) trans.Rollback();
                return AjaxResult.Error("预警条件添加失败");
            }

        }
        /// <summary>
        /// 获取数据列表
        /// </summary>
        public DataTable GetEarlyWarningConditionGridJson(string FlowCode, string FlowNodeCode)
        {
            try
            {
                string sql = @"select * from TbFlowEarlyWarningCondition where FlowCode=@FlowCode and FlowNodeCode=@FlowNodeCode";
                DataTable dt = Db.Context.FromSql(sql).AddInParameter("@FlowCode", DbType.String, FlowCode).AddInParameter("@FlowNodeCode", DbType.String, FlowNodeCode).ToDataTable();
                DataColumn dc = new DataColumn("EarlyWarningUser", typeof(string));
                dt.Columns.Add(dc);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string sqluser = "select FlowCode,FlowNodeCode,ActionType,PersonnelSource,PersonnelCode,EarlyWarningCode,DeptId,RoleId,ProjectId from  TbFlowNodeEarlyWarningPersonnel where EarlyWarningCode=@EarlyWarningCode";
                    DataTable dt1 = Db.Context.FromSql(sqluser).AddInParameter("@EarlyWarningCode", DbType.String, dt.Rows[i]["EarlyWarningCode"]).ToDataTable();
                    if (dt1 != null && dt1.Rows.Count > 0)
                    {
                        dt.Rows[i]["EarlyWarningUser"] = DataTableToJsonWithJsonNet(dt1);
                    }
                }
                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public string DataTableToJsonWithJsonNet(DataTable table)
        {
            string jsonString = string.Empty;
            jsonString = JsonConvert.SerializeObject(table);
            return jsonString;
        }
        public DataTable GetEarlyWarningFormJson(string EarlyWarningCode)
        {
            try
            {
                string sql = @"select * from TbFlowEarlyWarningCondition where EarlyWarningCode=@EarlyWarningCode";
                return Db.Context.FromSql(sql).AddInParameter("@EarlyWarningCode", DbType.String, EarlyWarningCode).ToDataTable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 获取数据列表
        /// </summary>
        public DataTable GetNodeEarlyWaringPersonnel(string EarlyWarningCode)
        {
            try
            {
                //                string sql = @"select b.id,b.Name,a.ActionType,'预警' as ActionTypeName,
                //                            case a.PersonnelSource when 'Department' then '部门' when 'Role' then '角色' when 'Personnel' then '操作员' end as TypeName,b.DeptId,b.DeptName,b.RoleId,b.RoleName
                //                            from TbFlowNodeEarlyWarningPersonnel a left join (
                //                            select TbCompany.ParentCompanyCode as pid,TbCompany.CompanyCode as id,TbCompany.CompanyFullName as Name,'Org' as TypeName,'' as DeptId,'' as DeptName,'' as RoleId,'' as RoleName,pc.ProjectId from TbCompany left join TbProjectCompany pc on TbCompany.CompanyCode=pc.CompanyCode 
                //                            union all
                //                            select '0' as pid,DepartmentId as id,DepartmentName as Name,'Dep' as TypeName,'' as DeptId,DepartmentName as DeptName,'' as RoleId,'' as RoleName,DepartmentProjectId as ProjectId from TbDepartment 
                //                            union all
                //                            select r.DepartmentId as pid,r.RoleId as id,r.RoleName as Name,'Role' as TypeName,r.DepartmentId,dp.DepartmentName as DeptName,r.RoleId,r.RoleName,dp.DepartmentProjectId as ProjectId  from TbRole r left join TbDepartment dp on r.DepartmentId=dp.DepartmentId where RoleName<>'系统管理员' 
                //                            union all
                //                            select r.RoleCode as pid,u.UserId as id,UserName as Name,'User' as TypeName,r.DeptId,dp.DepartmentName as DeptName,r.RoleCode,r1.RoleName,r.ProjectId from TbUser u 
                //                            left join TbUserRole r on u.UserId=r.UserCode 
                //                            left join TbDepartment dp on dp.DepartmentId=r.DeptId 
                //                            left join TbRole r1 on r.RoleCode=r1.RoleId
                //                            where UserClosed<>'离职' and u.UserCode<>'admin') b
                //                            on a.PersonnelCode=b.id and a.DeptId=b.DeptId and a.RoleId=b.RoleId and a.ProjectId=b.ProjectId
                //                            where a.EarlyWarningCode=@EarlyWarningCode and ActionType=6";
                string sql = @"select b.id,b.Name,a.ActionType,'预警' as ActionTypeName,
                            case a.PersonnelSource when 'Department' then '部门' when 'Role' then '角色' when 'Personnel' then '操作员' end as TypeName,b.DeptId,b.DeptName,b.RoleId,b.RoleName
                            from TbFlowNodeEarlyWarningPersonnel a left join (
                            select TbCompany.ParentCompanyCode as pid,TbCompany.CompanyCode as id,TbCompany.CompanyFullName as Name,'Org' as TypeName,'' as DeptId,'' as DeptName,'' as RoleId,'' as RoleName,pc.ProjectId from TbCompany left join TbProjectCompany pc on TbCompany.CompanyCode=pc.CompanyCode 
                            union all
                            select '0' as pid,DepartmentId as id,DepartmentName as Name,'Dep' as TypeName,'' as DeptId,DepartmentName as DeptName,'' as RoleId,'' as RoleName,DepartmentProjectId as ProjectId from TbDepartment 
                            union all
                            select r.DepartmentId as pid,r.RoleId as id,r.RoleName as Name,'Role' as TypeName,r.DepartmentId,dp.DepartmentName as DeptName,r.RoleId,r.RoleName,dp.DepartmentProjectId as ProjectId  from TbRole r left join TbDepartment dp on r.DepartmentId=dp.DepartmentId where RoleName<>'系统管理员' 
                            union all
                            select r.RoleCode as pid,u.UserId as id,UserName as Name,'User' as TypeName,r.DeptId,dp.DepartmentName as DeptName,r.RoleCode,r1.RoleName,r.ProjectId from TbUser u 
                            left join TbUserRole r on u.UserId=r.UserCode 
                            left join TbDepartment dp on dp.DepartmentId=r.DeptId 
                            left join TbRole r1 on r.RoleCode=r1.RoleId
                            where UserClosed<>'离职' and u.UserCode<>'admin') b
                            on a.PersonnelCode=b.id and a.DeptId=b.DeptId and a.RoleId=b.RoleId 
                            where a.EarlyWarningCode=@EarlyWarningCode and ActionType=6";
                return Db.Context.FromSql(sql).AddInParameter("@EarlyWarningCode", DbType.String, EarlyWarningCode).ToDataTable();
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// 获取预警数量
        /// </summary>
        /// <param name="usercode"></param>
        /// <returns></returns>
        public DataTable GetEarlyWarningCount(string usercode)
        {
            try
            {
                ////新增预警信息
                //EarlyWarning();
                //获取预警信息
                string sql = @"select COUNT(1) as earlywarningcount from TbFlowEarlyWarningInfo where EarlyWarningBTUser=@usercode and EarlyWarningStart=0";
                return Db.Context.FromSql(sql).AddInParameter("@usercode", DbType.String, usercode).ToDataTable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region 预警
        public void EarlyWarning()
        {
            DbTrans trans = null;
            try
            {
                using (trans = Db.Context.BeginTransaction())
                {
                    List<PM.DataEntity.TbFlowEarlyWarningInfo> list = new List<PM.DataEntity.TbFlowEarlyWarningInfo>();
                    string sql = @"select FlowCode,FlowNodeCode,FlowPerformID,FlowTitle,FormCode,FormDataCode,PreNodeCompleteDate,PerformState,EarlyWarningCode,EarlyWarningBegTime,EarlyWarningTime,EarlyWarningTimeType,PersonnelCode,PersonnelSource,ProjectId from (select Tb1.*,Tb2.EarlyWarningCode,case when Tb2.EarlyWarningTimeType='年' then DATEADD(YEAR,Tb2.EarlyWarningTime,Tb1.PreNodeCompleteDate) when Tb2.EarlyWarningTimeType='月' then DATEADD(MONTH,Tb2.EarlyWarningTime,Tb1.PreNodeCompleteDate) when Tb2.EarlyWarningTimeType='天' then DATEADD(DAY,Tb2.EarlyWarningTime,Tb1.PreNodeCompleteDate) when Tb2.EarlyWarningTimeType='时' then DATEADD(HOUR,Tb2.EarlyWarningTime,Tb1.PreNodeCompleteDate) when Tb2.EarlyWarningTimeType='分' then DATEADD(MINUTE,Tb2.EarlyWarningTime,Tb1.PreNodeCompleteDate) else '' end as EarlyWarningBegTime,Tb2.EarlyWarningTime,Tb2.EarlyWarningTimeType,Tb2.PersonnelCode,Tb2.PersonnelSource,Tb2.ProjectId from (select DISTINCT fp.FlowCode,fp.FormDataCode,fp.FlowTitle,fd.FormCode,fpo.FlowPerformID,fpo.FlowNodeCode,fpo.PreNodeCompleteDate,fpo.PerformState from TbFlowPerformOpinions fpo
left join TbFlowPerform fp on fpo.FlowPerformID=fp.FlowPerformID
left join TbFlowDefine fd on fp.FlowCode=fd.FlowCode where fpo.PerformState=-1) Tb1 
left join (select few.FlowCode,few.FlowNodeCode,few.EarlyWarningCode,few.EarlyWarningTime,few.EarlyWarningTimeType,fewp.PersonnelSource,fewp.PersonnelCode,fewp.ProjectId from TbFlowEarlyWarningCondition few
left join TbFlowNodeEarlyWarningPersonnel fewp on few.EarlyWarningCode=fewp.EarlyWarningCode
left join TbFlowEarlyWarningInfo ewi on ewi.EarlyWarningCode=fewp.EarlyWarningCode and ewi.FlowPerformID is null) Tb2 on Tb1.FlowCode=Tb2.FlowCode and Tb1.FlowNodeCode=Tb2.FlowNodeCode) Tb 
where GETDATE()>Tb.EarlyWarningBegTime and Tb.EarlyWarningCode is not null and Tb.FlowPerformID not in(select FlowPerformID from TbFlowEarlyWarningInfo where FlowPerformID=Tb.FlowPerformID and FlowCode=Tb.FlowCode and FlowNodeCode=Tb.FlowNodeCode)";
                    DataTable dt = Db.Context.FromSql(sql).ToDataTable();
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            #region  获取预警的用户

                            string sqlUser = "";
                            DataTable dtUser = null;
                            switch (dt.Rows[i]["PersonnelSource"].ToString())
                            {
                                case "Department":
                                    sqlUser = @"select ur.UserCode from TbDepartment dp
left join TbRole r on dp.DepartmentId=r.DepartmentId
left join TbUserRole ur on ur.DeptId=dp.DepartmentId and ur.RoleCode=r.RoleId where dp.DepartmentId=@PersonnelCode and ur.Flag=0 and dp.DepartmentProjectId=@ProjectId";
                                    dtUser = Db.Context.FromSql(sqlUser).AddInParameter("@PersonnelCode", DbType.String, dt.Rows[i]["PersonnelCode"].ToString()).AddInParameter("@ProjectId", DbType.String, dt.Rows[i]["ProjectId"].ToString()).ToDataTable();
                                    break;
                                case "Role":
                                    sqlUser = @"select ur.UserCode from TbRole r
left join TbUserRole ur on r.RoleId=ur.RoleCode where r.RoleId=@PersonnelCode and ur.Flag=0 and ur.ProjectId=@ProjectId";
                                    dtUser = Db.Context.FromSql(sqlUser).AddInParameter("@PersonnelCode", DbType.String, dt.Rows[i]["PersonnelCode"].ToString()).AddInParameter("@ProjectId", DbType.String, dt.Rows[i]["ProjectId"].ToString()).ToDataTable();
                                    break;
                                default:
                                    break;
                            }
                            if (dtUser != null && dtUser.Rows.Count > 0)
                            {
                                DataView dv = new DataView(dtUser); //虚拟视图吧，我这么认为
                                DataTable dtUser2 = dv.ToTable(true, "UserCode");
                                for (int j = 0; j < dtUser2.Rows.Count; j++)
                                {
                                    PM.DataEntity.TbFlowEarlyWarningInfo model = new PM.DataEntity.TbFlowEarlyWarningInfo();
                                    model.EarlyWarningCode = dt.Rows[i]["EarlyWarningCode"].ToString();
                                    model.FlowCode = dt.Rows[i]["FlowCode"].ToString();
                                    model.FlowNodeCode = dt.Rows[i]["FlowNodeCode"].ToString();
                                    model.FlowPerformID = dt.Rows[i]["FlowPerformID"].ToString();
                                    model.EarlyWarningStart = 0;
                                    model.EarlyWarningContent = dt.Rows[i]["FlowTitle"].ToString();
                                    model.EWFormDataCode = Convert.ToInt32(dt.Rows[i]["FormDataCode"]);
                                    model.EWFormCode = dt.Rows[i]["FormCode"].ToString();
                                    model.EarlyWarningBTUser = dtUser2.Rows[j]["UserCode"].ToString();
                                    model.EarlyWarningTime = DateTime.Now;
                                    list.Add(model);
                                }
                            }
                            else
                            {

                                PM.DataEntity.TbFlowEarlyWarningInfo model = new PM.DataEntity.TbFlowEarlyWarningInfo();
                                model.EarlyWarningCode = dt.Rows[i]["EarlyWarningCode"].ToString();
                                model.FlowCode = dt.Rows[i]["FlowCode"].ToString();
                                model.FlowNodeCode = dt.Rows[i]["FlowNodeCode"].ToString();
                                model.FlowPerformID = dt.Rows[i]["FlowPerformID"].ToString();
                                model.EarlyWarningStart = 0;
                                model.EarlyWarningContent = dt.Rows[i]["FlowTitle"].ToString();
                                model.EWFormDataCode = Convert.ToInt32(dt.Rows[i]["FormDataCode"]);
                                model.EWFormCode = dt.Rows[i]["FormCode"].ToString();
                                model.EarlyWarningBTUser = dt.Rows[i]["PersonnelCode"].ToString();
                                model.EarlyWarningTime = DateTime.Now;
                                list.Add(model);
                            }

                            #endregion
                        }

                        //添加预警信息
                        Repository<PM.DataEntity.TbFlowEarlyWarningInfo>.Insert(trans, list);
                        //调用手机端消息推送方法

                        trans.Commit();//提交事务
                    }
                }
            }
            catch (Exception)
            {
                if (trans != null) trans.Rollback();
            }
        }
        #endregion

        /// <summary>
        /// 获取该用户下的所有未处理的流程预警信息
        /// </summary>
        /// <returns></returns>
        public PageModel GetEarlyWarningInfo(PageSearchRequest pt, string usercode, string ProjectId, int state, DateTime? sdt, DateTime? edt)
        {
            string where = "";
            if (!string.IsNullOrWhiteSpace(ProjectId))
            {
                where += " and a.ProjectId=@ProjectId";
            }
            string sql = @"select a.ID,a.ProjectId,a.ProcessFactoryCode,a.EarlyWarningCode,a.FlowPerformID,a.FlowCode,a.EWFormCode,a.EWFormDataCode,a.EarlyWarningStart,case when a.EarlyWarningStart=0 then '未阅' when a.EarlyWarningStart=1 then '已阅' else '预警已撤销' end EarlyWarningStartName,a.FlowYjUserCode as EarlyWarningBTUser,u2.UserName as EarlyWarningBTUserName,a.FlowSpUserCode,u1.UserName as FlowUserName,b.PreNodeCompleteDate,EarlyWarningContent,a.EarlyWarningTime from  TbFlowEarlyWarningOtherInfo a
                           left join TbFlowPerformOpinions b on a.FlowPerformID=b.FlowPerformID and a.FlowNodeCode=b.FlowNodeCode and a.FlowPerformOID=b.id
                           left join TbUser u1 on a.FlowSpUserCode=u1.UserId
                           left join TbUser u2 on a.FlowYjUserCode=u2.UserId
                           where a.EarlyWarningStart is not null
                           and EarlyWarningStart=@EarlyWarningStart
                           and (ISNULL(@UserId,'')='' or a.FlowYjUserCode=@UserId)
                           and (isnull(@SDT,'')='' or a.EarlyWarningTime>=@SDT) 
                           and (isnull(@EDT,'')='' or a.EarlyWarningTime<=@EDT)";
            List<Parameter> parameter = new List<Parameter>();
            parameter.Add(new Parameter("@UserId", usercode, DbType.String, null));
            parameter.Add(new Parameter("@EarlyWarningStart", state, DbType.Int32, null));
            parameter.Add(new Parameter("@SDT", sdt, DbType.DateTime, null));
            parameter.Add(new Parameter("@EDT", edt, DbType.DateTime, null));
            parameter.Add(new Parameter("@ProjectId", ProjectId, DbType.String, null));
            try
            {
                var ret = Repository<TbFormEarlyWarningNodeInfo>.FromSqlToPageTable(sql, parameter, pt.rows, pt.page, "EarlyWarningTime", "desc");
                return ret;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 获取该用户下的所有的表单预警信息
        /// </summary>
        /// <returns></returns>
        public PageModel GetEarlyWarningFormInfo(PageSearchRequest pt, string usercode, string ProjectId, int state, DateTime? sdt, DateTime? edt)
        {
            //获取表单预警信息数量
            string where = "";
            if (!string.IsNullOrWhiteSpace(usercode))
            {
                where += " and a.EWUserCode=@UserId";
            }
            if (!string.IsNullOrWhiteSpace(ProjectId))
            {
                where += " and a.ProjectId=@ProjectId";
            }
            string sql = @"select a.ID,a.ProjectId,a.ProcessFactoryCode,a.EWUserCode,case when c.MenuName is null then '取样订单' else c.MenuName end MenuName,a.EarlyWarningCode as EWNodeCode,b.EarlyWarningContent as EWNodeName,a.EWFormDataCode,a.WorkArea,e.CompanyFullName as WorkAreaName,d.UserName as EWUserName,a.EWContent as EarlyWarningContent,a.EWStart,case when a.EWStart=0 then '未阅' when a.EWStart=1 then '已阅' else '预警已撤销' end EWStartName,a.EWTime as EarlyWarningTime from TbFormEarlyWarningNodeInfo a
            left join TbEarlyWarningSetUp b on a.EarlyWarningCode=b.EarlyWarningNewsCode
            left join TbSysMenu c on a.MenuCode=c.MenuCode
            left join TbUser d on a.EWUserCode=d.UserId
            left join TbCompany e on a.WorkArea=e.CompanyCode
            where a.MsgType=2 
            and a.EWStart=@EWStart
            and (ISNULL(@SDT,'')='' or a.EWTime>=@SDT)
            and (ISNULL(@EDT,'')='' or a.EWTime<=@EDT)" + where + @"";
            List<Parameter> parameter = new List<Parameter>();
            parameter.Add(new Parameter("@UserId", usercode, DbType.String, null));
            parameter.Add(new Parameter("@EWStart", state, DbType.Int32, null));
            parameter.Add(new Parameter("@SDT", sdt, DbType.DateTime, null));
            parameter.Add(new Parameter("@EDT", edt, DbType.DateTime, null));
            parameter.Add(new Parameter("@ProjectId", ProjectId, DbType.String, null));
            try
            {
                var ret = Repository<TbFormEarlyWarningNodeInfo>.FromSqlToPageTable(sql, parameter, pt.rows, pt.page, "EarlyWarningTime", "desc");
                return ret;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public AjaxResult HandleEarlyWarning(int ID, string EarlyType)
        {
            try
            {
                if (EarlyType == "流程预警")
                {
                    var earlywarningInfo = Repository<TbFlowEarlyWarningInfo>.First(p => p.ID == ID);
                    if (earlywarningInfo == null)
                        return AjaxResult.Warning("预警信息不存在");
                    earlywarningInfo.EarlyWarningStart = 1;//已阅
                    Repository<TbFlowEarlyWarningInfo>.Update(earlywarningInfo,true);
                    return AjaxResult.Success();
                }
                else
                {
                    var earlywarningInfo = Repository<TbFormEarlyWarningNodeInfo>.First(p => p.ID == ID);
                    if (earlywarningInfo == null)
                        return AjaxResult.Warning("预警信息不存在");
                    earlywarningInfo.EWStart = 1;//已阅
                    Repository<TbFormEarlyWarningNodeInfo>.Update(earlywarningInfo, true);
                    return AjaxResult.Success();
                }
            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 获取我的预警状态
        /// </summary>
        /// <param name="usercode">用户编码</param>
        /// <returns>状态列表</returns>
        public DataTable GetMyEarlyWarningState(string usercode, string ProjectId, DateTime? SDT, DateTime? EDT)
        {
            string where = "";
            if (!string.IsNullOrWhiteSpace(ProjectId))
            {
                where += " and a.ProjectId=@ProjectId";
            }
            string sql = @"select TbEarlyStart.*,isnull(count(TbEarlyInfo.EWStart),0) as EWStartCount from (select 0 as EarlyWarningStart,'未阅' as EarlyWarningStartName
                             union all
                             select 1 as EarlyWarningStart,'已阅' as EarlyWarningStartName
                             union all
                             select 2 as EarlyWarningStart,'预警已撤销' as EarlyWarningStartName) TbEarlyStart 
                             left join (select a.EWStart from TbFormEarlyWarningNodeInfo a
                             left join TbEarlyWarningSetUp b on a.EarlyWarningCode=b.EarlyWarningNewsCode
                             left join TbSysMenu c on a.MenuCode=c.MenuCode
                             left join TbUser d on a.EWUserCode=d.UserId
                             left join TbCompany e on a.WorkArea=e.CompanyCode
                             where a.MsgType=2
                             and (isnull(@UserId,'')=''  or a.EWUserCode=@UserId)
                             and (ISNULL(@SDT,'')='' or a.EWTime=@SDT)
                             and (ISNULL(@EDT,'')='' or a.EWTime=@EDT)
                             union all
                             select a.EarlyWarningStart from TbFlowEarlyWarningOtherInfo a
                             where a.EarlyWarningStart is not null
                             and (isnull(@UserId,'')=''  or a.FlowYjUserCode=@UserId)
                             and (ISNULL(@SDT,'')='' or a.EarlyWarningTime=@SDT)
                             and (ISNULL(@EDT,'')='' or a.EarlyWarningTime=@EDT)
                             ) TbEarlyInfo on TbEarlyStart.EarlyWarningStart=TbEarlyInfo.EWStart
                             group by TbEarlyStart.EarlyWarningStart,TbEarlyStart.EarlyWarningStartName";
            try
            {
                var ret = Db.Context.FromSql(sql)
               .AddInParameter("@UserId", DbType.String, usercode)
               .AddInParameter("@ProjectId", DbType.String, ProjectId)
               .AddInParameter("@SDT", DbType.DateTime, SDT)
               .AddInParameter("@EDT", DbType.DateTime, EDT).ToDataTable(); ;
                return ret;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
//        /// <summary>
//        /// 获取App我的预警状态
//        /// </summary>
//        /// <param name="usercode">用户编码</param>
//        /// <returns>状态列表</returns>
//        public DataTable GetAppMyEarlyWarningState(string usercode, string ProjectId, DateTime? SDT, DateTime? EDT)
//        {

//            string sql = @"select Tb.EarlyWarningStart,Tb.EarlyWarningStartName+'【'+isnull(cast(Tb1.EwCount as nvarchar),0)+'】' EarlyWarningStartName  from (select 0 as EarlyWarningStart,'未阅' as EarlyWarningStartName union all select 1 as EarlyWarningStart,'已阅' as EarlyWarningStartName union all select 2 as EarlyWarningStart,'预警已撤销' as EarlyWarningStartName) Tb
//                            left join (select TbEwInfo.EWStart,TbEwInfo.EWUserCode,COUNT(1) as EwCount from (
//                            select EWUserCode,EWStart from TbFormEarlyWarningNodeInfo where EWUserCode=@UserCode and (ISNULL(@SDT,'')='' or EWTime>=@SDT) and (ISNULL(@EDT,'')='' or EWTime<=@EDT) and (ISNULL(@ProjectId,'')='' or ProjectId=@ProjectId)
//                            union all 
//                            select Tb.EarlyWarningBTUser,tb.EarlyWarningStart  from (select ewi.ID,ewi.EarlyWarningStart,ewi.EarlyWarningCode,ewi.FlowCode,ewi.EarlyWarningBTUser,ewi.EarlyWarningContent,fo.FlowNodeCode,fo.FlowPerformID,fo.PreNodeCompleteDate,fo.UserCode,ewi.EarlyWarningTime as EarlyWarningBegTime,ewi.ProjectId from TbFlowEarlyWarningInfo ewi
//                            left join (select po.* from (select MAX(id) as id,FlowNodeCode,FlowPerformID from TbFlowPerformOpinions group by FlowNodeCode,FlowPerformID) Tb
//                            left join TbFlowPerformOpinions po on Tb.id=po.id) fo on ewi.FlowPerformID=fo.FlowPerformID and ewi.FlowNodeCode=fo.FlowNodeCode
//                            where ewi.EarlyWarningBTUser=@UserCode) Tb 
//                            left join TbUser u1 on u1.UserId=Tb.EarlyWarningBTUser
//                            left join TbUser u2 on u2.UserId=Tb.UserCode
//                            where (ISNULL(@SDT,'')='' or Tb.PreNodeCompleteDate>=@SDT) and (ISNULL(@EDT,'')='' or Tb.PreNodeCompleteDate<=@EDT) and (ISNULL(@ProjectId,'')='' or Tb.ProjectId=@ProjectId)) TbEwInfo group by TbEwInfo.EWStart,TbEwInfo.EWUserCode) as Tb1 on Tb.EarlyWarningStart=Tb1.EWStart";
//            DataTable dt = Db.Context.FromSql(sql).AddInParameter("@UserCode", DbType.String, usercode)
//                .AddInParameter("@SDT", DbType.DateTime, SDT)
//                .AddInParameter("@EDT", DbType.DateTime, EDT)
//                .AddInParameter("@ProjectId", DbType.String, ProjectId).ToDataTable();
//            return dt;
//        }
    }
}
