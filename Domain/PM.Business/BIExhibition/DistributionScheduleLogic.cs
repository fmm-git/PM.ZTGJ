using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using System.Data;

namespace PM.Business
{
    public class DistributionScheduleLogic
    {
        #region 展示

        /// <summary>
        /// 构件加工厂当月配送情况分析
        /// </summary>
        /// <returns></returns>
        public DataTable GetGJCDYPSAnalysis(PageSearchRequest psr, string PId)
        {
            string addsql = "";
            if (!string.IsNullOrWhiteSpace(PId))
            {
                addsql += "ProjectId='" + PId + "'";
            }
            else if (!string.IsNullOrWhiteSpace(psr.ProjectId))
            {
                addsql += "ProjectId='" + psr.ProjectId + "'";
            }
            string sql = @"select a.CompanyFullName,isnull(a.TotalAggregate,0.0000) TotalAggregate,
isnull(b.ZCDis,0.0000) ZCDis,isnull(c.TQDis,0.0000) TQDis,isnull(d.YHDis,0.0000) YHDis 
from (select b.CompanyFullName,isnull(Sum(a.TotalAggregate),0.0000) TotalAggregate from TbDistributionEnt a 
left join TbCompany b on a.ProcessFactoryCode=b.CompanyCode 
where DateDiff(month, a.InsertTime, GetDate()) = 0 and a." + addsql + @" group by b.CompanyFullName) a left join 
(select b.CompanyFullName,isnull(Sum(a.TotalAggregate),0.0000) ZCDis from TbDistributionEnt a 
left join TbCompany b on a.ProcessFactoryCode=b.CompanyCode 
left join TbDistributionPlanInfo c on a.DistributionPlanCode=c.DistributionPlanCode 
where DateDiff(month, a.InsertTime, GetDate()) = 0 and a.LoadCompleteTime=c.DistributionTime 
and a." + addsql + " and c." + addsql + @" 
group by b.CompanyFullName) b on a.CompanyFullName=b.CompanyFullName left join 
(select b.CompanyFullName,isnull(Sum(a.TotalAggregate),0.0000) TQDis from TbDistributionEnt a 
left join TbCompany b on a.ProcessFactoryCode=b.CompanyCode 
left join TbDistributionPlanInfo c on a.DistributionPlanCode=c.DistributionPlanCode 
where DateDiff(month, a.InsertTime, GetDate()) = 0 and a.LoadCompleteTime<c.DistributionTime 
and a." + addsql + " and c." + addsql + @" 
group by b.CompanyFullName) c on a.CompanyFullName=c.CompanyFullName left join 
(select b.CompanyFullName,isnull(Sum(a.TotalAggregate),0.0000) YHDis from TbDistributionEnt a 
left join TbCompany b on a.ProcessFactoryCode=b.CompanyCode 
left join TbDistributionPlanInfo c on a.DistributionPlanCode=c.DistributionPlanCode 
where DateDiff(month, a.InsertTime, GetDate()) = 0 and a.LoadCompleteTime>c.DistributionTime 
and a." + addsql + " and c." + addsql + @" 
group by b.CompanyFullName) d on a.CompanyFullName=d.CompanyFullName";
            var ret = Db.Context.FromSql(sql).ToDataTable();
            return ret;
        }

        /// <summary>
        /// 一号加工厂
        /// </summary>
        /// <returns></returns>
        public DataTable GetOneJia(PageSearchRequest psr, string PId)
        {
            string addsql = "";
            if (!string.IsNullOrWhiteSpace(PId))
            {
                addsql += "ProjectId='" + PId + "'";
            }
            else if (!string.IsNullOrWhiteSpace(psr.ProjectId))
            {
                addsql += "ProjectId='" + psr.ProjectId + "'";
            }
            string sql = @"select b.CompanyFullName,isnull(Sum(TotalAggregate),0.0000) TotalAggregate from TbDistributionEnt a 
left join TbCompany b on a.SiteCode=b.CompanyCode 
where ProcessFactoryCode='6386683214299275264' and a." + addsql + " and DateDiff(month, InsertTime, GetDate()) = 0 group by b.CompanyFullName";
            var ret = Db.Context.FromSql(sql).ToDataTable();
            return ret;
        }

