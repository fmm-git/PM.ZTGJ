using Dos.Common;
using Dos.ORM;
using PM.Business.Production;
using PM.Business.ShortMessage;
using PM.Business.System;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using PM.Domain.WebBase;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;

namespace PM.Business.Distribution
{
    /// <summary>
    /// 运输流程确认
    /// </summary>
    public class TransportFlowLogic
    {
        private readonly TbWorkOrderLogic _workOrderLogic = new TbWorkOrderLogic();
        private readonly ShortMessageTemplateLogic _smtLogic = new ShortMessageTemplateLogic();
        CensusdemoTask ct = new CensusdemoTask();
        /// <summary>
        /// 获取当前司机所在的运输流程信息
        /// </summary>
        /// <param name="userCode"></param>
        public List<TransportFlowResponseNew> GetTransportFlowInfo(string userCode)
        {
            try
            {
                StringBuilder sbSiteCode = new StringBuilder();
                string sql = @"select Tb.*,cp.CompanyFullName as SiteName from (select case when disEntOrder.ID is null then disEnt.ID else disEntOrder.ID end as DisEntOrderId,disEnt.DistributionCode,case when disEntOrder.SiteCode is null then disEnt.SiteCode else disEntOrder.SiteCode end SiteCode,case when disEntOrder.OrderCode is null then disEnt.OrderCode else disEntOrder.OrderCode end OrderCode,disEnt.VehicleCode,car.CarCph as CarNumber,disEnt.Driver,ur.UserName as DriverName,case when disEntOrder.OrderFlowState=1 then disEnt.FlowState when disEntOrder.OrderFlowState is null then disEnt.FlowState else disEntOrder.OrderFlowState end  as FlowState from TbDistributionEnt disEnt
                               left join TbDistributionEntOrder disEntOrder on disEnt.DistributionCode=disEntOrder.DistributionCode
                               left join TbUser ur on disEnt.Driver=ur.UserCode
                               left join TbCarInfo car on  disEnt.VehicleCode=car.CarCode) Tb
                               left join TbCompany cp on Tb.SiteCode=cp.CompanyCode
                               where Tb.FlowState<5 and Tb.Driver='" + userCode + "'  ";

                //参数化
                List<Parameter> parameter = new List<Parameter>();
                var model = Repository<TransportFlowResponseNew>.FromSql(sql, parameter, "DistributionCode", "desc");
                return model;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 运输流程出厂确认
        /// </summary>
        /// <returns></returns>
        public AjaxResult FactoryFlowConfirm(TransportFlowRequestNew request)
        {
            try
            {
                //查找配送装车信息
                var distribution = Repository<TbDistributionEnt>.First(p => p.DistributionCode == request.DistributionCode);
                if (distribution == null)
                    return AjaxResult.Error("配送装车信息不存在");
                //查找配送装车订单信息
                var distributionOrder = Repository<TbDistributionEntOrder>.Query(p => p.DistributionCode == request.DistributionCode);
                if (distributionOrder == null)
                    return AjaxResult.Error("配送装车订单信息不存在");
                List<TbTransportCarReport> transportCarList = new List<TbTransportCarReport>();//车辆运输卸货统计
                if (distributionOrder.Count() > 0)
                {
                    DateTime LeaveFactoryTime = DateTime.Now;
                    for (int i = 0; i < distributionOrder.Count(); i++)
                    {
                        TbTransportCarReport transportCarFlow = new TbTransportCarReport();
                        transportCarFlow.OrderCode = distributionOrder[i].OrderCode;
                        transportCarFlow.DistributionCode = distributionOrder[i].DistributionCode;
                        transportCarFlow.FlowState = request.FlowState;
                        transportCarFlow.SiteCode = distributionOrder[i].SiteCode;
                        transportCarFlow.TypeCode = distributionOrder[i].TypeCode;
                        transportCarFlow.TypeName = distributionOrder[i].TypeName;
                        transportCarFlow.PlanDistributionTime = distributionOrder[i].PlanDistributionTime;
                        transportCarFlow.LoadCompleteTime = distribution.LoadCompleteTime;
                        transportCarFlow.VehicleCode = distribution.VehicleCode;
                        transportCarFlow.LeaveFactoryTime = LeaveFactoryTime;
                        transportCarFlow.EnterSpaceTime = null;
                        transportCarFlow.WaitTime = null;
                        transportCarFlow.StartDischargeTime = null;
                        transportCarFlow.EndDischargeTime = null;
                        transportCarFlow.OutSpaceTime = null;
                        transportCarFlow.IsProblem = "否";
                        transportCarFlow.State = 0;
                        transportCarFlow.ProjectId = distribution.ProjectId;
                        transportCarFlow.DisEntOrderId = distributionOrder[i].ID;
                        transportCarList.Add(transportCarFlow);
                    }
                    //调用短信通知消息
                    CarCcSendNotice(distribution.ProjectId, distributionOrder, LeaveFactoryTime);
                }
                distribution.FlowState = request.FlowState;
                //运输流程信息
                TbTransportFlow transportFlow = new TbTransportFlow()
                {
                    DistributionCode = request.DistributionCode,
                    VehicleCode = request.VehicleCode,
                    Driver = request.Driver,
                    Enclosure = "",
                    FlowType = request.FlowState,
                    ArticleId = 0,
                    InsertTime = DateTime.Now
                };
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //修改配送装车信息
                    Repository<TbDistributionEnt>.Update(trans, distribution, true);
                    //添加车辆运输卸货统计
                    Repository<TbTransportCarReport>.Insert(trans, transportCarList, true);
                    //添加运输流程信息
                    Repository<TbTransportFlow>.Insert(trans, transportFlow, true);
                    trans.Commit();
                    return AjaxResult.Success();
                }

            }
            catch (Exception)
            {
                return AjaxResult.Error();
            }
        }

