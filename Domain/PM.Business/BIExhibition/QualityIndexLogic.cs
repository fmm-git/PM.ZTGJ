using PM.Common;
using PM.DataAccess.DbContext;
using System;
using System.Data;

namespace PM.Business.BIExhibition
{
    /// <summary>
    /// 质量指标
    /// </summary>
    public class QualityIndexLogic
    {
        //        /// <summary>
        //        /// 加工厂当月原材料到货分析
        //        /// </summary>
        //        /// <returns></returns>
        //        public DataTable JGCYCLArrival(PageSearchRequest psr,string PId) 
        //        {
        //            string addsql = "";
        //            if (!string.IsNullOrWhiteSpace(PId)) 
        //            {
        //                addsql += "ProjectId='" + PId + "'";
        //            }
        //            else if (!string.IsNullOrWhiteSpace(psr.ProjectId))
        //            {
        //                addsql += "ProjectId='" + psr.ProjectId + "'";
        //            }
        //            string sql = @"select a.BatchPlanNum,
        //isnull(a.BatchPlanTotal,0.0000) BatchPlanTotal,
        //isnull(b.InCount,0.0000) InCount,
        //a.ArrivalDate,isnull(b.InsertTime,a.ArrivalDate) InsertTime 
        //from TbFactoryBatchNeedPlan a left join TbInOrder b on a.BatchPlanNum=b.BatchPlanCode 
        //where DateDiff(month, b.InsertTime, GetDate()) = 0 and a.Examinestatus='审核完成' 
        //and b.Examinestatus='审核完成' and a." + addsql + " and b." + addsql;
        //            var ret = Db.Context.FromSql(sql).ToDataTable();
        //            DataTable dt = new DataTable();               //新建对象
        //            dt.Columns.Add("Ashi", typeof(decimal));   //新建第一列
        //            dt.Columns.Add("Tshi", typeof(decimal));      //新建第二列
        //            dt.Columns.Add("Yshi", typeof(decimal));      //新建第三列 
        //            if (ret.Rows.Count > 0)
        //            {          
        //                decimal Ashi = 0;
        //                decimal Tshi = 0;
        //                decimal Yshi = 0;
        //                for (var i = 0; i < ret.Rows.Count; i++) 
        //                {
        //                    var zongshu =Convert.ToDecimal(ret.Rows[i]["BatchPlanTotal"].ToString());
        //                    var daohuoshu = Convert.ToDecimal(ret.Rows[i]["InCount"].ToString());
        //                    var planTime = DateTime.Parse(ret.Rows[i]["ArrivalDate"].ToString());
        //                    var ArrivalTime = DateTime.Parse(ret.Rows[i]["InsertTime"].ToString());
        //                    if (planTime > ArrivalTime)
        //                    {
        //                        Tshi += daohuoshu;
        //                    }
        //                    else if (planTime < ArrivalTime) 
        //                    {
        //                        Yshi += daohuoshu;
        //                    }
        //                    else if (planTime == ArrivalTime) 
        //                    {
        //                        Ashi += daohuoshu;
        //                    }
        //                }
        //                dt.Rows.Add(Ashi, Tshi, Yshi); //新建第一行，并赋值 
        //                return dt;
        //            }
        //            dt.Rows.Add(0, 0, 0); //新建第一行，并赋值 
        //            return dt;
        //        }

        //        /// <summary>
        //        /// 当月原材料到货合格数量分析
        //        /// </summary>
        //        /// <returns></returns>
        //        public DataTable YCLArrivalQualified(PageSearchRequest psr, string PId)
        //        {
        //            string addsql = "";
        //            if (!string.IsNullOrWhiteSpace(PId))
        //            {
        //                addsql += "ProjectId='" + PId + "'";
        //            }
        //            else if (!string.IsNullOrWhiteSpace(psr.ProjectId))
        //            {
        //                addsql += "ProjectId='" + psr.ProjectId + "'";
        //            }
        //            string sql = "";
        //            var ret = Db.Context.FromSql(sql).ToDataTable();
        //            return ret;
        //        }

        //        /// <summary>
        //        /// 加工厂当月配送到站卸货时间分析
        //        /// </summary>
        //        /// <returns></returns>
        //        public DataTable JGCPSXHAnalysis(PageSearchRequest psr, string PId)
        //        {
        //            string addsql = "";
        //            if (!string.IsNullOrWhiteSpace(PId))
        //            {
        //                addsql += "ProjectId='" + PId + "'";
        //            }
        //            else if (!string.IsNullOrWhiteSpace(psr.ProjectId))
        //            {
        //                addsql += "ProjectId='" + psr.ProjectId + "'";
        //            }
        //            string sql = "";
        //            var ret = Db.Context.FromSql(sql).ToDataTable();
        //            return ret;
        //        }

