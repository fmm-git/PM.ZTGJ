using Dos.ORM;
using PM.Business.Production;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using PM.DataEntity.CostManage.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Business.CostManage
{
    /// <summary>
    /// 成本总报表
    /// </summary>
    public class CostReportLogic
    {
        private readonly TbWorkOrderLogic _workOrderLogic = new TbWorkOrderLogic();

        /// <summary>
        /// 获取分页列表数据(订单)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageModel GetDataListForPage(CostReportRequest request)
        {
            #region 模糊搜索条件
            var where = new Where<TbWorkOrder>();
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);
                where.And(p => p.SiteCode.In(SiteList));
            }
            if (!string.IsNullOrEmpty(request.ProjectId))
                where.And(p => p.ProjectId == request.ProjectId);
            #endregion

            try
            {
                var data = Db.Context.From<TbWorkOrder>()
                    .Select(
                      TbWorkOrder._.OrderCode
                    , TbWorkOrder._.SiteCode
                    , TbWorkOrder._.TypeCode
                    , TbWorkOrder._.TypeName
                    , TbCompany._.CompanyFullName)
                  .LeftJoin<TbCompany>((a, c) => a.SiteCode == c.CompanyCode)
                  .Where(where)
                  .OrderByDescending(p => p.ID)
                  .ToPageList(request.rows, request.page);
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 获取本月费用明细报表
        /// </summary>
        public Tuple<List<decimal>, List<decimal>> GetMonthReport(CostReportRequest request)
        {
            var dayS = DateTime.Now.AddDays(1 - DateTime.Now.Day).Date;
            var dayE = DateTime.Now.AddDays(1 - DateTime.Now.Day).Date.AddMonths(1).AddSeconds(-1);
            string ProjectId = "";
            if (!string.IsNullOrEmpty(request.ProjectId))
                ProjectId = request.ProjectId;
            SqlParameter[] par = new SqlParameter[] 
            {
                    new SqlParameter("@DateS",dayS),
                    new SqlParameter("@DateE",dayE),
                    new SqlParameter("@ProjectId",ProjectId),
            };
            try
            {
                var list = Db.Context.FromProc("CostMonthReport_Proc").AddParameter(par).ToList<decimal>();
                return GetData(list);
            }
            catch (Exception)
            {
                return new Tuple<List<decimal>, List<decimal>>(null, null);
            }
        }

        /// <summary>
        /// 获取订单费用明细报表
        /// </summary>
        public Tuple<List<decimal>, List<decimal>> GetOrderReport(CostReportRequest request)
        {
            string ProjectId = "";
            string SiteCode = "";
            if (!string.IsNullOrEmpty(request.ProjectId))
                ProjectId = request.ProjectId;
            if (!string.IsNullOrEmpty(request.SiteCode))
                SiteCode = request.SiteCode;
            SqlParameter[] par = new SqlParameter[] 
            {
                    new SqlParameter("@OrderCode",request.OrderCode),
                    new SqlParameter("@SiteCode",SiteCode),
                    new SqlParameter("@ProjectId",ProjectId),
            };
            try
            {
                var list = Db.Context.FromProc("CostOrderReport_Proc").AddParameter(par).ToList<decimal>();
                return GetData(list);
            }
            catch (Exception)
            {
                return new Tuple<List<decimal>, List<decimal>>(null, null);
            }
        }

        #region Private
        private Tuple<List<decimal>, List<decimal>> GetData(List<decimal> list)
        {
            var list2 = new List<decimal>();
            var total = list.Sum();
            if (total > 0)
            {
                list.ForEach(x =>
                {
                    if (x > 0)
                        list2.Add(decimal.Parse(((x / total) * 100).ToString("f5")));
                    else
                        list2.Add(0);
                });
            }
            else
            {
                list2 = new List<decimal>() { 0, 0, 0, 0 };
            }
            return new Tuple<List<decimal>, List<decimal>>(list, list2);
        }
        #endregion
    }
}
