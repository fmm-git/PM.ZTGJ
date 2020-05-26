using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.DataEntity.System.ViewModel
{
    public class HomeRequest : PageSearchRequest
    {
        public HomeRequest()
        {
            this.DayMonth = DateTime.Now;
        }
        public HomeRequest(string projectId, string processFactoryCode, string siteCode, string year, string month)
        {
            this.ProjectId = projectId;
            this.ProcessFactoryCode = processFactoryCode;
            this.SiteCode = siteCode;
            if (!string.IsNullOrEmpty(year) && !string.IsNullOrEmpty(month))
                this.DayMonth = DateTime.Parse(year + "-"+month + "-01");
        }
        public DateTime DayMonth { get; set; }
        public string CodeList { get; set; }
        public string Year { get { return this.DayMonth.Year.ToString(); } }
        public string Month { get { return this.DayMonth.Month.ToString(); } }
        public string DayMonthStr { get { return this.DayMonth.ToString("yyyy-MM"); } }
    }

    public class DMYDataModel
    {
        /// <summary>
        /// 今日
        /// </summary>
        public DataModel DayData { get; set; }

        /// <summary>
        /// 本月
        /// </summary>
        public DataModel MonthData { get; set; }

        /// <summary>
        /// 今年
        /// </summary>
        public DataModel YearData { get; set; }
    }

    public class DataModel
    {
        public DataModel(int type, List<TbWorkOrder> receive, List<TbWorkOrderDetail> over)
        {
            if (type >= 2)//月
            {
                receive = receive.Where(p => p.InsertTime.Month == DateTime.Now.Month).ToList();
                over = over.Where(p => p.FinishTime.Value.Month == DateTime.Now.Month).ToList();
            }
            if (type == 3)//日
            {
                receive = receive.Where(p => p.InsertTime.Day == DateTime.Now.Day).ToList();
                over = over.Where(p => p.FinishTime.Value.Day == DateTime.Now.Day).ToList();
            }
            this.ConfirmWorkCount = receive.Count;
            this.ConfirmWorkTotal = receive.Sum(p => p.WeightTotal);
            this.FinishingCount = over.Select(p => p.OrderCode).Distinct().Count();
            this.FinishingTotal = over.Sum(p => p.WeightSmallPlan);
        }
        /// <summary>
        /// 接单数
        /// </summary>
        public int ConfirmWorkCount { get; set; }

        /// <summary>
        /// 接单总量
        /// </summary>
        public decimal ConfirmWorkTotal { get; set; }

        /// <summary>
        /// 加工完成数
        /// </summary>
        public int FinishingCount { get; set; }

        /// <summary>
        /// 加工完成总量
        /// </summary>
        public decimal FinishingTotal { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        public string DateTimeStr { get { return DateTime.Now.ToString("yyyy-MM-dd"); } }

    }

    /// <summary>
    /// 加工订单
    /// </summary>
    public class OrderDataModel
    {
        /// <summary>
        /// 接单
        /// </summary>
        public int OrderCount { get; set; }

        /// <summary>
        /// 接订单总量
        /// </summary>
        public decimal OrderTotal { get; set; }

        /// <summary>
        /// 加工完成
        /// </summary>
        public int OverCount { get; set; }

        /// <summary>
        /// 加工完成总量
        /// </summary>
        public decimal OverTotal { get; set; }

        /// <summary>
        /// 计划完成
        /// </summary>
        public int PlanCount { get; set; }

        /// <summary>
        /// 计划完成总量
        /// </summary>
        public decimal PlanTotal { get; set; }
        public int TepCount { get { return PlanCount - OverCount - Distribution - Sign; } }

        /// <summary>
        /// 未完成
        /// </summary>
        public int NotOver { get; set; }

        /// <summary>
        /// 已配送,配送超期
        /// </summary>
        public int Distribution { get; set; }

        /// <summary>
        /// 未配送
        /// </summary>
        public int NotDistribution { get; set; }

        /// <summary>
        /// 已签收
        /// </summary>
        public int Sign { get; set; }

        /// <summary>
        /// 未签收
        /// </summary>
        public int NotSign { get; set; }

        /// <summary>
        /// 进度滞后
        /// </summary>
        public int Lag { get; set; }
        public int LagCount { get; set; }
        public decimal LagWeight { get; set; }

        public int TotalCount { get; set; }

    }
    /// <summary>
    /// 生产计划数据
    /// </summary>
    public class PlanRawMaterialStockModel
    {
        public PlanRawMaterialStockModel()
        {
            MonthPlan = new RawMaterialStockModel();
            BatchPlan = new RawMaterialStockModel();
            SupplyData = new RawMaterialStockModel();
        }
        /// <summary>
        /// 月度计划
        /// </summary>
        public RawMaterialStockModel MonthPlan { get; set; }

        /// <summary>
        /// 批次计划
        /// </summary>
        public RawMaterialStockModel BatchPlan { get; set; }

        /// <summary>
        /// 原材料供应 
        /// </summary>
        public RawMaterialStockModel SupplyData { get; set; }
        public void LabShow()
        {
            //计划不足
            if (MonthToBatch.BuildingSteel.TotalWeight > (MonthPlan.BuildingSteel.TotalWeight * ((decimal)0.1)))
                SupplyData.BuildingSteel.TooFew = true;
            if (MonthToBatch.SectionSteel.TotalWeight > (MonthPlan.SectionSteel.TotalWeight * ((decimal)0.1)))
                SupplyData.SectionSteel.TooFew = true;
            //供货不足
            if (SupplyToBatch.BuildingSteel.TotalWeight < 0)
                SupplyData.BuildingSteel.TooFew = true;
            if (SupplyToBatch.SectionSteel.TotalWeight < 0)
                SupplyData.SectionSteel.TooFew = true;
        }

        /// <summary>
        /// 差额 (月度->批次)
        /// </summary>
        public RawMaterialStockModel MonthToBatch
        {
            get
            {
                return CreateDifferenceRetData(this.MonthPlan, this.BatchPlan);
            }
        }

        /// <summary>
        /// 差额 (供应 ->批次)
        /// </summary>
        public RawMaterialStockModel SupplyToBatch
        {
            get
            {
                return CreateDifferenceRetData(this.SupplyData, this.BatchPlan);
            }
        }

        private RawMaterialStockModel CreateDifferenceRetData(RawMaterialStockModel a, RawMaterialStockModel b)
        {
            var ret = new RawMaterialStockModel();
            //建筑钢筋
            var Building = new RebarTypeRetModel();
            if (a != null && b != null)
                Building.TotalWeight = GetDifferenceData(a.BuildingSteel, b.BuildingSteel);
            ret.BuildingSteel = Building;
            //型钢
            var Steel = new RebarTypeRetModel();
            if (a != null && b != null)
                Steel.TotalWeight = GetDifferenceData(a.SectionSteel, b.SectionSteel);
            ret.SectionSteel = Steel;
            return ret;
        }

        /// <summary>
        /// 计算差额
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private decimal GetDifferenceData(RebarTypeRetModel a, RebarTypeRetModel b)
        {
            var total = a.TotalWeight - b.TotalWeight;
            if (total < 0)
                a.TooFew = true;
            else if (total > 0)
                a.TooMuch = true;
            return total;
        }

    }

    /// <summary>
    /// 库存信息
    /// </summary>
    public class RawMaterialStockModel
    {
        public RawMaterialStockModel()
        {
            SectionSteel = new RebarTypeRetModel();
            BuildingSteel = new RebarTypeRetModel();
        }
        public RawMaterialStockModel(List<PlanDataModel> data)
        {
            //建筑钢筋
            var Building = new RebarTypeRetModel();
            if (data != null)
            {
                var b = data.Where(p => p.RebarType == "BuildingSteel").ToList();
                Building.TotalWeight = b.Sum(p => p.TotalWeight);
                Building.Count = b.Count;
            }
            this.BuildingSteel = Building;
            //型钢
            var Steel = new RebarTypeRetModel();
            if (data != null)
            {
                var s = data.Where(p => p.RebarType == "SectionSteel").ToList();
                Steel.TotalWeight = s.Sum(p => p.TotalWeight);
                Steel.Count = s.Count;
            }
            this.SectionSteel = Steel;
        }
        /// <summary>
        /// 型钢
        /// </summary>
        public RebarTypeRetModel SectionSteel { get; set; }

        /// <summary>
        /// 建筑钢筋
        /// </summary>
        public RebarTypeRetModel BuildingSteel { get; set; }
    }

    public class RebarTypeRetModel
    {
        public RebarTypeRetModel()
        {
        }
        /// <summary>
        /// 库存不足
        /// </summary>
        /// <param name="data"></param>
        public RebarTypeRetModel(List<RebarTypeModel> data)
        {
            if (data.Any())
            {
                this.Count = data.Count;
                this.TotalWeight = data.Sum(p => p.RetCount);
                int notCount = data.Where(p => p.TotalCount < 0).Count();
                this.TooFew = notCount > 0;
                this.TooFewCount = notCount;
            }
        }

        /// <summary>
        /// 总重量,差额
        /// </summary>
        public decimal TotalWeight { get; set; }
        public int Count { get; set; }
        public bool TooFew { get; set; }
        public bool TooMuch { get; set; }
        public int TooFewCount { get; set; }
        public int TooMuchCount { get; set; }
    }
    public class RebarTypeModel
    {
        public string MaterialCode { get; set; }
        public decimal UseCount { get; set; }
        public decimal LockCount { get; set; }
        public decimal TotalCount { get { return UseCount + LockCount; } }
        public decimal RetCount { get { return TotalCount > 0 ? TotalCount : 0; } }
    }
    public class PlanDataModel
    {
        public decimal TotalWeight { get; set; }
        public decimal BatchWeight { get; set; }
        public decimal RetWeight { get { return TotalWeight - BatchWeight; } }
        public string RebarType { get; set; }
        public string DemandPlanCode { get; set; }
        public string BatchPlanNum { get; set; }
        public string WorkAreaCode { get; set; }
        public string BranchCode { get; set; }
        public DateTime InsertTime { get; set; }
    }

    public class MonthPlanReportModel
    {
        public int TooFewCount { get; set; }
        public int TooMuchCount { get; set; }
        public int EqualCount { get { return TotalCount - (TooMuchCount + TooFewCount); } }
        public int OverTime { get; set; }
        public int TotalCount { get; set; }
        public string pCode { get; set; }
    }

    public class SupplyDataReportModel
    {
        public SupplyDataReportModel()
        {
            SectionSteel = new SupplyDataModel();
            BuildingSteel = new SupplyDataModel();
        }
        /// <summary>
        /// 型钢
        /// </summary>
        public SupplyDataModel SectionSteel { get; set; }

        /// <summary>
        /// 建筑钢筋
        /// </summary>
        public SupplyDataModel BuildingSteel { get; set; }

        public int TotalCount { get; set; }

    }
    public class SupplyDataModel
    {
        public SupplyDataModel()
        {
            OverTime = new RebarTypeRetModel();
            OnTime = new RebarTypeRetModel();
            OverTimeNoSupply = new RebarTypeRetModel();
            NoSupply = new RebarTypeRetModel();
        }
        /// <summary>
        /// 超时
        /// </summary>
        public RebarTypeRetModel OverTime { get; set; }
        /// <summary>
        /// 按时
        /// </summary>
        public RebarTypeRetModel OnTime { get; set; }
        /// <summary>
        /// 超时未供货
        /// </summary>
        public RebarTypeRetModel OverTimeNoSupply { get; set; }
        /// <summary>
        /// 未供货
        /// </summary>
        public RebarTypeRetModel NoSupply { get; set; }
        public string pCode { get; set; }
    }

    public class GuideData
    {
        /// <summary>
        /// 本月
        /// </summary>
        public FactoryData MonthInfo { get; set; }
        /// <summary>
        /// 综合
        /// </summary>
        public FactoryData TotalInfo { get; set; }
        /// <summary>
        /// 报表
        /// </summary>
        public FactoryItmeData ReportInfo { get; set; }
        /// <summary>
        /// 联系人
        /// </summary>
        public string Contacts { get; set; }
        /// <summary>
        /// 联系电话
        /// </summary>
        public string Tel { get; set; }
        /// <summary>
        /// 联系地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        public string DateTimeStr { get { return DateTime.Now.ToString("yyyy-MM-dd"); } }

    }
    public class FactoryData
    {
        /// <summary>
        /// 生产信息
        /// </summary>
        public FactoryItmeData Production { get; set; }
        /// <summary>
        /// 配送信息
        /// </summary>
        public FactoryItmeData Delivery { get; set; }
        /// <summary>
        /// 站点,接单数
        /// </summary>
        public int Site { get; set; }
        /// <summary>
        /// 工区
        /// </summary>
        public int WorkArea { get; set; }
        /// <summary>
        /// 接单重量
        /// </summary>
        public decimal TotalWeight { get; set; }
    }
    public class FactoryItmeData
    {
        /// <summary>
        /// 订单数
        /// </summary>
        public int OrderCount { get; set; }
        public decimal OrderRate { get; set; }
        public int OtherCount { get; set; }
        public decimal OtherRate { get; set; }
        public decimal TotalRate { get; set; }
    }

    public class OrgLabData
    {
        public string OrgCode { get; set; }
        public string pCode { get; set; }
        public int LeveL { get; set; }
        public List<OrgLab> LabList { get; set; }
    }
    public class OrgLab
    {
        public OrgLab()
        {
        }
        public OrgLab(string name, int value, string color, int Order=0)
        {
            this.Name = name;
            this.Value = value;
            this.Color = color;
            this.Order = Order;
        }
        public string Name { get; set; }
        public int Value { get; set; }
        public string Color { get; set; }
        public int Order { get; set; }
    }

    public class MoreReportModel
    {
        public string name { get; set; }
        public decimal y { get; set; }
    }

    public class LagDataModel
    {
        public decimal TotalWeight { get; set; }
        /// <summary>
        /// 滞后数量
        /// </summary>
        public int Number { get; set; }
        /// <summary>
        /// 工区数量
        /// </summary>
        public int TotalCount { get; set; }
        public string ParentCompanyCode { get; set; }
    }
}
