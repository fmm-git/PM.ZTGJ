using Dos.ORM;
using PM.Business.Production;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using PM.DataEntity.EarlyWarning.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Business.EarlyWarning
{
    public class TbFormEarlyWarningNodeInfoLogic
    {
        private readonly TbWorkOrderLogic _workOrderLogic = new TbWorkOrderLogic();

        public DataTable GetEarlyWarningInfoList(string UserCode, int Start)
        {
            string sql = @" select TbEwInfo.*,sm.MenuName from (select * from (select fn.FlowNodeName,fewi.EWFormCode,fewi.EWFormDataCode,'流程预警' as EwType,fewi.EarlyWarningContent,fewi.EarlyWarningStart,fewi.EarlyWarningTime,fewi.EarlyWarningBTUser from TbFlowEarlyWarningInfo fewi
                            left join TbFlowDefine fd on fewi.FlowCode=fd.FlowCode and fewi.EWFormCode=fd.FormCode
                            left join TbFlowNode fn on fewi.FlowCode=fn.FlowCode and fewi.FlowNodeCode=fn.FlowNodeCode) Tb1
                            union all
                            select * from (select fewn.EWNodeName,fewni.MenuCode,fewni.EWFormDataCode,'表单预警' as EwType,fewni.EWContent,fewni.EWStart,fewni.EWTime,fewni.EWUserCode from TbFormEarlyWarningNodeInfo fewni
                            left join TbFormEarlyWarningNode fewn on fewn.MenuCode=fewni.MenuCode and fewn.EWNodeCode=fewni.EWNodeCode) Tb2) TbEwInfo 
                            left join TbSysMenu sm on TbEwInfo.EWFormCode=sm.MenuCode where TbEwInfo.EarlyWarningBTUser=@EarlyWarningBTUser and TbEwInfo.EarlyWarningStart=@Start";
            DataTable dt = Db.Context.FromSql(sql).AddInParameter("@EarlyWarningBTUser", DbType.String, UserCode)
                .AddInParameter("@Start", DbType.Int32, Start)
                .ToDataTable();

            return dt;
        }

        public DataTable GetFlowEarlyWarningInfoList(string UserCode, int Start)
        {
            string sql = @"select ewi.ID,sm.MenuName,fo.PreNodeCompleteDate,u2.UserName as FlowUserName,ewi.EWFormDataCode,ewc.EarlyWarningName,ewi.EarlyWarningStart,ewi.EarlyWarningContent,ewi.EarlyWarningTime as EarlyWarningBegTime from TbFlowEarlyWarningInfo ewi
left join (select po.* from (select MAX(id) as id,FlowNodeCode,FlowPerformID from TbFlowPerformOpinions group by FlowNodeCode,FlowPerformID) Tb
left join TbFlowPerformOpinions po on Tb.id=po.id) fo on ewi.FlowPerformID=fo.FlowPerformID and ewi.FlowNodeCode=fo.FlowNodeCode
left join TbSysMenu sm on ewi.EWFormCode=sm.MenuCode
left join TbFlowEarlyWarningCondition ewc on ewi.EarlyWarningCode=ewc.EarlyWarningCode and ewi.FlowCode=ewc.FlowCode and ewi.FlowNodeCode=ewc.FlowNodeCode
left join TbUser u1 on ewi.EarlyWarningBTUser=u1.UserId
left join TbUser u2 on fo.UserCode=u2.UserId
where ewi.EarlyWarningStart=@Start and ewi.EarlyWarningBTUser=(select UserId from TbUser where UserCode=@EarlyWarningBTUser)";
            DataTable dt = Db.Context.FromSql(sql).AddInParameter("@EarlyWarningBTUser", DbType.String, UserCode)
                .AddInParameter("@Start", DbType.Int32, Start)
                .ToDataTable();

            return dt;
        }

        public DataTable GetFormEarlyWarningInfoList(string UserCode, int Start)
        {
            string sql = @"select sm.MenuName,ewi.EWFormDataCode,case CONVERT(VARCHAR(50),ewi.EWNodeCode) when '1' then '工区物机部部长预警' when '2' then '分部物机部部长预警' when '3' then '经理部部长预警' else '其他预警' end as EWNodeName,c1.CompanyFullName as GqName,c2.CompanyFullName  as FbOrGdName,ewi.EWContent,ewi.EWTime,ewi.EWStart from TbFormEarlyWarningNodeInfo ewi
left join TbFormEarlyWarningNode fewn on ewi.MenuCode=fewn.MenuCode and ewi.EWNodeCode=fewn.EWNodeCode
left join TbSysMenu sm on ewi.MenuCode=sm.MenuCode
left join TbCompany c1 on ewi.WorkArea=c1.CompanyCode
left join TbCompany c2 on ewi.CompanyCode=c2.CompanyCode
left join TbUser u1 on ewi.EWUserCode=u1.UserCode where ewi.EWUserCode=@EarlyWarningBTUser and ewi.EWStart=@Start and ewi.MenuCode='RawMonthDemandPlan'
union all
select sm.MenuName,ewi.EWFormDataCode,case CONVERT(VARCHAR(50),ewi.EWNodeCode) when '1' then '站点负责人、加工厂调度人员预警' when '2' then '站点工程部部长、加工厂厂长预警' when '3' then '工区工程部部长预警' when '4' then '分部工程部部长预警' when '5' then '经理部工程部部长预警' else '其他预警' end as EWNodeName,c1.CompanyFullName as GqName,c2.CompanyFullName as FbOrGdName,ewi.EWContent,ewi.EWTime,ewi.EWStart from TbFormEarlyWarningNodeInfo ewi
left join TbFormEarlyWarningNode fewn on ewi.MenuCode=fewn.MenuCode and ewi.EWNodeCode=fewn.EWNodeCode
left join TbSysMenu sm on ewi.MenuCode=sm.MenuCode
left join TbCompany c1 on ewi.WorkArea=c1.CompanyCode
left join TbCompany c2 on ewi.CompanyCode=c2.CompanyCode
left join TbUser u1 on ewi.EWUserCode=u1.UserCode where ewi.EWUserCode=@EarlyWarningBTUser and ewi.EWStart=@Start and ewi.MenuCode='DistributionEnt'";
            DataTable dt = Db.Context.FromSql(sql).AddInParameter("@EarlyWarningBTUser", DbType.String, UserCode)
                .AddInParameter("@Start", DbType.Int32, Start)
                .ToDataTable();

            return dt;
        }

        /// <summary>
        /// 处理流程预警信息
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public AjaxResult HandleEarlyWarning(int ID, string EwType)
        {
            try
            {
                if (EwType == "流程预警")
                {
                    var earlywarningInfo = Repository<TbFlowEarlyWarningInfo>.First(p => p.ID == ID);
                    if (earlywarningInfo == null)
                        return AjaxResult.Warning("预警信息不存在");
                    earlywarningInfo.EarlyWarningStart = 1;//已阅
                    Repository<TbFlowEarlyWarningInfo>.Update(earlywarningInfo, true);
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


        public DataTable GetDeliveryEarlyWarningList(int EWStart = 0, string UserCode = "", string ProjectId = "")
        {
            #region 模糊搜索条件

            var where = new Where<TbFormEarlyWarningNodeInfo>();
            where.And(d => d.ProjectId == ProjectId);
            where.And(d => d.EWUserCode == UserCode);
            where.And(d => d.EWStart == EWStart);

            #endregion

            try
            {
                var data = Db.Context.From<TbFormEarlyWarningNodeInfo>()
                    .Select(
                      TbFormEarlyWarningNodeInfo._.All
                    , TbUser._.UserName
                    , TbCompany._.CompanyFullName.As("SiteName")
                    , TbSysMenu._.MenuName)
                    .AddSelect(Db.Context.From<TbCompany>()
                      .Select(p => p.CompanyFullName)
                      .Where(TbCompany._.CompanyCode == TbFormEarlyWarningNodeInfo._.WorkArea), "WorkAreaName")
                  .LeftJoin<TbUser>((a, c) => a.EWUserCode == c.UserCode)
                  .LeftJoin<TbCompany>((a, c) => a.CompanyCode == c.CompanyCode)
                  .LeftJoin<TbSysMenu>((a, c) => a.MenuCode == c.MenuCode)
                  .Where(where)
                  .OrderByDescending(p => p.ID).ToDataTable();
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region App首页预警信息
        /// <summary>
        /// 首页报警信息分析
        /// </summary>
        /// <returns></returns>
        public DataTable GetAlarmMessage(string Year, string Month,string ProjectId,string ProcessFactoryCode,string SiteCode)
        {

            #region 审批预警条件

            string where1 = "";
            if (!string.IsNullOrWhiteSpace(ProjectId))
            {
                where1 += " and fewoi.ProjectId='" + ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(ProcessFactoryCode))
            {
                where1 += " and fewoi.ProcessFactoryCode='" + ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(SiteCode, 5);//站点
                List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(SiteCode, 4);//工区
                string siteStr = string.Join("','", SiteList);
                string workAreaStr = string.Join("','", WorkAreaList);
                where1 += (" and (fewoi.SiteCode in('" + siteStr + "') or fewoi.WorkAreaCode in('" + workAreaStr + "'))");
            }

            #endregion

            #region 未报月度需求计划预警

            string where2 = "";
            if (!string.IsNullOrWhiteSpace(ProjectId))
            {
                where2 += " and fwqi.ProjectId='" + ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(SiteCode, 5);//站点
                List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(SiteCode, 4);//工区
                string siteStr = string.Join("','", SiteList);
                string workAreaStr = string.Join("','", WorkAreaList);
                where2 += (" and (fwqi.SiteCode in('" + siteStr + "') or fwqi.WorkArea in('" + workAreaStr + "'))");
            }

            #endregion

            #region 原材料供货超时

            string where3 = "";
            if (!string.IsNullOrWhiteSpace(ProjectId))
            {
                where3 += " and sup.ProjectId='" + ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(ProcessFactoryCode))
            {
                where3 += " and sup.ProcessFactoryCode='" + ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(SiteCode, 5);//站点
                List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(SiteCode, 4);//工区
                string siteStr = string.Join("','", SiteList);
                string workAreaStr = string.Join("','", WorkAreaList);
                where3 += (" and (sup.SiteCode in('" + siteStr + "') or sup.WorkAreaCode in('" + workAreaStr + "'))");
            }

            #endregion

            #region 加急订单

            string where4 = "";
            if (!string.IsNullOrWhiteSpace(ProjectId))
            {
                where4 += " and wo.ProjectId='" + ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(ProcessFactoryCode))
            {
                where4 += " and wo.ProcessFactoryCode='" + ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(SiteCode, 5);//站点
                string siteStr = string.Join("','", SiteList);
                where4 += (" and wo.SiteCode in('" + siteStr + "') ");
            }

            #endregion

            #region 加工进度滞后

            string where5 = "";
            if (!string.IsNullOrWhiteSpace(ProjectId))
            {
                where5 += " and op.ProjectId='" + ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(ProcessFactoryCode))
            {
                where5 += " and op.ProcessFactoryCode='" + ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(SiteCode, 5);//站点
                string siteStr = string.Join("','", SiteList);
                where5 += (" and op.SiteCode in('" + siteStr + "') ");
            }

            #endregion

            #region 配送超期

            string where6 = "";
            if (!string.IsNullOrWhiteSpace(ProjectId))
            {
                where6 += " and dpi.ProjectId='" + ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(ProcessFactoryCode))
            {
                where6 += " and dpi.ProcessFactoryCode='" + ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(SiteCode, 5);//站点
                string siteStr = string.Join("','", SiteList);
                where6 += (" and dpi.SiteCode in('" + siteStr + "') ");
            }

            #endregion

            #region 卸货超时

            string where7 = "";
            if (!string.IsNullOrWhiteSpace(ProjectId))
            {
                where7 += " and TbYj.ProjectId='" + ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(ProcessFactoryCode))
            {
                where7 += " and disEnt.ProcessFactoryCode='" + ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(SiteCode, 5);//站点
                string siteStr = string.Join("','", SiteList);
                where7 += (" and TbYj.SiteCode in('" + siteStr + "') ");
            }

            #endregion

            #region 签收超时

            string where8 = "";
            if (!string.IsNullOrWhiteSpace(ProjectId))
            {
                where8 += " and TbYj.ProjectId='" + ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(ProcessFactoryCode))
            {
                where8 += " and sfs.ProcessFactoryCode='" + ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(SiteCode, 5);//站点
                string siteStr = string.Join("','", SiteList);
                where8 += (" and TbYj.SiteCode in('" + siteStr + "') ");
            }

            #endregion

            string sql = @"select '审批超时' as YjTypeName,COUNT(1) as YjNum from (
                           select left(substring(fewoi.EarlyWarningContent, charindex(N'《',fewoi.EarlyWarningContent)+1, charindex(N'》',fewoi.EarlyWarningContent)-1),charindex('》',substring(fewoi.EarlyWarningContent, charindex(N'《',fewoi.EarlyWarningContent)+1, charindex(N'》',fewoi.EarlyWarningContent)-1))-1) as OddNumbers,fewoi.EarlyWarningContent,fewoi.EWFormDataCode,CONVERT(varchar(100), fewoi.EarlyWarningTime, 120) as EarlyWarningTime,EWFormCode,sm.MenuName,fn.FlowNodeName,cp1.CompanyFullName as BranchName,cp2.CompanyFullName as WorkAreaName,cp3.CompanyFullName as SiteName,cp4.CompanyFullName as ProcessFactoryName,u1.UserName as FlowSpUserName,u2.UserName as FlowYjUserName from TbFlowEarlyWarningOtherInfo fewoi
						   left join TbFlowNode fn on fewoi.FlowCode=fn.FlowCode and fewoi.FlowNodeCode=fn.FlowNodeCode
						   left join TbSysMenu sm on fewoi.EWFormCode=sm.MenuCode
						   left join TbCompany cp1 on fewoi.BranchCode=cp1.CompanyCode
						   left join TbCompany cp2 on fewoi.WorkAreaCode=cp2.CompanyCode
						   left join TbCompany cp3 on fewoi.SiteCode=cp3.CompanyCode
						   left join TbCompany cp4 on fewoi.ProcessFactoryCode=cp4.CompanyCode
						   left join TbUser u1 on fewoi.FlowSpUserCode=u1.UserId
						   left join TbUser u2 on fewoi.FlowYjUserCode=u2.UserId
						   where YEAR(fewoi.EarlyWarningTime)= @Year and MONTH(fewoi.EarlyWarningTime)= @Month " + where1 + @") Tb1
                           union all
                           select '未报月度需求计划' as YjTypeName,COUNT(1) as YjNum from (
                           select fwqi.EWContent,fwqi.EWUserCode,EWFormDataCode,cp1.CompanyFullName as  FbName,cp2.CompanyFullName as GqName, fwqi.EWTime from TbFormEarlyWarningNodeInfo  fwqi
                           left join TbSysMenu sm on fwqi.MenuCode=sm.MenuCode
                           left join TbCompany cp1 on fwqi.CompanyCode=cp1.CompanyCode
                           left join TbCompany cp2 on fwqi.WorkArea=cp2.CompanyCode
                           left join TbCompany cp3 on fwqi.SiteCode=cp3.CompanyCode
                           where fwqi.MenuCode='RawMonthDemandPlan' 
						   and YEAR(fwqi.EWTime)= @Year and MONTH(fwqi.EWTime)= @Month " + where2 + @") Tb2
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
                           select '配送超期' as YjTypeName,COUNT(1) as YjNum from (--配送超期
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
                .AddInParameter("@Year", DbType.String, Year)
                .AddInParameter("@Month", DbType.String, Month).ToDataTable();
            return ret;
        }
        /// <summary>
        /// 审批超时
        /// </summary>
        /// <param name="Month">月份</param>
        /// <returns></returns>
        public DataTable GetSpCsYjList(string Year, string Month, string ProjectId, string ProcessFactoryCode, string SiteCode)
        {
            string where = "";
            string sql = @"select left(substring(fewoi.EarlyWarningContent, charindex(N'《',fewoi.EarlyWarningContent)+1, charindex(N'》',fewoi.EarlyWarningContent)-1),charindex('》',substring(fewoi.EarlyWarningContent, charindex(N'《',fewoi.EarlyWarningContent)+1, charindex(N'》',fewoi.EarlyWarningContent)-1))-1) as OddNumbers,fewoi.EarlyWarningContent,fewoi.EWFormDataCode,CONVERT(varchar(100), fewoi.EarlyWarningTime, 120) as EarlyWarningTime,EWFormCode,sm.MenuName,fn.FlowNodeName,cp1.CompanyFullName as BranchName,cp2.CompanyFullName as WorkAreaName,cp3.CompanyFullName as SiteName,cp4.CompanyFullName as ProcessFactoryName,u1.UserName as FlowSpUserName,u2.UserName as FlowYjUserName from TbFlowEarlyWarningOtherInfo fewoi
left join TbFlowNode fn on fewoi.FlowCode=fn.FlowCode and fewoi.FlowNodeCode=fn.FlowNodeCode
left join TbSysMenu sm on fewoi.EWFormCode=sm.MenuCode
left join TbCompany cp1 on fewoi.BranchCode=cp1.CompanyCode
left join TbCompany cp2 on fewoi.WorkAreaCode=cp2.CompanyCode
left join TbCompany cp3 on fewoi.SiteCode=cp3.CompanyCode
left join TbCompany cp4 on fewoi.ProcessFactoryCode=cp4.CompanyCode
left join TbUser u1 on fewoi.FlowSpUserCode=u1.UserId
left join TbUser u2 on fewoi.FlowYjUserCode=u2.UserId
where YEAR(fewoi.EarlyWarningTime)= @Year and MONTH(fewoi.EarlyWarningTime)= @Month";
            if (!string.IsNullOrWhiteSpace(ProjectId))
            {
                where += " and fewoi.ProjectId='" + ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(ProcessFactoryCode))
            {
                where += " and fewoi.ProcessFactoryCode='" + ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(SiteCode, 5);//站点
                List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(SiteCode, 4);//工区
                string siteStr = string.Join("','", SiteList);
                string workAreaStr = string.Join("','", WorkAreaList);
                where += (" and (fewoi.SiteCode in('" + siteStr + "') or fewoi.WorkAreaCode in('" + workAreaStr + "'))");
            }
            var ret = Db.Context.FromSql(sql + where)
                .AddInParameter("@Year", DbType.String, Year)
                .AddInParameter("@Month", DbType.String, Month).ToDataTable();
            return ret;
        }

        /// <summary>
        /// 未按时提报月度需求计划
        /// </summary>
        /// <param name="Year"></param>
        /// <param name="Month"></param>
        /// <returns></returns>
        public DataTable GetWbydxqjhYjList(string Year, string Month, string ProjectId, string ProcessFactoryCode, string SiteCode)
        {
            string where = "";
            if (!string.IsNullOrWhiteSpace(ProjectId))
            {
                where += " and fwqi.ProjectId='" + ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(SiteCode, 5);//站点
                List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(SiteCode, 4);//工区
                string siteStr = string.Join("','", SiteList);
                string workAreaStr = string.Join("','", WorkAreaList);
                where += (" and (fwqi.SiteCode in('" + siteStr + "') or fwqi.WorkArea in('" + workAreaStr + "'))");
            }
            string sql = @"--未按时提报月度需求计划
                          select sm.MenuName,fwqi.EWContent,fwqi.EWUserCode,u.UserName,EWFormDataCode,cp1.CompanyFullName as  FbName,cp2.CompanyFullName as GqName,fwqi.EWTime from TbFormEarlyWarningNodeInfo  fwqi
                            left join TbSysMenu sm on fwqi.MenuCode=sm.MenuCode
                            left join TbCompany cp1 on fwqi.CompanyCode=cp1.CompanyCode
                            left join TbCompany cp2 on fwqi.WorkArea=cp2.CompanyCode
                            left join TbCompany cp3 on fwqi.SiteCode=cp3.CompanyCode
                            left join TbUser u on fwqi.EWUserCode=u.UserId
                            where fwqi.MenuCode='RawMonthDemandPlan' 
                            and YEAR(fwqi.EWTime)= @Year and MONTH(fwqi.EWTime)= @Month ";
            var ret = Db.Context.FromSql(sql + where)
                .AddInParameter("@Year", DbType.String, Year)
                .AddInParameter("@Month", DbType.String, Month).ToDataTable();
            return ret;
        }

        /// <summary>
        /// 原材料供货超时
        /// </summary>
        /// <param name="Year"></param>
        /// <param name="Month"></param>
        /// <returns></returns>
        public DataTable GetYclghCsYjList(string Year, string Month, string ProjectId, string ProcessFactoryCode, string SiteCode)
        {
            string where = "";
            if (!string.IsNullOrWhiteSpace(ProjectId))
            {
                where += " and sup.ProjectId='" + ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(ProcessFactoryCode))
            {
                where += " and sup.ProcessFactoryCode='" + ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(SiteCode, 5);//站点
                List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(SiteCode, 4);//工区
                string siteStr = string.Join("','", SiteList);
                string workAreaStr = string.Join("','", WorkAreaList);
                where += (" and (sup.SiteCode in('" + siteStr + "') or sup.WorkArea in('" + workAreaStr + "'))");
            }
            string sql = @"--原材料供货超时
                           select BatchPlanNum,cp1.CompanyFullName as BranchName,cp2.CompanyFullName as WorkAreaName,cp3.CompanyFullName as SiteName,cp4.CompanyFullName as ProcessFactoryName,sup.SupplyDate,sup.SupplyCompleteTime,sup.ProjectId from TbSupplyList sup
                           left join TbCompany cp1 on cp1.CompanyCode=sup.BranchCode
                           left join TbCompany cp2 on cp2.CompanyCode=sup.WorkAreaCode
                           left join TbCompany cp3 on cp3.CompanyCode=sup.SiteCode
                           left join TbCompany cp4 on cp4.CompanyCode=sup.ProcessFactoryCode
                           where YEAR(sup.SupplyDate)= @Year and MONTH(sup.SupplyDate)= @Month
                           and (sup.SupplyCompleteTime>sup.SupplyDate or (GETDATE()>sup.SupplyDate and sup.SupplyCompleteTime is null)) ";
            var ret = Db.Context.FromSql(sql + where)
                .AddInParameter("@Year", DbType.String, Year)
                .AddInParameter("@Month", DbType.String, Month).ToDataTable();
            return ret;
        }

        /// <summary>
        /// 加急订单个数
        /// </summary>
        /// <param name="Year"></param>
        /// <param name="Month"></param>
        /// <returns></returns>
        public DataTable GetJjOrderYjList(string Year, string Month, string ProjectId, string ProcessFactoryCode, string SiteCode)
        {
            string where = "";
            if (!string.IsNullOrWhiteSpace(ProjectId))
            {
                where += " and wo.ProjectId='" + ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(ProcessFactoryCode))
            {
                where += " and wo.ProcessFactoryCode='" + ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(SiteCode, 5);//站点
                string siteStr = string.Join("','", SiteList);
                where += (" and wo.SiteCode in('" + siteStr + "') ");
            }
            string sql = @"--加急订单个数
                           select wo.OrderCode,wo.DistributionTime,cp1.CompanyFullName as SiteName,cp2.CompanyFullName as ProcessFactoryName from TbWorkOrder wo
                           left join TbCompany cp1 on wo.SiteCode=cp1.CompanyCode
                           left join TbCompany cp2 on wo.ProcessFactoryCode=cp2.CompanyCode
                           where UrgentDegree='Urgent'
                           and YEAR(wo.DistributionTime)= @Year and MONTH(wo.DistributionTime)= @Month ";
            var ret = Db.Context.FromSql(sql + where)
                .AddInParameter("@Year", DbType.String, Year)
                .AddInParameter("@Month", DbType.String, Month).ToDataTable();
            return ret;
        }

        /// <summary>
        /// 加工进度滞后
        /// </summary>
        /// <param name="Year"></param>
        /// <param name="Month"></param>
        /// <returns></returns>
        public DataTable GetJgOrderZhYjList(string Year, string Month, string ProjectId, string ProcessFactoryCode, string SiteCode)
        {
            string where = "";
            if (!string.IsNullOrWhiteSpace(ProjectId))
            {
                where += " and op.ProjectId='" + ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(ProcessFactoryCode))
            {
                where += " and op.ProcessFactoryCode='" + ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(SiteCode, 5);//站点
                string siteStr = string.Join("','", SiteList);
                where += (" and op.SiteCode in('" + siteStr + "')");
            }
            string sql = @"--加工进度滞后
                           select op.OrderCode,cp1.CompanyFullName as SiteName,cp2.CompanyFullName as ProcessFactoryName,op.DistributionTime,op.FinishProcessingDateTime from TbOrderProgress op 
                           left join TbCompany cp1 on op.SiteCode=cp1.CompanyCode
                           left join TbCompany cp2 on op.ProcessFactoryCode=cp2.CompanyCode
                           where YEAR(op.DistributionTime)= @Year and MONTH(op.DistributionTime)= @Month
                           and (op.FinishProcessingDateTime>op.DistributionTime or (GETDATE()>op.DistributionTime and op.FinishProcessingDateTime is null)) ";
            var ret = Db.Context.FromSql(sql + where)
                .AddInParameter("@Year", DbType.String, Year)
                .AddInParameter("@Month", DbType.String, Month).ToDataTable();
            return ret;
        }

        /// <summary>
        /// 配送超期
        /// </summary>
        /// <param name="Year"></param>
        /// <param name="Month"></param>
        /// <returns></returns>
        public DataTable GetPsCsYjList(string Year, string Month, string ProjectId, string ProcessFactoryCode, string SiteCode)
        {
            string where = "";
            if (!string.IsNullOrWhiteSpace(ProjectId))
            {
                where += " and dpi.ProjectId='" + ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(ProcessFactoryCode))
            {
                where += " and dpi.ProcessFactoryCode='" + ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(SiteCode, 5);//站点
                string siteStr = string.Join("','", SiteList);
                where += (" and dpi.SiteCode in('" + siteStr + "')");
            }
            string sql = @"--配送超期
                           select dpi.OrderCode,cp1.CompanyFullName as SiteName,cp2.CompanyFullName as ProcessFactoryName,dpi.DistributionTime,dpi.DeliveryCompleteTime from TbDistributionPlanInfo dpi
                           left join TbCompany cp1 on dpi.SiteCode=cp1.CompanyCode
                           left join TbCompany cp2 on dpi.ProcessFactoryCode=cp2.CompanyCode
                           where YEAR(dpi.DistributionTime)= @Year and MONTH(dpi.DistributionTime)= @Month
                           and (dpi.DeliveryCompleteTime>dpi.DistributionTime or (GETDATE()>dpi.DistributionTime and dpi.DeliveryCompleteTime is null)) ";
            var ret = Db.Context.FromSql(sql + where)
                .AddInParameter("@Year", DbType.String, Year)
                .AddInParameter("@Month", DbType.String, Month).ToDataTable();
            return ret;
        }

        /// <summary>
        /// 卸货超期
        /// </summary>
        /// <param name="Year"></param>
        /// <param name="Month"></param>
        /// <returns></returns>
        public DataTable GetXhCsYjList(string Year, string Month, string ProjectId, string ProcessFactoryCode, string SiteCode)
        {
            string where = "";
            if (!string.IsNullOrWhiteSpace(ProjectId))
            {
                where += " and TbYj.ProjectId='" + ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(ProcessFactoryCode))
            {
                where += " and disEnt.ProcessFactoryCode='" + ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(SiteCode, 5);//站点
                string siteStr = string.Join("','", SiteList);
                where += (" and TbYj.SiteCode in('" + siteStr + "')");
            }
            string sql = @"select TbYj.*,cp1.CompanyFullName as SiteName,tcr.EnterSpaceTime,tcr.StartDischargeTime,tcr.EndDischargeTime,disEnt.ProcessFactoryCode,cp.CompanyFullName as ProcessFactoryName from (select min(EwTime) as EwTime,MenuCode,EWFormDataCode,CompanyCode,WorkArea,SiteCode,ProjectId from TbFormEarlyWarningNodeInfo where  MenuCode='SiteDischargeCargo' 
						   group by MenuCode,EWFormDataCode,CompanyCode,WorkArea,SiteCode,ProjectId) TbYj
						   left join TbTransportCarReport tcr on TbYj.EWFormDataCode=tcr.DisEntOrderId
                           left join TbDistributionEnt disEnt on tcr.DistributionCode=disEnt.DistributionCode
                           left join TbCompany cp on disEnt.ProcessFactoryCode=cp.CompanyCode
                           left join TbCompany cp1 on TbYj.SiteCode=cp1.CompanyCode
						   where YEAR(TbYj.EWTime)= @Year and MONTH(TbYj.EWTime)= @Month ";
            var ret = Db.Context.FromSql(sql + where)
                .AddInParameter("@Year", DbType.String, Year)
                .AddInParameter("@Month", DbType.String, Month).ToDataTable();
            return ret;
        }

        /// <summary>
        /// 签收超期
        /// </summary>
        /// <param name="Year"></param>
        /// <param name="Month"></param>
        /// <returns></returns>
        public DataTable GetQsCsYjList(string Year, string Month, string ProjectId, string ProcessFactoryCode, string SiteCode)
        {
            string where = "";
            if (!string.IsNullOrWhiteSpace(ProjectId))
            {
                where += " and TbYj.ProjectId='" + ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(ProcessFactoryCode))
            {
                where += " and sfs.ProcessFactoryCode='" + ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(SiteCode, 5);//站点
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
            var ret = Db.Context.FromSql(sql + where)
                .AddInParameter("@Year", DbType.String, Year)
                .AddInParameter("@Month", DbType.String, Month).ToDataTable();
            return ret;
        }
        #endregion
    }
}
