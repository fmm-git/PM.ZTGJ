using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PM.DataEntity
{
    /// <summary>
    /// 加工厂批次需求计划查询字段实体类
    /// </summary>
    public class FPiCiXQPlan : PageSearchRequest
    {
        /// <summary>
        /// 原字段 加工厂
        /// </summary>
        public string CXFinishedFactoryName { get; set; }

        /// <summary>
        /// 原字段 分部/工区/站点
        /// </summary>
        public string CXfbgqzd { get; set; }

        /// <summary>
        /// 原 供应商名称
        /// </summary>
        public string CXSupplierName { get; set; }

        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderCode { get; set; }
        /// <summary>
        /// 车牌号
        /// </summary>
        public string PlateNumber { get; set; }
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
        /// 原材料名称（原材料库存-统计报表）
        /// </summary>
        public string MaterialNameSelect1 { get; set; }

        /// <summary>
        /// 规格型号（原材料库存-统计报表）
        /// </summary>
        public string SpecificationModel1 { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public string KSdatetime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public string JSdatetime { get; set; }

        ///// <summary>
        ///// 分部
        ///// </summary>
        //public string BranchCode { get; set; }
        ///// <summary>
        ///// 工区
        ///// </summary>
        //public string WorkAreaCode { get; set; }

        /// <summary>
        /// 供应商名称
        /// </summary>
        public string SupplierCode { get; set; }

        /// <summary>
        /// 组织机构类型
        /// </summary>
        public string OrgType { get; set; }
        /// <summary>
        /// 组织机构编号
        /// </summary>
        public string CompanyCode { get; set; }

        /// <summary>
        /// 当前登录用户Code
        /// </summary>
        public string UserCode { get; set; }
         /// <summary>
         /// 批次计划编号
         /// </summary>
        public string BatchPlanNum { get; set; }
        /// <summary>
        /// 供货状态
        /// </summary>
        public string StateCode { get; set; }
        /// <summary>
        /// 历史月份
        /// </summary>
        public DateTime? HistoryMonth { get; set; }
        /// <summary>
        /// 车牌号
        /// </summary>
        public string CarCph { get; set; }
        /// <summary>
        /// 类型编号
        /// </summary>
        public string TypeCode { get; set; }
        /// <summary>
        /// 月度需求计划编号
        /// </summary>
        public string DemandPlanCode { get; set; }
        /// <summary>
        /// 是否按时供货
        /// </summary>
        public string IsAsGh { get; set; }
        /// <summary>
        /// 质量问题类型
        /// </summary>
        public string ZlWtType { get; set; }
        /// <summary>
        /// 钢筋类型
        /// </summary>
        public string SteelsTypeCode { get; set; }
        /// <summary>
        /// 导出
        /// </summary>
        public bool IsOutPut { get; set; }
    }
}