        //        /// <summary>
        //        /// 构件加工厂历史订单分析
        //        /// </summary>
        //        /// <returns></returns>
        //        public DataTable JGCLSOrderAnalysis(PageSearchRequest psr, string PId)
        //        {
        //            string addsql = "";
        //            if (!string.IsNullOrWhiteSpace(PId))
        //            {
        //                addsql += "ProjectId='" + PId + "'";
        //            }
        //            else if (!string.IsNullOrWhiteSpace(psr.ProjectId))
        //            {
        //                addsql += "ProjectId='" + psr.ProjectId + "'";
        //            }
        //            string th = " and Examinestatus='已退回'";
        //            string sh = " and Examinestatus='审核完成'";
        //            string sql = @"select * from (
        //select Tb.*,
        //case when Tb1.num is null then 0 else Tb1.num end Count1,
        //case when Tb2.num is null then 0 else Tb2.num end Count2 
        //from (
        //select years,months from(Select year(InsertTime) years,Month(InsertTime) months FROM TbOrderProgress 
        //where (DateDiff(month, InsertTime, GetDate()) >= 0 and DateDiff(month, InsertTime, GetDate()) <= 6) 
        //and " + addsql + th + @" 
        //group by year(InsertTime),Month(InsertTime) 
        //union all
        //select year(InsertTime) years,Month(InsertTime) months FROM TbProblemOrder 
        //where (DateDiff(month, InsertTime, GetDate()) >= 0 and DateDiff(month, InsertTime, GetDate()) <= 6) 
        //and " + addsql + sh + @" 
        //group by year(InsertTime),Month(InsertTime)) Tb group by years,months) Tb
        //left join (
        //Select year(InsertTime) years,Month(InsertTime) months,COUNT(*) num FROM TbOrderProgress 
        //where (DateDiff(month, InsertTime, GetDate()) >= 0 and DateDiff(month, InsertTime, GetDate()) <= 6) 
        //and " + addsql + th + @" 
        //group by year(InsertTime),Month(InsertTime)) Tb1 on Tb.years=Tb1.years and Tb.months=Tb1.months
        //left join (
        //select year(InsertTime) years,Month(InsertTime) months,COUNT(*) num FROM TbProblemOrder 
        //where (DateDiff(month, InsertTime, GetDate()) >= 0 and DateDiff(month, InsertTime, GetDate()) <= 6) 
        //and " + addsql + sh + @" 
        //group by year(InsertTime),Month(InsertTime)) Tb2 on Tb.years=Tb2.years and Tb.months=Tb2.months) bb 
        //order by bb.years,bb.months ";
        //            var ret = Db.Context.FromSql(sql).ToDataTable();
        //            if (ret.Rows.Count > 0)
        //            {
        //                if (ret.Rows.Count == 7)
        //                {
        //                    return ret;
        //                }
        //                else
        //                {
        //                    var kt = DateTime.Now.AddMonths(-6).ToString("yyyy-MM-dd");
        //                    var jt = DateTime.Now.ToString("yyyy-MM-dd");
        //                    int kyue = Convert.ToInt32(kt.Substring(5, 2));
        //                    int jyue = Convert.ToInt32(jt.Substring(5, 2));
        //                    DataTable dt = new DataTable();               //新建对象
        //                    dt.Columns.Add("years", typeof(string));   //新建第一列
        //                    dt.Columns.Add("months", typeof(int));      //新建第二列
        //                    dt.Columns.Add("Count1", typeof(int));
        //                    dt.Columns.Add("Count2", typeof(int));
        //                    var num = kyue;
        //                    for (var i = 0; i < 7; i++) 
        //                    {
        //                        dt.Rows.Add("", num, 0, 0);
        //                        if (num == 12)
        //                        {
        //                            num = 1;
        //                        }
        //                        num++;
        //                    }
        //                    for (var i = 0; i < dt.Rows.Count; i++) 
        //                    {
        //                        for (var j = 0; j < ret.Rows.Count; j++)
        //                        {
        //                            if (Convert.ToInt32(dt.Rows[i]["months"].ToString()) == Convert.ToInt32(ret.Rows[j]["months"].ToString())) 
        //                            {
        //                                dt.Rows[i]["Count1"] = ret.Rows[j]["Count1"].ToString();
        //                                dt.Rows[i]["Count2"] = ret.Rows[j]["Count2"].ToString();
        //                            }
        //                        }
        //                    }
        //                    return dt;
        //                }
        //            }
        //            else 
        //            {
        //                return ret;
        //            }
        //        }

