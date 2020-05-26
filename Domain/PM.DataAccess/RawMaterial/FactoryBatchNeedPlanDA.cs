using PM.DataAccess.DbContext;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.DataAccess.RawMaterial
{
    /// <summary>
    /// 数据访问处理层
    /// 加工厂批次需求计划
    /// </summary>
    public class FactoryBatchNeedPlanDA
    {

        #region 查询数据

        /// <summary>
        /// 自动带入需求计划明细
        /// </summary>
        /// <returns></returns>
        public DataTable GetXQJHDetail(string number,string ProjectId, string ProcessFactoryCode, string WorkAreaCode, string BranchCode)
        {
            var Code = number.Substring(0,2);
            var sql = "";
            if (Code == "XQ") {
                sql = @"select Tb.* from (select a.ID,a.DemandPlanCode,a.MaterialCode,a.MaterialName,a.SpecificationModel,a.MeasurementUnit,b.DictionaryText as MeasurementUnitText,case when isnull(a.DemandNum,0)-isnull(TbPcPlan.BatchPlanQuantity,0)>0 then isnull(a.DemandNum,0)-isnull(TbPcPlan.BatchPlanQuantity,0) else 0 end as DemandNum,a.SkillRequire,a.Remark,isnull(TbQkl.Qkl,0) as Qkl from TbRawMaterialMonthDemandPlanDetail a 
                        left join TbSysDictionaryData b on a.MeasurementUnit = b.DictionaryCode  
                        left join (select fbp.RawMaterialDemandNum,fbpi.RawMaterialNum,sum(fbpi.BatchPlanQuantity) as BatchPlanQuantity  from TbFactoryBatchNeedPlan fbp
                        left join TbFactoryBatchNeedPlanItem fbpi on fbp.BatchPlanNum=fbpi.BatchPlanNum 
                        where fbp.RawMaterialDemandNum like 'XQJH%'
                        group by fbp.RawMaterialDemandNum,fbpi.RawMaterialNum) TbPcPlan on a.DemandPlanCode=TbPcPlan.RawMaterialDemandNum and a.MaterialCode=TbPcPlan.RawMaterialNum
                        left join(select Tb1.MaterialCode,(isnull(Tb2.WeightSmallPlan,0)-isnull(Tb1.KcDtZl,0)) as Qkl from (select MaterialCode,ProjectId,TbStorage.ProcessFactoryCode,WorkAreaCode,TbCompany.ParentCompanyCode as BranchCode,SUM(LockCount)+SUM(UseCount) as KcDtZl
                        FROM TbRawMaterialStockRecord
                        left join TbStorage on TbRawMaterialStockRecord.StorageCode=TbStorage.StorageCode
                        left join TbCompany on TbRawMaterialStockRecord.WorkAreaCode=TbCompany.CompanyCode where 1=1 and TbRawMaterialStockRecord.ProjectId=@ProjectId and TbRawMaterialStockRecord.WorkAreaCode=@WorkAreaCode and TbStorage.ProcessFactoryCode=@ProcessFactoryCode and TbCompany.ParentCompanyCode=@BranchCode
                        group by MaterialCode,ProjectId,TbStorage.ProcessFactoryCode,WorkAreaCode,TbCompany.ParentCompanyCode) Tb1
                        left join (select od.MaterialCode,od.MaterialName,od.SpecificationModel,isnull(sum(od.WeightSmallPlan),0) as WeightSmallPlan,wo.ProjectId,cp1.ParentCompanyCode as WorkAreaCode,cp2.ParentCompanyCode as BranchCode,wo.ProcessFactoryCode from TbWorkOrder wo 
                        left join TbWorkOrderDetail od on wo.OrderCode=od.OrderCode
                        left join TbCompany cp1 on wo.SiteCode=cp1.CompanyCode
                        left join TbCompany cp2 on cp1.ParentCompanyCode=cp2.CompanyCode
                        where 1=1 and wo.Examinestatus='审核完成' and wo.ProcessingState='Received' and wo.ProjectId=@ProjectId and wo.ProcessFactoryCode=@ProcessFactoryCode and cp1.ParentCompanyCode=@WorkAreaCode and cp2.ParentCompanyCode=@BranchCode
                        group by od.MaterialCode,od.MaterialName,od.SpecificationModel,wo.ProjectId,cp1.ParentCompanyCode,cp2.ParentCompanyCode,wo.ProcessFactoryCode) Tb2 on Tb1.MaterialCode=Tb2.MaterialCode and Tb1.ProcessFactoryCode=Tb2.ProcessFactoryCode and Tb1.BranchCode=Tb2.BranchCode and Tb1.WorkAreaCode=Tb2.WorkAreaCode and Tb1.ProjectId=Tb2.ProjectId) TbQkl on a.MaterialCode=TbQkl.MaterialCode
                        where DemandPlanCode=@number and b.FDictionaryCode='Unit') Tb where Tb.DemandNum>0";
            }
            else if (Code == "BC")
            {
                //查询出原计划编号
                var monthPlan = Repository<TbRawMaterialMonthDemandSupplyPlan>.First(p => p.SupplyPlanCode == number);
                sql = @"select Tb.* from (select a.ID,a.SupplyPlanCode,a.MaterialCode,a.MaterialName,a.SpecificationModel,a.MeasurementUnit,TbPcPlan.RawMaterialDemandNum,
                        case 
                        when (isnull(a.DemandNum,0)+isnull(a.SupplyNum,0))-isnull(TbPcPlan.BatchPlanQuantity,0)>0 and TbPcPlan.RawMaterialDemandNum is not null
                        then (isnull(a.DemandNum,0)+isnull(a.SupplyNum,0))-isnull(TbPcPlan.BatchPlanQuantity,0) 
                        when (isnull(a.DemandNum,0)+isnull(a.SupplyNum,0))-isnull(TbPcPlan.BatchPlanQuantity,0)>0 and TbPcPlan.RawMaterialDemandNum is null
                        then (isnull(a.SupplyNum,0))-isnull(TbPcPlan.BatchPlanQuantity,0)
                        else 0 end as DemandNum,
                        a.SkillRequire,a.Remark,b.DictionaryText as MeasurementUnitText,isnull(TbQkl.Qkl,0) as Qkl
                        from TbRawMaterialMonthDemandSupplyPlanDetail a 
                        left join TbSysDictionaryData b on a.MeasurementUnit = b.DictionaryCode  
                        left join 
                        (
                        	select fbp.RawMaterialDemandNum,fbpi.RawMaterialNum,sum(fbpi.BatchPlanQuantity) as BatchPlanQuantity  
                        	from TbFactoryBatchNeedPlan fbp
                            left join TbFactoryBatchNeedPlanItem fbpi on fbp.BatchPlanNum=fbpi.BatchPlanNum 
                            where fbp.RawMaterialDemandNum like 'BCJH%' 
                            group by fbp.RawMaterialDemandNum,fbpi.RawMaterialNum
                        ) TbPcPlan on a.SupplyPlanCode=TbPcPlan.RawMaterialDemandNum and a.MaterialCode=TbPcPlan.RawMaterialNum
                        left join (select Tb1.MaterialCode,(isnull(Tb2.WeightSmallPlan,0)-isnull(Tb1.KcDtZl,0)) as Qkl from (select MaterialCode,ProjectId,TbStorage.ProcessFactoryCode,WorkAreaCode,TbCompany.ParentCompanyCode as BranchCode,SUM(LockCount)+SUM(UseCount) as KcDtZl
FROM TbRawMaterialStockRecord
left join TbStorage on TbRawMaterialStockRecord.StorageCode=TbStorage.StorageCode
left join TbCompany on TbRawMaterialStockRecord.WorkAreaCode=TbCompany.CompanyCode where 1=1 and TbRawMaterialStockRecord.ProjectId=@ProjectId and TbRawMaterialStockRecord.WorkAreaCode=@WorkAreaCode and TbStorage.ProcessFactoryCode=@ProcessFactoryCode and TbCompany.ParentCompanyCode=@BranchCode
group by MaterialCode,ProjectId,TbStorage.ProcessFactoryCode,WorkAreaCode,TbCompany.ParentCompanyCode) Tb1
left join (select od.MaterialCode,od.MaterialName,od.SpecificationModel,isnull(sum(od.WeightSmallPlan),0) as WeightSmallPlan,wo.ProjectId,cp1.ParentCompanyCode as WorkAreaCode,cp2.ParentCompanyCode as BranchCode,wo.ProcessFactoryCode from TbWorkOrder wo 
left join TbWorkOrderDetail od on wo.OrderCode=od.OrderCode
left join TbCompany cp1 on wo.SiteCode=cp1.CompanyCode
left join TbCompany cp2 on cp1.ParentCompanyCode=cp2.CompanyCode
where 1=1 and wo.Examinestatus='审核完成' and wo.ProcessingState='Received' and wo.ProjectId=@ProjectId and wo.ProcessFactoryCode=@ProcessFactoryCode and cp1.ParentCompanyCode=@WorkAreaCode and cp2.ParentCompanyCode=@BranchCode
group by od.MaterialCode,od.MaterialName,od.SpecificationModel,wo.ProjectId,cp1.ParentCompanyCode,cp2.ParentCompanyCode,wo.ProcessFactoryCode) Tb2 on Tb1.MaterialCode=Tb2.MaterialCode and Tb1.ProcessFactoryCode=Tb2.ProcessFactoryCode and Tb1.BranchCode=Tb2.BranchCode and Tb1.WorkAreaCode=Tb2.WorkAreaCode and Tb1.ProjectId=Tb2.ProjectId) TbQkl on a.MaterialCode=TbQkl.MaterialCode
                        where SupplyPlanCode=@number and b.FDictionaryCode='Unit') Tb where Tb.DemandNum>0";
            }
            var data = Db.Context.FromSql(sql)
                .AddInParameter("@number", DbType.String, number)
                .AddInParameter("@ProjectId", DbType.String, ProjectId)
                .AddInParameter("@ProcessFactoryCode", DbType.String, ProcessFactoryCode)
                .AddInParameter("@WorkAreaCode", DbType.String, WorkAreaCode)
                .AddInParameter("@BranchCode", DbType.String, BranchCode).ToDataTable();
            return data;
        }

        #endregion

    }
}
