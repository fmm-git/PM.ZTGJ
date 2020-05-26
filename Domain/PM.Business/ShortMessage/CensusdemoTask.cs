using Dos.ORM;
using PM.Business.System;
using PM.Common.Extension;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace PM.Business.ShortMessage
{
    public class CensusdemoTask
    {
        private readonly ShortMessageTemplateLogic _smtLogic = new ShortMessageTemplateLogic();

        #region 短信相关

        /// <summary>
        /// 短信状态返回判断
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public string MessageStr(int code)
        {
            switch (code)
            {
                case 2:
                    return "成功";
                case 400:
                    return "非法IP访问";
                case 401:
                    return "账号不能为空";
                case 403:
                    return "手机不能为空";
                case 4030:
                    return "手机号码已被列入黑名单";
                case 404:
                    return "短信内容不能为空";
                case 405:
                    return "APIID 或 API KEY 不正确";
                case 4050:
                    return "账号被冻结";
                case 40501:
                    return "动态密码已过期";
                case 40502:
                    return "动态密码校验失败";
                case 4051:
                    return "剩余条数不足";
                case 4052:
                    return "访问IP与备案IP不符";
                case 406:
                    return "手机号码格式不正确";
                case 407:
                    return "短信内容喊有敏感字符串";
                case 4070:
                    return "签名格式不正确";
                case 4071:
                    return "没有提示备案模板";
                case 4072:
                    return "提交的短信内容与审核通过的模板内容不匹配";
                case 40722:
                    return "变量的内容超过指定的长度【8】";
                case 4073:
                    return "短信内容超出长度限制";
                case 4074:
                    return "短信内容包含emoji符号";
                case 4075:
                    return "签名未通过审核";
                default:
                    return "";
            }
        }

        /// <summary>
        /// 短信提示
        /// </summary>
        /// <param name="phones">提醒的手机号，多个以","隔开</param>
        /// <param name="content">发送的内容</param>
        /// <returns></returns>
        public string ShortMessagePC(string phones, string content)
        {
            //发送短信网址
            var url = "http://124.42.243.98:8089/EPCPMS/sys/sendMsg.json";
            string textResponse = "";
            try
            {
                string postData = string.Format("phones={0}&content={1}", phones, content); // 要发放的数据
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                HttpWebRequest objWebRequest = (HttpWebRequest)WebRequest.Create(url); //发送地址
                objWebRequest.Method = "POST";//提交方式
                objWebRequest.ContentType = "application/x-www-form-urlencoded";
                objWebRequest.ContentLength = byteArray.Length;
                Stream newStream = objWebRequest.GetRequestStream(); // Send the data.
                newStream.Write(byteArray, 0, byteArray.Length); //写入参数
                newStream.Close();
                HttpWebResponse response = (HttpWebResponse)objWebRequest.GetResponse();//获取响应
                StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                //textResponse = sr.ReadToEnd() + "返回数据"; // 返回的数据
                textResponse = sr.ReadToEnd();
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            return textResponse;
        }


        #endregion

        #region 获取用户的电话号码已经身份证号码的接口

        /// <summary>
        /// 获取用户的电话号码已经身份证号码的接口
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public string up(string uid)
        {
            //数据网址
            var url = "http://124.42.243.98:8089/EPCPMS/server/byServer/queryUserInfo.json";
            var textResponse = "";
            try
            {
                string postData = string.Format("userId={0}", uid); // 要发放的数据
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                HttpWebRequest objWebRequest = (HttpWebRequest)WebRequest.Create(url); //发送地址
                objWebRequest.Method = "POST";//提交方式
                objWebRequest.ContentType = "application/x-www-form-urlencoded";
                objWebRequest.ContentLength = byteArray.Length;
                Stream newStream = objWebRequest.GetRequestStream(); // Send the data.
                newStream.Write(byteArray, 0, byteArray.Length); //写入参数
                newStream.Close();
                HttpWebResponse response = (HttpWebResponse)objWebRequest.GetResponse();//获取响应
                StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                textResponse = sr.ReadToEnd();
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            return textResponse;
        }

        #endregion

        #region 获取用户的UserId
        public DataTable GetUserId(string UserCode)
        {
            string sql = "select UserId from TbUser where UserCode='" + UserCode + "'";
            var ret = Db.Context.FromSql(sql).ToDataTable();
            return ret;
        }

        #endregion

        public DataTable GetParentCompany(string SiteCode)
        {
            //获取所有的上级编号
            string sqlparentCode = @"with tab as
                                    (
                                     select CompanyCode,ParentCompanyCode,CompanyFullName,OrgType from TbCompany where CompanyCode=@CompanyCode
                                     union all
                                     select b.CompanyCode,b.ParentCompanyCode,b.CompanyFullName,b.OrgType
                                     from
                                      tab a,
                                      TbCompany b 
                                      where a.ParentCompanyCode=b.CompanyCode
                                    )
                                    select * from tab where OrgType=3 or OrgType=4 or OrgType=5 or OrgType=2 order by OrgType asc;";
            var ret1 = Db.Context.FromSql(sqlparentCode).AddInParameter("@CompanyCode", DbType.String, SiteCode).ToDataTable();
            return ret1;
        }

        public List<NotiecUser> GetSendUser(string FormCode, string NoticeNewsCode, int ID, string EarlyType = "")
        {
            List<NotiecUser> nuList = new List<NotiecUser>();
            //查询出当前发起流程数据的业务表
            var menuTable = Repository<TbSysMenuTable>.First(p => p.MenuCode == FormCode && p.IsMainTabel == "0");
            //查询出业务数据对应的工区、分部、经理部、加工厂信息
            if (menuTable != null)
            {
                string sqlTableData = "";
                if (FormCode == "RawMonthDemandPlan" || FormCode == "RawMonthDemandSupplyPlan" || FormCode == "FactoryBatchNeedPlan" || FormCode == "RMProductionMaterial" || FormCode == "SupplyList")
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
                else if (FormCode == "WorkOrder" || FormCode == "ProblemOrder" || FormCode == "DistributionPlan")
                {
                    sqlTableData = @"select b.ParentCompanyCode as WorkAreaCode,c.ParentCompanyCode as BranchCode,d.ParentCompanyCode as JlbCode,a.ProcessFactoryCode from " + menuTable.TableName + @" a
left join TbCompany b on a.SiteCode=b.CompanyCode
left join TbCompany c on b.ParentCompanyCode=c.CompanyCode
left join TbCompany d on c.ParentCompanyCode=d.CompanyCode
where a.ID=@ID";
                }
                DataTable dtTableData = Db.Context.FromSql(sqlTableData).AddInParameter("@ID", DbType.Int32, ID).ToDataTable();
                if (dtTableData.Rows.Count > 0)
                {

                    if (!string.IsNullOrWhiteSpace(EarlyType))
                    {
                        var NoticeUserList1 = Repository<TbFlowNodeEarlyWarningPersonnel>.Query(p => p.EarlyWarningCode == NoticeNewsCode);
                        string sqlUser = "";
                        for (int i = 0; i < NoticeUserList1.Count; i++)
                        {
                            if (NoticeUserList1[i].PersonnelSource == "Role")//选择角色
                            {
                                string OrgId = "";
                                switch (NoticeUserList1[i].OrgType)
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
                                sqlUser = "select a.UserCode from TbUserRole a where 1=1 and a.Flag=0 and a.DeptId='" + NoticeUserList1[i].DeptId + "' and a.RoleCode='" + NoticeUserList1[i].RoleId + "' and a.ProjectId='" + NoticeUserList1[i].ProjectId + "' and a.OrgType=" + NoticeUserList1[i].OrgType + " and a.OrgId='" + OrgId + "' ";
                                DataTable dtUser = Db.Context.FromSql(sqlUser).ToDataTable();
                                if (dtUser.Rows.Count > 0)
                                {
                                    for (int u = 0; u < dtUser.Rows.Count; u++)
                                    {
                                        NotiecUser nuModel = new NotiecUser();
                                        nuModel.PersonnelSource = "Personnel";
                                        nuModel.PersonnelCode = dtUser.Rows[u]["UserCode"].ToString();
                                        nuList.Add(nuModel);
                                    }
                                }
                            }
                            else//直接选择用户
                            {
                                NotiecUser nuModel = new NotiecUser();
                                nuModel.PersonnelSource = NoticeUserList1[i].PersonnelSource;
                                nuModel.PersonnelCode = NoticeUserList1[i].PersonnelCode;
                                nuList.Add(nuModel);
                            }
                        }
                    }
                    else
                    {
                        var NoticeUserList2 = Repository<TbNoticeNewsOrg>.Query(p => p.NoticeNewsCode == NoticeNewsCode);
                        string sqlUser = "";
                        for (int i = 0; i < NoticeUserList2.Count; i++)
                        {
                            if (NoticeUserList2[i].PersonnelSource == "Role")//选择角色
                            {
                                string OrgId = "";
                                switch (NoticeUserList2[i].OrgType)
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
                                sqlUser = "select a.UserCode from TbUserRole a where 1=1 and a.Flag=0 and a.DeptId='" + NoticeUserList2[i].DeptId + "' and a.RoleCode='" + NoticeUserList2[i].RoleId + "' and a.ProjectId='" + NoticeUserList2[i].ProjectId + "' and a.OrgType=" + NoticeUserList2[i].OrgType + " and a.OrgId='" + OrgId + "' ";
                                DataTable dtUser = Db.Context.FromSql(sqlUser).ToDataTable();
                                if (dtUser.Rows.Count > 0)
                                {
                                    for (int u = 0; u < dtUser.Rows.Count; u++)
                                    {
                                        NotiecUser nuModel = new NotiecUser();
                                        nuModel.PersonnelSource = "Personnel";
                                        nuModel.PersonnelCode = dtUser.Rows[u]["UserCode"].ToString();
                                        nuList.Add(nuModel);
                                    }
                                }
                            }
                            else//直接选择用户
                            {
                                NotiecUser nuModel = new NotiecUser();
                                nuModel.PersonnelSource = NoticeUserList2[i].PersonnelSource;
                                nuModel.PersonnelCode = NoticeUserList2[i].PersonnelCode;
                                nuList.Add(nuModel);
                            }
                        }
                    }
                }
            }
            return nuList;
        }

        public DataTable GetDataOrg(string FormCode, int ID)
        {
            //查询出当前发起流程数据的业务表
            var menuTable = Repository<TbSysMenuTable>.First(p => p.MenuCode == FormCode && p.IsMainTabel == "0");
            string sqlTableData = "";
            //查询出业务数据对应的工区、分部、经理部、加工厂信息
            if (menuTable != null)
            {
                if (menuTable.MenuCode == "TbWorkOrder")//加工订单
                {
                    sqlTableData = @"select TbWorkOrder.OrderCode as DataCode,TbWorkOrder.SiteCode,cp.CompanyFullName as SiteName,cp.ParentCompanyCode as WorkAreaCode,cp1.CompanyFullName as WorkAreaName,cp1.ParentCompanyCode as BranchCode,cp2.CompanyFullName as BranchName,cp2.ParentCompanyCode as ManagerDepartmentCode,cp3.CompanyFullName as ManagerDepartmentName,TbWorkOrder.ProcessFactoryCode from TbWorkOrder 
		left join TbCompany cp on TbWorkOrder.SiteCode=cp.CompanyCode
		left join TbCompany cp1 on cp1.CompanyCode=cp.ParentCompanyCode
		left join TbCompany cp2 on cp1.ParentCompanyCode=cp2.CompanyCode
		left join TbCompany cp3 on cp2.ParentCompanyCode=cp3.CompanyCode
		where TbWorkOrder.ID=@ID";
                }
                else if (menuTable.MenuCode == "TbRMProductionMaterial")//生产领料
                {
                    sqlTableData = @"select TbRMProductionMaterial.CollarCode as DataCode,TbRMProductionMaterial.SiteCode,cp.CompanyFullName as SiteName,TbRMProductionMaterial.WorkAreaCode,cp1.CompanyFullName as WorkAreaName,TbRMProductionMaterial.BranchCode,cp2.CompanyFullName as BranchName,cp3.CompanyCode as ManagerDepartmentCode,cp3.CompanyFullName as ManagerDepartmentName,TbRMProductionMaterial.ProcessFactoryCode from TbRMProductionMaterial 
		left join TbCompany cp on TbRMProductionMaterial.SiteCode=cp.CompanyCode
		left join TbCompany cp1 on TbRMProductionMaterial.WorkAreaCode=cp1.CompanyCode
		left join TbCompany cp2 on TbRMProductionMaterial.BranchCode=cp2.CompanyCode
		left join TbCompany cp3 on cp2.ParentCompanyCode=cp3.CompanyCode
		where TbRMProductionMaterial.ID=@ID";
                }
                else if (menuTable.MenuCode == "TbInOrder")//到货入库
                {
                    sqlTableData = @"select TbInOrder.InOrderCode as DataCode,TbInOrder.SiteCode,cp.CompanyFullName as SiteName,TbInOrder.WorkAreaCode,cp1.CompanyFullName as WorkAreaName,cp2.CompanyCode as BranchCode,cp2.CompanyFullName as BranchName,cp3.CompanyCode as ManagerDepartmentCode,cp3.CompanyFullName as ManagerDepartmentName,st.ProcessFactoryCode from TbInOrder 
		left join TbCompany cp on TbInOrder.SiteCode=cp.CompanyCode
		left join TbCompany cp1 on TbInOrder.WorkAreaCode=cp1.CompanyCode
		left join TbCompany cp2 on cp1.ParentCompanyCode=cp2.CompanyCode
		left join TbCompany cp3 on cp2.ParentCompanyCode=cp3.CompanyCode
		left join TbStorage st on TbInOrder.StorageCode=st.StorageCode
		where TbInOrder.ID=@ID";
                }
                else if (menuTable.MenuCode == "TbRawMaterialMonthDemandPlan")//月度需求计划
                {
                    sqlTableData = @"select TbRawMaterialMonthDemandPlan.DemandPlanCode as DataCode,TbRawMaterialMonthDemandPlan.SiteCode,cp.CompanyFullName as SiteName,TbRawMaterialMonthDemandPlan.WorkAreaCode,cp1.CompanyFullName as WorkAreaName,TbRawMaterialMonthDemandPlan.BranchCode,cp2.CompanyFullName as BranchName,cp3.CompanyCode as ManagerDepartmentCode,cp3.CompanyFullName as ManagerDepartmentName,TbRawMaterialMonthDemandPlan.ProcessFactoryCode from TbRawMaterialMonthDemandPlan 
		left join TbCompany cp on TbRawMaterialMonthDemandPlan.SiteCode=cp.CompanyCode
		left join TbCompany cp1 on TbRawMaterialMonthDemandPlan.WorkAreaCode=cp1.CompanyCode
		left join TbCompany cp2 on TbRawMaterialMonthDemandPlan.BranchCode=cp2.CompanyCode
		left join TbCompany cp3 on cp2.ParentCompanyCode=cp3.CompanyCode
		where TbRawMaterialMonthDemandPlan.ID=@ID";
                }
                else if (menuTable.MenuCode == "TbFactoryBatchNeedPlan")//加工厂批次需求计划
                {
                    sqlTableData = @"select TbFactoryBatchNeedPlan.BatchPlanNum,TbFactoryBatchNeedPlan.SiteCode,cp.CompanyFullName as SiteName,TbFactoryBatchNeedPlan.WorkAreaCode,cp1.CompanyFullName as WorkAreaName,TbFactoryBatchNeedPlan.BranchCode,cp2.CompanyFullName as BranchName,cp3.CompanyCode as ManagerDepartmentCode,cp3.CompanyFullName as ManagerDepartmentName,TbFactoryBatchNeedPlan.ProcessFactoryCode from TbFactoryBatchNeedPlan 
		left join TbCompany cp on TbFactoryBatchNeedPlan.SiteCode=cp.CompanyCode
		left join TbCompany cp1 on TbFactoryBatchNeedPlan.WorkAreaCode=cp1.CompanyCode
		left join TbCompany cp2 on TbFactoryBatchNeedPlan.BranchCode=cp2.CompanyCode
		left join TbCompany cp3 on cp2.ParentCompanyCode=cp3.CompanyCode
		where TbFactoryBatchNeedPlan.ID=@ID";
                }
                else if (menuTable.MenuCode == "TbRawMaterialMonthDemandSupplyPlan")//月度需求补充计划
                {
                    sqlTableData = @"select TbRawMaterialMonthDemandSupplyPlan.SupplyPlanCode as DataCode,TbRawMaterialMonthDemandSupplyPlan.SiteCode,cp.CompanyFullName as SiteName,TbRawMaterialMonthDemandSupplyPlan.WorkAreaCode,cp1.CompanyFullName as WorkAreaName,TbRawMaterialMonthDemandSupplyPlan.BranchCode,cp2.CompanyFullName as BranchName,cp3.CompanyCode as ManagerDepartmentCode,cp3.CompanyFullName as ManagerDepartmentName,TbRawMaterialMonthDemandSupplyPlan.ProcessFactoryCode from TbRawMaterialMonthDemandSupplyPlan 
		left join TbCompany cp on TbRawMaterialMonthDemandSupplyPlan.SiteCode=cp.CompanyCode
		left join TbCompany cp1 on TbRawMaterialMonthDemandSupplyPlan.WorkAreaCode=cp1.CompanyCode
		left join TbCompany cp2 on TbRawMaterialMonthDemandSupplyPlan.BranchCode=cp2.CompanyCode
		left join TbCompany cp3 on cp2.ParentCompanyCode=cp3.CompanyCode
		where TbRawMaterialMonthDemandSupplyPlan.ID=@ID";
                }
                else if (menuTable.MenuCode == "TbProblemOrder")//问题订单
                {
                    sqlTableData = @"select TbProblemOrder.ProblemOrderCode as DataCode,TbProblemOrder.SiteCode,cp.CompanyFullName as SiteName,cp.ParentCompanyCode as WorkAreaCode,cp1.CompanyFullName as WorkAreaName,cp1.ParentCompanyCode as BranchCode,cp2.CompanyFullName as BranchName,cp3.CompanyCode as ManagerDepartmentCode,cp3.CompanyFullName as ManagerDepartmentName,TbProblemOrder.ProcessFactoryCode from TbProblemOrder 
		left join TbCompany cp on TbProblemOrder.SiteCode=cp.CompanyCode
		left join TbCompany cp1 on cp1.CompanyCode=cp.ParentCompanyCode
		left join TbCompany cp2 on cp1.ParentCompanyCode=cp2.CompanyCode
		left join TbCompany cp3 on cp2.ParentCompanyCode=cp3.CompanyCode
		where TbProblemOrder.ID=@ID";
                }
                else if (menuTable.MenuCode == "TbSampleOrder")//取样订单
                {
                    sqlTableData = @"select TbSampleOrder.SampleOrderCode as DataCode,TbSampleOrder.SiteCode,cp1.CompanyFullName as SiteName,TbSampleOrder.WorkAreaCode,cp1.CompanyFullName as WorkAreaName,cp.ParentCompanyCode as BranchCode,cp2.CompanyFullName as BranchName,cp3.CompanyCode as ManagerDepartmentCode,cp3.CompanyFullName as ManagerDepartmentName,TbSampleOrder.ProcessFactoryCode from TbSampleOrder
		left join TbCompany cp on TbSampleOrder.SiteCode=cp.CompanyCode
		left join TbCompany cp1 on TbSampleOrder.WorkAreaCode=cp1.CompanyCode
		left join TbCompany cp2 on cp1.ParentCompanyCode=cp2.CompanyCode
		left join TbCompany cp3 on cp2.ParentCompanyCode=cp3.CompanyCode
		where TbSampleOrder.ID=@ID";
                }
                else if (menuTable.MenuCode == "TbSignforDuiZhang")//签收对账单
                {
                    sqlTableData = @"select TbSignforDuiZhang.SigninNuber as DataCode,TbSignforDuiZhang.SiteCode,cp.CompanyFullName as SiteName,cp.ParentCompanyCode as WorkAreaCode,cp1.CompanyFullName as WorkAreaName,cp1.ParentCompanyCode as BranchCode,cp2.CompanyFullName as BranchName,cp3.CompanyCode as ManagerDepartmentCode,cp3.CompanyFullName as ManagerDepartmentName,TbSignforDuiZhang.ProcessFactoryCode from TbSignforDuiZhang
		left join TbCompany cp on TbSignforDuiZhang.SiteCode=cp.CompanyCode
		left join TbCompany cp1 on cp.ParentCompanyCode=cp1.CompanyCode
		left join TbCompany cp2 on cp1.ParentCompanyCode=cp2.CompanyCode
		left join TbCompany cp3 on cp2.ParentCompanyCode=cp3.CompanyCode
		where TbSignforDuiZhang.ID=@ID";
                }
                else if (menuTable.MenuCode == "TbSettlementOrder")//结算单
                {
                    sqlTableData = @"select TbSettlementOrder.SettlementCode as DataCode,TbSettlementOrder.SiteCode,cp.CompanyFullName as SiteName,cp.ParentCompanyCode as WorkAreaCode,cp1.CompanyFullName as WorkAreaName,cp1.ParentCompanyCode as BranchCode,cp2.CompanyFullName as BranchName,cp3.CompanyCode as ManagerDepartmentCode,cp3.CompanyFullName as ManagerDepartmentName,TbSettlementOrder.ProcessFactoryCode from TbSettlementOrder
		left join TbCompany cp on TbSettlementOrder.SiteCode=cp.CompanyCode
		left join TbCompany cp1 on cp.ParentCompanyCode=cp1.CompanyCode
		left join TbCompany cp2 on cp1.ParentCompanyCode=cp2.CompanyCode
		left join TbCompany cp3 on cp2.ParentCompanyCode=cp3.CompanyCode
		where TbSettlementOrder.ID=@ID";
                }
                else
                {
                    sqlTableData = "";
                }
            }
            DataTable dtTableData = Db.Context.FromSql(sqlTableData).AddInParameter("@ID", DbType.Int32, ID).ToDataTable();
            return dtTableData;
        }
        public class NotiecUser
        {
            public string PersonnelSource { get; set; }
            public string PersonnelCode { get; set; }
        }
    }
}