        /// <summary>
        /// 加工厂当月原材料到货分析
        /// </summary>
        /// <returns></returns>
        public DataTable JGCYCLArrival(string OrgType, string ProjectId, string HistoryMonth1)
        {
            if (string.IsNullOrWhiteSpace(OrgType))
            {
                OrgType = OperatorProvider.Provider.CurrentUser.OrgType;
            }
            if (OrgType == "1") //加工厂
            {
                ProjectId = "";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(ProjectId))
                {
                    ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
                }
            }
            string sql = @"select * from (SELECT CompanyCode,CompanyFullName,(按时供货量+延迟供货量+未供货量) as dhzl,按时供货量 as tqdh,延迟供货量 as ycdh,未供货量 as asdh FROM (select TbJgc.CompanyCode,TbJgc.CompanyFullName,TbJgc.dhType,isnull(sum(TbGh.HasSupplierTotal),0) as HasSupplierTotal from (
                            select CompanyCode,CompanyFullName,'到货总量' as dhType from TbCompany where OrgType=1
                            union all
                            select CompanyCode,CompanyFullName,'按时供货量' as dhType from TbCompany where OrgType=1
                            union all
                            select CompanyCode,CompanyFullName,'延迟供货量' as dhType from TbCompany where OrgType=1
                            union all
                            select CompanyCode,CompanyFullName,'未供货量' as dhType from TbCompany where OrgType=1) TbJgc
                            left join(
                            select sup.ProcessFactoryCode,sup.BatchPlanTotal,sup.HasSupplierTotal,'按时供货量' as dhType from TbSupplyList sup
                            where ((sup.SupplyCompleteTime is not null and CONVERT(varchar(100), sup.SupplyCompleteTime, 23)<=CONVERT(varchar(100), sup.SupplyDate, 23)) or (sup.SupplyCompleteTime is null and CONVERT(varchar(100), sup.SupplyDate, 23)>=CONVERT(varchar(100), GETDATE(), 23) and sup.HasSupplierTotal>0))
                            and (ISNULL(@ProjectId,'')='' or sup.ProjectId=@ProjectId) and (ISNULL(@HistoryMonth1,'')='' or CONVERT(varchar(7) ,sup.SupplyDate, 120)=@HistoryMonth1) 
                            union all
                            select sup.ProcessFactoryCode,sup.BatchPlanTotal,sup.HasSupplierTotal,'延迟供货量' as dhType from TbSupplyList sup
                            where (sup.SupplyCompleteTime is not null and CONVERT(varchar(100), sup.SupplyCompleteTime, 23)>CONVERT(varchar(100), sup.SupplyDate, 23) or (sup.SupplyCompleteTime is null and CONVERT(varchar(100), sup.SupplyDate, 23)<CONVERT(varchar(100), GETDATE(), 23) and sup.HasSupplierTotal>0))
                            and (ISNULL(@ProjectId,'')='' or sup.ProjectId=@ProjectId) and (ISNULL(@HistoryMonth1,'')='' or CONVERT(varchar(7) ,sup.SupplyDate, 120)=@HistoryMonth1) 
                            union all 
                            select sup.ProcessFactoryCode,sup.BatchPlanTotal,case when isnull(sup.BatchPlanTotal,0)-isnull(sup.HasSupplierTotal,0)<0 then 0 else isnull(sup.BatchPlanTotal,0)-isnull(sup.HasSupplierTotal,0) end as HasSupplierTotal,'未供货量' as dhType from TbSupplyList sup
                            where sup.SupplyCompleteTime is null and (ISNULL(@ProjectId,'')='' or sup.ProjectId=@ProjectId) and (ISNULL(@HistoryMonth1,'')='' or CONVERT(varchar(7) ,sup.SupplyDate, 120)=@HistoryMonth1) ) TbGh on TbJgc.CompanyCode=TbGh.ProcessFactoryCode and TbJgc.dhType=TbGh.dhType  
                            group by TbJgc.CompanyCode,TbJgc.CompanyFullName,TbJgc.dhType) AS P
                            PIVOT 
                            (
                            SUM(HasSupplierTotal) FOR 
                            p.dhType IN ([到货总量],[按时供货量],[延迟供货量],[未供货量])
                            ) AS T) Tb order by Tb.CompanyCode asc";
            var dt = Db.Context.FromSql(sql)
                .AddInParameter("@ProjectId", DbType.String, ProjectId)
                .AddInParameter("@HistoryMonth1", DbType.String, HistoryMonth1).ToDataTable();
            return dt;
        }

        /// <summary>
        /// 当月原材料到货合格数量分析
        /// </summary>
        /// <returns></returns>
        public DataTable YCLArrivalQualified(string OrgType, string ProjectId)
        {
            if (string.IsNullOrWhiteSpace(OrgType))
            {
                OrgType = OperatorProvider.Provider.CurrentUser.OrgType;
            }
            if (OrgType == "1") //加工厂
            {
                ProjectId = "";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(ProjectId))
                {
                    ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
                }
            }
            string addsql = "";
            if (!string.IsNullOrWhiteSpace(ProjectId))
            {
                addsql += "ProjectId='" + ProjectId + "'";
            }
            string sql = "";
            var ret = Db.Context.FromSql(sql).ToDataTable();
            return ret;
        }

        /// <summary>
        /// 加工厂当月配送到站各类问题次数统计
        /// </summary>
        /// <returns></returns>
        public DataTable JGCPSXHAnalysis(string OrgType, string ProjectId, string HistoryMonth2)
        {
            if (string.IsNullOrWhiteSpace(OrgType))
            {
                OrgType = OperatorProvider.Provider.CurrentUser.OrgType;
            }
            if (OrgType == "1") //加工厂
            {
                ProjectId = "";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(ProjectId))
                {
                    ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
                }
            }
            string sql = @"select * from (SELECT CompanyCode,CompanyFullName,等待卸货超时次数 as ddxhcscs,卸货超时次数 as xhcscs,签收超时次数 as qscscs,卸货过程中问题次数 as                 xhgczwtcs FROM (select TbCp.*,isnull(TbPsDzCs.fsCs,0) as FsCs from (
                            select CompanyCode, CompanyFullName,'等待卸货超时次数' as PsDzType from TbCompany where OrgType = 1
                            union all
                            select CompanyCode, CompanyFullName,'卸货超时次数' as PsDzType from TbCompany where OrgType = 1
                            union all
                            select CompanyCode, CompanyFullName,'签收超时次数' as PsDzType from TbCompany where OrgType = 1
                            union all
                            select CompanyCode, CompanyFullName,'卸货过程中问题次数' as PsDzType from TbCompany where OrgType = 1) TbCp
                               left join(
                               --等待卸货超时次数
                               select disEnt.ProcessFactoryCode, tcr.DisEntOrderId, '等待卸货超时次数' as PsDzType, COUNT(1) fsCs from TbTransportCarReport tcr
                               left join TbDistributionEnt disEnt on tcr.DistributionCode = disEnt.DistributionCode
                               where (ISNULL(@HistoryMonth2,'')='' or CONVERT(varchar(7) ,tcr.LoadCompleteTime, 120)=@HistoryMonth2) 
                               and(isnull(@ProjectId, '') = '' or tcr.ProjectId = @ProjectId)
                               and tcr.WaitTime > 30
                               group by disEnt.ProcessFactoryCode, tcr.DisEntOrderId
                               union all
                               --卸货超时次数
                               select Tb.*, COUNT(1) as fsCs from(
                               select disEnt.ProcessFactoryCode, EWFormDataCode, '卸货超时次数' as PsDzType from TbFormEarlyWarningNodeInfo fewni
                               left join TbDistributionEntOrder disEntOrder on fewni.EWFormDataCode = disEntOrder.ID
                               left join TbDistributionEnt disEnt on disEntOrder.DistributionCode = disEnt.DistributionCode
                               where fewni.MenuCode = 'SiteDischargeCargo'
                               and(ISNULL(@HistoryMonth2,'')='' or CONVERT(varchar(7) ,disEnt.LoadCompleteTime, 120)=@HistoryMonth2) 
                               and(isnull(@ProjectId, '') = '' or fewni.ProjectId = @ProjectId)
                               and fewni.EWTime > '2019-06-15'
                               group by disEnt.ProcessFactoryCode, EWFormDataCode) Tb
                               group by Tb.ProcessFactoryCode, Tb.EWFormDataCode, Tb.PsDzType
                               union all
                               --签收超时次数
                               select Tb.*, COUNT(1) as fsCs from(
                               select disEnt.ProcessFactoryCode, EWFormDataCode, '签收超时次数' as PsDzType from TbFormEarlyWarningNodeInfo fewni
                               left join TbDistributionEntOrder disEntOrder on fewni.EWFormDataCode = disEntOrder.ID
                               left join TbDistributionEnt disEnt on disEntOrder.DistributionCode = disEnt.DistributionCode
                               where fewni.MenuCode = 'SemiFinishedSign'
                               and(ISNULL(@HistoryMonth2,'')='' or CONVERT(varchar(7) ,disEnt.LoadCompleteTime, 120)=@HistoryMonth2) 
                               and(isnull(@ProjectId, '') = '' or fewni.ProjectId = @ProjectId)
                               and fewni.EWTime > '2019-06-15'
                               group by disEnt.ProcessFactoryCode, EWFormDataCode) Tb
                               group by Tb.ProcessFactoryCode, Tb.EWFormDataCode, Tb.PsDzType
                               union all
                               --卸货过程中问题次数
                               select disEnt.ProcessFactoryCode, ac.DisEntOrderId, '卸货过程中问题次数' as PsDzType, COUNT(1) as fsCs  from TbArticle ac
                               left join TbDistributionEntOrder disEntOrder on ac.DisEntOrderId = disEntOrder.ID
                               left join TbDistributionEnt disEnt on disEntOrder.DistributionCode = disEnt.DistributionCode
                               where (ISNULL(@HistoryMonth2,'')='' or CONVERT(varchar(7) ,disEnt.LoadCompleteTime, 120)=@HistoryMonth2) 
                               and(isnull(@ProjectId, '') = '' or disEnt.ProjectId = @ProjectId)
                               and ac.DisEntOrderId is not null
                               group by disEnt.ProcessFactoryCode, ac.DisEntOrderId) TbPsDzCs on TbCp.CompanyCode = TbPsDzCs.ProcessFactoryCode and TbCp.PsDzType = TbPsDzCs.PsDzType) AS P
                            PIVOT
                            (
                            SUM(FsCs) FOR
                            p.PsDzType IN([等待卸货超时次数],[卸货超时次数],[签收超时次数],[卸货过程中问题次数])
                            ) AS T) Tb order by Tb.CompanyCode asc";
            var ret = Db.Context.FromSql(sql)
                .AddInParameter("@ProjectId", DbType.String, ProjectId)
                .AddInParameter("@HistoryMonth2", DbType.String, HistoryMonth2).ToDataTable();
            return ret;
        }
        /// <summary>
        /// 加工厂当月配送到站问题明细
        /// </summary>
        /// <param name="Jgc"></param>
        /// <param name="PsType"></param>
        /// <param name="OrgType"></param>
        /// <param name="ProjectId"></param>
        /// <returns></returns>
        public DataTable GetPsDzWtMxList(string Jgc,string PsType, string OrgType, string ProjectId,string HistoryMonth2)
        {
            if (string.IsNullOrWhiteSpace(OrgType))
            {
                OrgType = OperatorProvider.Provider.CurrentUser.OrgType;
            }
            if (OrgType == "1") //加工厂
            {
                ProjectId = "";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(ProjectId))
                {
                    ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
                }
            }
            string sql = "";
            if (PsType== "等待卸货超时次数")
            {
                sql = @"select disEnt.ProcessFactoryCode,cp.CompanyFullName as ProcessFactoryName,cp2.CompanyFullName as SiteName,disEntOrder.DistributionCode,disEntOrder.OrderCode,disEnt.LoadCompleteTime from TbTransportCarReport tcr
left join TbDistributionEntOrder disEntOrder on disEntOrder.DistributionCode=tcr.DistributionCode and disEntOrder.ID=tcr.DisEntOrderId
left join TbDistributionEnt disEnt on tcr.DistributionCode = disEnt.DistributionCode
left join TbCompany cp on disEnt.ProcessFactoryCode=cp.CompanyCode
left join TbCompany cp2 on disEntOrder.SiteCode=cp2.CompanyCode
where (ISNULL(@HistoryMonth2,'')='' or CONVERT(varchar(7) ,tcr.LoadCompleteTime, 120)=@HistoryMonth2) 
--MONTH(tcr.LoadCompleteTime) = MONTH(GETDATE())
and(isnull(@ProjectId, '') = '' or tcr.ProjectId = @ProjectId)
and tcr.WaitTime > 30 and cp.CompanyFullName like '%" + Jgc+"%'";
            }
            else if (PsType== "卸货超时次数")
            {
                sql = @"select disEnt.ProcessFactoryCode,cp.CompanyFullName as ProcessFactoryName,cp2.CompanyFullName as SiteName,disEntOrder.DistributionCode,disEntOrder.OrderCode,disEnt.LoadCompleteTime from TbFormEarlyWarningNodeInfo fewni
left join TbDistributionEntOrder disEntOrder on fewni.EWFormDataCode = disEntOrder.ID
left join TbDistributionEnt disEnt on disEntOrder.DistributionCode = disEnt.DistributionCode
left join TbCompany cp on disEnt.ProcessFactoryCode=cp.CompanyCode
left join TbCompany cp2 on disEntOrder.SiteCode=cp2.CompanyCode
where fewni.MenuCode = 'SiteDischargeCargo'
and(ISNULL(@HistoryMonth2,'')='' or CONVERT(varchar(7) ,disEnt.LoadCompleteTime, 120)=@HistoryMonth2) 
--MONTH(disEnt.LoadCompleteTime) = MONTH(GETDATE())
and(isnull(@ProjectId, '') = '' or fewni.ProjectId = @ProjectId)
and fewni.EWTime > '2019-06-15'
and cp.CompanyFullName like '%" + Jgc+@"%'
group by disEnt.ProcessFactoryCode,cp.CompanyFullName,cp2.CompanyFullName,disEntOrder.DistributionCode,disEntOrder.OrderCode,disEnt.LoadCompleteTime";
            }
            else if (PsType== "签收超时次数")
            {
                sql = @"select disEnt.ProcessFactoryCode,cp.CompanyFullName as ProcessFactoryName,cp2.CompanyFullName as SiteName,disEntOrder.DistributionCode,disEntOrder.OrderCode,disEnt.LoadCompleteTime from TbFormEarlyWarningNodeInfo fewni
left join TbDistributionEntOrder disEntOrder on fewni.EWFormDataCode = disEntOrder.ID
left join TbDistributionEnt disEnt on disEntOrder.DistributionCode = disEnt.DistributionCode
left join TbCompany cp on disEnt.ProcessFactoryCode=cp.CompanyCode
left join TbCompany cp2 on disEntOrder.SiteCode=cp2.CompanyCode
where fewni.MenuCode = 'SemiFinishedSign'
and(ISNULL(@HistoryMonth2,'')='' or CONVERT(varchar(7) ,disEnt.LoadCompleteTime, 120)=@HistoryMonth2) 
--and MONTH(disEnt.LoadCompleteTime) = MONTH(GETDATE())
and(isnull(@ProjectId, '') = '' or fewni.ProjectId = @ProjectId)
and fewni.EWTime > '2019-06-15'
and cp.CompanyFullName like '%" + Jgc+@"%'
group by disEnt.ProcessFactoryCode,cp.CompanyFullName,cp2.CompanyFullName,disEntOrder.DistributionCode,disEntOrder.OrderCode,disEnt.LoadCompleteTime";
            }
            else if (PsType== "卸货过程中问题次数")
            {
                sql = @"select disEnt.ProcessFactoryCode,cp.CompanyFullName as ProcessFactoryName,cp2.CompanyFullName as SiteName,disEntOrder.DistributionCode,disEntOrder.OrderCode,disEnt.LoadCompleteTime from TbArticle ac
left join TbDistributionEntOrder disEntOrder on ac.DisEntOrderId = disEntOrder.ID
left join TbDistributionEnt disEnt on disEntOrder.DistributionCode = disEnt.DistributionCode
left join TbCompany cp on disEnt.ProcessFactoryCode=cp.CompanyCode
left join TbCompany cp2 on disEntOrder.SiteCode=cp2.CompanyCode
where (ISNULL(@HistoryMonth2,'')='' or CONVERT(varchar(7) ,disEnt.LoadCompleteTime, 120)=@HistoryMonth2) 
--MONTH(disEnt.LoadCompleteTime) = MONTH(GETDATE())
and(isnull(@ProjectId, '') = '' or disEnt.ProjectId = @ProjectId)
and ac.DisEntOrderId is not null
and cp.CompanyFullName like '%" + Jgc+@"%'
group by disEnt.ProcessFactoryCode,cp.CompanyFullName,cp2.CompanyFullName,disEntOrder.DistributionCode,disEntOrder.OrderCode,disEnt.LoadCompleteTime";
            }
            var ret = Db.Context.FromSql(sql)
                .AddInParameter("@ProjectId", DbType.String, ProjectId)
                .AddInParameter("@HistoryMonth2", DbType.String, HistoryMonth2).ToDataTable();
            return ret;
        }

        /// <summary>
        /// 一号加工厂
        /// </summary>
        /// <returns></returns>
        public DataTable GetOneJgc(string OrgType, string ProjectId)
        {
            if (string.IsNullOrWhiteSpace(OrgType))
            {
                OrgType = OperatorProvider.Provider.CurrentUser.OrgType;
            }
            if (OrgType == "1") //加工厂
            {
                ProjectId = "";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(ProjectId))
                {
                    ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
                }
            }
            string sql = @"select * from (