        /// <summary>
        /// 二号加工厂
        /// </summary>
        /// <returns></returns>
        public DataTable GetTwoJia(PageSearchRequest psr, string PId)
        {
            string addsql = "";
            if (!string.IsNullOrWhiteSpace(PId))
            {
                addsql += "ProjectId='" + PId + "'";
            }
            else if (!string.IsNullOrWhiteSpace(psr.ProjectId))
            {
                addsql += "ProjectId='" + psr.ProjectId + "'";
            }
            string sql = @"select b.CompanyFullName,isnull(Sum(TotalAggregate),0.0000) TotalAggregate from TbDistributionEnt a left join TbCompany b on a.SiteCode=b.CompanyCode 
where ProcessFactoryCode='6386683729561128960' and a." + addsql + " and DateDiff(month, InsertTime, GetDate()) = 0 group by b.CompanyFullName";
            var ret = Db.Context.FromSql(sql).ToDataTable();
            return ret;
        }

        /// <summary>
        /// 三号加工厂
        /// </summary>
        /// <returns></returns>
        public DataTable GetThreeJia(PageSearchRequest psr, string PId)
        {
            string addsql = "";
            if (!string.IsNullOrWhiteSpace(PId))
            {
                addsql += "ProjectId='" + PId + "'";
            }
            else if (!string.IsNullOrWhiteSpace(psr.ProjectId))
            {
                addsql += "ProjectId='" + psr.ProjectId + "'";
            }
            string sql = @"select b.CompanyFullName,isnull(Sum(TotalAggregate),0.0000) TotalAggregate from TbDistributionEnt a left join TbCompany b on a.SiteCode=b.CompanyCode 
where ProcessFactoryCode='6386683947165814784' and a." + addsql + " and DateDiff(month, InsertTime, GetDate()) = 0 group by b.CompanyFullName";
            var ret = Db.Context.FromSql(sql).ToDataTable();
            return ret;
        }

        #endregion

        #region 新版统计报表

