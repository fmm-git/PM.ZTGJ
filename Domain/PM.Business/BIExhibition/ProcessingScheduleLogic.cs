using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Business
{
    /// <summary>
    /// 加工进度展示
    /// </summary>
    public class ProcessingScheduleLogic
    {

        #region 展示

        /// <summary>
        /// 各站点各订单进度分析图
        /// </summary>
        /// <returns></returns>
        public DataTable GetGanttList(PageSearchRequest psr,string PId)
        {
            string addsql = "";
            if (!string.IsNullOrWhiteSpace(PId))
            {
                addsql += "ProjectId='" + PId + "'";
            }else if (!string.IsNullOrWhiteSpace(psr.ProjectId)) 
            {
                addsql += "ProjectId='" + psr.ProjectId + "'";
            }
            string sql = "select OrderCode,InsertTime as 'start_date',DistributionTime as end_date,(AccumulativeQuantity/WeightTotal) progress from TbOrderProgress where AccumulativeQuantity <> 0 and AccumulativeQuantity<=WeightTotal and " + addsql;
            var ret = Db.Context.FromSql(sql).ToDataTable();
            return ret;
        }

        /// <summary>
        /// 站点订单进度分析
        /// </summary>
        /// <returns></returns>
        public DataTable GetSiteSchedule(PageSearchRequest psr, string PId)
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
            string sql = "select a.SiteCode,b.CompanyFullName,Sum(a.WeightTotal) WeightTotal,Sum(a.AccumulativeQuantity) AccumulativeQuantity,Sum(ISNULL((a.WeightTotal-a.AccumulativeQuantity),0)) unfinished from TbOrderProgress a left join TbCompany b on a.SiteCode=b.CompanyCode where 1=1 and AccumulativeQuantity <> 0 and AccumulativeQuantity<=WeightTotal and a." + addsql + " group by a.SiteCode,b.CompanyFullName";
            var ret = Db.Context.FromSql(sql).ToDataTable();
            return ret;
        }

        /// <summary>
        /// 加工厂各类订单状态量分析
        /// </summary>
        /// <returns></returns>
        public DataTable PFKOSAnalysis(PageSearchRequest psr, string PId)
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
            var sql = @"select isnull(Sum(WeightTotal),0.0000) WeightTotal,(select '1') TypeCode from TbWorkOrder 
where Examinestatus <> '未发起' and Examinestatus <> '已退回' and ProjectId='' 
 union all
select isnull(Sum(a.WeightTotal),0.0000) ConfirmedTotal,(select '2') TypeCode from TbWorkOrder a 
left join TbSysDictionaryData b on a.ProcessingState=b.DictionaryCode 
where b.FDictionaryCode='ProcessingState' and b.DictionaryText='确认加工' and a." + addsql + @" 
 union all
select isnull(SUM(AccumulativeQuantity),0.0000) ProcessingTotal,(select '3') TypeCode from TbOrderProgress 
where AccumulativeQuantity>0 and AccumulativeQuantity<WeightTotal and " + addsql + @" 
 union all
select isnull(SUM(WeightTotal),0.0000) UnprocessedTotal,(select '4') TypeCode from TbOrderProgress 
where AccumulativeQuantity=0 and " + addsql + @"  
 union all
select isnull(SUM(WeightTotal),0.0000) ProcessingNonDeliveryTotal,(select '5') TypeCode from TbOrderProgress 
where OrderCode not in 
(select a.OrderCode from TbDistributionPlanInfo a 
inner join TbDistributionEnt b on a.DistributionPlanCode=b.DistributionPlanCode where a." + addsql + " and b." + addsql + @") 
and WeightTotal=AccumulativeQuantity and " + addsql + @"  
 union all
select isnull(SUM(TotalAggregate),0.0000) DistributionTotal,(select '6') TypeCode 
from TbDistributionEnt where " + addsql;
            var ret = Db.Context.FromSql(sql).ToDataTable();
            return ret;
        }

        /// <summary>
        /// 订单进度
        /// </summary>
        /// <returns></returns>
        public DataTable OrderJinDu(PageSearchRequest psr, string PId)
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
            var sql = "select a.OrderCode,b.CompanyFullName,a.DistributionTime,c.LoadCompleteTime from TbDistributionPlanInfo a left join TbCompany b on a.SiteCode=b.CompanyCode inner join TbDistributionEnt c on a.DistributionPlanCode=c.DistributionPlanCode where 1=1 and a." + addsql + " and c." + addsql;
            var ret = Db.Context.FromSql(sql).ToDataTable();
            if (ret.Rows.Count > 0) 
            {
                ret.Columns.Add(new DataColumn("Sta"));
                for (var i = 0; i < ret.Rows.Count; i++) 
                {
                    var jh = Convert.ToDateTime(ret.Rows[i]["DistributionTime"].ToString());
                    var wc = Convert.ToDateTime(ret.Rows[i]["LoadCompleteTime"].ToString());
                    if (wc > jh)
                    {
                        ret.Rows[i]["Sta"] = "3";
                    }
                    else if (wc == jh)
                    {
                        ret.Rows[i]["Sta"] = "1";
                    }
                    else 
                    {
                        ret.Rows[i]["Sta"] = "2";
                    }
                }
            }
            return ret;
        }

        #endregion

        #region 新版统计报表

        /// <summary>
        /// 当月订单状态量统计
        /// </summary>
        /// <returns></returns>
        public Tuple<DataTable> WorkOrderStatusTj(string OrgType, string ProjectId)
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

            var ret1 = @"select * from (select TbCp.*,TbOrderType.OrderType,isnull(TbOrderZl.SumWeightTotal,0) as SumWeightTotal from (select cp.CompanyCode,cp.CompanyFullName,OrgType from TbCompany cp 
