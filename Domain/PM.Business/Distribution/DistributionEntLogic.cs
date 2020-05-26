using Dos.ORM;
using PM.Business.Production;
using PM.Business.RawMaterial;
using PM.Business.ShortMessage;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataAccess.Distribution;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PM.Business.Distribution
{
    /// <summary>
    /// 逻辑处理层
    /// 装车管理：配送装车
    /// </summary>
    public class DistributionEntLogic
    {
        //配送装车数据访问处理层类
        public readonly DistributionEntDA _deDA = new DistributionEntDA();
        private readonly TbWorkOrderLogic _workOrderLogic = new TbWorkOrderLogic();
        //发送短信
        CensusdemoTask ct = new CensusdemoTask();

        #region 查询数据

        /// <summary>
        /// 首页查询
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public PageModel GetAllOrBySearch(FPiCiXQPlan entity)
        {
            try
            {
                string where = " where 1=1 ";
                StringBuilder sbSiteCode = new StringBuilder();
                #region 数据权限新

                if (!string.IsNullOrWhiteSpace(entity.ProcessFactoryCode))
                {
                    where += " and Tb.ProcessFactoryCode='" + entity.ProcessFactoryCode + "'";
                }
                if (!string.IsNullOrWhiteSpace(entity.ProjectId))
                {
                    where += " and Tb.ProjectId='" + entity.ProjectId + "'";
                }
                if (!string.IsNullOrWhiteSpace(entity.OrderCode))
                {
                    where += " and Tb.OrderCode like'%" + entity.OrderCode + "%'";
                }
                if (!string.IsNullOrWhiteSpace(entity.TypeCode))
                {
                    where += " and Tb.TypeCode like'%" + entity.TypeCode + "%'";
                }
                if (!string.IsNullOrWhiteSpace(entity.CarCph))
                {
                    where += " and ci.CarCph like '%" + entity.CarCph + "%'";
                }
                if (entity.HistoryMonth.HasValue)
                {
                    where += " and YEAR(Tb.LoadCompleteTime)=" + entity.HistoryMonth.Value.Year + " and MONTH(Tb.LoadCompleteTime)=" + entity.HistoryMonth.Value.Month + "";
                }
                if (!string.IsNullOrWhiteSpace(entity.SiteCode))
                {
                    List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(entity.SiteCode, 5);
                    for (int i = 0; i < SiteList.Count; i++)
                    {
                        if (i == (SiteList.Count - 1))
                        {
                            sbSiteCode.Append("'" + SiteList[i] + "'");
                        }
                        else
                        {
                            sbSiteCode.Append("'" + SiteList[i] + "',");
                        }
                    }
                    if (SiteList.Count > 0)
                    {
                        where += " and Tb.SiteCode in(" + sbSiteCode + ")";
                    }
                }

                #endregion

                string sql = @"select Tb.*,cp1.CompanyFullName as ProcessFactoryName,cp2.CompanyFullName as SiteName,ci.CarCph,u1.UserName as CarUser,u2.UserName as ContactsName from (select row_number() over(order by disEnt.ID) as ID,disEnt.ID as DieEntId,disEntOrder.ID as DisEntOrderId,disEnt.DistributionCode,disEnt.ProjectId,disEnt.ProcessFactoryCode,disEnt.VehicleCode,disEnt.Driver,case when disEntOrder.SignState is null then disEnt.SignState else  disEntOrder.SignState end SignState,case when disEntOrder.UnloadingState is null then disEnt.UnloadingState else disEntOrder.UnloadingState end UnloadingState,disEnt.LoadCompleteTime,case when disEntOrder.OrderCode is null then disEnt.OrderCode else disEntOrder.OrderCode end as OrderCode,case when disEntOrder.TypeCode is null then disEnt.TypeCode else disEntOrder.TypeCode end as TypeCode,case when disEntOrder.TypeName is null then disEnt.TypeName else disEntOrder.TypeName end as TypeName,case when disEntOrder.UsePart is null then disEnt.UsePart else disEntOrder.UsePart end as UsePart,case when disEntOrder.SiteCode is null then disEnt.SiteCode else disEntOrder.SiteCode end SiteCode,case when disEntOrder.DistributionAddress is null then disEnt.DistributionAddress else disEntOrder.DistributionAddress end as DistributionAddress,case when disEntOrder.PlanDistributionTime is null then disEnt.PlanDistributionTime else disEntOrder.PlanDistributionTime end as PlanDistributionTime,case when disEntOrder.SiteContacts is null then disEnt.Contacts else disEntOrder.SiteContacts end as Contacts,case when disEntOrder.SiteContactTel is null then disEnt.ContactWay else disEntOrder.SiteContactTel end as ContactWay,case when disEntOrder.SiteWeightTotal is null then disEnt.TotalAggregate else disEntOrder.SiteWeightTotal end as TotalAggregate,disEntOrder.DisEntOrderIdentity,disEnt.PrintCount  from  TbDistributionEnt disEnt 
                               left join TbDistributionEntOrder disEntOrder on disEnt.DistributionCode=disEntOrder.DistributionCode) Tb
                               left join TbCompany cp1 on cp1.CompanyCode=Tb.ProcessFactoryCode
                               left join TbCompany cp2 on cp2.CompanyCode=Tb.SiteCode
                               left join TbCarInfo ci on Tb.VehicleCode=ci.CarCode
                               left join TbUser u1 on Tb.Driver=u1.UserCode
                               left join TbUser u2 on Tb.Contacts=u2.UserCode ";

                //参数化
                List<Parameter> parameter = new List<Parameter>();
                var model = Repository<TbDistributionPlanInfo>.FromSqlToPageTable(sql + where, parameter, entity.rows, entity.page, "DistributionCode", "desc");
                return model;
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// 以ID查询装车信息
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public Tuple<DataTable, DataTable, DataTable, DataTable> GetFormJson(int keyValue, int? keyValue1)
        {
            var ret = Db.Context.From<TbDistributionEnt>()
              .Select(
                      TbDistributionEnt._.All
                      , TbUser._.UserName
                      , TbCarInfo._.CarCph)
                    .LeftJoin<TbCompany>((a, c) => a.SiteCode == c.CompanyCode)
                    .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                    .Where(TbCompany._.CompanyCode == TbDistributionEnt._.ProcessFactoryCode), "ProcessFactoryName")
                    .AddSelect(Db.Context.From<TbUser>().Select(p => p.UserName)
                    .Where(TbUser._.UserCode == TbDistributionEnt._.Contacts), "ContactsName")
                    .AddSelect(Db.Context.From<TbUser>().Select(p => p.UserName)
                    .Where(TbUser._.UserCode == TbDistributionEnt._.Driver), "CarUser")
                    .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                    .LeftJoin<TbCarInfo>((a, c) => a.VehicleCode == c.CarCode)
                    .Where(p => p.ID == keyValue).ToDataTable();
            var where = new Where<TbDistributionEntOrder>();
            if (ret != null && ret.Rows.Count > 0)
            {
                if (!string.IsNullOrWhiteSpace(ret.Rows[0]["DistributionCode"].ToString()))
                {
                    string DistributionCode = ret.Rows[0]["DistributionCode"].ToString();
                    where.And(p => p.DistributionCode == DistributionCode);
                }
                if (keyValue1 != null && keyValue1 > 0)
                {
                    where.And(p => p.ID == keyValue1);
                }

            }
            var order = Db.Context.From<TbDistributionEntOrder>()
                .Select(
                        TbDistributionEntOrder._.All,
                        TbCompany._.CompanyFullName.As("SiteName"),
                        TbUser._.UserName.As("SiteContactsName")
                )
                .LeftJoin<TbCompany>((a, c) => a.SiteCode == c.CompanyCode)
                .LeftJoin<TbUser>((a, c) => a.SiteContacts == c.UserCode)
                .Where(where).ToDataTable();
            string where1 = "where 1=1";
            if (ret != null && ret.Rows.Count > 0)
            {
                if (!string.IsNullOrWhiteSpace(ret.Rows[0]["DistributionCode"].ToString()))
                {
                    string DistributionCode = ret.Rows[0]["DistributionCode"].ToString();
                    where1 += (" and disEntItem.DistributionCode='" + DistributionCode + "'");
                }
                if (order != null && order.Rows.Count > 0)
                {
                    string DisEntOrderIdentity = order.Rows[0]["DisEntOrderIdentity"].ToString();
                    where1 += (" and disEntItem.DisEntOrderIdentity='" + DisEntOrderIdentity + "'");
                }

            }
            //查找配送装车订单明细信息
            var sql = @"select disEntItem.*,wo.SiteCode,cp.CompanyFullName as SiteName,wo.TypeCode,wo.TypeName,pt.ProcessingTechnologyName,sd.DictionaryText as MeasurementUnitName from TbDistributionEntItem disEntItem
left join TbWorkOrder wo on disEntItem.OrderCode=wo.OrderCode
left join TbProcessingTechnology pt on disEntItem.ProcessingTechnology=pt.ID
left join TbSysDictionaryData sd on disEntItem.MeteringUnit=sd.DictionaryCode and sd.FDictionaryCode='Unit'
left join TbCompany cp on wo.SiteCode=cp.CompanyCode ";
            var items = Db.Context.FromSql(sql + where1).ToDataTable();
            //查找该条配送装车所有的订单信息
            var orderAll = Db.Context.From<TbDistributionEntOrder>()
                .Select(
                        TbDistributionEntOrder._.All,
                        TbCompany._.CompanyFullName.As("SiteName"),
                        TbUser._.UserName.As("SiteContactsName")
                )
                .LeftJoin<TbCompany>((a, c) => a.SiteCode == c.CompanyCode)
                .LeftJoin<TbUser>((a, c) => a.SiteContacts == c.UserCode)
                .Where(p => p.DistributionCode == ret.Rows[0]["DistributionCode"].ToString()).ToDataTable();
            return new Tuple<DataTable, DataTable, DataTable, DataTable>(ret, order, items, orderAll);
        }
        ///// <summary>
        ///// 以ID查询装车信息
        ///// </summary>
        ///// <param name="keyValue"></param>
        ///// <returns></returns>
        //public Tuple<DataTable, DataTable> GetFormJson(int keyValue)
        //{
        //    var ret = Db.Context.From<TbDistributionEnt>()
        //      .Select(
        //              TbDistributionEnt._.All
        //              , TbCompany._.CompanyFullName.As("SiteName")
        //              , TbUser._.UserName
        //              , TbCarInfo._.CarCph)
        //            .LeftJoin<TbCompany>((a, c) => a.SiteCode == c.CompanyCode)
        //            .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
        //            .Where(TbCompany._.CompanyCode == TbDistributionEnt._.ProcessFactoryCode), "ProcessFactoryName")
        //            .AddSelect(Db.Context.From<TbUser>().Select(p => p.UserName)
        //            .Where(TbUser._.UserCode == TbDistributionEnt._.Contacts), "ContactsName")
        //            .AddSelect(Db.Context.From<TbUser>().Select(p => p.UserName)
        //            .Where(TbUser._.UserCode == TbDistributionEnt._.Driver), "CarUser")
        //            .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
        //            .LeftJoin<TbCarInfo>((a, c) => a.VehicleCode == c.CarCode)
        //            .Where(p => p.ID == keyValue).ToDataTable();
        //    //查找明细信息
        //    var items = Db.Context.From<TbDistributionEntItem>()
        //        .Select(
        //                TbDistributionEntItem._.All,
        //                TbProcessingTechnology._.ProcessingTechnologyName.As("ProcessingTechnologyValue")
        //        )
        //        .LeftJoin<TbProcessingTechnology>((a, c) => a.ProcessingTechnology == c.ID)
        //        .Where(p => p.DistributionCode == ret.Rows[0]["DistributionCode"].ToString()).ToDataTable();
        //    return new Tuple<DataTable, DataTable>(ret, items);
        //}

        /// <summary>
        /// 配送计划弹窗
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageModel GetPSJHGridJson(DistributionPlanRequest request)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder sbSiteCode = new StringBuilder();
            sb.Append(" and a.ProcessingState='Finishing'");
            if (!string.IsNullOrWhiteSpace(request.keyword))
            {
                sb.Append(" a.DistributionPlanCode like '%" + request.keyword + "'%");
            }
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                sb.Append(" and a.ProjectId='" + request.ProjectId + "'");
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                sb.Append(" and a.ProcessFactoryCode='" + request.ProcessFactoryCode + "'");
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);
                for (int i = 0; i < SiteList.Count; i++)
                {
                    if (i == (SiteList.Count - 1))
                    {
                        sbSiteCode.Append("'" + SiteList[i] + "'");
                    }
                    else
                    {
                        sbSiteCode.Append("'" + SiteList[i] + "',");
                    }
                }
                if (SiteList.Count > 0)
                {
                    sb.Append(" and a.SiteCode in(" + sbSiteCode + ")");
                }
            }
            try
            {
                //参数化
                string sql = @"select a.*,b.PSAmount,c.CompanyFullName as ProcessFactoryName,d.CompanyFullName as SiteName from TbDistributionPlanInfo a 
left join (select OrderCode,SUM(PSAmount) PSAmount from TbDistributionPlanDetailInfo where 1=1 group by OrderCode) b on a.OrderCode=b.OrderCode 
left join TbCompany c on a.ProcessFactoryCode=c.CompanyCode 
left join TbCompany d on a.SiteCode=d.CompanyCode where 1=1 and b.PSAmount>0 ";
                List<Dos.ORM.Parameter> para = new List<Dos.ORM.Parameter>();
                var ret = Repository<TbFactoryBatchNeedPlan>.FromSqlToPageTable(sql + sb.ToString(), para, request.rows, request.page, "DistributionPlanCode", "desc");
                return ret;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// add明细弹窗
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public PageModel GetPSJHItemGridJson(DistributionPlanRequest request)
        {
            var sp = Regex.Split(request.PSJHCode, ",");
            var where = new Where<TbDistributionPlanDetailInfo>();
            where.And(d => d.OrderCode.In(sp));
            where.And(d => d.PSAmount != 0);
            if (!string.IsNullOrWhiteSpace(request.keyword))
            {
                where.And(d => d.ComponentName.Like(request.keyword) || d.MaterialName.Like(request.keyword));
            }
            try
            {
                var data = Db.Context.From<TbDistributionPlanDetailInfo>()
                    .Select(
                    TbDistributionPlanDetailInfo._.ID.As("DID"),
                    TbDistributionPlanDetailInfo._.PSAmount,
                    TbDistributionPlanDetailInfo._.ComponentName,
                    TbDistributionPlanDetailInfo._.DaetailWorkStrat,
                    TbDistributionPlanDetailInfo._.HeatNo,
                    TbDistributionPlanDetailInfo._.ItemUseNum,
                    TbDistributionPlanDetailInfo._.DaetailWorkStrat,
                    TbDistributionPlanDetailInfo._.LargePattern,
                    TbDistributionPlanDetailInfo._.Manufactor,
                    TbDistributionPlanDetailInfo._.MaterialCode,
                    TbDistributionPlanDetailInfo._.MaterialName,
                    //TbDistributionPlanDetailInfo._.MeasurementUnit,
                    TbDistributionPlanDetailInfo._.MeasurementUnitZl,
                    TbDistributionPlanDetailInfo._.Number,
                    TbDistributionPlanDetailInfo._.OrderCode,
                    TbDistributionPlanDetailInfo._.PackNumber,
                    TbDistributionPlanDetailInfo._.PackNumberSecond,
                    TbDistributionPlanDetailInfo._.QRCode,
                    TbDistributionPlanDetailInfo._.RevokeStart,
                    TbDistributionPlanDetailInfo._.SpecificationModel,
                    TbDistributionPlanDetailInfo._.TestReportNo,
                    TbDistributionPlanDetailInfo._.WeightSmallPlan,
                    TbDistributionPlanDetailInfo._.ProcessingTechnology,
                    TbSysDictionaryData._.DictionaryText.As("MeasurementUnit"),
                    TbProcessingTechnology._.ProcessingTechnologyName.As("ProcessingTechnologyValue"))
                    .LeftJoin<TbSysDictionaryData>((a, c) => a.MeasurementUnit == c.DictionaryCode)
                    .LeftJoin<TbProcessingTechnology>((a, c) => a.ProcessingTechnology == c.ID)
                    .Where(where).ToPageList(request.rows, request.page);
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 根据站点查询站点用户
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageModel GetSiteUser(TbUserRequest ur)
        {
            //string ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
            string addsql = "";
            if (!string.IsNullOrWhiteSpace(ur.keyword))
            {
                addsql += " and UserName like '%" + ur.keyword + "%'";
            }
            string sql = @"select UserCode,UserName,UserClosed as InsertUserCode,UserSex as CollarState,MobilePhone,IDNumber,UserId from TbUser where 1=1" + addsql;
            List<Dos.ORM.Parameter> para = new List<Dos.ORM.Parameter>();
            var pm = Repository<TbUser>.FromSqlToPageTable(sql, para, ur.rows, ur.page, "UserCode", "desc");
            return pm;
        }

        #endregion

        #region （新增、编辑）数据

        /// <summary>
        /// 新增数据
        /// </summary>
        public AjaxResult Insert(TbDistributionEnt model, List<TbDistributionEntOrder> orders, List<TbDistributionEntItem> items, bool isApp = false)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            model.Examinestatus = "未发起";
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //添加信息
                    Repository<TbDistributionEnt>.Insert(trans, model, isApp);
                    //添加配送装车订单信息
                    Repository<TbDistributionEntOrder>.Insert(trans, orders, isApp);
                    //添加明细信息
                    Repository<TbDistributionEntItem>.Insert(trans, items, isApp);
                    trans.Commit();
                    //调用短信通知消息
                    DisEntEndSendNotice(model, orders);
                    return AjaxResult.Success();
                }
            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.ToString());
            }
        }

        #region  配送装车完成通知
        public bool DisEntEndSendNotice(TbDistributionEnt model, List<TbDistributionEntOrder> orders)
        {
            try
            {
                List<TbSMSAlert> myDxList = new List<TbSMSAlert>();//App推送
                List<TbFormEarlyWarningNodeInfo> myMsgList = new List<TbFormEarlyWarningNodeInfo>();//Pc端推送
                var NoticeModel = Repository<TbNoticeNewsSetUp>.First(p => p.NoticeNewsCode == "XXTZ0006" && p.IsStart == 1);
                if (NoticeModel != null)
                {
                    //查找消息模板信息
                    var shortMessageTemplateModel = Repository<TbShortMessageTemplate>.First(p => p.TemplateCode == "MB0017");
                    if (shortMessageTemplateModel != null)
                    {
                        if (orders.Count > 0)
                        {
                            for (int i = 0; i < orders.Count; i++)
                            {
                                //获取经理部、分部、工区
                                TbRawMaterialMonthDemandPlanLogic rmdp = new TbRawMaterialMonthDemandPlanLogic();
                                DataTable dt = ct.GetParentCompany(orders[i].SiteCode);
                                if (dt.Rows.Count > 0)
                                {
                                    string ManagerDepartmentCode = "";
                                    string BranchCode = "";
                                    string WorkAreaCode = "";
                                    string SiteName = "";
                                    for (int o = 0; o < dt.Rows.Count; o++)
                                    {
                                        if (dt.Rows[o]["OrgType"].ToString() == "2")
                                        {
                                            ManagerDepartmentCode = dt.Rows[o]["CompanyCode"].ToString();
                                        }
                                        else if (dt.Rows[o]["OrgType"].ToString() == "3")
                                        {
                                            BranchCode = dt.Rows[o]["CompanyCode"].ToString();
                                        }
                                        else if (dt.Rows[o]["OrgType"].ToString() == "4")
                                        {
                                            WorkAreaCode = dt.Rows[o]["CompanyCode"].ToString();
                                        }
                                        else if (dt.Rows[o]["OrgType"].ToString() == "5")
                                        {
                                            SiteName = dt.Rows[o]["CompanyFullName"].ToString();
                                        }
                                    }
                                    //短信、通知内容  
                                    var content = shortMessageTemplateModel.TemplateContent;
                                    var s = content.Replace("变量：站点", SiteName);
                                    var ShortContent = s.Replace("变量：加工订单编号、类型名称", orders[i].OrderCode + "、" + orders[i].TypeName);
                                    string UserId = ct.GetUserId(orders[i].SiteContacts).Rows[0]["UserId"].ToString();
                                    if (NoticeModel.App == 1)
                                    {
                                        if (!string.IsNullOrWhiteSpace(orders[i].SiteContactTel))
                                        {
                                            var myDxMsg = new TbSMSAlert()
                                            {
                                                InsertTime = DateTime.Now,
                                                ManagerDepartment = ManagerDepartmentCode,
                                                Branch = BranchCode,
                                                WorkArea = WorkAreaCode,
                                                Site = orders[i].SiteCode,
                                                UserCode = UserId,
                                                UserTel = orders[i].SiteContactTel,
                                                DXType = "",
                                                BusinessCode = shortMessageTemplateModel.TemplateCode,
                                                DataCode = orders[i].DistributionCode,
                                                ShortContent = ShortContent,
                                                FromCode = "DistributionEnt",
                                                MsgType = "1"
                                            };
                                            myDxList.Add(myDxMsg);
                                        }
                                    }
                                    if (NoticeModel.Pc == 1)
                                    {
                                        var myFormEarlyMsg = new TbFormEarlyWarningNodeInfo()
                                        {

                                            MenuCode = "DistributionEnt",
                                            EWNodeCode = NoticeModel.ID,
                                            EWUserCode = UserId,
                                            ProjectId = model.ProjectId,
                                            EarlyWarningCode = NoticeModel.NoticeNewsCode,
                                            EWFormDataCode = orders[i].ID,
                                            CompanyCode = BranchCode,
                                            WorkArea = WorkAreaCode,
                                            SiteCode = orders[i].SiteCode,
                                            MsgType = "1",
                                            EWContent = ShortContent,
                                            EWStart = 0,
                                            EWTime = DateTime.Now,
                                            ProcessFactoryCode = "",
                                            DataCode = orders[i].DistributionCode,
                                            EarlyTitle = "【" + orders[i].DistributionCode + "】" + NoticeModel.NoticeNewsName
                                        };
                                        myMsgList.Add(myFormEarlyMsg);
                                    }
                                }
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
                    if (myDxList.Any())
                    {
                        //添加短信信息
                        Repository<TbSMSAlert>.Insert(trans, myDxList, true);
                    }
                    if (myMsgList.Any())
                    {
                        //添加表单预警信息
                        Repository<TbFormEarlyWarningNodeInfo>.Insert(trans, myMsgList, true);
                    }
                    trans.Commit();//提交事务 

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
        /// 修改数据
        /// </summary>
        public AjaxResult Update(TbDistributionEnt model, List<TbDistributionEntOrder> orders, List<TbDistributionEntItem> items, int? keyValue1, bool isApp = false)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            var anyRet = AnyInfo(model.ID);
            if (anyRet.state.ToString() != ResultType.success.ToString())
                return anyRet;
            try
            {
                int disEntOrderId = 0;
                string disEntOrderIdentity = "";
                if (keyValue1 != null && keyValue1 > 0)
                {
                    var monthOrder = Repository<TbDistributionEntOrder>.First(p => p.ID == keyValue1);
                    disEntOrderId = monthOrder.ID;
                    disEntOrderIdentity = monthOrder.DisEntOrderIdentity;
                }
                using (DbTrans trans = Db.Context.BeginTransaction())//使用事务
                {
                    //修改配送装车信息
                    Repository<TbDistributionEnt>.Update(trans, model, p => p.ID == model.ID, isApp);
                    if (disEntOrderId > 0 && !string.IsNullOrWhiteSpace(disEntOrderIdentity))
                    {
                        //删除历史配送装车订单信息
                        Repository<TbDistributionEntOrder>.Delete(trans, p => p.DistributionCode == model.DistributionCode && p.ID == disEntOrderId, isApp);
                        //添加配送装车订单信息
                        Repository<TbDistributionEntOrder>.Insert(trans, orders, isApp);
                        //删除历史配送装车订单明细信息
                        Repository<TbDistributionEntItem>.Delete(trans, p => p.DistributionCode == model.DistributionCode && p.DisEntOrderIdentity == disEntOrderIdentity, isApp);
                        //添加配送装车订单明细信息
                        Repository<TbDistributionEntItem>.Insert(trans, items, isApp);
                    }
                    else
                    {
                        //删除历史配送装车订单信息
                        Repository<TbDistributionEntOrder>.Delete(trans, p => p.DistributionCode == model.DistributionCode, isApp);
                        //添加配送装车订单信息
                        Repository<TbDistributionEntOrder>.Insert(trans, orders, isApp);
                        //删除历史配送装车订单明细信息
                        Repository<TbDistributionEntItem>.Delete(trans, p => p.DistributionCode == model.DistributionCode, isApp);
                        //添加配送装车订单明细信息
                        Repository<TbDistributionEntItem>.Insert(trans, items, isApp);
                    }



                    trans.Commit();//提交事务

                    return AjaxResult.Success();
                }
            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 确认装车
        /// </summary>
        /// <returns></returns>
        public AjaxResult ConfirmZC(int keyValue)
        {
            var monthPlan = Repository<TbDistributionEnt>.First(p => p.ID == keyValue);
            if (monthPlan == null)
                return AjaxResult.Warning("信息不存在");
            if (monthPlan.Examinestatus == "已确认")
                return AjaxResult.Warning("信息已确认完成,不能进行此操作");
            var monthPlanItem = Db.Context.From<TbDistributionEntItem>().Where(p => p.DistributionCode == monthPlan.DistributionCode).ToList();
            var planItemIDs = monthPlanItem.Select(p => p.PlanItemID).ToList();
            if (planItemIDs.Any())
            {
                for (var i = 0; i < monthPlanItem.Count; i++)
                {
                    if (monthPlanItem[i].Number != 0)
                    {
                        var PlanItem = Repository<TbDistributionPlanDetailInfo>.First(p => p.ID == monthPlanItem[i].PlanItemID);
                        if (monthPlanItem[i].Number > PlanItem.PackNumberSecond)
                        {
                            return AjaxResult.Warning("配送计划明细剩余件数不足，请检查...");
                        }
                    }
                }
            }
            return AjaxResult.Success(monthPlan);
        }

        /// <summary>
        /// 判断信息是否可操作
        /// </summary>
        /// <returns></returns>
        public AjaxResult AnyInfo(int keyValue)
        {
            var monthPlan = Repository<TbDistributionEnt>.First(p => p.ID == keyValue);
            if (monthPlan == null)
                return AjaxResult.Warning("信息不存在");
            if (monthPlan.FlowState != 0)
            {
                return AjaxResult.Warning("该配送装车信息已出厂,不能进行此操作");
            }
            //if (monthPlan.Examinestatus == "已确认")
            //    return AjaxResult.Warning("信息已确认完成,不能进行此操作");
            //if (monthPlan.UnloadingState == "已完成")
            //    return AjaxResult.Warning("信息已卸货完成,不能进行此操作");
            return AjaxResult.Success(monthPlan);
        }

        /// <summary>
        /// 打印增加次数
        /// </summary>
        /// <returns></returns>
        public int AddCount(int keyValue)
        {
            var uptModel = Db.Context.From<TbDistributionEnt>().Where(d => d.ID == keyValue).First();
            if (uptModel == null)
            {
                return 0;
            }
            uptModel.PrintCount = uptModel.PrintCount + 1;
            var count = Db.Context.Update<TbDistributionEnt>(uptModel);
            if (count > 0)
            {
                return 1;
            }
            return 0;
        }

        #endregion

        #region 删除数据

        /// <summary>
        /// 删除数据
        /// </summary>
        public AjaxResult Delete(int keyValue)
        {
            try
            {
                //判断信息是否存在
                var anyRet = AnyInfo(keyValue);
                if (anyRet.state.ToString() != ResultType.success.ToString())
                    return anyRet;
                //var dataList = Db.Context.From<TbDistributionEntItem>().Where(d => d.DistributionCode == ((TbDistributionEnt)anyRet.data).DistributionCode).ToList();
                ////查询配送计划详细信息
                //var anyRet2 = GetPlanDetail(dataList, false, true);
                //if (anyRet2.state.ToString() != ResultType.success.ToString())
                //    return anyRet;
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //删除配送装车信息
                    Repository<TbDistributionEnt>.Delete(trans, p => p.ID == keyValue);
                    //删除配送装车订单信息
                    Repository<TbDistributionEntOrder>.Delete(trans, p => p.DistributionCode == ((TbDistributionEnt)anyRet.data).DistributionCode);
                    //删除配送装车订单明细信息
                    Repository<TbDistributionEntItem>.Delete(trans, p => p.DistributionCode == ((TbDistributionEnt)anyRet.data).DistributionCode);
                    ////修改配送计划数量
                    //if (anyRet2.data != null)
                    //    Repository<TbDistributionPlanDetailInfo>.Update(trans, (List<TbDistributionPlanDetailInfo>)anyRet2.data);
                    trans.Commit();//提交事务

                    return AjaxResult.Success();
                }
            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.ToString());
            }
        }

        #endregion

        #region 扫码装车 APP

        /// <summary>
        /// 扫码装车
        /// </summary>
        /// <param name="id">加工订单明细Id</param>
        /// <returns></returns>
        public AjaxResult GetOrderItemByQRCode(int id)
        {
            //查找订单明细
            string sqlItem = GetPsOrderItemSQL() + " where Tb.WorkorderdetailId=" + id;
            var detailInfo = Db.Context.FromSql(sqlItem).ToFirst<WorkOrderDetailInfoResponse>();
            if (detailInfo == null)
                return AjaxResult.Error("二维码信息错误");
            //查找订单信息
            var orderInfo = Db.Context.From<TbWorkOrder>()
              .Select(
                      TbWorkOrder._.ID
                      , TbWorkOrder._.OrderCode
                      , TbWorkOrder._.TypeCode
                      , TbWorkOrder._.TypeName
                      , TbWorkOrder._.UsePart
                      , TbWorkOrder._.DistributionTime.As("PlanDistributionTime")
                      , TbWorkOrder._.DistributionAdd.As("DistributionAddress")
                      , TbWorkOrder._.SiteCode
                      , TbCompany._.CompanyFullName.As("SiteName"))
                    .LeftJoin<TbCompany>((a, c) => a.SiteCode == c.CompanyCode)
                    .Where(p => p.OrderCode == detailInfo.OrderCode).First<WorkOrderInfoResponse>();
            if (orderInfo == null)
                return AjaxResult.Error("二维码信息错误");
            DistributionEntResponse model = new DistributionEntResponse()
            {
                WorkOrderInfo = orderInfo,
                WorkOrderDetailInfo = detailInfo
            };
            return AjaxResult.Success(model);
        }

        #endregion

        #region Private

        /// <summary>
        /// 配送计划明细信息
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        private AjaxResult GetPlanDetail(List<TbDistributionEntItem> items, bool isUpdate = false, bool isDelete = false)
        {
            var planItemIDs = items.Where(p => p.DataType == 1).Select(p => p.PlanItemID).ToList();
            if (!planItemIDs.Any())
            {
                //扫码加载数据（加工订单明细）
                //加工订单明细数量
                foreach (var item in items)
                {
                    var orderAmount = item.OrderAmount;
                    var number = items.Where(p => p.PlanItemID == item.PlanItemID).Sum(p => p.Number);
                    if (orderAmount < number)
                        return AjaxResult.Error("配送数量有误");
                }
                return AjaxResult.Success();
            }
            var planDetails = Repository<TbDistributionPlanDetailInfo>.Query(p => p.ID.In(planItemIDs)).ToList();
            if (!planDetails.Any())
                return AjaxResult.Error("配送计划明细信息不存在");
            foreach (var x in planDetails)
            {
                var item = items.FirstOrDefault(p => p.PlanItemID == x.ID);
                var count = x.PSAmount - item.Number;
                if (isUpdate)
                    count = (x.PSAmount + item.PSAmountOld) - item.Number;
                if (isDelete)
                    count = x.PSAmount + item.Number;
                if (count < 0)
                    return AjaxResult.Error("配送数量有误");
                x.PSAmount = count;
            }
            return AjaxResult.Success(planDetails);
        }

        #endregion

        #region 获取该站点所属工区用户
        public PageModel GetWorkAreaUser(TbUserRequest request, string CompanyCode)
        {
            //参数化
            List<Parameter> parameter = new List<Parameter>();
            string where = " where 1=1 and ur.OrgId=(select ParentCompanyCode from TbCompany where CompanyCode='" + CompanyCode + "')";
            if (!string.IsNullOrWhiteSpace(request.keyword))
            {
                where += " and cp1.CompanyFullName like @keyword or cp.CompanyFullName like @keyword or u.UserName like @keyword";
                parameter.Add(new Parameter("@keyword", '%' + request.keyword + '%', DbType.String, null));
            }
            string sql = @"select distinct u.UserId,u.UserCode,u.UserName,ur.OrgId,cp1.CompanyFullName as BranchName,cp.CompanyFullName as WorkAreaName from TbUserRole ur 
left join TbUser u on ur.UserCode=u.UserId
left join TbCompany cp on cp.CompanyCode=ur.OrgId
left join TbCompany cp1 on cp1.CompanyCode=cp.ParentCompanyCode ";
            var model = Repository<TbUser>.FromSqlToPageTable(sql + where, parameter, request.rows, request.page, "UserId", "asc");
            return model;
        }
        #endregion

        #region 配送装车弹框信息
        /// <summary>
        /// 配送计划弹窗
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ProcessFactoryCode">加工厂编号</param>
        /// <param name="DisEntOrderCoder">已经选择了订单编号</param>
        /// <returns></returns>
        public PageModel GetJgcPsOrder(DistributionPlanRequest request, string ProcessFactoryCode)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder sbSiteCode = new StringBuilder();
            sb.Append(" where 1=1 ");
            if (!string.IsNullOrWhiteSpace(request.keyword))
            {
                sb.Append(" and (Tb.TypeCode like '%" + request.keyword + "%' or Tb.TypeName like '%" + request.keyword + "%' or Tb.SiteName like '%" + request.keyword + "%' or Tb.OrderCode like '%" + request.keyword + "%')");
            }
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                sb.Append(" and Tb.ProjectId='" + request.ProjectId + "'");
            }
            if (!string.IsNullOrWhiteSpace(ProcessFactoryCode))
            {
                sb.Append(" and Tb.ProcessFactoryCode='" + ProcessFactoryCode + "'");
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);
                for (int i = 0; i < SiteList.Count; i++)
                {
                    if (i == (SiteList.Count - 1))
                    {
                        sbSiteCode.Append("'" + SiteList[i] + "'");
                    }
                    else
                    {
                        sbSiteCode.Append("'" + SiteList[i] + "',");
                    }
                }
                if (SiteList.Count > 0)
                {
                    sb.Append(" and Tb.SiteCode in(" + sbSiteCode + ")");
                }
            }
            if (!string.IsNullOrWhiteSpace(request.DisEntOrderCoder))
            {
                sb.Append(" and Tb.OrderCode not in (" + request.DisEntOrderCoder + ")");
            }
            try
            {
                //参数化
                string sql = @"select Tb.* from (select op.OrderCode,sd.DictionaryText as ProcessingStateName,op.DistributionTime,op.TypeCode,op.TypeName,op.UsePart,op.DistributionAdd,op.ProjectId,op.SiteCode,cp.CompanyFullName as SiteName,op.ProcessFactoryCode from TbOrderProgress op
				               left join TbOrderProgressDetail opd on op.OrderCode=opd.OrderCode AND PackNumber>0
				               left join (select dei.OrderCode,dei.WorkorderdetailId,isnull(sum(dei.ThisTimePackCount),0) as YzcBs from TbDistributionEntItem dei group by dei.OrderCode,dei.WorkorderdetailId) TbZc on TbZc.OrderCode=opd.OrderCode and TbZc.WorkorderdetailId=opd.WorkorderdetailId
				               left join TbCompany cp on op.SiteCode=cp.CompanyCode
				               left join TbSysDictionaryData sd on sd.DictionaryCode=op.ProcessingState and sd.FDictionaryCode='ProcessingState'
                               LEFT JOIN TbWorkOrder two ON two.OrderCode=op.OrderCode
				               where opd.RevokeStart='正常' and (opd.DaetailWorkStrat='加工中' or opd.DaetailWorkStrat='加工完成')
                               AND two.IsOffline=0 --线上订单
				               and (case when AlreadyCompleted=WeightSmallPlan then CEILING(Number*1.0/PackNumber) else FLOOR(AlreadyCompleted/PackNumber/(MeasurementUnitZl*ItemUseNum)) end)-ISNULL(TbZc.YzcBs,0)>0
				               group by op.OrderCode,sd.DictionaryText,op.DistributionTime,op.TypeCode,op.TypeName,op.UsePart,op.DistributionAdd,op.ProjectId,op.SiteCode,cp.CompanyFullName,op.ProcessFactoryCode) Tb  ";
                List<Dos.ORM.Parameter> para = new List<Dos.ORM.Parameter>();
                var ret = Repository<TbWorkOrder>.FromSqlToPageTable(sql + sb.ToString(), para, request.rows, request.page, "OrderCode", "desc");
                return ret;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 配送装车订单明细弹窗
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public PageModel GetPsOrderItem(DistributionPlanRequest request, string OrderCode)
        {
            string OrderCodeNew = "";
            string[] sp = null;
            if (!string.IsNullOrWhiteSpace(OrderCode))
            {
                sp = OrderCode.Split(',');
                if (sp.Length > 0)
                {
                    for (int i = 0; i < sp.Length; i++)
                    {
                        if (i == sp.Length - 1)
                        {
                            OrderCodeNew += "'" + sp[i] + "'";
                        }
                        else
                        {
                            OrderCodeNew += "'" + sp[i] + "'" + ",";
                        }
                    }
                }

            }
            string where = " where 1=1 and (Tb.YjgBs-Tb.YzcBs)>0 ";
            where += " and Tb.OrderCode in(" + OrderCodeNew + ")";
            if (!string.IsNullOrWhiteSpace(request.keyword))
            {
                where += " and (Tb.OrderCode like '%" + request.keyword + "%' or Tb.ComponentName like '%" + request.keyword + "%' or Tb.MaterialCode like '%" + request.keyword + "%'  or Tb.MaterialName like '%" + request.keyword + "%' or Tb.TypeCode like '%" + request.keyword + "%' or Tb.TypeName like '%" + request.keyword + "%' or cp1.CompanyFullName like '%" + request.keyword + "%')";
            }
            try
            {
                //参数化
                string sql = GetPsOrderItemSQL();
                List<Dos.ORM.Parameter> para = new List<Dos.ORM.Parameter>();
                var ret = Repository<TbWorkOrder>.FromSqlToPageTable(sql + where.ToString(), para, request.rows, request.page, "OrderCode", "desc");
                return ret;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        ///配送装车订单明细SQL 
        /// </summary>
        /// <returns></returns>
        private string GetPsOrderItemSQL()
        {
            string sql = @"select Tb.OrderCode,Tb.SiteCode,cp1.CompanyFullName as SiteName,Tb.TypeCode,Tb.TypeName,Tb.DaetailWorkStrat,Tb.WorkorderdetailId,Tb.ComponentName,Tb.MaterialCode,Tb.MaterialName,Tb.SpecificationModel,Tb.MeasurementUnit,sd.DictionaryText as MeasurementUnitName,Tb.MeasurementUnitZl,Tb.ItemUseNum,Tb.Number,Tb.WeightSmallPlan,Tb.PackNumber,Tb.PackagesNumber,(Tb.YjgBs-Tb.YzcBs) as CanLoadingPackCount,case when Tb.YjgBs=Tb.PackagesNumber then Tb.Number-Tb.YzcJs else (Tb.YjgBs*wod.PackNumber-Tb.YzcJs) end  as CanLoadingJs,pt.ID as ProcessingTechnology,pt.ProcessingTechnologyName  as ProcessingTechnologyValue,Tb.AlreadyCompleted,wod.Manufactor,wod.HeatNo,wod.TestReportNo from (
                               select op.SiteCode,op.TypeCode,op.TypeName,opd.*,CEILING(Number*1.0/PackNumber) as PackagesNumber,case when AlreadyCompleted=WeightSmallPlan then CEILING(Number*1.0/PackNumber) else FLOOR(AlreadyCompleted/PackNumber/(MeasurementUnitZl*ItemUseNum)) end as YjgBs,isnull(TbZc.YzcBs,0) as YzcBs,ISNULL(TbZc.YzcJs,0) as YzcJs  from TbOrderProgress op
                               left join TbOrderProgressDetail opd on op.OrderCode=opd.OrderCode
                               left join (select dei.OrderCode,dei.WorkorderdetailId,isnull(sum(dei.ThisTimePackCount),0) as YzcBs,isnull(sum(dei.Number),0) as YzcJs from TbDistributionEntItem dei group by dei.OrderCode,dei.WorkorderdetailId) TbZc on TbZc.OrderCode=opd.OrderCode and TbZc.WorkorderdetailId=opd.WorkorderdetailId
                               left join TbCompany cp on op.SiteCode=cp.CompanyCode
                               where PackNumber>0 and opd.RevokeStart='正常' and (opd.DaetailWorkStrat='加工中' or opd.DaetailWorkStrat='加工完成')) Tb 
                               left join TbCompany cp1 on Tb.SiteCode=cp1.CompanyCode
                               left join TbWorkOrderDetail wod on Tb.WorkorderdetailId=wod.ID
                               left join TbSysDictionaryData sd on Tb.MeasurementUnit=sd.DictionaryCode and sd.FDictionaryCode='Unit' 
                               left join TbProcessingTechnology pt on wod.ProcessingTechnology=pt.ID ";
            return sql;
        }
        #endregion

        #region 导出

        public DataTable GetExportList(FPiCiXQPlan entity)
        {
            try
            {
                string where = " where 1=1 ";
                StringBuilder sbSiteCode = new StringBuilder();
                #region 数据权限新

                if (!string.IsNullOrWhiteSpace(entity.ProcessFactoryCode))
                {
                    where += " and Tb.ProcessFactoryCode='" + entity.ProcessFactoryCode + "'";
                }
                if (!string.IsNullOrWhiteSpace(entity.ProjectId))
                {
                    where += " and Tb.ProjectId='" + entity.ProjectId + "'";
                }
                if (!string.IsNullOrWhiteSpace(entity.OrderCode))
                {
                    where += " and Tb.OrderCode like'%" + entity.OrderCode + "%'";
                }
                if (!string.IsNullOrWhiteSpace(entity.TypeCode))
                {
                    where += " and Tb.TypeCode like'%" + entity.TypeCode + "%'";
                }
                if (!string.IsNullOrWhiteSpace(entity.CarCph))
                {
                    where += " and ci.CarCph like '%" + entity.CarCph + "%'";
                }
                if (entity.HistoryMonth.HasValue)
                {
                    where += " and YEAR(Tb.LoadCompleteTime)=" + entity.HistoryMonth.Value.Year + " and MONTH(Tb.LoadCompleteTime)=" + entity.HistoryMonth.Value.Month + "";
                }
                if (!string.IsNullOrWhiteSpace(entity.SiteCode))
                {
                    List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(entity.SiteCode, 5);
                    for (int i = 0; i < SiteList.Count; i++)
                    {
                        if (i == (SiteList.Count - 1))
                        {
                            sbSiteCode.Append("'" + SiteList[i] + "'");
                        }
                        else
                        {
                            sbSiteCode.Append("'" + SiteList[i] + "',");
                        }
                    }
                    if (SiteList.Count > 0)
                    {
                        where += " and Tb.SiteCode in(" + sbSiteCode + ")";
                    }
                }

                //排序
                where += " order by DistributionCode desc";

                #endregion

                string sql = @"select Tb.*,cp1.CompanyFullName as ProcessFactoryName,cp2.CompanyFullName as SiteName,ci.CarCph,u1.UserName as CarUser,u2.UserName as ContactsName from (select row_number() over(order by disEnt.ID) as ID,disEnt.ID as DieEntId,disEntOrder.ID as DisEntOrderId,disEnt.DistributionCode,disEnt.ProjectId,disEnt.ProcessFactoryCode,disEnt.VehicleCode,disEnt.Driver,case when disEntOrder.SignState is null then disEnt.SignState else  disEntOrder.SignState end SignState,case when disEntOrder.UnloadingState is null then disEnt.UnloadingState else disEntOrder.UnloadingState end UnloadingState,disEnt.LoadCompleteTime,case when disEntOrder.OrderCode is null then disEnt.OrderCode else disEntOrder.OrderCode end as OrderCode,case when disEntOrder.TypeCode is null then disEnt.TypeCode else disEntOrder.TypeCode end as TypeCode,case when disEntOrder.TypeName is null then disEnt.TypeName else disEntOrder.TypeName end as TypeName,case when disEntOrder.UsePart is null then disEnt.UsePart else disEntOrder.UsePart end as UsePart,case when disEntOrder.SiteCode is null then disEnt.SiteCode else disEntOrder.SiteCode end SiteCode,case when disEntOrder.DistributionAddress is null then disEnt.DistributionAddress else disEntOrder.DistributionAddress end as DistributionAddress,case when disEntOrder.PlanDistributionTime is null then disEnt.PlanDistributionTime else disEntOrder.PlanDistributionTime end as PlanDistributionTime,case when disEntOrder.SiteContacts is null then disEnt.Contacts else disEntOrder.SiteContacts end as Contacts,case when disEntOrder.SiteContactTel is null then disEnt.ContactWay else disEntOrder.SiteContactTel end as ContactWay,case when disEntOrder.SiteWeightTotal is null then disEnt.TotalAggregate else disEntOrder.SiteWeightTotal end as TotalAggregate,disEntOrder.DisEntOrderIdentity,disEnt.PrintCount  from  TbDistributionEnt disEnt 
                               left join TbDistributionEntOrder disEntOrder on disEnt.DistributionCode=disEntOrder.DistributionCode) Tb
                               left join TbCompany cp1 on cp1.CompanyCode=Tb.ProcessFactoryCode
                               left join TbCompany cp2 on cp2.CompanyCode=Tb.SiteCode
                               left join TbCarInfo ci on Tb.VehicleCode=ci.CarCode
                               left join TbUser u1 on Tb.Driver=u1.UserCode
                               left join TbUser u2 on Tb.Contacts=u2.UserCode ";

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
