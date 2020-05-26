using Dos.ORM;
using PM.Common;
using PM.Common.Helper;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using PM.DataEntity.Flow;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.DataAccess.Flow
{
    public class FlowNodeDA : Repository<TbFlowNode>
    {
        /// <summary>
        /// 初始化流程开始、结束节点
        /// </summary>
        /// <param name="FlowCode"></param>
        /// <returns></returns>
        public int LoadingNode(string FlowCode)
        {
            try
            {
                ProcSection proc = Db.Context.FromProc("proc_loadingFlowNode")
                .AddInParameter("@FlowCode", DbType.String, FlowCode)
                .AddOutParameter("@result", DbType.Int32);
                proc.ExecuteNonQuery();
                int result = Convert.ToInt32(proc.GetReturnValues().First().Value);
                return result;
            }
            catch (Exception)
            {
                return 0;
            }
        }
        /// <summary>
        /// 添加节点
        /// </summary>
        /// <param name="FlowCode"></param>
        /// <param name="NodeName"></param>
        /// <param name="ActiveId"></param>
        /// <param name="FlowNodeCode"></param>
        /// <returns></returns>
        public int AddNode(string FlowCode, string NodeName, int ActiveId, int FlowNodeCode, int AllApproval, int FreeCandidates, int BlankNode, string NodeLeft, string NodeTop, string FlowNodeEvent)
        {
            int result = Db.Context.FromProc("Flow_NodeAdd")
                .AddInParameter("@FlowCode", DbType.String, FlowCode)
                .AddInParameter("@NodeName", DbType.String, NodeName)
                .AddInParameter("@CurrentNode", DbType.Int32, ActiveId)
                .AddInParameter("@AllApproval", DbType.Int32, AllApproval)
                .AddInParameter("@FreeCandidates", DbType.Int32, FreeCandidates)
                .AddInParameter("@BlankNode", DbType.Int32, BlankNode)
                .AddInParameter("@NodeLeft", DbType.String, NodeLeft)
                .AddInParameter("@NodeTop", DbType.String, NodeTop)
                .AddInParameter("@FlowNodeEvent", DbType.String, FlowNodeEvent)
                .AddInParameter("@NodeId", DbType.Int32, FlowNodeCode).ExecuteNonQuery();
            return result;
        }
        public DataTable GetNodePersonnel(string FLowCode, string FlowNodeCode)
        {
            string ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
//            string sql = @"select a.id,b.id as UserTypeId,b.Name,a.ActionType,case a.ActionType when -1 then '抄送' else '审批' end as ActionTypeName ,
//                            case a.PersonnelSource when 'Department' then '部门' when 'Role' then '角色' when 'Personnel' then '操作员' end as TypeName,b.DeptId,b.DeptName,b.RoleId,b.RoleName,b.ProjectId
//                            from TbFlowNodePersonnel a left join (
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
//                            where UserClosed<>'离职' and u.UserCode<>'admin' and r.Flag=0) b
//                            on a.PersonnelCode=b.id and a.DeptId=b.DeptId and a.RoleId=b.RoleId and a.ProjectId=b.ProjectId
//                            where a.FlowCode=@FlowCode and a.FlowNodeCode=@FlowNodeCode and a.ProjectId=@ProjectId";
            string sql = @"select a.id,b.id as UserTypeId,b.Name,a.ActionType,case a.ActionType when -1 then '抄送' else '审批' end as ActionTypeName ,
                            case a.PersonnelSource when 'Department' then '部门' when 'Role' then '角色' when 'Personnel' then '操作员' end as TypeName,b.DeptId,b.DeptName,b.RoleId,b.RoleName,b.ProjectId,a.OrgId
                            from TbFlowNodePersonnel a left join (
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
                            where UserClosed<>'离职' and u.UserCode<>'admin' and r.Flag=0) b
                            on a.PersonnelCode=b.id and a.DeptId=b.DeptId and a.RoleId=b.RoleId 
                            where a.FlowCode=@FlowCode and a.FlowNodeCode=@FlowNodeCode and a.ProjectId=@ProjectId";
            return Db.Context.FromSql(sql).AddInParameter("@FlowCode", DbType.String, FLowCode)
                .AddInParameter("@FlowNodeCode", DbType.String, FlowNodeCode).AddInParameter("@ProjectId", DbType.String, ProjectId).ToDataTable();
        }
        public int UpdateNode(string FlowCode, string NodeName, int FlowNodeCode, int AllApproval, int FreeCandidates, int BlankNode, string FlowNodeEvent, List<TbFlowNodePersonnel> list)
        {
            var model = GetNode(FlowCode, Convert.ToString(FlowNodeCode));
            model.FlowNodeName = NodeName;
            model.AllApproval = AllApproval;
            model.FreeCandidates = FreeCandidates;
            model.BlankNode = BlankNode;
            model.FlowNodeEvent = FlowNodeEvent;
            DbTrans trans = null;
            try
            {
                using (trans = Db.Context.BeginTransaction())
                {
                    string FlowNodeCode1 = Convert.ToString(FlowNodeCode);
                    Repository<TbFlowNode>.Update(trans, model);
                    Repository<TbFlowNodePersonnel>.Delete(trans, p => p.FlowCode == FlowCode && p.FlowNodeCode == FlowNodeCode1);
                    Repository<TbFlowNodePersonnel>.Insert(trans, list);
                    trans.Commit();//提交事务
                    return 1;
                }
            }
            catch (Exception)
            {
                if (trans != null) trans.Rollback();
                return 0;
            }
        }

        public TbFlowNode GetNode(string FlowCode, string FlowNodeCode)
        {
            return First(f => f.FlowCode == FlowCode && f.FlowNodeCode == FlowNodeCode);
        }

        public DataTable GetNodeList(string FlowCode)
        {
            string sql = @"select nodeui.FlowCode,nodeui.FlowNodeCode,nodeui.processData,nodeui.icon,nodeui.NodeLeft,nodeui.NodeTop,
                            node.FlowNodeName,node.AllApproval,node.FreeCandidates,node.AddSignature,node.AllowedSkip,node.LimitDay,
                            node.LimitHour,node.LimitMinutes,node.AllowedRejected,node.FreeRejected,node.RejectedToNodeCode,node.BlankNode,
                            even.ActionCode,even.StateCode,even.fmCode,even.EventDescription
                            ,background=(case when even.FlowNodeCode!='NULL' then 'red' end) 
                            from TbFlowNodeUI nodeui 
                            left join TbFlowNode node on nodeui.FlowCode=node.FlowCode and nodeui.FlowNodeCode=node.FlowNodeCode
                            left join TbFlowNodeEvent even on nodeui.FlowCode=even.FlowCode and nodeui.FlowNodeCode=even.FlowNodeCode   
                            where node.FlowCode=@FlowCode";
            DataTable table = Db.Context.FromSql(sql).AddInParameter("@FlowCode", DbType.String, FlowCode).ToDataTable();
            return table;
        }
        public List<TbFlowNode> GetAllByFlowCode(string FlowCode)
        {
            var where = new Where<TbFlowNode>();
            where.And(p => p.FlowCode == FlowCode);
            return Db.Context.From<TbFlowNode>().Where(where).ToList<TbFlowNode>();
        }
        public int DeleteNode(string FlowCode, string FlowNodeCode)
        {
            try
            {
                int NonQuery = Db.Context.FromProc("flow_deleteflownode")
                .AddInParameter("@FlowCode", DbType.String, FlowCode)
                .AddInParameter("@FlowNodeCode", DbType.String, FlowNodeCode)
                .ExecuteNonQuery();
                return NonQuery;
            }
            catch (Exception)
            {
                return 0;
            }

        }

        /// <summary>
        /// 获取组织机构下部门角色人员
        /// </summary>
        /// <param name="OrgType"></param>
        /// <returns></returns>
        public List<OrgEntity> GetDeptOrRoleOrUser(int OrgType, string CompanyId)
        {
            List<OrgEntity> list = new List<OrgEntity>();
            string ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
            //2、查询部门
            List<OrgEntity> list2 = new List<OrgEntity>();
            string sql2 = "";
            if (OrgType==1)
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
            //4、查询人员
            List<OrgEntity> list4 = new List<OrgEntity>();
            if (list3.Count > 0)
            {
                string sql4 = "";
                if (OrgType == 1)
                {
                    sql4 = @"select ur.RoleCode as pid,ur.UserCode+'-'+cast(ROW_NUMBER() OVER (ORDER BY NEWID()) as varchar) as id,u.UserName as Name,'User' as TypeName,cast(ur.OrgType as varchar) as OrgType,ur.ProjectId,ur.DeptId,dp.DepartmentName as DeptName,ur.RoleCode as RoleId,r.RoleName from TbUserRole ur
            left join TbUser u on ur.UserCode=u.UserId 
            left join TbDepartment dp on dp.DepartmentId=ur.DeptId
            left join TbRole r on ur.RoleCode=r.RoleId 
            where ur.OrgId=@CompanyId and Flag=0 and u.UserName is not null order by ur.UserCode asc";
                }
                else
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
            }
            list.AddRange(list4);
            return list;
        }
        /// <summary>
        /// 获取组织机构
        /// </summary>
        /// <returns></returns>
        public List<OrgEntity> GetOrg()
        {
            string ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
            string sql1 = @"select case when cp.ParentCompanyCode='1' then '0' else cp.ParentCompanyCode end as pid,cp.CompanyCode as id,cp.CompanyFullName as Name,'Org' as TypeName,cast(cp.OrgType as varchar) as OrgType,pc.ProjectId from TbCompany cp 
            left join TbProjectCompany pc on cp.CompanyCode=pc.CompanyCode  where pc.ProjectId=@ProjectId and cp.OrgType!=1  order by cp.id asc";
            DataTable dt1 = Db.Context.FromSql(sql1).AddInParameter("@ProjectId", DbType.String, ProjectId).ToDataTable();
            string sql2 = "select CompanyCode as id,CompanyFullName as Name,'Org' as TypeName,OrgType from TbCompany where OrgType=1";
            DataTable dt2 = Db.Context.FromSql(sql2).ToDataTable();
            if (dt2 != null && dt2.Rows.Count > 0)
            {
                DataTable dt3 = dt1.Select("pid='0'").CopyToDataTable();
                for (int i = 0; i < dt2.Rows.Count; i++)
                {
                    DataRow newRow = dt1.NewRow();
                    newRow["pid"] = dt3.Rows[0]["id"];
                    newRow["id"] = dt2.Rows[i]["id"];
                    newRow["Name"] = dt2.Rows[i]["Name"];
                    newRow["TypeName"] = dt2.Rows[i]["TypeName"];
                    newRow["OrgType"] = dt2.Rows[i]["OrgType"];
                    newRow["ProjectId"] = dt3.Rows[0]["ProjectId"];
                    dt1.Rows.Add(newRow);
                }
            }
            return ModelConvertHelper<OrgEntity>.ToList(dt1);

        }
        public DataTable GetLastNodeInfo(string FlowCode, string ChildNodeCode)
        {
            try
            {
                DataTable dt = Db.Context.FromSql("select * from TbFlowNodeUI where FlowCode=@FlowCode and   FlowNodeCode in (select ParentNodeCode from TbFlowNodeRelation where FlowCode=@FlowCode and ChildNodeCode=@ChildNodeCode)")
                    .AddInParameter("@FlowCode", DbType.String, FlowCode)
                    .AddInParameter("@ChildNodeCode", DbType.String, ChildNodeCode)
                    .ToDataTable();
                return dt;
            }
            catch (Exception)
            {
                return new DataTable();
            }
        }
        /// <summary>
        /// 流程编码获取节点名称
        /// </summary>
        /// <param name="FlowCode"></param>
        /// <returns></returns>
        public DataTable GetFlowNodeName(string FlowCode)
        {
            DataTable table = Db.Context.FromSql("select FlowNodeName from TbFlowNode where FlowCode=@FlowCode and FlowNodeCode in   (select FlowNodeCode from TbFlowNodeUI where processData='' and FlowNodeCode!=9999 and FlowCode=@FlowCode)")
                .AddInParameter("@FlowCode", DbType.String, FlowCode).ToDataTable();
            return table;
        }
        /// <summary>
        /// 通过组织机构类型查找相应的部门
        /// </summary>
        /// <param name="OrgType"></param>
        /// <returns></returns>
        public List<TbDepartment> GetDepartmentByOrgType(int OrgType)
        {
            return Db.Context.From<TbDepartment>().Select(TbDepartment._.DepartmentCode, TbDepartment._.DepartmentName)
                .LeftJoin<TbCompany>((a, b) => a.CompanyCode == b.CompanyCode)
                .Where(TbCompany._.OrgType == OrgType).ToList<TbDepartment>();
        }
        /// <summary>
        /// 通过部门编码获取部门下角色
        /// </summary>
        /// <param name="DepCode"></param>
        /// <returns></returns>
        public List<TbRole> GetRoleByDepCode(string DepCode)
        {
            return Db.Context.From<TbRole>().Select(TbRole._.RoleCode, TbRole._.RoleName)
                .LeftJoin<TbDepartment>((a, b) => a.DepartmentId == b.DepartmentId)
                .Where(TbDepartment._.DepartmentCode == DepCode).ToList();

        }

        public int UpdateNodeRelation(string FlowCode)
        {
            List<TbFlowNodeUI> list = Db.Context.From<TbFlowNodeUI>().Select(TbFlowNodeUI._.All).Where(p => p.FlowCode == FlowCode).ToList<TbFlowNodeUI>();
            List<TbFlowNodeRelation> rlist = new List<TbFlowNodeRelation>();
            #region 数据处理
            if (list != null)
            {
                foreach (TbFlowNodeUI nodeui in list)
                {
                    string[] array = nodeui.processData.Split(',');
                    int len = array.Length;
                    if (len > 1)
                    {
                        for (int i = 0; i < len; i++)
                        {
                            TbFlowNodeRelation model = new TbFlowNodeRelation()
                            {
                                FlowCode = nodeui.FlowCode,
                                ParentNodeCode = nodeui.FlowNodeCode,
                                ChildNodeCode = array[i]
                            };
                            rlist.Add(model);
                        }
                    }
                    else
                    {
                        TbFlowNodeRelation model = new TbFlowNodeRelation()
                        {
                            FlowCode = nodeui.FlowCode,
                            ParentNodeCode = nodeui.FlowNodeCode,
                            ChildNodeCode = nodeui.processData
                        };
                        rlist.Add(model);
                    }
                }
            }
            #endregion
            int result = 0;
            result = Repository<TbFlowNodeRelation>.Delete(p => p.FlowCode == FlowCode);
            if (result > 0)
                result = Repository<TbFlowNodeRelation>.Insert(rlist);
            return result;
        }

        #region
        /// <summary>
        /// 获取下级没有设置执行人的自由选择人节点
        /// </summary>
        /// <param name="FlowCode">流程代码</param>
        /// <param name="FlowPerformID">流程执行流水号</param>
        /// <param name="CurrNodeCode">当前执行节点代码</param>
        /// <returns></returns>
        public DataTable GetChildNoUserFreeNode(string FlowCode, string FlowPerformID, int CurrNodeCode)
        {
            try
            {
                if (CurrNodeCode == 9999)//当前节点是结束节点
                {
                    return null;
                }
                string strSQL = string.Empty;
                if (CurrNodeCode == 0) //是发起节点，则当前流程还未发出
                {
                    //获取下级节点中的没有设置执行人的自由选人节点 
                    strSQL = "Select '' as PersonnelCode,'' as PersonnelName,a.FlowCode,a.FlowNodeCode,a.FlowNodeName From (Select * From TbFlowNode Where FlowCode='" + FlowCode + "' And FreeCandidates=1 And FlowNodeCode In (" +
                       "Select ChildNodeCode From TbFlowNodeRelation Where FlowCode='" + FlowCode + "' And ChildNodeCode<>9999 And ParentNodeCode=0) " +
                       ") as a Left Join (Select * From TbFlowNodePersonnel Where FlowCode='" + FlowCode + "' And ActionType=0) as b " +
                       " On a.FlowCode=b.FlowCode And a.FlowNodeCode=b.FlowNodeCode Where b.id is  null ";
                }
                else //是流程执行过程中
                {
                    //获取下级节点中的没有设置执行人的自由选人节点
                    strSQL = "Select '' as PersonnelCode,'' as PersonnelName,a.FlowCode,a.FlowNodeCode,a.FlowNodeName From (Select * From TbFlowPerformNode Where FlowPerformID='" + FlowPerformID + "' And FreeCandidates=1 And FlowNodeCode In (" +
                            "Select ChildNodeCode From TbFlowPerformNodeRelation Where FlowPerformID='" + FlowPerformID + "' And ChildNodeCode<>9999 And ParentNodeCode=" + CurrNodeCode.ToString() + ")" +
                            ") as a Left Join (Select * From TbFlowPerformNodePersonnel Where FlowPerformID='" + FlowPerformID + "' And ActionType=0) as b" +
                            " On a.FlowPerformID=b.FlowPerformID And a.FlowNodeCode=b.FlowNodeCode Where b.id is  null";
                }
                DataTable tbChildNode = Db.Context.FromSql(strSQL).ToDataTable();//获取下级自由选人节点
                if (tbChildNode == null || tbChildNode.Rows.Count <= 0)//没有下级自由选人节点
                {
                    return null;
                }
                if (CurrNodeCode != 0)
                {
                    //判断下级自由选人节点的所有父节点是否审批完成
                    DataTable dtFreeNodes = tbChildNode.Clone();
                    string parentNodeStr = "select a.ParentNodeCode from TbFlowPerformNodeRelation a left join TbFlowPerformNode b on a.FlowPerformID=b.FlowPerformID and b.FlowNodeCode=a.ParentNodeCode " +
                                                        "where a.FlowPerformID=@FlowPerformID and a.ChildNodeCode=@ChildNodeCode and a.ParentNodeCode!=@ParentNodeCode and b.FlowNodeState not in (3,7,8)";
                    DataTable dtParentNode = null;//未完成审批的父节点
                    foreach (DataRow drChildNode in tbChildNode.Rows)
                    {
                        dtParentNode = Db.Context.FromSql(parentNodeStr).AddInParameter("@FlowPerformID", DbType.String, FlowPerformID)
                            .AddInParameter("@ChildNodeCode", DbType.Int32, drChildNode["FlowNodeCode"])
                            .AddInParameter("@ParentNodeCode", DbType.Int32, CurrNodeCode).ToDataTable();
                        if (dtParentNode != null && dtParentNode.Rows.Count == 0)//所有上级节点都已审批完成
                        {
                            dtFreeNodes.ImportRow(drChildNode);

                        }
                    }
                    return dtFreeNodes;
                }
                else
                {
                    return tbChildNode;
                }
            }
            catch (Exception ex)
            {
                throw null;
            }
        }

        #endregion
    }
}
