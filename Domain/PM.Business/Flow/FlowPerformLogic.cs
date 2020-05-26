using Dos.ORM;
using Newtonsoft.Json;
using PM.Business.ShortMessage;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Reflection;
using System.Text;
using System.Data.SqlClient;
using PM.DataEntity.RawMaterial.ViewModel;
using PM.Business.RawMaterial;

namespace PM.Business
{
    public class FlowPerformLogic
    {
        private readonly EndingStocksLogic _endingStocks = new EndingStocksLogic();

        #region 发起流程
        /// <summary>
        /// 发出流程
        /// </summary>
        /// <param name="FlowCode">流程编码</param>
        /// <param name="FormCode">表单编码</param>
        /// <param name="FormDataCode">数据ID</param>
        /// <param name="UserCode">用户编码</param>
        /// <param name="FlowTitle">流程主题</param>
        /// <param name="FlowLevel">流程级别</param>
        /// <param name="FinalCutoffTime">流程截止日期</param>
        public AjaxResult InitFlowPerform(string FlowCode, string FormCode, string FormDataCode, string UserCode, string FlowTitle, string FlowLevel, DateTime? FinalCutoffTime)
        {
            int result = 0;
            string message = string.Empty;
            try
            {
                var proc = Db.Context.FromProc("Flow_InitFlowPerform")
                .AddInParameter("@FlowCode", DbType.String, FlowCode)
                .AddInParameter("@FormCode", DbType.String, FormCode)
                .AddInParameter("@FormDataCode", DbType.String, FormDataCode)
                .AddInParameter("@UserCode", DbType.String, UserCode)
                .AddInParameter("@FlowTitle", DbType.String, FlowTitle)
                .AddInParameter("@FlowLevel", DbType.String, FlowLevel)
                .AddInParameter("@FinalCutoffTime", DbType.DateTime, FinalCutoffTime)
                .AddOutParameter("@message", DbType.String, 100);
                result = proc.ExecuteNonQuery();
                Dictionary<string, object> returnValue = proc.GetReturnValues();
                foreach (KeyValuePair<string, object> kv in returnValue)
                {
                    message = Convert.ToString(kv.Value);
                }
                if (result > 0 && string.IsNullOrEmpty(message))
                {
                    return AjaxResult.Success("流程发起成功");
                }
                else
                {
                    return AjaxResult.Error(message);
                }
            }
            catch (Exception e)
            {
                return AjaxResult.Error(e.Message);
            }
        }
        public string GetCount(string usercode, string ProjectId)
        {
            string json = "{0}\"ApprovalCount\":{1},\"MsgCount\":{2},\"earlywarningcount\":{3}{4}";
            string sql = @"select COUNT(1) as MsgCount from TbFlowPerformOpinions o
                           left join TbFlowPerform p on o.FlowPerformID=p.FlowPerformID
                           left join TbFlowDefine d on p.FlowCode=d.FlowCode
                           where o.UserCode=@UserCode and (ISNULL(@ProjectId,'')='' or d.ProjectId=@ProjectId) and (PerformState=-1 or PerformState=7)
                           union all
                           select COUNT(1) as MsgCount from TbFormEarlyWarningNodeInfo m where m.MsgType=1 and m.EWStart=0 and m.EWUserCode=@UserCode and (ISNULL(@ProjectId,'')='' or m.ProjectId=@ProjectId)
                           union all
                           select SUM(Tb.EarlyCount) as earlywarningcount from (select COUNT(1) as EarlyCount from TbFormEarlyWarningNodeInfo m where m.MsgType=2 and m.EWStart=0 and m.EWUserCode=@UserCode and (ISNULL(@ProjectId,'')='' or m.ProjectId=@ProjectId)
                           union all
                           select COUNT(1) as EarlyCount from TbFlowEarlyWarningOtherInfo o where o.EarlyWarningStart=0 and o.FlowYjUserCode=@UserCode and (ISNULL(@ProjectId,'')='' or o.ProjectId=@ProjectId)) Tb";
            DataTable dt = Db.Context.FromSql(sql)
                .AddInParameter("@UserCode", DbType.String, usercode)
                .AddInParameter("@ProjectId", DbType.String, ProjectId)
                .ToDataTable();
            if (dt != null && dt.Rows.Count > 0)
            {
                json = string.Format(json, "{", Convert.ToString(dt.Rows[0]["MsgCount"]), Convert.ToString(dt.Rows[1]["MsgCount"]), Convert.ToString(dt.Rows[2]["MsgCount"]), "}");
            }
            else
            {
                json = string.Format(json, "{", "0", "0", "0", "}");
            }
            return json;
        }

        public DataTable GetAppCount(string UserId, string ProjectId)
        {
            string sql = @"select COUNT(1) as MsgCount from TbFlowPerformOpinions o
                           left join TbFlowPerform p on o.FlowPerformID=p.FlowPerformID
                           left join TbFlowDefine d on p.FlowCode=d.FlowCode
                           where o.UserCode=@UserCode and (ISNULL(@ProjectId,'')='' or d.ProjectId=@ProjectId) and (PerformState=-1 or PerformState=7)
                           union all
                           select COUNT(1) as MsgCount from TbFormEarlyWarningNodeInfo m where m.MsgType=1 and m.EWStart=0 and m.EWUserCode=@UserCode and (ISNULL(@ProjectId,'')='' or m.ProjectId=@ProjectId)
                           union all
                           select SUM(Tb.EarlyCount) as earlywarningcount from (select COUNT(1) as EarlyCount from TbFormEarlyWarningNodeInfo m where m.MsgType=2 and m.EWStart=0 and m.EWUserCode=@UserCode and (ISNULL(@ProjectId,'')='' or m.ProjectId=@ProjectId)
                           union all
                           select COUNT(1) as EarlyCount from TbFlowEarlyWarningOtherInfo o where o.EarlyWarningStart=0 and o.FlowYjUserCode=@UserCode and (ISNULL(@ProjectId,'')='' or o.ProjectId=@ProjectId)) Tb";
            DataTable dt = Db.Context.FromSql(sql)
                .AddInParameter("@UserCode", DbType.String, UserId)
                .AddInParameter("@ProjectId", DbType.String, ProjectId)
                .ToDataTable();
            return dt;
        }
        #endregion

        #region 我的发起
        /// <summary>
        /// 获取我的发起
        /// </summary>
        /// <param name="status">发起流程状态-1：审批中 9：审核完成 3：已退回 1：已终止</param>
        /// <returns>发起内容</returns>
        public string GetMyInitiate(PageSearchRequest pt, int status, string usercode, DateTime sdt, DateTime edt)
        {
            string sql = @"select distinct case m.FlowLevel when 0 then '普通' when 1 then '加急' end as FlowLevel ,
            m.FlowTitle,m.BeginTime,m.EndTime,s.MenuName,m.FlowPerformID,
            (select top 1 PerformDate from TbFlowPerformOpinions where FlowPerformID=m.FlowPerformID and (PerformDate is not null or PerformDate!='') order by PerformDate desc) as PerformDate,
            m.FlowCode,m.FormCode,m.FormDataCode,
            case m.FlowState when -1 then '审批中' when 9 then '审核完成' when 3 then '已退回' when 4 then '已终止' when 2 then '已撤销' end as FlowState,
            m.FlowState as FlowPerFormState,fd.ProjectId
            from TbFlowPerform m
            left join TbFlowDefine fd on m.FlowCode=m.FlowCode
            left join TbSysMenu s on m.FormCode=s.MenuCode
            where UserCode=@UserCode
            and ((ISNULL(@FlowState,'')='') or (m.FlowState=@FlowState))
            and ((ISNULL(@ProjectId,'')='') or fd.ProjectId=@ProjectId)
            and m.BeginTime>=@SDT and m.BeginTime<=@EDT order by m.BeginTime desc";
            DataTable dt = Db.Context.FromSql(sql).AddInParameter("@UserCode", DbType.String, usercode)
                .AddInParameter("@FlowState", DbType.Int32, status)
                .AddInParameter("@SDT", DbType.DateTime, sdt)
                .AddInParameter("@EDT", DbType.DateTime, edt)
                .AddInParameter("@ProjectId", DbType.String, OperatorProvider.Provider.CurrentUser.ProjectId)
                .ToDataTable();

            if (dt != null)
            {
                try
                {
                    var count = dt.Rows.Count;
                    var model = new PageModel(pt.page, pt.rows, count, dt);
                    return JsonConvert.SerializeObject(model);
                }
                catch (Exception)
                {
                    return "[]";
                }
            }
            else return "[]";
        }
        /// <summary>
        /// 撤销我发起的流程
        /// </summary>
        /// <param name="flowperformid">流程ID</param>
        /// <returns></returns>
        public AjaxResult Cancel(string flowperformid)
        {
            int result = Db.Context.FromProc("Flow_CancelFlow").AddInParameter("@FlowPerformID", DbType.String, flowperformid).ExecuteNonQuery();
            if (result > 0) return AjaxResult.Success();
            else return AjaxResult.Error("流程撤销失败");
        }

        public string GetMyInitiateStatus(string UserCode)
        {
            string sql = @"select FlowState,case FlowState when -1 then '审批中【'+CONVERT(varchar(30),COUNT(1))+'】' 
                            when 9 then '审核完成【'+CONVERT(varchar(30),COUNT(1))+'】'  
                            when 4 then '已终止【'+CONVERT(varchar(30),COUNT(1))+'】' 
                            when 3 then '已退回【'+CONVERT(varchar(30),COUNT(1))+'】' 
                            when 2 then '已撤销【'+CONVERT(varchar(30),COUNT(1))+'】' end as FlowStateName 
                            from TbFlowPerform where UserCode=@UserCode group by FlowState";
            DataTable dt = Db.Context.FromSql(sql).AddInParameter("@UserCode", DbType.String, UserCode).ToDataTable();
            if (dt != null)
                return JsonConvert.SerializeObject(dt);
            else return "[]";
        }
        #endregion