        #region 车辆出厂通知
        public bool CarCcSendNotice(string ProjectId, List<TbDistributionEntOrder> distributionOrder, DateTime LeaveFactoryTime)
        {
            try
            {
                //原材料到货入库不合格材料退回通知
                List<TbFormEarlyWarningNodeInfo> myMsgList = new List<TbFormEarlyWarningNodeInfo>();//Pc端推送
                List<TbSMSAlert> myDxList = new List<TbSMSAlert>();//App端
                var NoticeModel = Repository<TbNoticeNewsSetUp>.First(p => p.NoticeNewsCode == "XXTZ0007" && p.IsStart == 1);
                if (NoticeModel != null)
                {
                    //查找短信模板信息
                    var shortMessageTemplateModel = Repository<TbShortMessageTemplate>.First(p => p.TemplateCode == "MB0005");
                    if (shortMessageTemplateModel != null)
                    {
                        for (int i = 0; i < distributionOrder.Count; i++)
                        {
                            //获取工区、分部、经理部、站点
                            DataTable dtParentOrg = ct.GetParentCompany(distributionOrder[i].SiteCode);
                            string ManagerDepartment = "";
                            string BranchCode = "";
                            string WorkAreaCode = "";
                            string SiteName = "";
                            if (dtParentOrg != null && dtParentOrg.Rows.Count > 0)
                            {
                                for (int org = 0; org < dtParentOrg.Rows.Count; org++)
                                {
                                    if (dtParentOrg.Rows[org]["OrgType"].ToString() == "2")
                                    {
                                        ManagerDepartment = dtParentOrg.Rows[org]["CompanyCode"].ToString();
                                    }
                                    else if (dtParentOrg.Rows[org]["OrgType"].ToString() == "3")
                                    {
                                        BranchCode = dtParentOrg.Rows[org]["CompanyCode"].ToString();
                                    }
                                    else if (dtParentOrg.Rows[org]["OrgType"].ToString() == "4")
                                    {
                                        WorkAreaCode = dtParentOrg.Rows[org]["CompanyCode"].ToString();
                                    }
                                    else if (dtParentOrg.Rows[org]["OrgType"].ToString() == "5")
                                    {
                                        SiteName = dtParentOrg.Rows[org]["CompanyFullName"].ToString();
                                    }
                                }
                            }
                            //短信内容
                            string content = shortMessageTemplateModel.TemplateContent;
                            var s = content.Replace("变量：站点", SiteName);
                            var a = s.Replace("变量：类型编码", distributionOrder[i].TypeCode);
                            var ShortContent = a.Replace("变量：时间", Convert.ToDateTime(LeaveFactoryTime).ToString("g"));
                            string UserId = ct.GetUserId(distributionOrder[i].SiteContacts).Rows[0]["UserId"].ToString();
                            if (NoticeModel.App == 1)
                            {
                                if (!string.IsNullOrWhiteSpace(distributionOrder[i].SiteContactTel))
                                {
                                    var myDxMsg = new TbSMSAlert()
                                    {
                                        InsertTime = DateTime.Now,
                                        ManagerDepartment = ManagerDepartment,
                                        Branch = BranchCode,
                                        WorkArea = WorkAreaCode,
                                        Site = distributionOrder[i].SiteCode,
                                        UserCode = UserId,
                                        UserTel = distributionOrder[i].SiteContactTel,
                                        DXType = "",
                                        BusinessCode = shortMessageTemplateModel.TemplateCode,
                                        DataCode = distributionOrder[i].DistributionCode,
                                        ShortContent = ShortContent,
                                        FromCode = "TransportProcess",
                                        MsgType = "1"

                                    };
                                    myDxList.Add(myDxMsg);
                                }
                            }

                            if (NoticeModel.Pc == 1)
                            {
                                var myFormEarlyMsg = new TbFormEarlyWarningNodeInfo()
                                {
                                    MenuCode = "TransportProcess",
                                    EWNodeCode = NoticeModel.ID,
                                    EWUserCode = UserId,
                                    ProjectId = ProjectId,
                                    EarlyWarningCode = NoticeModel.NoticeNewsCode,
                                    EWFormDataCode = distributionOrder[i].ID,
                                    CompanyCode = BranchCode,
                                    WorkArea = WorkAreaCode,
                                    SiteCode = distributionOrder[i].SiteCode,
                                    MsgType = "1",
                                    EWContent = ShortContent,
                                    EWStart = 0,
                                    EWTime = DateTime.Now,
                                    ProcessFactoryCode = "",
                                    DataCode = distributionOrder[i].DistributionCode,
                                    EarlyTitle = "【" + distributionOrder[i].DistributionCode + "】" + NoticeModel.NoticeNewsName
                                };
                                myMsgList.Add(myFormEarlyMsg);
                            }
                        }
                        for (int i = 0; i < myDxList.Count; i++)
                        {
                            //调用短信发送接口
                            //string dx = ct.ShortMessagePC(myDxList[i].UserTel, myDxList[i].ShortContent);
                            string dx = ct.ShortMessagePC("15756321745", myDxList[i].ShortContent);
                            var jObject1 = Newtonsoft.Json.Linq.JObject.Parse(dx);
                            var logmsg = jObject1["data"][0]["code"].ToString();
                            myDxList[i].DXType = logmsg;
                        }
                    }
                }
                using (DbTrans trans = Db.Context.BeginTransaction())
                {

                    if (myMsgList.Any())
                    {
                        //'我的消息'推送
                        Repository<TbFormEarlyWarningNodeInfo>.Insert(trans, myMsgList, true);
                    }
                    if (myDxList.Any())
                    {
                        //'短信'推送
                        Repository<TbSMSAlert>.Insert(trans, myDxList, true);
                    }
                    trans.Commit();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

        }
        #endregion

        /// <summary>
        /// 运输订单流程信息
        /// </summary>
        /// <returns></returns>
        public AjaxResult GetDisEntOrderFlowInfo(int DisEntOrderId)
        {
            try
            {
                string sql = @"select disEntOrder.ID as DisEntOrderId,disEnt.DistributionCode,disEntOrder.SiteCode,cp.CompanyFullName as SiteName,disEntOrder.OrderCode,disEnt.VehicleCode,car.CarCph as CarNumber,disEnt.Driver,ur.UserName as DriverName,disEntOrder.OrderFlowState as FlowState from TbDistributionEnt disEnt
                               left join TbDistributionEntOrder disEntOrder on disEnt.DistributionCode=disEntOrder.DistributionCode
                               left join TbUser ur on disEnt.Driver=ur.UserCode
                               left join TbCompany cp on disEntOrder.SiteCode=cp.CompanyCode
                               left join TbCarInfo car on  disEnt.VehicleCode=car.CarCode
                               where disEntOrder.ID=" + DisEntOrderId + "";
                //参数化
                List<Parameter> parameter = new List<Parameter>();
                var model = Repository<TransportFlowResponseNew>.FromSql(sql, parameter, "DistributionCode", "desc");
                if (model == null || model.Count() == 0)
                    return AjaxResult.Error("配送装车订单信息不存在");
                //查找问题数
                var count = Repository<TbArticle>.Count(p => p.Code == model[0].DistributionCode && p.DisEntOrderId == model[0].DisEntOrderId);
                model[0].ProblemCount = count;
                return AjaxResult.Success(model);

            }
            catch (Exception)
            {
                return AjaxResult.Error();
            }
        }


        /// <summary>
        /// 运输订单流程确认
        /// </summary>
        /// <returns></returns>
        public AjaxResult OrderFlowConfirm(TransportFlowRequestNew request)
        {
            string DischargeCargoCode = "";
            try
            {
                if (request.FlowState > 5 || request.FlowState < 1)
                    return AjaxResult.Error("配送流程状态错误");
                //查找配送装车信息
                var distribution = Repository<TbDistributionEnt>.First(p => p.DistributionCode == request.DistributionCode);
                if (distribution == null)
                    return AjaxResult.Error("配送装车信息不存在");
                //查找配送装车订单信息
                var distributionOrder = Repository<TbDistributionEntOrder>.First(p => p.DistributionCode == request.DistributionCode && p.ID == request.DisEntOrderId);
                if (distributionOrder == null)
                    return AjaxResult.Error("配送装车订单信息不存在");
                //查找车辆运输统计信息
                var transportCar = Repository<TbTransportCarReport>.First(p => p.DistributionCode == request.DistributionCode && p.DisEntOrderId == request.DisEntOrderId);
                if (transportCar == null)
                    return AjaxResult.Error("车辆运输统计信息不存在");
                distributionOrder.OrderFlowState += 1;
                if (distributionOrder.OrderFlowState != request.FlowState)
                    return AjaxResult.Error("配送流程状态错误");
                if (!string.IsNullOrEmpty(request.fileIds))
                {
                    if (!string.IsNullOrEmpty(distributionOrder.Enclosure))
                    {
                        distributionOrder.Enclosure = distributionOrder.Enclosure + "," + request.fileIds;
                    }
                    else
                    {
                        distributionOrder.Enclosure = request.fileIds;
                    }
                }
                TbTransportFlow transportFlow = null;//运输流程节点信息
                TbSiteDischargeCargo siteDischargeCargo = null;//卸货单
                List<TbSiteDischargeCargoDetail> siteDischargeCargoDetail = null;//卸货单明细
                request.Driver = distribution.Driver;
                request.VehicleCode = distribution.VehicleCode;
                if (request.FlowState != 1)
                {
                    if (transportCar.IsProblem == "否")
                    {
                        //判断运输流程是否存在问题
                        var Any = Repository<TbArticle>.Any(p => p.Code == request.DistributionCode && p.DisEntOrderId == request.DisEntOrderId);
                        if (Any)
                        {
                            transportCar.IsProblem = "是";
                            transportCar.FlowState = 3;
                        }
                    }
                }
                switch (request.FlowState)
                {
                    case 2://进场
                        transportCar.EnterSpaceTime = DateTime.Now;
                        break;
                    case 3://开始卸货
                        var endTime = DateTime.Now;
                        DateTime startTime = transportCar.EnterSpaceTime.Value;
                        var arry = (endTime - startTime).TotalMinutes.ToString().Split('.');
                        int waitTime = 0;
                        int.TryParse(arry[0], out waitTime);
                        transportCar.StartDischargeTime = endTime;
                        transportCar.WaitTime = waitTime;//等待卸货时间=开始卸货时间-运输到场时间
                        if (waitTime > 30)
                            transportCar.FlowState = 2;
                        break;
                    case 4://卸货完成
                        transportCar.EndDischargeTime = DateTime.Now;
                        //生成卸货单
                        var siteDischargeInfo = GetSiteDischargeInfoNew(distribution, distributionOrder, request.Driver);
                        if (siteDischargeInfo.Item2 == null)
                            return AjaxResult.Error("配送装车单明细信息不存在");
                        siteDischargeCargo = siteDischargeInfo.Item1;
                        siteDischargeCargoDetail = siteDischargeInfo.Item2;
                        //保存站点卸货编号
                        DischargeCargoCode = siteDischargeCargo.DischargeCargoCode;
                        //调用短信通知消息
                        QsEndSendNotice(distributionOrder, siteDischargeCargo);
                        break;
                    case 5://出场
                        transportCar.OutSpaceTime = DateTime.Now;
                        transportCar.State = 1;
                        //判断该配送单号是否所有的配送订单信息都已经出场
                        //查找配送装车订单信息
                        var transportCarList = Repository<TbTransportCarReport>.Query(p => p.DistributionCode == request.DistributionCode && p.DisEntOrderId != request.DisEntOrderId && p.State == 0);
                        if (transportCarList.Count() == 0)
                        {
                            distribution.FlowState = 6;//该条配送装车编号已经全部配送完成
                        }
                        break;
                    default:
                        break;
                }
                //添加运输流程信息
                transportFlow = MakeTransportFlowInfoNew(request);
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    if (request.FlowState == 5)
                    {
                        //修改配送装车信息
                        Repository<TbDistributionEnt>.Update(trans, distribution, true);
                    }
                    //修改配送装车订单信息
                    Repository<TbDistributionEntOrder>.Update(trans, distributionOrder, true);
                    //编辑车辆运输卸货统计信息
                    Repository<TbTransportCarReport>.Update(trans, transportCar, true);
                    //添加运输流程信息
                    Repository<TbTransportFlow>.Insert(trans, transportFlow, true);
                    //卸货完成生成站点卸货单
                    if (siteDischargeCargo != null)
                    {
                        Repository<TbSiteDischargeCargo>.Insert(trans, siteDischargeCargo, true);
                        Repository<TbSiteDischargeCargoDetail>.Insert(trans, siteDischargeCargoDetail, true);
                    }
                    trans.Commit();
                    //生成签收单(站点卸货-点击卸货完成)
                    if (request.FlowState == 4)
                    {
                        //获取站点卸货信息
                        var siteDcModel = Repository<TbSiteDischargeCargo>.First(p => p.DischargeCargoCode == DischargeCargoCode);
                        if (siteDcModel != null)
                        {
                            SiteDischargeCargoLogic _siteDischargeCargo = new SiteDischargeCargoLogic();
                            _siteDischargeCargo.DischargeCargoConfirm(siteDcModel.ID, true);
                        }

                    }
                    return AjaxResult.Success();
                }

            }
            catch (Exception)
            {
                return AjaxResult.Error();
            }
        }

        #region 半成品签收提醒

        public bool QsEndSendNotice(TbDistributionEntOrder distributionOrder, TbSiteDischargeCargo siteDischargeCargo)
        {
            try
            {
                //原材料到货入库不合格材料退回通知
                List<TbFormEarlyWarningNodeInfo> myMsgList = new List<TbFormEarlyWarningNodeInfo>();//Pc端推送
                List<TbSMSAlert> myDxList = new List<TbSMSAlert>();//App端
                var NoticeModel = Repository<TbNoticeNewsSetUp>.First(p => p.NoticeNewsCode == "XXTZ0008" && p.IsStart == 1);
                if (NoticeModel != null)
                {
                    //调用短信发送接口
                    //查找短信模板信息
                    var shortMessageTemplateModel = Repository<TbShortMessageTemplate>.First(p => p.TemplateCode == "MB0007");
                    if (shortMessageTemplateModel != null)
                    {
                        //获取工区、分部、经理部
                        DataTable dtParentOrg = ct.GetParentCompany(distributionOrder.SiteCode);
                        string ManagerDepartment = "";
                        string BranchCode = "";
                        string WorkAreaCode = "";
                        string SiteName = "";
                        if (dtParentOrg != null && dtParentOrg.Rows.Count > 0)
                        {
                            for (int org = 0; org < dtParentOrg.Rows.Count; org++)
                            {
                                if (dtParentOrg.Rows[org]["OrgType"].ToString() == "2")
                                {
                                    ManagerDepartment = dtParentOrg.Rows[org]["CompanyCode"].ToString();
                                }
                                else if (dtParentOrg.Rows[org]["OrgType"].ToString() == "3")
                                {
                                    BranchCode = dtParentOrg.Rows[org]["CompanyCode"].ToString();
                                }
                                else if (dtParentOrg.Rows[org]["OrgType"].ToString() == "4")
                                {
                                    WorkAreaCode = dtParentOrg.Rows[org]["CompanyCode"].ToString();
                                }
                                else if (dtParentOrg.Rows[org]["OrgType"].ToString() == "5")
                                {
                                    SiteName = dtParentOrg.Rows[org]["CompanyFullName"].ToString();
                                }
                            }
                        }

                        //短信内容
                        string content = shortMessageTemplateModel.TemplateContent;
                        var a = content.Replace("变量：站点", SiteName);
                        var b = a.Replace("变量：类型编码", siteDischargeCargo.TypeCode);
                        var ShortContent = b.Replace("变量：时间（年/月/日/时/分）", Convert.ToDateTime(siteDischargeCargo.InsertTime).ToString("g"));
                        string UserId = ct.GetUserId(distributionOrder.SiteContacts).Rows[0]["UserId"].ToString();
                        if (NoticeModel.App == 1)
                        {
                            if (!string.IsNullOrWhiteSpace(distributionOrder.SiteContactTel))
                            {
                                var myDxMsg = new TbSMSAlert()
                                {
                                    InsertTime = DateTime.Now,
                                    ManagerDepartment = ManagerDepartment,
                                    Branch = BranchCode,
                                    WorkArea = WorkAreaCode,
                                    Site = distributionOrder.SiteCode,
                                    UserCode = UserId,
                                    UserTel = distributionOrder.SiteContactTel,
                                    DXType = "",
                                    BusinessCode = shortMessageTemplateModel.TemplateCode,
                                    DataCode = siteDischargeCargo.DischargeCargoCode,
                                    ShortContent = ShortContent,
                                    FromCode = "TransportProcess",
                                    MsgType = "1"

                                };
                                myDxList.Add(myDxMsg);
                            }
                        }

                        if (NoticeModel.Pc == 1)
                        {
                            var myFormEarlyMsg = new TbFormEarlyWarningNodeInfo()
                            {
                                MenuCode = "TransportProcess",
                                EWNodeCode = NoticeModel.ID,
                                EWUserCode = UserId,
                                ProjectId = siteDischargeCargo.ProjectId,
                                EarlyWarningCode = NoticeModel.NoticeNewsCode,
                                EWFormDataCode = distributionOrder.ID,
                                CompanyCode = BranchCode,
                                WorkArea = WorkAreaCode,
                                SiteCode = distributionOrder.SiteCode,
                                MsgType = "1",
                                EWContent = ShortContent,
                                EWStart = 0,
                                EWTime = DateTime.Now,
                                ProcessFactoryCode = siteDischargeCargo.ProcessFactoryCode,
                                DataCode = siteDischargeCargo.DischargeCargoCode,
                                EarlyTitle = "【" + siteDischargeCargo.DischargeCargoCode + "】" + NoticeModel.NoticeNewsName
                            };
                            myMsgList.Add(myFormEarlyMsg);
                        }
                        for (int i = 0; i < myDxList.Count; i++)
                        {
                            //调用短信发送接口
                            //string dx = ct.ShortMessagePC(myDxList[i].UserTel, myDxList[i].ShortContent);
                            string dx = ct.ShortMessagePC("15756321745", myDxList[i].ShortContent);
                            var jObject1 = Newtonsoft.Json.Linq.JObject.Parse(dx);
                            var logmsg = jObject1["data"][0]["code"].ToString();
                            myDxList[i].DXType = logmsg;
                        }
                    }

                }
                using (DbTrans trans = Db.Context.BeginTransaction())
                {

                    if (myMsgList.Any())
                    {
                        Repository<TbFormEarlyWarningNodeInfo>.Insert(trans, myMsgList, true);
                    }
                    if (myDxList.Any())
                    {
                        Repository<TbSMSAlert>.Insert(trans, myDxList,true);
                    }
                    trans.Commit();
                    return true;
                }
            }
            catch (Exception)
            {

                throw;
            }

        }

        #endregion

        /// <summary>
        /// 运输流程确认
        /// </summary>
        /// <returns></returns>
        public AjaxResult FlowConfirm(TransportFlowRequest request)
        {
            try
            {
                if (request.FlowState > 5 || request.FlowState < 1)
                    return AjaxResult.Error("配送流程状态错误");
                //查找配送装车信息
                var distribution = Repository<TbDistributionEnt>.First(p => p.DistributionCode == request.DistributionCode);
                if (distribution == null)
                    return AjaxResult.Error("配送装车信息不存在");
                if (distribution.FlowState != request.FlowState)
                    return AjaxResult.Error("配送流程状态错误");
                if (!string.IsNullOrEmpty(request.fileIds))
                {
                    if (!string.IsNullOrEmpty(distribution.Enclosure))
                    {
                        distribution.Enclosure = distribution.Enclosure + "," + request.fileIds;
                    }
                    else
                    {
                        distribution.Enclosure = request.fileIds;
                    }
                }
                distribution.FlowState += 1;
                TbTransportCarReport transportCar = null;//车辆运输卸货统计
                TbTransportFlow transportFlow = null;//运输流程节点信息
                TbSiteDischargeCargo siteDischargeCargo = null;//卸货单
                List<TbSiteDischargeCargoDetail> siteDischargeCargoDetail = null;//卸货单明细
                request.Driver = distribution.Driver;
                request.VehicleCode = distribution.VehicleCode;
                if (request.FlowState != 1)
                {
                    transportCar = Repository<TbTransportCarReport>.First(p => p.DistributionCode == request.DistributionCode);
                    if (transportCar == null)
                        return AjaxResult.Error("运输信息不存在");
                    if (transportCar.IsProblem == "否")
                    {
                        //判断运输流程是否存在问题
                        var Any = Repository<TbArticle>.Any(p => p.Code == request.DistributionCode);
                        if (Any)
                        {
                            transportCar.IsProblem = "是";
                            transportCar.FlowState = 3;
                        }
                    }
                }
                switch (request.FlowState)
                {
                    case 1://出厂
                        //添加车辆运输卸货统计信息
                        transportCar = MakeTransportCarReport(distribution);
                        distribution.Examinestatus = "已确认";
                        break;
                    case 2://进场
                        transportCar.EnterSpaceTime = DateTime.Now;
                        break;
                    case 3://开始卸货
                        var endTime = DateTime.Now;
                        DateTime startTime = transportCar.EnterSpaceTime.Value;
                        var arry = (endTime - startTime).TotalMinutes.ToString().Split('.');
                        int waitTime = 0;
                        int.TryParse(arry[0], out waitTime);
                        transportCar.StartDischargeTime = endTime;
                        transportCar.WaitTime = waitTime;//等待卸货时间=开始卸货时间-运输到场时间
                        if (waitTime > 30)
                            transportCar.FlowState = 2;
                        break;
                    case 4://卸货完成
                        transportCar.EndDischargeTime = DateTime.Now;
                        //生成卸货单
                        var siteDischargeInfo = GetSiteDischargeInfo(distribution);
                        if (siteDischargeInfo.Item2 == null)
                            return AjaxResult.Error("配送装车单明细信息不存在");
                        siteDischargeCargo = siteDischargeInfo.Item1;
                        siteDischargeCargoDetail = siteDischargeInfo.Item2;
                        break;
                    case 5://出场
                        transportCar.OutSpaceTime = DateTime.Now;
                        transportCar.State = 1;
                        break;
                    default:
                        break;
                }

                //添加运输流程信息
                transportFlow = MakeTransportFlowInfo(request);
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //修改配送装车信息
                    Repository<TbDistributionEnt>.Update(trans, distribution, true);
                    //编辑车辆运输卸货统计信息
                    if (request.FlowState == 1)
                        Repository<TbTransportCarReport>.Insert(trans, transportCar, true);
                    else
                        Repository<TbTransportCarReport>.Update(trans, transportCar, true);

                    //添加运输流程信息
                    Repository<TbTransportFlow>.Insert(trans, transportFlow, true);
                    //当前流程为开始卸货流程时，判断是否存在预警信息，如果存在就把预警信息状态改为撤销
                    if (transportFlow.FlowType == 3)
                    {
                        Db.Context.FromSql("update TbFormEarlyWarningNodeInfo set EWStart=2 where MenuCode='DistributionEnt' and EWFormDataCode=" + distribution.ID + " and EWStart=0").SetDbTransaction(trans).ExecuteNonQuery();
                    }
                    //卸货完成生成站点卸货单
                    if (siteDischargeCargo != null)
                    {
                        Repository<TbSiteDischargeCargo>.Insert(trans, siteDischargeCargo, true);
                        Repository<TbSiteDischargeCargoDetail>.Insert(trans, siteDischargeCargoDetail, true);
                    }
                    trans.Commit();
                    return AjaxResult.Success();
                }

            }
            catch (Exception)
            {
                return AjaxResult.Error();
            }
        }