        public DataTable DisStatusTj(string OrgType, string ProjectId)
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
            //string sqlwhere = "";
            //if (!string.IsNullOrWhiteSpace(ProjectId))
            //{
            //    sqlwhere += (" and ProjectId='" + ProjectId + "'");
            //}
            //string sql = @"select * from (SELECT CompanyCode,CompanyFullName,(提前配送+延后配送+正常配送) as pszl,提前配送 as tqps,延后配送 as yhps,正常配送 as zcps FROM (select tb.CompanyCode,Tb.CompanyFullName,psType,isnull(WeightTotal,0) as WeightTotal from (
            //                     select CompanyCode,CompanyFullName,'配送总量' as psType from TbCompany where OrgType=1
            //                     union all
            //                     select CompanyCode,CompanyFullName,'提前配送' as psType from TbCompany where OrgType=1
            //                     union all
            //                     select CompanyCode,CompanyFullName,'延后配送' as psType from TbCompany where OrgType=1
            //                     union all
            //                     select CompanyCode,CompanyFullName,'正常配送' as psType from TbCompany where OrgType=1
            //                     ) Tb
            //                     left join (
            //                     select tb.ProcessFactoryCode,tb.DistributionTiemStart,sum(tb.WeightGauge) as WeightTotal from (
            //                     select * from (select dei.WeightGauge,de.TypeCode,de.ProjectId,de.SiteCode,de.ProcessFactoryCode,dpi.OrderCode,dpi.DistributionStart,dpi.DistributionTiemStart,dpi.DistributionTime from TbDistributionEnt de
            //                     left join TbDistributionEntItem dei on de.DistributionCode=dei.DistributionCode
            //                     left join TbDistributionPlanDetailInfo dpdi on dei.PlanItemID=dpdi.ID
            //                     left join TbDistributionPlanInfo dpi on dpdi.OrderCode=dpi.OrderCode
            //                     where UnloadingState='已完成') tb1 where 1=1 " + sqlwhere + @") tb 
            //                     group by tb.ProcessFactoryCode,tb.DistributionTiemStart) Tb2  
            //                     on Tb.CompanyCode=Tb2.ProcessFactoryCode and tb.psType=Tb2.DistributionTiemStart)
            //                     AS P
            //                     PIVOT 
            //                     (
            //                         SUM(WeightTotal) FOR 
            //                         p.psType IN ([配送总量],[提前配送],[延后配送],[正常配送])
            //                    ) AS T) Tb order by Tb.CompanyCode asc";
            string sql = @"select * from (SELECT CompanyCode,CompanyFullName,(提前配送+延后配送+正常配送) as pszl,提前配送 as tqps,延后配送 as yhps,正常配送 as zcps FROM       (select Tb.*,isnull(TbDisPlan.WeightSmallPlan,0) as WeightSmallPlan from (select CompanyCode,CompanyFullName,'配送总量' as psType from TbCompany where OrgType=1
                union all
                select CompanyCode,CompanyFullName,'提前配送' as psType from TbCompany where OrgType=1
                union all
                select CompanyCode,CompanyFullName,'延后配送' as psType from TbCompany where OrgType=1
                union all
                select CompanyCode,CompanyFullName,'正常配送' as psType from TbCompany where OrgType=1) Tb
                left join (--提前配送
                select dpi.ProcessFactoryCode,'提前配送' as PsType, isnull(Convert(decimal(18,5),SUM(dpdi.MeasurementUnitZl*dpdi.ItemUseNum*(dpdi.Number-dpdi.PSAmount))),0) as WeightSmallPlan from TbDistributionPlanInfo dpi
                left join TbDistributionPlanDetailInfo dpdi on dpi.OrderCode=dpdi.OrderCode
                where MONTH(dpi.DistributionTime)=MONTH(GETDATE()) 
                and (isnull(@ProjectId,'')='' or dpi.ProjectId=@ProjectId) and dpdi.RevokeStart='正常'
                and dpi.DistributionTiemStart='提前配送'
                group by dpi.ProcessFactoryCode
                union all
                --延后配送(计划配送时间大小于了实际配送时间)
                select dpi.ProcessFactoryCode,'延后配送' as PsType,isnull(Convert(decimal(18,5),SUM(dpdi.MeasurementUnitZl*dpdi.ItemUseNum*(dpdi.Number-dpdi.PSAmount))),0) as WeightSmallPlan from TbDistributionPlanInfo dpi
                left join TbDistributionPlanDetailInfo dpdi on dpi.OrderCode=dpdi.OrderCode
                where MONTH(dpi.DistributionTime)=MONTH(GETDATE()) 
                and (isnull(@ProjectId,'')='' or dpi.ProjectId=@ProjectId) and dpdi.RevokeStart='正常'
                and dpi.DistributionTiemStart='延后配送'
                group by dpi.ProcessFactoryCode
                union all
                --延后配送(计划配送时间大于系统当前时间同时还没有配送)
                select dpi.ProcessFactoryCode,'延后配送' as PsType,isnull(Convert(decimal(18,5),SUM(dpdi.MeasurementUnitZl*dpdi.ItemUseNum*(dpdi.Number))),0) as WeightSmallPlan from TbDistributionPlanInfo dpi
                left join TbDistributionPlanDetailInfo dpdi on dpi.OrderCode=dpdi.OrderCode
                where MONTH(dpi.DistributionTime)=MONTH(GETDATE()) 
                and (isnull(@ProjectId,'')='' or dpi.ProjectId=@ProjectId) and dpdi.RevokeStart='正常'
                and dpi.DistributionTime<GETDATE()
                and dpi.DistributionTiemStart='暂无'
                group by dpi.ProcessFactoryCode
                union all
                --正常配送(计划配送时间等于了实际配送时间)
                select dpi.ProcessFactoryCode,'正常配送' as PsType,isnull(Convert(decimal(18,5),SUM(dpdi.MeasurementUnitZl*dpdi.ItemUseNum*(dpdi.Number-dpdi.PSAmount))),0) as WeightSmallPlan from TbDistributionPlanInfo dpi
                left join TbDistributionPlanDetailInfo dpdi on dpi.OrderCode=dpdi.OrderCode
                where MONTH(dpi.DistributionTime)=MONTH(GETDATE()) 
                and (isnull(@ProjectId,'')='' or dpi.ProjectId=@ProjectId) and dpdi.RevokeStart='正常'
                and dpi.DistributionTiemStart='正常配送'
                group by dpi.ProcessFactoryCode) TbDisPlan on Tb.CompanyCode=TbDisPlan.ProcessFactoryCode and Tb.psType=TbDisPlan.PsType) AS P
                PIVOT 
                (
                SUM(WeightSmallPlan) FOR 
                p.psType IN ([配送总量],[提前配送],[延后配送],[正常配送])
                ) AS T) Tb order by Tb.CompanyCode asc";
            var dt = Db.Context.FromSql(sql).AddInParameter("@ProjectId", DbType.String, ProjectId).ToDataTable();
            return dt;
        }

        public DataTable OneJgc(string OrgType, string ProjectId)
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
            //string sqlwhere = "";
            //if (!string.IsNullOrWhiteSpace(ProjectId))
            //{
            //    sqlwhere += (" and tb1.ProjectId='"+ProjectId+"' ");
            //}
            //string sqlChildeson = @"select tb1.ProcessFactoryCode,tb1.ProcessFactoryName,tb1.SiteCode,tb1.SiteName,sum(convert(decimal(18,4),tb1.WeightGauge)) as WeightTotal from (select dei.WeightGauge,de.TypeCode,de.ProjectId,de.SiteCode,de.ProcessFactoryCode,dpi.OrderCode,dpi.DistributionStart,dpi.DistributionTiemStart,dpi.DistributionTime,cp.CompanyFullName as ProcessFactoryName,cp1.CompanyFullName as SiteName from TbDistributionEnt de
            //                        left join TbDistributionEntItem dei on de.DistributionCode=dei.DistributionCode
            //                        left join TbDistributionPlanDetailInfo dpdi on dei.PlanItemID=dpdi.ID
            //                        left join TbDistributionPlanInfo dpi on dpdi.OrderCode=dpi.OrderCode
            //                        left join TbCompany cp on de.ProcessFactoryCode=cp.CompanyCode
            //                        left join TbCompany cp1 on de.SiteCode=cp1.CompanyCode
            //                        where UnloadingState='已完成') tb1 where 1=1 and tb1.ProcessFactoryCode='6386683214299275264' " + sqlwhere + @" group by tb1.ProcessFactoryCode,tb1.ProcessFactoryName,tb1.SiteCode,tb1.SiteName";
            string sqlChildeson = @"select dpi.ProcessFactoryCode,cp1.CompanyFullName as ProcessFactoryName,dpi.SiteCode,cp2.CompanyFullName as SiteName,isnull(Convert(decimal(18,5),SUM(dpdi.MeasurementUnitZl*dpdi.ItemUseNum*(dpdi.Number-dpdi.PSAmount))),0) as WeightTotal from TbDistributionPlanInfo dpi