        #region 我的审批
        /// <summary>
        /// 获取我的审批
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="usercode"></param>
        /// <param name="PerformState"></param>
        /// <returns></returns>
        public string GetMyApproval(PageSearchRequest pt, string usercode, int PerformState, DateTime? SDT, DateTime? EDT)
        {
            DataTable dt = Db.Context.FromProc("SP_GetMyApproval")
                .AddInParameter("@UserCode", DbType.String, usercode)
                .AddInParameter("@PerformState", DbType.Int16, PerformState)
                .AddInParameter("@SDT", DbType.DateTime, SDT)
                .AddInParameter("@EDT", DbType.DateTime, EDT)
                .AddInParameter("@ProjectId", DbType.String, OperatorProvider.Provider.CurrentUser.ProjectId).ToDataTable();
            if (dt != null)
                return JsonConvert.SerializeObject(dt);
            else return "[]";
            //string sql = @"exec SP_GetMyApproval '" + usercode + "'," + PerformState + ",'" + SDT + "','" + EDT + "','" + OperatorProvider.Provider.CurrentUser.ProjectId + "'";
            //List<Dos.ORM.Parameter> para = new List<Dos.ORM.Parameter>();
            //var ret = Repository<TbWorkOrder>.FromSqlToPageTable(sql, para, pt.rows, pt.page, "FlowPerformID", "desc");
            //return ret;
        }
        /// <summary>
        /// 获取App我的审批列表
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="usercode"></param>
        /// <param name="PerformState"></param>
        /// <returns></returns>
        public DataTable GetAppMyApproval(PageSearchRequest pt, string usercode, string ProjectId, int PerformState, DateTime? SDT, DateTime? EDT)
        {
            DataTable dt = Db.Context.FromProc("SP_GetMyApproval")
                .AddInParameter("@UserCode", DbType.String, usercode)
                .AddInParameter("@PerformState", DbType.Int16, PerformState)
                .AddInParameter("@SDT", DbType.DateTime, SDT)
                .AddInParameter("@EDT", DbType.DateTime, EDT)
                .AddInParameter("@ProjectId", DbType.String, ProjectId).ToDataTable();
            return dt;
        }
        /// <summary>
        /// 获取审批过程中的审批数据
        /// </summary>
        /// <param name="FormCode"></param>
        /// <param name="FormDataCode"></param>
        /// <returns></returns>
        public string GetApprovalInfo(string FormCode, string FormDataCode)
        {
            string sql = @"select case f.FlowState when -1 then '审批中' 
                            when 9 then '审核完成'  
                            when 4 then '已终止' 
                            when 3 then '已退回' 
                            when 2 then '已撤销' end as FlowState,f.FlowTitle,'1' as FlowNodeCode,f.FlowCode,f.FormCode,f.FormDataCode,u.UserName,f.FlowPerformID,f.FlowCode,
            case f.FlowLevel when 0 then '普通' when 1 then '加急' when 2 then '紧急' End as FlowLevel,
            (select MAX(PerformDate) from TbFlowPerformOpinions where FlowPerformID=f.FlowPerformID) as PreNodeCompleteDate,
            f.BeginTime,f.EndTime
            from TbFlowPerform f
            left join TbUser u on f.UserCode=u.UserCode
            where f.FormCode=@FormCode and f.FormDataCode=@FormDataCode order by f.FlowPerformID desc";
            DataTable dt = Db.Context.FromSql(sql).AddInParameter("@FormCode", DbType.String, FormCode)
                .AddInParameter("@FormDataCode", DbType.String, FormDataCode).ToDataTable();
            if (dt != null)
            {
                return JsonConvert.SerializeObject(dt);
            }
            else return "[]";
        }

        /// <summary>
        /// 获取我的审批状态
        /// </summary>
        /// <param name="usercode">用户编码</param>
        /// <returns>状态列表</returns>
        public string GetMyApprovalState(string usercode, DateTime? SDT, DateTime? EDT)
        {
            string sql = @"select Tb1.*,isnull(Tb2.PerformStateCount,0) as PerformStateCount from (
select -1 as PerformState,'db' as PerformStateName
						   union all
						   select 7,'dbyy'
                           union all
                           select 0,'cs'
                           union all
                           select 1,'ty'
                           union all
                           select 3,'th'
                           union all
                           select 4,'cx'
                           union all
                           select 9,'qtyh') as Tb1
                           left join
                           (select o.PerformState,
                                  case o.PerformState when -1 then COUNT(1)
                                  when 7 then COUNT(1)
                                  when 0 then COUNT(1) 
                                  when 1 then COUNT(1)
                                  when 2 then COUNT(1)
                                  when 3 then COUNT(1) 
                                  when 4 then COUNT(1)
                                  when 5 then COUNT(1) 
                                  when 9 then COUNT(1)  end as PerformStateCount 
                                  from TbFlowPerformOpinions o
                                  left join TbFlowPerform p on o.FlowPerformID=p.FlowPerformID
                                  left join TbFlowDefine d on p.FlowCode=d.FlowCode
                                  where o.UserCode=@UserCode
                                  and (ISNULL(@SDT,'')='' or o.PreNodeCompleteDate>=@SDT) 
                                  and (ISNULL(@EDT,'')='' or o.PreNodeCompleteDate<@EDT)
                                  and (ISNULL(@ProjectId,'')='' or d.ProjectId=@ProjectId)
                                  group by o.PerformState) as Tb2 on Tb1.PerformState=Tb2.PerformState";
            DataTable dt = Db.Context.FromSql(sql)
                .AddInParameter("@UserCode", DbType.String, usercode)
                .AddInParameter("@SDT", DbType.DateTime, SDT)
                .AddInParameter("@EDT", DbType.DateTime, EDT)
                .AddInParameter("@ProjectId", DbType.String, OperatorProvider.Provider.CurrentUser.ProjectId).ToDataTable();
            if (dt != null)
                return JsonConvert.SerializeObject(dt);
            else return "[]";
        }