select Tb.*,
case when Tb1.num is null then 0 else Tb1.num end Count1,
case when Tb2.num is null then 0 else Tb2.num end Count2 
from (
select years,months from(Select year(InsertTime) years,Month(InsertTime) months FROM TbWorkOrder 
where (DateDiff(month, InsertTime, GetDate()) >= 0 and DateDiff(month, InsertTime, GetDate()) <= 6) 
and (isnull(@ProjectId,'')='' or ProjectId=@ProjectId) and Examinestatus='已退回' and ProcessFactoryCode='6386683214299275264'
group by year(InsertTime),Month(InsertTime) 
union all
select year(InsertTime) years,Month(InsertTime) months FROM TbProblemOrder 
where (DateDiff(month, InsertTime, GetDate()) >= 0 and DateDiff(month, InsertTime, GetDate()) <= 6) 
and (isnull(@ProjectId,'')='' or ProjectId=@ProjectId) and Examinestatus='审核完成' and ProcessFactoryCode='6386683214299275264'
group by year(InsertTime),Month(InsertTime)) Tb group by years,months) Tb
left join (
Select year(InsertTime) years,Month(InsertTime) months,COUNT(*) num FROM TbWorkOrder 
where (DateDiff(month, InsertTime, GetDate()) >= 0 and DateDiff(month, InsertTime, GetDate()) <= 6) 
and (isnull(@ProjectId,'')='' or ProjectId=@ProjectId) and Examinestatus='已退回' and ProcessFactoryCode='6386683214299275264'
group by year(InsertTime),Month(InsertTime)) Tb1 on Tb.years=Tb1.years and Tb.months=Tb1.months
left join (
select year(InsertTime) years,Month(InsertTime) months,COUNT(*) num FROM TbProblemOrder 
where (DateDiff(month, InsertTime, GetDate()) >= 0 and DateDiff(month, InsertTime, GetDate()) <= 6) 
and (isnull(@ProjectId,'')='' or ProjectId=@ProjectId) and Examinestatus='审核完成' and ProcessFactoryCode='6386683214299275264'
group by year(InsertTime),Month(InsertTime)) Tb2 on Tb.years=Tb2.years and Tb.months=Tb2.months) bb 
order by bb.years,bb.months ";
            var ret = Db.Context.FromSql(sql).AddInParameter("@ProjectId", DbType.String, ProjectId).ToDataTable();
            if (ret.Rows.Count > 0)
            {
                if (ret.Rows.Count == 7)
                {
                    return ret;
                }
                else
                {
                    var kt = DateTime.Now.AddMonths(-6).ToString("yyyy-MM-dd");
                    var jt = DateTime.Now.ToString("yyyy-MM-dd");
                    int kyue = Convert.ToInt32(kt.Substring(5, 2));
                    int jyue = Convert.ToInt32(jt.Substring(5, 2));
                    DataTable dt = new DataTable();               //新建对象
                    dt.Columns.Add("years", typeof(string));   //新建第一列
                    dt.Columns.Add("months", typeof(int));      //新建第二列
                    dt.Columns.Add("Count1", typeof(int));
                    dt.Columns.Add("Count2", typeof(int));
                    var num = kyue;
                    for (var i = 0; i < 7; i++)
                    {
                        dt.Rows.Add("", num, 0, 0);
                        if (num == 12)
                        {
                            num = 1;
                        }
                        num++;
                    }
                    for (var i = 0; i < dt.Rows.Count; i++)
                    {
                        for (var j = 0; j < ret.Rows.Count; j++)
                        {
                            if (Convert.ToInt32(dt.Rows[i]["months"].ToString()) == Convert.ToInt32(ret.Rows[j]["months"].ToString()))
                            {
                                dt.Rows[i]["Count1"] = ret.Rows[j]["Count1"].ToString();
                                dt.Rows[i]["Count2"] = ret.Rows[j]["Count2"].ToString();
                            }
                        }
                    }
                    return dt;
                }
            }
            else
            {
                return ret;
            }
        }
        /// <summary>
        /// 二号加工厂
        /// </summary>
        /// <returns></returns>
        public DataTable GetTwoJgc(string OrgType, string ProjectId)
        {
            if (string.IsNullOrWhiteSpace(OrgType))
            {
                OrgType = OperatorProvider.Provider.CurrentUser.OrgType;
            }
            if (OrgType == "1") //加工厂
            {
                ProjectId = "";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(ProjectId))
                {
                    ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
                }
            }
            string sql = @"select * from (
