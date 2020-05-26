using Dos.ORM;
using PM.Business.Production;
using PM.Business.ShortMessage;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using PM.DataEntity.Distribution.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Business.Distribution
{
    /// <summary>
    /// 配送计划
    /// </summary>
    public class DistributionPlan
    {

        private readonly TbWorkOrderLogic _workOrderLogic = new TbWorkOrderLogic();//发送短信
        CensusdemoTask ct = new CensusdemoTask();

        #region 数据列表页

        /// <summary>
        /// 高级查询类型名称查询
        /// </summary>
        /// <returns></returns>
        public List<TbDistributionPlanInfo> TypeNameSelect()
        {
            var data = Db.Context.From<TbDistributionPlanInfo>()
                .Select(
                    TbDistributionPlanInfo._.TypeCode,
                    TbDistributionPlanInfo._.TypeName
                ).ToList();
            return data;
        }

        /// <summary>
        /// 数据列表信息查询
        /// </summary>
        /// <returns></returns>
        public PageModel GetGridJson(TbDistributionPlanInfoRequest request)
        {
            //组装查询语句
            #region 模糊搜索条件

            var where = new Where<TbDistributionPlanInfo>();
            if (!string.IsNullOrWhiteSpace(request.OrderCode))
            {
                where.And(p => p.OrderCode.Like(request.OrderCode));
            }
            if (!string.IsNullOrWhiteSpace(request.TypeName))
            {
                where.And(p => p.TypeName.Like(request.TypeName) || p.TypeCode.Like(request.TypeName));
            }
            if (!string.IsNullOrWhiteSpace(request.DistributionStart))
            {
                if (request.DistributionStart == "未配送")
                {
                    var distributionStart = new WhereClip("(TbDistributionPlanInfo.DistributionStart='未配送' and CONVERT(varchar(100), TbDistributionPlanInfo.DistributionTime, 23)>=CONVERT(varchar(100), GETDATE(), 23))");
                    where.And(distributionStart);
                }
                else if (request.DistributionStart == "超期未配送")
                {
                    var distributionStart = new WhereClip("(TbDistributionPlanInfo.DistributionStart='未配送' and CONVERT(varchar(100), TbDistributionPlanInfo.DistributionTime, 23)<CONVERT(varchar(100), GETDATE(), 23))");
                    where.And(distributionStart);
                }
                else
                {
                    if (request.DistributionStart=="已配送")
                    {
                        where.And(p => p.DistributionStart == "配送完成"||p.DistributionStart=="部分配送");
                    }
                    else
                    {
                        where.And(p => p.DistributionStart == request.DistributionStart);
                    }
                }
            }
            if (!string.IsNullOrWhiteSpace(request.IsOffline))
            {
                if (request.IsOffline == "系统记录")
                {
                    var IsOffline = new WhereClip("(TbWorkOrder.IsOffline=0 and TbDistributionPlanInfo.DistributionStart!='' and TbDistributionPlanInfo.DistributionStart!='未配送')");
                    where.And(IsOffline);
                }
                else
                {
                    var IsOffline = new WhereClip("(TbWorkOrder.IsOffline=1)");
                    where.And(IsOffline);
                }
            }
            if (request.HistoryMonth.HasValue)
            {
                string HistoryMonth = Convert.ToDateTime(request.HistoryMonth).ToString("yyyy-MM");
                var historyMonth = new WhereClip("(CONVERT(varchar(7), TbDistributionPlanInfo.DistributionTime, 120)='" + HistoryMonth + "')");
                where.And(historyMonth);

            }
            #endregion

            #region 数据权限新

            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where.And(p => p.ProcessFactoryCode == request.ProcessFactoryCode);
            }
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where.And(p => p.ProjectId == request.ProjectId);
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);
                if (SiteList.Count > 0)
                {
                    where.And(p => p.SiteCode.In(SiteList));
                }
            }
            if (!string.IsNullOrWhiteSpace(request.DistributionTimeStart))
            {
                if (request.DistributionTimeStart == "延迟配送")
                {
                    var distributionTime = new WhereClip("((SELECT (case when two.IsOffline=0 THEN( SELECT top 1 de.LoadCompleteTime FROM TbDistributionEntOrder deo LEFT JOIN TbDistributionEnt de ON deo.DistributionCode=de.DistributionCode WHERE deo.OrderCode LIKE '%'+two.OrderCode+'%' ORDER BY de.LoadCompleteTime desc ) ELSE (SELECT DeliveryCompleteTime FROM TbDistributionPlanInfo WHERE OrderCode=two.OrderCode)END) AS LoadCompleteTime FROM TbWorkOrder two WHERE two.OrderCode=TbDistributionPlanInfo.OrderCode)>CONVERT(varchar(100), TbDistributionPlanInfo.DistributionTime, 23))");
                    where.And(distributionTime);
                }
                else if (request.DistributionTimeStart == "提前配送")
                {
                    var distributionTime = new WhereClip("((SELECT (case when two.IsOffline=0 THEN( SELECT top 1 de.LoadCompleteTime FROM TbDistributionEntOrder deo LEFT JOIN TbDistributionEnt de ON deo.DistributionCode=de.DistributionCode WHERE deo.OrderCode LIKE '%'+two.OrderCode+'%' ORDER BY de.LoadCompleteTime desc ) ELSE (SELECT DeliveryCompleteTime FROM TbDistributionPlanInfo WHERE OrderCode=two.OrderCode)END) AS LoadCompleteTime FROM TbWorkOrder two WHERE two.OrderCode=TbDistributionPlanInfo.OrderCode)<CONVERT(varchar(100), TbDistributionPlanInfo.DistributionTime, 23))");
                    where.And(distributionTime);
                }
                else
                {
                    var distributionTime = new WhereClip("((SELECT (case when two.IsOffline=0 THEN( SELECT top 1 de.LoadCompleteTime FROM TbDistributionEntOrder deo LEFT JOIN TbDistributionEnt de ON deo.DistributionCode=de.DistributionCode WHERE deo.OrderCode LIKE '%'+two.OrderCode+'%' ORDER BY de.LoadCompleteTime desc ) ELSE (SELECT DeliveryCompleteTime FROM TbDistributionPlanInfo WHERE OrderCode=two.OrderCode)END) AS LoadCompleteTime FROM TbWorkOrder two WHERE two.OrderCode=TbDistributionPlanInfo.OrderCode)=CONVERT(varchar(100), TbDistributionPlanInfo.DistributionTime, 23))");
                    where.And(distributionTime);
                }
            }
            if (!string.IsNullOrWhiteSpace(request.SignState))
            {
                if (request.SignState == "未签收")
                {
                    var signState = new WhereClip("((SELECT (case when two.IsOffline=0 THEN (SELECT COUNT(1) AS number FROM TbDistributionEntOrder WHERE OrderCode LIKE '%'+two.OrderCode+'%') ELSE (SELECT COUNT(1) FROM TbSemiFinishedSign WHERE OrderCodeH=two.OrderCode)END) total FROM TbWorkOrder two where two.OrderCode=TbDistributionPlanInfo.OrderCode)!=0 and (SELECT (case when two.IsOffline=0 THEN(SELECT COUNT(1) AS number FROM TbDistributionEntOrder WHERE OrderCode LIKE '%'+two.OrderCode+'%'AND SignState='未签收') ELSE (SELECT COUNT(1) FROM TbSemiFinishedSign WHERE OrderCodeH=two.OrderCode AND OperateState='未签收')END) number FROM TbWorkOrder two WHERE two.OrderCode=TbDistributionPlanInfo.OrderCode)>0 and TbDistributionPlanInfo.DistributionStart!='' and TbDistributionPlanInfo.DistributionStart!='未配送')");
                    where.And(signState);
                }
                else
                {
                    var signState = new WhereClip("((SELECT (case when two.IsOffline=0 THEN (SELECT COUNT(1) AS number FROM TbDistributionEntOrder WHERE OrderCode LIKE '%'+two.OrderCode+'%') ELSE (SELECT COUNT(1) FROM TbSemiFinishedSign WHERE OrderCodeH=two.OrderCode)END) total FROM TbWorkOrder two where two.OrderCode=TbDistributionPlanInfo.OrderCode)!=0 and (SELECT (case when two.IsOffline=0 THEN(SELECT COUNT(1) AS number FROM TbDistributionEntOrder WHERE OrderCode LIKE '%'+two.OrderCode+'%'AND SignState='未签收') ELSE (SELECT COUNT(1) FROM TbSemiFinishedSign WHERE OrderCodeH=two.OrderCode AND OperateState='未签收')END) number FROM TbWorkOrder two WHERE two.OrderCode=TbDistributionPlanInfo.OrderCode)=0 and TbDistributionPlanInfo.DistributionStart!='' and TbDistributionPlanInfo.DistributionStart!='未配送')");
                    where.And(signState);
                }

            }
            if (!string.IsNullOrWhiteSpace(request.ProblemType))
            {
                if (request.ProblemType=="等待卸货超时")
                {
                    var signState = new WhereClip("((select Count(1) as IsWaitTimeCount from  TbTransportCarReport where DisEntOrderId is not null and OrderCode like '%'+TbDistributionPlanInfo.OrderCode+'%' and isnull(WaitTime,0)>30)>0)");
                    where.And(signState);
                }
                else if (request.ProblemType=="配送过程问题")
                {
                    var signState = new WhereClip("((select count(1) as IsProblemCount from TbTransportCarReport where DisEntOrderId is not null and  OrderCode like '%'+TbDistributionPlanInfo.OrderCode+'%' and IsProblem='是')>0)");
                    where.And(signState);
                }
                else if (request.ProblemType=="工区签收超时")
                {
                    var problemType = new WhereClip("((select COUNT(1) qscsnumber from (select case when sdc.OrderCode is null then sfs.OrderCodeH else sdc.OrderCode end OrderCode from TbSemiFinishedSign sfs left join (select sdc.DischargeCargoCode,case when sdc.OrderCode is null then dis.OrderCode else sdc.OrderCode end OrderCode from TbSiteDischargeCargo sdc left join TbDistributionEnt dis on sdc.DistributionCode=dis.DistributionCode) sdc on sfs.DischargeCargoCode=sdc.DischargeCargoCode where (sfs.DischargeCargoCode not in(select DischargeCargoCode from TbSiteDischargeCargo where DisEntOrderId is null) or sfs.DischargeCargoCode is null) and (DATEADD(dd,1,sfs.InsertTime)<sfs.SigninTime or (sfs.SigninTime is null and DATEADD(dd,1,sfs.InsertTime)<getdate()))) sfs where sfs.OrderCode like '%'+TbDistributionPlanInfo.OrderCode+'%')>0)");
                    where.And(problemType);
                }
            }
            #endregion

            try
            {
                var sql = Db.Context.From<TbDistributionPlanInfo>()
              .Select(
                        TbDistributionPlanInfo._.All
                      , TbCompany._.CompanyFullName.As("SiteName")
                      , TbSysDictionaryData._.DictionaryText.As("ProcessingStateNew")
                      , TbUser._.UserName
                      , TbSemiFinishedSign._.ID.As("SemiFinishedSignId")
                      , TbSemiFinishedSign._.Enclosure.As("EnclosureSF")
                      , TbOrderProgress._.FinishProcessingDateTime
                      , TbWorkOrder._.IsOffline)
                    .LeftJoin<TbCompany>((a, c) => a.SiteCode == c.CompanyCode)
                    .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                    .Where(TbCompany._.CompanyCode == TbDistributionPlanInfo._.ProcessFactoryCode), "ProcessFactoryName")
                    .AddSelect(Db.Context.From<TbSysDictionaryData>().Select(p => p.DictionaryText)
                    .Where(TbSysDictionaryData._.DictionaryCode == TbDistributionPlanInfo._.UrgentDegree && TbSysDictionaryData._.FDictionaryCode == "UrgentDegree"), "UrgentDegreeNew")
                    .LeftJoin<TbSysDictionaryData>((a, c) => a.ProcessingState == c.DictionaryCode && c.FDictionaryCode == "ProcessingState")
                    .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                    .LeftJoin<TbSemiFinishedSign>((a, c) => a.OrderCode == c.OrderCodeH)
                    .LeftJoin<TbOrderProgress>((a, c) => a.OrderCode == c.OrderCode)
                    .LeftJoin<TbWorkOrder>((a, c) => a.OrderCode == c.OrderCode)
                      .Where(where).OrderByDescending(d => d.ID);
                var ret = new PageModel();
                if (request.IsOutPut)
                {
                    ret.rows = sql.ToList<DistributionPlanInfoModel>();
                }
                else
                {
                    ret = sql.ToPageList<DistributionPlanInfoModel>(request.rows, request.page);
                }

                //获取加工订单配送完成时间
                var dataList = (List<DistributionPlanInfoModel>)ret.rows;
                if (dataList.Count > 0)
                {
                    string orderStr = string.Join("','", dataList.Select(p => p.OrderCode).ToList());
                    var completeTimeData = _workOrderLogic.GetCompleteTimeByOrder(orderStr);
                    var signStateData = _workOrderLogic.GetSignStateByOrder(orderStr);
                    dataList.ForEach(x =>
                    {
                        var completeTime = completeTimeData.FirstOrDefault(p => p.OrderCode == x.OrderCode);
                        if (string.IsNullOrEmpty(completeTime.LoadCompleteTime))
                            completeTime.LoadCompleteTime = "";
                        else
                            completeTime.LoadCompleteTime = DateTime.Parse(completeTime.LoadCompleteTime).ToString("yyyy-MM-dd");
                        if (!string.IsNullOrWhiteSpace(completeTime.LoadCompleteTime))
                        {

                            x.LoadCompleteTime = Convert.ToDateTime(completeTime.LoadCompleteTime);
                        }
                        else
                        {

                            x.LoadCompleteTime = null;
                        }
                        var signStated = signStateData.FirstOrDefault(p => p.OrderCode == x.OrderCode);
                        if (signStated.total == 0 || x.DistributionStart == "" || x.DistributionStart == "未配送")
                            x.SignState = "";
                        else if (signStated.number > 0)
                            x.SignState = "未签收";
                        else
                            x.SignState = "已签收";
                        var WaitTime = GetWaitTime(x.OrderCode);//等待卸货是否超时
                        if (Convert.ToInt32(WaitTime.Rows[0]["IsWaitTimeCount"]) > 0)
                            x.WaitTime = "超时";
                        else
                            x.WaitTime = "";
                        var IsProblem = GetIsProblem(x.OrderCode);//是否存在问题
                        if (Convert.ToInt32(IsProblem.Rows[0]["IsProblemCount"]) > 0)
                            x.IsProblem = "存在问题";
                        else
                            x.IsProblem = "";
                    });
                    ret.rows = dataList;
                }
                return ret;
            }
            catch (Exception ex)
            {
                throw;
            }
            //try
            //{
            //    string where = "";
            //    StringBuilder sbSiteCode = new StringBuilder();
            //    #region 数据权限新

            //    if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            //    {
            //        where += " and P.ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
            //    }
            //    if (!string.IsNullOrWhiteSpace(request.ProjectId))
            //    {
            //        where += " and P.ProjectId='" + request.ProjectId + "'";
            //    }
            //    if (!string.IsNullOrWhiteSpace(request.TypeName))
            //    {
            //        where += " and (P.TypeName like '%" + request.TypeName + "%' or P.TypeCode like '%" + request.TypeName + "%')";
            //    }
            //    if (!string.IsNullOrWhiteSpace(request.DistributionStart))
            //    {
            //        where += " and P.DistributionStart='" + request.DistributionStart + "'";
            //    }
            //    if (!string.IsNullOrWhiteSpace(request.OrderCode))
            //    {
            //        where += " and P.OrderCode like '%" + request.OrderCode + "%'";
            //    }
            //    if (!string.IsNullOrWhiteSpace(request.SiteCode))
            //    {
            //        List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);
            //        for (int i = 0; i < SiteList.Count; i++)
            //        {
            //            if (i == (SiteList.Count - 1))
            //            {
            //                sbSiteCode.Append("'" + SiteList[i] + "'");
            //            }
            //            else
            //            {
            //                sbSiteCode.Append("'" + SiteList[i] + "',");
            //            }
            //        }
            //        if (SiteList.Count > 0)
            //        {
            //            where += " and p.SiteCode in(" + sbSiteCode + ")";
            //        }

            //    }

            //    #endregion
            //    string sql = @"select p.ID,P.OrderCode,P.TypeCode,P.TypeName,P.UsePart,P.DistributionTiemStart,P.DistributionTime,U.LoadCompleteTime,P.DistributionStart,cp1.CompanyFullName as SiteName,cp2.CompanyFullName as WorkAreaName,cp3.CompanyFullName as BranchName,cp4.CompanyFullName as ProcessFactoryName,P.DistributionAdd,P.WeightTotal,U.ContactWay,ur1.UserName as ContactUser,P.InsertUserCode,ur2.UserName as InsertUserName,P.DeliveryCompleteTime from TbDistributionPlanInfo P
            //                    LEFT JOIN (select * from (select ROW_NUMBER() OVER ( PARTITION BY OrderCode order by DisEntId asc) rowNum ,* from (
            //                    select SUBSTRING(Tb.OrderCode,number,case when CHARINDEX(',',Tb.OrderCode+',',number)-number>0 then CHARINDEX(',',Tb.OrderCode+',',number)-number when CHARINDEX(',',Tb.OrderCode+',',number)-number<=0 then 0 end)  as OrderCode,Tb.ID as DisEntId,Tb.LoadCompleteTime,Tb.Contacts,Tb.ContactWay from (select disent.ID,case when disentorder.OrderCode is null then disent.OrderCode else disentorder.OrderCode end as OrderCode ,disent.LoadCompleteTime,case when disentorder.SiteContacts is null then disent.Contacts else disentorder.SiteContacts end Contacts,case when disentorder.SiteContactTel is null then disent.ContactWay else disentorder.SiteContactTel end ContactWay from TbDistributionEnt disent
            //                    left join TbDistributionEntOrder disentorder on disent.DistributionCode=disentorder.DistributionCode) Tb,master..spt_values s
            //                    where s.number >=1 and s.type = 'P' and SUBSTRING(','+Tb.OrderCode,s.number,1) = ',') Tb) Tb1 where Tb1.rowNum=1) U ON P.OrderCode=U.OrderCode
            //                    left join TbCompany cp1 on p.SiteCode=cp1.CompanyCode
            //                    left join TbCompany cp2 on cp2.CompanyCode=cp1.ParentCompanyCode
            //                    left join TbCompany cp3 on cp3.CompanyCode=cp2.ParentCompanyCode
            //                    left join TbCompany cp4 on cp4.CompanyCode=P.ProcessFactoryCode
            //                    left join TbUser ur1 on U.Contacts=ur1.UserCode
            //                    left join TbUser ur2 on ur2.UserCode=P.InsertUserCode 
            //                    LEFT JOIN TbWorkOrder two ON two.OrderCode=P.OrderCode
            //                    where 1=1 and two.IsOffline=0 --线上订单
            //                    and (ISNULL(@DistributionBegTime,'')='' OR CONVERT(VARCHAR(10),P.DistributionTime,120)>=@DistributionBegTime) 
            //                    and (ISNULL(@DistributionEndTime,'')='' OR CONVERT(VARCHAR(10),P.DistributionTime,120)<=@DistributionEndTime) " + where + "";

            //    //参数化
            //    List<Parameter> parameter = new List<Parameter>();
            //    parameter.Add(new Parameter("@DistributionBegTime", request.DistributionBegTime, DbType.DateTime, null));
            //    parameter.Add(new Parameter("@DistributionEndTime", request.DistributionEndTime, DbType.String, null));
            //    var model = Repository<TbDistributionPlanInfo>.FromSqlToPageTable(sql, parameter, request.rows, request.page, "OrderCode", "desc");
            //    return model;
            //}
            //catch (Exception)
            //{

            //    throw;
            //}
        }
        /// <summary>
        /// 获取等待时长
        /// </summary>
        /// <param name="OrderCode"></param>
        /// <returns></returns>
        public DataTable GetWaitTime(string OrderCode)
        {
            string sql = @"select Count(1) as IsWaitTimeCount from  TbTransportCarReport where OrderCode like '%" + OrderCode + "%' and isnull(WaitTime,0)>30 ";
            DataTable dt = Db.Context.FromSql(sql).ToDataTable();
            return dt;
        }
        /// <summary>
        /// 获取是否存在问题
        /// </summary>
        /// <param name="OrderCode"></param>
        /// <returns></returns>
        public DataTable GetIsProblem(string OrderCode)
        {
            string sql = @"select count(1) as IsProblemCount from TbTransportCarReport where OrderCode like '%" + OrderCode + "%' and IsProblem='是' ";
            DataTable dt = Db.Context.FromSql(sql).ToDataTable();
            return dt;
        }
        public Tuple<DataTable, DataTable, DataTable> GetDistributionForm(TbDistributionPlanInfoRequest request)
        {
            DataTable dt1 = GetJgcMonthPsQk(request);
            DataTable dt2 = GetJgcMonthPssFx1(request);
            DataTable dt3 = GetJgcMonthPssFx2(request);
            return new Tuple<DataTable, DataTable, DataTable>(dt1, dt2, dt3);
        }

        /// <summary>
        /// 获取生产进度展示报表数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public DataTable GetJgcMonthPsQk(TbDistributionPlanInfoRequest request)
        {
            string sqlwhere = SqlWhere(request);
            //            string sqlChilde = @"select * from (SELECT CompanyCode,CompanyFullName,(提前配送+延后配送+正常配送) as 配送总量,提前配送,延后配送,正常配送 FROM (select tb.CompanyCode,Tb.CompanyFullName,psType,isnull(WeightTotal,0) as WeightTotal from (select CompanyCode,CompanyFullName,'配送总量' as psType from TbCompany where OrgType=1
            //                          union all
            //                          select CompanyCode,CompanyFullName,'提前配送' as psType from TbCompany where OrgType=1
            //                          union all
            //                          select CompanyCode,CompanyFullName,'延后配送' as psType from TbCompany where OrgType=1
            //                          union all
            //                          select CompanyCode,CompanyFullName,'正常配送' as psType from TbCompany where OrgType=1) Tb
            //                          left join (
            //                          select tb.ProcessFactoryCode,tb.DistributionTiemStart,sum(tb.WeightTotal) as WeightTotal from (select OrderCode,DistributionStart,TypeCode,ProcessFactoryCode,SiteCode,ProjectId,case when DistributionTime<GETDATE() and DistributionTiemStart='暂无' then '延后配送'else DistributionTiemStart end as DistributionTiemStart,DistributionTime,convert(decimal(18,4),WeightTotal) as WeightTotal from TbDistributionPlanInfo
            //                          where 1=1  " + sqlwhere + @"
            //                          ) tb group by tb.ProcessFactoryCode,tb.DistributionTiemStart) Tb2  on Tb.CompanyCode=Tb2.ProcessFactoryCode and tb.psType=Tb2.DistributionTiemStart)
            //                           AS P
            //                           PIVOT 
            //                           (
            //                               SUM(WeightTotal) FOR 
            //                               p.psType IN ([配送总量],[提前配送],[延后配送],[正常配送])
            //                           ) AS T) Tb order by Tb.CompanyCode asc";
            string sqlChilde = @"select * from (SELECT CompanyCode,CompanyFullName,(提前配送+延后配送+正常配送) as 配送总量,提前配送,延后配送,正常配送 FROM (select tb.CompanyCode,Tb.CompanyFullName,psType,isnull(WeightTotal,0) as WeightTotal from (
                                 select CompanyCode,CompanyFullName,'配送总量' as psType from TbCompany where OrgType=1
                                 union all
                                 select CompanyCode,CompanyFullName,'提前配送' as psType from TbCompany where OrgType=1
                                 union all
                                 select CompanyCode,CompanyFullName,'延后配送' as psType from TbCompany where OrgType=1
                                 union all
                                 select CompanyCode,CompanyFullName,'正常配送' as psType from TbCompany where OrgType=1
                                 ) Tb
                                 left join (
                                 select tb.ProcessFactoryCode,tb.DistributionTiemStart,sum(tb.WeightGauge) as WeightTotal from (
                                 select * from (select dei.WeightGauge,deo.TypeCode,de.ProjectId,deo.SiteCode,de.ProcessFactoryCode,dpi.OrderCode,dpi.DistributionStart,dpi.DistributionTiemStart,dpi.DistributionTime,cp.CompanyFullName from TbDistributionEnt de
								 left join TbDistributionEntOrder deo on de.DistributionCode=deo.DistributionCode
                                 left join TbDistributionEntItem dei on deo.DistributionCode=dei.DistributionCode and deo.DisEntOrderIdentity=dei.DisEntOrderIdentity
                                 left join TbDistributionPlanDetailInfo dpdi on dei.WorkorderdetailId=dpdi.WorkorderdetailId
                                 left join TbDistributionPlanInfo dpi on dpdi.OrderCode=dpi.OrderCode
                                 left join TbCompany cp on de.ProcessFactoryCode=cp.CompanyCode
                                 where deo.UnloadingState='已完成') tb1 where 1=1 " + sqlwhere + @") tb 
                                 group by tb.ProcessFactoryCode,tb.DistributionTiemStart) Tb2  
                                 on Tb.CompanyCode=Tb2.ProcessFactoryCode and tb.psType=Tb2.DistributionTiemStart)
                                 AS P
                                 PIVOT 
                                 (
                                     SUM(WeightTotal) FOR 
                                     p.psType IN ([配送总量],[提前配送],[延后配送],[正常配送])
                                ) AS T) Tb order by Tb.CompanyCode asc";
            var dt = Db.Context.FromSql(sqlChilde).ToDataTable();
            return dt;
        }

        public DataTable GetJgcMonthPssFx1(TbDistributionPlanInfoRequest request)
        {

            string sqlwhere = SqlWhere(request);
            //            string sqlChilde = @"select ProcessFactoryCode,cp.CompanyFullName as ProcessFactoryName,sum(convert(decimal(18,4),WeightTotal)) as WeightTotal from TbDistributionPlanInfo dp
            //                          left join TbCompany cp on  dp.ProcessFactoryCode=cp.CompanyCode
            //                          where 1=1  " + sqlwhere + @"
            //                          group by ProcessFactoryCode,cp.CompanyFullName";
            string sqlChilde = @"select tb1.ProcessFactoryCode,tb1.CompanyFullName as ProcessFactoryName,sum(convert(decimal(18,4),tb1.WeightGauge)) as WeightTotal from (select dei.WeightGauge,deo.TypeCode,de.ProjectId,deo.SiteCode,de.ProcessFactoryCode,dpi.OrderCode,dpi.DistributionStart,dpi.DistributionTiemStart,dpi.DistributionTime,cp.CompanyFullName from TbDistributionEnt de
								 left join TbDistributionEntOrder deo on de.DistributionCode=deo.DistributionCode
                                 left join TbDistributionEntItem dei on deo.DistributionCode=dei.DistributionCode and deo.DisEntOrderIdentity=dei.DisEntOrderIdentity
                                 left join TbDistributionPlanDetailInfo dpdi on dei.WorkorderdetailId=dpdi.WorkorderdetailId
                                 left join TbDistributionPlanInfo dpi on dpdi.OrderCode=dpi.OrderCode
                                 left join TbCompany cp on de.ProcessFactoryCode=cp.CompanyCode
                                 where deo.UnloadingState='已完成') tb1 where 1=1 " + sqlwhere + @" group by tb1.ProcessFactoryCode,tb1.CompanyFullName";
            var dt1 = Db.Context.FromSql(sqlChilde).ToDataTable();
            return dt1;
        }

        public DataTable GetJgcMonthPssFx2(TbDistributionPlanInfoRequest request)
        {
            string sqlwhere = SqlWhere(request);
            //            string sqlChildeson = @"select ProcessFactoryCode,cp.CompanyFullName as ProcessFactoryName,SiteCode,cp1.CompanyFullName as SiteName,sum(convert(decimal(18,4),WeightTotal)) as WeightTotal from TbDistributionPlanInfo dp
            //                            left join TbCompany cp on  dp.ProcessFactoryCode=cp.CompanyCode
            //                            left join TbCompany cp1 on cp1.CompanyCode=dp.SiteCode
            //                            where 1=1 " + sqlwhere + @"
            //                            group by ProcessFactoryCode,cp.CompanyFullName,SiteCode,cp1.CompanyFullName";
            string sqlChildeson = @"select tb1.ProcessFactoryCode,tb1.ProcessFactoryName,tb1.SiteCode,tb1.SiteName,sum(convert(decimal(18,4),tb1.WeightGauge)) as WeightTotal from (select dei.WeightGauge,deo.TypeCode,de.ProjectId,deo.SiteCode,de.ProcessFactoryCode,dpi.OrderCode,dpi.DistributionStart,dpi.DistributionTiemStart,dpi.DistributionTime,cp.CompanyFullName as ProcessFactoryName,cp1.CompanyFullName as SiteName from TbDistributionEnt de
                                    left join TbDistributionEntOrder deo on de.DistributionCode=deo.DistributionCode
                                    left join TbDistributionEntItem dei on deo.DistributionCode=dei.DistributionCode and deo.DisEntOrderIdentity=dei.DisEntOrderIdentity
                                    left join TbDistributionPlanDetailInfo dpdi on dei.WorkorderdetailId=dpdi.WorkorderdetailId
                                    left join TbDistributionPlanInfo dpi on dpdi.OrderCode=dpi.OrderCode
                                    left join TbCompany cp on de.ProcessFactoryCode=cp.CompanyCode
                                    left join TbCompany cp1 on deo.SiteCode=cp1.CompanyCode
                                    where deo.UnloadingState='已完成') tb1 where 1=1 " + sqlwhere + @" group by tb1.ProcessFactoryCode,tb1.ProcessFactoryName,tb1.SiteCode,tb1.SiteName";
            var dt2 = Db.Context.FromSql(sqlChildeson).ToDataTable();
            return dt2;
        }


        public string SqlWhere(TbDistributionPlanInfoRequest request)
        {
            string sqlwhere = "";
            StringBuilder sbSiteCode = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(request.TypeCode))
            {
                sqlwhere += " and TypeCode='" + request.TypeCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.DistributionStart))
            {
                sqlwhere += " and DistributionStart='" + request.DistributionStart + "'";
            }
            if (request.DistributionBegTime != null)
            {
                sqlwhere += " and CONVERT(VARCHAR(10),DistributionTime,120)>='" + request.DistributionBegTime + "' ";
            }
            if (request.DistributionEndTime != null)
            {
                sqlwhere += " and CONVERT(VARCHAR(10),DistributionTime,120)<='" + request.DistributionEndTime + "' ";
            }
            #region 数据权限新

            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                sqlwhere += (" and ProcessFactoryCode='" + request.ProcessFactoryCode + "'");
            }
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                sqlwhere += (" and ProjectId='" + request.ProjectId + "'");
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
                    sqlwhere += " and SiteCode in(" + sbSiteCode + ")";
                }

            }

            #endregion

            return sqlwhere;
        }

        #endregion

        #region 查看
        /// <summary>
        /// 查看页面数据查询
        /// </summary>
        /// <returns></returns>
        public Tuple<DataTable, DataTable> GetFormJson(int keyValue)
        {
            try
            {
                var Plan = Db.Context.From<TbDistributionPlanInfo>()
                    .Select(
                        TbDistributionPlanInfo._.ID,
                        TbDistributionPlanInfo._.Enclosure,
                        TbDistributionPlanInfo._.DistributionPlanCode,
                        TbDistributionPlanInfo._.OrderCode,
                        TbDistributionPlanInfo._.DistributionTime,
                        TbDistributionPlanInfo._.UsePart,
                        TbDistributionPlanInfo._.TypeCode,
                        TbDistributionPlanInfo._.TypeName,
                        TbDistributionPlanInfo._.WeightTotal,
                        TbDistributionPlanInfo._.DistributionAdd,
                        TbUser._.UserName,
                        TbDistributionPlanInfo._.InsertTime,
                        TbDistributionPlanInfo._.Remark
                    ).LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                    .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                    .Where(TbCompany._.CompanyCode == TbDistributionPlanInfo._.SiteCode), "SiteName")
                    .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                    .Where(TbCompany._.CompanyCode == TbDistributionPlanInfo._.ProcessFactoryCode), "ProcessFactoryName")
                    .Where(p => p.ID == keyValue).ToDataTable();
                if (Plan.Rows.Count > 0)
                {
                    //查询联系人与联想方式
                    string lxrInfo = @"select top 1 TbUser.UserName as Contacts,SiteContactTel ContactWay from TbDistributionEntOrder
                                       left join TbUser on TbDistributionEntOrder.SiteContacts=TbUser.UserCode
                                       where OrderCode like '%" + Plan.Rows[0]["OrderCode"].ToString() + "%' order by TbDistributionEntOrder.ID asc";
                    var DtLxr = Db.Context.FromSql(lxrInfo).ToDataTable();
                    if (DtLxr.Rows.Count > 0)
                    {
                        DataColumn dc1 = new DataColumn("Contacts", typeof(string));
                        dc1.DefaultValue = DtLxr.Rows[0]["Contacts"].ToString();
                        Plan.Columns.Add(dc1);
                        DataColumn dc2 = new DataColumn("ContactWay", typeof(string));
                        dc2.DefaultValue = DtLxr.Rows[0]["ContactWay"].ToString();
                        Plan.Columns.Add(dc2);
                    }
                }
                var Details = Db.Context.From<TbDistributionPlanDetailInfo>()
                    .Select(
                    TbDistributionPlanDetailInfo._.All,
                    TbProcessingTechnology._.ProcessingTechnologyName.As("TbProcessingTechnologyValue")
                    ).LeftJoin<TbProcessingTechnology>((a, c) => a.ProcessingTechnology == c.ID)
                    .Where(p => p.OrderCode == Plan.Rows[0]["OrderCode"].ToString()).ToDataTable();
                return new Tuple<DataTable, DataTable>(Plan, Details);
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region 修改计划配送时间
        public AjaxResult SubmitForm(int ID, DateTime DistributionTime)
        {
            if (ID == 0 && DistributionTime == null)
                return AjaxResult.Warning("参数错误");
            //修改信息
            TbDistributionPlanInfo plan = new TbDistributionPlanInfo();
            plan.DistributionTime = DistributionTime;
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    Repository<TbDistributionPlanInfo>.Update(trans, plan, p => p.ID == ID);
                    trans.Commit();
                    //调用短信通知消息
                    UpdateDisTiemSendNotice(ID, DistributionTime);
                    return AjaxResult.Success();
                }
            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.ToString());
            }
        }
        #endregion

        #region 计划配送时间修改通知
        public bool UpdateDisTiemSendNotice(int ID, DateTime DistributionTime)
        {
            try
            {
                //配送计划时间修改通知 信息
                List<TbSMSAlert> myDxList = new List<TbSMSAlert>();//PC端推送
                List<TbFormEarlyWarningNodeInfo> myMsgList = new List<TbFormEarlyWarningNodeInfo>();//Pc端推送
                var NoticeModel = Repository<TbNoticeNewsSetUp>.First(p => p.NoticeNewsCode == "XXTZ0005" && p.IsStart == 1);
                if (NoticeModel != null)
                {
                    //查找消息模板信息
                    var shortMessageTemplateModel = Repository<TbShortMessageTemplate>.First(p => p.TemplateCode == "MB0016");
                    if (shortMessageTemplateModel != null)
                    {
                        var planModel = Repository<TbDistributionPlanInfo>.First(p => p.ID == ID);
                        if (planModel != null)
                        {
                            var orderModel = Repository<TbWorkOrder>.First(p => p.OrderCode == planModel.OrderCode);
                            if (orderModel != null)
                            {
                                //短信、消息内容
                                var content = shortMessageTemplateModel.TemplateContent;
                                var s = content.Replace("变量：加工订单编号、类型名称", planModel.OrderCode + "、" + planModel.TypeName);
                                var a = s.Replace("变量：原计划配送时间", planModel.DistributionTime.ToString());
                                var ShortContent = a.Replace("变量：修改后的配送时间", DistributionTime.ToString());
                                //获取配置的用户
                                List<CensusdemoTask.NotiecUser> listUser = ct.GetSendUser("DistributionPlan", NoticeModel.NoticeNewsCode, ID);
                                //获取订单录入人
                                string UserId = ct.GetUserId(orderModel.InsertUserCode).Rows[0]["UserId"].ToString();
                                if (!string.IsNullOrWhiteSpace(UserId))
                                {
                                    CensusdemoTask.NotiecUser nuModel = new CensusdemoTask.NotiecUser();
                                    nuModel.PersonnelSource = "Personnel";
                                    nuModel.PersonnelCode = UserId;
                                    listUser.Add(nuModel);
                                }
                                if (listUser.Any())
                                {
                                    DataTable dt = ct.GetParentCompany(orderModel.SiteCode);
                                    if (dt.Rows.Count > 0)
                                    {
                                        string ManagerDepartmentCode = "";
                                        string BranchCode = "";
                                        string WorkAreaCode = "";
                                        for (int i = 0; i < dt.Rows.Count; i++)
                                        {
                                            if (dt.Rows[i]["OrgType"].ToString() == "2")
                                            {
                                                ManagerDepartmentCode = dt.Rows[i]["CompanyCode"].ToString();
                                            }
                                            else if (dt.Rows[i]["OrgType"].ToString() == "3")
                                            {
                                                BranchCode = dt.Rows[i]["CompanyCode"].ToString();
                                            }
                                            else if (dt.Rows[i]["OrgType"].ToString() == "4")
                                            {
                                                WorkAreaCode = dt.Rows[i]["CompanyCode"].ToString();
                                            }
                                        }
                                        for (int u = 0; u < listUser.Count; u++)
                                        {
                                            if (NoticeModel.App == 1)
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
                                                        ManagerDepartment = ManagerDepartmentCode,
                                                        Branch = BranchCode,
                                                        WorkArea = WorkAreaCode,
                                                        Site = planModel.SiteCode,
                                                        UserCode = listUser[u].PersonnelCode,
                                                        UserTel = tel,
                                                        DXType = "",
                                                        BusinessCode = shortMessageTemplateModel.TemplateCode,
                                                        DataCode = planModel.OrderCode,
                                                        ShortContent = ShortContent,
                                                        FromCode = "DistributionPlan",
                                                        MsgType = "1"

                                                    };
                                                    myDxList.Add(myDxMsg);
                                                }
                                            }
                                            if (NoticeModel.Pc == 1)
                                            {
                                                var myFormEarlyMsg = new TbFormEarlyWarningNodeInfo()
                                                {

                                                    MenuCode = "DistributionPlan",
                                                    EWNodeCode = NoticeModel.ID,
                                                    EWUserCode = UserId,
                                                    ProjectId = planModel.ProjectId,
                                                    EarlyWarningCode = NoticeModel.NoticeNewsCode,
                                                    EWFormDataCode = planModel.ID,
                                                    CompanyCode = BranchCode,
                                                    WorkArea = WorkAreaCode,
                                                    SiteCode = planModel.SiteCode,
                                                    MsgType = "1",
                                                    EWContent = ShortContent,
                                                    EWStart = 0,
                                                    EWTime = DateTime.Now,
                                                    ProcessFactoryCode = "",
                                                    DataCode = planModel.OrderCode,
                                                    EarlyTitle = "【" + planModel.OrderCode + "】" + NoticeModel.NoticeNewsName
                                                };
                                                myMsgList.Add(myFormEarlyMsg);
                                            }
                                        }
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

        //原材料名称查询
        public List<TbRawMaterialArchives> MaterialNameSelect()
        {
            try
            {
                string sql = @"SELECT MaterialName FROM TbRawMaterialArchives
                                GROUP BY MaterialName";
                return Db.Context.FromSql(sql).ToList<TbRawMaterialArchives>();
            }
            catch (Exception)
            {

                throw;
            }
        }


        #region 统计报表

        public DataTable Img1(TbDistributionPlanInfoRequest request)
        {
            string where = "";
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where += " and a.ProjectId='" + request.ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where += " and a.ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);
                StringBuilder sbSiteCode = new StringBuilder();
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
                    where += " and a.SiteCode in(" + sbSiteCode + ")";
                }
            }
            string sql = @"select '未配送' as PsType,'' as PsTiemType,isnull(sum(a.WeightTotal),0) as WpsWeight,0 as WeightTotal from TbDistributionPlanInfo a where 1=1 " + where + @" and a.DistributionStart='未配送' 
and CONVERT(varchar(100), a.DistributionTime, 23)>=CONVERT(varchar(100), getdate(), 23)
union all
select '超期未配送' as PsType,'' as PsTiemType,isnull(sum(a.WeightTotal),0) as WpsWeight,0 as WeightTotal from TbDistributionPlanInfo a where 1=1 " + where + @" and a.DistributionStart='未配送' 
and CONVERT(varchar(100), a.DistributionTime, 23)<CONVERT(varchar(100), getdate(), 23)
union all
select Tb.PsType,Tb.PsTiemType,0 as WpsWeight,sum(Tb.WeightTotal) as WeightTotal from (select '已配送' as PsType,case when CONVERT(varchar(100), a.DistributionTime, 23)<CONVERT(varchar(100),min(c.LoadCompleteTime),23) then '延后' when CONVERT(varchar(100), a.DistributionTime, 23)>CONVERT(varchar(100),min(c.LoadCompleteTime),23) then '提前' else '正常' end PsTiemType,a.WeightTotal from TbDistributionPlanInfo a
left join TbDistributionEntItem b on a.OrderCode=b.OrderCode
left join TbDistributionEnt c on b.DistributionCode=c.DistributionCode
where 1=1 " + where + @" and a.DistributionStart='配送完成' and c.LoadCompleteTime is not null group by a.OrderCode,a.WeightTotal,a.DistributionTime
union all
select '已配送' as PsType,case when CONVERT(varchar(100), a.DistributionTime, 23)<CONVERT(varchar(100),a.DeliveryCompleteTime,23) then '延后' when CONVERT(varchar(100), a.DistributionTime, 23)>CONVERT(varchar(100),a.DeliveryCompleteTime,23) then '提前' else '正常' end PsTiemType,a.WeightTotal from TbDistributionPlanInfo a
left join TbWorkOrder b on a.OrderCode=b.OrderCode where 1=1 " + where + @" and IsOffline=1) Tb group by Tb.PsType,Tb.PsTiemType
union all
select '部分配送' as PsType,case when CONVERT(varchar(100), Tb1.DistributionTime, 23)<CONVERT(varchar(100),Tb1.LoadCompleteTime,23) then '延后' when CONVERT(varchar(100), Tb1.DistributionTime, 23)>CONVERT(varchar(100),Tb1.LoadCompleteTime,23) then '提前' else '正常' end PsTiemType,isnull(Tb1.WeightTotal,0)-isnull(Tb2.WeightGauge,0) as WpsWeight,Tb2.WeightGauge from (select a.OrderCode,a.WeightTotal,a.DistributionTime,min(c.LoadCompleteTime) as LoadCompleteTime from TbDistributionPlanInfo a
left join TbDistributionEntItem b on a.OrderCode=b.OrderCode
left join TbDistributionEnt c on b.DistributionCode=c.DistributionCode
where 1=1 " + where + @" and a.DistributionStart='部分配送' and c.LoadCompleteTime is not null group by a.OrderCode,a.WeightTotal,a.DistributionTime) Tb1
left join(select b.OrderCode,c.WeightGauge from TbDistributionPlanInfo a
left join TbDistributionPlanDetailInfo b on a.OrderCode=b.OrderCode
left join TbDistributionEntItem c on b.WorkOrderDetailId=c.WorkOrderDetailId
left join TbDistributionEntOrder d on c.DisEntOrderIdentity=d.DisEntOrderIdentity
where  a.DistributionStart='部分配送' and d.UnloadingState='已完成') Tb2 on Tb1.OrderCode=Tb2.OrderCode";
            DataTable dt = Db.Context.FromSql(sql).ToDataTable();
            decimal ypszl = 0;//应配送总量
            decimal wpsl = 0;//未配送量
            decimal cqwpsl = 0;//超期未配送量
            decimal ypsl = 0;//已配送量
            decimal yhpsl = 0;//延后配送量
            decimal tqpsl = 0;//提前配送量
            decimal zcpsl = 0;//正常配送量
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i]["PsType"].ToString() == "未配送")
                    {
                        wpsl += Convert.ToDecimal(dt.Rows[i]["WpsWeight"]);
                    }
                    else if (dt.Rows[i]["PsType"].ToString() == "超期未配送")
                    {
                        cqwpsl += Convert.ToDecimal(dt.Rows[i]["WpsWeight"]);
                    }
                    else if (dt.Rows[i]["PsType"].ToString() == "已配送")
                    {
                        if (dt.Rows[i]["PsTiemType"].ToString() == "提前")
                        {
                            tqpsl += Convert.ToDecimal(dt.Rows[i]["WeightTotal"]);
                        }
                        else if (dt.Rows[i]["PsTiemType"].ToString() == "延后")
                        {
                            yhpsl += Convert.ToDecimal(dt.Rows[i]["WeightTotal"]);
                        }
                        else //正常
                        {
                            zcpsl += Convert.ToDecimal(dt.Rows[i]["WeightTotal"]);
                        }
                    }
                    else //部分配送
                    {
                        if (dt.Rows[i]["PsTiemType"].ToString() == "提前")
                        {
                            tqpsl += Convert.ToDecimal(dt.Rows[i]["WeightTotal"]);
                            wpsl += Convert.ToDecimal(dt.Rows[i]["WpsWeight"]);
                        }
                        else if (dt.Rows[i]["PsTiemType"].ToString() == "延后")
                        {
                            yhpsl += Convert.ToDecimal(dt.Rows[i]["WeightTotal"]);
                            cqwpsl += Convert.ToDecimal(dt.Rows[i]["WpsWeight"]);
                        }
                        else //正常
                        {
                            zcpsl += Convert.ToDecimal(dt.Rows[i]["WeightTotal"]);
                            wpsl += Convert.ToDecimal(dt.Rows[i]["WpsWeight"]);
                        }
                    }
                }

                ypsl = yhpsl + tqpsl + zcpsl;
                ypszl = wpsl + cqwpsl + ypsl;
            }
            DataTable dtPs = new DataTable();
            DataColumn dc = null;
            dc = dtPs.Columns.Add("ypszl", Type.GetType("System.Decimal"));
            dc = dtPs.Columns.Add("wpsl", Type.GetType("System.Decimal"));
            dc = dtPs.Columns.Add("cqwpsl", Type.GetType("System.Decimal"));
            dc = dtPs.Columns.Add("ypsl", Type.GetType("System.Decimal"));
            dc = dtPs.Columns.Add("yhpsl", Type.GetType("System.Decimal"));
            dc = dtPs.Columns.Add("tqpsl", Type.GetType("System.Decimal"));
            dc = dtPs.Columns.Add("zcpsl", Type.GetType("System.Decimal"));
            DataRow newRow = dtPs.NewRow();
            newRow["ypszl"] = ypszl;
            newRow["wpsl"] = wpsl;
            newRow["cqwpsl"] = cqwpsl;
            newRow["ypsl"] = ypsl;
            newRow["yhpsl"] = yhpsl;
            newRow["tqpsl"] = tqpsl;
            newRow["zcpsl"] = zcpsl;
            dtPs.Rows.Add(newRow);
            return dtPs;

        }

        public DataTable Img2(TbDistributionPlanInfoRequest request)
        {
            string where = "";
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where += " and o.ProjectId='" + request.ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where += " and o.ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);
                StringBuilder sbSiteCode = new StringBuilder();
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
                    where += " and a.SiteCode in(" + sbSiteCode + ")";
                }
            }
            string sql = @"select TbType.Type,isnull(TbTypeInfo.WaitXhCsCount,0) as Count from (select '等待卸货超时' as Type
                          union all
                          select '配送过程问题' as Type
                          union all
                          select '工区签收超时' as Type) TbType
                          left join (select TbDdXhCs.Type,sum(TbDdXhCs.WaitXhCsCount) as WaitXhCsCount from (select '等待卸货超时' as Type,c.OrderCode,count(1) as WaitXhCsCount from  TbTransportCarReport a
                          left join TbDistributionEntOrder b on a.DisEntOrderId=b.ID
                          left join TbDistributionEntItem c on b.DisEntOrderIdentity=c.DisEntOrderIdentity 
						  left join TbWorkOrder o on c.OrderCode=o.OrderCode
						  where 1=1 " + where + @" and a.WaitTime>30 group by c.OrderCode) TbDdXhCs group by TbDdXhCs.Type
                          union all
                          select TbPsWt.Type,sum(TbPsWt.ProblemCount) as ProblemCount from (select '配送过程问题' as Type,a.OrderCode,count(1) as ProblemCount from (select a.DisEntOrderId,c.OrderCode from  TbTransportCarReport a
                          left join TbDistributionEntOrder b on a.DisEntOrderId=b.ID
                          left join TbDistributionEntItem c on b.DisEntOrderIdentity=c.DisEntOrderIdentity 
						  left join TbWorkOrder o on c.OrderCode=o.OrderCode
                          where 1=1 " + where + @"  and a.IsProblem='是' group by a.DisEntOrderId,c.OrderCode) a
                          left join TbArticle b on a.DisEntOrderId=b.DisEntOrderId group by a.OrderCode) TbPsWt group by TbPsWt.Type
                          union all
                          select TbQsCs.Type,sum(TbQsCs.QsCsCount) as QsCsCount from (select '工区签收超时' as Type,a.OrderCodeH,count(1) as QsCsCount from TbSemiFinishedSign a 
						  left join TbWorkOrder o on a.OrderCodeH=o.OrderCode
						  WHERE 1=1 " + where + @" and OrderCodeH is not null and (DATEADD(dd,1,a.InsertTime)<a.SigninTime or (a.SigninTime is null and DATEADD(dd,1,a.InsertTime)<getdate())) group by a.OrderCodeH
                          union all
                          select '工区签收超时' as Type,Tb.OrderCode,count(1) as QsCsCount  from (select a.ID,a.InsertTime,a.SigninTime,b.DisEntOrderId,d.OrderCode from TbSemiFinishedSign a
                          left join TbSiteDischargeCargo b on a.DischargeCargoCode=b.DischargeCargoCode
                          left join TbDistributionEntOrder c on b.DisEntOrderId=c.ID
                          left join TbDistributionEntItem d on c.DisEntOrderIdentity=d.DisEntOrderIdentity
						  left join TbWorkOrder o on c.OrderCode=o.OrderCode
                          where 1=1 " + where + @"  and a.DischargeCargoCode is not null group by a.ID,a.InsertTime,a.SigninTime,b.DisEntOrderId,d.OrderCode) Tb
                          where (DATEADD(dd,1,Tb.InsertTime)<Tb.SigninTime or (Tb.SigninTime is null and DATEADD(dd,1,Tb.InsertTime)<getdate()))
                          group by Tb.Ordercode) TbQsCs group by TbQsCs.Type) TbTypeInfo on TbType.Type=TbTypeInfo.Type";
            DataTable dt = Db.Context.FromSql(sql).ToDataTable();
            return dt;
        }

        public DataTable Img3(TbDistributionPlanInfoRequest request)
        {
            string where = "";
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where += " and a.ProjectId='" + request.ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where += " and a.ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);
                StringBuilder sbSiteCode = new StringBuilder();
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
                    where += " and a.SiteCode in(" + sbSiteCode + ")";
                }
            }
            string sql = @"select cp.CompanyCode as SiteCode,cp.CompanyFullName as SiteName,isnull(sum(TbPs.WeightTotal),0) as WeightTotal from (select a.SiteCode,sum(WeightTotal) as WeightTotal from  TbDistributionPlanInfo a
                           where 1=1 " + where + @" and a.DistributionStart='配送完成'
                           group by a.SiteCode
                           union all
                           select a.SiteCode,sum(c.WeightGauge) as WeightGauge from TbDistributionPlanInfo a
                           left join TbDistributionPlanDetailInfo b on a.OrderCode=b.OrderCode
                           left join TbDistributionEntItem c on b.WorkOrderDetailId=c.WorkOrderDetailId
                           left join TbDistributionEntOrder d on c.DisEntOrderIdentity=d.DisEntOrderIdentity
                           where 1=1 " + where + @" and a.DistributionStart='部分配送' and d.UnloadingState='已完成'
                           group by a.SiteCode) TbPs
                           left join TbCompany cp on TbPs.SiteCode=cp.CompanyCode
                           group by cp.CompanyCode,cp.CompanyFullName";
            DataTable dt = Db.Context.FromSql(sql).ToDataTable();
            return dt;
        }

        #endregion
    }
}
