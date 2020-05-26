using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Business.BIExhibition
{
    /// <summary>
    /// 设备展示Logic
    /// </summary>
    public class EquipmentExhibitionLogic
    {
        /// <summary>
        /// 获取数据列表(批次计划明细)
        /// </summary>
//        public PageModel GetBatchPlanItemDataList(InOrderRequest request)
//        {
//            #region 搜索条件

//            List<Parameter> parameter = new List<Parameter>();
//            string where = " where 1=1";
//            if (!string.IsNullOrWhiteSpace(request.keyword))
//            {
//                where += " and a.RawMaterialNum like @RawMaterialNum";
//                parameter.Add(new Parameter("@RawMaterialNum", '%' + request.keyword + '%', DbType.String, null));
//            }
//            if (!string.IsNullOrWhiteSpace(request.BatchPlanCode))
//            {
//                where += " and a.BatchPlanNum=@BatchPlanCode";
//                parameter.Add(new Parameter("@BatchPlanCode", request.BatchPlanCode, DbType.String, null));
//            }
//            #endregion

//            var sql = @"select
//                            a.ID,
//                            a.Standard as SpecificationModel,
//                            a.RawMaterialNum as MaterialCode,
//                            a.MeasurementUnit,
//                            a.BatchPlanQuantity,
//                            b.DictionaryText as MeasurementUnitText,
//                            c.MaterialName,
//                            isnull(d.HasSupplier,0) as HasSupplier,
//                            isnull(e.PassCount,0)as TotalPassCount
//                            from TbFactoryBatchNeedPlanItem a
//                            left join TbSysDictionaryData b on a.MeasurementUnit=b.DictionaryCode
//                            left join TbRawMaterialArchives c on a.RawMaterialNum=c.MaterialCode
//                            left join TbSupplyListDetail d on a.RawMaterialNum=d.RawMaterialNum and a.BatchPlanNum=d.BatchPlanNum
//                            left join
//                            (
//                            select
//                            MaterialCode,
//                            isnull(SUM(PassCount),0)as PassCount
//                            from TbInOrderItem
//                            where InOrderCode in(select InOrderCode from TbInOrder where BatchPlanCode=@BatchPlanCode)
//                            group by MaterialCode
//                            ) e on a.RawMaterialNum=e.MaterialCode";
//            try
//            {
//                var data = Repository<TbFactoryBatchNeedPlanItem>.FromSqlToPageTable(sql + where, parameter, request.rows, request.page, "ID");
//                return data;
//            }
//            catch (Exception)
//            {
//                throw;
//            }
//        }
    }
}
