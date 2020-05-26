using Dos.ORM;
using PM.Business.ShortMessage;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using PM.DataEntity.EarlyWarning.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Threading;
using System.Linq;
using Dos.Common;

namespace PM.Business.EarlyWarning
{
    public class EarlyWarningTimer
    {
        public static bool status1 = true;
        public static bool status2 = true;
        public static bool status3 = true;
        public static bool status4 = true;
        //发送短信
        CensusdemoTask ct = new CensusdemoTask();
        public EarlyWarningTimer()
        {
            Thread a = new Thread(g =>
                {
                    try
                    {
                        while (true)
                        {
                            if (status1)
                            {
                                status1 = false;
                                GetEarlyInfo();
                                Thread.Sleep(1000 * 60 * 5);
                            }
                            else
                            {
                                Thread.Sleep(1000 * 60);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //线程里面的代码报错了，，，就会到这来，，，但是你也不确定线程里面代码会不会报错
                        //此处，睡眠30秒，，，最好这里面写个日志记录下来。。
                        Dos.Common.LogHelper.Error("超时预警:" + ex.Message, "系统日志");
                        Thread.Sleep(1000 * 30);
                    }
                });
            a.IsBackground = true;
            a.Start();
            //在每个月的二十号开始预警
            DateTime dt1 = Convert.ToDateTime(DateTime.Now.ToString());
            DateTime dt2 = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM" + "-19 09:00:00"));
            DateTime dt3 = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM" + "-24 09:00:00"));
            if (dt1 > dt2 && dt1 < dt3)
            {
                Thread b = new Thread(g =>
                {
                    try
                    {
                        while (true)
                        {
                            if (status2)
                            {
                                status2 = false;
                                GetRamDmPlanEarlyInfo();
                                Thread.Sleep(1000 * 60 * 5);
                            }
                            else
                            {
                                Thread.Sleep(1000 * 60);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //线程里面的代码报错了，，，就会到这来，，，但是你也不确定线程里面代码会不会报错
                        //此处，睡眠30秒，，，最好这里面写个日志记录下来。。
                        Dos.Common.LogHelper.Error("超时预警:" + ex.Message, "系统日志");
                        Thread.Sleep(1000 * 30);
                    }
                });
                b.IsBackground = true;
                b.Start();
            }

            Thread c = new Thread(g =>
            {
                try
                {
                    while (true)
                    {
                        if (status3)
                        {
                            status3 = false;
                            GetTransportShortMessageWarn();
                            Thread.Sleep(1000 * 60 * 5);
                        }
                        else
                        {
                            Thread.Sleep(1000 * 60);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //线程里面的代码报错了，，，就会到这来，，，但是你也不确定线程里面代码会不会报错
                    //此处，睡眠30秒，，，最好这里面写个日志记录下来。。
                    Dos.Common.LogHelper.Error("超时预警:" + ex.Message, "系统日志");
                    Thread.Sleep(1000 * 30);
                }
            });
            c.IsBackground = true;
            c.Start();
            Thread d = new Thread(g =>
            {
                try
                {
                    while (true)
                    {
                        if (status3)
                        {
                            status4 = false;
                            GetExaminestatus();
                            Thread.Sleep(1000 * 60 * 5);
                        }
                        else
                        {
                            Thread.Sleep(1000 * 60);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //线程里面的代码报错了，，，就会到这来，，，但是你也不确定线程里面代码会不会报错
                    //此处，睡眠30秒，，，最好这里面写个日志记录下来。。
                    Dos.Common.LogHelper.Error("超时预警:" + ex.Message, "系统日志");
                    Thread.Sleep(1000 * 30);
                }
            });
            d.IsBackground = true;
            d.Start();
        }

        #region 超时预警新版
        public void GetEarlyInfo()
        {
            try
            {
                List<TbFormEarlyWarningNodeInfo> myMsgList = new List<TbFormEarlyWarningNodeInfo>();//Pc端推送
                List<TbSMSAlert> myDxList = new List<TbSMSAlert>();//App端
                string sql = @"select TbEarlyInfo.*,DATEDIFF(MINUTE,TbEarlyInfo.EarlyBegTiem,GETDATE()) as SjJgMinute,cast(DATEDIFF(MINUTE,TbEarlyInfo.EarlyBegTiem,GETDATE())*1.0/60/24 as decimal(18,2)) as SjJgDay,cast(DATEDIFF(MINUTE,TbEarlyInfo.EarlyBegTiem,GETDATE())*1.0/60 as decimal(18,2)) as SjJgHour,TbFormEarlyInfo.ID as FormEarlyInfoID from (select TbEarlySz.ID,TbEarlySz.EarlyWarningNewsCode,TbEarlySz.EarlyWarningContent,TbEarlySz.MenuCode,TbEarlySz.EarlyWarningNewsName,TbEarlySz.App,TbEarlySz.Pc,TbEarlySz.IsStart,TbEarlySz.EarlyWarningFrequency,TbEarlySz.IsBackstage,TbEarlySz.EarlyMenuCodeNode,TbEarlySz.EarlyWarningFrequencyType,TbEarlySz.OrgType,TbEarlySz.PersonnelSource,TbEarlySz.PersonnelCode,TbEarlySz.DeptId,TbEarlySz.RoleId,TbEarlyMenu.ID as EarlyDataId,TbEarlyMenu.EarlyCode,TbEarlyMenu.ProjectId,TbEarlyMenu.EarlyTiem,case when TbEarlySz.EarlyWarningFrequencyType='天' then DATEADD(DAY,TbEarlySz.EarlyWarningFrequency,DATEADD(DAY,TbEarlySz.TriggerTimeDay,DATEADD(HOUR,TbEarlySz.TriggerTimeHour,DATEADD(MINUTE,TbEarlySz.TriggerTimeMinute,TbEarlyMenu.EarlyTiem)))) when TbEarlySz.EarlyWarningFrequencyType='时' then DATEADD(HOUR,TbEarlySz.EarlyWarningFrequency,DATEADD(DAY,TbEarlySz.TriggerTimeDay,DATEADD(HOUR,TbEarlySz.TriggerTimeHour,DATEADD(MINUTE,TbEarlySz.TriggerTimeMinute,TbEarlyMenu.EarlyTiem))))else DATEADD(MINUTE,TbEarlySz.EarlyWarningFrequency,DATEADD(DAY,TbEarlySz.TriggerTimeDay,DATEADD(HOUR,TbEarlySz.TriggerTimeHour,DATEADD(MINUTE,TbEarlySz.TriggerTimeMinute,TbEarlyMenu.EarlyTiem)))) end as EarlyBegTiem,TbEarlyMenu.SiteCode,TbEarlyMenu.SiteName,TbEarlyMenu.WorkAreaCode,TbEarlyMenu.WorkAreaName,TbEarlyMenu.BranchCode,TbEarlyMenu.BranchName,TbEarlyMenu.ManagerDepartmentCode,TbEarlyMenu.ProcessFactoryCode,TbEarlyMenu.TypeCode from (select a.*,b.OrgType,b.ProjectId,b.PersonnelSource,b.PersonnelCode,b.DeptId,b.RoleId from TbEarlyWarningSetUp a
                           left join TbNoticeNewsOrg b on a.EarlyWarningNewsCode=b.NoticeNewsCode
                           where a.IsStart=1 and a.EarlyMenuCodeNode=0 and b.NewsType=2) TbEarlySz
                           left join (
                           --供货超时预警
                           select 'SupplyList' as MenuCode,a.ID,a.BatchPlanNum as EarlyCode,f.InsertTime as EarlyTiem,a.ProjectId,'' as SiteCode,'' as SiteName,a.WorkAreaCode,b.CompanyFullName as WorkAreaName,a.BranchCode,c.CompanyFullName as BranchName,c.ParentCompanyCode as ManagerDepartmentCode,a.ProcessFactoryCode,'' as TypeCode,'' as SiteContacts,'' as SiteContactTel from TbSupplyList a 
                           left join TbCompany b on a.WorkAreaCode=b.CompanyCode
                           left join TbCompany c on a.BranchCode=c.CompanyCode
                           left join TbFactoryBatchNeedPlan f on a.BatchPlanNum=f.BatchPlanNum
                           where a.StateCode='未供货'
                           union all
                           --取样订单质检报告上传超时
                           select * from (
                           select 'SampleOrder1' as MenuCode,a.ID,a.SampleOrderCode,a.ProcessingStateTime,a.ProjectId,'' as SiteCode,'' as SiteName,a.WorkAreaCode,b.CompanyFullName as WorkAreaName,c.CompanyCode as BranchCode,c.CompanyFullName as BranchName,c.ParentCompanyCode as ManagerDepartmentCode,a.ProcessFactoryCode,'' as TypeCode,'' as SiteContacts,'' as SiteContactTel from TbSampleOrder a
                           left join TbCompany b on a.WorkAreaCode=b.CompanyCode
                           left join TbCompany c on b.ParentCompanyCode=c.CompanyCode
                           where a.IsUpLoad=0 and  a.RebarType='BuildingSteel'
                           union all
                           select 'SampleOrder3' as MenuCode,a.ID,a.SampleOrderCode,a.ProcessingStateTime,a.ProjectId,'' as SiteCode,'' as SiteName,a.WorkAreaCode,b.CompanyFullName as WorkAreaName,c.CompanyCode as BranchCode,c.CompanyFullName as BranchName,c.ParentCompanyCode as ManagerDepartmentCode,a.ProcessFactoryCode,a.RebarType as TypeCode,a.InsertUserCode as SiteContacts,'' as SiteContactTel from TbSampleOrder a
                           left join TbCompany b on a.WorkAreaCode=b.CompanyCode
                           left join TbCompany c on b.ParentCompanyCode=c.CompanyCode
                           where a.IsUpLoad=0 and a.RebarType='SectionSteel') TbSo
                           union all
                           --等待卸货超时预警
                           select 'TransportProcess' as MenuCode,a.ID,a.DistributionCode,a.EnterSpaceTime,a.ProjectId,a.SiteCode,b.CompanyFullName as SiteName,c.CompanyCode as WorkAreaCode,c.CompanyFullName as WorkAreaName,d.CompanyCode as BranchCode,d.CompanyFullName as BranchName,d.ParentCompanyCode as ManagerDepartmentCode,e.ProcessFactoryCode,a.TypeCode,o.SiteContacts,o.SiteContactTel from TbTransportCarReport a
                           left join TbCompany b on a.SiteCode=b.CompanyCode
                           left join TbCompany c on b.ParentCompanyCode=c.CompanyCode
                           left join TbCompany d on c.ParentCompanyCode=d.CompanyCode
                           left join TbDistributionEntOrder o on a.DisEntOrderId=o.ID
                           left join TbDistributionEnt e on a.DistributionCode=e.DistributionCode
                           where a.FlowState=2  
                           union all
                           --半成品签收超时预警
                           select 'SemiFinishedSign' as MenuCode,a.ID,a.SigninNuber,a.InsertTime,a.ProjectId,a.SiteCode,b.CompanyFullName as SiteName,c.CompanyCode as WorkAreaCode,c.CompanyFullName as WorkAreaName,d.CompanyCode as BranchCode,d.CompanyFullName as BranchName,d.ParentCompanyCode as ManagerDepartmentCode,a.ProcessFactoryCode,a.TypeCode,'' as SiteContacts,'' as SiteContactTel from TbSemiFinishedSign a
                           left join TbCompany b on a.SiteCode=b.CompanyCode
                           left join TbCompany c on b.ParentCompanyCode=c.CompanyCode
                           left join TbCompany d on c.ParentCompanyCode=d.CompanyCode
                           where a.OperateState='未签收') TbEarlyMenu on TbEarlySz.MenuCode=TbEarlyMenu.MenuCode and TbEarlySz.ProjectId=TbEarlyMenu.ProjectId) TbEarlyInfo
                           left join TbFormEarlyWarningNodeInfo TbFormEarlyInfo on TbEarlyInfo.MenuCode=TbFormEarlyInfo.MenuCode 
                           and TbEarlyInfo.EarlyWarningNewsCode=TbFormEarlyInfo.EarlyWarningCode 
                           and TbEarlyInfo.EarlyDataId=TbFormEarlyInfo.EWFormDataCode
                           where TbEarlyInfo.EarlyDataId is not null and GETDATE()>TbEarlyInfo.EarlyBegTiem and TbFormEarlyInfo.ID is null and TbEarlyInfo.EarlyBegTiem>'2020-04-01'";
                DataTable dt = Db.Context.FromSql(sql).ToDataTable();
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        List<CensusdemoTask.NotiecUser> nuList = new List<CensusdemoTask.NotiecUser>();
                        if (dt.Rows[i]["PersonnelSource"].ToString() == "Role")//选择角色
                        {
                            string OrgId = "";
                            switch (dt.Rows[i]["OrgType"].ToString())
                            {
                                case "1":
                                    OrgId = dt.Rows[i]["ProcessFactoryCode"].ToString();
                                    break;
                                case "2":
                                    OrgId = dt.Rows[i]["ManagerDepartmentCode"].ToString();
                                    break;
                                case "3":
                                    OrgId = dt.Rows[i]["BranchCode"].ToString();
                                    break;
                                case "4":
                                    OrgId = dt.Rows[i]["WorkAreaCode"].ToString();
                                    break;
                                default:
                                    break;
                            }
                            string sqlUser = "select a.UserCode from TbUserRole a where 1=1 and a.Flag=0 and a.DeptId='" + dt.Rows[i]["DeptId"] + "' and a.RoleCode='" + dt.Rows[i]["RoleId"] + "' and a.ProjectId='" + dt.Rows[i]["ProjectId"] + "' and a.OrgType=" + dt.Rows[i]["OrgType"] + " and a.OrgId='" + OrgId + "' ";
                            DataTable dtUser = Db.Context.FromSql(sqlUser).ToDataTable();
                            if (dtUser.Rows.Count > 0)
                            {
                                for (int u = 0; u < dtUser.Rows.Count; u++)
                                {
                                    CensusdemoTask.NotiecUser nuModel = new CensusdemoTask.NotiecUser();
                                    nuModel.PersonnelSource = "Personnel";
                                    nuModel.PersonnelCode = dtUser.Rows[u]["UserCode"].ToString();
                                    nuList.Add(nuModel);
                                }
                            }
                        }
                        else//直接选择用户
                        {
                            CensusdemoTask.NotiecUser nuModel = new CensusdemoTask.NotiecUser();
                            nuModel.PersonnelSource = dt.Rows[i]["PersonnelSource"].ToString();
                            nuModel.PersonnelCode = dt.Rows[i]["PersonnelCode"].ToString();
                            nuList.Add(nuModel);
                        }

                        #region 供货超时预警

                        if (dt.Rows[i]["MenuCode"].ToString() == "SupplyList")
                        {
                            //查找消息模板信息
                            var shortMessageTemplateModel = Repository<TbShortMessageTemplate>.First(p => p.TemplateCode == "MB0011");
                            if (shortMessageTemplateModel != null)
                            {
                                string content = shortMessageTemplateModel.TemplateContent;
                                var s = content.Replace("变量：分部/工区", dt.Rows[i]["BranchName"].ToString() + "/" + dt.Rows[i]["WorkAreaName"].ToString());
                                var a = s.Replace("变量：批次计划编号", dt.Rows[i]["EarlyCode"].ToString());
                                var ShortContent = a.Replace("变量：时间（天）", dt.Rows[i]["SjJgDay"] + "（天）");
                                for (int u = 0; u < nuList.Count; u++)
                                {
                                    if (dt.Rows[i]["App"].ToString() == "1")
                                    {
                                        //调用BIM获取人员电话或者身份证号码的的接口
                                        string userInfo = ct.up(nuList[u].PersonnelCode);
                                        var jObject = Newtonsoft.Json.Linq.JObject.Parse(userInfo);
                                        string tel = jObject["data"][0]["MobilePhone"].ToString();
                                        if (!string.IsNullOrWhiteSpace(tel))
                                        {
                                            var myDxMsg = new TbSMSAlert()
                                            {
                                                InsertTime = DateTime.Now,
                                                ManagerDepartment = dt.Rows[i]["ManagerDepartmentCode"].ToString(),
                                                Branch = dt.Rows[i]["BranchCode"].ToString(),
                                                WorkArea = dt.Rows[i]["WorkAreaCode"].ToString(),
                                                Site = dt.Rows[i]["SiteCode"].ToString(),
                                                UserCode = nuList[u].PersonnelCode,
                                                UserTel = tel,
                                                DXType = "",
                                                BusinessCode = shortMessageTemplateModel.TemplateCode,
                                                DataCode = dt.Rows[i]["EarlyCode"].ToString(),
                                                ShortContent = ShortContent,
                                                FromCode = dt.Rows[i]["MenuCode"].ToString(),
                                                MsgType = "2"

                                            };
                                            myDxList.Add(myDxMsg);
                                        }
                                    }
                                    if (dt.Rows[i]["Pc"].ToString() == "1")
                                    {
                                        var myFormEarlyMsg = new TbFormEarlyWarningNodeInfo()
                                        {

                                            MenuCode = dt.Rows[i]["MenuCode"].ToString(),
                                            EWNodeCode = Convert.ToInt32(dt.Rows[i]["ID"]),
                                            EWUserCode = nuList[u].PersonnelCode,
                                            ProjectId = dt.Rows[i]["ProjectId"].ToString(),
                                            EarlyWarningCode = dt.Rows[i]["EarlyWarningNewsCode"].ToString(),
                                            EWFormDataCode = Convert.ToInt32(dt.Rows[i]["EarlyDataId"]),
                                            CompanyCode = dt.Rows[i]["BranchCode"].ToString(),
                                            WorkArea = dt.Rows[i]["WorkAreaCode"].ToString(),
                                            SiteCode = dt.Rows[i]["SiteCode"].ToString(),
                                            MsgType = "2",
                                            EWContent = ShortContent,
                                            EWStart = 0,
                                            EWTime = DateTime.Now,
                                            ProcessFactoryCode = dt.Rows[i]["ProcessFactoryCode"].ToString(),
                                            DataCode = dt.Rows[i]["EarlyCode"].ToString(),
                                            EarlyTitle = "【" + dt.Rows[i]["EarlyCode"].ToString() + "】" + dt.Rows[i]["EarlyWarningNewsName"].ToString()
                                        };
                                        myMsgList.Add(myFormEarlyMsg);
                                    }
                                }
                            }
                        }

                        #endregion

                        #region 取样订单质检报告上传超时

                        else if (dt.Rows[i]["MenuCode"].ToString().StartsWith("SampleOrder"))
                        {
                            //查找消息模板信息
                            var shortMessageTemplateModel = Repository<TbShortMessageTemplate>.First(p => p.TemplateCode == "MB0015");
                            if (shortMessageTemplateModel != null)
                            {
                                //短信内容
                                string content = shortMessageTemplateModel.TemplateContent;
                                var s = content.Replace("变量：分部/工区", dt.Rows[i]["BranchName"].ToString() + "/" + dt.Rows[i]["WorkAreaName"].ToString());
                                var ShortContent = s.Replace("变量：取样单号", dt.Rows[i]["EarlyCode"].ToString());
                                //判断是型钢还是建筑钢筋
                                if (dt.Rows[i]["TypeCode"].ToString() == "SectionSteel")
                                {
                                    //调用BIM获取人员电话或者身份证号码的的接口
                                    string UserId = ct.GetUserId(dt.Rows[i]["SiteContacts"].ToString()).Rows[0]["UserId"].ToString();
                                    if (!string.IsNullOrWhiteSpace(UserId))
                                    {
                                        CensusdemoTask.NotiecUser nuModel = new CensusdemoTask.NotiecUser();
                                        nuModel.PersonnelSource = "Personnel";
                                        nuModel.PersonnelCode =UserId;
                                        nuList.Add(nuModel);
                                    }
                                }
                                for (int u = 0; u < nuList.Count; u++)
                                {
                                    if (dt.Rows[i]["App"].ToString() == "1")
                                    {
                                        //调用BIM获取人员电话或者身份证号码的的接口
                                        string userInfo = ct.up(nuList[u].PersonnelCode);
                                        var jObject = Newtonsoft.Json.Linq.JObject.Parse(userInfo);
                                        string tel = jObject["data"][0]["MobilePhone"].ToString();
                                        if (!string.IsNullOrWhiteSpace(tel))
                                        {
                                            var myDxMsg = new TbSMSAlert()
                                            {
                                                InsertTime = DateTime.Now,
                                                ManagerDepartment = dt.Rows[i]["ManagerDepartmentCode"].ToString(),
                                                Branch = dt.Rows[i]["BranchCode"].ToString(),
                                                WorkArea = dt.Rows[i]["WorkAreaCode"].ToString(),
                                                Site = dt.Rows[i]["SiteCode"].ToString(),
                                                UserCode = nuList[u].PersonnelCode,
                                                UserTel = tel,
                                                DXType = "",
                                                BusinessCode = shortMessageTemplateModel.TemplateCode,
                                                DataCode = dt.Rows[i]["EarlyCode"].ToString(),
                                                ShortContent = ShortContent,
                                                FromCode = dt.Rows[i]["MenuCode"].ToString(),
                                                MsgType = "2"

                                            };
                                            myDxList.Add(myDxMsg);
                                        }
                                    }
                                    if (dt.Rows[i]["Pc"].ToString() == "1")
                                    {
                                        var myFormEarlyMsg = new TbFormEarlyWarningNodeInfo()
                                        {

                                            MenuCode = dt.Rows[i]["MenuCode"].ToString(),
                                            EWNodeCode = Convert.ToInt32(dt.Rows[i]["ID"]),
                                            EWUserCode = nuList[u].PersonnelCode,
                                            ProjectId = dt.Rows[i]["ProjectId"].ToString(),
                                            EarlyWarningCode = dt.Rows[i]["EarlyWarningNewsCode"].ToString(),
                                            EWFormDataCode = Convert.ToInt32(dt.Rows[i]["EarlyDataId"]),
                                            CompanyCode = dt.Rows[i]["BranchCode"].ToString(),
                                            WorkArea = dt.Rows[i]["WorkAreaCode"].ToString(),
                                            SiteCode = dt.Rows[i]["SiteCode"].ToString(),
                                            MsgType = "2",
                                            EWContent = ShortContent,
                                            EWStart = 0,
                                            EWTime = DateTime.Now,
                                            ProcessFactoryCode = dt.Rows[i]["ProcessFactoryCode"].ToString(),
                                            DataCode = dt.Rows[i]["EarlyCode"].ToString(),
                                            EarlyTitle = "【" + dt.Rows[i]["EarlyCode"].ToString() + "】" + dt.Rows[i]["EarlyWarningNewsName"].ToString()
                                        };
                                        myMsgList.Add(myFormEarlyMsg);
                                    }
                                }
                            }
                        }
                        #endregion

                        #region 等待卸货超时预警

                        else if (dt.Rows[i]["MenuCode"].ToString() == "TransportProcess")
                        {
                            //查找消息模板信息
                            var shortMessageTemplateModel = Repository<TbShortMessageTemplate>.First(p => p.TemplateCode == "MB0006");
                            if (shortMessageTemplateModel != null)
                            {
                                //短信内容
                                string content = shortMessageTemplateModel.TemplateContent;
                                var s = content.Replace("变量：站点", dt.Rows[i]["SiteName"].ToString());
                                var a = s.Replace("变量：类型编码", dt.Rows[i]["TypeCode"].ToString());
                                var b = a.Replace("变量：时间（年/月/日/时/分）", Convert.ToDateTime(dt.Rows[i]["EarlyTiem"]).ToString("g"));
                                var ShortContent = b.Replace("变量：时间（小时）", dt.Rows[0]["SjJgHour"].ToString() + "（小时）");
                                if (dt.Rows[i]["IsBackstage"].ToString() == "1")//后台选人
                                {
                                    //获取用户userId
                                    string UserId = ct.GetUserId(dt.Rows[i]["SiteContacts"].ToString()).Rows[0]["UserId"].ToString();
                                    CensusdemoTask.NotiecUser model = new CensusdemoTask.NotiecUser();
                                    model.PersonnelSource = "Personnel";
                                    model.PersonnelCode = UserId;
                                    nuList.Add(model);
                                }
                                for (int u = 0; u < nuList.Count; u++)
                                {
                                    if (dt.Rows[i]["App"].ToString() == "1")
                                    {
                                        //调用BIM获取人员电话或者身份证号码的的接口
                                        string userInfo = ct.up(nuList[u].PersonnelCode);
                                        var jObject = Newtonsoft.Json.Linq.JObject.Parse(userInfo);
                                        string tel = jObject["data"][0]["MobilePhone"].ToString();
                                        if (!string.IsNullOrWhiteSpace(tel))
                                        {
                                            var myDxMsg = new TbSMSAlert()
                                            {
                                                InsertTime = DateTime.Now,
                                                ManagerDepartment = dt.Rows[i]["ManagerDepartmentCode"].ToString(),
                                                Branch = dt.Rows[i]["BranchCode"].ToString(),
                                                WorkArea = dt.Rows[i]["WorkAreaCode"].ToString(),
                                                Site = dt.Rows[i]["SiteCode"].ToString(),
                                                UserCode = nuList[u].PersonnelCode,
                                                UserTel = tel,
                                                DXType = "",
                                                BusinessCode = shortMessageTemplateModel.TemplateCode,
                                                DataCode = dt.Rows[i]["EarlyCode"].ToString(),
                                                ShortContent = ShortContent,
                                                FromCode = dt.Rows[i]["MenuCode"].ToString(),
                                                MsgType = "2"

                                            };
                                            myDxList.Add(myDxMsg);
                                        }
                                    }
                                    if (dt.Rows[i]["Pc"].ToString() == "1")
                                    {
                                        var myFormEarlyMsg = new TbFormEarlyWarningNodeInfo()
                                        {

                                            MenuCode = dt.Rows[i]["MenuCode"].ToString(),
                                            EWNodeCode = Convert.ToInt32(dt.Rows[i]["ID"]),
                                            EWUserCode = nuList[u].PersonnelCode,
                                            ProjectId = dt.Rows[i]["ProjectId"].ToString(),
                                            EarlyWarningCode = dt.Rows[i]["EarlyWarningNewsCode"].ToString(),
                                            EWFormDataCode = Convert.ToInt32(dt.Rows[i]["EarlyDataId"]),
                                            CompanyCode = dt.Rows[i]["BranchCode"].ToString(),
                                            WorkArea = dt.Rows[i]["WorkAreaCode"].ToString(),
                                            SiteCode = dt.Rows[i]["SiteCode"].ToString(),
                                            MsgType = "2",
                                            EWContent = ShortContent,
                                            EWStart = 0,
                                            EWTime = DateTime.Now,
                                            ProcessFactoryCode = dt.Rows[i]["ProcessFactoryCode"].ToString(),
                                            DataCode = dt.Rows[i]["EarlyCode"].ToString(),
                                            EarlyTitle = "【" + dt.Rows[i]["EarlyCode"].ToString() + "】" + dt.Rows[i]["EarlyWarningNewsName"].ToString()
                                        };
                                        myMsgList.Add(myFormEarlyMsg);
                                    }
                                }
                            }
                        }
                        #endregion

                        #region 半成品签收超时预警

                        else if (dt.Rows[i]["MenuCode"].ToString() == "SemiFinishedSign")
                        {
                            //查找消息模板信息
                            var shortMessageTemplateModel = Repository<TbShortMessageTemplate>.First(p => p.TemplateCode == "MB0008");
                            if (shortMessageTemplateModel != null)
                            {
                                //短信内容
                                string content = shortMessageTemplateModel.TemplateContent;
                                var s = content.Replace("变量：站点", dt.Rows[i]["SiteName"].ToString());
                                var a = s.Replace("变量：类型编码", dt.Rows[i]["TypeCode"].ToString());
                                var b = a.Replace("变量：时间（年/月/日/时/分）", Convert.ToDateTime(dt.Rows[i]["EarlyTiem"]).ToString("g"));
                                var ShortContent = b.Replace("变量：时间（小时/天）", dt.Rows[i]["SjJgHour"].ToString() + "/" + dt.Rows[i]["SjJgDay"].ToString() + "（小时/天）");
                                for (int u = 0; u < nuList.Count; u++)
                                {
                                    if (dt.Rows[i]["App"].ToString() == "1")
                                    {
                                        //调用BIM获取人员电话或者身份证号码的的接口
                                        string userInfo = ct.up(nuList[u].PersonnelCode);
                                        var jObject = Newtonsoft.Json.Linq.JObject.Parse(userInfo);
                                        string tel = jObject["data"][0]["MobilePhone"].ToString();
                                        if (!string.IsNullOrWhiteSpace(tel))
                                        {
                                            var myDxMsg = new TbSMSAlert()
                                            {
                                                InsertTime = DateTime.Now,
                                                ManagerDepartment = dt.Rows[i]["ManagerDepartmentCode"].ToString(),
                                                Branch = dt.Rows[i]["BranchCode"].ToString(),
                                                WorkArea = dt.Rows[i]["WorkAreaCode"].ToString(),
                                                Site = dt.Rows[i]["SiteCode"].ToString(),
                                                UserCode = nuList[u].PersonnelCode,
                                                UserTel = tel,
                                                DXType = "",
                                                BusinessCode = shortMessageTemplateModel.TemplateCode,
                                                DataCode = dt.Rows[i]["EarlyCode"].ToString(),
                                                ShortContent = ShortContent,
                                                FromCode = dt.Rows[i]["MenuCode"].ToString(),
                                                MsgType = "2"

                                            };
                                            myDxList.Add(myDxMsg);
                                        }
                                    }
                                    if (dt.Rows[i]["Pc"].ToString() == "1")
                                    {
                                        var myFormEarlyMsg = new TbFormEarlyWarningNodeInfo()
                                        {

                                            MenuCode = dt.Rows[i]["MenuCode"].ToString(),
                                            EWNodeCode = Convert.ToInt32(dt.Rows[i]["ID"]),
                                            EWUserCode = nuList[u].PersonnelCode,
                                            ProjectId = dt.Rows[i]["ProjectId"].ToString(),
                                            EarlyWarningCode = dt.Rows[i]["EarlyWarningNewsCode"].ToString(),
                                            EWFormDataCode = Convert.ToInt32(dt.Rows[i]["EarlyDataId"]),
                                            CompanyCode = dt.Rows[i]["BranchCode"].ToString(),
                                            WorkArea = dt.Rows[i]["WorkAreaCode"].ToString(),
                                            SiteCode = dt.Rows[i]["SiteCode"].ToString(),
                                            MsgType = "2",
                                            EWContent = ShortContent,
                                            EWStart = 0,
                                            EWTime = DateTime.Now,
                                            ProcessFactoryCode = dt.Rows[i]["ProcessFactoryCode"].ToString(),
                                            DataCode = dt.Rows[i]["EarlyCode"].ToString(),
                                            EarlyTitle = "【" + dt.Rows[i]["EarlyCode"].ToString() + "】" + dt.Rows[i]["EarlyWarningNewsName"].ToString()
                                        };
                                        myMsgList.Add(myFormEarlyMsg);
                                    }
                                }
                            }
                        }

                        #endregion
                    }
                    //调用短信接口发送短信
                    for (int m = 0; m < myDxList.Count; m++)
                    {
                        //var dx = ct.ShortMessagePC(myDxList[m].UserTel, myDxList[m].ShortContent);
                        var dx = ct.ShortMessagePC("15756321745", myDxList[m].ShortContent);
                        var jObject = Newtonsoft.Json.Linq.JObject.Parse(dx);
                        var logmsg = jObject["data"][0]["code"].ToString();
                        myDxList[m].DXType = logmsg;
                    }
                }
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    if (myMsgList.Any())
                    {
                        //添加表单预警信息
                        Repository<PM.DataEntity.TbFormEarlyWarningNodeInfo>.Insert(trans, myMsgList, true);
                    }

                    if (myDxList.Any())
                    {
                        //向短信信息表中插入数据
                        Repository<TbSMSAlert>.Insert(trans, myDxList, true);
                    }
                    //调用手机端消息推送方法
                    trans.Commit();//提交事务
                }
                status1 = true;
            }
            catch (Exception)
            {

                throw;
            }

        }
        public void GetRamDmPlanEarlyInfo()
        {
            try
            {
                List<TbFormEarlyWarningNodeInfo> myMsgList = new List<TbFormEarlyWarningNodeInfo>();//Pc端推送
                List<TbSMSAlert> myDxList = new List<TbSMSAlert>();//App端

                string sql = @"select * from (select TbEarly.*,case when TbEarly.EarlyWarningFrequencyType='天' then DATEADD(DAY,TbEarly.EarlyWarningFrequency,DATEADD(DAY,TbEarly.TriggerTimeDay,DATEADD(HOUR,TbEarly.TriggerTimeHour,DATEADD(MINUTE,TbEarly.TriggerTimeMinute,TbEarly.EarlyTiem)))) when TbEarly.EarlyWarningFrequencyType='时' then DATEADD(HOUR,TbEarly.EarlyWarningFrequency,DATEADD(DAY,TbEarly.TriggerTimeDay,DATEADD(HOUR,TbEarly.TriggerTimeHour,DATEADD(MINUTE,TbEarly.TriggerTimeMinute,TbEarly.EarlyTiem))))else DATEADD(MINUTE,TbEarly.EarlyWarningFrequency,DATEADD(DAY,TbEarly.TriggerTimeDay,DATEADD(HOUR,TbEarly.TriggerTimeHour,DATEADD(MINUTE,TbEarly.TriggerTimeMinute,TbEarly.EarlyTiem)))) end as EarlyBegTiem,TbFormEarlyInfo.ID as FormEarlyID from (select TbEarlySz.ID,TbEarlySz.EarlyWarningNewsCode,TbEarlySz.EarlyWarningContent,TbEarlySz.MenuCode,TbEarlySz.EarlyWarningNewsName,TbEarlySz.App,TbEarlySz.Pc,TbEarlySz.IsStart,TbEarlySz.TriggerTimeDay,TbEarlySz.TriggerTimeHour,TbEarlySz.TriggerTimeMinute,TbEarlySz.EarlyWarningFrequency,TbEarlySz.IsBackstage,TbEarlySz.EarlyMenuCodeNode,TbEarlySz.EarlyWarningFrequencyType,TbEarlySz.OrgType,TbEarlySz.PersonnelSource,TbEarlySz.PersonnelCode,TbEarlySz.DeptId,TbEarlySz.RoleId,convert(varchar(7),dateadd(MONTH,1,getdate()),120) as EarlyDataId,convert(varchar(7),dateadd(MONTH,1,getdate()),120) as EarlyCode,convert(varchar(20),dateadd(dd,-day(getdate()),getdate()),23) as EarlyTiem,'' as SiteCode,'' as SiteName,TbEarlyInfo.WorkAreaCode,TbEarlyInfo.WorkAreaName,TbEarlyInfo.BranchCode,TbEarlyInfo.BranchName,TbEarlyInfo.ManagerDepartmentCode as ManagerDepartmentCode,TbEarlyInfo.ProcessFactoryCode,'' TypeCode,TbEarlyInfo.ProjectId from (select ew.*,b.OrgType,b.ProjectId,b.PersonnelSource,b.PersonnelCode,b.DeptId,b.RoleId from TbEarlyWarningSetUp ew
left join TbNoticeNewsOrg b on ew.EarlyWarningNewsCode=b.NoticeNewsCode
where ew.MenuCode='RawMonthDemandPlan' and ew.IsStart=1 and b.NewsType=2) TbEarlySz
left join(select 'RawMonthDemandPlan' as MenuCode,TbGq.* from (select cp2.CompanyCode as WorkAreaCode,cp2.CompanyFullName as WorkAreaName,cp3.CompanyCode as BranchCode,cp3.CompanyFullName as BranchName,cp3.ParentCompanyCode as ManagerDepartmentCode,wo.ProcessFactoryCode,wo.ProjectId  from  TbWorkOrder wo
left join TbCompany cp1 on wo.SiteCode=cp1.CompanyCode
left join TbCompany cp2 on cp2.CompanyCode=cp1.ParentCompanyCode
left join TbCompany cp3 on cp3.CompanyCode=cp2.ParentCompanyCode
where wo.Examinestatus!='已退回' and wo.Examinestatus!='已撤销'
and (CONVERT(varchar(7) ,wo.InsertTime, 120)=convert(varchar(7),dateadd(MONTH,-2,getdate()),120) or CONVERT(varchar(7) ,wo.InsertTime, 120)=convert(varchar(7),dateadd(MONTH,-1,getdate()),120) or CONVERT(varchar(7) ,wo.InsertTime, 120)=convert(varchar(7),getdate(),120))
group by cp2.CompanyCode,cp2.CompanyFullName,cp3.CompanyCode,cp3.CompanyFullName,cp3.ParentCompanyCode,wo.ProcessFactoryCode,wo.ProjectId
union all
select rmp.WorkAreaCode,cp1.CompanyFullName as WorkAreaName,rmp.BranchCode,cp2.CompanyFullName as BranchName,cp2.ParentCompanyCode as ManagerDepartmentCode,rmp.ProcessFactoryCode,rmp.ProjectId from TbRawMaterialMonthDemandPlan rmp
left join TbCompany cp1 on rmp.WorkAreaCode=cp1.CompanyCode
left join TbCompany cp2 on rmp.BranchCode=cp2.CompanyCode
 where rmp.Examinestatus!='已退回' and rmp.Examinestatus!='已撤销'
and (CONVERT(varchar(7) ,rmp.InsertTime, 120)=convert(varchar(7),dateadd(MONTH,-2,getdate()),120) or CONVERT(varchar(7) ,rmp.InsertTime, 120)=convert(varchar(7),dateadd(MONTH,-1,getdate()),120) or CONVERT(varchar(7) ,rmp.InsertTime, 120)=convert(varchar(7),getdate(),120))) TbGq 
where TbGq.WorkAreaCode not in(select WorkAreaCode from TbRawMaterialMonthDemandPlan rmp where rmp.DemandMonth='2020-05' and rmp.ProjectId=TbGq.ProjectId and rmp.ProcessFactoryCode=TbGq.ProcessFactoryCode)
group by TbGq.ManagerDepartmentCode,TbGq.BranchCode,TbGq.BranchName,TbGq.WorkAreaCode,TbGq.WorkAreaName,TbGq.ProcessFactoryCode,TbGq.ProjectId) TbEarlyInfo on TbEarlySz.MenuCode=TbEarlyInfo.MenuCode and TbEarlySz.ProjectId=TbEarlyInfo.ProjectId) TbEarly
left join TbFormEarlyWarningNodeInfo TbFormEarlyInfo on TbEarly.MenuCode=TbFormEarlyInfo.MenuCode 
and TbEarly.EarlyWarningNewsCode=TbFormEarlyInfo.EarlyWarningCode 
and TbEarly.EarlyDataId=TbFormEarlyInfo.EWFormDataCode
and TbEarly.ProcessFactoryCode=TbFormEarlyInfo.ProcessFactoryCode) as TbEarlyInfo
where  GETDATE()>TbEarlyInfo.EarlyBegTiem and TbEarlyInfo.FormEarlyID is null   ";
                DataTable dt = Db.Context.FromSql(sql).ToDataTable();
                if (dt.Rows.Count > 0)
                {
                    //查找消息模板信息
                    var shortMessageTemplateModel = Repository<TbShortMessageTemplate>.First(p => p.TemplateCode == "MB0011");
                    if (shortMessageTemplateModel != null)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            List<CensusdemoTask.NotiecUser> nuList = new List<CensusdemoTask.NotiecUser>();
                            if (dt.Rows[i]["PersonnelSource"].ToString() == "Role")//选择角色
                            {
                                string OrgId = "";
                                switch (dt.Rows[i]["OrgType"].ToString())
                                {
                                    case "1":
                                        OrgId = dt.Rows[i]["ProcessFactoryCode"].ToString();
                                        break;
                                    case "2":
                                        OrgId = dt.Rows[i]["ManagerDepartmentCode"].ToString();
                                        break;
                                    case "3":
                                        OrgId = dt.Rows[i]["BranchCode"].ToString();
                                        break;
                                    case "4":
                                        OrgId = dt.Rows[i]["WorkAreaCode"].ToString();
                                        break;
                                    default:
                                        break;
                                }
                                string sqlUser = "select a.UserCode from TbUserRole a where 1=1 and a.Flag=0 and a.DeptId='" + dt.Rows[i]["DeptId"] + "' and a.RoleCode='" + dt.Rows[i]["RoleId"] + "' and a.ProjectId='" + dt.Rows[i]["ProjectId"] + "' and a.OrgType=" + dt.Rows[i]["OrgType"] + " and a.OrgId='" + OrgId + "' ";
                                DataTable dtUser = Db.Context.FromSql(sqlUser).ToDataTable();
                                if (dtUser.Rows.Count > 0)
                                {
                                    for (int u = 0; u < dtUser.Rows.Count; u++)
                                    {
                                        CensusdemoTask.NotiecUser nuModel = new CensusdemoTask.NotiecUser();
                                        nuModel.PersonnelSource = "Personnel";
                                        nuModel.PersonnelCode = dtUser.Rows[u]["UserCode"].ToString();
                                        nuList.Add(nuModel);
                                    }
                                }
                            }
                            else//直接选择用户
                            {
                                CensusdemoTask.NotiecUser nuModel = new CensusdemoTask.NotiecUser();
                                nuModel.PersonnelSource = dt.Rows[i]["PersonnelSource"].ToString();
                                nuModel.PersonnelCode = dt.Rows[i]["PersonnelCode"].ToString();
                                nuList.Add(nuModel);
                            }
                            for (int u = 0; u < nuList.Count; u++)
                            {
                                //短信内容                                          
                                string content = shortMessageTemplateModel.TemplateContent;
                                var ShortContent = content.Replace("变量：分部/工区", dt.Rows[i]["BranchName"] + "/" + dt.Rows[i]["WorkAreaName"]);
                                if (dt.Rows[i]["App"].ToString() == "1")
                                {
                                    //调用BIM获取人员电话或者身份证号码的的接口
                                    string userInfo = ct.up(nuList[u].PersonnelCode);
                                    var jObject = Newtonsoft.Json.Linq.JObject.Parse(userInfo);
                                    string tel = jObject["data"][0]["MobilePhone"].ToString();
                                    if (!string.IsNullOrWhiteSpace(tel))
                                    {
                                        var myDxMsg = new TbSMSAlert()
                                        {
                                            InsertTime = DateTime.Now,
                                            ManagerDepartment = dt.Rows[i]["ManagerDepartmentCode"].ToString(),
                                            Branch = dt.Rows[i]["BranchCode"].ToString(),
                                            WorkArea = dt.Rows[i]["WorkAreaCode"].ToString(),
                                            Site = dt.Rows[i]["SiteCode"].ToString(),
                                            UserCode = nuList[u].PersonnelCode,
                                            UserTel = tel,
                                            DXType = "",
                                            BusinessCode = shortMessageTemplateModel.TemplateCode,
                                            DataCode = dt.Rows[i]["EarlyCode"].ToString(),
                                            ShortContent = ShortContent,
                                            FromCode = dt.Rows[i]["MenuCode"].ToString(),
                                            MsgType = "2"
                                        };
                                        myDxList.Add(myDxMsg);
                                    }
                                }
                                if (dt.Rows[i]["Pc"].ToString() == "1")
                                {
                                    var myFormEarlyMsg = new TbFormEarlyWarningNodeInfo()
                                    {
                                        MenuCode = dt.Rows[i]["MenuCode"].ToString(),
                                        EWNodeCode = Convert.ToInt32(dt.Rows[i]["ID"]),
                                        EWUserCode = nuList[u].PersonnelCode,
                                        ProjectId = dt.Rows[i]["ProjectId"].ToString(),
                                        EarlyWarningCode = dt.Rows[i]["EarlyWarningNewsCode"].ToString(),
                                        EWFormDataCode = Convert.ToInt32(dt.Rows[i]["EarlyDataId"]),
                                        CompanyCode = dt.Rows[i]["BranchCode"].ToString(),
                                        WorkArea = dt.Rows[i]["WorkAreaCode"].ToString(),
                                        SiteCode = dt.Rows[i]["SiteCode"].ToString(),
                                        ProcessFactoryCode = dt.Rows[i]["ProcessFactoryCode"].ToString(),
                                        MsgType = "2",
                                        EWContent = ShortContent,
                                        EWStart = 0,
                                        EWTime = DateTime.Now,
                                        DataCode = dt.Rows[i]["EarlyCode"].ToString(),
                                        EarlyTitle = "【" + dt.Rows[i]["EarlyCode"].ToString() + "】" + dt.Rows[i]["EarlyWarningNewsName"].ToString()
                                    };
                                    myMsgList.Add(myFormEarlyMsg);
                                }
                            }
                        }
                    }
                    //调用短信接口发送短信
                    for (int m = 0; m < myDxList.Count; m++)
                    {
                        //var dx = ct.ShortMessagePC(myDxList[m].UserTel, myDxList[m].ShortContent);
                        var dx = ct.ShortMessagePC("15756321745", myDxList[m].ShortContent);
                        var jObject = Newtonsoft.Json.Linq.JObject.Parse(dx);
                        var logmsg = jObject["data"][0]["code"].ToString();
                        myDxList[m].DXType = logmsg;
                    }
                }
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    if (myMsgList.Any())
                    {
                        //添加表单预警信息
                        Repository<PM.DataEntity.TbFormEarlyWarningNodeInfo>.Insert(trans, myMsgList, true);
                    }

