using Dos.ORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace PM.DataEntity
{
    public class DistributionPlanRequest : PageSearchRequest
    {
        /// <summary>
        /// 标识ID
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// 编号
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public int Name { get; set; }

        /// <summary>
        /// 当前登录用户Code
        /// </summary>
        public string UserCode { get; set; }

        /// <summary>
        /// 弹框查询
        /// </summary>
        public string keyword { get; set; }

        public string PSJHCode { get; set; }
        /// <summary>
        /// 已经选择了的订单编号
        /// </summary>
        public string DisEntOrderCoder { get; set; }
    }

    /// <summary>
    /// 运输流程返回数据
    /// </summary>
    public class TransportFlowResponse
    {
        /// <summary>
        /// 配送装车单号
        /// </summary>
        public string DistributionCode { get; set; }
        /// <summary>
        /// 车牌号
        /// </summary>
        public string VehicleCode { get; set; }
        /// <summary>
        /// 驾驶员
        /// </summary>
        public string Driver { get; set; }
        /// <summary>
        /// 运输流程当前状态 1出厂 2进场 3开始卸货 4卸货完成 5出场
        /// </summary>
        public int FlowState { get; set; }
        /// <summary>
        /// 问题数
        /// </summary>
        public int Count { get; set; }
    }

    /// <summary>
    /// 运输流程返回数据
    /// </summary>
    public class TransportFlowResponseNew : Entity
    {
        /// <summary>
        /// 配送装车订单Id
        /// </summary>
        public int? DisEntOrderId { get; set; }
        /// <summary>
        /// 配送装车单号
        /// </summary>
        public string DistributionCode { get; set; }
        /// <summary>
        /// 站点名称
        /// </summary>
        public string SiteName { get; set; }
        /// <summary>
        /// 配送订单编号
        /// </summary>
        public string OrderCode { get; set; }
        /// <summary>
        /// 车辆编号
        /// </summary>
        public string VehicleCode { get; set; }
        /// <summary>
        /// 车牌号
        /// </summary>
        public string CarNumber { get; set; }
        /// <summary>
        /// 驾驶员Code
        /// </summary>
        public string Driver { get; set; }
        /// <summary>
        /// 驾驶员名称
        /// </summary>
        public string DriverName { get; set; }
        /// <summary>
        /// 运输流程当前状态 0未出厂 1出厂 6运输完成
        /// </summary>
        public int FlowState { get; set; }

        /// <summary>
        /// 问题数量
        /// </summary>
        public int ProblemCount { get; set; }
    }

    /// <summary>
    /// 运输流程提交数据
    /// </summary>
    public class TransportFlowRequest
    {
        /// <summary>
        /// 配送装车单号
        /// </summary>
        public string DistributionCode { get; set; }
        /// <summary>
        /// 附件
        /// </summary>
        public string Enclosure { get; set; }
        /// <summary>
        /// 1出厂 2进场 3开始卸货 4卸货完成 5出场
        /// </summary>
        public int FlowState { get; set; }
        /// <summary>
        /// 问题内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 车牌号
        /// </summary>
        public string VehicleCode { get; set; }
        /// <summary>
        /// 驾驶员
        /// </summary>
        public string Driver { get; set; }

        /// <summary>
        /// 当前登录用户Code
        /// </summary>
        public string UserCode { get; set; }

        /// <summary>
        /// 附件
        /// </summary>
        public string fileIds { get; set; }
    }
    [Serializable]
    /// <summary>
    /// 运输流程提交数据
    /// </summary>
    public class TransportFlowRequestNew
    {
        /// <summary>
        /// 配送装车单号
        /// </summary>
        public string DistributionCode { get; set; }
        /// <summary>
        /// 配送装车订单Id
        /// </summary>
        public int? DisEntOrderId { get; set; }
        /// <summary>
        ///0 未出厂 1出厂 6配送完成/ 1未进场 2进场 3开始卸货 4卸货完成 5出场
        /// </summary>
        public int FlowState { get; set; }
        /// <summary>
        /// 车牌号
        /// </summary>
        public string VehicleCode { get; set; }
        /// <summary>
        /// 驾驶员
        /// </summary>
        public string Driver { get; set; }
        /// <summary>
        /// 当前登录用户Code
        /// </summary>
        public string UserCode { get; set; }
        /// <summary>
        /// 附件
        /// </summary>
        public string Enclosure { get; set; }
        /// <summary>
        /// 附件
        /// </summary>
        public string fileIds { get; set; }
        /// <summary>
        /// 问题内容
        /// </summary>
        public string Content { get; set; }
    }

    /// <summary>
    /// 扫码二维码返回数据
    /// </summary>
    public class DistributionEntResponse
    {
        public WorkOrderInfoResponse WorkOrderInfo { get; set; }
        public WorkOrderDetailInfoResponse WorkOrderDetailInfo { get; set; }
    }

    /// <summary>
    /// 订单信息（扫码）
    /// </summary>
    public class WorkOrderInfoResponse
    {
        public int ID { get; set; }
        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderCode { get; set; }
        /// <summary>
        /// 类型编号
        /// </summary>
        public string TypeCode { get; set; }
        /// <summary>
        /// 类型名称
        /// </summary>
        public string TypeName { get; set; }
        /// <summary>
        /// 计划配送时间
        /// </summary>
        public string PlanDistributionTime { get; set; }
        /// <summary>
        /// 使用部位
        /// </summary>
        public string UsePart { get; set; }
        /// <summary>
        /// 配送地址
        /// </summary>
        public string DistributionAddress { get; set; }
        /// <summary>
        /// 站点编号
        /// </summary>
        public string SiteCode { get; set; }
        /// <summary>
        /// 站点名称
        /// </summary>
        public string SiteName { get; set; }

    }
    /// <summary>
    /// 订单明细信息（扫码）
    /// </summary>
    public class WorkOrderDetailInfoResponse
    {
        /// <summary>
        /// 订单明细id
        /// </summary>
        public int WorkorderdetailId { get; set; }
        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderCode { get; set; }
        /// <summary>
        /// 构件名称
        /// </summary>
        public string ComponentName { get; set; }
        /// <summary>
        /// 原材料编号
        /// </summary>
        public string MaterialCode { get; set; }
        /// <summary>
        /// 原材料名称
        /// </summary>
        public string MaterialName { get; set; }
        /// <summary>
        /// 规格
        /// </summary>
        public string SpecificationModel { get; set; }
        public string Standard { get { return SpecificationModel; } }
        /// <summary>
        /// 计量单位
        /// </summary>
        public string MeasurementUnitName { get; set; }
        public string MeteringUnit { get { return MeasurementUnitName; } }
        /// <summary>
        /// 单位重量
        /// </summary>
        public decimal MeasurementUnitZl { get; set; }
        public decimal UnitWeight { get { return MeasurementUnitZl; } }
        /// <summary>
        /// 单件用量
        /// </summary>
        public decimal ItemUseNum { get; set; }
        public decimal SingletonWeight { get { return ItemUseNum; } }
        /// <summary>
        /// 件数
        /// </summary>
        public int Number { get; set; }
        public int GjNumber { get { return Number; } }
        /// <summary>
        /// 重量小计
        /// </summary>
        public decimal WeightSmallPlan { get; set; }
        public decimal WeightGauge { get { return WeightSmallPlan; } }
        /// <summary>
        /// 打包件数
        /// </summary>
        public int PackNumber { get; set; }
        /// <summary>
        /// 打包包数
        /// </summary>
        public int PackagesNumber { get; set; }
        /// <summary>
        /// 可装车包数
        /// </summary>
        public int CanLoadingPackCount { get; set; }
        /// <summary>
        /// 本次装车包数
        /// </summary>
        public int ThisTimePackCount { get; set; }
        /// <summary>
        /// 本次装车件数
        /// </summary>
        public int CanLoadingJs { get; set; }
        /// <summary>
        /// 加工工艺
        /// </summary>
        public string ProcessingTechnology { get; set; }
        /// <summary>
        /// 加工工艺
        /// </summary>
        public string ProcessingTechnologyValue { get; set; }
        public string ProcessingTechnologyName { get { return ProcessingTechnologyValue; } }
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
        public string TestReport { get; set; }
    }

    /// <summary>
    /// 运输过程查询
    /// </summary>
    public class TransportProcessRequest : PageSearchRequest
    {
        /// <summary>
        /// 车辆配送编号
        /// </summary>
        public string DistributionCode { get; set; }
        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderCode { get; set; }
        /// <summary>
        /// 类型编号
        /// </summary>
        public string TypeCode { get; set; }
        /// <summary>
        /// 卸货过程状态 1正常 2等待超过30分钟
        /// </summary>
        public int FlowState { get; set; }
        /// <summary>
        /// 卸货存在问题
        /// </summary>
        public string IsProblem { get; set; }

        //是否首页展示
        public bool IsIndex { get; set; }

    }

    /// <summary>
    /// 问题填报数据
    /// </summary>
    public class ProblemReportRequest
    {
        /// <summary>
        /// 车辆配送编号
        /// </summary>
        public string DistributionCode { get; set; }
        /// <summary>
        /// 配送装车订单Id
        /// </summary>
        public int? DisEntOrderId { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 概要
        /// </summary>
        public string Abstract { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public int FlowState { get; set; }
        /// <summary>
        /// 录入人编号
        /// </summary>
        public string InsertUserCode { get; set; }
    }

    public class DistributionPlanInfoModel
    {
        public int ID { get; set; }
        public string OrderCode { get; set; }
        public string TypeCode { get; set; }
        public string TypeName { get; set; }
        public string ProcessFactoryCode { get; set; }
        public string SiteCode { get; set; }
        public string UsePart { get; set; }
        public string ProcessingState { get; set; }
        /// <summary>
        /// 计划配送时间
        /// </summary>
        public DateTime? DistributionTime { get; set; }
        public string DistributionAdd { get; set; }
        public decimal? WeightTotal { get; set; }
        public string UrgentDegree { get; set; }
        public string Remark { get; set; }
        public string Enclosure { get; set; }
        public string InsertUserCode { get; set; }
        public DateTime? InsertTime { get; set; }
        public string Examinestatus { get; set; }
        public string ProjectId { get; set; }
        public string OrderStart { get; set; }
        public string DistributionPlanCode { get; set; }
        /// <summary>
        /// 配送状态
        /// </summary>
        public string DistributionStart { get; set; }
        /// <summary>
        /// 配送时间状态
        /// </summary>
        public string DistributionTiemStart { get; set; }
        /// <summary>
        /// 配送完成时间
        /// </summary>
        public DateTime? DeliveryCompleteTime { get; set; }
        /// <summary>
        ///  是否线下订单 0否 1是
        /// </summary>
        public int IsOffline { get; set; }
        public string SiteName { get; set; }
        /// <summary>
        /// 配送状态
        /// </summary>
        public string ProcessingStateNew { get; set; }
        public string UserName { get; set; }
        public string ProcessFactoryName { get; set; }
        /// <summary>
        /// 紧急程度
        /// </summary>
        public string UrgentDegreeNew { get; set; }
        /// <summary>
        /// 实际配送时间
        /// </summary>
        public DateTime? LoadCompleteTime { get; set; }
        /// <summary>
        /// 签收状态
        /// </summary>
        public string SignState { get; set; }
        public string EnclosureSF { get; set; }
        public string SemiFinishedSignId { get; set; }
        /// <summary>
        /// 加工完成时间
        /// </summary>
        public string FinishProcessingDateTime { get; set; }
        /// <summary>
        /// 等待时长
        /// </summary>
        public string WaitTime { get; set; }
        /// <summary>
        /// 是否存在问题
        /// </summary>
        public string IsProblem { get; set; }

    }
}