        /// <summary>
        /// 获取App我的审批状态
        /// </summary>
        /// <param name="usercode">用户编码</param>
        /// <returns>状态列表</returns>
        public DataTable GetAppMyApprovalState(string usercode, string ProjectId, DateTime? SDT, DateTime? EDT)
        {
            string sql = @"select Tb1.*,isnull(Tb2.PerformStateCount,0) as PerformStateCount from (
select -1 as PerformState,'db' as PerformStateName
						   union all
						   select 7,'dbyy'
                           union all
                           select 0,'cs'
                           union all
                           select 1,'ty'
                           union all
                           select 3,'th'
                           union all
                           select 4,'cx'
                           union all
                           select 9,'qtyh') as Tb1
                           left join
                           (select o.PerformState,
                                  case o.PerformState when -1 then COUNT(1)
                                  when 7 then COUNT(1)
                                  when 0 then COUNT(1) 
                                  when 1 then COUNT(1)
                                  when 2 then COUNT(1)
                                  when 3 then COUNT(1) 
                                  when 4 then COUNT(1)
                                  when 5 then COUNT(1) 
                                  when 9 then COUNT(1)  end as PerformStateCount 
                                  from TbFlowPerformOpinions o
                                  left join TbFlowPerform p on o.FlowPerformID=p.FlowPerformID
                                  left join TbFlowDefine d on p.FlowCode=d.FlowCode
                                  where o.UserCode=@UserCode
                                  and (ISNULL(@SDT,'')='' or o.PreNodeCompleteDate>=@SDT) 
                                  and (ISNULL(@EDT,'')='' or o.PreNodeCompleteDate<@EDT)
                                  and (ISNULL(@ProjectId,'')='' or d.ProjectId=@ProjectId)
                                  group by o.PerformState) as Tb2 on Tb1.PerformState=Tb2.PerformState";
            DataTable dt = Db.Context.FromSql(sql)
                .AddInParameter("@UserCode", DbType.String, usercode)
                .AddInParameter("@SDT", DbType.DateTime, SDT)
                .AddInParameter("@EDT", DbType.DateTime, EDT)
                .AddInParameter("@ProjectId", DbType.String, ProjectId).ToDataTable();
            return dt;
        }
        /// <summary>
        /// 获取原单据地址
        /// </summary>
        /// <param name="formCode">单据编码</param>
        /// <returns>原单据地址</returns>
        public string LoadOrderForm(string formCode)
        {
            string sql = "select MenuUrl from TbSysMenu where MenuCode=@MenuCode";
            StringBuilder sb = new StringBuilder();
            DataTable dt = Db.Context.FromSql(sql).AddInParameter("@MenuCode", DbType.String, formCode).ToDataTable();
            if (dt != null)
            {
                string url = Convert.ToString(dt.Rows[0]["MenuUrl"]);
                string[] arr = url.Split(new char[1] { '/' });
                for (int i = 0; i < arr.Length - 1; i++)
                {
                    sb.Append(arr[i]);
                    sb.Append("/");
                }
                sb.Append("Details");
            }
            return "{\"url\":\"" + sb.ToString() + "\"}";
        }

        public string LoadApprovalOptions(string flowperformid)
        {
            string sql = @"select * from ( select 1 as sort,'' as PerformState,'发起' as FlowNodeName,'' as UserName,BeginTime,null as PerformDate,'' as PerformOpinions,'' as UserType from TbFlowPerform where FlowPerformID=@FlowPerformID
            union all
          select 2 as sort,case o.PerformState when -1 then '未阅' 
                        when 0 then '已阅' when 1 then '同意'
                        when 2 then '不同意' when 3 then '退回'
                        when 4 then '撤销' when 5 then '超时跳过'
                        when 9 then '非会签其他用户已处理' end as PerformState,
            n.FlowNodeName,u.UserName,o.PreNodeCompleteDate,o.PerformDate,o.PerformOpinions,
            case o.UserType when -1 then '抄送人' when 0 then '执行人' end as UserType
             from (select top 9999 * from TbFlowPerformOpinions where FlowPerformID=@FlowPerformID order by PreNodeCompleteDate asc) o
            left join (select FlowPerformID,FlowCode,FlowNodeCode,FlowNodeName from TbFlowPerformNode where FlowPerformID=@FlowPerformID) n 
            on o.FlowPerformID=n.FlowPerformID and o.FlowNodeCode=n.FlowNodeCode
            left join TbUser u 
            on o.UserCode=u.UserId 
            union all
            select 3 as sort,'' as PerformState,'结束' as FlowNodeName,'' as UserName,EndTime,EndTime,'' as PerformOpinions,'' as UserType from TbFlowPerform where FlowPerformID=@FlowPerformID) Tb order by Tb.sort,Tb.BeginTime asc";
            DataTable dt = Db.Context.FromSql(sql).AddInParameter("@FlowPerformID", DbType.String, flowperformid).ToDataTable();
            if (dt != null)
                return JsonConvert.SerializeObject(dt);
            else return "[]";
        }
        /// <summary>
        /// 审批并发起流程
        /// </summary>
        /// <param name="FlowCode">流程编码</param>
        /// <param name="performState">流程状态</param>
        /// <param name="performOpinions">审批意见</param>
        /// <param name="FlowPerformID">流程流水号</param>
        /// <param name="flowNodeCode">流程节点编码</param>
        /// <param name="UserCode">用户编码</param>
        /// <param name="FlowTitle">流程主题</param>
        /// <param name="FreeNodeUser">自由选人用户编码</param>
        /// <returns></returns>
        public AjaxResult Approval(string FlowCode, int performState, string performOpinions, string FlowPerformID, string flowNodeCode, string UserCode, string FlowTitle, string FreeNodeUser)
        {
            string message = string.Empty;
            var proc = Db.Context.FromProc("Flow_DisposeApprovalOpinion")
                .AddInParameter("@FlowCode", DbType.String, FlowCode)
                .AddInParameter("@performState", DbType.Int32, performState)
                .AddInParameter("@performOpinions", DbType.String, performOpinions)
                .AddInParameter("@FlowPerformID", DbType.String, FlowPerformID)
                .AddInParameter("@flowNodeCode", DbType.String, flowNodeCode)
                .AddInParameter("@FreeNodeUser", DbType.String, FreeNodeUser)
                .AddInParameter("@UserCode", DbType.String, UserCode)
                .AddInParameter("@FlowTitle", DbType.String, FlowTitle)
                .AddOutParameter("@message", DbType.String, 50);
            int result = proc.ExecuteNonQuery();
            Dictionary<string, object> returnValue = proc.GetReturnValues();
            foreach (KeyValuePair<string, object> kv in returnValue)
            {
                message = Convert.ToString(kv.Value);
            }
            if (result > 0 && message.Length == 0)
            {
                return AjaxResult.Success();
            }
            else
            {
                return AjaxResult.Error(message);
            }
        }
        /// <summary>
        /// 修改加工订单中订单状态审批
        /// </summary>
        /// <param name="FlowCode"></param>
        /// <param name="performState"></param>
        /// <param name="performOpinions"></param>
        /// <param name="FlowPerformID"></param>
        /// <param name="flowNodeCode"></param>
        /// <param name="UserCode"></param>
        /// <param name="FlowTitle"></param>
        /// <param name="FreeNodeUser"></param>
        /// <returns></returns>
        public AjaxResult GetFlowWorkNode(string FlowCode, int performState, string performOpinions, string FlowPerformID, string flowNodeCode, string UserCode, string FlowTitle, string FreeNodeUser)
        {
            string message = string.Empty;
            try
            {
                string sql = @"select FormDataCode from TbFlowNode fn
left join TbFlowPerform fp on fn.FlowCode=fp.FlowCode
left join TbFlowPerformOpinions fpo on fpo.FlowPerformID=fp.FlowPerformID and fn.FlowNodeCode=fpo.FlowNodeCode where fn.FlowCode=@FlowCode and FlowNodeName like '%加工厂接收%' and fp.FlowPerformID=@FlowPerformID and (fpo.PerformState=-1 or fpo.PerformState=7)";
                DataTable dt = Db.Context.FromSql(sql).AddInParameter("@FlowCode", DbType.String, FlowCode).AddInParameter("@FlowPerformID", DbType.String, FlowPerformID)
                    .ToDataTable();
                bool flag = ApprovalBool(FlowCode, performState, performOpinions, FlowPerformID, flowNodeCode, UserCode, FlowTitle, FreeNodeUser);
                if (dt != null && dt.Rows.Count > 0)
                {
                    if (flag && performState == 1)
                    {
                        //修改加工订单加工状态
                        string sql1 = " update TbWorkOrder set ProcessingState='Received' where ID=" + Convert.ToInt32(dt.Rows[0]["FormDataCode"]) + "";
                        Db.Context.FromSql(sql1).ExecuteNonQuery();

                        //订单中的某种材料汇总重量≥当前库存重量的80%  系统发送预警信息给加工厂材料负责人，并以短信通知
                        _endingStocks.SendMsgForMaterialStockByOrder(Convert.ToInt32(dt.Rows[0]["FormDataCode"]));
                    }
                }
                if (flag)
                {
                    return AjaxResult.Success();
                }
                else
                {
                    return AjaxResult.Error("审批过程中发生错误");
                }
            }
            catch (Exception ex)
            {

                return AjaxResult.Error("审批过程中发生错误" + ex.Message);
            }
        }

        public bool ApprovalBool(string FlowCode, int performState, string performOpinions, string FlowPerformID, string flowNodeCode, string UserCode, string FlowTitle, string FreeNodeUser)
        {
            string message = string.Empty;
            var proc = Db.Context.FromProc("Flow_DisposeApprovalOpinion")
                .AddInParameter("@FlowCode", DbType.String, FlowCode)
                .AddInParameter("@performState", DbType.Int32, performState)
                .AddInParameter("@performOpinions", DbType.String, performOpinions)
                .AddInParameter("@FlowPerformID", DbType.String, FlowPerformID)
                .AddInParameter("@flowNodeCode", DbType.String, flowNodeCode)
                .AddInParameter("@FreeNodeUser", DbType.String, FreeNodeUser)
                .AddInParameter("@UserCode", DbType.String, UserCode)
                .AddInParameter("@FlowTitle", DbType.String, FlowTitle)
                .AddOutParameter("@message", DbType.String, 50);
            int result = proc.ExecuteNonQuery();
            Dictionary<string, object> returnValue = proc.GetReturnValues();
            foreach (KeyValuePair<string, object> kv in returnValue)
            {
                message = Convert.ToString(kv.Value);
            }
            if (result > 0 && message.Length == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 修改审批状态（抄送人）
        /// </summary>
        /// <param name="FlowPerformID"></param>
        /// <param name="flowNodeCode"></param>
        /// <param name="performState"></param>
        /// <param name="UserType"></param>
        /// <param name="UserCode"></param>
        /// <returns></returns>
        public AjaxResult UpdatePerformState(string FlowPerformID, string FlowCode, string FlowNodeCode, int performState, string UserType, string UserCode)
        {
            try
            {
                string sql = @"select id from TbFlowPerformOpinions where FlowPerformID=@FlowPerformID and FlowNodeCode=@FlowNodeCode and UserCode=@UserCode and UserType=@UserType ";
                DataTable dt = Db.Context.FromSql(sql).AddInParameter("@FlowPerformID", DbType.String, FlowPerformID)
                    .AddInParameter("@FlowNodeCode", DbType.String, FlowNodeCode)
                    .AddInParameter("@performState", DbType.Int32, performState)
                    .AddInParameter("@UserType", DbType.String, UserType)
                    .AddInParameter("@UserCode", DbType.String, UserCode)
                    .ToDataTable();
                if (dt.Rows.Count > 0)
                {
                    //获取下一节点是不是结束节点
                    string sqlNextNode = @"select a.FlowNodeCode
	                                       from 
	                                       (select b.FlowPerformID,ISNULL(b.FlowNodeCode,a.ChildNodeCode) as FlowNodeCode ,b.ActionType,
	                                       b.PersonnelSource,b.PersonnelCode  from (select ChildNodeCode from TbFlowPerformNodeRelation 
	                                       where FlowPerformID=@FlowPerformID and FlowCode=@FlowCode 
	                                       and ParentNodeCode=@FlowNodeCode) a 
	                                       left join TbFlowPerformNodePersonnel b on a.ChildNodeCode=b.FlowNodeCode and b.FlowPerformID=@FlowPerformID and b.FlowCode=@FlowCode) a
	                                       left join TbFlowPerformNode b on a.FlowPerformID=b.FlowPerformID and a.FlowNodeCode=b.FlowNodeCode";
                    DataTable dtNextNode = Db.Context.FromSql(sqlNextNode)
                        .AddInParameter("@FlowPerformID", DbType.String, FlowPerformID)
                        .AddInParameter("@FlowCode", DbType.String, FlowCode)
                        .AddInParameter("@FlowNodeCode", DbType.String, FlowNodeCode).ToDataTable();

                    //判断当前节点是否是最后一个节点
                    if (dtNextNode != null && dtNextNode.Rows.Count > 0 && Convert.ToString(dtNextNode.Rows[0]["FlowNodeCode"]) == "9999")
                    {
                        //获取当前节点是不是只有抄送人员
                        string sqlIsCsCount = @"select COUNT(1) IsCount from TbFlowPerformOpinions where FlowPerformID=@FlowPerformID and FlowNodeCode=@FlowNodeCode and UserType!=-1";
                        DataTable dtIsCsCount = Db.Context.FromSql(sqlIsCsCount)
                       .AddInParameter("@FlowPerformID", DbType.String, FlowPerformID)
                       .AddInParameter("@FlowNodeCode", DbType.String, FlowNodeCode).ToDataTable();
                        if (dtIsCsCount != null && dtIsCsCount.Rows.Count > 0 && Convert.ToInt32(dtIsCsCount.Rows[0]["IsCount"]) == 0)
                        {
                            //获取除当前登录人要处理的抄送信息之外的抄送信息是否处理完成
                            string sqlIsClCount = @"select COUNT(1) IsClCount from TbFlowPerformOpinions where FlowPerformID=@FlowPerformID and FlowNodeCode=@FlowNodeCode and PerformState!=0 and UserCode!=@UserCode";
                            DataTable dtIsClCount = Db.Context.FromSql(sqlIsClCount)
                                                    .AddInParameter("@FlowPerformID", DbType.String, FlowPerformID)
                                                    .AddInParameter("@FlowNodeCode", DbType.String, FlowNodeCode)
                                                    .AddInParameter("@UserCode", DbType.String, UserCode)
                                                    .ToDataTable();
                            if (dtIsClCount != null && dtIsClCount.Rows.Count > 0 && Convert.ToInt32(dtIsClCount.Rows[0]["IsClCount"]) == 0)
                            {
                                //获取要修改审批状态的业务数据
                                string sqlFlow = @"select distinct fp.id,fp.FormDataCode,fp.FormCode,fp.FlowState,sm.TableName from TbFlowPerform fp
                                                       left join TbFlowPerformOpinions fpo on fp.FlowPerformID=fpo.FlowPerformID 
                                                       left join TbSysMenuTable sm on fp.FormCode=sm.MenuCode
                                                       where fpo.FlowPerformID=@FlowPerformID and FlowNodeCode=@FlowNodeCode";
                                DataTable dtFlow = Db.Context.FromSql(sqlFlow)
                                                  .AddInParameter("@FlowPerformID", DbType.String, FlowPerformID)
                                                  .AddInParameter("@FlowNodeCode", DbType.String, FlowNodeCode).ToDataTable();
                                if (dtFlow != null && dtFlow.Rows.Count > 0)
                                {
                                    using (DbTrans trans = Db.Context.BeginTransaction())
                                    {
                                        Db.Context.FromSql("update TbFlowPerformOpinions set performState=0,PerformDate=GETDATE(),PerformOpinions='抄送人无需审批！' where id=@id").AddInParameter("@id", DbType.Int32, Convert.ToInt32(dt.Rows[0]["id"])).ExecuteNonQuery();
                                        //修改流程状态
                                        Db.Context.FromSql("update TbFlowPerform set FlowState=9 where FlowPerformID=@FlowPerformID")
                                                  .AddInParameter("@FlowPerformID", DbType.String, FlowPerformID).SetDbTransaction(trans).ExecuteNonQuery();
                                        string sqlUpdateYe = "update " + dtFlow.Rows[0]["TableName"] + " set Examinestatus='审核完成' where ID=" + dtFlow.Rows[0]["FormDataCode"] + "";
                                        //修改业务数据审批状态
                                        Db.Context.FromSql(sqlUpdateYe).SetDbTransaction(trans).ExecuteNonQuery();
                                        trans.Commit();//提交事务
                                        return AjaxResult.Success();
                                    }
                                }
                            }
                            else
                            {
                                Db.Context.FromSql("update TbFlowPerformOpinions set performState=0,PerformDate=GETDATE(),PerformOpinions='抄送人无需审批！' where id=@id").AddInParameter("@id", DbType.Int32, Convert.ToInt32(dt.Rows[0]["id"])).ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            Db.Context.FromSql("update TbFlowPerformOpinions set performState=0,PerformDate=GETDATE(),PerformOpinions='抄送人无需审批！' where id=@id").AddInParameter("@id", DbType.Int32, Convert.ToInt32(dt.Rows[0]["id"])).ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        //string sql1 = "update TbFlowPerformOpinions set performState=0,PerformDate=GETDATE(),PerformOpinions='抄送人无需审批！' where id=@id";
                        Db.Context.FromSql("update TbFlowPerformOpinions set performState=0,PerformDate=GETDATE(),PerformOpinions='抄送人无需审批！' where id=@id").AddInParameter("@id", DbType.Int32, Convert.ToInt32(dt.Rows[0]["id"])).ExecuteNonQuery();
                    }
                }
                return AjaxResult.Success();
            }
            catch (Exception ex)
            {
                return AjaxResult.Error("审阅过程中发生错误:" + ex.Message);
            }

        }
        /// <summary>
        /// 修改审批状态（执行人）
        /// </summary>
        /// <param name="FlowPerformID"></param>
        /// <param name="flowNodeCode"></param>
        /// <param name="performState"></param>
        /// <param name="UserType"></param>
        /// <param name="UserCode"></param>
        /// <returns></returns>
        public AjaxResult UpdatePerformStateZxr(string FlowPerformID, string FlowCode, string FlowNodeCode, int performState, string UserType, string UserCode)
        {
            try
            {
                string sql = @"select id from TbFlowPerformOpinions where FlowPerformID=@FlowPerformID and FlowNodeCode=@FlowNodeCode and UserCode=@UserCode and UserType=@UserType and PerformState=@performState";
                DataTable dt = Db.Context.FromSql(sql).AddInParameter("@FlowPerformID", DbType.String, FlowPerformID)
                    .AddInParameter("@FlowNodeCode", DbType.String, FlowNodeCode)
                    .AddInParameter("@performState", DbType.Int32, performState)
                    .AddInParameter("@UserType", DbType.String, UserType)
                    .AddInParameter("@UserCode", DbType.String, UserCode)
                    .ToDataTable();
                if (dt.Rows.Count > 0)
                {
                    using (DbTrans trans = Db.Context.BeginTransaction())
                    {
                        string sqlUpdateYe = "update TbFlowPerformOpinions set performState=7 where id=@id";
                        //修改业务数据审批状态
                        Db.Context.FromSql(sqlUpdateYe).AddInParameter("@id", DbType.Int32, Convert.ToInt32(dt.Rows[0]["id"])).SetDbTransaction(trans).ExecuteNonQuery();
                        trans.Commit();//提交事务
                        return AjaxResult.Success();
                    }
                }

                return AjaxResult.Success();
            }
            catch (Exception ex)
            {
                return AjaxResult.Error("审阅过程中发生错误:" + ex.Message);
            }

        }

        /// <summary>
        /// 获取原材料到货入库信息是否是审核完成，如果是审核完成调用短信接口，发送短信信息
        /// </summary>
        /// <param name="ID">数据id</param>
        /// <returns></returns>
        public string LoadInOrder(int ID)
        {
            string IsFormalSystem = ConfigurationManager.AppSettings["IsFormalSystem"];
            string msg = "";
            if (IsFormalSystem == "true")
            {
                try
                {
                    List<TbFlowPerformMessage> myMsgList = new List<TbFlowPerformMessage>();//'我的消息'推送
                    List<TbSMSAlert> myDxList = new List<TbSMSAlert>();//'短信'推送
                    string sql = @"select ino.InOrderCode,cp5.CompanyCode as ManagerDepartmentCode,cp5.CompanyFullName as ManagerDepartmentName,cp2.CompanyCode as BranchCode,cp2.CompanyFullName as BranchName,WorkAreaCode,cp3.CompanyFullName as WorkAreaName,SiteCode,cp4.CompanyFullName as SiteName,ino.UserCode,ino.Tel,ino.SupplierCode,ino.SiteUserTel from TbInOrder ino
            left join TbCompany cp3 on ino.WorkAreaCode = cp3.CompanyCode
            left join TbCompany cp4 on ino.SiteCode = cp4.CompanyCode
            left join TbCompany cp2 on cp3.ParentCompanyCode = cp2.CompanyCode
            left join TbCompany cp5 on cp2.ParentCompanyCode = cp5.CompanyCode where ino.ID=@ID and ino.Examinestatus='审核完成'";
                    StringBuilder sb = new StringBuilder();
                    DataTable dt = Db.Context.FromSql(sql).AddInParameter("@ID", DbType.Int32, ID).ToDataTable();
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        //调用短信发送接口
                        //查找短信模板信息
                        var shortMessageTemplateModel = Repository<TbShortMessageTemplate>.First(p => p.TemplateCode == "MB0004");
                        if (shortMessageTemplateModel == null)
                        {
                            msg = "原材料到货入库短信通知模板不存在";
                        }
                        else
                        {
                            //短信、消息内容
                            string content = shortMessageTemplateModel.TemplateContent;
                            var bl = dt.Rows[0]["BranchName"].ToString() + "/" + dt.Rows[0]["WorkAreaName"].ToString(); ;
                            var a = content.Replace("变量：分部/工区", bl);
                            //发送短信信息
                            TbSMSAlert tbsms = new TbSMSAlert();
                            tbsms.InsertTime = DateTime.Now;
                            tbsms.ManagerDepartment = dt.Rows[0]["ManagerDepartmentCode"].ToString();
                            tbsms.Branch = dt.Rows[0]["BranchCode"].ToString();
                            tbsms.WorkArea = dt.Rows[0]["WorkAreaCode"].ToString();
                            tbsms.Site = dt.Rows[0]["SiteCode"].ToString();
                            tbsms.UserCode = dt.Rows[0]["SupplierCode"].ToString();
                            tbsms.UserTel = dt.Rows[0]["SiteUserTel"].ToString();
                            tbsms.DXType = "";
                            tbsms.BusinessCode = shortMessageTemplateModel.TemplateCode;
                            tbsms.DataCode = dt.Rows[0]["InOrderCode"].ToString();
                            tbsms.ShortContent = a;
                            tbsms.FromCode = "InOrder";
                            tbsms.MsgType = "4";
                            myDxList.Add(tbsms);

                            //发送我的消息中的信息
                            TbFlowPerformMessage fpm = new TbFlowPerformMessage();
                            fpm.messageID = Guid.NewGuid().ToString();
                            fpm.messageCreateTime = DateTime.Now;
                            fpm.messageType = 13;
                            fpm.messageTitle = "【" + dt.Rows[0]["BranchName"].ToString() + "/" + dt.Rows[0]["WorkAreaName"].ToString() + "】原材料到货入库提醒";
                            fpm.messageContent = a;
                            fpm.IsRead = -1;
                            fpm.UserCode = dt.Rows[0]["SupplierCode"].ToString();
                            fpm.MsgType = "4";
                            myMsgList.Add(fpm);

                        }
                    }
                    //发送短信
                    CensusdemoTask ct = new CensusdemoTask();
                    if (myDxList.Count > 0)
                    {
                        //发送短信
                        for (int m = 0; m < myDxList.Count; m++)
                        {
                            var dx = ct.ShortMessagePC(myDxList[m].UserTel, myDxList[m].ShortContent);
                            var jObject1 = Newtonsoft.Json.Linq.JObject.Parse(dx);
                            var logmsg = jObject1["data"][0]["code"].ToString();
                            myDxList[m].DXType = logmsg;
                        }
                    }
                    using (DbTrans trans = Db.Context.BeginTransaction())
                    {
                        if (myMsgList.Any())
                        {
                            Repository<TbFlowPerformMessage>.Insert(trans, myMsgList);
                        }
                        if (myDxList.Any())
                        {
                            Repository<TbSMSAlert>.Insert(trans, myDxList);
                        }
                        trans.Commit();//提交事务
                    }
                }
                catch (Exception ex)
                {
                    return msg + ex.Message;
                }
            }
            return msg;
        }

        #endregion

        #region 我的消息
        public string GetMessageState(string usercode, DateTime? SDT, DateTime? EDT)
        {
            string sql = @"select Tb1.IsRead,case Tb1.IsRead when -1 then '未读【'+isnull(cast(Tb2.messagestate as nvarchar),0)+'】' when 0 then  '已读【'+isnull(cast(Tb2.messagestate as nvarchar),0)+'】' end as messagestate from (select -1 as IsRead union all select 0 as IsRead) Tb1 left join (select  m.IsRead,case m.IsRead when -1 then COUNT(1)
                            when 0 then COUNT(1) end as messagestate
                            from TbFlowPerformMessage m  
                            LEFT JOIN TbUser tu ON m.UserCode=tu.UserCode
                            where (m.usercode=@UserCode OR tu.UserId=@UserCode) 
                            and ((ISNULL(@SDT,'')='') or m.messageCreateTime>=@SDT) 
                            and ((ISNULL(@EDT,'')='') or m.messageCreateTime<=@EDT)  group by m.IsRead) Tb2 on Tb1.IsRead=Tb2.IsRead";
            DataTable dt = Db.Context.FromSql(sql).AddInParameter("@UserCode", DbType.String, usercode)
                .AddInParameter("@SDT", DbType.DateTime, SDT)
                .AddInParameter("@EDT", DbType.DateTime, EDT).ToDataTable();
            if (dt != null)
                return JsonConvert.SerializeObject(dt);
            else return "[]";

        }
        /// <summary>
        /// App我的消息状态
        /// </summary>
        /// <param name="usercode"></param>
        /// <param name="SDT"></param>
        /// <param name="EDT"></param>
        /// <returns></returns>
        public DataTable GetAppMessageState(string usercode, DateTime? SDT, DateTime? EDT)
        {
            string sql = @"select Tb1.IsRead,case Tb1.IsRead when -1 then '未读【'+isnull(cast(Tb2.messagestate as nvarchar),0)+'】' when 0 then  '已读【'+isnull(cast(Tb2.messagestate as nvarchar),0)+'】' end as messagestate from (select -1 as IsRead union all select 0 as IsRead) Tb1 left join (select  m.IsRead,case m.IsRead when -1 then COUNT(1)
                            when 0 then COUNT(1) end as messagestate
                            from TbFlowPerformMessage m  
                            LEFT JOIN TbUser tu ON m.UserCode=tu.UserCode
                            where (m.usercode=@UserCode OR tu.UserId=@UserCode) 
                            and ((ISNULL(@SDT,'')='') or m.messageCreateTime>=@SDT) 
                            and ((ISNULL(@EDT,'')='') or m.messageCreateTime<=@EDT)  group by m.IsRead) Tb2 on Tb1.IsRead=Tb2.IsRead";
            DataTable dt = Db.Context.FromSql(sql).AddInParameter("@UserCode", DbType.String, usercode)
                .AddInParameter("@SDT", DbType.DateTime, SDT)
                .AddInParameter("@EDT", DbType.DateTime, EDT).ToDataTable();
            return dt;

        }
        public PageModel GetMyMessage(PageSearchRequest pt, string usercode, int status, DateTime? SDT, DateTime? EDT)
        {
            string sql = @"select m.ID,m.messageCreateTime,case m.messageType when 3 then '审批消息退回' when 11 then '材料库存报警' when 12 then '供货提醒' when 13 then '原材料到货入库提醒' when 14 then '取样订单加工完成通知提醒' when 15 then '原材料到货入库不合格材料退回通知提醒' when 16 then '取样订单检验文件上传超时提醒' when 17 then '配送计划时间修改通知提醒' when 18 then '配送装车完成通知提醒' when 19 then '车辆出厂通知提醒' when 20 then '半成品签收通知提醒' end as messageType,m.messageTitle,
                            m.messageContent ,case m.isread when -1 then '未读' else '已读' end as isread,f.FormCode,f.FormDataCode,m.readtime
                            from TbFlowPerformMessage m
                            left join TbFlowPerform f on m.FlowPerformID=f.FlowPerformID
                            LEFT JOIN TbUser tu ON m.UserCode=tu.UserCode
                            where (m.usercode=@UserCode OR tu.UserId=@UserCode) and m.IsRead=@IsRead
                            and ((ISNULL(@SDT,'')='') or m.messageCreateTime>=@SDT) 
                            and ((ISNULL(@EDT,'')='') or m.messageCreateTime<=@EDT)";
            List<Parameter> parameter = new List<Parameter>();
            parameter.Add(new Parameter("@UserCode", usercode, DbType.String, null));
            parameter.Add(new Parameter("@IsRead", status, DbType.Int32, null));
            parameter.Add(new Parameter("@SDT", SDT, DbType.String, null));
            parameter.Add(new Parameter("@EDT", EDT, DbType.String, null));
            try
            {
                var ret = Repository<TbFlowPerformMessage>.FromSqlToPageTable(sql, parameter, pt.rows, pt.page, "messageCreateTime");
                return ret;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public AjaxResult UpdateStatus(int ID)
        {
            string sql = "update TbFlowPerformMessage set IsRead=0 where id=@ID";
            int reslut = Db.Context.FromSql(sql).AddInParameter("ID", DbType.Int32, ID).ExecuteNonQuery();
            if (reslut > 0)
                return AjaxResult.Success("修改成功");
            else return AjaxResult.Error("修改失败");

        }
        #endregion

        #region 流程条件设置
        /// <summary>
        /// 获取对应表单字段信息
        /// </summary>
        /// <param name="FormCode">表单编码</param>
        /// <returns>字段列表</returns>
        public string GetFields(string FormCode)
        {
            string tableName = string.Empty;
            string sql = "select TableName from TbSysMenuTable where MenuCode=@MenuCode and IsMainTabel=0";
            DataTable dt = Db.Context.FromSql(sql).AddInParameter("@MenuCode", DbType.String, FormCode).ToDataTable();
            if (dt != null && dt.Rows.Count > 0)
            {
                tableName = Convert.ToString(dt.Rows[0]["TableName"]);
            }

            //实体对象
            var filemodel = Assembly.Load("PM.DomainEntity").CreateInstance("PM.DomainEntity." + tableName);
            //获取实体列中返回所有字段的方法
            MethodInfo mi = filemodel.GetType().GetMethod("GetFields");
            //获取实体类中该方法所需参数
            ParameterInfo[] paramsInfo = mi.GetParameters();
            //实例化实体类中方法所需参数
            Object[] paras = new Object[] { };
            object obj = Activator.CreateInstance(filemodel.GetType());
            //获取实体类中所有字段信息
            Field[] fields = (Field[])mi.Invoke(obj, paras);

            List<FlowFiledInfo> list = new List<FlowFiledInfo>();
            foreach (Field F in fields)
            {
                FlowFiledInfo ffi = new FlowFiledInfo();
                string fileName = string.Format(F.FieldName, "", "");
                ffi.tableName = tableName;
                ffi.filedName = fileName;
                ffi.filedDescription = F.Description;
                ffi.fieldType = (filemodel.GetType().GetProperty(fileName).PropertyType).Name;
                list.Add(ffi);
            }
            return JsonConvert.SerializeObject(list);
        }
        /// <summary>
        /// 添加审批流程节点条件
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public AjaxResult AddFlowNodeJudgeCriteria(TbFlowNodeJudgeCriteria t)
        {
            try
            {
                if (string.IsNullOrEmpty(t.JudgeRelation)) t.JudgeRelation = "";
                Db.Context.Insert<TbFlowNodeJudgeCriteria>(t);
                return AjaxResult.Success("条件添加成功");
            }
            catch (Exception)
            {
                return AjaxResult.Error("条件添加失败");
            }
        }
        /// <summary>
        /// 修改审批流程条件
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public AjaxResult UpdateFlowNodeJudgeCriteria(TbFlowNodeJudgeCriteria t)
        {
            try
            {
                //Repository<TbProjectTargetCostTemplate>.Update(model, p => p.ID == model.ID);
                int result = Repository<TbFlowNodeJudgeCriteria>.Update(t, p => p.id == t.id);
                if (result > 0)
                    return AjaxResult.Success("条件修改成功");
                else return AjaxResult.Error("条件修改失败");
            }
            catch (Exception)
            {
                return AjaxResult.Error("条件修改失败");
            }
        }
        public class FlowFiledInfo
        {
            public string tableName { get; set; }
            public string filedName { get; set; }
            public string filedDescription { get; set; }
            public string fieldType { get; set; }
        }
        #endregion

        #region 流程事件设置
        /// <summary>
        /// 获取流程事件对应的操作
        /// </summary>
        /// <param name="FlowCode"></param>
        /// <param name="FlowNodeCode"></param>
        /// <returns></returns>
        public string GetFlowEvent(string FlowCode, string FlowNodeCode)
        {
            string sql = string.Empty;
            DataTable dt = null;
            if (string.IsNullOrEmpty(FlowNodeCode))
            {
                sql = "select  ProcName,Remark from TbFlowEventProc where FlowCode=@FlowCode and (FlowNodeCode is null or FlowNodeCode = '')";
                dt = Db.Context.FromSql(sql).AddInParameter("@FlowCode", DbType.String, FlowCode).ToDataTable();
            }
            else
            {
                sql = "select  ProcName,Remark from TbFlowEventProc where FlowCode=@FlowCode and FlowNodeCode=@FlowNodeCode";
                dt = Db.Context.FromSql(sql).AddInParameter("@FlowCode", DbType.String, FlowCode)
                    .AddInParameter("@FlowNodeCode", DbType.String, FlowNodeCode).ToDataTable();
            }
            if (dt != null)
            {
                return JsonConvert.SerializeObject(dt);
            }
            else
            {
                return "[]";
            }
        }
        #endregion

        #region App审批消息
        public DataTable GetFlowList(string UserCode, int PerformState)
        {
            string sql = @" select o.PreNodeCompleteDate,o.FlowNodeCode,o.FlowPerformID,f.FlowCode,f.FormCode,f.MenuName,f.UserName,f.FormDataCode,
                            case o.PerformState when -1 then '未阅' 
                            when 0 then '已阅' when 1 then '同意'
                            when 2 then '不同意' when 3 then '退回'
                            when 4 then '终止' when 5 then '超时跳过'
                            when 9 then '非会签其他用户已处理' end as PerformState,
                            case f.FlowLevel when 0 then '普通' when 1 then '加急' when 2 then '紧急' End as FlowLevel,
                            f.FlowTitle,f.UserName,f.BeginTime,f.EndTime,n.FlowNodeName 
                            from TbFlowPerformOpinions o
                            left join (
                            select s.MenuName,a.FormDataCode,a.FormCode,a.FlowCode,a.FlowPerformID,a.FlowLevel,a.FlowTitle,a.BeginTime,
                            a.EndTime,b.UserCode,b.UserName from TbFlowPerform a 
                            left join TbUser b on a.UserCode=b.UserCode
                            left join TbSysMenu s on a.FormCode=s.MenuCode
                            ) f on o.FlowPerformID=f.FlowPerformID 
                            left join TbFlowPerformNode n on o.FlowPerformID=n.FlowPerformID and o.FlowNodeCode=n.FlowNodeCode
                            where o.UserCode=(select UserId from TbUser where UserCode=@UserCode) 
                            and o.PerformState=@PerformState";
            DataTable dt = Db.Context.FromSql(sql).AddInParameter("@UserCode", DbType.String, UserCode)
                .AddInParameter("@PerformState", DbType.Int32, PerformState)
                .ToDataTable();

            return dt;
        }

        /// <summary>
        /// 获取原单据地址
        /// </summary>
        /// <param name="formCode">单据编码</param>
        /// <returns>原单据地址</returns>
        public object LoadFormUrl(string HostAddress, string formCode, int keyValue)
        {
            string sql = "select MenuUrl from TbSysMenu where MenuCode=@MenuCode";
            StringBuilder sb = new StringBuilder();
            DataTable dt = Db.Context.FromSql(sql).AddInParameter("@MenuCode", DbType.String, formCode).ToDataTable();
            if (dt != null)
            {
                string url = Convert.ToString(dt.Rows[0]["MenuUrl"]);
                string[] arr = url.Split(new char[1] { '/' });
                for (int i = 0; i < arr.Length - 1; i++)
                {
                    sb.Append(arr[i]);
                    sb.Append("/");
                }
                sb.Append("Details");
            }
            return "{\"url\":\"http://" + HostAddress + sb.ToString() + "?keyValue=" + keyValue + "\"}";
        }
        /// <summary>
        /// 获取表单审批意见
        /// </summary>
        /// <param name="flowperformid">流程表单id</param>
        /// <returns></returns>
        public DataTable LoadFormSpOptions(string flowperformid)
        {
            string sql = @"select '' as PerformState,'发起' as FlowNodeName,'' as UserName,BeginTime,null as PerformDate,'' as PerformOpinions,'' as UserType from TbFlowPerform where FlowPerformID=@FlowPerformID
            union all
            select case o.PerformState when -1 then '未阅' 
                        when 0 then '已阅' when 1 then '同意'
                        when 2 then '不同意' when 3 then '退回'
                        when 4 then '终止' when 5 then '超时跳过'
                        when 9 then '非会签其他用户已处理' end as PerformState,
            n.FlowNodeName,u.UserName,o.PerformDate,o.PerformDate,o.PerformOpinions,
            case o.UserType when 1 then '抄送人' when 0 then '执行人' end as UserType
             from (select top 9999 * from TbFlowPerformOpinions where FlowPerformID=@FlowPerformID order by PreNodeCompleteDate asc) o
            left join (select FlowPerformID,FlowCode,FlowNodeCode,FlowNodeName from TbFlowPerformNode where FlowPerformID=@FlowPerformID) n 
            on o.FlowPerformID=n.FlowPerformID and o.FlowNodeCode=n.FlowNodeCode
            left join TbUser u 
            on o.UserCode=u.UserId
            union all
            select '' as PerformState,'结束' as FlowNodeName,'' as UserName,EndTime,EndTime,'' as PerformOpinions,'' as UserType from TbFlowPerform where FlowPerformID=@FlowPerformID";
            DataTable dt = Db.Context.FromSql(sql).AddInParameter("@FlowPerformID", DbType.String, flowperformid).ToDataTable();
            return dt;
        }
        #endregion

        #region  发起流程新版TQ

        public DataTable GetDataExaminestatus(string FormCode, int DataId)
        {
            string sql = @"select a.FlowCode,a.FormCode,a.FormDataCode,a.FlowPerformID from TbFlowPerform a
left join TbFlowPerformOpinions b on a.FlowPerformID=b.FlowPerformID
where 1=1 and a.FormCode=@FormCode and a.FormDataCode=@FormDataCode group by a.FlowCode,a.FormCode,a.FormDataCode,a.FlowPerformID";
            DataTable dt = Db.Context.FromSql(sql)
                    .AddInParameter("@FormCode", DbType.String, FormCode)
                    .AddInParameter("@FormDataCode", DbType.Int32, DataId)
                    .ToDataTable();
            return dt;
        }

        public DataTable GetFlowDefine(string FormCode, string ProjectId, string OtherParma)
        {
            string sql = @"select a.*,b.MenuName as FormName from TbFlowDefine a 
left join TbSysMenu b on a.FormCode=b.MenuCode
where 1=1 and a.FlowType='New' and a.FormCode=@FormCode and a.ProjectId=@ProjectId and (ISNULL(@OtherParma,'')='' or a.FlowName like '%'+@OtherParma+'%')";
            DataTable dt = Db.Context.FromSql(sql)
                   .AddInParameter("@FormCode", DbType.String, FormCode)
                   .AddInParameter("@ProjectId", DbType.String, ProjectId)
                   .AddInParameter("@OtherParma", DbType.String, OtherParma)
                   .ToDataTable();
            return dt;
        }

        /// <summary>
        /// 发出流程
        /// </summary>
        /// <param name="FlowCode">流程编码</param>
        /// <param name="FormCode">表单编码</param>
        /// <param name="FormDataCode">数据ID</param>
        /// <param name="UserCode">用户编码</param>
        /// <param name="FlowTitle">流程主题</param>
        /// <param name="FlowLevel">流程级别</param>
        /// <param name="FinalCutoffTime">流程截止日期</param>
        public AjaxResult InitFlowPerformNew(string FlowCode, string FormCode, string FormDataCode, string UserCode, string FlowTitle, int FlowLevel)
        {
            try
            {
                #region 初始化流程信息

                string FlowPerformID = GetNextFlowPerformID("FlowPerformID");
                //查询出当前发起流程数据的业务表
                var menuTable = Repository<TbSysMenuTable>.First(p => p.MenuCode == FormCode && p.IsMainTabel == "0");
                #region 向TbFlowPerform表中插入数据
                TbFlowPerform fpModel = new TbFlowPerform();
                fpModel.FlowPerformID = FlowPerformID;
                fpModel.UserCode = UserCode;
                fpModel.FlowCode = FlowCode;
                fpModel.FormCode = FormCode;
                fpModel.FormDataCode = FormDataCode;
                fpModel.BeginTime = DateTime.Now;
                fpModel.FlowState = -1;
                fpModel.FlowLevel = FlowLevel;
                fpModel.FlowTitle = FlowTitle;
                #endregion
                #region 向TbFlowPerformNodePersonnel表中插入数据
                List<TbFlowPerformNodePersonnel> fpnpList = new List<TbFlowPerformNodePersonnel>();
                //查找TbFlowNodePersonnel表中的数据
                var flowNodePersonnelList = Db.Context.From<TbFlowNodePersonnel>().Select(TbFlowNodePersonnel._.All).Where(p => p.FlowCode == FlowCode).ToList();
                if (flowNodePersonnelList.Count > 0)
                {
                    //查询出业务数据对应的工区、分部、经理部、加工厂信息
                    if (menuTable != null)
                    {
                        string sqlTableData = "";
                        if (FormCode == "RawMonthDemandPlan" || FormCode == "RawMonthDemandSupplyPlan" || FormCode == "FactoryBatchNeedPlan" || FormCode == "RMProductionMaterial")
                        {
                            sqlTableData = @"select a.BranchCode,a.WorkAreaCode,a.ProcessFactoryCode,b.ParentCompanyCode as JlbCode from " + menuTable.TableName + @" a
left join TbCompany b on a.BranchCode=b.CompanyCode where a.ID=@ID";
                        }
                        else if (FormCode == "InOrder")
                        {
                            sqlTableData = @"select a.WorkAreaCode,b.ParentCompanyCode as BranchCode,c.ParentCompanyCode as JlbCode,d.ProcessFactoryCode from " + menuTable.TableName + @" a
left join TbCompany b on a.WorkAreaCode=b.CompanyCode
left join TbCompany c on b.ParentCompanyCode=c.CompanyCode
left join TbStorage d on a.StorageCode=d.StorageCode  where a.ID=@ID";
                        }
                        else if (FormCode == "SampleOrder")
                        {
                            sqlTableData = @"select a.WorkAreaCode,b.ParentCompanyCode as BranchCode,c.ParentCompanyCode as JlbCode,a.ProcessFactoryCode from " + menuTable.TableName + @" a
left join TbCompany b on a.WorkAreaCode=b.CompanyCode
left join TbCompany c on b.ParentCompanyCode=c.CompanyCode  where a.ID=@ID";
                        }
                        else if (FormCode == "WorkOrder" || FormCode == "ProblemOrder")
                        {
                            sqlTableData = @"select b.ParentCompanyCode as WorkAreaCode,c.ParentCompanyCode as BranchCode,d.ParentCompanyCode as JlbCode,a.ProcessFactoryCode from " + menuTable.TableName + @" a
left join TbCompany b on a.SiteCode=b.CompanyCode
left join TbCompany c on b.ParentCompanyCode=c.CompanyCode
left join TbCompany d on c.ParentCompanyCode=d.CompanyCode
where a.ID=@ID";
                        }
                        DataTable dtTableData = Db.Context.FromSql(sqlTableData).AddInParameter("@ID", DbType.Int32, FormDataCode).ToDataTable();
                        if (dtTableData.Rows.Count > 0)
                        {
                            string sqlUser = "";
                            for (int i = 0; i < flowNodePersonnelList.Count; i++)
                            {
                                if (flowNodePersonnelList[i].PersonnelSource == "Role")//选择角色
                                {
                                    string OrgId = "";
                                    switch (flowNodePersonnelList[i].OrgType)
                                    {
                                        case "1":
                                            OrgId = dtTableData.Rows[0]["ProcessFactoryCode"].ToString();
                                            break;
                                        case "2":
                                            OrgId = dtTableData.Rows[0]["JlbCode"].ToString();
                                            break;
                                        case "3":
                                            OrgId = dtTableData.Rows[0]["BranchCode"].ToString();
                                            break;
                                        case "4":
                                            OrgId = dtTableData.Rows[0]["WorkAreaCode"].ToString();
                                            break;
                                        default:
                                            break;
                                    }
                                    sqlUser = "select a.UserCode from TbUserRole a where 1=1 and a.Flag=0 and a.DeptId='" + flowNodePersonnelList[i].DeptId + "' and a.RoleCode='" + flowNodePersonnelList[i].RoleId + "' and a.ProjectId='" + flowNodePersonnelList[i].ProjectId + "' and a.OrgType=" + flowNodePersonnelList[i].OrgType + " and a.OrgId='" + OrgId + "' ";
                                    DataTable dtUser = Db.Context.FromSql(sqlUser).AddInParameter("@ID", DbType.Int32, FormDataCode).ToDataTable();
                                    if (dtUser.Rows.Count > 0)
                                    {
                                        for (int u = 0; u < dtUser.Rows.Count; u++)
                                        {
                                            TbFlowPerformNodePersonnel fpnpModel = new TbFlowPerformNodePersonnel();
                                            fpnpModel.FlowPerformID = FlowPerformID;
                                            fpnpModel.FlowCode = flowNodePersonnelList[i].FlowCode;
                                            fpnpModel.FlowNodeCode = flowNodePersonnelList[i].FlowNodeCode;
                                            fpnpModel.ActionType = flowNodePersonnelList[i].ActionType;
                                            fpnpModel.PersonnelSource = "Personnel";
                                            fpnpModel.PersonnelCode = dtUser.Rows[u]["UserCode"].ToString();
                                            fpnpList.Add(fpnpModel);
                                        }
                                    }
                                }
                                else//直接选择用户
                                {
                                    TbFlowPerformNodePersonnel fpnpModel = new TbFlowPerformNodePersonnel();
                                    fpnpModel.FlowPerformID = FlowPerformID;
                                    fpnpModel.FlowCode = flowNodePersonnelList[i].FlowCode;
                                    fpnpModel.FlowNodeCode = flowNodePersonnelList[i].FlowNodeCode;
                                    fpnpModel.ActionType = flowNodePersonnelList[i].ActionType;
                                    fpnpModel.PersonnelSource = flowNodePersonnelList[i].PersonnelSource;
                                    fpnpModel.PersonnelCode = flowNodePersonnelList[i].PersonnelCode;
                                    fpnpList.Add(fpnpModel);
                                }
                            }
                        }
                    }
                }
                #endregion
                #region 向TbFlowPerformNode表中插入数据
                List<TbFlowPerformNode> fpnList = new List<TbFlowPerformNode>();
                //查找TbFlowNode表中的数据
                var flowNodeList = Db.Context.From<TbFlowNode>().Select(TbFlowNode._.All).Where(p => p.FlowCode == FlowCode).ToList();
                if (flowNodeList.Count > 0)
                {
                    for (int i = 0; i < flowNodeList.Count; i++)
                    {
                        TbFlowPerformNode fpnModel = new TbFlowPerformNode();
                        fpnModel.FlowPerformID = FlowPerformID;
                        fpnModel.FlowCode = flowNodeList[i].FlowCode;
                        fpnModel.FlowNodeCode = flowNodeList[i].FlowNodeCode;
                        fpnModel.FlowNodeName = flowNodeList[i].FlowNodeName;
                        fpnModel.AllApproval = flowNodeList[i].AllApproval;
                        fpnModel.FreeCandidates = flowNodeList[i].FreeCandidates;
                        fpnModel.AddSignature = flowNodeList[i].AddSignature;
                        fpnModel.AllowedSkip = flowNodeList[i].AllowedSkip;
                        fpnModel.LimitDay = flowNodeList[i].LimitDay;
                        fpnModel.LimitHour = flowNodeList[i].LimitHour;
                        fpnModel.LimitMinutes = flowNodeList[i].LimitMinutes;
                        fpnModel.AllowedRejected = flowNodeList[i].AllowedRejected;
                        fpnModel.FreeRejected = flowNodeList[i].FreeRejected;
                        fpnModel.RejectedToNodeCode = flowNodeList[i].RejectedToNodeCode;
                        fpnModel.FlowNodeState = 0;
                        fpnModel.BlankNode = flowNodeList[i].BlankNode;
                        fpnModel.FlowNodeEvent = flowNodeList[i].FlowNodeEvent;
                        fpnList.Add(fpnModel);
                    }
                }
                #endregion
                #region 向TbFlowPerformNodeRelation表中插入数据
                List<TbFlowPerformNodeRelation> fpnrList = new List<TbFlowPerformNodeRelation>();
                //查找TbFlowNodeRelation表中的数据
                var flowNodeRelationList = Db.Context.From<TbFlowNodeRelation>().Select(TbFlowNodeRelation._.All).Where(p => p.FlowCode == FlowCode).ToList();
                if (flowNodeRelationList.Count > 0)
                {
                    for (int i = 0; i < flowNodeRelationList.Count; i++)
                    {
                        TbFlowPerformNodeRelation fpnrModel = new TbFlowPerformNodeRelation();
                        fpnrModel.FlowPerformID = FlowPerformID;
                        fpnrModel.FlowCode = flowNodeRelationList[i].FlowCode;
                        fpnrModel.ParentNodeCode = flowNodeRelationList[i].ParentNodeCode;
                        fpnrModel.ChildNodeCode = flowNodeRelationList[i].ChildNodeCode;
                        fpnrList.Add(fpnrModel);
                    }
                }
                #endregion
                #region 向TbFlowPerformNodeUI表中插入数据
                List<TbFlowPerformNodeUI> fpnuList = new List<TbFlowPerformNodeUI>();
                //查找TbFlowNodeRelation表中的数据
                var flowNodeUIList = Db.Context.From<TbFlowNodeUI>().Select(TbFlowNodeUI._.All).Where(p => p.FlowCode == FlowCode).ToList();
                if (flowNodeUIList.Count > 0)
                {
                    for (int i = 0; i < flowNodeUIList.Count; i++)
                    {
                        TbFlowPerformNodeUI fpnuModel = new TbFlowPerformNodeUI();
                        fpnuModel.FlowPerformID = FlowPerformID;
                        fpnuModel.FlowCode = flowNodeUIList[i].FlowCode;
                        fpnuModel.FlowNodeCode = flowNodeUIList[i].FlowNodeCode;
                        fpnuModel.processData = flowNodeUIList[i].processData;
                        fpnuModel.icon = flowNodeUIList[i].icon;
                        fpnuModel.NodeLeft = flowNodeUIList[i].NodeLeft;
                        fpnuModel.NodeTop = flowNodeUIList[i].NodeTop;
                        fpnuList.Add(fpnuModel);
                    }
                }
                #endregion
                //修改业务表数据的审批状态
                string sqlYwTable = "update " + menuTable.TableName + " set Examinestatus='审批中' where ID=" + FormDataCode + "";
                //更新TbFlowCode表
                string CodeType = "FlowPerformID";
                var flowCodeModel = Repository<TbFlowCode>.First(p => p.CodeType == CodeType);
                flowCodeModel.CodeValue = flowCodeModel.CodeValue + 1;

                #endregion

                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    Repository<TbFlowPerform>.Insert(trans, fpModel);
                    Repository<TbFlowPerformNodePersonnel>.Insert(trans, fpnpList);
                    Repository<TbFlowPerformNode>.Insert(trans, fpnList);
                    Repository<TbFlowPerformNodeRelation>.Insert(trans, fpnrList);
                    Repository<TbFlowPerformNodeUI>.Insert(trans, fpnuList);
                    Db.Context.FromSql(sqlYwTable).SetDbTransaction(trans).ExecuteNonQuery();
                    Repository<TbFlowCode>.Update(trans, flowCodeModel);
                    trans.Commit();

                    #region 发起流程

                    bool flag = Flow_LaunchFlow(FlowCode, FlowPerformID, "0", DateTime.Now, UserCode, FlowTitle, menuTable.TableName, FormDataCode, 1, FormCode);
                    if (flag == false)
                    {
                        //删除流程初始化对应的数据
                        Repository<TbFlowPerform>.Delete(trans, p => p.FlowPerformID == FlowPerformID);
                        Repository<TbFlowPerformNodePersonnel>.Delete(trans, p => p.FlowPerformID == FlowPerformID);
                        Repository<TbFlowPerformNode>.Delete(trans, p => p.FlowPerformID == FlowPerformID);
                        Repository<TbFlowPerformNodeRelation>.Delete(trans, p => p.FlowPerformID == FlowPerformID);
                        Repository<TbFlowPerformNodeUI>.Delete(trans, p => p.FlowPerformID == FlowPerformID);
                        Db.Context.FromSql("update " + menuTable.TableName + " set Examinestatus='未发起' where ID=" + FormDataCode + "").SetDbTransaction(trans).ExecuteNonQuery();
                        var flowCodeModelNew = Repository<TbFlowCode>.First(p => p.CodeType == CodeType);
                        flowCodeModelNew.CodeValue = flowCodeModelNew.CodeValue - 1;
                        Repository<TbFlowCode>.Update(trans, flowCodeModelNew);
                        trans.Commit();
                    }

                    #endregion

                    return AjaxResult.Success();
                }
            }
            catch (Exception ex)
            {
                return AjaxResult.Error();
            }
        }

        /// <summary>
        /// 审批并发起流程
        /// </summary>
        /// <param name="FlowCode">流程编码</param>
        /// <param name="performState">流程状态</param>
        /// <param name="performOpinions">审批意见</param>
        /// <param name="FlowPerformID">流程流水号</param>
        /// <param name="FlowNodeCode">流程节点编码</param>
        /// <param name="UserCode">用户编码</param>
        /// <param name="FlowTitle">流程主题</param>
        /// <returns></returns>
        public AjaxResult ApprovalNew(string FlowCode, int PerformState, string PerformOpinions, string FlowPerformID, string FlowNodeCode, string UserCode, string FlowTitle)
        {
            try
            {
                //修改TbFlowPerformOpinions表中的状态跟审批意见、审批时间
                var fpoModel = Repository<TbFlowPerformOpinions>.First(p => p.FlowPerformID == FlowPerformID && p.FlowNodeCode == FlowNodeCode && p.UserCode == UserCode && (p.PerformState == -1 || p.PerformState == 7));
                if (fpoModel != null)
                {
                    fpoModel.PerformState = PerformState;
                    fpoModel.PerformDate = DateTime.Now;
                    fpoModel.PerformOpinions = PerformOpinions;
                }
                //判断是会签还是非会签
                List<TbFlowPerformOpinions> fpoList = new List<TbFlowPerformOpinions>();
                var fpnModel = Repository<TbFlowPerformNode>.First(p => p.FlowPerformID == FlowPerformID && p.FlowNodeCode == FlowNodeCode);
                if (fpnModel != null)
                {
                    if (fpnModel.AllApproval == 0)//非会签
                    {
                        //非会签用户流程状态为已完成
                        var fpofhqList = Repository<TbFlowPerformOpinions>.Query(p => p.FlowPerformID == FlowPerformID && p.FlowNodeCode == FlowNodeCode && p.UserCode != UserCode && p.UserType == 0).ToList();
                        if (fpofhqList.Count > 0)
                        {
                            for (int i = 0; i < fpofhqList.Count; i++)
                            {
                                TbFlowPerformOpinions fhqModel = fpofhqList[i];
                                fhqModel.PerformState = 9;
                                fpoList.Add(fhqModel);
                            }
                        }
                        //--更新节点状态
                        fpnModel.FlowNodeState = PerformState;
                        var fpModel = Repository<TbFlowPerform>.First(p => p.FlowPerformID == FlowPerformID);
                        if (fpModel != null)
                        {
                            var menuTable = Repository<TbSysMenuTable>.First(p => p.MenuCode == fpModel.FormCode && p.IsMainTabel == "0");
                            if (menuTable != null)
                            {
                                bool flag = Flow_LaunchFlow(FlowCode, FlowPerformID, FlowNodeCode, DateTime.Now, UserCode, FlowTitle, menuTable.TableName, fpModel.FormDataCode, PerformState, fpModel.FormCode);
                            }
                        }

                    }
                    else//会签
                    {
                        //获取未审批的节点
                        var fpohqList = Repository<TbFlowPerformOpinions>.Query(p => p.FlowPerformID == FlowPerformID && p.FlowNodeCode == FlowNodeCode && p.UserCode != UserCode && p.PerformState != 1 && p.UserType == 0).ToList();
                        if (fpohqList.Count == 0)
                        {
                            //--更新节点状态
                            fpnModel.FlowNodeState = PerformState;
                            var fpModel = Repository<TbFlowPerform>.First(p => p.FlowPerformID == FlowPerformID);
                            if (fpModel != null)
                            {
                                var menuTable = Repository<TbSysMenuTable>.First(p => p.MenuCode == fpModel.FormCode && p.IsMainTabel == "0");
                                if (menuTable != null)
                                {
                                    bool flag = Flow_LaunchFlow(FlowCode, FlowPerformID, FlowNodeCode, DateTime.Now, UserCode, FlowTitle, menuTable.TableName, fpModel.FormDataCode, PerformState, fpModel.FormCode);
                                }
                            }
                        }
                    }
                }

                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    Repository<TbFlowPerformOpinions>.Update(trans, fpoModel);
                    if (fpoList.Count > 0)
                    {
                        Repository<TbFlowPerformOpinions>.Update(trans, fpoList);
                    }
                    Repository<TbFlowPerformNode>.Update(trans, fpnModel);
                    trans.Commit();
                }
                //修改TbFlowPerformNode表中节点状态
                return AjaxResult.Success();
            }
            catch (Exception ex)
            {
                return AjaxResult.Error();
            }
        }

        /// <summary>
        /// 发起流程
        /// </summary>
        /// <param name="FlowCode">流程Code</param>
        /// <param name="FlowPerformID">流程ID</param>
        /// <param name="FlowNodeCode">流程节点Code</param>
        /// <param name="processTime">上一步处理时间</param>
        /// <param name="UserCode">用户编码</param>
        /// <param name="FlowTitle">流程主题</param>
        /// <param name="TableName">业务表名称</param>
        /// <param name="FormDataCode">业务表数据ID</param>
        /// <param name="PerformState">操作类型：同意、退回</param>
        /// <returns></returns>
        public bool Flow_LaunchFlow(string FlowCode, string FlowPerformID, string FlowNodeCode, DateTime processTime, string UserCode, string FlowTitle, string TableName, string FormDataCode, int PerformState, string FormCode)
        {
            bool flag = false;
            try
            {
                var model = Repository<TbFlowPerform>.First(p => p.FlowPerformID == FlowPerformID);
                string sqlYwTable = "";//业务表sql
                string sqlFe = "";//流程节点事件sql
                string sqlEarlyInfo = "";//流程预警信息sql
                //向插入TbFlowPerformOpinions
                List<TbFlowPerformOpinions> fpoList = new List<TbFlowPerformOpinions>();
                if (model != null)
                {
                    //判断是同意还是退回操作
                    if (PerformState == 3)//退回
                    {
                        //修改TbFlowPerform表中流程状态
                        model.FlowState = PerformState;
                        //更改业务单据状态为审核完成
                        sqlYwTable = "update " + TableName + " set Examinestatus='已退回' where ID=" + FormDataCode + "";
                    }
                    else
                    {
                        //查找当前节点的下一节点
                        var fpnrModel = Repository<TbFlowPerformNodeRelation>.First(p => p.FlowPerformID == FlowPerformID && p.FlowCode == FlowCode && p.ParentNodeCode == FlowNodeCode);
                        if (fpnrModel != null)
                        {
                            //查找该流程是否存在流程事件
                            var fpnModel = Repository<TbFlowPerformNode>.First(p => p.FlowPerformID == FlowPerformID && p.FlowNodeCode == FlowNodeCode);
                            if (!string.IsNullOrWhiteSpace(fpnModel.FlowNodeEvent))
                            {
                                //存在流程事件
                                if (!string.IsNullOrWhiteSpace(FormCode))
                                {
                                    //查找菜单流程事件
                                    var feModel = Repository<TbFlowEvent>.First(p => p.FormCode == FormCode);
                                    sqlFe = feModel.EventDescriptionsql;
                                }
                            }
                            FlowNodeCode = fpnrModel.ChildNodeCode;
                            //下一节点为最后一个节点，改变该流程的状态
                            if (FlowNodeCode == "9999")
                            {
                                //修改TbFlowPerform表中的流程状态
                                model.FlowState = 9;
                                model.EndTime = DateTime.Now;
                                //更改业务单据状态为审核完成
                                sqlYwTable = "update " + TableName + " set Examinestatus='审核完成' where ID=" + FormDataCode + "";
                                if (FormCode == "WorkOrder")
                                {
                                    //订单中的某种材料汇总重量≥当前库存重量的80%  系统发送预警信息给加工厂材料负责人，并以短信通知
                                    _endingStocks.SendMsgForMaterialStockByOrderNew(Convert.ToInt32(FormDataCode));
                                }
                            }
                            else
                            {
                                //查找TbFlowNodePersonnel表中的数据
                                var flowNodePersonnelList = Db.Context.From<TbFlowPerformNodePersonnel>().Select(TbFlowPerformNodePersonnel._.All).Where(p => p.FlowCode == FlowCode && p.FlowNodeCode == FlowNodeCode && p.FlowPerformID == FlowPerformID).ToList();
                                if (flowNodePersonnelList.Count > 0)
                                {
                                    for (int i = 0; i < flowNodePersonnelList.Count; i++)
                                    {
                                        TbFlowPerformOpinions fpoModel = new TbFlowPerformOpinions();
                                        fpoModel.FlowPerformID = FlowPerformID;
                                        fpoModel.FlowNodeCode = FlowNodeCode;
                                        fpoModel.PreNodeCompleteDate = processTime;
                                        fpoModel.UserCode = flowNodePersonnelList[i].PersonnelCode;
                                        fpoModel.UserType = flowNodePersonnelList[i].ActionType;
                                        fpoModel.PerformState = -1;
                                        fpoModel.ReviewNodeCode = "-1";
                                        fpoModel.ReviewWhy = "";
                                        fpoList.Add(fpoModel);
                                    }
                                }
                                var fdModel = Repository<TbFlowDefine>.First(p => p.FlowCode == FlowCode);
                                if (fdModel.FlowSpOrCsType == "抄送")
                                {
                                    //更改业务单据状态为审核完成
                                    sqlYwTable = "update " + TableName + " set Examinestatus='审核完成' where ID=" + FormDataCode + "";
                                }
                            }
                        }
                    }
                    //获取流程预警信息
                    sqlEarlyInfo = @"update TbFlowEarlyWarningOtherInfo set EarlyWarningStart=2 
                                     where FlowPerformID='"+FlowPerformID+@"' and FlowNodeCode='"+FlowNodeCode+@"' 
                                     and EWFormCode='" + FormCode +@"' and EWFormDataCode="+FormDataCode+" and EarlyWarningStart=0";

                }
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    Repository<TbFlowPerform>.Update(trans, model);
                    if (!string.IsNullOrWhiteSpace(sqlYwTable))
                    {
                        Db.Context.FromSql(sqlYwTable).SetDbTransaction(trans).ExecuteNonQuery();
                    }
                    if (!string.IsNullOrWhiteSpace(sqlFe))
                    {
                        Db.Context.FromSql(sqlFe).AddInParameter("@ID", DbType.String, FormDataCode).SetDbTransaction(trans).ExecuteNonQuery();
                    }
                    if (fpoList.Count > 0)
                    {
                        Repository<TbFlowPerformOpinions>.Insert(trans, fpoList);
                    }
                    if (!string.IsNullOrWhiteSpace(sqlEarlyInfo))
                    {
                          Db.Context.FromSql(sqlEarlyInfo).SetDbTransaction(trans).ExecuteNonQuery();
                    }
                    trans.Commit();
                    flag = true;
                }
            }
            catch (Exception ex)
            {
                flag = false;
            }
            return flag;
        }
        public string GetNextFlowPerformID(string CodeType)
        {
            string FlowPerformID = "";
            string sqlType = "select CodePrefix,CodeValue from TbFlowCode where CodeType=@CodeType";
            DataTable dt = Db.Context.FromSql(sqlType)
                  .AddInParameter("@CodeType", DbType.String, CodeType)
                  .ToDataTable();
            if (dt.Rows.Count > 0)
            {
                int code = Convert.ToInt32(dt.Rows[0]["CodeValue"]) + 1;
                FlowPerformID += dt.Rows[0]["CodePrefix"].ToString() + code;
            }
            return FlowPerformID;
        }
        #endregion

        #region 新版我的消息TQ
        public DataTable GetMessageStateNew(string UserId, string ProjectId, DateTime? SDT, DateTime? EDT)
        {
            string where = "";
            if (!string.IsNullOrWhiteSpace(UserId))
            {
                where += " and a.EWUserCode=@UserId";
            }
            if (!string.IsNullOrWhiteSpace(ProjectId))
            {
                where += " and a.ProjectId=@ProjectId";
            }

            string sql = @"select 0 as EWStart,'未阅' as EWStartName,COUNT(1) as StartCount from TbFormEarlyWarningNodeInfo a
            left join TbNoticeNewsSetUp b on a.EarlyWarningCode=b.NoticeNewsCode
            left join TbSysMenu c on a.MenuCode=c.MenuCode
            left join TbUser d on a.EWUserCode=d.UserId
            left join TbCompany e on a.WorkArea=e.CompanyCode
            where a.MsgType=1 
            and (ISNULL(@SDT,'')='' or a.EWTime>=@SDT)
            and (ISNULL(@EDT,'')='' or a.EWTime<=@EDT)
            and a.EWStart=0 " + where + @"
            union all
            select 1 as EWStart,'已阅' as EWStartName,COUNT(1) as StartCount from TbFormEarlyWarningNodeInfo a
            left join TbNoticeNewsSetUp b on a.EarlyWarningCode=b.NoticeNewsCode
            left join TbSysMenu c on a.MenuCode=c.MenuCode
            left join TbUser d on a.EWUserCode=d.UserId
            left join TbCompany e on a.WorkArea=e.CompanyCode
            where a.MsgType=1 
            and (ISNULL(@SDT,'')='' or a.EWTime>=@SDT)
            and (ISNULL(@EDT,'')='' or a.EWTime<=@EDT)
            and a.EWStart=1 " + where + @"";
            try
            {
                var ret = Db.Context.FromSql(sql)
                .AddInParameter("@UserId", DbType.String, UserId)
                .AddInParameter("@ProjectId", DbType.String, ProjectId)
                .AddInParameter("@SDT", DbType.DateTime, SDT)
                .AddInParameter("@EDT", DbType.DateTime, EDT).ToDataTable(); ;
                return ret;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public PageModel GetMessageStateList(PageSearchRequest pt, string UserId, string ProjectId, int status, DateTime? SDT, DateTime? EDT)
        {
            string where = "";
            if (!string.IsNullOrWhiteSpace(UserId))
            {
                where += " and a.EWUserCode=@UserId";
            } if (!string.IsNullOrWhiteSpace(ProjectId))
            {
                where += " and a.ProjectId=@ProjectId";
            }

            string sql = @"select a.ID,a.ProjectId,a.ProcessFactoryCode,a.EWUserCode,c.MenuName,b.NoticeNewsName as messageType,a.EarlyTitle as messageTitle,d.UserName,a.EWContent as messageContent,a.EWStart,case when a.EWStart=0 then '未阅' else '已阅' end isread,a.EWTime as messageCreateTime from TbFormEarlyWarningNodeInfo a
            left join TbNoticeNewsSetUp b on a.EarlyWarningCode=b.NoticeNewsCode
            left join TbSysMenu c on a.MenuCode=c.MenuCode
            left join TbUser d on a.EWUserCode=d.UserId
            left join TbCompany e on a.WorkArea=e.CompanyCode
            where a.MsgType=1
            and (ISNULL(@SDT,'')='' or a.EWTime>=@SDT)
            and (ISNULL(@EDT,'')='' or a.EWTime<=@EDT)
            and a.EWStart=@EWStart " + where + @"";
            try
            {
                List<Parameter> parameter = new List<Parameter>();
                parameter.Add(new Parameter("@UserId", UserId, DbType.String, null));
                parameter.Add(new Parameter("@ProjectId", ProjectId, DbType.String, null));
                parameter.Add(new Parameter("@SDT", SDT, DbType.String, null));
                parameter.Add(new Parameter("@EDT", EDT, DbType.String, null));
                parameter.Add(new Parameter("@EWStart", status, DbType.Int32, null));
                var ret = Repository<TbFormEarlyWarningNodeInfo>.FromSqlToPageTable(sql, parameter, pt.rows, pt.page, "messageCreateTime");
                return ret;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public AjaxResult UpdateStatusNew(int ID)
        {
            string sql = "update TbFormEarlyWarningNodeInfo set EWStart=1 where id=@ID";
            int reslut = Db.Context.FromSql(sql).AddInParameter("ID", DbType.Int32, ID).ExecuteNonQuery();
            if (reslut > 0)
                return AjaxResult.Success("修改成功");
            else return AjaxResult.Error("修改失败");

        }
        #endregion
    }
}