select Tb.*,
case when Tb1.num is null then 0 else Tb1.num end Count1,
case when Tb2.num is null then 0 else Tb2.num end Count2 
from (
select years,months from(Select year(InsertTime) years,Month(InsertTime) months FROM TbWorkOrder 
where (DateDiff(month, InsertTime, GetDate()) >= 0 and DateDiff(month, InsertTime, GetDate()) <= 6) 
and (isnull(@ProjectId,'')='' or ProjectId=@ProjectId) and Examinestatus='已退回' and ProcessFactoryCode='6386683729561128960'
group by year(InsertTime),Month(InsertTime) 
union all
select year(InsertTime) years,Month(InsertTime) months FROM TbProblemOrder 
where (DateDiff(month, InsertTime, GetDate()) >= 0 and DateDiff(month, InsertTime, GetDate()) <= 6) 
and (isnull(@ProjectId,'')='' or ProjectId=@ProjectId) and Examinestatus='审核完成' and ProcessFactoryCode='6386683729561128960'
group by year(InsertTime),Month(InsertTime)) Tb group by years,months) Tb
left join (
Select year(InsertTime) years,Month(InsertTime) months,COUNT(*) num FROM TbWorkOrder 
where (DateDiff(month, InsertTime, GetDate()) >= 0 and DateDiff(month, InsertTime, GetDate()) <= 6) 
and (isnull(@ProjectId,'')='' or ProjectId=@ProjectId) and Examinestatus='已退回' and ProcessFactoryCode='6386683729561128960'
group by year(InsertTime),Month(InsertTime)) Tb1 on Tb.years=Tb1.years and Tb.months=Tb1.months
left join (
select year(InsertTime) years,Month(InsertTime) months,COUNT(*) num FROM TbProblemOrder 
where (DateDiff(month, InsertTime, GetDate()) >= 0 and DateDiff(month, InsertTime, GetDate()) <= 6) 
and (isnull(@ProjectId,'')='' or ProjectId=@ProjectId) and Examinestatus='审核完成' and ProcessFactoryCode='6386683729561128960'
group by year(InsertTime),Month(InsertTime)) Tb2 on Tb.years=Tb2.years and Tb.months=Tb2.months) bb 
order by bb.years,bb.months ";
            var ret = Db.Context.FromSql(sql).AddInParameter("@ProjectId", DbType.String, ProjectId).ToDataTable();
            if (ret.Rows.Count > 0)
            {
                if (ret.Rows.Count == 7)
                {
                    return ret;
                }
                else
                {
                    var kt = DateTime.Now.AddMonths(-6).ToString("yyyy-MM-dd");
                    var jt = DateTime.Now.ToString("yyyy-MM-dd");
                    int kyue = Convert.ToInt32(kt.Substring(5, 2));
                    int jyue = Convert.ToInt32(jt.Substring(5, 2));
                    DataTable dt = new DataTable();               //新建对象
                    dt.Columns.Add("years", typeof(string));   //新建第一列
                    dt.Columns.Add("months", typeof(int));      //新建第二列
                    dt.Columns.Add("Count1", typeof(int));
                    dt.Columns.Add("Count2", typeof(int));
                    var num = kyue;
                    for (var i = 0; i < 7; i++)
                    {
                        dt.Rows.Add("", num, 0, 0);
                        if (num == 12)
                        {
                            num = 1;
                        }
                        num++;
                    }
                    for (var i = 0; i < dt.Rows.Count; i++)
                    {
                        for (var j = 0; j < ret.Rows.Count; j++)
                        {
                            if (Convert.ToInt32(dt.Rows[i]["months"].ToString()) == Convert.ToInt32(ret.Rows[j]["months"].ToString()))
                            {
                                dt.Rows[i]["Count1"] = ret.Rows[j]["Count1"].ToString();
                                dt.Rows[i]["Count2"] = ret.Rows[j]["Count2"].ToString();
                            }
                        }
                    }
                    return dt;
                }
            }
            else
            {
                return ret;
            }
        }
        /// <summary>
        /// 三号加工厂
        /// </summary>
        /// <returns></returns>
        public DataTable GetThreeJgc(string OrgType, string ProjectId)
        {
            if (string.IsNullOrWhiteSpace(OrgType))
            {
                OrgType = OperatorProvider.Provider.CurrentUser.OrgType;
            }
            if (OrgType == "1") //加工厂
            {
                ProjectId = "";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(ProjectId))
                {
                    ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
                }
            }
            string sql = @"select * from (
