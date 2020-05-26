using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PM.DataEntity.Production.ViewModel
{
    public class WorkOrderRequest : PageSearchRequest
    {
        ///// <summary>
        ///// 加工厂编号
        ///// </summary>
        //public string ProcessFactoryCode { get; set; }
        /// <summary>
        /// 类型编号
        /// </summary>
        public string TypeCode { get; set; }
        /// <summary>
        /// 加工状态
        /// </summary>
        public string ProcessingState { get; set; }
        /// <summary>
        /// 紧急程度
        /// </summary>
        public string UrgentDegree { get; set; }

        /// <summary>
        /// 组织机构类型
        /// </summary>
        public string OrgType { get; set; }
        /// <summary>
        /// 组织机构编号
        /// </summary>
        public string CompanyCode { get; set; }
        /// <summary>
        /// 审批状态
        /// </summary>
        public string Examinestatus { get; set; }
        /// <summary>
        /// 计划配送开始时间
        /// </summary>
        public DateTime? BegTime { get; set; }
        /// <summary>
        /// 计划配送结束时间
        /// </summary>
        public DateTime? EndTiem { get; set; }
        /// <summary>
        /// 订单状态类型
        /// </summary>
        public string OrderType { get; set; }
        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderCode { get; set; }
        /// <summary>
        /// 领料状态
        /// </summary>
        public string PickingState { get; set; }

        //public string ProjectId { get; set; }

        public string keyword { get; set; }
        /// <summary>
        /// 签收状态
        /// </summary>
        public string SignState { get; set; }
        /// <summary>
        /// 配送状态
        /// </summary>
        public string DistributionStart { get; set; }
        /// <summary>
        /// 历史月份
        /// </summary>
        public DateTime? HistoryMonth { get; set; }
        /// <summary>
        /// 订单状态
        /// </summary>
        public string OrderStart { get; set; }


        /// <summary>
        /// 查询类型（主表查询条件，还是报表查询）
        /// </summary>
        public string CxType { get; set; }
        /// <summary>
        /// 月份类型
        /// </summary>
        public string MonthType { get; set; }
        /// <summary>
        /// 是否未完成
        /// </summary>
        public bool IsNotOver { get; set; }
        /// <summary>
        /// 是否选中左边组织机构
        /// </summary>
        public string IsLeft { get; set; }
        /// <summary>
        /// 是否全部
        /// </summary>
        public string IsQB { get; set; }
        /// <summary>
        /// 订单进度状态
        /// </summary>
        public string OrderProcessingState { get; set; }
        public bool IsOutPut { get; set; }

    }
    public class WorkOrderDetail
    {
        public int ID { get; set; }
        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderCode { get; set; }
        /// <summary>
        /// 构件名称
        /// </summary>
        public string ComponentName { get; set; }
        /// <summary>
        /// 大样图
        /// </summary>
        public string LargePattern { get; set; }
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
        /// <summary>
        /// 计量单位
        /// </summary>
        public string MeasurementUnitText { get; set; }
        /// <summary>
        /// 计量单位编号
        /// </summary>
        public string MeasurementUnit { get; set; }
        /// <summary>
        /// 单位重量
        /// </summary>
        public decimal MeasurementUnitZl { get; set; }
        /// <summary>
        /// 重量小计
        /// </summary>
        public decimal ItemUseNum { get; set; }
        /// <summary>
        /// 件数
        /// </summary>
        public int Number { get; set; }
        /// <summary>
        /// 重量小计
        /// </summary>
        public decimal WeightSmallPlan { get; set; }
        /// <summary>
        /// 加工状态
        /// </summary>
        public string DaetailWorkStrat { get; set; }
        /// <summary>
        /// 撤销状态
        /// </summary>
        public string RevokeStart { get; set; }
        /// <summary>
        /// 技术要求
        /// </summary>
        public string SkillRequirement { get; set; }
        /// <summary>
        /// 加工工艺
        /// </summary>
        public int ProcessingTechnology { get; set; }
        /// <summary>
        /// 加工工艺名称
        /// </summary>
        public string ProcessingTechnologyName { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 打包数量
        /// </summary>
        public int PackNumber { get; set; }
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

        public int PackageNumber { get; set; }
    }
    public class WorkOrderPackResponse : PageSearchRequest
    {
        public int ID { get; set; }
        public string OrderCode { get; set; }
        public string TypeCode { get; set; }
        public string TypeName { get; set; }
        //public string SiteCode { get; set; }
        public string SiteName { get; set; }
        public DateTime? DistributionTime { get; set; }
        public string ComponentName { get; set; }
        public string Number { get; set; }
        public string PackNumber { get; set; }
        public string lldyNum { get; set; }
        public string syNum { get; set; }
        public string DistributionStart { get; set; }
    }

    /// <summary>
    /// 加工订单列表
    /// </summary>
    public class WorkOrderListModel
    {
        /// <summary>
        /// ID
        /// </summary>
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
        /// 加工厂编号
        /// </summary>
        public string ProcessFactoryCode { get; set; }
        /// <summary>
        /// 站点编号
        /// </summary>
        public string SiteCode { get; set; }
        /// <summary>
        /// 使用部位
        /// </summary>
        public string UsePart { get; set; }
        /// <summary>
        /// 加工状态
        /// </summary>
        public string ProcessingState { get; set; }
        /// <summary>
        /// 配送时间
        /// </summary>
        public string DistributionTime { get; set; }
        /// <summary>
        /// 配送地址
        /// </summary>
        public string DistributionAdd { get; set; }
        /// <summary>
        /// 总量合计
        /// </summary>
        public decimal WeightTotal { get; set; }
        /// <summary>
        /// 紧急程度
        /// </summary>
        public string UrgentDegree { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 附件
        /// </summary>
        public string Enclosure { get; set; }
        /// <summary>
        /// 录入人
        /// </summary>
        public string InsertUserCode { get; set; }
        /// <summary>
        /// 录入时间
        /// </summary>
        public string InsertTime { get; set; }
        /// <summary>
        /// 审批状态
        /// </summary>
        public string Examinestatus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ProjectId { get; set; }
        /// <summary>
        /// 订单状态
        /// </summary>
        public string OrderStart { get; set; }
        /// <summary>
        /// 领料状态
        /// </summary>
        public string PickingState { get; set; }
        /// <summary>
        ///  是否线下订单 0否 1是
        /// </summary>
        public int IsOffline { get; set; }
        public string SiteName { get; set; }
        public string ProcessingStateNew { get; set; }
        public string ExaminestatusNew { get; set; }
        public string UserName { get; set; }
        public string DistributionStart { get; set; }
        public string ProcessFactoryName { get; set; }
        public string UrgentDegreeNew { get; set; }
        public string LoadCompleteTime { get; set; }
        public string SignState { get; set; }
        public string EnclosureSF { get; set; }
        public string SemiFinishedSignId { get; set; }
        /// <summary>
        /// 加工完成时间
        /// </summary>
        public string FinishProcessingDateTime { get; set; }
    }
    public class SignStateListModel
    {
        public string OrderCode { get; set; }
        public int total { get; set; }       
        public int number { get; set; }
        public string LoadCompleteTime { get; set; }
    }
}