where cp.OrgType=1) TbCp
left join(
		select * from (select '订单总量' as OrderType,1 as OrgType
		union all
		select '已接收订单量' as OrderType,1 as OrgType
		union all
		select '已领料订单量' as OrderType,1 as OrgType
		union all
		select '加工中订单量' as OrderType,1 as OrgType
		union all
		select '未配送订单量' as OrderType,1 as OrgType
		union all
		select '已配送订单量' as OrderType,1 as OrgType) TbType
) TbOrderType on TbCp.OrgType=TbOrderType.OrgType
left join (
		select isnull(Convert(decimal(18,5),SUM(wod.MeasurementUnitZl*wod.ItemUseNum*wod.Number)),0) as SumWeightTotal,'订单总量' as OrderType,wo.ProcessFactoryCode from TbWorkOrder wo
		left join TbWorkOrderDetail wod on wo.OrderCode=wod.OrderCode 
		where wo.Examinestatus='审核完成' 
		and MONTH(wo.InsertTime)=MONTH(GETDATE()) 
		and (isnull(@ProjectId,'')='' or wo.ProjectId=@ProjectId) and wod.RevokeStart='正常'
		group by wo.ProcessFactoryCode
		union all
		select isnull(Convert(decimal(18,5),SUM(wod.MeasurementUnitZl*wod.ItemUseNum*wod.Number)),0) as SumWeightTotal,'已接收订单量' as OrderType,wo.ProcessFactoryCode from TbWorkOrder wo
		left join TbWorkOrderDetail wod on wo.OrderCode=wod.OrderCode 
		where wo.Examinestatus='审核完成' 
		and MONTH(wo.InsertTime)=MONTH(GETDATE()) 
		and (isnull(@ProjectId,'')='' or wo.ProjectId=@ProjectId) and wo.ProcessingState='Received'
	    and wod.RevokeStart='正常' 
		group by wo.ProcessFactoryCode
		union all
		select isnull(Convert(decimal(18,5),SUM(wod.MeasurementUnitZl*wod.ItemUseNum*wod.Number)),0) as SumWeightTotal,'已领料订单量' as OrderType,wo.ProcessFactoryCode from TbWorkOrder wo
		left join TbWorkOrderDetail wod on wo.OrderCode=wod.OrderCode 
		where wo.Examinestatus='审核完成' 
		and MONTH(wo.InsertTime)=MONTH(GETDATE()) 
		and (isnull(@ProjectId,'')='' or wo.ProjectId=@ProjectId) and wo.ProcessingState='AlreadyReceived'
	    and wod.RevokeStart='正常' 
		group by wo.ProcessFactoryCode
		union all
		select isnull(Convert(decimal(18,5),SUM(wod.MeasurementUnitZl*wod.ItemUseNum*wod.Number)),0) as SumWeightTotal,'加工中订单量' as OrderType,wo.ProcessFactoryCode from TbWorkOrder wo
		left join TbWorkOrderDetail wod on wo.OrderCode=wod.OrderCode 
		where wo.Examinestatus='审核完成' 
		and MONTH(wo.InsertTime)=MONTH(GETDATE()) 
		and (isnull(@ProjectId,'')='' or wo.ProjectId=@ProjectId) and wo.ProcessingState='Processing'
	    and wod.RevokeStart='正常'
		group by wo.ProcessFactoryCode
		union all
		select isnull(Convert(decimal(18,5),sum(dpdi.MeasurementUnitZl*dpdi.ItemUseNum*dpdi.PSAmount)),0) as SumWeightTotal,'未配送订单量' as OrderType,dpi.ProcessFactoryCode  from TbDistributionPlanInfo dpi 
		left join TbDistributionPlanDetailInfo dpdi on dpi.OrderCode=dpdi.OrderCode
		where dpi.OrderCode in(
        select wo.OrderCode from TbWorkOrder wo
		left join TbWorkOrderDetail wod on wo.OrderCode=wod.OrderCode 
		where wo.Examinestatus='审核完成' 
		and MONTH(wo.InsertTime)=MONTH(GETDATE()) 
		and (isnull(@ProjectId,'')='' or wo.ProjectId=@ProjectId)
	    and wod.RevokeStart='正常') and dpdi.PSAmount>0
		group by dpi.ProcessFactoryCode
		union all
	    select isnull(Convert(decimal(18,5),sum(dpdi.MeasurementUnitZl*dpdi.ItemUseNum*(dpdi.Number-dpdi.PSAmount))),0) as SumWeightTotal,'已配送订单量' as OrderType,dpi.ProcessFactoryCode  from TbDistributionPlanInfo dpi 
		left join TbDistributionPlanDetailInfo dpdi on dpi.OrderCode=dpdi.OrderCode
		where dpi.OrderCode in(
        select wo.OrderCode from TbWorkOrder wo
		left join TbWorkOrderDetail wod on wo.OrderCode=wod.OrderCode 
		where wo.Examinestatus='审核完成' 
		and MONTH(wo.InsertTime)=MONTH(GETDATE()) 
		and (isnull(@ProjectId,'')='' or wo.ProjectId=@ProjectId)
	    and wod.RevokeStart='正常') and dpdi.PSAmount!=dpdi.Number
		group by dpi.ProcessFactoryCode
) TbOrderZl on TbCp.CompanyCode=TbOrderZl.ProcessFactoryCode and TbOrderType.OrderType=TbOrderZl.OrderType)  AS P
PIVOT 
(
    SUM(SumWeightTotal/*行转列后 列的值*/) FOR 
    p.OrderType/*需要行转列的列*/ IN ([订单总量],[已接收订单量],[已领料订单量],[加工中订单量],[未配送订单量],[已配送订单量]/*列的值*/)
) AS T";
            DataTable dt1 = Db.Context.FromSql(ret1).AddInParameter("@ProjectId", DbType.String, ProjectId).ToDataTable();
            return new Tuple<DataTable>(dt1);
        }

        /// <summary>
        /// 当月订单加工加工量统计
        /// </summary>
        /// <returns></returns>
        public Tuple<DataTable> WorkOrderWclTj(string OrgType, string ProjectId)
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

            var ret1 = @"select cp.CompanyCode,cp.CompanyFullName,isnull(TbTj.WeightSmallPlan,0) as WeightSmallPlan,isnull(TbTj.AlreadyCompleted,0) as AlreadyCompleted,isnull(TbTj.NoCompleted,0) as NoCompleted from TbCompany cp