        /// <summary>
        /// 问题填报
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public AjaxResult ProblemReport(ProblemReportRequest request)
        {
            if (request == null)
                return AjaxResult.Warning("参数错误");
            var model = MapperHelper.Map<ProblemReportRequest, TbArticle>(request);
            model.Code = request.DistributionCode;
            model.DisEntOrderId = request.DisEntOrderId;
            model.FlowType = request.FlowState;
            model.CommentCount = 0;
            model.BrowseCount = 0;
            model.CollectCount = 0;
            model.Type = 2;
            model.InsertTime = DateTime.Now;
            try
            {
                //添加信息
                Repository<TbArticle>.Insert(model, true);
                return AjaxResult.Success();
            }
            catch (Exception ex)
            {
                return AjaxResult.Error();
            }
        }

        /// <summary>
        /// 获取数据列表(车辆运输卸货统计)
        /// </summary>
        public PageModel GetDataListForPage(TransportProcessRequest request)
        {
            try
            {
                string where = " where 1=1 ";
                string sbSiteCode = "";
                #region 数据权限新

                if (!string.IsNullOrWhiteSpace(request.ProjectId))
                {
                    where += " and Tb.ProjectId='" + request.ProjectId + "'";
                }
                if (request.FlowState > 0)
                {
                    where += " and Tb.FlowState=" + request.FlowState + "";
                }
                if (!string.IsNullOrEmpty(request.IsProblem))
                    where += " and Tb.FlowState in(" + request.IsProblem + ")";

                if (!string.IsNullOrWhiteSpace(request.DistributionCode))
                {
                    where += " and Tb.DistributionCode like '%" + request.DistributionCode + "%'";
                }
                if (!string.IsNullOrWhiteSpace(request.OrderCode))
                {
                    where += " and Tb.OrderCode like '%" + request.OrderCode + "%'";
                }
                if (!string.IsNullOrWhiteSpace(request.TypeCode))
                {
                    where += " and Tb.TypeCode like '%" + request.TypeCode + "%'";
                }
                if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
                {
                    where += " and Tb.ProcessFactoryCode=" + request.ProcessFactoryCode + "";
                }
                if (request.IsIndex)
                {
                    where += " and Tb.IsProblem ='是'";
                }
                if (!string.IsNullOrWhiteSpace(request.SiteCode))
                {
                    List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);
                    if (SiteList.Count > 0)
                    {
                        sbSiteCode = string.Join("','", SiteList);
                        where += " and Tb.SiteCode in('" + sbSiteCode + "')";
                    }
                }

                #endregion

                string sql = @"select * from 
                               (
                               	select 
                               	tcr.*,
                               	car.CarCph+tu.UserName AS CarCph,
                               	cp.CompanyFullName as SiteName,
                               	disEnt.Enclosure,
                               	disEntOld.ProcessFactoryCode 
                               	from TbTransportCarReport tcr
                                  left join TbCarInfo car on tcr.VehicleCode=car.CarCode
                                  left join TbCompany cp on tcr.SiteCode=cp.CompanyCode
                                  left JOIN
                                   (
                                   	select 
                                   	disEntOrder.ID,
                                   	disEnt.DistributionCode,
                                   	case when disEntOrder.Enclosure is null then disEnt.Enclosure 
                                   	else disEntOrder.Enclosure end as Enclosure 
                                   	from TbDistributionEnt disEnt 
                                   	left join TbDistributionEntOrder disEntOrder on disEnt.DistributionCode=disEntOrder.DistributionCode 
                                   ) disEnt on disEnt.DistributionCode=tcr.DistributionCode and disEnt.ID=tcr.DisEntOrderId
                                  left join TbDistributionEnt disEntOld on disEntOld.DistributionCode=tcr.DistributionCode
                                  left JOIN TbUser tu on disEntOld.Driver=tu.UserCode
                               ) Tb ";
                //参数化
                List<Parameter> parameter = new List<Parameter>();
                var model = Repository<TbSiteDischargeCargo>.FromSqlToPageTable(sql + where, parameter, request.rows, request.page, "DistributionCode", "desc");
                return model;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region Private

        /// <summary>
        /// 生成运输流程信息
        /// </summary>
        /// <returns></returns>
        private TbTransportFlow MakeTransportFlowInfo(TransportFlowRequest request)
        {
            //运输流程信息
            TbTransportFlow transportFlow = new TbTransportFlow()
            {
                DistributionCode = request.DistributionCode,
                VehicleCode = request.VehicleCode,
                Driver = request.Driver,
                Enclosure = request.Enclosure,
                FlowType = request.FlowState,
                ArticleId = 0,
                InsertTime = DateTime.Now
            };
            return transportFlow;
        }

        /// <summary>
        /// 生成运输流程信息
        /// </summary>
        /// <returns></returns>
        private TbTransportFlow MakeTransportFlowInfoNew(TransportFlowRequestNew request)
        {
            //运输流程信息
            TbTransportFlow transportFlow = new TbTransportFlow()
            {
                DistributionCode = request.DistributionCode,
                DisEntOrderId = request.DisEntOrderId,
                VehicleCode = request.VehicleCode,
                Driver = request.Driver,
                Enclosure = request.Enclosure,
                FlowType = request.FlowState,
                ArticleId = 0,
                InsertTime = DateTime.Now
            };
            return transportFlow;
        }

        /// <summary>
        /// 生成车辆运输卸货统计信息
        /// </summary>
        /// <returns></returns>
        private TbTransportCarReport MakeTransportCarReport(TbDistributionEnt distribution)
        {
            //运输流程信息
            TbTransportCarReport transportCarReport = new TbTransportCarReport()
            {
                OrderCode = distribution.OrderCode,
                DistributionCode = distribution.DistributionCode,
                SiteCode = distribution.SiteCode,
                TypeCode = distribution.TypeCode,
                TypeName = distribution.TypeName,
                PlanDistributionTime = distribution.PlanDistributionTime,
                LoadCompleteTime = distribution.LoadCompleteTime,
                VehicleCode = distribution.VehicleCode,
                ProjectId = distribution.ProjectId,
                LeaveFactoryTime = DateTime.Now,
                FlowState = 1,
                IsProblem = "否",
                State = 0
            };
            return transportCarReport;
        }

        /// <summary>
        /// 生成卸货单信息
        /// </summary>
        /// <param name="distributionCode">配送装车单信息</param>
        /// <returns>Item1:卸货单信息 Item2:卸货单明细信息</returns>
        private Tuple<TbSiteDischargeCargo, List<TbSiteDischargeCargoDetail>> GetSiteDischargeInfo(TbDistributionEnt distribution)
        {
            TbSiteDischargeCargo siteDischargeCargo = null;//卸货单
            List<TbSiteDischargeCargoDetail> siteDischargeCargoDetail = null;//卸货单明细
            siteDischargeCargo = MapperHelper.Map<TbDistributionEnt, TbSiteDischargeCargo>(distribution);
            siteDischargeCargo.DischargeCargoCode = CreateCode.GetTableMaxCode("ZDXH", "DischargeCargoCode", "TbSiteDischargeCargo");
            siteDischargeCargo.SumTotal = distribution.TotalAggregate;
            siteDischargeCargo.InsertTime = DateTime.Now;
            siteDischargeCargo.Examinestatus = "未发起";
            siteDischargeCargo.DischargeType = "NoComplete";//未完成
            siteDischargeCargo.Enclosure = "";
            siteDischargeCargo.Remark = "";
            //查找配送装车单明细
            var distributionEntItem = Repository<TbDistributionEntItem>.Query(p => p.DistributionCode == distribution.DistributionCode).ToList();
            if (!distributionEntItem.Any())
                return new Tuple<TbSiteDischargeCargo, List<TbSiteDischargeCargoDetail>>(siteDischargeCargo, siteDischargeCargoDetail);
            siteDischargeCargoDetail = MapperHelper.Map<TbDistributionEntItem, TbSiteDischargeCargoDetail>(distributionEntItem);
            siteDischargeCargoDetail.ForEach(x =>
            {
                x.DischargeCargoCode = siteDischargeCargo.DischargeCargoCode;
                var item = distributionEntItem.FirstOrDefault(p => p.ID == x.ID);
                x.SpecificationModel = item.Standard;
                x.MeasurementUnit = item.MeteringUnit;
                x.MeasurementUnitZl = Convert.ToDecimal(item.UnitWeight);
                x.ItemUseNum = Convert.ToDecimal(item.SingletonWeight);
                x.WeightSmallPlan = Convert.ToDecimal(item.WeightGauge);
                x.TestReportNo = item.TestReport;
            });
            return new Tuple<TbSiteDischargeCargo, List<TbSiteDischargeCargoDetail>>(siteDischargeCargo, siteDischargeCargoDetail);
        }

        /// <summary>
        /// 生成卸货单信息
        /// </summary>
        /// <param name="distributionCode">配送装车单信息</param>
        /// <returns>Item1:卸货单信息 Item2:卸货单明细信息</returns>
        private Tuple<TbSiteDischargeCargo, List<TbSiteDischargeCargoDetail>> GetSiteDischargeInfoNew(TbDistributionEnt distribution, TbDistributionEntOrder distributionOrder, string userCode)
        {
            TbSiteDischargeCargo siteDischargeCargo = new TbSiteDischargeCargo();//卸货单
            List<TbSiteDischargeCargoDetail> siteDischargeCargoDetail = new List<TbSiteDischargeCargoDetail>();//卸货单明细
            siteDischargeCargo.DistributionCode = distribution.DistributionCode;
            siteDischargeCargo.ProcessFactoryCode = distribution.ProcessFactoryCode;
            siteDischargeCargo.VehicleCode = distribution.VehicleCode;
            siteDischargeCargo.Driver = distribution.Driver;
            siteDischargeCargo.InsertUserCode = userCode;
            siteDischargeCargo.DischargeCargoCode = CreateCode.GetTableMaxCode("ZDXH", "DischargeCargoCode", "TbSiteDischargeCargo");
            siteDischargeCargo.TypeCode = distributionOrder.TypeCode;
            siteDischargeCargo.TypeName = distributionOrder.TypeName;
            siteDischargeCargo.UsePart = distributionOrder.UsePart;
            siteDischargeCargo.PlanDistributionTime = distributionOrder.PlanDistributionTime;
            siteDischargeCargo.DistributionAddress = distributionOrder.DistributionAddress;
            siteDischargeCargo.OrderCode = distributionOrder.OrderCode;
            siteDischargeCargo.DisEntOrderId = distributionOrder.ID;
            siteDischargeCargo.SiteCode = distributionOrder.SiteCode;
            siteDischargeCargo.Contacts = distributionOrder.SiteContacts;
            siteDischargeCargo.ContactWay = distributionOrder.SiteContactTel;
            siteDischargeCargo.SumTotal = Convert.ToDecimal(distributionOrder.SiteWeightTotal);
            siteDischargeCargo.ProjectId = distribution.ProjectId;
            siteDischargeCargo.InsertTime = DateTime.Now;
            siteDischargeCargo.Examinestatus = "未发起";
            siteDischargeCargo.DischargeType = "NoComplete";//未完成
            siteDischargeCargo.Enclosure = "";
            siteDischargeCargo.Remark = "";
            //查找配送装车单明细
            var distributionEntItem = Repository<TbDistributionEntItem>.Query(p => p.DistributionCode == distribution.DistributionCode && p.DisEntOrderIdentity == distributionOrder.DisEntOrderIdentity).ToList();
            if (!distributionEntItem.Any())
                return new Tuple<TbSiteDischargeCargo, List<TbSiteDischargeCargoDetail>>(siteDischargeCargo, siteDischargeCargoDetail);
            siteDischargeCargoDetail = MapperHelper.Map<TbDistributionEntItem, TbSiteDischargeCargoDetail>(distributionEntItem);
            siteDischargeCargoDetail.ForEach(x =>
            {
                x.DischargeCargoCode = siteDischargeCargo.DischargeCargoCode;
                var item = distributionEntItem.FirstOrDefault(p => p.ID == x.ID);
                x.SpecificationModel = item.Standard;
                x.MeasurementUnit = item.MeteringUnit;
                x.MeasurementUnitZl = Convert.ToDecimal(item.UnitWeight);
                x.ItemUseNum = Convert.ToDecimal(item.SingletonWeight);
                x.WeightSmallPlan = Convert.ToDecimal(item.WeightGauge);
                x.Number = Convert.ToInt32(item.GjNumber);
                x.TestReportNo = item.TestReport;
                x.OrderCode = item.OrderCode;
                x.DisEntOrderId = distributionOrder.ID;
                x.DisEntOrderItemId = item.ID;
                x.XhPackCount = item.ThisTimePackCount;
                x.XhNumber = item.Number;
            });
            return new Tuple<TbSiteDischargeCargo, List<TbSiteDischargeCargoDetail>>(siteDischargeCargo, siteDischargeCargoDetail);
        }

        #endregion

        #region 首页卸货过程时间展示
        /// <summary>
        /// 获取数据列表(车辆运输卸货统计)
        /// </summary>
        public DataTable GetCarReportGridJson(TransportProcessRequest request)
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
            string sql = @"select tcr.*,cp.CompanyFullName as SiteName,ci.CarCph,de.Enclosure from TbTransportCarReport  tcr
                           left join TbCompany cp on tcr.SiteCode=cp.CompanyCode
                           left join TbCarInfo ci on tcr.VehicleCode=ci.CarCode
                           left join TbDistributionEnt de on tcr.DistributionCode=de.DistributionCode
                           where (ISNULL(@ProjectId,'')='' or tcr.ProjectId=@ProjectId)
                           and (ISNULL(@ProcessFactoryCode,'')='' or de.ProcessFactoryCode=@ProcessFactoryCode)
                           and (tcr.IsProblem='是' or tcr.FlowState=2)  " + where + @"
                           order by id desc";
            DataTable dt = Db.Context.FromSql(sql)
               .AddInParameter("@ProjectId", DbType.String, request.ProjectId)
               .AddInParameter("@ProcessFactoryCode", DbType.String, request.ProcessFactoryCode)
               .ToDataTable();

            return dt;
        }