left join TbDistributionPlanDetailInfo dpdi on dpi.OrderCode = dpdi.OrderCode
left join TbCompany cp1 on dpi.ProcessFactoryCode = cp1.CompanyCode
left join TbCompany cp2 on dpi.SiteCode = cp2.CompanyCode
where MONTH(dpi.DistributionTime)= MONTH(GETDATE())
and(isnull(@ProjectId, '') = '' or dpi.ProjectId = @ProjectId) and dpdi.RevokeStart = '正常' and dpdi.Number - dpdi.PSAmount > 0
and dpi.ProcessFactoryCode = '6386683214299275264'
group by dpi.ProcessFactoryCode,dpi.SiteCode,cp1.CompanyFullName,cp2.CompanyFullName";
            var dt2 = Db.Context.FromSql(sqlChildeson).AddInParameter("@ProjectId", DbType.String, ProjectId).ToDataTable();
            return dt2;
        }

        public DataTable TwoJgc(string OrgType, string ProjectId)
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
            //string sqlwhere = "";
            //if (!string.IsNullOrWhiteSpace(ProjectId))
            //{
            //    sqlwhere += (" and tb1.ProjectId='" + ProjectId + "' ");
            //}
            //string sqlChildeson = @"select tb1.ProcessFactoryCode,tb1.ProcessFactoryName,tb1.SiteCode,tb1.SiteName,sum(convert(decimal(18,4),tb1.WeightGauge)) as WeightTotal from (select dei.WeightGauge,de.TypeCode,de.ProjectId,de.SiteCode,de.ProcessFactoryCode,dpi.OrderCode,dpi.DistributionStart,dpi.DistributionTiemStart,dpi.DistributionTime,cp.CompanyFullName as ProcessFactoryName,cp1.CompanyFullName as SiteName from TbDistributionEnt de
            //                        left join TbDistributionEntItem dei on de.DistributionCode=dei.DistributionCode
            //                        left join TbDistributionPlanDetailInfo dpdi on dei.PlanItemID=dpdi.ID
            //                        left join TbDistributionPlanInfo dpi on dpdi.OrderCode=dpi.OrderCode
            //                        left join TbCompany cp on de.ProcessFactoryCode=cp.CompanyCode
            //                        left join TbCompany cp1 on de.SiteCode=cp1.CompanyCode
            //                        where UnloadingState='已完成') tb1 where 1=1 and tb1.ProcessFactoryCode='6386683729561128960' " + sqlwhere + @" group by tb1.ProcessFactoryCode,tb1.ProcessFactoryName,tb1.SiteCode,tb1.SiteName";
            string sqlChildeson = @"select dpi.ProcessFactoryCode,cp1.CompanyFullName as ProcessFactoryName,dpi.SiteCode,cp2.CompanyFullName as SiteName,isnull(Convert(decimal(18,5),SUM(dpdi.MeasurementUnitZl*dpdi.ItemUseNum*(dpdi.Number-dpdi.PSAmount))),0) as WeightTotal from TbDistributionPlanInfo dpi
