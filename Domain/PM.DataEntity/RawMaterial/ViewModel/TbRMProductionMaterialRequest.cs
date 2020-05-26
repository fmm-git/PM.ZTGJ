using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PM.DataEntity.Production.ViewModel
{
    public class TbRMProductionMaterialRequest : PageSearchRequest
    { 
        /// <summary>
        /// 领料单号
        /// </summary>
        public string CollarCode { get; set; }
        /// <summary>
        /// 加工订单
        /// </summary>
        public string OrderCode { get; set; }
        /// <summary>
        /// 历史月份
        /// </summary>
        public DateTime? HistoryMonth { get; set; }
        /// <summary>
        /// 使用部位
        /// </summary>
        public string CollarPosition { get; set; }
        /// <summary>
        /// 加工厂名称
        /// </summary>
        public string ProcessFactoryName { get; set; }
        /// <summary>
        /// 领用状态数量
        /// </summary>
        public int CollarStateNum { get; set; }
        /// <summary>
        /// 领用状态
        /// </summary>
        public string CollarState { get; set; }
        /// <summary>
        /// 分部编号
        /// </summary>
        public string BranchCode { get; set; }
        /// <summary>
        /// 分部名称
        /// </summary>
        public string BranchName { get; set; }
        /// <summary>
        /// 工区编号
        /// </summary>
        public string WorkAreaCode { get; set; }
        /// <summary>
        /// 工区名称
        /// </summary>
        public string WorkAreaName { get; set; }
        /// <summary>
        /// 站点名称
        /// </summary>
        public string SiteName { get; set; }
        /// <summary>
        /// 领料状态选中事件文本
        /// </summary>
        public string CollarStateSelected { get; set; }
        /// <summary>
        /// 加工订单类型编码
        /// </summary>
        public string TypeName { get; set; }
        public string TypeCode { get; set; }
        /// <summary>
        /// 领用人编号
        /// </summary>
        public string UserCode { get; set; }
        /// <summary>
        /// 领用人名称
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 在职状态
        /// </summary>
        public string InsertUserCode { get; set; }
        /// <summary>
        /// 联系电话
        /// </summary>
        public string MobilePhone { get; set; }
        /// <summary>
        /// 身份证号
        /// </summary>
        public string IDNumber { get; set; }
        /// <summary>
        /// 公司简称
        /// </summary>
        public string CompanyShortName { get; set; }
        /// <summary>
        /// 公司类型
        /// </summary>
        public int OrgType { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }
    }

    public class RMProductionMaterialRequest : PageSearchRequest
    {
        public string  keyword { get; set; }
        public string  OrderCode{ get; set; }
    }

    public class WorkOrderDetailDetailResponse
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int WorkOrderItemId { get; set; }
        /// <summary>
        /// 原材料名称/编号
        /// </summary>
        public string MaterialCode { get; set; }
        /// <summary>
        /// 规格型号
        /// </summary>
        public string SpecificationModel { get; set; }
        /// <summary>
        /// 单位重量
        /// </summary>
        public decimal MeasurementUnitZl { get; set; }
        /// <summary>
        /// 单位用量
        /// </summary>
        public decimal ItemUseNum { get; set; }
        /// <summary>
        /// 件数
        /// </summary>
        public int Number { get; set; }
        /// <summary>
        /// 件数
        /// </summary>
        public int NumberH { get; set; }
        /// <summary>
        /// 重量小计
        /// </summary>
        public decimal WeightSmallPlan { get; set; }
        /// <summary>
        /// 使用材料类型
        /// </summary>
        public string RMTypeName { get; set; }
        /// <summary>
        /// 尺寸选择
        /// </summary>
        public decimal SizeSelection { get; set; }
        /// <summary>
        /// 根数
        /// </summary>
        public int RootNumber { get; set; }

        public decimal WeightSmallPlanN { get; set; }
        /// <summary>
        /// 厂家
        /// </summary>
        public string Manufactor { get; set; }
        /// <summary>
        /// 炉批号
        /// </summary>
        public string HeatNo { get; set; }
        /// <summary>
        /// 检测报告编号
        /// </summary>
        public string TestReportNo { get; set; }
        /// <summary>
        /// 原材料名称/编号
        /// </summary>
        public string MaterialName { get; set; }
        /// <summary>
        /// 构件名称
        /// </summary>
        public string ComponentName { get; set; }
        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderCode { get; set; }
        public decimal WeightSmallPlanNH { get; set; }
        public List<RMProductionMaterialItmeBackRequest> plan { get; set; }
    }

    public class RMProductionMaterialItmeNumberRequest
    {
        public string MaterialCode { get; set; }

        /// <summary>
        /// 重量小计
        /// </summary>
        public decimal WeightSmallPlan { get; set; }
        /// <summary>
        /// 件数
        /// </summary>
        public int Number { get; set; }
    }

    public class RMProductionMaterialItmeBackRequest
    {
        /// <summary>
        /// 领料单号
        /// </summary>
        public string CollarCode { get; set; }
        /// <summary>
        /// 尺寸
        /// </summary>
        public decimal SizeSelection { get; set; }

        /// <summary>
        /// 可用根数
        /// </summary>
        public int UseNumber { get; set; }

        /// <summary>
        /// 领用根数
        /// </summary>
        public int RootNumber { get; set; }
        /// <summary>
        /// 多领根数
        /// </summary>
        public int RootNumberCL { get; set; }

        /// <summary>
        /// 领料件数
        /// </summary>
        public int NumberH { get; set; }

        /// <summary>
        /// 原材料编号
        /// </summary>
        public string MaterialCode { get; set; }
        public string MaterialName { get; set; }

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
        /// 使用材料类型
        /// </summary>
        public string RMTypeName { get; set; }

        /// <summary>
        /// 领料重量小计
        /// </summary>
        public decimal WeightSmallPlanN { get; set; }

        /// <summary>
        /// 加工订单明细Id
        /// </summary>
        public int WorkOrderItemId { get; set; }

        /// <summary>
        /// 库存Id
        /// </summary>
        public int StockID { get; set; }

        /// <summary>
        /// 件数
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// 方法类型 1剪切，2同尺寸，3不同尺寸，4拼接原材料，5自选余料，6自选原材料，7推荐原材料
        /// </summary>
        public int PlanIndex { get; set; }

        /// <summary>
        /// 标记
        /// </summary>
        public string LableStr { get; set; }

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
    }

    public class ReportPieResponse
    {
        public decimal Total { get; set; }
        public decimal yl { get; set; }
        public decimal ycl { get; set; }
        public decimal value { get; set; }
        public string text { get; set; }
    }
    public class SiteDataReport
    {
        public string name { get; set; }
        public List<decimal> data { get; set; }
    }
}
