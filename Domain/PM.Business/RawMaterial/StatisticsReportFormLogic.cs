using Dos.ORM;
using PM.Business.Production;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using PM.DataEntity.RawMaterial.ViewModel;
using PM.DataEntity.System.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Business.RawMaterial
{
    public class StatisticsReportFormLogic
    {
        private readonly TbWorkOrderLogic _workOrderLogic = new TbWorkOrderLogic();

        #region 收发库存报表

        //原材料名称查询
        public List<TbRawMaterialArchives> MaterialNameSelect()
        {
            try
            {
                string sql = @"SELECT MaterialName FROM TbRawMaterialArchives
                                GROUP BY MaterialName";
                return Db.Context.FromSql(sql).ToList<TbRawMaterialArchives>();
            }
            catch (Exception)
            {

                throw;
            }
        }

        //        public Tuple<DataTable, DataTable, DataTable> GetReportForm1(StatisticsReportFormRequest request)
        //        {

        //            #region 模糊搜索条件

        //            string where = "";
        //            string where2 = "";
        //            if (!string.IsNullOrWhiteSpace(request.MaterialName))
        //            {
        //                where += " and detail.MaterialName like '%" + request.MaterialName + "%'";

        //            }
        //            if (request.SpecificationModel != null)
        //            {
        //                where += " and detail.SpecificationModel like '%" + request.SpecificationModel + "%'";
        //            }
        //            where2 = where;
        //            if (!string.IsNullOrWhiteSpace(request.SiteCode))
        //            {
        //                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);
        //                string siteStr = string.Join("','", SiteList);
        //                where += " and com.CompanyCode in('" + siteStr + "')";
        //                where2 += " and detail.EndSiteCode in('" + siteStr + "')";
        //            }

        //            #endregion

        //            var ret1 = @"select isnull(sum(detail.PassCount),0) as FeedNum from TbInOrderItem detail
        //left join TbInOrder ino on detail.InOrderCode=ino.InOrderCode
        //left join TbCompany com on ino.SiteCode=com.CompanyCode where 1=1 and (ISNULL(@ProjectId,'')='' OR ino.ProjectId=@ProjectId)  ";
        //            DataTable dt1 = Db.Context.FromSql(ret1 + where).AddInParameter("@ProjectId", DbType.String, request.ProjectId).ToDataTable();

        //            var ret2 = @"select isnull(SUM(detail.WeightSmallPlan),0) RawMaterialNum from TbRMProductionMaterialDetail detail
        //left join TbRMProductionMaterial pm on detail.CollarCode=pm.CollarCode
        //left join TbCompany com on pm.SiteCode=com.CompanyCode where 1=1 and (ISNULL(@ProjectId,'')='' OR pm.ProjectId=@ProjectId)  ";
        //            DataTable dt2 = Db.Context.FromSql(ret2 + where).AddInParameter("@ProjectId", DbType.String, request.ProjectId).ToDataTable();

        //            var ret3 = @"select 
        //                        sum(detail.Weight) YLNum
        //                        from TbCloutStockPlace detail
        //                        where detail.State=1 and (ISNULL(@ProjectId,'')='' OR detail.ProjectId=@ProjectId) {where}
        //                        group by detail.EndSiteCode";
        //            ret3 = ret3.Replace("{where}", where2);
        //            DataTable dt3 = Db.Context.FromSql(ret3).AddInParameter("@ProjectId", DbType.String, request.ProjectId).ToDataTable();

        //            return new Tuple<DataTable, DataTable, DataTable>(dt1, dt2, dt3);
        //        }
        /// <summary>
        /// 进料与发料分析报表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Tuple<DataTable> GetReportForm1(StatisticsReportFormRequest request)
        {
            string where = "";
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);//站点
                string siteCodeStr = string.Join("','", SiteList);
                List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 4);//工区
                string workAreaStr = string.Join("','", WorkAreaList);
                where += " and (WorkAreaCode in('" + workAreaStr + "') or SiteCode in('" + siteCodeStr + "'))";
            }


            string sql = @"select isnull(sum(inoi.PassCount),0) as FeedNum from TbInOrder ino
                           left join TbInOrderItem inoi on ino.InOrderCode=inoi.InOrderCode
                           where ino.Examinestatus='审核完成' and inoi.ChackState=1
                           and (ISNULL(@BegTime,'')='' or ino.InsertTime>=CONVERT(varchar,@BegTime,101)) 
                           and (ISNULL(@EndTime,'')='' or ino.InsertTime<=@EndTime)
                           and (ISNULL(@MaterialName,'')='' or inoi.MaterialName like '%'+@MaterialName+'%')
                           and (ISNULL(@SpecificationModel,'')='' or inoi.SpecificationModel like '%'+@SpecificationModel+'%')
                           and (ISNULL(@ProjectId,'')='' or ino.ProjectId=@ProjectId)
                            " + where + @"
                           union all
                           select isnull(sum(rmpmp.WeightSmallPlanN),0) as YLNum from TbRMProductionMaterial rmpm
                           left join TbRMProductionMaterialDetail rmpmd on rmpm.CollarCode=rmpmd.CollarCode
                           left join TbRMProductionMaterialPlan rmpmp on rmpmd.CollarCode=rmpmp.CollarCode and rmpmd.MaterialCode=rmpmp.MaterialCode and rmpmd.WorkOrderItemId=rmpmp.WorkOrderItemId
                           left join TbRawMaterialArchives rma on rmpmp.MaterialCode=rma.MaterialCode
                           where rmpm.Examinestatus='审核完成'
                           and rmpmp.RMTypeName='余料' 
                           and (ISNULL(@BegTime,'')='' or rmpm.CollarDate>=@BegTime)
                           and (ISNULL(@EndTime,'')='' or rmpm.CollarDate<=@EndTime)
                           and (ISNULL(@ProjectId,'')='' or rmpm.ProjectId=@ProjectId)
                           and (ISNULL(@ProcessFactoryCode,'')='' or rmpm.ProcessFactoryCode=@ProcessFactoryCode)
                            " + where + @"
                           and (ISNULL(@MaterialName,'')='' or rma.MaterialName like '%'+@MaterialName+'%')
                           and (ISNULL(@SpecificationModel,'')='' or rma.SpecificationModel like '%'+@SpecificationModel+'%')
                           union all
                           select sum(Tb.RawMaterialNum) as RawMaterialNum from (select isnull(sum(rmpmp.WeightSmallPlanN),0) as RawMaterialNum from TbRMProductionMaterial rmpm
						   left join TbRMProductionMaterialDetail rmpmd on rmpm.CollarCode=rmpmd.CollarCode
						   left join TbRMProductionMaterialPlan rmpmp on rmpmd.CollarCode=rmpmp.CollarCode and rmpmd.MaterialCode=rmpmp.MaterialCode and rmpmd.WorkOrderItemId=rmpmp.WorkOrderItemId
						   left join TbRawMaterialArchives rma on rmpmp.MaterialCode=rma.MaterialCode
						   where  rmpm.Examinestatus='审核完成'
						   and rmpmp.RMTypeName='原材料' 
						   and (ISNULL(@BegTime,'')='' or rmpm.CollarDate>=@BegTime)
						   and (ISNULL(@EndTime,'')='' or rmpm.CollarDate<=@EndTime)
						   and (ISNULL(@ProjectId,'')='' or rmpm.ProjectId=@ProjectId)
						   and (ISNULL(@ProcessFactoryCode,'')='' or rmpm.ProcessFactoryCode=@ProcessFactoryCode)
						   " + where + @"
						   and (ISNULL(@MaterialName,'')='' or rma.MaterialName like '%'+@MaterialName+'%')
						   and (ISNULL(@SpecificationModel,'')='' or rma.SpecificationModel like '%'+@SpecificationModel+'%')
						   union all
						   select isnull(sum(rpmd.WeightSmallPlanN),0) as RawMaterialNum from TbRMProductionMaterialDetail rpmd 
						   left join TbRMProductionMaterial rpm on rpmd.CollarCode=rpm.CollarCode
						   where  rpm.Examinestatus='审核完成'
						   and rpmd.WorkOrderItemId not in(select WorkOrderItemId from TbRMProductionMaterialPlan)
						   and (ISNULL(@BegTime,'')='' or rpm.CollarDate>=@BegTime)
						   and (ISNULL(@EndTime,'')='' or rpm.CollarDate<=@EndTime)
						   and (ISNULL(@ProjectId,'')='' or rpm.ProjectId=@ProjectId)
						   and (ISNULL(@ProcessFactoryCode,'')='' or rpm.ProcessFactoryCode=@ProcessFactoryCode)
						   " + where + @"
						   and (ISNULL(@MaterialName,'')='' or rpmd.MaterialName like '%'+@MaterialName+'%')
						   and (ISNULL(@SpecificationModel,'')='' or rpmd.SpecificationModel like '%'+@SpecificationModel+'%')) Tb";
            DataTable dt1 = Db.Context.FromSql(sql)
                .AddInParameter("@BegTime", DbType.DateTime, request.BegTime)
                .AddInParameter("@EndTime", DbType.DateTime, request.EndTime)
                .AddInParameter("@ProjectId", DbType.String, request.ProjectId)
                .AddInParameter("@ProcessFactoryCode", DbType.String, request.ProcessFactoryCode)
                .AddInParameter("@MaterialName", DbType.String, request.MaterialName)
                .AddInParameter("@SpecificationModel", DbType.String, request.SpecificationModel)
                .ToDataTable();
            return new Tuple<DataTable>(dt1);
        }
        /// <summary>
        /// 获取数据列表(余料发料量统计表)
        /// </summary>
        public PageModel GetYLDataList(StatisticsReportFormRequest request)
        {
            //#region 搜索条件

            //List<Parameter> parameter = new List<Parameter>();
            //string where = " where 1=1";
            //parameter.Add(new Parameter("@ProjectId", request.ProjectId, DbType.String, null));
            //if (!string.IsNullOrWhiteSpace(request.SiteCode))
            //{
            //    List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);
            //    string siteStr = string.Join("','", SiteList);
            //    where += " and a.EndSiteCode in('" + siteStr + "')";
            //}
            //if (!string.IsNullOrWhiteSpace(request.MaterialName))
            //{
            //    where += " and b.MaterialName like @MaterialName";
            //    parameter.Add(new Parameter("@MaterialName", "%" + request.MaterialName + "%", DbType.String, null));
            //}
            //#endregion

            //            var sql = @"select 
            //                            a.RootNumber,
            //                            a.SizeSelection,
            //                            a.MaterialCode,
            //                            a.WeightSmallPlanN,
            //                            b.MaterialName,
            //                            b.SpecificationModel,
            //                            b.MeasurementUnitZl,
            //                            c.CompanyFullName StartSiteName,
            //                            d.CompanyFullName EndSiteName
            //                            from 
            //                            (
            //                            select 
            //                            a.MaterialCode,
            //                            a.Size as SizeSelection,
            //                            a.StartSiteCode,
            //                            a.EndSiteCode,
            //                            sum(a.Number) RootNumber,
            //                            sum(a.Weight) WeightSmallPlanN
            //                            from TbCloutStockPlace a
            //                            where a.State=1 and (ISNULL(@ProjectId,'')='' OR a.ProjectId=@ProjectId)
            //                            group by a.MaterialCode,a.Size,a.StartSiteCode,a.EndSiteCode
            //                            ) a
            //                            left join TbRawMaterialArchives b on a.MaterialCode=b.MaterialCode
            //                            left join TbCompany c on c.CompanyCode=a.StartSiteCode
            //                            left join TbCompany d on d.CompanyCode=a.EndSiteCode";
            string where = "";
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);//站点
                string siteCodeStr = string.Join("','", SiteList);
                List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 4);//工区
                string workAreaStr = string.Join("','", WorkAreaList);
                where += " and a.EndSiteCode in('" + siteCodeStr + "')";
            }
            string sql = @"select 
                           a.RootNumber,
                           a.SizeSelection,
                           a.MaterialCode,
                           a.WeightSmallPlanN,
                           b.MaterialName,
                           b.SpecificationModel,
                           b.MeasurementUnitZl,
                           c.CompanyFullName StartSiteName,
                           d.CompanyFullName EndSiteName
                           from 
                           (
                           select 
                           a.MaterialCode,
                           a.Size as SizeSelection,
                           a.StartSiteCode,
                           a.EndSiteCode,
                           sum(a.Number) RootNumber,
                           sum(a.Weight) WeightSmallPlanN
                           from TbCloutStockPlace a
                           left join TbRMProductionMaterial rmpm on a.CollarCode=rmpm.CollarCode
                           where a.State=1 
                           and (ISNULL(@ProjectId,'')='' OR a.ProjectId=@ProjectId)
                           and (ISNULL(@BegTime,'')='' or rmpm.CollarDate>=@BegTime)
                           and (ISNULL(@EndTime,'')='' or rmpm.CollarDate<=@EndTime)
                           and (ISNULL(@ProjectId,'')='' or rmpm.ProjectId=@ProjectId)
                           and (ISNULL(@ProcessFactoryCode,'')='' or rmpm.ProcessFactoryCode=@ProcessFactoryCode)
                           " + where + @"
                           group by a.MaterialCode,a.Size,a.StartSiteCode,a.EndSiteCode
                           ) a
                           left join TbRawMaterialArchives b on a.MaterialCode=b.MaterialCode
                           left join TbCompany c on c.CompanyCode=a.StartSiteCode
                           left join TbCompany d on d.CompanyCode=a.EndSiteCode
                           where 1=1 
                           and (ISNULL(@MaterialName,'')='' or b.MaterialName like '%'+@MaterialName+'%')
                           and (ISNULL(@SpecificationModel,'')='' or b.SpecificationModel like '%'+@SpecificationModel+'%')";
            //参数化
            List<Dos.ORM.Parameter> para = new List<Dos.ORM.Parameter>();
            para.Add(new Parameter("@ProjectId", request.ProjectId, DbType.String, null));
            para.Add(new Parameter("@ProcessFactoryCode", request.ProcessFactoryCode, DbType.String, null));
            para.Add(new Parameter("@MaterialName", request.MaterialName, DbType.String, null));
            para.Add(new Parameter("@SpecificationModel", request.SpecificationModel, DbType.String, null));
            para.Add(new Parameter("@BegTime", request.BegTime, DbType.DateTime, null));
            para.Add(new Parameter("@EndTime", request.EndTime, DbType.DateTime, null));
            try
            {
                var data = Repository<TbFactoryBatchNeedPlanItem>.FromSqlToPageTable(sql, para, request.rows, request.page, "MaterialCode");
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// 列表数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageModel GetTransceiverGridJson(StatisticsReportFormRequest request)
        {
            #region 模糊搜索条件
            string where = " where 1=1 ";
            if (!string.IsNullOrWhiteSpace(request.MaterialName))
            {
                where += " and Tb1.MaterialName like '%" + request.MaterialName + "%'";
            }
            if (request.SpecificationModel != null)
            {
                where += " and Tb1.SpecificationModel like '%" + request.SpecificationModel + "%'";
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 4);
                string workAreaStr = string.Join("','", WorkAreaList);
                where += " and Tb1.WorkAreaCode in('" + workAreaStr + "')";
            }
            #endregion

            string sql = @"select * from (
	select Tb.*,isnull(TbXqPlan.DemandNum,0) as LjDemandNum,isnull(TbPcPlan.BatchPlanQuantity,0) as LjBatchPlanQuantity,isnull(TbGh.HasSupplier,0) as LjHasSupplier,isnull(ProcessFinishData.HistoryMonthCount,0) as HistoryMonthCount from (select rs.MaterialCode,rs.MaterialName,rs.SpecificationModel,cp1.CompanyCode as BranchCode,cp1.CompanyFullName as BranchName,rs.WorkAreaCode,cp.CompanyFullName as WorkAreaName,sum(rs.Count) as InitialCount,rs.ProjectId from TbRawMaterialStockRecord rs
                           left join TbCompany cp on cp.CompanyCode=rs.WorkAreaCode
                           left join TbCompany cp1 on cp1.CompanyCode=cp.ParentCompanyCode
                           left join TbStorage s on rs.StorageCode=s.StorageCode
                           where (ISNULL(@ProjectId,'')='' or rs.ProjectId=@ProjectId)
                           and (ISNULL(@ProcessFactoryCode,'')='' or s.ProcessFactoryCode=@ProcessFactoryCode)
                           and rs.ChackState=1
                           group by rs.MaterialCode,rs.MaterialName,rs.SpecificationModel,cp1.CompanyCode,cp1.CompanyFullName,rs.WorkAreaCode,cp.CompanyFullName,rs.ProjectId) Tb
                           left join (select rmpd.MaterialCode,rmp.WorkAreaCode,rmp.ProjectId,rmp.ProcessFactoryCode,sum(rmpd.DemandNum) as DemandNum from TbRawMaterialMonthDemandPlan rmp
                           left join TbRawMaterialMonthDemandPlanDetail rmpd on rmp.DemandPlanCode=rmpd.DemandPlanCode 
                           where rmp.Examinestatus='审核完成' 
                           and rmp.DemandPlanCode not in(select DemandPlanCode from TbRawMaterialMonthDemandSupplyPlan where Examinestatus='审核完成') 
                           and (ISNULL(@Year,'')='' or (YEAR(rmp.DeliveryDate)=@Year and MONTH(rmp.DeliveryDate)=@Month))
                           and (ISNULL(@ProcessFactoryCode,'')='' or rmp.ProcessFactoryCode=@ProcessFactoryCode) 
                           and (ISNULL(@ProjectId,'')='' or rmp.ProjectId=@ProjectId) group by rmpd.MaterialCode,rmp.WorkAreaCode,rmp.ProjectId,rmp.ProcessFactoryCode
                           union all
                           select rmspd.MaterialCode,rmsp.WorkAreaCode,rmsp.ProjectId,rmsp.ProcessFactoryCode,sum((rmspd.SupplyNum+rmspd.DemandNum)) as DemandNum  from TbRawMaterialMonthDemandSupplyPlan rmsp
                           left join TbRawMaterialMonthDemandSupplyPlanDetail rmspd on rmsp.SupplyPlanCode=rmspd.SupplyPlanCode 
                           where rmsp.Examinestatus='审核完成'
                           and (ISNULL(@Year,'')='' or (YEAR(rmsp.SupplyTime)=@Year and MONTH(rmsp.SupplyTime)=@Month))
                           and (ISNULL(@ProcessFactoryCode,'')='' or rmsp.ProcessFactoryCode=@ProcessFactoryCode) 
                           and (ISNULL(@ProjectId,'')='' or rmsp.ProjectId=@ProjectId) group by rmspd.MaterialCode,rmsp.WorkAreaCode,rmsp.ProjectId,rmsp.ProcessFactoryCode) TbXqPlan 
                           on Tb.WorkAreaCode=TbXqPlan.WorkAreaCode and Tb.MaterialCode=TbXqPlan.MaterialCode
                           left join (select fbpi.RawMaterialNum,fbp.WorkAreaCode,fbp.ProjectId,fbp.ProcessFactoryCode,sum(fbpi.BatchPlanQuantity) as BatchPlanQuantity from TbFactoryBatchNeedPlan fbp
                           left join TbFactoryBatchNeedPlanItem fbpi on fbp.BatchPlanNum=fbpi.BatchPlanNum where fbp.Examinestatus='审核完成'
                           and (ISNULL(@Year,'')='' or (YEAR(fbp.ArrivalDate)=@Year and MONTH(fbp.ArrivalDate)=@Month))
                           and (ISNULL(@ProcessFactoryCode,'')='' or fbp.ProcessFactoryCode=@ProcessFactoryCode) 
                           and (ISNULL(@ProjectId,'')='' or fbp.ProjectId=@ProjectId) group by fbpi.RawMaterialNum,fbp.WorkAreaCode,fbp.ProjectId,fbp.ProcessFactoryCode) TbPcPlan 
                           on Tb.WorkAreaCode=TbPcPlan.WorkAreaCode and Tb.MaterialCode=TbPcPlan.RawMaterialNum
                           left join (select sld.RawMaterialNum,sl.WorkAreaCode,sl.ProjectId,sl.ProcessFactoryCode,sum(sld.HasSupplier) as HasSupplier from TbSupplyList sl
                           left join TbSupplyListDetail sld on sl.BatchPlanNum=sld.BatchPlanNum where 1=1
                           and (ISNULL(@Year,'')='' or (YEAR(sl.SupplyDate)=@Year and MONTH(sl.SupplyDate)=@Month))
                           and (ISNULL(@ProcessFactoryCode,'')='' or sl.ProcessFactoryCode=@ProcessFactoryCode) 
                           and (ISNULL(@ProjectId,'')='' or sl.ProjectId=@ProjectId)
                           group by sld.RawMaterialNum,sl.WorkAreaCode,sl.ProjectId,sl.ProcessFactoryCode) TbGh on Tb.WorkAreaCode=TbGh.WorkAreaCode and Tb.MaterialCode=TbGh.RawMaterialNum
                           LEFT JOIN --历史月份加工完成量
                           (
                           	    SELECT 
								tc.ParentCompanyCode AS WorkAreaCode,
								twod.MaterialCode,
								SUM(twod.WeightSmallPlan) AS HistoryMonthCount
								FROM
									TbWorkOrderDetail twod
									INNER JOIN TbWorkOrder two ON two.OrderCode = twod.OrderCode
									LEFT JOIN TbCompany tc ON two.SiteCode=tc.CompanyCode
								WHERE 
								twod.DaetailWorkStrat='加工完成' 
								AND (ISNULL(@ProjectId,'')='' or two.ProjectId=@ProjectId)
								AND (ISNULL(@ProcessFactoryCode,'')='' or two.ProcessFactoryCode=@ProcessFactoryCode)
								AND (ISNULL(@Yearf,'')='' or (YEAR(twod.FinishTime)=@Yearf and MONTH(twod.FinishTime)=@Month))
								GROUP BY tc.ParentCompanyCode,twod.MaterialCode
							)ProcessFinishData ON Tb.WorkAreaCode=ProcessFinishData.WorkAreaCode and Tb.MaterialCode=ProcessFinishData.MaterialCode 
                           ) Tb1  ";
            //string year = string.Empty;
            string year = DateTime.Now.Year.ToString();
            string yearf = DateTime.Now.Year.ToString();
            string month = DateTime.Now.Month.ToString();
            if (request.HistoryMonth.HasValue)
            {
                year = request.HistoryMonth.Value.Year.ToString();
                month = request.HistoryMonth.Value.Month.ToString();
                yearf = year;
            }
            //参数化
            List<Parameter> para = new List<Parameter>();
            para.Add(new Parameter("@ProjectId", request.ProjectId, DbType.String, null));
            para.Add(new Parameter("@ProcessFactoryCode", request.ProcessFactoryCode, DbType.String, null));
            para.Add(new Parameter("@Year", year, DbType.String, null));
            para.Add(new Parameter("@Yearf", yearf, DbType.String, null));
            para.Add(new Parameter("@Month", month, DbType.String, null));
            try
            {
                var model = Repository<TbRawMaterialMonthDemandPlanDetail>.FromSqlToPageTable(sql + where, para, request.rows, request.page, "MaterialCode", "asc");
                DataTable dt = GetExportList(request);
                decimal hj = Convert.ToDecimal(dt.Compute("sum(HistoryMonthCount)", "true"));
                //model.userdata = new { HistoryMonthCount1 = GetHistoryMonthCount(where, para) };
                model.userdata = new { HistoryMonthCount1 = hj };
                return model;
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        /// <summary>
        /// 月度需求计划明细列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageModel GetRawPlanGridJson(StatisticsReportFormRequest request)
        {
            //组装查询语句
            #region 模糊搜索条件

            var where = "";

            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where += (" and ProcessFactoryCode='" + request.ProcessFactoryCode + "' ");
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                where += "and WorkAreaCode='" + request.SiteCode + "' ";
            }
            if (!string.IsNullOrEmpty(request.ProjectId))
            {
                where += "and ProjectId='" + request.ProjectId + "' ";
            }
            if (!string.IsNullOrWhiteSpace(request.MaterialCode))
            {
                where += "and MaterialCode='" + request.MaterialCode + "' ";
            }
            if (!string.IsNullOrWhiteSpace(request.BegTime))
            {
                where += "and DeliveryDate>='" + request.BegTime + "' ";
            }
            if (!string.IsNullOrWhiteSpace(request.EndTime))
            {
                where += "and DeliveryDate<='" + request.EndTime + "' ";
            }
            #endregion

            try
            {
                string sql = @"select * from (select rmp.DemandPlanCode,rmp.Examinestatus,rmp.ProjectId,rmp.WorkAreaCode,rmp.ProcessFactoryCode,cp.CompanyFullName as ProcessFactoryName,sdd.DictionaryText as RebarTypeNew,DeliveryDate,rmpd.MaterialCode,rmpd.MaterialName,rmpd.DemandNum from TbRawMaterialMonthDemandPlan rmp
                               left join TbRawMaterialMonthDemandPlanDetail rmpd on rmp.DemandPlanCode=rmpd.DemandPlanCode
                               left join TbCompany cp on rmp.ProcessFactoryCode=cp.CompanyCode 
                               left join TbSysDictionaryData sdd on rmp.RebarType=sdd.DictionaryCode 
                               union all
                               select rmsp.SupplyPlanCode,rmsp.Examinestatus,rmsp.ProjectId,rmsp.WorkAreaCode,rmsp.ProcessFactoryCode,cp.CompanyFullName as ProcessFactoryName,sdd.DictionaryText as RebarTypeNew,DeliveryDate,rmspd.MaterialCode,rmspd.MaterialName,rmspd.SupplyNum from TbRawMaterialMonthDemandSupplyPlan rmsp
                               left join TbRawMaterialMonthDemandSupplyPlanDetail rmspd on rmsp.SupplyPlanCode=rmspd.SupplyPlanCode
                               left join TbRawMaterialMonthDemandPlan rmp on rmsp.DemandPlanCode=rmp.DemandPlanCode
                               left join TbCompany cp on rmsp.ProcessFactoryCode=cp.CompanyCode
                               left join TbSysDictionaryData sdd on rmp.RebarType=sdd.DictionaryCode) Tb
                               where 1=1 and Tb.Examinestatus='审核完成' ";
                //参数化
                List<Dos.ORM.Parameter> para = new List<Dos.ORM.Parameter>();
                var model = Repository<TbRawMaterialMonthDemandPlanDetail>.FromSqlToPageTable(sql + where, para, request.rows, request.page, "MaterialCode", "asc");
                return model;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 批次计划明细列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageModel GetBatchPlanGridJson(StatisticsReportFormRequest request)
        {
            //组装查询语句
            #region 模糊搜索条件

            var where = "";

            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where += (" and ProcessFactoryCode='" + request.ProcessFactoryCode + "' ");
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                where += "and WorkAreaCode='" + request.SiteCode + "' ";
            }
            if (!string.IsNullOrEmpty(request.ProjectId))
            {
                where += "and ProjectId='" + request.ProjectId + "' ";
            }
            if (!string.IsNullOrWhiteSpace(request.MaterialCode))
            {
                where += "and RawMaterialNum='" + request.MaterialCode + "' ";
            }
            if (!string.IsNullOrWhiteSpace(request.BegTime))
            {
                where += "and ArrivalDate>='" + request.BegTime + "' ";
            }
            if (!string.IsNullOrWhiteSpace(request.EndTime))
            {
                where += "and ArrivalDate<='" + request.EndTime + "' ";
            }
            #endregion

            try
            {
                string sql = @"select * from (select fbp.BatchPlanNum,fbp.Examinestatus,fbp.RawMaterialDemandNum,sdd.DictionaryText as RebarTypeNew,fbp.ProjectId,fbp.ProcessFactoryCode,cp.CompanyFullName as ProcessFactoryName,fbp.WorkAreaCode,fbp.ArrivalDate,fbpi.RawMaterialNum,fbpi.MaterialName,fbpi.BatchPlanQuantity,sup.SupplyDate,supd.HasSupplier from TbFactoryBatchNeedPlan fbp
                               left join TbFactoryBatchNeedPlanItem fbpi  on fbp.BatchPlanNum=fbpi.BatchPlanNum
                               left join TbCompany cp on fbp.ProcessFactoryCode=cp.CompanyCode
                               left join TbSysDictionaryData sdd on fbp.SteelsTypeCode=sdd.DictionaryCode
                               left join TbSupplyList sup on sup.BatchPlanNum=fbp.BatchPlanNum
                               left join TbSupplyListDetail supd on fbpi.BatchPlanNum=supd.BatchPlanNum and fbpi.RawMaterialNum=supd.RawMaterialNum) Tb
                               where 1=1 and Examinestatus='审核完成'  ";
                //参数化
                List<Dos.ORM.Parameter> para = new List<Dos.ORM.Parameter>();
                var model = Repository<TbFactoryBatchNeedPlanItem>.FromSqlToPageTable(sql + where, para, request.rows, request.page, "RawMaterialNum", "asc");
                return model;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 获取月加工完成量
        /// </summary>
        /// <returns></returns>
        public decimal GetHistoryMonthCount(string where, List<Parameter> para)
        {
            string sql = @"select ISNULL(sum(Tb1.HistoryMonthCount),0) from (SELECT 
								tc.ParentCompanyCode AS WorkAreaCode,
								twod.MaterialCode,
								twod.MaterialName,
								twod.SpecificationModel,
								SUM(twod.WeightSmallPlan) AS HistoryMonthCount
								FROM
									TbWorkOrderDetail twod
									INNER JOIN TbWorkOrder two ON two.OrderCode = twod.OrderCode
									LEFT JOIN TbCompany tc ON two.SiteCode=tc.CompanyCode
								WHERE 
								twod.DaetailWorkStrat='加工完成' 
								AND (ISNULL(@ProjectId,'')='' or two.ProjectId=@ProjectId)
								AND (ISNULL(@ProcessFactoryCode,'')='' or two.ProcessFactoryCode=@ProcessFactoryCode)
								AND (ISNULL(@Yearf,'')='' or (YEAR(twod.FinishTime)=@Yearf and DAY(twod.FinishTime)=@Month))
								GROUP BY tc.ParentCompanyCode,twod.MaterialCode,twod.MaterialName,twod.SpecificationModel)Tb1 ";

            var ret = Repositorys<decimal>.FromSql(sql + where, para);
            return ret[0];
        }

        #endregion

        #region 加工订单报表
        /// <summary>
        /// 各加工厂订单重量分类分析
        /// </summary>
        /// <returns></returns>
        public Tuple<DataTable> WorkOrderWeightTypeFx(string ProjectId)
        {
            if (string.IsNullOrWhiteSpace(ProjectId))
            {
                ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
            }

            #region 模糊搜索条件

            string where = "";

            #endregion

            var ret1 = @"select wo.TypeName,wo.ProcessFactoryCode,com.CompanyFullName as ProcessFactoryName,CONVERT(decimal(18,2),SUM(wod.WeightSmallPlan)) as WeightSmallPlan from TbWorkOrder wo
left join TbWorkOrderDetail wod on wo.OrderCode=wod.OrderCode 
left join TbCompany com on wo.ProcessFactoryCode=com.CompanyCode  where 1=1 and wo.Examinestatus='审核完成' and wo.ProjectId=@ProjectId group by wo.TypeName,wo.ProcessFactoryCode,com.CompanyFullName ";
            DataTable dt1 = Db.Context.FromSql(ret1 + where).AddInParameter("@ProjectId", DbType.String, ProjectId).ToDataTable();
            DataTable dtWork = new DataTable();
            DataColumn dc1 = new DataColumn("TypeName", Type.GetType("System.String"));
            DataColumn dc2 = new DataColumn("No1", Type.GetType("System.Decimal"));
            DataColumn dc3 = new DataColumn("No2", Type.GetType("System.Decimal"));
            DataColumn dc4 = new DataColumn("No3", Type.GetType("System.Decimal"));
            dtWork.Columns.Add(dc1);
            dtWork.Columns.Add(dc2);
            dtWork.Columns.Add(dc3);
            dtWork.Columns.Add(dc4);
            for (int j = 0; j < dt1.Rows.Count; j++)
            {
                DataRow dr = dtWork.NewRow();
                dr["TypeName"] = dt1.Rows[j]["TypeName"];
                if (dt1.Rows[j]["ProcessFactoryCode"].ToString() == "6386683214299275264")
                {
                    dr["No1"] = dt1.Rows[j]["WeightSmallPlan"];
                    dr["No2"] = 0;
                    dr["No3"] = 0;

                }
                else if (dt1.Rows[j]["ProcessFactoryCode"].ToString() == "6386683729561128960")
                {
                    dr["No1"] = 0;
                    dr["No2"] = dt1.Rows[j]["WeightSmallPlan"];
                    dr["No3"] = 0;
                }
                else if (dt1.Rows[j]["ProcessFactoryCode"].ToString() == "6386683947165814784")
                {
                    dr["No1"] = 0;
                    dr["No2"] = 0;
                    dr["No3"] = dt1.Rows[j]["WeightSmallPlan"];
                }
                dtWork.Rows.Add(dr);

            }


            return new Tuple<DataTable>(dtWork);
        }

        /// <summary>
        /// 各个加工厂当月订单重量分类分析
        /// </summary>
        /// <returns></returns>
        public Tuple<DataTable> WorkOrderMonthWeightTypeFx(string ProjectId)
        {
            if (string.IsNullOrWhiteSpace(ProjectId))
            {
                ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
            }

            #region 模糊搜索条件

            string where = "";

            #endregion

            var ret1 = @"select wo.TypeName,wo.ProcessFactoryCode,com.CompanyFullName as ProcessFactoryName,CONVERT(decimal(18,2),SUM(wod.WeightSmallPlan)) as WeightSmallPlan from TbWorkOrder wo
left join TbWorkOrderDetail wod on wo.OrderCode=wod.OrderCode 
left join TbCompany com on wo.ProcessFactoryCode=com.CompanyCode  where 1=1 and wo.Examinestatus='审核完成' and wo.ProjectId=@ProjectId
and wo.InsertTime>=(SELECT DATEADD(mm, DATEDIFF(mm, 0, GETDATE()), 0)) and wo.InsertTime<=(SELECT DATEADD(d, - 1, DATEADD(mm, DATEDIFF(mm, 0, GETDATE())+1, 0)))
group by wo.TypeName,wo.ProcessFactoryCode,com.CompanyFullName ";
            DataTable dt1 = Db.Context.FromSql(ret1 + where).AddInParameter("@ProjectId", DbType.String, ProjectId).ToDataTable();
            DataTable dtWork = new DataTable();
            DataColumn dc1 = new DataColumn("TypeName", Type.GetType("System.String"));
            DataColumn dc2 = new DataColumn("No1", Type.GetType("System.Decimal"));
            DataColumn dc3 = new DataColumn("No2", Type.GetType("System.Decimal"));
            DataColumn dc4 = new DataColumn("No3", Type.GetType("System.Decimal"));
            dtWork.Columns.Add(dc1);
            dtWork.Columns.Add(dc2);
            dtWork.Columns.Add(dc3);
            dtWork.Columns.Add(dc4);
            for (int j = 0; j < dt1.Rows.Count; j++)
            {
                DataRow dr = dtWork.NewRow();
                dr["TypeName"] = dt1.Rows[j]["TypeName"];
                if (dt1.Rows[j]["ProcessFactoryCode"].ToString() == "6386683214299275264")
                {
                    dr["No1"] = dt1.Rows[j]["WeightSmallPlan"];
                    dr["No2"] = 0;
                    dr["No3"] = 0;

                }
                else if (dt1.Rows[j]["ProcessFactoryCode"].ToString() == "6386683729561128960")
                {
                    dr["No1"] = 0;
                    dr["No2"] = dt1.Rows[j]["WeightSmallPlan"];
                    dr["No3"] = 0;
                }
                else if (dt1.Rows[j]["ProcessFactoryCode"].ToString() == "6386683947165814784")
                {
                    dr["No1"] = 0;
                    dr["No2"] = 0;
                    dr["No3"] = dt1.Rows[j]["WeightSmallPlan"];
                }
                dtWork.Rows.Add(dr);

            }

            return new Tuple<DataTable>(dtWork);
        }

        /// <summary>
        /// 当月各个加工厂订单重量分析
        /// </summary>
        /// <returns></returns>
        public Tuple<DataTable> WorkOrderMonthWeightFx(string ProjectId)
        {
            if (string.IsNullOrWhiteSpace(ProjectId))
            {
                ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
            }

            #region 模糊搜索条件

            string where = "";

            #endregion

            var ret1 = @"select Tb.CompanyCode,Tb.CompanyFullName,isnull(Tb1.WeightSmallPlan,0) as WeightSmallPlan from(select CompanyCode,CompanyFullName from TbCompany where OrgType=1) Tb
left join (select wo.ProcessFactoryCode,com.CompanyFullName as ProcessFactoryName,CONVERT(decimal(18,2),SUM(wod.WeightSmallPlan)) as WeightSmallPlan from TbWorkOrder wo
left join TbWorkOrderDetail wod on wo.OrderCode=wod.OrderCode 
left join TbCompany com on wo.ProcessFactoryCode=com.CompanyCode  where 1=1 and wo.Examinestatus='审核完成' and wo.ProjectId=@ProjectId
and wo.InsertTime>=(SELECT DATEADD(mm, DATEDIFF(mm, 0, GETDATE()), 0)) and wo.InsertTime<=(SELECT DATEADD(d, - 1, DATEADD(mm, DATEDIFF(mm, 0, GETDATE())+1, 0)))
group by wo.ProcessFactoryCode,com.CompanyFullName) Tb1 on Tb.CompanyCode=Tb1.ProcessFactoryCode";
            DataTable dt1 = Db.Context.FromSql(ret1 + where).AddInParameter("@ProjectId", DbType.String, ProjectId).ToDataTable();
            return new Tuple<DataTable>(dt1);
        }
        /// <summary>
        /// 当月各个加工厂订单重量分析
        /// </summary>
        /// <returns></returns>
        public Tuple<DataTable> WorkOrderAllMonthWeightFx(string ProjectId)
        {
            if (string.IsNullOrWhiteSpace(ProjectId))
            {
                ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
            }

            #region 模糊搜索条件

            string where = "";

            #endregion

            var ret1 = @"select * from (select com.CompanyCode,com.CompanyFullName,Tb.AllMonth,isnull(CONVERT(decimal(18,2),SUM(wod.WeightSmallPlan)),0) as WeightSmallPlan from (select CompanyCode,CompanyFullName,'1' as PCode from TbCompany where OrgType=1) com 
left join(select '1' as PCode,RIGHT(convert(varchar(7),dateadd(mm,-t.number,getdate()),120),2) as AllMonth
from(select number from master..spt_values where type='P') t
where year(dateadd(mm,-t.number,getdate()))=year(getdate())) Tb on Tb.PCode=com.PCode 
left join TbWorkOrder wo on wo.ProcessFactoryCode=com.CompanyCode and MONTH(wo.InsertTime)=Tb.AllMonth
left join TbWorkOrderDetail wod on wo.OrderCode=wod.OrderCode and wo.Examinestatus='审核完成'and wo.ProjectId=@ProjectId
group by com.CompanyCode,com.CompanyFullName,Tb.AllMonth) Tb
pivot (SUM(Tb.WeightSmallPlan) for AllMonth IN([01],[02],[03],[04],[05],[06],[07],[08],[09],[10],[11],[12])) pvt ";
            DataTable dt1 = Db.Context.FromSql(ret1 + where).AddInParameter("@ProjectId", DbType.String, ProjectId).ToDataTable();
            return new Tuple<DataTable>(dt1);
        }

        /// <summary>
        /// 加工厂历史订单量分析
        /// </summary>
        /// <returns></returns>
        public List<YearList> GetWorkYear()
        {
            var ret1 = @"select YEAR(InsertTime) Year,CONVERT(varchar,YEAR(InsertTime))+'年' YearName from TbWorkOrder  group by YEAR(InsertTime) order by YEAR(InsertTime) desc";
            return Db.Context.FromSql(ret1).ToList<YearList>();
        }
        /// <summary>
        /// 加工厂历史订单量分析
        /// </summary>
        /// <returns></returns>
        public Tuple<DataTable> WorkOrderHistoryFx(string OrgType, string Year, string ProjectId)
        {
            if (string.IsNullOrWhiteSpace(OrgType))
            {
                OrgType = OperatorProvider.Provider.CurrentUser.OrgType;
            }
            if (OrgType == "1") //加工厂
            {
                ProjectId = "";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(ProjectId))
                {
                    ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
                }
            }

            var ret1 = @"select TbYearComp.Year,TbYearComp.CompanyCode,TbYearComp.CompanyFullName,isnull(TbWorkYear.一月,0) as 一月,isnull(TbWorkYear.二月,0) as 二月,isnull(TbWorkYear.三月,0) as 三月,isnull(TbWorkYear.四月,0) as 四月,isnull(TbWorkYear.五月,0) as 五月,isnull(TbWorkYear.六月,0) as 六月,isnull(TbWorkYear.七月,0) as 七月,isnull(TbWorkYear.八月,0) as 八月,isnull(TbWorkYear.九月,0) as 九月,isnull(TbWorkYear.十月,0) as 十月,isnull(TbWorkYear.十一月,0) as 十一月,isnull(TbWorkYear.十二月,0) as 十二月,TbYearComp.ShowOrder from (select TbYear.Year,TbComp.CompanyCode,TbComp.CompanyFullName,TbComp.ShowOrder from (select YEAR(InsertTime) Year,1 type from TbWorkOrder group by YEAR(InsertTime)) TbYear
                         left join (select CompanyCode,CompanyFullName,1 type,1 as ShowOrder from TbCompany cp where cp.OrgType=1 and cp.CompanyCode='6386683214299275264'
union all
select CompanyCode,CompanyFullName,1 type,2 as ShowOrder from TbCompany cp where cp.OrgType=1 and cp.CompanyCode='6386683729561128960'
union all
select CompanyCode,CompanyFullName,1 type,3 as ShowOrder from TbCompany cp where cp.OrgType=1 and cp.CompanyCode='6386683947165814784') TbComp on TbYear.type=TbComp.type) TbYearComp
                         left join (SELECT YEAR(InsertTime) Year,'6386683214299275264' as ProcessFactoryCode,
                         isnull(SUM(CASE WHEN MONTH(InsertTime) =1 THEN WeightSmallPlan ELSE 0 END),0) 一月, 
                         isnull(SUM(CASE WHEN MONTH(InsertTime) =2 THEN WeightSmallPlan ELSE 0 END),0) 二月,
                         isnull(SUM(CASE WHEN MONTH(InsertTime) =3 THEN WeightSmallPlan ELSE 0 END),0) 三月,
                         isnull(SUM(CASE WHEN MONTH(InsertTime) =4 THEN WeightSmallPlan ELSE 0 END),0) 四月,
                         isnull(SUM(CASE WHEN MONTH(InsertTime) =5 THEN WeightSmallPlan ELSE 0 END),0) 五月,
                         isnull(SUM(CASE WHEN MONTH(InsertTime) =6 THEN WeightSmallPlan ELSE 0 END),0) 六月,
                         isnull(SUM(CASE WHEN MONTH(InsertTime) =7 THEN WeightSmallPlan ELSE 0 END),0) 七月,
                         isnull(SUM(CASE WHEN MONTH(InsertTime) =8 THEN WeightSmallPlan ELSE 0 END),0) 八月,
                         isnull(SUM(CASE WHEN MONTH(InsertTime) =9 THEN WeightSmallPlan ELSE 0 END),0) 九月,
                         isnull(SUM(CASE WHEN MONTH(InsertTime) =10 THEN WeightSmallPlan ELSE 0 END),0) 十月,
                         isnull(SUM(CASE WHEN MONTH(InsertTime) =11 THEN WeightSmallPlan ELSE 0 END),0) 十一月,
                         isnull(SUM(CASE WHEN MONTH(InsertTime) =12 THEN WeightSmallPlan ELSE 0 END),0) 十二月
                         FROM TbWorkOrder 
                         left join TbWorkOrderDetail on TbWorkOrder.OrderCode=TbWorkOrderDetail.OrderCode
                         where ProcessFactoryCode='6386683214299275264' 
                         and (ISNULL(@ProjectId,'')='' or ProjectId=@ProjectId) 
                         and Examinestatus='审核完成' 
                         and TbWorkOrderDetail.RevokeStart='正常' 
                         GROUP BY YEAR(InsertTime)
                         union all
                         SELECT YEAR(InsertTime) Year,'6386683729561128960' as ProcessFactoryCode,
                         isnull(SUM(CASE WHEN MONTH(InsertTime) =1 THEN WeightSmallPlan ELSE 0 END),0) 一月, 
                         isnull(SUM(CASE WHEN MONTH(InsertTime) =2 THEN WeightSmallPlan ELSE 0 END),0) 二月,
                         isnull(SUM(CASE WHEN MONTH(InsertTime) =3 THEN WeightSmallPlan ELSE 0 END),0) 三月,
                         isnull(SUM(CASE WHEN MONTH(InsertTime) =4 THEN WeightSmallPlan ELSE 0 END),0) 四月,
                         isnull(SUM(CASE WHEN MONTH(InsertTime) =5 THEN WeightSmallPlan ELSE 0 END),0) 五月,
                         isnull(SUM(CASE WHEN MONTH(InsertTime) =6 THEN WeightSmallPlan ELSE 0 END),0) 六月,
                         isnull(SUM(CASE WHEN MONTH(InsertTime) =7 THEN WeightSmallPlan ELSE 0 END),0) 七月,
                         isnull(SUM(CASE WHEN MONTH(InsertTime) =8 THEN WeightSmallPlan ELSE 0 END),0) 八月,
                         isnull(SUM(CASE WHEN MONTH(InsertTime) =9 THEN WeightSmallPlan ELSE 0 END),0) 九月,
                         isnull(SUM(CASE WHEN MONTH(InsertTime) =10 THEN WeightSmallPlan ELSE 0 END),0) 十月,
                         isnull(SUM(CASE WHEN MONTH(InsertTime) =11 THEN WeightSmallPlan ELSE 0 END),0) 十一月,
                         isnull(SUM(CASE WHEN MONTH(InsertTime) =12 THEN WeightSmallPlan ELSE 0 END),0) 十二月 
                         FROM TbWorkOrder 
                         left join TbWorkOrderDetail on TbWorkOrder.OrderCode=TbWorkOrderDetail.OrderCode
                         where ProcessFactoryCode='6386683729561128960' 
                         and (ISNULL(@ProjectId,'')='' or ProjectId=@ProjectId) 
                         and Examinestatus='审核完成' 
                         and TbWorkOrderDetail.RevokeStart='正常' 
                         GROUP BY YEAR(InsertTime)
                         union all
                         SELECT YEAR(InsertTime) Year,'6386683947165814784' as ProcessFactoryCode,
                         isnull(SUM(CASE WHEN MONTH(InsertTime) =1 THEN WeightSmallPlan ELSE 0 END),0) 一月, 
                         isnull(SUM(CASE WHEN MONTH(InsertTime) =2 THEN WeightSmallPlan ELSE 0 END),0) 二月,
                         isnull(SUM(CASE WHEN MONTH(InsertTime) =3 THEN WeightSmallPlan ELSE 0 END),0) 三月,
                         isnull(SUM(CASE WHEN MONTH(InsertTime) =4 THEN WeightSmallPlan ELSE 0 END),0) 四月,
                         isnull(SUM(CASE WHEN MONTH(InsertTime) =5 THEN WeightSmallPlan ELSE 0 END),0) 五月,
                         isnull(SUM(CASE WHEN MONTH(InsertTime) =6 THEN WeightSmallPlan ELSE 0 END),0) 六月,
                         isnull(SUM(CASE WHEN MONTH(InsertTime) =7 THEN WeightSmallPlan ELSE 0 END),0) 七月,
                         isnull(SUM(CASE WHEN MONTH(InsertTime) =8 THEN WeightSmallPlan ELSE 0 END),0) 八月,
                         isnull(SUM(CASE WHEN MONTH(InsertTime) =9 THEN WeightSmallPlan ELSE 0 END),0) 九月,
                         isnull(SUM(CASE WHEN MONTH(InsertTime) =10 THEN WeightSmallPlan ELSE 0 END),0) 十月,
                         isnull(SUM(CASE WHEN MONTH(InsertTime) =11 THEN WeightSmallPlan ELSE 0 END),0) 十一月,
                         isnull(SUM(CASE WHEN MONTH(InsertTime) =12 THEN WeightSmallPlan ELSE 0 END),0) 十二月
                         FROM TbWorkOrder 
                         left join TbWorkOrderDetail on TbWorkOrder.OrderCode=TbWorkOrderDetail.OrderCode
                         where ProcessFactoryCode='6386683947165814784' 
                         and (ISNULL(@ProjectId,'')='' or ProjectId=@ProjectId) 
                         and Examinestatus='审核完成' 
                         and TbWorkOrderDetail.RevokeStart='正常' 
                         GROUP BY YEAR(InsertTime)) TbWorkYear on TbYearComp.Year=TbWorkYear.Year and TbYearComp.CompanyCode=TbWorkYear.ProcessFactoryCode
                         where TbYearComp.Year=@Year order by TbYearComp.ShowOrder asc ";
            DataTable dt1 = Db.Context.FromSql(ret1).AddInParameter("@Year", DbType.String, Year).AddInParameter("@ProjectId", DbType.String, ProjectId).ToDataTable();
            return new Tuple<DataTable>(dt1);
        }

        /// <summary>
        /// 加工厂历史订单量分析
        /// </summary>
        /// <returns></returns>
        public Tuple<DataTable> SameMonthWorkOrderTypeTj(string OrgType, string Year, string Month, string ProjectId)
        {
            if (string.IsNullOrWhiteSpace(OrgType))
            {
                OrgType = OperatorProvider.Provider.CurrentUser.OrgType;
            }
            if (OrgType == "1") //加工厂
            {
                ProjectId = "";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(ProjectId))
                {
                    ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
                }
            }

            //var ret1 = @"select TbJgc.Year,TbJgc.CompanyCode,TbJgc.CompanyFullName,isnull(TbType.WeightTotal,0) as WeightTotal,isnull(TbType.ZcOrder,0) as ZcOrder,isnull(TbType.JjOrder,0) as JjOrder from (select TbYear.Year,TbComp.CompanyCode,TbComp.CompanyFullName from (select YEAR(InsertTime) Year,1 type from TbWorkOrder  group by YEAR(InsertTime)) TbYear
            //             left join (select CompanyCode,CompanyFullName,1 type from TbCompany cp where cp.OrgType=1) TbComp on TbYear.type=TbComp.type) TbJgc
            //             left join (
            //             select YEAR(InsertTime) Year,MONTH(InsertTime) Month,ProcessFactoryCode,sum(WeightSmallPlan) as WeightTotal,
            //             isnull(SUM(CASE WHEN OrderStart ='正常订单' THEN WeightSmallPlan ELSE 0 END),0) ZcOrder, 
            //             isnull(SUM(CASE WHEN OrderStart ='加急订单' THEN WeightSmallPlan ELSE 0 END),0) JjOrder from TbWorkOrder
            //             left join TbWorkOrderDetail on TbWorkOrder.OrderCode=TbWorkOrderDetail.OrderCode
            //             where (ISNULL(@ProjectId,'')='' or ProjectId=@ProjectId) 
            //             and Examinestatus='审核完成' 
            //             and ProcessFactoryCode='6386683214299275264' 
            //             and TbWorkOrderDetail.RevokeStart='正常'
            //             group by YEAR(InsertTime),MONTH(InsertTime),ProcessFactoryCode
            //             union all
            //             select YEAR(InsertTime) Year,MONTH(InsertTime) Month,ProcessFactoryCode,sum(WeightSmallPlan) as WeightTotal,
            //             isnull(SUM(CASE WHEN OrderStart ='正常订单' THEN WeightSmallPlan ELSE 0 END),0) ZcOrder, 
            //             isnull(SUM(CASE WHEN OrderStart ='加急订单' THEN WeightSmallPlan ELSE 0 END),0) JjOrder from TbWorkOrder
            //             left join TbWorkOrderDetail on TbWorkOrder.OrderCode=TbWorkOrderDetail.OrderCode
            //             where (ISNULL(@ProjectId,'')='' or ProjectId=@ProjectId) 
            //             and Examinestatus='审核完成' 
            //             and ProcessFactoryCode='6386683729561128960' 
            //             and TbWorkOrderDetail.RevokeStart='正常'
            //             and  MONTH(InsertTime)=@Month
            //             group by YEAR(InsertTime),MONTH(InsertTime),ProcessFactoryCode
            //             union all
            //             select YEAR(InsertTime) Year,MONTH(InsertTime) Month,ProcessFactoryCode,sum(WeightSmallPlan) as WeightTotal,
            //             isnull(SUM(CASE WHEN OrderStart ='正常订单' THEN WeightSmallPlan ELSE 0 END),0) ZcOrder, 
            //             isnull(SUM(CASE WHEN OrderStart ='加急订单' THEN WeightSmallPlan ELSE 0 END),0) JjOrder from TbWorkOrder
            //             left join TbWorkOrderDetail on TbWorkOrder.OrderCode=TbWorkOrderDetail.OrderCode
            //             where (ISNULL(@ProjectId,'')='' or ProjectId=@ProjectId)
            //              and Examinestatus='审核完成' 
            //              and ProcessFactoryCode='6386683947165814784' 
            //             and TbWorkOrderDetail.RevokeStart='正常'
            //              and  MONTH(InsertTime)=@Month
            //             group by YEAR(InsertTime),MONTH(InsertTime),ProcessFactoryCode
            //             ) TbType on TbJgc.CompanyCode=TbType.ProcessFactoryCode and TbJgc.Year=TbType.Year where TbJgc.Year=@Year ";
            var ret1 = @"select TbCp.CompanyCode,TbCp.CompanyFullName,isnull(TbWo.WeightTotal,0) as WeightTotal,isnull(TbWo.ZcOrder,0) as ZcOrder,isnull(TbWo.JjOrder,0) as JjOrder from TbCompany TbCp