left join (select isnull(Convert(decimal(18,5),SUM(opd.MeasurementUnitZl*opd.ItemUseNum*opd.Number)),0) as WeightSmallPlan,SUM(AlreadyCompleted) as AlreadyCompleted,(isnull(Convert(decimal(18,5),SUM(opd.MeasurementUnitZl*opd.ItemUseNum*opd.Number)),0)-SUM(AlreadyCompleted)) as NoCompleted,ProcessFactoryCode from TbOrderProgressDetail opd
left join TbOrderProgress op on opd.OrderCode=op.OrderCode
where opd.RevokeStart='正常' 
and (ISNULL(@ProjectId,'')='' or ProjectId=@ProjectId) and MONTH(InsertTime)=MONTH(GETDATE()) 
group by ProcessFactoryCode) TbTj on cp.CompanyCode=tbtj.ProcessFactoryCode
where cp.OrgType=1";
            DataTable dt1 = Db.Context.FromSql(ret1).AddInParameter("@ProjectId", DbType.String, ProjectId).ToDataTable();
            return new Tuple<DataTable>(dt1);
        }

        /// <summary>
        /// 当月订单加工完成进度量统计
        /// </summary>
        /// <returns></returns>
        public Tuple<DataTable> WorkOrderProgressTj(string OrgType, string ProjectId)
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

            var ret1 = @"select cp.CompanyCode,cp.CompanyFullName,isnull(sum(TbTj.TqPs),0) as TqPs,isnull(sum(TbTj.YhPs),0) as YhPs,isnull(sum(TbTj.ZcPs),0) as ZcPs from TbCompany cp
                        left join (select 
						case when CONVERT(varchar(100), op.FinishProcessingDateTime, 23)< CONVERT(varchar(100), op.DistributionTime, 23) then isnull(Convert(decimal(18,5),SUM(opd.MeasurementUnitZl*opd.ItemUseNum*opd.Number)),0) end as TqPs,
						case when CONVERT(varchar(100), op.FinishProcessingDateTime, 23)> CONVERT(varchar(100), op.DistributionTime, 23) or (op.FinishProcessingDateTime is null and CONVERT(varchar(100), op.DistributionTime, 23)>GETDATE()) then isnull(Convert(decimal(18,5),SUM(opd.MeasurementUnitZl*opd.ItemUseNum*opd.Number)),0) end as YhPs,
						case when CONVERT(varchar(100), op.FinishProcessingDateTime, 23)= CONVERT(varchar(100), op.DistributionTime, 23) then isnull(Convert(decimal(18,5),SUM(opd.MeasurementUnitZl*opd.ItemUseNum*opd.Number)),0) end as ZcPs,
						ProcessFactoryCode from TbOrderProgressDetail opd
                        left join TbOrderProgress op on opd.OrderCode=op.OrderCode
                        where opd.RevokeStart='正常'  
                        and (ISNULL(@ProjectId,'')='' or ProjectId=@ProjectId) and MONTH(InsertTime)=MONTH(GETDATE()) 
                        group by ProcessFactoryCode,op.FinishProcessingDateTime,op.DistributionTime) TbTj on cp.CompanyCode=tbtj.ProcessFactoryCode
                        where cp.OrgType=1
                        group by cp.CompanyCode,cp.CompanyFullName";
            DataTable dt1 = Db.Context.FromSql(ret1).AddInParameter("@ProjectId", DbType.String, ProjectId).ToDataTable();
            return new Tuple<DataTable>(dt1);
        }

        /// <summary>
        /// 订单配送进度量统计
        /// </summary>
        /// <returns></returns>
        public Tuple<DataTable> DistributionProgressTj(string OrgType, string ProjectId)
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

            var ret1 = @"select cp.CompanyCode,cp.CompanyFullName,isnull(TbPsTj.TqPs,0) as TqPs,isnull(TbPsTj.Yhps,0) as YhPs,isnull(TbPsTj.ZcPs,0) as ZcPs from TbCompany cp
                        left join(
                        select Tb.DistributionTiemStart,tb.ProcessFactoryCode,SUM(tb.TqPs) as TqPs,SUM(Tb.YhPs) as Yhps,SUM(Tb.ZcPs) as ZcPs from (select dp.DistributionStart,dp.DistributionTiemStart,dp.ProcessFactoryCode,case when dp.DistributionTiemStart='提前配送' then TbZc.PsWeight end as TqPs,case when dp.DistributionTiemStart='延后配送' then TbZc.PsWeight end as YhPs,case when dp.DistributionTiemStart='延后配送' then TbZc.PsWeight end as ZcPs from TbDistributionPlanInfo dp
                        left join TbDistributionPlanDetailInfo dpd on dp.OrderCode=dpd.OrderCode
                        left join (select de.DistributionPlanCode,de.ProcessFactoryCode,dei.PlanItemID,sum(dei.UnitWeight*dei.SingletonWeight*dei.Number) as PsWeight from TbDistributionEntItem dei
                        left join TbDistributionEnt de on dei.DistributionCode=de.DistributionCode  where de.UnloadingState='已完成' and (ISNULL(@ProjectId,'')='' or ProjectId=@ProjectId) group by de.DistributionPlanCode,de.ProcessFactoryCode,dei.PlanItemID) TbZc on TbZc.PlanItemID=dpd.ID and TbZc.DistributionPlanCode=dp.DistributionPlanCode
                        where (ISNULL(@ProjectId,'')='' or dp.ProjectId=@ProjectId) 
                        and dp.DistributionStart!='未配送') Tb 
                        group by Tb.DistributionTiemStart,tb.ProcessFactoryCode) TbPsTj on cp.CompanyCode=TbPsTj.ProcessFactoryCode
                        where cp.OrgType=1";
            DataTable dt1 = Db.Context.FromSql(ret1).AddInParameter("@ProjectId", DbType.String, ProjectId).ToDataTable();
            return new Tuple<DataTable>(dt1);
        }

        #endregion 

    }
}
