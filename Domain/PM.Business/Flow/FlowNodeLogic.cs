using Dos.ORM;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PM.Common;
using PM.Common.Helper;
using PM.DataAccess.DbContext;
using PM.DataAccess.Flow;
using PM.DataEntity;
using PM.DataEntity.Flow;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Business
{
    public class FlowNodeLogic
    {
        /// <summary>
        /// 用户编号
        /// </summary>
        private string UserCode = OperatorProvider.Provider.CurrentUser == null ? "" : OperatorProvider.Provider.CurrentUser.UserCode;
        /// <summary>
        /// 用户名称
        /// </summary>
        private string UserName = OperatorProvider.Provider.CurrentUser == null ? "" : OperatorProvider.Provider.CurrentUser.UserName;
        FlowNodeDA dao = new FlowNodeDA();

        /// <summary>
        /// 初始化流程节点
        /// </summary>
        /// <param name="FlowCode"></param>
        /// <returns></returns>
        public int LoadingNode(string FlowCode)
        {
            return dao.LoadingNode(FlowCode);
        }
        public AjaxResult AddNodeInfo(string NoeInfo)
        {
            JObject NodeObj = (JObject)JsonConvert.DeserializeObject(NoeInfo);
            string FlowCode = Convert.ToString(NodeObj["FlowCode"]);
            int FlowNodeCode = Convert.ToInt32(NodeObj["ID"]);
            string NodeName = Convert.ToString(NodeObj["Name"]);
            int ActiveId = Convert.ToInt32(NodeObj["CurrentNode"]);
            int AllApproval = Convert.ToInt32(NodeObj["AllApproval"]);
            int FreeCandidates = Convert.ToInt32(NodeObj["FreeCandidates"]);
            int BlankNode = Convert.ToInt32(NodeObj["BlankNode"]);
            string NodeLeft = Convert.ToString(NodeObj["NodeLeft"]);
            string NodeTop = Convert.ToString(NodeObj["NodeTop"]);
            string FlowNodeEvent = Convert.ToString(NodeObj["FlowNodeEvent"]);
            JArray array = (JArray)NodeObj["SPLIST"];
            int result = 0;
            DbTrans trans = null;
            try
            {
                using (trans = Db.Context.BeginTransaction())
                {
                    #region 添加节点

                    List<TbFlowNode> list = dao.GetAllByFlowCode(FlowCode);
                    for (int i = 0; i < list.Count; i++)
                    {
                        int count = list.Where(f => f.FlowNodeCode == Convert.ToString(FlowNodeCode)).Count();
                        if (count > 0)
                        {
                            FlowNodeCode = FlowNodeCode + 10;
                        }
                        else
                        {
                            break;
                        }
                    }
                    //添加节点
                    result = dao.AddNode(FlowCode, NodeName, ActiveId, FlowNodeCode, AllApproval, FreeCandidates, BlankNode, NodeLeft, NodeTop, FlowNodeEvent);
                    if (result > 0 && array.Count > 0 && Convert.ToString(array[0]) != "[]")
                    {
                        result = 0;
                        List<TbFlowNodePersonnel> plist = new List<TbFlowNodePersonnel>();
                        for (int i = 0; i < array.Count; i++)
                        {
                            TbFlowNodePersonnel model = new TbFlowNodePersonnel();
                            model.FlowCode = FlowCode;
                            model.FlowNodeCode = FlowNodeCode.ToString();
                            int ActionType = 0;
                            switch (Convert.ToString(array[i]["ActionType"]))
                            {
                                case "审批":
                                    ActionType = 0;
                                    break;
                                case "抄送":
                                    ActionType = -1;
                                    break;
                            }
                            model.ActionType = ActionType;
                            string source = string.Empty;
                            switch (Convert.ToString(array[i]["TypeName"]))
                            {
                                case "部门":
                                    source = "Department";
                                    break;
                                case "角色":
                                    source = "Role";
                                    break;
                                case "人员":
                                    source = "Personnel";
                                    break;
                            }
                            model.PersonnelSource = source;
                            model.PersonnelCode = Convert.ToString(array[i]["UserTypeId"]);
                            model.DeptId = Convert.ToString(array[i]["DeptId"]);
                            model.RoleId = Convert.ToString(array[i]["RoleId"]);
                            model.ProjectId = Convert.ToString(array[i]["ProjectId"]);
                            model.OrgId = Convert.ToString(array[i]["OrgId"]);
                            model.OrgType = Convert.ToString(array[i]["OrgType"]);
                            plist.Add(model);
                        }
                        //添加节点人员
                        result = Repository<TbFlowNodePersonnel>.Insert(plist);
                    }

                    #endregion
                    if (result > 0)
                    {
                        return AjaxResult.Success();
                    }
                    else
                    {
                        return AjaxResult.Error();
                    }

                }
            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.ToString());
            }

        }
        public AjaxResult UpdateNode(string NodeInfo)
        {
            JObject NodeObj = (JObject)JsonConvert.DeserializeObject(NodeInfo);
            string FlowCode = Convert.ToString(NodeObj["FlowCode"]);
            int FlowNodeCode = Convert.ToInt32(NodeObj["ID"]);
            string NodeName = Convert.ToString(NodeObj["Name"]);
            int ActiveId = Convert.ToInt32(NodeObj["CurrentNode"]);
            int AllApproval = Convert.ToInt32(NodeObj["AllApproval"]);
            int FreeCandidates = Convert.ToInt32(NodeObj["FreeCandidates"]);
            int BlankNode = Convert.ToInt32(NodeObj["BlankNode"]);
            string NodeLeft = Convert.ToString(NodeObj["NodeLeft"]);
            string NodeTop = Convert.ToString(NodeObj["NodeTop"]);
            string FlowNodeEvent = Convert.ToString(NodeObj["FlowNodeEvent"]);
            JArray array = (JArray)NodeObj["SPLIST"];
            int result = 0;
            List<TbFlowNodePersonnel> plist = new List<TbFlowNodePersonnel>();
            try
            {
                if (array.Count > 0)
                {
                    for (int i = 0; i < array.Count; i++)
                    {
                        TbFlowNodePersonnel model = new TbFlowNodePersonnel();
                        model.FlowCode = FlowCode;
                        model.FlowNodeCode = FlowNodeCode.ToString();
                        int ActionType = 0;
                        switch (Convert.ToString(array[i]["ActionType"]))
                        {
                            case "审批":
                                ActionType = 0;
                                break;
                            case "抄送":
                                ActionType = -1;
                                break;
                        }
                        model.ActionType = ActionType;
                        string source = string.Empty;
                        switch (Convert.ToString(array[i]["TypeName"]))
                        {
                            case "部门":
                                source = "Department";
                                break;
                            case "角色":
                                source = "Role";
                                break;
                            case "人员":
                                source = "Personnel";
                                break;
                        }
                        model.PersonnelSource = source;
                        model.PersonnelCode = Convert.ToString(array[i]["UserTypeId"]);
                        model.DeptId = Convert.ToString(array[i]["DeptId"]);
                        model.RoleId = Convert.ToString(array[i]["RoleId"]);
                        model.ProjectId = Convert.ToString(array[i]["ProjectId"]);
                        model.OrgId = Convert.ToString(array[i]["OrgId"]);
                        model.OrgType = Convert.ToString(array[i]["OrgType"]);
                        plist.Add(model);
                    }
                }
                result = dao.UpdateNode(FlowCode, NodeName, FlowNodeCode, AllApproval, FreeCandidates, BlankNode, FlowNodeEvent, plist);
                if (result > 0)
                {
                    return AjaxResult.Success();
                }
                else
                {
                    return AjaxResult.Error();
                }
            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.ToString());
            }
        }
        /// <summary>
        /// 获取流程节点
        /// </summary>
        /// <param name="FlowCode"></param>
        /// <returns></returns>
        public string GetNodeList(string FlowCode)
        {
            DataTable table = dao.GetNodeList(FlowCode);
            if (table != null)
                return JsonConvert.SerializeObject(table);
            else return JsonConvert.SerializeObject(new DataTable());
        }
        /// <summary>
        /// 通过流程编码及流节点编码获取审批人
        /// </summary>
        /// <param name="FlowCode"></param>
        /// <param name="FlowNodeCode"></param>
        /// <returns></returns>
        public string GetNodePersonnel(string FlowCode, string FlowNodeCode)
        {
            var table = dao.GetNodePersonnel(FlowCode, FlowNodeCode);
            return JsonConvert.SerializeObject(table);
        }
        /// <summary>
        /// 通过流程编码及节点编码获取节点信息
        /// </summary>
        /// <param name="FlowCode"></param>
        /// <param name="FlowNodeCode"></param>
        /// <returns></returns>
        public AjaxResult<TbFlowNode> GetNode(string FlowCode, string FlowNodeCode)
        {
            TbFlowNode entity = dao.GetNode(FlowCode, FlowNodeCode);
            return AjaxResult<TbFlowNode>.Success(entity);
        }
        /// <summary>
        /// 删除节点
        /// </summary>
        /// <param name="FlowCode"></param>
        /// <param name="FlowNodeCode"></param>
        /// <param name="ParentNodeCode"></param>
        /// <param name="ChildNodeCode"></param>
        /// <returns></returns>
        public AjaxResult DeleteNode(string FlowCode, string FlowNodeCode)
        {
            try
            {
                int NonQuery = dao.DeleteNode(FlowCode, FlowNodeCode);
                if (NonQuery > 0)
                    return AjaxResult.Success();
                else return AjaxResult.Error();
            }
            catch (Exception)
            {
                return AjaxResult.Error();
            }

        }
        public string GetLastNodeInfo(string FlowCode, string ChildNodeCode)
        {
            try
            {
                DataTable dt = dao.GetLastNodeInfo(FlowCode, ChildNodeCode);
                return JsonConvert.SerializeObject(dt);
            }
            catch (Exception)
            {
                return JsonConvert.SerializeObject(new DataTable());
            }
        }
        /// <summary>
        /// 流程编码获取节点名称
        /// </summary>
        /// <param name="FlowCode"></param>
        /// <returns></returns>
        public string GetFlowNodeName(string FlowCode)
        {
            DataTable table = dao.GetFlowNodeName(FlowCode);
            if (table != null)
                return JsonConvert.SerializeObject(table);
            else return JsonConvert.SerializeObject(new DataTable());
        }
        public AjaxResult Insert(IEnumerable<TbFlowNode> list)
        {
            DbTrans trans = null;
            try
            {
                using (trans = Db.Context.BeginTransaction())
                {
                    FlowNodeDA.Insert(trans, list);
                    trans.Commit();
                    return AjaxResult.Success();
                }
            }
            catch (Exception)
            {
                if (trans != null) trans.Rollback();
                return AjaxResult.Error();
            }

        }

        /// <summary>
        /// 获取部门角色人员
        /// </summary>
        /// <param name="OrgType"></param>
        /// <returns></returns>
        public List<OrgEntity> GetDeptOrRoleOrUser(int OrgType, string CompanyId)
        {
            return dao.GetDeptOrRoleOrUser(OrgType, CompanyId);
        }

        /// <summary>
        /// 获取组织机构
        /// </summary>
        /// <returns></returns>
        public List<OrgEntity> GetOrg()
        {
            return dao.GetOrg();
        }

        #region 获取下级没有设置执行人的自由选择人节点
        /// <summary>
        /// 获取下级没有设置执行人的自由选择人节点
        /// </summary>
        /// <param name="FlowCode">流程代码</param>
        /// <param name="FlowPerformID">流程执行流水号</param>
        /// <param name="CurrNodeCode">当前执行节点代码</param>
        /// <returns></returns>
        public string GetChildNoUserFreeNode(string FlowCode, string FlowPerformID, int CurrNodeCode)
        {
            return JsonConvert.SerializeObject(dao.GetChildNoUserFreeNode(FlowCode, FlowPerformID, CurrNodeCode));
        }
        /// <summary>
        /// 通过组织机构类型查找相应的部门
        /// </summary>
        /// <param name="OrgType"></param>
        /// <returns></returns>
        public string GetDepartmentByOrgType(int OrgType)
        {
            return JsonConvert.SerializeObject(dao.GetDepartmentByOrgType(OrgType));
        }
        public string GetRoleByDepCode(string DepCode)
        {
            return JsonConvert.SerializeObject(dao.GetRoleByDepCode(DepCode));
        }
        #endregion


        #region 流程新版TQ

        /// <summary>
        /// 获取组织机构下部门角色人员
        /// </summary>
        /// <param name="OrgType">组织机构类型</param>
        /// <returns></returns>
        public List<OrgEntity> GetDeptOrRoleOrUserNew(string OrgType)
        {
            List<OrgEntity> list = new List<OrgEntity>();
            string ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
            //2、查询部门
            List<OrgEntity> list2 = new List<OrgEntity>();
            string sql2 = "";
            if (OrgType == "1")//加工厂
            {
                sql2 = @"select '0' as pid,dp.DepartmentId as id,dp.DepartmentName as Name,'Dep' as TypeName,dp.DepartmentType as OrgType,dp.DepartmentProjectId as ProjectId,dp.DepartmentId as DeptId,dp.DepartmentName as DeptName,'' as RoleId,'' as RoleName from TbDepartment dp where dp.DepartmentProjectId='6245721945602523136' and DepartmentType=@OrgType order by dp.DepartmentId asc";
            }
            else
            {
                sql2 = @"select '0' as pid,dp.DepartmentId as id,dp.DepartmentName as Name,'Dep' as TypeName,dp.DepartmentType as OrgType,dp.DepartmentProjectId as ProjectId,dp.DepartmentId as DeptId,dp.DepartmentName as DeptName,'' as RoleId,'' as RoleName from TbDepartment dp where dp.DepartmentProjectId=@ProjectId and DepartmentType=@OrgType order by dp.DepartmentId asc";
            }
            DataTable dt2 = Db.Context.FromSql(sql2).AddInParameter("@ProjectId", DbType.String, ProjectId).AddInParameter("@OrgType", DbType.String, OrgType).ToDataTable();
            list2.AddRange(ModelConvertHelper<OrgEntity>.ToList(dt2));
            list.AddRange(list2);
            //3、查询角色
            List<OrgEntity> list3 = new List<OrgEntity>();
            if (list2.Count > 0)
            {
                for (int i = 0; i < list2.Count; i++)
                {
                    string sql3 = @"select r.DepartmentId as pid,r.RoleId as id,r.RoleName as Name,'Role' as TypeName,'' as OrgType,dp.DepartmentProjectId as ProjectId,dp.DepartmentId as DeptId,dp.DepartmentName as DeptName,r.RoleId as RoleId,r.RoleName as RoleName from TbRole r left join TbDepartment dp on r.DepartmentId=dp.DepartmentId where r.DepartmentId=@DepartmentId order by r.RoleId asc";
                    DataTable dt3 = Db.Context.FromSql(sql3).AddInParameter("@DepartmentId", DbType.String, list2[i].id).ToDataTable();
                    list3.AddRange(ModelConvertHelper<OrgEntity>.ToList(dt3));
                }
            }
            list.AddRange(list3);
            if (OrgType == "1" || OrgType == "2")
            {
                string CompanyId = "";
                if (OrgType == "1")
                {
                    CompanyId = OperatorProvider.Provider.CurrentUser.ProcessFactoryCode;
                }
                else
                {
                    CompanyId = OperatorProvider.Provider.CurrentUser.CompanyId;
                }
                //4、查询人员
                List<OrgEntity> list4 = new List<OrgEntity>();
                if (list3.Count > 0)
                {
                    string sql4 = "";
                    if (OrgType == "1")
                    {
                        sql4 = @"select ur.RoleCode as pid,ur.UserCode+'-'+cast(ROW_NUMBER() OVER (ORDER BY NEWID()) as varchar) as id,u.UserName as Name,'User' as TypeName,cast(ur.OrgType as varchar) as OrgType,ur.ProjectId,ur.DeptId,dp.DepartmentName as DeptName,ur.RoleCode as RoleId,r.RoleName from TbUserRole ur
            left join TbUser u on ur.UserCode=u.UserId 
            left join TbDepartment dp on dp.DepartmentId=ur.DeptId
            left join TbRole r on ur.RoleCode=r.RoleId 
            where ur.OrgId=@CompanyId and Flag=0 and u.UserName is not null order by ur.UserCode asc";
                    }
                    else if (OrgType == "2")
                    {
                        sql4 = @"select ur.RoleCode as pid,ur.UserCode+'-'+cast(ROW_NUMBER() OVER (ORDER BY NEWID()) as varchar) as id,u.UserName as Name,'User' as TypeName,cast(ur.OrgType as varchar) as OrgType,ur.ProjectId,ur.DeptId,dp.DepartmentName as DeptName,ur.RoleCode as RoleId,r.RoleName from TbUserRole ur
            left join TbUser u on ur.UserCode=u.UserId 
            left join TbDepartment dp on dp.DepartmentId=ur.DeptId
            left join TbRole r on ur.RoleCode=r.RoleId 
            where ur.OrgId=@CompanyId and ur.ProjectId=@ProjectId and Flag=0 and u.UserName is not null order by ur.UserCode asc";
                    }

                    DataTable dt4 = Db.Context.FromSql(sql4)
                        .AddInParameter("@ProjectId", DbType.String, ProjectId)
                        .AddInParameter("@CompanyId", DbType.String, CompanyId).ToDataTable();
                    for (int i = 0; i < list3.Count; i++)
                    {
                        DataTable dt5 = new DataTable();
                        DataRow[] dr = dt4.Select("pid='" + list3[i].id + "' and DeptId='" + list3[i].pid + "'");
                        if (dr != null && dr.Count() > 0)
                        {
                            dt5 = dr.CopyToDataTable();
                        }
                        list4.AddRange(ModelConvertHelper<OrgEntity>.ToList(dt5));
                    }

                    list.AddRange(list4);
                }

            }
            return list;

        }

        public DataTable GetNodeNew(string FlowCode, string FlowNodeCode)
        {
            //查找当前节点
            var ret = Db.Context.From<TbFlowNode>().Where(p => p.FlowCode == FlowCode && p.FlowNodeCode == FlowNodeCode).ToDataTable();
            return ret;
        }
        public DataTable GetNodePersonnelNew(string FlowCode, string FlowNodeCode, string OrgType)
        {
            string ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
            //查找当前节点下的人员
            string sql = @"select a.id,b.id as UserTypeId,b.Name,case a.ActionType when -1 then '抄送' else '审批' end as ActionType,a.PersonnelSource  as TypeName,b.DeptId,b.DeptName,b.RoleId,b.RoleName,b.ProjectId,a.OrgId,a.OrgType
                            from TbFlowNodePersonnel a left join (
                            select TbCompany.ParentCompanyCode as pid,TbCompany.CompanyCode as id,TbCompany.CompanyFullName as Name,'Org' as TypeName,'' as DeptId,'' as DeptName,'' as RoleId,'' as RoleName,pc.ProjectId from TbCompany left join TbProjectCompany pc on TbCompany.CompanyCode=pc.CompanyCode 
                            union all
                            select '0' as pid,DepartmentId as id,DepartmentName as Name,'Dep' as TypeName,'' as DeptId,DepartmentName as DeptName,'' as RoleId,'' as RoleName,DepartmentProjectId as ProjectId from TbDepartment 
                            union all
                            select r.DepartmentId as pid,r.RoleId as id,r.RoleName as Name,'Role' as TypeName,r.DepartmentId,dp.DepartmentName as DeptName,r.RoleId,r.RoleName,dp.DepartmentProjectId as ProjectId  from TbRole r left join TbDepartment dp on r.DepartmentId=dp.DepartmentId where RoleName<>'系统管理员' 
                            union all
                            select r.RoleCode as pid,u.UserId as id,UserName as Name,'Personnel' as TypeName,r.DeptId,dp.DepartmentName as DeptName,r.RoleCode,r1.RoleName,r.ProjectId from TbUser u 
                            left join TbUserRole r on u.UserId=r.UserCode 
                            left join TbDepartment dp on dp.DepartmentId=r.DeptId 
                            left join TbRole r1 on r.RoleCode=r1.RoleId
                            where UserClosed<>'离职' and u.UserCode<>'admin' and r.Flag=0) b
                            on a.PersonnelCode=b.id and a.DeptId=b.DeptId and a.RoleId=b.RoleId 
                            where a.FlowCode=@FlowCode and a.FlowNodeCode=@FlowNodeCode and a.ProjectId=@ProjectId and a.OrgType=@OrgType";
            DataTable items = Db.Context.FromSql(sql)
                .AddInParameter("@FlowCode", DbType.String, FlowCode)
                .AddInParameter("@FlowNodeCode", DbType.String, FlowNodeCode)
                .AddInParameter("@OrgType", DbType.String, OrgType)
                .AddInParameter("@ProjectId", DbType.String, ProjectId)
                .ToDataTable();
            return items;
        }
        /// <summary>
        /// 获取菜单流程事件
        /// </summary>
        /// <param name="FormCode"></param>
        /// <returns></returns>
        public DataTable GetFlowNodeEvent(string FormCode)
        {
            var ret = Db.Context.From<TbFlowEvent>()
                    .Select(
                      TbFlowEvent._.All
                    , TbSysMenu._.MenuName)
                  .LeftJoin<TbSysMenu>((a, c) => a.FormCode == c.MenuCode).Where(p => p.FormCode == FormCode).ToDataTable();
            return ret;
        }

        public DataTable GetNodeEarlyWarningNew(string EarlyWarningCode)
        {
            //查找当前节点
            var ret = Db.Context.From<TbFlowEarlyWarningCondition>()
                .Select(TbFlowEarlyWarningCondition._.All
                , TbFlowNode._.FlowNodeName)
                .LeftJoin<TbFlowNode>((a, c) => a.FlowCode == c.FlowCode && a.FlowNodeCode == c.FlowNodeCode)
                .Where(p => p.EarlyWarningCode == EarlyWarningCode).ToDataTable();
            return ret;
        }
        public DataTable GetNodeEarlyWarningPersonnelNew(string FlowCode, string FlowNodeCode, string EarlyWarningCode, string OrgType)
        {
            string ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
            //查找当前节点下的人员
            string sql = @"select a.id,b.id as UserTypeId,b.Name,case when ActionType=6 then '审批超时预警' end as ActionType,a.PersonnelSource  as TypeName,b.DeptId,b.DeptName,b.RoleId,b.RoleName,b.ProjectId,a.OrgId,a.OrgType
                            from TbFlowNodeEarlyWarningPersonnel a left join (
                            select TbCompany.ParentCompanyCode as pid,TbCompany.CompanyCode as id,TbCompany.CompanyFullName as Name,'Org' as TypeName,'' as DeptId,'' as DeptName,'' as RoleId,'' as RoleName,pc.ProjectId from TbCompany left join TbProjectCompany pc on TbCompany.CompanyCode=pc.CompanyCode 
                            union all
                            select '0' as pid,DepartmentId as id,DepartmentName as Name,'Dep' as TypeName,'' as DeptId,DepartmentName as DeptName,'' as RoleId,'' as RoleName,DepartmentProjectId as ProjectId from TbDepartment 
                            union all
                            select r.DepartmentId as pid,r.RoleId as id,r.RoleName as Name,'Role' as TypeName,r.DepartmentId,dp.DepartmentName as DeptName,r.RoleId,r.RoleName,dp.DepartmentProjectId as ProjectId  from TbRole r left join TbDepartment dp on r.DepartmentId=dp.DepartmentId where RoleName<>'系统管理员' 
                            union all
                            select r.RoleCode as pid,u.UserId as id,UserName as Name,'Personnel' as TypeName,r.DeptId,dp.DepartmentName as DeptName,r.RoleCode,r1.RoleName,r.ProjectId from TbUser u 
                            left join TbUserRole r on u.UserId=r.UserCode 
                            left join TbDepartment dp on dp.DepartmentId=r.DeptId 
                            left join TbRole r1 on r.RoleCode=r1.RoleId
                            where UserClosed<>'离职' and u.UserCode<>'admin' and r.Flag=0) b
                            on a.PersonnelCode=b.id and a.DeptId=b.DeptId and a.RoleId=b.RoleId 
                            where a.FlowCode=@FlowCode and a.FlowNodeCode=@FlowNodeCode and a.EarlyWarningCode=@EarlyWarningCode and a.ProjectId=@ProjectId and a.OrgType=@OrgType";
            DataTable items = Db.Context.FromSql(sql)
                .AddInParameter("@FlowCode", DbType.String, FlowCode)
                .AddInParameter("@FlowNodeCode", DbType.String, FlowNodeCode)
                .AddInParameter("@EarlyWarningCode", DbType.String, EarlyWarningCode)
                .AddInParameter("@OrgType", DbType.String, OrgType)
                .AddInParameter("@ProjectId", DbType.String, ProjectId)
                .ToDataTable();
            return items;
        }

        /// <summary>
        /// 新增预警条件
        /// </summary>
        /// <param name="EarylWarningInfo"></param>
        /// <returns></returns>
        public AjaxResult AddEarylWarning(string EarylWarningInfo)
        {
            DbTrans trans = null;
            try
            {
                using (trans = Db.Context.BeginTransaction())
                {
                    //添加预警信息
                    JObject NodeObj = (JObject)JsonConvert.DeserializeObject(EarylWarningInfo);
                    string FlowCode = Convert.ToString(NodeObj["FlowCode"]);
                    string FlowNodeCode = Convert.ToString(NodeObj["FlowNodeCode"]);
                    string EarlyWarningCode = Convert.ToString(NodeObj["EarlyWarningCode"]);
                    string EarlyWarningName = Convert.ToString(NodeObj["EarlyWarningName"]);
                    int EarlyWarningTime = Convert.ToInt32(NodeObj["EarlyWarningTime"]);
                    string EarlyWarningTimeType = Convert.ToString(NodeObj["EarlyWarningTimeType"]);
                    int EarlyWarningOrder = Convert.ToInt32(NodeObj["EarlyWarningOrder"]);
                    string Remark = Convert.ToString(NodeObj["Remark"]);
                    int App = Convert.ToInt32(NodeObj["App"]);
                    int Pc = Convert.ToInt32(NodeObj["Pc"]);
                    int IsStart = Convert.ToInt32(NodeObj["IsStart"]);
                    JArray array = (JArray)NodeObj["SPLIST"];
                    TbFlowEarlyWarningCondition model = new TbFlowEarlyWarningCondition();
                    model.EarlyWarningCode = EarlyWarningCode;
                    model.FlowCode = FlowCode;
                    model.FlowNodeCode = FlowNodeCode;
                    model.EarlyWarningName = EarlyWarningName;
                    model.EarlyWarningTime = EarlyWarningTime;
                    model.EarlyWarningTimeType = EarlyWarningTimeType;
                    model.EarlyWarningOrder = EarlyWarningOrder;
                    model.Remark = Remark;
                    model.FlowType = "New";
                    model.App = App;
                    model.Pc = Pc;
                    model.IsStart = IsStart;
                    List<TbFlowNodeEarlyWarningPersonnel> plist = new List<TbFlowNodeEarlyWarningPersonnel>();
                    if (array.Count > 0 && Convert.ToString(array[0]) != "[]")
                    {
                        for (int j = 0; j < array.Count; j++)
                        {
                            JObject objuser = (JObject)array[j];
                            TbFlowNodeEarlyWarningPersonnel modeluser = new TbFlowNodeEarlyWarningPersonnel();
                            modeluser.FlowCode = Convert.ToString(FlowCode);
                            modeluser.FlowNodeCode = Convert.ToString(FlowNodeCode);
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
                            string source = string.Empty;
                            switch (Convert.ToString(array[j]["TypeName"]))
                            {
                                case "部门":
                                    source = "Department";
                                    break;
                                case "角色":
                                    source = "Role";
                                    break;
                                case "人员":
                                    source = "Personnel";
                                    break;
                            }
                            modeluser.PersonnelSource = source;
                            modeluser.EarlyWarningCode = EarlyWarningCode;
                            modeluser.DeptId = Convert.ToString(objuser["DeptId"]);
                            modeluser.RoleId = Convert.ToString(objuser["RoleId"]);
                            modeluser.ProjectId = Convert.ToString(objuser["ProjectId"]);
                            modeluser.OrgId = Convert.ToString(objuser["OrgId"]);
                            modeluser.OrgType = Convert.ToString(objuser["OrgType"]);
                            plist.Add(modeluser);
                        }
                    }
                    //添加预警条件
                    Repository<TbFlowEarlyWarningCondition>.Insert(trans, model);
                    //添加预警人员
                    Repository<TbFlowNodeEarlyWarningPersonnel>.Insert(trans, plist);
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
        /// 修改预警条件
        /// </summary>
        /// <param name="EarylWarningInfo"></param>
        /// <returns></returns>
        public AjaxResult UpdateEarylWarning(string EarylWarningInfo)
        {
            DbTrans trans = null;
            try
            {
                using (trans = Db.Context.BeginTransaction())
                {
                    //添加预警信息
                    JObject NodeObj = (JObject)JsonConvert.DeserializeObject(EarylWarningInfo);
                    string FlowCode = Convert.ToString(NodeObj["FlowCode"]);
                    string FlowNodeCode = Convert.ToString(NodeObj["FlowNodeCode"]);
                    string EarlyWarningCode = Convert.ToString(NodeObj["EarlyWarningCode"]);
                    string EarlyWarningName = Convert.ToString(NodeObj["EarlyWarningName"]);
                    int EarlyWarningTime = Convert.ToInt32(NodeObj["EarlyWarningTime"]);
                    string EarlyWarningTimeType = Convert.ToString(NodeObj["EarlyWarningTimeType"]);
                    int EarlyWarningOrder = Convert.ToInt32(NodeObj["EarlyWarningOrder"]);
                    string Remark = Convert.ToString(NodeObj["Remark"]);
                    int App = Convert.ToInt32(NodeObj["App"]);
                    int Pc = Convert.ToInt32(NodeObj["Pc"]);
                    int IsStart = Convert.ToInt32(NodeObj["IsStart"]);
                    JArray array = (JArray)NodeObj["SPLIST"];
                    var model = Repository<TbFlowEarlyWarningCondition>.First(p => p.EarlyWarningCode == EarlyWarningCode);
                    if (model != null)
                    {
                        model.EarlyWarningCode = EarlyWarningCode;
                        model.FlowCode = FlowCode;
                        model.FlowNodeCode = FlowNodeCode;
                        model.EarlyWarningName = EarlyWarningName;
                        model.EarlyWarningTime = EarlyWarningTime;
                        model.EarlyWarningTimeType = EarlyWarningTimeType;
                        model.EarlyWarningOrder = EarlyWarningOrder;
                        model.Remark = Remark;
                        model.FlowType = "New";
                        model.App = App;
                        model.Pc = Pc;
                        model.IsStart = IsStart;
                    }
                    List<TbFlowNodeEarlyWarningPersonnel> plist = new List<TbFlowNodeEarlyWarningPersonnel>();
                    if (array.Count > 0 && Convert.ToString(array[0]) != "[]")
                    {
                        for (int j = 0; j < array.Count; j++)
                        {
                            JObject objuser = (JObject)array[j];
                            TbFlowNodeEarlyWarningPersonnel modeluser = new TbFlowNodeEarlyWarningPersonnel();
                            modeluser.FlowCode = Convert.ToString(FlowCode);
                            modeluser.FlowNodeCode = Convert.ToString(FlowNodeCode);
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
                            string source = string.Empty;
                            switch (Convert.ToString(array[j]["TypeName"]))
                            {
                                case "部门":
                                    source = "Department";
                                    break;
                                case "角色":
                                    source = "Role";
                                    break;
                                case "人员":
                                    source = "Personnel";
                                    break;
                            }
                            modeluser.PersonnelSource = source;
                            modeluser.EarlyWarningCode = EarlyWarningCode;
                            modeluser.DeptId = Convert.ToString(objuser["DeptId"]);
                            modeluser.RoleId = Convert.ToString(objuser["RoleId"]);
                            modeluser.ProjectId = Convert.ToString(objuser["ProjectId"]);
                            modeluser.OrgId = Convert.ToString(objuser["OrgId"]);
                            modeluser.OrgType = Convert.ToString(objuser["OrgType"]);
                            plist.Add(modeluser);
                        }
                    }
                    //添加预警条件
                    Repository<TbFlowEarlyWarningCondition>.Update(trans, model);
                    //删除预警人员
                    Repository<TbFlowNodeEarlyWarningPersonnel>.Delete(trans, p => p.EarlyWarningCode == model.EarlyWarningCode);
                    //添加预警人员
                    Repository<TbFlowNodeEarlyWarningPersonnel>.Insert(trans, plist);
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
        #endregion
    }
}