        #endregion

        #region 导出

        public DataTable GetExportList(TransportProcessRequest request)
        {
            try
            {
                string where = " where 1=1 ";
                string sbSiteCode = "";
                #region 数据权限新

                if (!string.IsNullOrWhiteSpace(request.ProjectId))
                {
                    where += " and Tb.ProjectId='" + request.ProjectId + "'";
                }
                if (request.FlowState > 0)
                {
                    where += " and Tb.FlowState=" + request.FlowState + "";
                }
                if (!string.IsNullOrEmpty(request.IsProblem))
                    where += " and Tb.FlowState in(" + request.IsProblem + ")";

                if (!string.IsNullOrWhiteSpace(request.DistributionCode))
                {
                    where += " and Tb.DistributionCode like '%" + request.DistributionCode + "%'";
                }
                if (!string.IsNullOrWhiteSpace(request.OrderCode))
                {
                    where += " and Tb.OrderCode like '%" + request.OrderCode + "%'";
                }
                if (!string.IsNullOrWhiteSpace(request.TypeCode))
                {
                    where += " and Tb.TypeCode like '%" + request.TypeCode + "%'";
                }
                if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
                {
                    where += " and Tb.ProcessFactoryCode=" + request.ProcessFactoryCode + "";
                }
                if (request.IsIndex)
                {
                    where += " and Tb.IsProblem ='是'";
                }
                if (!string.IsNullOrWhiteSpace(request.SiteCode))
                {
                    List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);
                    if (SiteList.Count > 0)
                    {
                        sbSiteCode = string.Join("','", SiteList);
                        where += " and Tb.SiteCode in('" + sbSiteCode + "')";
                    }
                }

                where += " order by DistributionCode desc";

                #endregion

                string sql = @"select * from 
                               (
                               	select 
                               	tcr.*,
                                case tcr.FlowState when 1 then '正常' else '等待超过30分钟' end as FlowStateShow,
                               	car.CarCph+tu.UserName AS CarCph,
                               	cp.CompanyFullName as SiteName,
                               	disEnt.Enclosure,
                               	disEntOld.ProcessFactoryCode 
                               	from TbTransportCarReport tcr
                                  left join TbCarInfo car on tcr.VehicleCode=car.CarCode
                                  left join TbCompany cp on tcr.SiteCode=cp.CompanyCode
                                  left JOIN
                                   (
                                   	select 
                                   	disEntOrder.ID,
                                   	disEnt.DistributionCode,
                                   	case when disEntOrder.Enclosure is null then disEnt.Enclosure 
                                   	else disEntOrder.Enclosure end as Enclosure 
                                   	from TbDistributionEnt disEnt 
                                   	left join TbDistributionEntOrder disEntOrder on disEnt.DistributionCode=disEntOrder.DistributionCode 
                                   ) disEnt on disEnt.DistributionCode=tcr.DistributionCode and disEnt.ID=tcr.DisEntOrderId
                                  left join TbDistributionEnt disEntOld on disEntOld.DistributionCode=tcr.DistributionCode
                                  left JOIN TbUser tu on disEntOld.Driver=tu.UserCode
                               ) Tb ";
                var data = Db.Context.FromSql(sql + where).ToDataTable();
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion


    }
}