left join(select sum(wod.WeightSmallPlan) as WeightTotal,
isnull(SUM(CASE WHEN OrderStart = '正常订单' THEN wod.WeightSmallPlan ELSE 0 END), 0) ZcOrder,
isnull(SUM(CASE WHEN OrderStart = '加急订单' THEN wod.WeightSmallPlan ELSE 0 END), 0) JjOrder,wo.ProcessFactoryCode from TbWorkOrder wo
left join TbWorkOrderDetail wod on wo.OrderCode = wod.OrderCode
where(ISNULL(@ProjectId, '') = '' or wo.ProjectId = @ProjectId) and wo.Examinestatus = '审核完成'
and wod.RevokeStart = '正常' and YEAR(wo.InsertTime)= @Year and MONTH(wo.InsertTime)= @Month
group by wo.ProcessFactoryCode) TbWo on TbCp.CompanyCode = TbWo.ProcessFactoryCode where TbCp.OrgType = 1";
            DataTable dt1 = Db.Context.FromSql(ret1).AddInParameter("@Month", DbType.String, Month).AddInParameter("@Year", DbType.String, Year).AddInParameter("@ProjectId", DbType.String, ProjectId).ToDataTable();
            return new Tuple<DataTable>(dt1);
        }

        /// <summary>
        /// 订单类型分布情况
        /// </summary>
        /// <returns></returns>
        public Tuple<DataTable> UrgentWorkOrderFbQk(string OrgType, string Year, string Month, string ProjectId, string JgcCode, string TypeName)
        {
            if (string.IsNullOrWhiteSpace(OrgType))
            {
                OrgType = OperatorProvider.Provider.CurrentUser.OrgType;
            }
            if (OrgType == "1") //加工厂
            {
                ProjectId = "";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(ProjectId))
                {
                    ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
                }
            }

            var ret1 = @"select YEAR(InsertTime) Year,MONTH(InsertTime) Month,ProcessFactoryCode,SiteCode,cp.CompanyFullName as SiteName,COUNT(1) SiteNum from TbWorkOrder 
                         left join TbCompany cp on cp.CompanyCode=SiteCode
                         where Examinestatus='审核完成' 
                         and (ISNULL(@ProjectId,'')='' or ProjectId=@ProjectId)
                         and YEAR(InsertTime)=@Year and MONTH(InsertTime)=@Month
                         and ((@JgcCode='0' and ProcessFactoryCode='6386683214299275264') or (@JgcCode='1' and ProcessFactoryCode='6386683729561128960') or (@JgcCode='2' and ProcessFactoryCode='6386683947165814784')) 
                         and ((@TypeName='订单总量' and 1=1) or (@TypeName='普通订单量' and OrderStart='正常订单') or (@TypeName='加急订单量' and OrderStart='加急订单'))
                         group by YEAR(InsertTime),MONTH(InsertTime),ProcessFactoryCode,SiteCode,cp.CompanyFullName ";
            DataTable dt1 = Db.Context.FromSql(ret1)
                .AddInParameter("@Month", DbType.String, Month)
                .AddInParameter("@Year", DbType.String, Year)
                .AddInParameter("@ProjectId", DbType.String, ProjectId)
                .AddInParameter("@JgcCode", DbType.String, JgcCode)
                .AddInParameter("@TypeName", DbType.String, TypeName).ToDataTable();
            return new Tuple<DataTable>(dt1);
        }


        /// <summary>
        /// 订单类型分布情况
        /// </summary>
        /// <returns></returns>
        public Tuple<DataTable> MaterialsUsedTypeTj(string OrgType, string Year, string Month, string ProjectId)
        {
            if (string.IsNullOrWhiteSpace(OrgType))
            {
                OrgType = OperatorProvider.Provider.CurrentUser.OrgType;
            }
            if (OrgType == "1") //加工厂
            {
                ProjectId = "";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(ProjectId))
                {
                    ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
                }
            }

            var ret1 = @"select cp.CompanyCode,cp.CompanyFullName,ISNULL(TbYl.Total,0) as Total,isnull(tbyl.ycl,0) as ycl,isnull(TbYl.yl,0) as yl from TbCompany cp
                         left join (
                         select 
                          a.ProcessFactoryCode,
                          sum(ISNULL(a.Total,0))Total, --总领料量
                          sum(ISNULL(b.yl,0))yl,--余料
                          sum(ISNULL(c.ycl,0))ycl--原材料
                         from TbRMProductionMaterial a
                         left join (
                            select CollarCode,SUM(WeightSmallPlanN) yl from TbRMProductionMaterialPlan
                            where RMTypeName='余料'
                            group by CollarCode
                         ) b on a.CollarCode=b.CollarCode
                         left join (
                            select tb.CollarCode,isnull(SUM(ycl),0) as ycl from (select CollarCode,SUM(WeightSmallPlanN) ycl from TbRMProductionMaterialPlan
                            where RMTypeName='原材料'
                            group by CollarCode
                            union all
                            select CollarCode,SUM(WeightSmallPlanN) ycl from TbRMProductionMaterialDetail 
                            where WorkOrderItemId not in (select WorkOrderItemId from TbRMProductionMaterialPlan)
                            group by CollarCode) tb group by tb.CollarCode
                         ) c on a.CollarCode=c.CollarCode
                         where Examinestatus='审核完成'
                         and YEAR(a.CollarDate)=@Year and MONTH(CollarDate)=@Month
                         and (ISNULL(@ProjectId,'')='' or a.ProjectId=@ProjectId)
                         group by a.ProcessFactoryCode) TbYl on cp.CompanyCode=TbYl.ProcessFactoryCode
                         where cp.OrgType=1 ";
            DataTable dt1 = Db.Context.FromSql(ret1)
                .AddInParameter("@Month", DbType.String, Month)
                .AddInParameter("@Year", DbType.String, Year)
                .AddInParameter("@ProjectId", DbType.String, ProjectId).ToDataTable();
            return new Tuple<DataTable>(dt1);
        }
        #endregion

        #region

        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public DataTable GetExportList(StatisticsReportFormRequest request)
        {
            #region 模糊搜索条件

            string where = " where 1=1 ";
            if (!string.IsNullOrWhiteSpace(request.MaterialName))
            {
                where += " and Tb1.MaterialName like '%" + request.MaterialName + "%'";
            }
            if (request.SpecificationModel != null)
            {
                where += " and Tb1.SpecificationModel like '%" + request.SpecificationModel + "%'";
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 4);
                string workAreaStr = string.Join("','", WorkAreaList);
                where += " and Tb1.WorkAreaCode in('" + workAreaStr + "')";
            }
            #endregion

            string sql = @"select * from (
	select Tb.*,isnull(TbXqPlan.DemandNum,0) as LjDemandNum,isnull(TbPcPlan.BatchPlanQuantity,0) as LjBatchPlanQuantity,isnull(TbGh.HasSupplier,0) as LjHasSupplier,isnull(ProcessFinishData.HistoryMonthCount,0) as HistoryMonthCount from (select rs.MaterialCode,rs.MaterialName,rs.SpecificationModel,cp1.CompanyCode as BranchCode,cp1.CompanyFullName as BranchName,rs.WorkAreaCode,cp.CompanyFullName as WorkAreaName,sum(rs.Count) as InitialCount,rs.ProjectId from TbRawMaterialStockRecord rs
                           left join TbCompany cp on cp.CompanyCode=rs.WorkAreaCode
                           left join TbCompany cp1 on cp1.CompanyCode=cp.ParentCompanyCode
                           left join TbStorage s on rs.StorageCode=s.StorageCode
                           where (ISNULL(@ProjectId,'')='' or rs.ProjectId=@ProjectId)
                           and (ISNULL(@ProcessFactoryCode,'')='' or s.ProcessFactoryCode=@ProcessFactoryCode)
                           and rs.ChackState=1
                           group by rs.MaterialCode,rs.MaterialName,rs.SpecificationModel,cp1.CompanyCode,cp1.CompanyFullName,rs.WorkAreaCode,cp.CompanyFullName,rs.ProjectId) Tb
                           left join (select rmpd.MaterialCode,rmp.WorkAreaCode,rmp.ProjectId,rmp.ProcessFactoryCode,sum(rmpd.DemandNum) as DemandNum from TbRawMaterialMonthDemandPlan rmp
                           left join TbRawMaterialMonthDemandPlanDetail rmpd on rmp.DemandPlanCode=rmpd.DemandPlanCode 
                           where rmp.Examinestatus='审核完成' 
                           and rmp.DemandPlanCode not in(select DemandPlanCode from TbRawMaterialMonthDemandSupplyPlan where Examinestatus='审核完成') 
                           and (ISNULL(@Year,'')='' or (YEAR(rmp.DeliveryDate)=@Year and MONTH(rmp.DeliveryDate)=@Month))
                           and (ISNULL(@ProcessFactoryCode,'')='' or rmp.ProcessFactoryCode=@ProcessFactoryCode) 
                           and (ISNULL(@ProjectId,'')='' or rmp.ProjectId=@ProjectId) group by rmpd.MaterialCode,rmp.WorkAreaCode,rmp.ProjectId,rmp.ProcessFactoryCode
                           union all
                           select rmspd.MaterialCode,rmsp.WorkAreaCode,rmsp.ProjectId,rmsp.ProcessFactoryCode,sum((rmspd.SupplyNum+rmspd.DemandNum)) as DemandNum  from TbRawMaterialMonthDemandSupplyPlan rmsp
                           left join TbRawMaterialMonthDemandSupplyPlanDetail rmspd on rmsp.SupplyPlanCode=rmspd.SupplyPlanCode 
                           where rmsp.Examinestatus='审核完成'
                           and (ISNULL(@Year,'')='' or (YEAR(rmsp.SupplyTime)=@Year and MONTH(rmsp.SupplyTime)=@Month))
                           and (ISNULL(@ProcessFactoryCode,'')='' or rmsp.ProcessFactoryCode=@ProcessFactoryCode) 
                           and (ISNULL(@ProjectId,'')='' or rmsp.ProjectId=@ProjectId) group by rmspd.MaterialCode,rmsp.WorkAreaCode,rmsp.ProjectId,rmsp.ProcessFactoryCode) TbXqPlan 
                           on Tb.WorkAreaCode=TbXqPlan.WorkAreaCode and Tb.MaterialCode=TbXqPlan.MaterialCode
                           left join (select fbpi.RawMaterialNum,fbp.WorkAreaCode,fbp.ProjectId,fbp.ProcessFactoryCode,sum(fbpi.BatchPlanQuantity) as BatchPlanQuantity from TbFactoryBatchNeedPlan fbp
                           left join TbFactoryBatchNeedPlanItem fbpi on fbp.BatchPlanNum=fbpi.BatchPlanNum where fbp.Examinestatus='审核完成'
                           and (ISNULL(@Year,'')='' or (YEAR(fbp.ArrivalDate)=@Year and MONTH(fbp.ArrivalDate)=@Month))
                           and (ISNULL(@ProcessFactoryCode,'')='' or fbp.ProcessFactoryCode=@ProcessFactoryCode) 
                           and (ISNULL(@ProjectId,'')='' or fbp.ProjectId=@ProjectId) group by fbpi.RawMaterialNum,fbp.WorkAreaCode,fbp.ProjectId,fbp.ProcessFactoryCode) TbPcPlan 
                           on Tb.WorkAreaCode=TbPcPlan.WorkAreaCode and Tb.MaterialCode=TbPcPlan.RawMaterialNum
                           left join (select sld.RawMaterialNum,sl.WorkAreaCode,sl.ProjectId,sl.ProcessFactoryCode,sum(sld.HasSupplier) as HasSupplier from TbSupplyList sl
                           left join TbSupplyListDetail sld on sl.BatchPlanNum=sld.BatchPlanNum where 1=1
                           and (ISNULL(@Year,'')='' or (YEAR(sl.SupplyDate)=@Year and MONTH(sl.SupplyDate)=@Month))
                           and (ISNULL(@ProcessFactoryCode,'')='' or sl.ProcessFactoryCode=@ProcessFactoryCode) 
                           and (ISNULL(@ProjectId,'')='' or sl.ProjectId=@ProjectId)
                           group by sld.RawMaterialNum,sl.WorkAreaCode,sl.ProjectId,sl.ProcessFactoryCode) TbGh on Tb.WorkAreaCode=TbGh.WorkAreaCode and Tb.MaterialCode=TbGh.RawMaterialNum
                           LEFT JOIN --历史月份加工完成量
                           (
                           	    SELECT 
								tc.ParentCompanyCode AS WorkAreaCode,
								twod.MaterialCode,
								SUM(twod.WeightSmallPlan) AS HistoryMonthCount
								FROM
									TbWorkOrderDetail twod
									INNER JOIN TbWorkOrder two ON two.OrderCode = twod.OrderCode
									LEFT JOIN TbCompany tc ON two.SiteCode=tc.CompanyCode
								WHERE 
								twod.DaetailWorkStrat='加工完成' 
								AND (ISNULL(@ProjectId,'')='' or two.ProjectId=@ProjectId)
								AND (ISNULL(@ProcessFactoryCode,'')='' or two.ProcessFactoryCode=@ProcessFactoryCode)
								AND (ISNULL(@Yearf,'')='' or (YEAR(twod.FinishTime)=@Yearf and MONTH(twod.FinishTime)=@Month))
								GROUP BY tc.ParentCompanyCode,twod.MaterialCode
							)ProcessFinishData ON Tb.WorkAreaCode=ProcessFinishData.WorkAreaCode and Tb.MaterialCode=ProcessFinishData.MaterialCode 
                           ) Tb1  ";
            //string year = string.Empty;
            string year = DateTime.Now.Year.ToString();
            string yearf = DateTime.Now.Year.ToString();
            string month = DateTime.Now.Month.ToString();
            if (request.HistoryMonth.HasValue)
            {
                year = request.HistoryMonth.Value.Year.ToString();
                month = request.HistoryMonth.Value.Month.ToString();
                yearf = year;
            }

            //var sqlNew = sql + where;
            //sqlNew = sqlNew.Replace("@ProjectId", request.ProjectId);
            //sqlNew = sqlNew.Replace("@ProcessFactoryCode", request.ProcessFactoryCode);
            //sqlNew = sqlNew.Replace("@Year", year);
            //sqlNew = sqlNew.Replace("@Yearf", yearf);
            //sqlNew = sqlNew.Replace("@Month", month);
            try
            {
                var model = Db.Context.FromSql(sql + where)
                .AddInParameter("@ProjectId", DbType.String, request.ProjectId)
                .AddInParameter("@ProcessFactoryCode", DbType.String, request.ProcessFactoryCode)
                .AddInParameter("@Year", DbType.String, year)
                .AddInParameter("@Yearf", DbType.String, yearf)
                .AddInParameter("@Month", DbType.String, month).ToDataTable();
                return model;
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        #endregion
    }
}