                    if (myDxList.Any())
                    {
                        //向短信信息表中插入数据
                        Repository<TbSMSAlert>.Insert(trans, myDxList, true);
                    }
                    //调用手机端消息推送方法
                    trans.Commit();//提交事务
                }
                status2 = true;
            }
            catch (Exception)
            {

                throw;
            }

        }
        public void GetTransportShortMessageWarn()
        {
            try
            {
                List<TbSMSAlert> myDxList = new List<TbSMSAlert>();//App推送
                List<TbFormEarlyWarningNodeInfo> myMsgList = new List<TbFormEarlyWarningNodeInfo>();//PC端推送
                var NoticeModel = Repository<TbNoticeNewsSetUp>.First(p => p.NoticeNewsCode == "XXTZ0008" && p.IsStart == 1);
                if (NoticeModel != null)
                {
                    //查找消息模板信息
                    var shortMessageTemplateModel = Repository<TbShortMessageTemplate>.First(p => p.TemplateCode == "MB0017");
                    if (shortMessageTemplateModel != null)
                    {
                        string sql = @"select Tb1.*,cp.CompanyFullName as SiteName,cp.CompanyCode as WorkAreaCode,cp2.CompanyCode as BranchCode,cp2.ParentCompanyCode as ManagerDepartmentCode,u.UserId,de.ProcessFactoryCode,tsmw.ID from  (select Tb.*,case when Tb.FlowState=1 then DATEADD(HOUR,2,Tb.LeaveFactoryTime) when Tb.FlowState=2 then DATEADD(HOUR,2,Tb.EnterSpaceTime) when Tb.FlowState=3 then DATEADD(HOUR,2,Tb.StartDischargeTime) when Tb.FlowState=4 then DATEADD(HOUR,2,Tb.EndDischargeTime) end DxTxBegTime from (select trc.DistributionCode,trc.DisEntOrderId,trc.SiteCode,trc.TypeCode,trc.TypeName,trc.LeaveFactoryTime,trc.EnterSpaceTime,trc.StartDischargeTime,trc.EndDischargeTime,trc.OutSpaceTime,case when trc.EnterSpaceTime is null then 1 when trc.StartDischargeTime is null then 2 when trc.EndDischargeTime is null then 3 when trc.OutSpaceTime is null then 4 end as FlowState,disEnt.Driver from TbTransportCarReport trc
left join TbDistributionEnt disEnt on trc.DistributionCode=disEnt.DistributionCode
where State=0 and LeaveFactoryTime is not null) Tb ) Tb1 
left join TbCompany cp on Tb1.SiteCode=cp.CompanyCode
left join TbCompany cp1 on cp.ParentCompanyCode=cp1.CompanyCode
left join TbCompany cp2 on cp1.ParentCompanyCode=cp2.CompanyCode
left join TbUser u on Tb1.Driver=u.UserCode
left join TbDistributionEnt de on Tb1.DistributionCode=de.DistributionCode
left join TbFlowPerformMessage tsmw on Tb1.DistributionCode=tsmw.EWFormDataCode and Tb1.DisEntOrderId=tsmw.FlowPerformID  and Tb1.FlowState=tsmw.FlowNodeCode
where GETDATE()>Tb1.DxTxBegTime and tsmw.ID is null and Tb1.DxTxBegTime>'2020-04-01'";
                        DataTable dt = Db.Context.FromSql(sql).ToDataTable();
                        if (dt.Rows.Count > 0)
                        {
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                //短信、通知内容  
                                var content = shortMessageTemplateModel.TemplateContent;
                                var s = content.Replace("变量：配送编号", dt.Rows[i]["DistributionCode"].ToString());
                                var ShortContent = s.Replace("变量：站点", dt.Rows[i]["SiteName"].ToString());
                                if (NoticeModel.App == 1)
                                {
                                    //调用BIM获取人员电话或者身份证号码的的接口
                                    string userInfo = ct.up(dt.Rows[i]["UserId"].ToString());
                                    var jObject = Newtonsoft.Json.Linq.JObject.Parse(userInfo);
                                    string tel = jObject["data"][0]["MobilePhone"].ToString();
                                    if (!string.IsNullOrWhiteSpace(tel))
                                    {
                                        var myDxMsg = new TbSMSAlert()
                                        {
                                            InsertTime = DateTime.Now,
                                            ManagerDepartment = dt.Rows[i]["ManagerDepartmentCode"].ToString(),
                                            Branch = dt.Rows[i]["BranchCode"].ToString(),
                                            WorkArea = dt.Rows[i]["WorkAreaCode"].ToString(),
                                            Site = dt.Rows[i]["SiteCode"].ToString(),
                                            UserCode = dt.Rows[i]["UserId"].ToString(),
                                            UserTel = tel,
                                            DXType = "",
                                            BusinessCode = shortMessageTemplateModel.TemplateCode,
                                            DataCode = dt.Rows[i]["DisEntOrderId"].ToString(),
                                            ShortContent = ShortContent,
                                            FromCode = "TransportProcess",
                                            MsgType = "1"
                                        };
                                        myDxList.Add(myDxMsg);
                                    }
                                }
                                if (NoticeModel.Pc == 1)
                                {
                                    //'我的消息'推送 
                                    var myFormEarlyMsg = new TbFormEarlyWarningNodeInfo()
                                    {

                                        MenuCode = "TransportProcess",
                                        EWNodeCode = Convert.ToInt32(dt.Rows[i]["DisEntOrderId"]),
                                        EWUserCode = dt.Rows[i]["UserId"].ToString(),
                                        ProjectId = dt.Rows[i]["ProjectId"].ToString(),
                                        EarlyWarningCode = dt.Rows[i]["EarlyWarningNewsCode"].ToString(),
                                        EWFormDataCode = Convert.ToInt32(dt.Rows[i]["DisEntOrderId"]),
                                        CompanyCode = dt.Rows[i]["BranchCode"].ToString(),
                                        WorkArea = dt.Rows[i]["WorkAreaCode"].ToString(),
                                        SiteCode = dt.Rows[i]["SiteCode"].ToString(),
                                        MsgType = "1",
                                        EWContent = ShortContent,
                                        EWStart = 0,
                                        EWTime = DateTime.Now,
                                        ProcessFactoryCode = dt.Rows[i]["ProcessFactoryCode"].ToString(),
                                        DataCode = dt.Rows[i]["DistributionCode"].ToString(),
                                        EarlyTitle = "【" + dt.Rows[i]["DistributionCode"].ToString() + "】" + NoticeModel.NoticeNewsName
                                    };
                                    myMsgList.Add(myFormEarlyMsg);
                                }
                            }
                            //调用短信接口发送短信
                            for (int m = 0; m < myDxList.Count; m++)
                            {
                                //var dx = ct.ShortMessagePC(myDxList[m].UserTel, myDxList[m].ShortContent);
                                var dx = ct.ShortMessagePC("15756321745", myDxList[m].ShortContent);
                                var jObject = Newtonsoft.Json.Linq.JObject.Parse(dx);
                                var logmsg = jObject["data"][0]["code"].ToString();
                                myDxList[m].DXType = logmsg;
                            }
                        }
                    }
                }
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    if (myMsgList.Any())
                    {
                        //添加表单预警信息
                        Repository<PM.DataEntity.TbFormEarlyWarningNodeInfo>.Insert(trans, myMsgList, true);
                    }

                    if (myDxList.Any())
                    {
                        //向短信信息表中插入数据
                        Repository<TbSMSAlert>.Insert(trans, myDxList, true);
                    }
                    //调用手机端消息推送方法
                    trans.Commit();//提交事务
                }
                status3 = true;

            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public void GetExaminestatus()
        {
            try
            {
                List<TbFlowEarlyWarningOtherInfo> myMsgList = new List<TbFlowEarlyWarningOtherInfo>();//Pc端推送
                List<TbSMSAlert> myDxList = new List<TbSMSAlert>();//App端
                string sql = @"select *,cast(DATEDIFF(MINUTE,TbEarlyInfo.EarlyBegTime,GETDATE())*1.0/60/24 as decimal(18,2)) as SjJgDay,cast(DATEDIFF(MINUTE,TbEarlyInfo.EarlyBegTime,GETDATE())*1.0/60 as decimal(18,2)) as SjJgHour from (select TbFlowEarly.*,TbFlowEarlyData.id as FlowPerformOID,TbFlowEarlyData.FlowPerformID,TbFlowEarlyData.FlowTitle,TbFlowEarlyData.FormCode,TbFlowEarlyData.FormDataCode,TbFlowEarlyData.PreNodeCompleteDate,TbFlowEarlyData.UserCode,case when TbFlowEarly.EarlyWarningTimeType='日' then DATEADD(DAY,TbFlowEarly.EarlyWarningTime,TbFlowEarlyData.PreNodeCompleteDate) when TbFlowEarly.EarlyWarningTimeType='时' then DATEADD(HOUR,TbFlowEarly.EarlyWarningTime,TbFlowEarlyData.PreNodeCompleteDate) else  DATEADD(MINUTE,TbFlowEarly.EarlyWarningTime,TbFlowEarlyData.PreNodeCompleteDate)  end EarlyBegTime,smt.TableName,sm.MenuName from (select fewc.*,fnwp.PersonnelSource,fnwp.PersonnelCode,fnwp.DeptId,fnwp.RoleId,fnwp.ProjectId,fnwp.OrgType from TbFlowEarlyWarningCondition fewc
left join TbFlowNodeEarlyWarningPersonnel fnwp on fewc.FlowCode=fnwp.FlowCode and fewc.FlowNodeCode=fnwp.FlowNodeCode and fewc.EarlyWarningCode=fnwp.EarlyWarningCode
where fewc.FlowType='New' and fewc.IsStart=1) TbFlowEarly
left join (select fpo.id,fp.FlowCode,fp.FlowPerformID,fp.FlowTitle,fp.FormCode,fp.FormDataCode,fpo.FlowNodeCode,fpo.UserCode,fpo.PreNodeCompleteDate from TbFlowPerform fp
left join TbFlowPerformOpinions fpo on fp.FlowPerformID=fpo.FlowPerformID 
where fp.FlowState=-1 and fpo.UserType=0  and fpo.PerformState=-1 and fpo.id=(select MAX(id) id from TbFlowPerformOpinions where FlowNodeCode=fpo.FlowNodeCode and FlowPerformID=fpo.FlowPerformID and PerformState=-1 and UserType=0)) TbFlowEarlyData on TbFlowEarly.FlowCode=TbFlowEarlyData.FlowCode and TbFlowEarly.FlowNodeCode=TbFlowEarlyData.FlowNodeCode
left join TbSysMenuTable smt on TbFlowEarlyData.FormCode=smt.MenuCode
left join TbSysMenu sm on TbFlowEarlyData.FormCode=sm.MenuCode
where TbFlowEarlyData.FlowCode is not null) TbEarlyInfo 
where GETDATE()>TbEarlyInfo.EarlyBegTime";
                DataTable dt = Db.Context.FromSql(sql).ToDataTable();
                if (dt.Rows.Count > 0)
                {
                    //查找消息模板信息
                    var shortMessageTemplateModel = Repository<TbShortMessageTemplate>.First(p => p.TemplateCode == "MB0003");
                    if (shortMessageTemplateModel != null)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            DataTable dtOrg = ct.GetDataOrg(dt.Rows[i]["FormCode"].ToString(), Convert.ToInt32(dt.Rows[i]["FormDataCode"]));
                            if (dtOrg.Rows.Count > 0)
                            {
                                //获取要发送短信通知消息的用户
                                List<CensusdemoTask.NotiecUser> listUser = ct.GetSendUser(dt.Rows[i]["FormCode"].ToString(), dt.Rows[i]["EarlyWarningCode"].ToString(), Convert.ToInt32(dt.Rows[i]["FormDataCode"]), "流程预警");
                                for (int u = 0; u < listUser.Count; u++)
                                {
                                    //短信内容                                          
                                    string content = shortMessageTemplateModel.TemplateContent;
                                    var a = content;
                                    if (!string.IsNullOrWhiteSpace(dtOrg.Rows[0]["SiteCode"].ToString()))
                                    {
                                        a = content.Replace("变量：分部/工区/站点", dtOrg.Rows[0]["BranchName"] + "/" + dtOrg.Rows[0]["WorkAreaName"] + "/" + dtOrg.Rows[0]["SiteName"]);
                                    }
                                    else
                                    {
                                        a = content.Replace("变量：分部/工区/站点", dtOrg.Rows[0]["BranchName"] + "/" + dtOrg.Rows[0]["WorkAreaName"]);
                                    }
                                    var b = a.Replace("变量：表单功能", dt.Rows[i]["MenuName"].ToString());
                                    var ShortContent = b.Replace("变量：时间（小时/天）", dt.Rows[i]["SjJgHour"] + "/" + dt.Rows[i]["SjJgDay"]);
                                    if (dt.Rows[i]["App"].ToString() == "1")
                                    {
                                        //调用BIM获取人员电话或者身份证号码的的接口
                                        string userInfo = ct.up(listUser[u].PersonnelCode);
                                        var jObject = Newtonsoft.Json.Linq.JObject.Parse(userInfo);
                                        string tel = jObject["data"][0]["MobilePhone"].ToString();
                                        if (!string.IsNullOrWhiteSpace(tel))
                                        {
                                            var myDxMsg = new TbSMSAlert()
                                            {
                                                InsertTime = DateTime.Now,
                                                ManagerDepartment = dtOrg.Rows[0]["ManagerDepartmentCode"].ToString(),
                                                Branch = dtOrg.Rows[0]["BranchCode"].ToString(),
                                                WorkArea = dtOrg.Rows[0]["WorkAreaCode"].ToString(),
                                                Site = dtOrg.Rows[0]["SiteCode"].ToString(),
                                                UserCode = listUser[u].PersonnelCode,
                                                UserTel = tel,
                                                DXType = "",
                                                BusinessCode = shortMessageTemplateModel.TemplateCode,
                                                DataCode = dt.Rows[i]["EarlyCode"].ToString(),
                                                ShortContent = ShortContent,
                                                FromCode = dt.Rows[i]["FormCode"].ToString(),
                                                MsgType = "3"
                                            };
                                            myDxList.Add(myDxMsg);
                                        }
                                    }
                                    if (dt.Rows[i]["Pc"].ToString() == "1")
                                    {
                                        var myFormEarlyMsg = new TbFlowEarlyWarningOtherInfo()
                                        {
                                            EarlyWarningCode = dt.Rows[i]["EarlyWarningCode"].ToString(),
                                            FlowPerformOID = Convert.ToInt32(dt.Rows[i]["FlowPerformOID"]),
                                            FlowPerformID = dt.Rows[i]["FlowPerformID"].ToString(),
                                            FlowCode = dt.Rows[i]["FlowCode"].ToString(),
                                            FlowNodeCode = dt.Rows[i]["FlowNodeCode"].ToString(),
                                            EWFormCode = dt.Rows[i]["FormCode"].ToString(),
                                            EWFormDataCode = Convert.ToInt32(dt.Rows[i]["FormDataCode"]),
                                            EarlyWarningContent = ShortContent,
                                            EarlyWarningTime = DateTime.Now,
                                            SiteCode = dtOrg.Rows[0]["BranchCode"].ToString(),
                                            WorkAreaCode = dtOrg.Rows[0]["WorkAreaCode"].ToString(),
                                            BranchCode = dtOrg.Rows[0]["BranchCode"].ToString(),
                                            ProcessFactoryCode = dtOrg.Rows[0]["ProcessFactoryCode"].ToString(),
                                            ProjectId = dtOrg.Rows[0]["ProjectId"].ToString(),
                                            FlowSpUserCode = dtOrg.Rows[0]["UserCode"].ToString(),
                                            FlowYjUserCode = listUser[u].PersonnelCode,
                                            TableName = dtOrg.Rows[0]["TableName"].ToString(),
                                            DataCode = dtOrg.Rows[0]["DataCode"].ToString(),
                                            EarlyWarningStart = 0
                                        };
                                        myMsgList.Add(myFormEarlyMsg);
                                    }
                                }
                            }
                        }
                    }
                    //调用短信接口发送短信
                    for (int m = 0; m < myDxList.Count; m++)
                    {
                        //var dx = ct.ShortMessagePC(myDxList[m].UserTel, myDxList[m].ShortContent);
                        var dx = ct.ShortMessagePC("15756321745", myDxList[m].ShortContent);
                        var jObject = Newtonsoft.Json.Linq.JObject.Parse(dx);
                        var logmsg = jObject["data"][0]["code"].ToString();
                        myDxList[m].DXType = logmsg;
                    }
                }
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    if (myMsgList.Any())
                    {
                        //添加表单预警信息
                        Repository<PM.DataEntity.TbFlowEarlyWarningOtherInfo>.Insert(trans, myMsgList, true);
                    }
                    if (myDxList.Any())
                    {
                        //向短信信息表中插入数据
                        Repository<TbSMSAlert>.Insert(trans, myDxList, true);
                    }
                    //调用手机端消息推送方法
                    trans.Commit();//提交事务
                }
                status4 = true;
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion

    }
}