select Tb.*,
case when Tb1.num is null then 0 else Tb1.num end Count1,
case when Tb2.num is null then 0 else Tb2.num end Count2 
from (
select years,months from(Select year(InsertTime) years,Month(InsertTime) months FROM TbWorkOrder 
where (DateDiff(month, InsertTime, GetDate()) >= 0 and DateDiff(month, InsertTime, GetDate()) <= 6) 
and (isnull(@ProjectId,'')='' or ProjectId=@ProjectId) and Examinestatus='已退回' and ProcessFactoryCode='6386683947165814784'
group by year(InsertTime),Month(InsertTime) 
union all
select year(InsertTime) years,Month(InsertTime) months FROM TbProblemOrder 
where (DateDiff(month, InsertTime, GetDate()) >= 0 and DateDiff(month, InsertTime, GetDate()) <= 6) 
and (isnull(@ProjectId,'')='' or ProjectId=@ProjectId) and Examinestatus='审核完成' and ProcessFactoryCode='6386683947165814784'
group by year(InsertTime),Month(InsertTime)) Tb group by years,months) Tb
left join (
Select year(InsertTime) years,Month(InsertTime) months,COUNT(*) num FROM TbWorkOrder 
where (DateDiff(month, InsertTime, GetDate()) >= 0 and DateDiff(month, InsertTime, GetDate()) <= 6) 
and (isnull(@ProjectId,'')='' or ProjectId=@ProjectId) and Examinestatus='已退回' and ProcessFactoryCode='6386683947165814784'
group by year(InsertTime),Month(InsertTime)) Tb1 on Tb.years=Tb1.years and Tb.months=Tb1.months
left join (
select year(InsertTime) years,Month(InsertTime) months,COUNT(*) num FROM TbProblemOrder 
where (DateDiff(month, InsertTime, GetDate()) >= 0 and DateDiff(month, InsertTime, GetDate()) <= 6) 
and (isnull(@ProjectId,'')='' or ProjectId=@ProjectId) and Examinestatus='审核完成' and ProcessFactoryCode='6386683947165814784'
group by year(InsertTime),Month(InsertTime)) Tb2 on Tb.years=Tb2.years and Tb.months=Tb2.months) bb 
order by bb.years,bb.months ";
            var ret = Db.Context.FromSql(sql).AddInParameter("@ProjectId", DbType.String, ProjectId).ToDataTable();
            if (ret.Rows.Count > 0)
            {
                if (ret.Rows.Count == 7)
                {
                    return ret;
                }
                else
                {
                    var kt = DateTime.Now.AddMonths(-6).ToString("yyyy-MM-dd");
                    var jt = DateTime.Now.ToString("yyyy-MM-dd");
                    int kyue = Convert.ToInt32(kt.Substring(5, 2));
                    int jyue = Convert.ToInt32(jt.Substring(5, 2));
                    DataTable dt = new DataTable();               //新建对象
                    dt.Columns.Add("years", typeof(string));   //新建第一列
                    dt.Columns.Add("months", typeof(int));      //新建第二列
                    dt.Columns.Add("Count1", typeof(int));
                    dt.Columns.Add("Count2", typeof(int));
                    var num = kyue;
                    for (var i = 0; i < 7; i++)
                    {
                        dt.Rows.Add("", num, 0, 0);
                        if (num == 12)
                        {
                            num = 1;
                        }
                        num++;
                    }
                    for (var i = 0; i < dt.Rows.Count; i++)
                    {
                        for (var j = 0; j < ret.Rows.Count; j++)
                        {
                            if (Convert.ToInt32(dt.Rows[i]["months"].ToString()) == Convert.ToInt32(ret.Rows[j]["months"].ToString()))
                            {
                                dt.Rows[i]["Count1"] = ret.Rows[j]["Count1"].ToString();
                                dt.Rows[i]["Count2"] = ret.Rows[j]["Count2"].ToString();
                            }
                        }
                    }
                    return dt;
                }
            }
            else
            {
                return ret;
            }
        }

    }
}