left join TbDistributionPlanDetailInfo dpdi on dpi.OrderCode = dpdi.OrderCode
left join TbCompany cp1 on dpi.ProcessFactoryCode = cp1.CompanyCode
left join TbCompany cp2 on dpi.SiteCode = cp2.CompanyCode
where MONTH(dpi.DistributionTime)= MONTH(GETDATE())
and(isnull(@ProjectId, '') = '' or dpi.ProjectId = @ProjectId) and dpdi.RevokeStart = '正常' and dpdi.Number - dpdi.PSAmount > 0
and dpi.ProcessFactoryCode = '6386683729561128960'
group by dpi.ProcessFactoryCode,dpi.SiteCode,cp1.CompanyFullName,cp2.CompanyFullName";
            var dt2 = Db.Context.FromSql(sqlChildeson).AddInParameter("@ProjectId", DbType.String, ProjectId).ToDataTable();
            return dt2;
        }

        public DataTable ThreeJgc(string OrgType, string ProjectId)
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
            //string sqlwhere = "";
            //if (!string.IsNullOrWhiteSpace(ProjectId))
            //{
            //    sqlwhere += (" and tb1.ProjectId='" + ProjectId + "' ");
            //}
            //string sqlChildeson = @"select tb1.ProcessFactoryCode,tb1.ProcessFactoryName,tb1.SiteCode,tb1.SiteName,sum(convert(decimal(18,4),tb1.WeightGauge)) as WeightTotal from (select dei.WeightGauge,de.TypeCode,de.ProjectId,de.SiteCode,de.ProcessFactoryCode,dpi.OrderCode,dpi.DistributionStart,dpi.DistributionTiemStart,dpi.DistributionTime,cp.CompanyFullName as ProcessFactoryName,cp1.CompanyFullName as SiteName from TbDistributionEnt de
            //                        left join TbDistributionEntItem dei on de.DistributionCode=dei.DistributionCode
            //                        left join TbDistributionPlanDetailInfo dpdi on dei.PlanItemID=dpdi.ID
            //                        left join TbDistributionPlanInfo dpi on dpdi.OrderCode=dpi.OrderCode
            //                        left join TbCompany cp on de.ProcessFactoryCode=cp.CompanyCode
            //                        left join TbCompany cp1 on de.SiteCode=cp1.CompanyCode
            //                        where UnloadingState='已完成') tb1 where 1=1 and tb1.ProcessFactoryCode='6386683947165814784' " + sqlwhere + @" group by tb1.ProcessFactoryCode,tb1.ProcessFactoryName,tb1.SiteCode,tb1.SiteName";
            string sqlChildeson = @"select dpi.ProcessFactoryCode,cp1.CompanyFullName as ProcessFactoryName,dpi.SiteCode,cp2.CompanyFullName as SiteName,isnull(Convert(decimal(18,5),SUM(dpdi.MeasurementUnitZl*dpdi.ItemUseNum*(dpdi.Number-dpdi.PSAmount))),0) as WeightTotal from TbDistributionPlanInfo dpi
left join TbDistributionPlanDetailInfo dpdi on dpi.OrderCode = dpdi.OrderCode
left join TbCompany cp1 on dpi.ProcessFactoryCode = cp1.CompanyCode
left join TbCompany cp2 on dpi.SiteCode = cp2.CompanyCode
where MONTH(dpi.DistributionTime)= MONTH(GETDATE())
and(isnull(@ProjectId, '') = '' or dpi.ProjectId = @ProjectId) and dpdi.RevokeStart = '正常' and dpdi.Number - dpdi.PSAmount > 0
and dpi.ProcessFactoryCode = '6386683947165814784'
group by dpi.ProcessFactoryCode,dpi.SiteCode,cp1.CompanyFullName,cp2.CompanyFullName";
            var dt2 = Db.Context.FromSql(sqlChildeson).AddInParameter("@ProjectId", DbType.String, ProjectId).ToDataTable();
            return dt2;
        }
        #endregion
    }
}
