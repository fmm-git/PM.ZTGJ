using PM.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.DataEntity.RawMaterial.ViewModel
{
    /// <summary>
    /// 列表收索
    /// </summary>
    public class StockTakingRequest : PageSearchRequest
    {
        /// <summary>
        /// 盘点单号
        /// </summary>
        public string TakNum { get; set; }
        /// <summary>
        /// 仓库类型
        /// </summary>
        public string WarehouseType { get; set; }
        /// <summary>
        /// 盘点开始时间
        /// </summary>
        public DateTime? TakDayS { get; set; }
        /// <summary>
        /// 盘点结束时间
        /// </summary>
        public DateTime? TakDayE { get; set; }
    }

    public class EndingStocksRequest
    {
        public EndingStocksRequest()
        {
            if (OperatorProvider.Provider.CurrentUser != null)
            {
                this.ProcessFactoryCode = OperatorProvider.Provider.CurrentUser.ProcessFactoryCode;
            }
        }

        /// <summary>
        /// 加工厂编码
        /// </summary>
        public string ProcessFactoryCode { get; set; }
        /// <summary>
        /// 原材料编号
        /// </summary>
        public string MaterialCode { get; set; }
        /// <summary>
        /// 原材料名称
        /// </summary>
        public string MaterialNameSelect { get; set; }
        /// <summary>
        /// 规格型号
        /// </summary>
        public string SpecificationModel { get; set; }
        /// <summary>
        /// 年份
        /// </summary>
        public int Year { get; set; }
        /// <summary>
        /// 月份
        /// </summary>
        public int Month { get; set; }

        public string ProjectId { get; set; }

        public string OrgType { get; set; }
    }

    #region 原材料总库存用量统计

    public class MaterialTotalStockReportModel
    {
        public string MaterialName { get; set; }
        public string SpecificationModel { get; set; }
        public string MaterialCode { get; set; }
        public string StorageCode { get; set; }
        public decimal Count { get; set; }
        public decimal WeightSmallPlan { get; set; }
        public decimal Point { get { return Count <= 0 ? 0 : Math.Ceiling((WeightSmallPlan / Count) * 100); } }
        public string RebarType { get; set; }
    }
    public class MaterialTotalStockReportRetModel
    {
        public List<string> Name { get; set; }
        public List<DataModel> Data1 { get; set; }
        public List<DataModel> Data2 { get; set; }
        public RebarTypeDataModel SumRebarTypeData { get; set; }
    }

    #endregion

    #region 原材料总库存及订单需求量历史分析

    public class MaterialTotalHistoryStockReportModel
    {
        public string Yar { get; set; }
        public string Mth { get; set; }
        public string Dayi { get; set; }
        public string MaterialCode { get; set; }
        public decimal Count { get; set; }
        public decimal WeightSmallPlan { get; set; }
    }
    public class MaterialTotalHistoryStockRetModel
    {
        public List<string> Name { get; set; }
        public List<decimal> Data1 { get; set; }
        public List<decimal> Data2 { get; set; }
    }

    #endregion

    #region 原材料用量总排行

    public class MaterialRankingListReportModel
    {
        public string MaterialName { get; set; }
        public string SpecificationModel { get; set; }
        public string MaterialCode { get; set; }
        public decimal WeightSmallPlan { get; set; }
    }
    public class MaterialRankingListReportRetModel
    {
        public List<string> Name { get; set; }
        public List<DataModel> Data { get; set; }
    }
    public class DataModel
    {
        public decimal y { get; set; }
        public string code { get; set; }
        public decimal point { get; set; }
        public string RebarType { get; set; }
    }
    public class RebarTypeDataModel
    {
        public decimal xg1 { get; set; }
        public decimal xg2 { get; set; }
        public decimal jzgj1 { get; set; }
        public decimal jzgj2 { get; set; }
    }
    #endregion

    #region 加工工艺用料总排行

    public class ProcessingRankingReportModel
    {
        public string ProcessingTechnologyName { get; set; }
        public int Yar { get; set; }
        public decimal WeightSmallPlan { get; set; }
    }
    public class ProcessingRankingRetModel
    {
        public List<string> Name { get; set; }
        public List<decimal> Data { get; set; }
    }

    #endregion
}
