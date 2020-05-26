using PM.Common;
using PM.Common.Extension;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace PM.DataEntity
{
    /// <summary>
    /// 初期库存查询条件
    /// </summary>
    public class RawMaterialStockRequest : PageSearchRequest
    {
        /// <summary>
        /// 原材料
        /// </summary>
        public string MaterialName { get; set; }
        public string MaterialCode { get; set; }
        /// <summary>
        /// 分部编号
        /// </summary>
        public string BranchCode { get; set; }
        /// <summary>
        /// 工区编号
        /// </summary>
        public string WorkAreaCode { get; set; }

        /// <summary>
        /// 规格型号
        /// </summary>
        public string SpecificationModel { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public string MaterialType { get; set; }
        /// <summary>
        /// 钢筋类型
        /// </summary>
        public string RebarType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string keyword { get; set; }

        /// <summary>
        /// 是否余料
        /// </summary>
        public bool IsYL { get; set; }

        /// <summary>
        /// 是否历史数据
        /// </summary>
        public bool IsOld { get; set; }
        /// <summary>
        /// 是否预警
        /// </summary>
        public bool IsEarly { get; set; }
        /// <summary>
        /// 是否缺口量
        /// </summary>
        public string IsQkl { get; set; }
    }

    /// <summary>
    /// 原材料到货入库查询条件
    /// </summary>
    public class InOrderRequest : PageSearchRequest
    {
        /// <summary>
        /// 入库单号
        /// </summary>
        public string InOrderCode { get; set; }

        /// <summary>
        /// 站点
        /// </summary>
        public string SiteName { get; set; }

        /// <summary>
        /// 供应商名称
        /// </summary>
        public string SupplierCode { get; set; }

        /// <summary>
        /// 入库仓库
        /// </summary>
        public string StorageCode { get; set; }

        /// <summary>
        /// 弹框收索条件
        /// </summary>
        public string keyword { get; set; }

        /// <summary>
        /// 批次计划编号
        /// </summary>
        public string BatchPlanCode { get; set; }

        /// <summary>
        /// 原材料编号
        /// </summary>
        public string MaterialCode { get; set; }

        /// <summary>
        /// 原材料编号
        /// </summary>
        public string MaterialCodeStr { get; set; }

        /// <summary>
        /// 工区
        /// </summary>
        public string WorkAreaCode { get; set; }
        /// <summary>
        /// 余料库存Ids
        /// </summary>
        public string CloutStockID { get; set; }
        public string CollarCode { get; set; }
        public int WorkOrderItemId { get; set; }
        public string Manufactor { get; set; }
        public string HeatNo { get; set; }
        /// <summary>
        /// 取样状态
        /// </summary>
        public string SampleOrderState { get; set; }
        /// <summary>
        /// 检测状态
        /// </summary>
        public string ChackResult { get; set; }
        /// <summary>
        /// 历史月份
        /// </summary>
        public DateTime? HistoryMonth { get; set; }
        /// <summary>
        /// 月度需求计划编号
        /// </summary>
        public string DemandPlanCode { get; set; }
        /// <summary>
        /// 钢筋类型
        /// </summary>
        public string RebarType { get; set; }
    }


    /// <summary>
    /// 原材料取样订单查询条件
    /// </summary>
    public class SampleOrderRequest : PageSearchRequest
    {

        /// <summary>
        /// 检测状态
        /// </summary>
        public string CheckStatus { get; set; }

        /// <summary>
        /// 弹框收索条件
        /// </summary>
        public string keyword { get; set; }

        /// <summary>
        /// 单号
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 入库编号
        /// </summary>
        public string InOrderCode { get; set; }
        /// <summary>
        /// 加工状态
        /// </summary>
        public int ProcessingState { get; set; }
        /// <summary>
        /// 检测结果
        /// </summary>
        public int ChackResult { get; set; }
        /// <summary>
        /// 历史月份
        /// </summary>
        public DateTime? HistoryMonth { get; set; }
        /// <summary>
        /// 月度需求计划编号
        /// </summary>
        public string DemandPlanCode { get; set; }
        /// <summary>
        /// 批次需求计划编号
        /// </summary>
        public string BatchPlanNum { get; set; }
        public bool IsOutPut { get; set; }
        public string RebarType { get; set; }

    }

    /// <summary>
    /// 余料库存查询条件
    /// </summary>
    public class CloutStockRequest : PageSearchRequest
    {
        /// <summary>
        /// ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 原材料
        /// </summary>
        public string MaterialName { get; set; }

        /// <summary>
        /// 原材料名称（余料流向）
        /// </summary>
        public string MaterialName1 { get; set; }

        /// <summary>
        /// 领料单号
        /// </summary>
        public string CollarCode { get; set; }
        public string SiteCodeS { get; set; }
        public string SiteCodeE { get; set; }
        public DateTime? PlaceTimeS { get; set; }
        public DateTime? PlaceTimeE { get; set; }

        /// <summary>
        /// 规格型号
        /// </summary>
        public string SpecificationModel { get; set; }

        /// <summary>
        /// 规格型号（余料流向）
        /// </summary>
        public string SpecificationModel1 { get; set; }
        /// <summary>
        /// 单位重量
        /// </summary>
        public decimal MeasurementUnitZl { get; set; }

        /// <summary>
        /// 弹框收索条件
        /// </summary>
        public string keyword { get; set; }
        /// <summary>
        /// 1.剪切 2.同尺寸 3.不同尺寸 4.拼接原材料 5自选余料，6自选原材料，7推荐原材料
        /// </summary>
        public int PlanIndex { get; set; }
        /// <summary>
        /// 1.剪切 2.拼接
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// 原材料编号
        /// </summary>
        public string MaterialCode { get; set; }
        /// <summary>
        /// 单件用量
        /// </summary>
        public decimal ItemUseNum { get; set; }
        /// <summary>
        /// 重量小计
        /// </summary>
        public decimal WeightSmallPlan { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// 加工订单明细Id
        /// </summary>
        public int WorkOrderItemId { get; set; }

        /// <summary>
        /// 是否自选
        /// </summary>
        public bool IsZX { get; set; }
        /// <summary>
        /// 工区编号
        /// </summary>
        public string WorkAreaCode { get; set; }
        public List<DetailsData> DetailsDatas
        {
            get
            {
                return JsonEx.JsonToObj<List<DetailsData>>(this.DetailsDatasStr);
            }
        }
        public string DetailsDatasStr { get; set; }

        /// <summary>
        /// 是否可以拼接
        /// </summary>
        public bool IsPJOk { get; set; }
        public string OrderDetailsDataStr { get; set; }

        public List<OrderDetailsData> OrderDetailsDatas
        {
            get
            {
                return JsonEx.JsonToObj<List<OrderDetailsData>>(this.OrderDetailsDataStr);
            }
        }

    }

    public class CloutStockReportRequest : PageSearchRequest
    {
        public string MaterialName { get; set; }
        public string SpecificationModel { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string OrgType { get; set; }

        public DateTime? HistoryMonth { get; set; }
    }
    public class DetailsData
    {
        public int StockID { get; set; }
        public int WorkOrderItemId { get; set; }
        public string RMTypeName { get; set; }
        public int NumberH { get; set; }
        public int RootNumber { get; set; }
        public decimal WeightSmallPlanN { get; set; }
        public decimal MeasurementUnitZl { get; set; }
        public decimal ItemUseNum { get; set; }
    }
    public class OrderDetailsData
    {
        public decimal ItemUseNum { get; set; }
        public decimal MeasurementUnitZl { get; set; }
        public int Number { get; set; }
        public int WorkOrderItemId { get; set; }
        public decimal WeightSmallPlan { get; set; }
        public string MaterialCode { get; set; }
        public string MaterialName { get; set; }
        public string SpecificationModel { get; set; }
    }

    public class CloutStockResponse : CloutStockYCLResponse
    {
        /// <summary>
        /// 库存ID
        /// </summary>
        public int StockID { get; set; }
        /// <summary>
        /// 尺寸
        /// </summary>
        public decimal SizeSelection { get; set; }

        /// <summary>
        /// 件数
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// 可用根数
        /// </summary>
        public int UseNumber { get; set; }

        /// <summary>
        /// 领料件数
        /// </summary>
        public int NumberH { get; set; }

        /// <summary>
        /// 领料件数
        /// </summary>
        public int NumberHH { get { return NumberH; } }

        /// <summary>
        /// 领用根数
        /// </summary>
        public int RootNumber { get; set; }

        /// <summary>
        /// 单位重量
        /// </summary>
        public decimal MeasurementUnitZl { get; set; }

        /// <summary>
        /// 单件用量
        /// </summary>
        public decimal ItemUseNum { get; set; }

        /// <summary>
        /// 重量小计
        /// </summary>
        public decimal WeightSmallPlan { get; set; }

        /// <summary>
        /// 领料重量小计
        /// </summary>
        public decimal WeightSmallPlanN { get; set; }

        /// <summary>
        /// 领料单号
        /// </summary>
        public string CollarCode { get; set; }

        /// <summary>
        /// 原材料编号
        /// </summary>
        public string MaterialCode { get; set; }
        public string MaterialName { get; set; }
        /// <summary>
        /// 材料类型
        /// </summary>
        public string RMTypeName { get; set; }

        /// <summary>
        /// 加工订单明细Id
        /// </summary>
        public int WorkOrderItemId { get; set; }

        /// <summary>
        /// 原材料重量
        /// </summary>
        public decimal Weight { get; set; }

        /// <summary>
        /// 规格
        /// </summary>
        public string SpecificationModel { get; set; }
        /// 厂家
        /// </summary>
        public string Factory { get; set; }
        /// <summary>
        /// 炉批号
        /// </summary>
        public string BatchNumber { get; set; }
        /// <summary>
        /// 检测报告编号
        /// </summary>
        public string TestReportNo { get; set; }

        /// <summary>
        /// 是否可以拼接
        /// </summary>
        public bool IsPJOk { get; set; }
    }

    public class CloutStockYCLResponse
    {
        /// <summary>
        /// 尺寸
        /// </summary>
        public decimal SizeSelectionycl { get; set; }

        /// <summary>
        /// 可用根数
        /// </summary>
        public int UseNumberycl { get; set; }

        /// <summary>
        /// 领料件数
        /// </summary>
        public int NumberHycl { get; set; }

        /// <summary>
        /// 领料件数
        /// </summary>
        public int NumberHyl { get; set; }

        /// <summary>
        /// 领用根数
        /// </summary>
        public int RootNumberycl { get; set; }

        /// <summary>
        /// 领料重量小计
        /// </summary>
        public decimal WeightSmallPlanNycl { get; set; }

        /// <summary>
        /// 材料类型
        /// </summary>
        public string RMTypeNameycl { get; set; }

        /// <summary>
        /// 标记
        /// </summary>
        public string LableStr { get; set; }

        public int StockIDstr { get; set; }

        /// <summary>
        /// 方法类型 1剪切，2同尺寸，3不同尺寸，4拼接原材料，5自选余料，6自选原材料
        /// </summary>
        public int PlanIndex { get; set; }

        /// <summary>
        /// 相差长度
        /// </summary>
        public decimal hxcd { get; set; }

        /// <summary>
        /// 相差长度
        /// </summary>
        public decimal hxcdLast { get; set; }

        /// <summary>
        /// 一个余料使用完拼成的件数
        /// </summary>
        public decimal CountLast { get; set; }

        /// <summary>
        /// 是否可再次拼接
        /// </summary>
        public bool ispj { get; set; }

        /// <summary>
        /// 拼接or剪切 true:拼接
        /// </summary>
        public bool pjorjq { get; set; }
    }

    public class YLPJData
    {
        /// <summary>
        /// 拼接成件数
        /// </summary>
        public decimal Count { get; set; }
        /// <summary>
        /// 余料根数
        /// </summary>
        public decimal CountIndex { get; set; }
        /// <summary>
        /// 剩余余料尺寸
        /// </summary>
        public decimal PJsize { get; set; }
        /// <summary>
        /// 剩余余料尺寸
        /// </summary>
        public decimal PJsizeLast { get; set; }

        /// <summary>
        /// 一个余料使用完拼成的件数
        /// </summary>
        public int CountLast { get; set; }
        /// <summary>
        /// 总件数(根数计算使用)
        /// </summary>
        public int Countret { get; set; }
        /// <summary>
        /// 最后
        /// </summary>
        public bool IsLast { get; set; }
    }

    /// <summary>
    /// 仓库管理查询条件
    /// </summary>
    public class StorageRequest : PageSearchRequest
    {
        /// <summary>
        /// 仓库名称
        /// </summary>
        public string StorageName { get; set; }
    }

    /// <summary>
    /// 供应商查询条件
    /// </summary>
    public class SupplierRequest : PageSearchRequest
    {
        /// <summary>
        /// 供应商名称
        /// </summary>
        public string SupplierName { get; set; }
    }

    /// <summary>
    /// 供应商查询条件
    /// </summary>
    public class CarInfoRequest : PageSearchRequest
    {
        /// <summary>
        /// 车辆编号
        /// </summary>
        public string CarCode { get; set; }
        /// <summary>
        /// 车牌号
        /// </summary>
        public string CarCph { get; set; }
        /// <summary>
        /// 所属加工厂编号
        /// </summary>
        public string ProcessFactoryCode { get; set; }

        public string keyword { get; set; }
    }

    public class RawMaterialStockRecordRequest
    {
        public RawMaterialStockRecordRequest()
        {
            if (OperatorProvider.Provider.CurrentUser != null)
                this.ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
        }
        /// <summary>
        /// 项目Id
        /// </summary>
        public string ProjectId { get; set; }

        /// <summary>
        /// 仓库编号
        /// </summary>
        public string StorageCode { get; set; }

        /// <summary>
        /// 站点编号
        /// </summary>
        public string SiteCode { get; set; }

        /// <summary>
        /// 模块编号
        /// </summary>
        public string ModelCode { get; set; }

        /// <summary>
        /// 操作类型
        /// </summary>
        public OperationEnum.OperationType OperationType { get; set; }
    }

    /// <summary>
    /// 原材料库存记录操作类型
    /// </summary>
    public class OperationEnum
    {
        /// <summary>
        /// 操作类型
        /// </summary>
        public enum OperationType
        {
            /// <summary>
            /// 增加
            /// </summary>
            [Description("增加")]
            Add = 1,

            /// <summary>
            /// 减少 
            /// </summary>
            [Description("减少")]
            Reduce = 2,

            /// <summary>
            /// 取消添加
            /// </summary>
            [Description("取消增加")]
            CancelAdd = 3,

            /// <summary>
            /// 取消减少
            /// </summary>
            [Description("取消减少")]
            CancelReduce = 4,
        }

        /// <summary>
        /// 材料类型
        /// </summary>
        public enum MaterialType
        {
            圆钢 = 9,
            螺纹钢 = 12,
        }
    }

    public class RawMaterialStockRecordResponse
    {
        /// <summary>
        /// id
        /// </summary>
        public int InOrderItemId { get; set; }
        /// <summary>
        /// 站点编号
        /// </summary>
        public string InOrderSiteName { get; set; }
        /// <summary>
        /// 库存数量
        /// </summary>
        public decimal Count { get; set; }
        /// <summary>
        /// 库存总量
        /// </summary>
        public decimal SurplusNumber { get { return this.Count > 0 ? this.Count : 0; } }

        /// <summary>
        /// 动态库存总量
        /// </summary>
        public decimal PassCount { get; set; }
        /// <summary>
        /// 厂家
        /// </summary>
        public string Factory { get; set; }
        /// <summary>
        /// 炉批号
        /// </summary>
        public string BatchNumber { get; set; }
        /// <summary>
        /// 质检报告
        /// </summary>
        public string TestReportNo { get; set; }
        /// <summary>
        /// 录入时间
        /// </summary>
        public DateTime InOrderTime { get; set; }
    }

    public class RawMaterialStockResponse
    {
        public string MaterialCode { get; set; }
        public string ProjectId { get; set; }
        public string MaterialType { get; set; }
        public string SiteCode { get; set; }
        public string StorageCode { get; set; }
        public string WorkAreaCode { get; set; }

        /// <summary>
        /// 库存数量
        /// </summary>
        public decimal Count { get; set; }
        /// <summary>
        /// 库存总量
        /// </summary>
        public decimal SurplusNumber { get { return this.PassCount >= 0 ? this.PassCount : 0; } }

        /// <summary>
        /// 动态库存总量
        /// </summary>
        public decimal PassCount { get { return LockCount + UseCount; } }

        /// <summary>
        /// 缺口量
        /// </summary>
        public decimal QKl { get; set; }

        /// <summary>
        /// 抵扣数量
        /// </summary>
        public decimal LockCount { get; set; }
        public decimal UseCount { get; set; }
        public string MaterialName { get; set; }
        public string SpecificationModel { get; set; }
        public string StorageName { get; set; }
        public string SiteName { get; set; }
        public string WorkAreaName { get; set; }
        public string BranchName { get; set; }
        public string MaterialTypeText { get; set; }
        public string RebarTypeText { get; set; }
        public string MeasurementUnitText { get; set; }

    }

    public class RawMaterialStockItemResponse
    {
        /// <summary>
        /// 库存数量
        /// </summary>
        public decimal Count { get; set; }

        /// <summary>
        /// 动态库存总量
        /// </summary>
        public decimal PassCount { get { return LockCount + UseCount; } }
        /// <summary>
        /// 库存总量
        /// </summary>
        public decimal SurplusNumber { get { return this.PassCount >= 0 ? this.PassCount : 0; } }
        /// <summary>
        /// 抵扣数量
        /// </summary>
        public decimal LockCount { get; set; }
        public decimal UseCount { get; set; }
        public string Factory { get; set; }
        public string BatchNumber { get; set; }
        public string InsertTime { get; set; }
        public int ID { get; set; }
        public decimal HistoryCount { get; set; }
        public int ChackState { get; set; }
        public string Enclosure { get; set; } 
        public string TestReportNo { get; set; }
    }
}
