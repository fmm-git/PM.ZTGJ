using Dos.ORM;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataAccess.RawMaterial;
using PM.DataEntity;
using PM.DataEntity.Production.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PM.Business.Production;
using PM.DataEntity.System.ViewModel;

namespace PM.Business.RawMaterial
{
    /// <summary>
    /// 逻辑处理层
    /// 加工厂批次需求计划
    /// </summary>
    public class FactoryBatchNeedPlanLogic
    {
        //加工厂批次需求计划数据访问处理层类
        public readonly FactoryBatchNeedPlanDA _fbnpDA = new FactoryBatchNeedPlanDA();
        private readonly TbWorkOrderLogic orderProLogic = new TbWorkOrderLogic();

        #region 查询数据

        public class CompanyModel
        {
            /// <summary>
            /// 用户编号
            /// </summary>
            public List<string> CompanyCode { get; set; }
        }

        /// <summary>
        /// 首页查询
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public PageModel GetAllOrBySearch(FPiCiXQPlan entity)
        {
            StringBuilder sb = new StringBuilder();
            //组装查询语句
            #region 模糊搜索条件

            if (!string.IsNullOrWhiteSpace(entity.SupplierCode))
            {
                sb.Append(" and a.SupplierCode='" + entity.SupplierCode + "'");
            }
            if (!string.IsNullOrWhiteSpace(entity.BatchPlanNum))
            {
                sb.Append(" and a.BatchPlanNum like '%" + entity.BatchPlanNum + "'%");
            }
            if (entity.HistoryMonth.HasValue)
            {
                sb.Append(" and YEAR(a.InsertTime)=" + entity.HistoryMonth.Value.Year + " and MONTH(a.InsertTime)=" + entity.HistoryMonth.Value.Month);
            }
            //供货状态
            if (!string.IsNullOrWhiteSpace(entity.StateCode))
            {
                sb.Append(" and a.Examinestatus='审核完成'");
                sb.Append(" and a.StateName='" + entity.StateCode + "'");
                //if (entity.StateCode == "未供货")
                //{
                //    sb.Append(" and ISNULL(i.StateCode,'未供货')='未供货' and CONVERT(varchar(100),ISNULL(i.SupplyDate, DATEADD(DAY,4,A.InsertTime)),23)>=CONVERT(varchar(100),GETDATE(),23)");
                //}
                //else if (entity.StateCode == "超期未供货")
                //{
                //    sb.Append(" and ISNULL(i.StateCode,'未供货')='未供货' and CONVERT(varchar(100),ISNULL(i.SupplyDate, DATEADD(DAY,4,A.InsertTime)),23)<CONVERT(varchar(100),GETDATE(),23)");
                //}
                //else if (entity.StateCode == "供货不足")
                //{
                //    sb.Append(" and ISNULL(i.StateCode,'未供货')='部分供货'");
                //}
                //else if (entity.StateCode == "供货过多")
                //{
                //    sb.Append(" and ISNULL(i.StateCode,'未供货')='已供货' and isnull(i.HasSupplierTotal,0)>isnull(a.BatchPlanTotal,0)+(isnull(a.BatchPlanTotal,0)*0.05) ");
                //}
                //else if (entity.StateCode == "供货完成")
                //{
                //    sb.Append(" and ISNULL(i.StateCode,'未供货')='已供货' and isnull(i.HasSupplierTotal,0)<=isnull(a.BatchPlanTotal,0)+(isnull(a.BatchPlanTotal,0)*0.05) ");
                //}
            }
            //是否按时供货
            if (!string.IsNullOrWhiteSpace(entity.IsAsGh))
            {
                sb.Append(" and a.IsAsGh='" + entity.IsAsGh + "'");
                //if (entity.IsAsGh == "按时供货")
                //{
                //    sb.Append(" and (ISNULL(i.StateCode,'未供货')='部分供货' and CONVERT(varchar(100),ISNULL(i.SupplyDate, DATEADD(DAY,4,A.InsertTime)),23)>=CONVERT(varchar(100),GETDATE(),23)) or (ISNULL(i.StateCode,'未供货')='已供货' and CONVERT(varchar(100),i.SupplyCompleteTime,23)<=CONVERT(varchar(100),i.SupplyDate,23))");
                //}
                //else
                //{
                //    sb.Append(" and (ISNULL(i.StateCode,'未供货')='部分供货' and CONVERT(varchar(100),ISNULL(i.SupplyDate, DATEADD(DAY,4,A.InsertTime)),23)< CONVERT(varchar(100),GETDATE(),23)) or (ISNULL(i.StateCode,'未供货')='已供货' and CONVERT(varchar(100),i.SupplyCompleteTime,23)>CONVERT(varchar(100),i.SupplyDate,23))");
                //}
            }
            //质量问题类型
            if (!string.IsNullOrWhiteSpace(entity.ZlWtType))
            {
                if (entity.ZlWtType == "验收不合格")
                {
                    sb.Append(" and isnull(a.NoPassTotal,0)>0");
                }
                else
                {
                    sb.Append(" and isnull(a.QyBhgCount,0)>0");
                }
            }
            //月度需求计划编号
            if (!string.IsNullOrWhiteSpace(entity.DemandPlanCode))
            {
                sb.Append(" and (a.DemandPlanCode='" + entity.DemandPlanCode + "' or a.RawMaterialDemandNum='" +
                          entity.DemandPlanCode + "')");
            }
            #endregion

            #region 数据权限

            if (!string.IsNullOrWhiteSpace(entity.ProcessFactoryCode))
            {
                sb.Append(" and a.ProcessFactoryCode='" + entity.ProcessFactoryCode + "'");
            }
            if (!string.IsNullOrWhiteSpace(entity.ProjectId))
            {
                sb.Append(" and a.ProjectId='" + entity.ProjectId + "'");
            }
            if (!string.IsNullOrWhiteSpace(entity.SiteCode))
            {
                List<string> SiteList = orderProLogic.GetCompanyWorkAreaOrSiteList(entity.SiteCode, 5);//站点
                List<string> WorkAreaList = orderProLogic.GetCompanyWorkAreaOrSiteList(entity.SiteCode, 4);//工区
                string siteStr = string.Join("','", SiteList);
                string workAreaStr = string.Join("','", WorkAreaList);
                sb.Append(" and (a.SiteCode in('" + siteStr + "') or a.WorkAreaCode in('" + workAreaStr + "'))");
            }

            #endregion

            try
            {
                string sql = @"select * from (select 
                               a.*,b.CompanyFullName BranchName,
                               c.CompanyFullName SiteName,d.CompanyFullName ProcessFactoryName,f.CompanyFullName as WorkAreaName,
                               e.UserName AcceptorName,g.DictionaryText RebarTypeNew,
                               h.UserName,ISNULL(i.StateCode,'未供货') as StateCode,case when ISNULL(i.StateCode,'未供货')='未供货' and ISNULL(i.SupplyDate, DATEADD(DAY,4,A.InsertTime))<GETDATE() then '超期未供货' when ISNULL(i.StateCode,'未供货')='未供货' and ISNULL(i.SupplyDate, DATEADD(DAY,4,A.InsertTime))>=GETDATE() then '未供货' when ISNULL(i.StateCode,'未供货')='部分供货' then '供货不足' when ISNULL(i.StateCode,'未供货')='已供货' and (ISNULL(a.BatchPlanTotal,'')+ISNULL(a.BatchPlanTotal,'')*0.05)<ISNULL(i.HasSupplierTotal,'') then '供货过多' when ISNULL(i.StateCode,'未供货')='已供货' and ISNULL(i.HasSupplierTotal,'')>=ISNULL(a.BatchPlanTotal,'') and ISNULL(i.HasSupplierTotal,'')<=(ISNULL(a.BatchPlanTotal,'')+ISNULL(a.BatchPlanTotal,'')*0.05)  then '供货完成' end StateName,case when ISNULL(i.StateCode,'未供货')='部分供货' and ISNULL(i.SupplyDate, DATEADD(DAY,4,A.InsertTime))<GETDATE() then '超时供货' when ISNULL(i.StateCode,'未供货')='部分供货' and ISNULL(i.SupplyDate, DATEADD(DAY,4,A.InsertTime))>=GETDATE() then '按时供货'  when ISNULL(i.StateCode,'未供货')='已供货' and ISNULL(i.SupplyDate, DATEADD(DAY,4,A.InsertTime))<i.SupplyCompleteTime then '超时供货' when ISNULL(i.StateCode,'未供货')='已供货' and ISNULL(i.SupplyDate, DATEADD(DAY,4,A.InsertTime))>=i.SupplyCompleteTime then '按时供货' end IsAsGh ,i.ID as SupId,
                               ISNULL(i.SupplyDate, DATEADD(DAY,4,A.InsertTime)) as SupplyDate,isnull(i.HasSupplierTotal,0) as HasSupplierTotal,i.SupplyCompleteTime,case when j.DemandPlanCode is null then a.RawMaterialDemandNum else j.DemandPlanCode end DemandPlanCode,k.NoPassTotal,r.QyBhgCount,ghrk.BatchPlanNum as IsAdd,TbGhHg.PassCount
                               from TbFactoryBatchNeedPlan a
                               left join TbCompany b on a.BranchCode = b.CompanyCode 
                               left join TbCompany c on a.SiteCode=c.CompanyCode 
                               left join TbCompany d on a.ProcessFactoryCode=d.CompanyCode 
                               left join TbCompany f on a.WorkAreaCode=f.CompanyCode 
                               left join TbUser e on a.Acceptor=e.UserCode  
                               left join TbSysDictionaryData g on a.SteelsTypeCode=g.DictionaryCode and g.FDictionaryCode='RebarType' 
                               left join TbUser h on a.InsertUserCode=h.UserCode
                               LEFT JOIN TbSupplyList i ON a.BatchPlanNum=i.BatchPlanNum
							   left join TbRawMaterialMonthDemandSupplyPlan j on a.RawMaterialDemandNum=j.SupplyPlanCode
							   left join (select BatchPlanCode,sum(NoPassTotal) as NoPassTotal from TbInOrder group by BatchPlanCode) k on  a.BatchPlanNum=k.BatchPlanCode
                               left join (select io.BatchPlanCode,sum(ioi.PassCount) as QyBhgCount from TbSampleOrder  so
                               left join TbSampleOrderItem soi on so.SampleOrderCode=soi.SampleOrderCode
                               left join TbInOrderItem ioi on soi.InOrderItemId=ioi.ID
                               left join TbInOrder io on so.InOrderCode=io.InOrderCode
                               where (so.ChackResult=2 or so.ChackResult=3) and soi.ChackState=2
                               group by io.BatchPlanCode) r on a.BatchPlanNum=r.BatchPlanCode 
							   left join(select a.BatchPlanNum from TbSupplyListDetailHistory a
							   left join TbInOrderItem b on a.BatchPlanItemNewCode=b.BatchPlanItemNewCode where b.BatchPlanItemNewCode is null group by a.BatchPlanNum) ghrk on a.BatchPlanNum=ghrk.BatchPlanNum
							   left join(select TbGhHg.BatchPlanCode,sum(TbGhHg.PassCount) as PassCount from (select a.BatchPlanCode,case when c.ChackState=2 then 0 else b.PassCount end PassCount from TbInOrder a
                               left join TbInOrderItem b on a.InOrderCode=b.InOrderCode
                               left join TbSampleOrderItem c on b.ID=c.InOrderItemId) TbGhHg  group by TbGhHg.BatchPlanCode) TbGhHg on a.BatchPlanNum=TbGhHg.BatchPlanCode) a
							   where 1=1  ";

                //参数化
                List<Dos.ORM.Parameter> para = new List<Dos.ORM.Parameter>();
                var ret = new PageModel();
                if (entity.IsOutPut)
                {
                    ret.rows = Repository<TbFactoryBatchNeedPlan>.FromSqlToDataTable(sql + sb.ToString(), para, "BatchPlanNum", "desc");
                }
                else
                {
                    ret = Repository<TbFactoryBatchNeedPlan>.FromSqlToPageTable(sql + sb.ToString(), para, entity.rows, entity.page, "BatchPlanNum", "desc");
                }
                return ret;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 以ID查询批次需求计划
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public Tuple<DataTable, DataTable> GetFormJson(int keyValue)
        {
            var ret = Db.Context.From<TbFactoryBatchNeedPlan>()
            .Select(
                    TbFactoryBatchNeedPlan._.All
                    , TbCompany._.CompanyFullName.As("BranchName")
                    , TbUser._.UserName.As("InsertUserName"))
                  .LeftJoin<TbCompany>((a, c) => a.BranchCode == c.CompanyCode)
                  .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                  .Where(TbCompany._.CompanyCode == TbFactoryBatchNeedPlan._.SiteCode), "SiteName")
                  .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                  .Where(TbCompany._.CompanyCode == TbFactoryBatchNeedPlan._.ProcessFactoryCode), "ProcessFactoryName")
                  .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                  .Where(TbCompany._.CompanyCode == TbFactoryBatchNeedPlan._.WorkAreaCode), "WorkAreaName")
                  .AddSelect(Db.Context.From<TbUser>().Select(p => p.UserName)
                  .Where(TbUser._.UserCode == TbFactoryBatchNeedPlan._.Acceptor), "AcceptorName")
                  .AddSelect(Db.Context.From<TbUser>().Select(p => p.UserName)
                  .Where(TbUser._.UserCode == TbFactoryBatchNeedPlan._.SiteStaffCode), "SiteStaffName")
                  .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                    .Where(p => p.ID == keyValue).ToDataTable();
            //查找明细信息
            //var items = Db.Context.From<TbFactoryBatchNeedPlanItem>().Select(
            //   TbFactoryBatchNeedPlanItem._.All,
            //   TbSysDictionaryData._.DictionaryText.As("MeasurementUnitText"),
            //   TbSupplyListDetail._.HasSupplier)
            //   .LeftJoin<TbSysDictionaryData>((a, c) => a.MeasurementUnit == c.DictionaryCode)
            //   .LeftJoin<TbSupplyListDetail>((a, c) => a.ID == c.BatchPlanItemId)
            //   .Where(p => p.BatchPlanNum == ret.Rows[0]["BatchPlanNum"].ToString()).ToDataTable();
            string sql = @"select a.ID,a.RawMaterialNum,a.MaterialName,a.Standard,a.MeasurementUnit,a.BatchPlanQuantity,a.TechnicalRequirement,a.Remarks,a.BatchPlanNum,c.DictionaryText as MeasurementUnitText,d.HasSupplier,isnull(e.Qkl,0) as Qkl from TbFactoryBatchNeedPlanItem a
                            left join TbFactoryBatchNeedPlan b on a.BatchPlanNum=b.BatchPlanNum
                            left join TbSysDictionaryData c on a.MeasurementUnit=c.DictionaryCode
                            left join TbSupplyListDetail d on a.ID=d.BatchPlanItemId
                            left join (select Tb1.MaterialCode,case when ISNULL(Tb1.LockCount,0)+isnull(Tb1.UseCount,0)-ISNULL(WeightSmallPlan,0)>=0 then 0 else -1*(ISNULL(Tb1.LockCount,0)+isnull(Tb1.UseCount,0)-ISNULL(WeightSmallPlan,0)) end as Qkl from (select a.MaterialCode,SUM(COUNT) as COUNT,SUM(a.LockCount) as LockCount,SUM(a.UseCount) as UseCount from TbRawMaterialStockRecord a left join TbStorage b on a.StorageCode=b.StorageCode where a.WorkAreaCode='" + ret.Rows[0]["WorkAreaCode"].ToString() + @"' and b.ProcessFactoryCode='" + ret.Rows[0]["ProcessFactoryCode"].ToString() + @"' and a.ProjectId='" + ret.Rows[0]["ProjectId"].ToString() + @"' group by a.MaterialCode) Tb1
                            left join (select od.MaterialCode,isnull(sum(od.WeightSmallPlan),0) as WeightSmallPlan from TbWorkOrder wo left join TbWorkOrderDetail od on wo.OrderCode=od.OrderCode left join TbCompany cp1 on wo.SiteCode=cp1.CompanyCode where wo.Examinestatus='审核完成' and wo.ProcessingState='Received' and cp1.ParentCompanyCode='" + ret.Rows[0]["WorkAreaCode"].ToString() + @"' and wo.ProcessFactoryCode='" + ret.Rows[0]["ProcessFactoryCode"].ToString() + @"' and wo.ProjectId='" + ret.Rows[0]["ProjectId"].ToString() + @"' group by od.MaterialCode) Tb2 on Tb1.MaterialCode=Tb2.MaterialCode) e on a.RawMaterialNum=e.MaterialCode
                            where a.BatchPlanNum='" + ret.Rows[0]["BatchPlanNum"].ToString() + "'";
            DataTable items = Db.Context.FromSql(sql)
                //.AddInParameter("@ProjectId", DbType.String, ret.Rows[0]["ProjectId"].ToString())
                //.AddInParameter("@ProcessFactoryCode", DbType.String, ret.Rows[0]["ProcessFactoryCode"].ToString())
                //.AddInParameter("@WorkAreaCode", DbType.String, ret.Rows[0]["WorkAreaCode"].ToString())
                //.AddInParameter("@BranchCode", DbType.String, ret.Rows[0]["BranchCode"].ToString())
               .ToDataTable();
            return new Tuple<DataTable, DataTable>(ret, items);
        }

        /// <summary>
        /// 以ID查询批次需求计划
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public Tuple<DataTable, DataTable> GetFormDetailJson(int keyValue)
        {
            var ret = Db.Context.From<TbFactoryBatchNeedPlan>()
            .Select(
                    TbFactoryBatchNeedPlan._.All
                    , TbCompany._.CompanyFullName.As("BranchName")
                    , TbUser._.UserName.As("InsertUserName"))
                  .LeftJoin<TbCompany>((a, c) => a.BranchCode == c.CompanyCode)
                  .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                  .Where(TbCompany._.CompanyCode == TbFactoryBatchNeedPlan._.SiteCode), "SiteName")
                  .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                  .Where(TbCompany._.CompanyCode == TbFactoryBatchNeedPlan._.ProcessFactoryCode), "ProcessFactoryName")
                  .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                  .Where(TbCompany._.CompanyCode == TbFactoryBatchNeedPlan._.WorkAreaCode), "WorkAreaName")
                  .AddSelect(Db.Context.From<TbUser>().Select(p => p.UserName)
                  .Where(TbUser._.UserCode == TbFactoryBatchNeedPlan._.Acceptor), "AcceptorName")
                  .AddSelect(Db.Context.From<TbUser>().Select(p => p.UserName)
                  .Where(TbUser._.UserCode == TbFactoryBatchNeedPlan._.SiteStaffCode), "SiteStaffName")
                  .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                    .Where(p => p.ID == keyValue).ToDataTable();
            //查找明细信息
            string sql = @"select a.ID,a.RawMaterialNum,a.MaterialName,a.Standard,a.MeasurementUnit,a.BatchPlanQuantity,a.TechnicalRequirement,a.Remarks,a.BatchPlanNum,ISNULL(a.Qkl,0) as Qkl,CONVERT(varchar(100), DATEADD(DAY,4,f.InsertTime), 23) as SupplfyDate,s.SupplyCompleteTime,isnull(b.HasSupplier,0) as HasSupplier,
case when isnull(b.HasSupplier,0)<=0 and s.SupplyCompleteTime is null 
and CONVERT(varchar(100), DATEADD(DAY,4,f.InsertTime), 23)<CONVERT(varchar(100),GETDATE(),23) then '超时未供货' 
when isnull(b.HasSupplier,0)<=0 and s.SupplyCompleteTime is null 
and CONVERT(varchar(100), DATEADD(DAY,4,f.InsertTime), 23)>=CONVERT(varchar(100),GETDATE(),23) then '未供货' 
when isnull(b.HasSupplier,0)>0 and (isnull(a.BatchPlanQuantity,0)+(isnull(a.BatchPlanQuantity,0)*0.05))<isnull(b.HasSupplier,0) then '供货过多' 
when isnull(b.HasSupplier,0)>0 and isnull(b.HasSupplier,0)>=isnull(a.BatchPlanQuantity,0) and isnull(b.HasSupplier,0)<=(isnull(a.BatchPlanQuantity,0)+(isnull(a.BatchPlanQuantity,0)*0.05)) then '供货完成' when isnull(b.HasSupplier,0)>0 and isnull(a.BatchPlanQuantity,0)>isnull(b.HasSupplier,0) then '供货不足' end IsGh,f.ProjectId,f.ProcessFactoryCode,f.WorkAreaCode,f.BranchCode,isnull(g.Qkl,0) as Qkl,c.ThisTime,c.ThisTimeCount,d.ID as InorderId,d.NoPass,d.NoPassReason,case when e.ChackState=2 then d.PassCount else 0 end Unqualified,e.Enclosure from TbFactoryBatchNeedPlanItem a
left join TbSupplyList s on a.BatchPlanNum=s.BatchPlanNum
left join TbSupplyListDetail b on  a.BatchPlanNum=b.BatchPlanNum  and a.ID=b.BatchPlanItemId
left join TbSupplyListDetailHistory c on a.BatchPlanNum=c.BatchPlanNum and a.ID=c.BatchPlanItemId 
left join TbInOrderItem d on c.BatchPlanItemNewCode=d.BatchPlanItemNewCode
left join TbSampleOrderItem e on  e.InOrderItemId=d.ID
left join TbFactoryBatchNeedPlan f on a.BatchPlanNum=f.BatchPlanNum
left join (select Tb1.MaterialCode,case when ISNULL(Tb1.LockCount,0)+isnull(Tb1.UseCount,0)-ISNULL(WeightSmallPlan,0)>=0 then 0 else -1*(ISNULL(Tb1.LockCount,0)+isnull(Tb1.UseCount,0)-ISNULL(WeightSmallPlan,0)) end as Qkl from (select a.MaterialCode,SUM(COUNT) as COUNT,SUM(a.LockCount) as LockCount,SUM(a.UseCount) as UseCount from TbRawMaterialStockRecord a left join TbStorage b on a.StorageCode=b.StorageCode where a.WorkAreaCode='" + ret.Rows[0]["WorkAreaCode"].ToString() + @"' and b.ProcessFactoryCode='" + ret.Rows[0]["ProcessFactoryCode"].ToString() + @"' and a.ProjectId='" + ret.Rows[0]["ProjectId"].ToString() + @"' group by a.MaterialCode) Tb1
left join (select od.MaterialCode,isnull(sum(od.WeightSmallPlan),0) as WeightSmallPlan from TbWorkOrder wo left join TbWorkOrderDetail od on wo.OrderCode=od.OrderCode left join TbCompany cp1 on wo.SiteCode=cp1.CompanyCode where wo.Examinestatus='审核完成' and wo.ProcessingState='Received' and cp1.ParentCompanyCode='" + ret.Rows[0]["WorkAreaCode"].ToString() + @"' and wo.ProcessFactoryCode='" + ret.Rows[0]["ProcessFactoryCode"].ToString() + @"' and wo.ProjectId='" + ret.Rows[0]["ProjectId"].ToString() + @"' group by od.MaterialCode) Tb2 on Tb1.MaterialCode=Tb2.MaterialCode) g on a.RawMaterialNum=g.MaterialCode where a.BatchPlanNum='" + ret.Rows[0]["BatchPlanNum"].ToString() + @"' order by a.ID asc,c.ThisTime desc";
            DataTable items = Db.Context.FromSql(sql)
                //.AddInParameter("@ProjectId", DbType.String, ret.Rows[0]["ProjectId"].ToString())
                //.AddInParameter("@ProcessFactoryCode", DbType.String, ret.Rows[0]["ProcessFactoryCode"].ToString())
                //.AddInParameter("@WorkAreaCode", DbType.String, ret.Rows[0]["WorkAreaCode"].ToString())
                //.AddInParameter("@BranchCode", DbType.String, ret.Rows[0]["BranchCode"].ToString())
                .ToDataTable();
            return new Tuple<DataTable, DataTable>(ret, items);
        }

        /// <summary>
        /// 月度需求计划弹窗
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageModel GetXQJHGridJson(RawMonthDemPlanRequest request, string keyword)
        {
            //DateTime now = DateTime.Now;
            //DateTime FirstDay = new DateTime(now.Year, now.Month, 1); //本月第一天
            //DateTime LastDay = Convert.ToDateTime(FirstDay.AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd") + " 23:59:59"); //本月最后一天

            //            string addsql = "";
            //            if (!string.IsNullOrWhiteSpace(keyword))
            //            {
            //                addsql += " and c1.CompanyFullName like '%" + keyword + "%' ";
            //            }
            //            if (!string.IsNullOrEmpty(request.ProjectId))
            //            {
            //                addsql += " and dp.ProjectId='" + request.ProjectId + "'";
            //            }
            //            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            //            {
            //                List<string> SiteList = orderProLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);//站点
            //                List<string> WorkAreaList = orderProLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 4);//工区
            //                string siteStr = string.Join("','", SiteList);
            //                string workAreaStr = string.Join("','", WorkAreaList);
            //                addsql += " and (dp.SiteCode in('" + siteStr + "') or dp.WorkAreaCode in('" + workAreaStr + "'))";
            //            }
            //            string sql = @"SELECT * FROM (
            //                            select 
            //                            CASE WHEN bcjh.IsSupply>0 THEN '' ELSE dp.DemandPlanCode END DemandPlanCode,
            //                            bcjh.SupplyPlanCode,dp.BranchCode,c1.CompanyFullName as BranchName,dp.WorkAreaCode,c4.CompanyFullName as WorkAreaName,dp.SiteCode,
            //                            c2.CompanyFullName as SiteName,
            //                            CASE WHEN bcjh.IsSupply>0 THEN bcjh.ProcessFactoryCode ELSE dp.ProcessFactoryCode END ProcessFactoryCode,
            //                            CASE WHEN bcjh.IsSupply>0 THEN c5.CompanyFullName ELSE c3.CompanyFullName END ProcessFactoryName,
            //                            dp.RebarType,s1.DictionaryText as RebarTypeName,
            //                            CASE
            //                            WHEN bcjh.SupplyPlanCode IS NOT NULL THEN
            //                            (isnull(bcjh.GrandTotalPlanNum,0)-isnull(pcjh.BatchPlanQuantity,0))
            //                            ELSE 
            //                            (isnull(dp.PlanTotal,0)-isnull(pcjh.BatchPlanQuantity,0))	
            //                            	END  as PlanTotal,
            //                            dp.DeliveryDate,dp.DeliveryAdd,dp.DemandMonth 
            //                            from TbRawMaterialMonthDemandPlan dp
            //                            left join TbCompany c1 on dp.BranchCode=c1.CompanyCode
            //                            left join TbCompany c2 on dp.SiteCode=c2.CompanyCode
            //                            left join TbCompany c3 on dp.ProcessFactoryCode=c3.CompanyCode 
            //                            left join TbCompany c4 on dp.WorkAreaCode=c4.CompanyCode
            //                            left join TbSysDictionaryData s1 on s1.DictionaryCode=dp.RebarType
            //                            LEFT JOIN TbRawMaterialMonthDemandSupplyPlan bcjh on dp.DemandPlanCode=bcjh.DemandPlanCode AND bcjh.Examinestatus='审核完成' 
            //                            left JOIN
            //                             (
            //                             	select fbp.RawMaterialDemandNum,sum(fbp.BatchPlanTotal) as BatchPlanQuantity  
            //                             	from TbFactoryBatchNeedPlan fbp
            //                                group by fbp.RawMaterialDemandNum
            //                             ) as pcjh on (pcjh.RawMaterialDemandNum=dp.DemandPlanCode OR pcjh.RawMaterialDemandNum=bcjh.SupplyPlanCode) 
            //                            left join TbCompany c5 on bcjh.ProcessFactoryCode=c5.CompanyCode  
            //                            where  1=1  " + addsql +
            //                            @"and dp.Examinestatus='审核完成'
            //                            ) retData WHERE PlanTotal>0";
            string where = "";
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                where += " and Tb.BranchName like '%" + keyword + "%' or Tb.WorkAreaName like '%" + keyword + "%' or Tb.DemandPlanCode like '%" + keyword + "%' ";
            }
            if (!string.IsNullOrEmpty(request.ProjectId))
            {
                where += " and Tb.ProjectId='" + request.ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where += " and Tb.ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> WorkAreaList = orderProLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 4);//工区
                string workAreaStr = string.Join("','", WorkAreaList);
                where += " and Tb.WorkAreaCode in('" + workAreaStr + "')";
            }
            string sql = @"select Tb.*,isnull(Tb.PlanTotalOld,0)+isnull(Tb.SupplyPlanNum,0)-isnull(Tb.BatchPlanTotal,0) as PlanTotal from (select a.DemandMonth,a.DemandPlanCode,a.ProjectId,a.BranchCode,cp1.CompanyFullName as BranchName,a.WorkAreaCode,cp2.CompanyFullName as WorkAreaName,a.ProcessFactoryCode,cp3.CompanyFullName as ProcessFactoryName,a.RebarType,sd.DictionaryText as RebarTypeName,a.PlanTotal as PlanTotalOld,a.DeliveryDate,a.DeliveryAdd,case when b.Examinestatus='审核完成' then b.SupplyPlanCode else '' end SupplyPlanCode,case when b.Examinestatus='审核完成' then b.SupplyPlanNum else 0 end SupplyPlanNum,TbPc.BatchPlanTotal from TbRawMaterialMonthDemandPlan a
                            left join TbRawMaterialMonthDemandSupplyPlan b on a.DemandPlanCode=b.DemandPlanCode
                            left join (select TbPc.DemandPlanCode,SUM(TbPc.BatchPlanTotal) as BatchPlanTotal from (select a.BatchPlanTotal,case when b.DemandPlanCode is null then a.RawMaterialDemandNum else b.DemandPlanCode end DemandPlanCode from TbFactoryBatchNeedPlan a
                            left join TbRawMaterialMonthDemandSupplyPlan b on  a.RawMaterialDemandNum=b.SupplyPlanCode) TbPc
                            group by TbPc.DemandPlanCode) TbPc on a.DemandPlanCode=TbPc.DemandPlanCode
                            left join TbCompany cp1 on a.BranchCode=cp1.CompanyCode
                            left join TbCompany cp2 on a.WorkAreaCode=cp2.CompanyCode
                            left join TbCompany cp3 on a.ProcessFactoryCode=cp3.CompanyCode
                            left join TbSysDictionaryData sd on a.RebarType=sd.DictionaryCode
                            where 1=1 and a.Examinestatus='审核完成') Tb where 1=1 and isnull(Tb.PlanTotalOld,0)+isnull(Tb.SupplyPlanNum,0)-isnull(Tb.BatchPlanTotal,0)>0 " + where + @"";

            //参数化
            List<Dos.ORM.Parameter> para = new List<Dos.ORM.Parameter>();
            var data = Repository<TbDistributionPlanInfo>.FromSqlToPageTable(sql, para, request.rows, request.page, "DemandPlanCode", "desc");
            return data;

        }

        /// <summary>
        /// 自动带入需求计划明细
        /// </summary>
        /// <returns></returns>
        public DataTable GetXQJHDetail(string number, string ProjectId, string ProcessFactoryCode, string WorkAreaCode, string BranchCode)
        {
            //var data = _fbnpDA.GetXQJHDetail(number,ProjectId,ProcessFactoryCode,WorkAreaCode,BranchCode);
            string sql = @"select TbJh.DemandPlanCode,TbJh.MaterialCode,TbJh.MaterialName,TbJh.SpecificationModel,TbJh.MeasurementUnit,sd.DictionaryText as MeasurementUnitText,isnull(TbJh.DemandNum,0)-isnull(TbPc.BatchPlanQuantity,0) as DemandNum,isnull(TbQkl.Qkl,0) as Qkl from (select Tb.DemandPlanCode,Tb.MaterialCode,Tb.MaterialName,Tb.SpecificationModel,Tb.MeasurementUnit,SUM(Tb.DemandNum) as DemandNum from (select b.DemandPlanCode,b.MaterialCode,b.MaterialName,b.SpecificationModel,b.MeasurementUnit,b.DemandNum from TbRawMaterialMonthDemandPlan a
                            left join TbRawMaterialMonthDemandPlanDetail b on a.DemandPlanCode=b.DemandPlanCode where 1=1 and b.DemandPlanCode=@number and b.DemandNum>0
                            union all
                            select a.DemandPlanCode,b.MaterialCode,b.MaterialName,b.SpecificationModel,b.MeasurementUnit,b.SupplyNum from TbRawMaterialMonthDemandSupplyPlan a
                            left join TbRawMaterialMonthDemandSupplyPlanDetail b on a.SupplyPlanCode=b.SupplyPlanCode where 1=1 and a.DemandPlanCode=@number and b.SupplyNum>0) Tb group by Tb.DemandPlanCode,Tb.MaterialCode,Tb.MaterialName,Tb.SpecificationModel,Tb.MeasurementUnit) TbJh
                            left join (select TbPc.DemandPlanCode,TbPc.RawMaterialNum,SUM(TbPc.BatchPlanQuantity) as BatchPlanQuantity from (select case when c.DemandPlanCode is null then a.RawMaterialDemandNum else c.DemandPlanCode end DemandPlanCode,b.BatchPlanQuantity,b.RawMaterialNum from TbFactoryBatchNeedPlan a
                            left join TbFactoryBatchNeedPlanItem b on a.BatchPlanNum=b.BatchPlanNum
                            left join TbRawMaterialMonthDemandSupplyPlan c on a.RawMaterialDemandNum=c.SupplyPlanCode) TbPc
                            group by TbPc.DemandPlanCode,TbPc.RawMaterialNum) TbPc on TbJh.DemandPlanCode=TbPc.DemandPlanCode and TbJh.MaterialCode=TbPc.RawMaterialNum
                            left join (select Tb1.MaterialCode,(isnull(Tb2.WeightSmallPlan,0)-isnull(Tb1.KcDtZl,0)) as Qkl from (select MaterialCode,ProjectId,TbStorage.ProcessFactoryCode,WorkAreaCode,TbCompany.ParentCompanyCode as BranchCode,SUM(LockCount)+SUM(UseCount) as KcDtZl
                            FROM TbRawMaterialStockRecord
                            left join TbStorage on TbRawMaterialStockRecord.StorageCode=TbStorage.StorageCode
                            left join TbCompany on TbRawMaterialStockRecord.WorkAreaCode=TbCompany.CompanyCode 
                            where 1=1 and TbRawMaterialStockRecord.ProjectId=@ProjectId 
                            and TbRawMaterialStockRecord.WorkAreaCode=@WorkAreaCode 
                            and TbStorage.ProcessFactoryCode=@ProcessFactoryCode and TbCompany.ParentCompanyCode=@BranchCode
                            group by MaterialCode,ProjectId,TbStorage.ProcessFactoryCode,WorkAreaCode,TbCompany.ParentCompanyCode) Tb1
                            left join (select od.MaterialCode,od.MaterialName,od.SpecificationModel,isnull(sum(od.WeightSmallPlan),0) as WeightSmallPlan,wo.ProjectId,cp1.ParentCompanyCode as WorkAreaCode,cp2.ParentCompanyCode as BranchCode,wo.ProcessFactoryCode from TbWorkOrder wo 
                            left join TbWorkOrderDetail od on wo.OrderCode=od.OrderCode
                            left join TbCompany cp1 on wo.SiteCode=cp1.CompanyCode
                            left join TbCompany cp2 on cp1.ParentCompanyCode=cp2.CompanyCode
                            where 1=1 and wo.Examinestatus='审核完成' and wo.ProcessingState='Received' and wo.ProjectId=@ProjectId and wo.ProcessFactoryCode=@ProcessFactoryCode and cp1.ParentCompanyCode=@WorkAreaCode and cp2.ParentCompanyCode=@BranchCode
                            group by od.MaterialCode,od.MaterialName,od.SpecificationModel,wo.ProjectId,cp1.ParentCompanyCode,cp2.ParentCompanyCode,wo.ProcessFactoryCode) Tb2 on Tb1.MaterialCode=Tb2.MaterialCode and Tb1.ProcessFactoryCode=Tb2.ProcessFactoryCode and Tb1.BranchCode=Tb2.BranchCode and Tb1.WorkAreaCode=Tb2.WorkAreaCode and Tb1.ProjectId=Tb2.ProjectId) TbQkl on TbJh.MaterialCode=TbQkl.MaterialCode
                            left join TbSysDictionaryData sd on TbJh.MeasurementUnit=sd.DictionaryCode where isnull(TbJh.DemandNum,0)-isnull(TbPc.BatchPlanQuantity,0)>0";
            var data = Db.Context.FromSql(sql)
              .AddInParameter("@number", DbType.String, number)
              .AddInParameter("@ProjectId", DbType.String, ProjectId)
              .AddInParameter("@ProcessFactoryCode", DbType.String, ProcessFactoryCode)
              .AddInParameter("@WorkAreaCode", DbType.String, WorkAreaCode)
              .AddInParameter("@BranchCode", DbType.String, BranchCode).ToDataTable();
            return data;
        }

        public PageModel GetProcessFactoryUser(TbCompanyRequest request)
        {
            //参数化
            List<Parameter> parameter = new List<Parameter>();
            string where = " where 1=1 and ur.OrgType=1 and Flag=0 and ur.UserCode!='admin' ";
            if (!string.IsNullOrWhiteSpace(request.keyword))
            {
                where += " and c.CompanyFullName like @keyword or u.UserName like @keyword";
                parameter.Add(new Parameter("@keyword", '%' + request.keyword + '%', DbType.String, null));
            }
            //if (!string.IsNullOrWhiteSpace(request.ProjectId))
            //{
            //    where += " and (ISNULL(@ProjectId,'')='' or ur.ProjectId=@ProjectId)";
            //    parameter.Add(new Parameter("@ProjectId", request.ProjectId, DbType.String, null));
            //}
            if (!string.IsNullOrWhiteSpace(request.CompanyCode))
            {
                where += " and (ISNULL(@CompanyCode,'')='' or ur.OrgId=@CompanyCode)";
                parameter.Add(new Parameter("@CompanyCode", request.CompanyCode, DbType.String, null));
            }
            string sql = @"select ur.ID,u.UserCode,ur.UserCode as UserId,u.UserName,ur.OrgId as CompanyCode,CompanyFullName from TbUserRole ur
left join TbUser u on ur.UserCode=u.UserId
left join TbCompany c on ur.OrgId=c.CompanyCode ";
            var model = Repository<TbCompany>.FromSqlToPageTable(sql + where, parameter, request.rows, request.page, "ID", "asc");
            return model;
        }
        #endregion

        #region （新增、编辑）数据

        /// <summary>
        /// 新增数据
        /// </summary>
        public AjaxResult Insert(TbFactoryBatchNeedPlan model, List<TbFactoryBatchNeedPlanItem> items)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            model.Examinestatus = "未发起";
            model.InsertUserCode = OperatorProvider.Provider.CurrentUser.UserCode;
            TbFactoryBatchNeedPlan fbnpModel = null;
            if (model.IsDefault == 1)
            {
                fbnpModel = Repository<TbFactoryBatchNeedPlan>.First(p => p.ProcessFactoryCode == model.ProcessFactoryCode && p.IsDefault == 1);
                if (fbnpModel != null)
                {
                    fbnpModel.IsDefault = 0;
                }
            }
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //添加信息,明细信息
                    Repository<TbFactoryBatchNeedPlan>.Insert(trans, model);
                    if (fbnpModel != null)
                    {
                        Repository<TbFactoryBatchNeedPlan>.Update(trans, fbnpModel);
                    }
                    Repository<TbFactoryBatchNeedPlanItem>.Insert(trans, items);
                    trans.Commit();
                    return AjaxResult.Success();
                }
            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.ToString());
            }
        }
        //public AjaxResult Insert(TbFactoryBatchNeedPlan model, List<TbFactoryBatchNeedPlanItem> items, TbSupplyList supList, List<TbSupplyListDetail> supDetail)
        //{
        //    if (model == null)
        //        return AjaxResult.Warning("参数错误");
        //    model.Examinestatus = "未发起";
        //    model.InsertUserCode = OperatorProvider.Provider.CurrentUser.UserCode;

        //    //供应清单
        //    supList.InsertUserCode = OperatorProvider.Provider.CurrentUser.UserCode;
        //    supList.SupplyDate = model.InsertTime;
        //    supList.StateCode = "未供货";
        //    //----end
        //    try
        //    {
        //        using (DbTrans trans = Db.Context.BeginTransaction())
        //        {
        //            //添加信息,明细信息
        //            Repository<TbFactoryBatchNeedPlan>.Insert(trans, model);
        //            Repository<TbFactoryBatchNeedPlanItem>.Insert(trans, items);

        //            //添加供应清单及明细
        //            Repository<TbSupplyList>.Insert(trans, supList);
        //            Repository<TbSupplyListDetail>.Insert(trans, supDetail);

        //            trans.Commit();
        //            return AjaxResult.Success();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return AjaxResult.Error(ex.ToString());
        //    }
        //}

        /// <summary>
        /// 修改数据
        /// </summary>
        public AjaxResult Update(TbFactoryBatchNeedPlan model, List<TbFactoryBatchNeedPlanItem> items)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            var anyRet = AnyInfo(model.ID);
            if (anyRet.state.ToString() != ResultType.success.ToString())
                return anyRet;
            TbFactoryBatchNeedPlan fbnpModel = null;
            if (model.IsDefault == 1)
            {
                fbnpModel = Repository<TbFactoryBatchNeedPlan>.First(p => p.ProcessFactoryCode == model.ProcessFactoryCode && p.IsDefault == 1);
                if (fbnpModel != null)
                {
                    fbnpModel.IsDefault = 0;
                }
            }
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())//使用事务
                {
                    //修改信息
                    Repository<TbFactoryBatchNeedPlan>.Update(trans, model, p => p.ID == model.ID);
                    if (fbnpModel != null)
                    {
                        Repository<TbFactoryBatchNeedPlan>.Update(trans, fbnpModel);
                    }
                    if (items.Count > 0)
                    {
                        //删除历史明细信息,添加明细信息
                        Repository<TbFactoryBatchNeedPlanItem>.Delete(p => p.BatchPlanNum == model.BatchPlanNum);
                        Repository<TbFactoryBatchNeedPlanItem>.Insert(trans, items);

                    }
                    trans.Commit();//提交事务

                    return AjaxResult.Success();
                }
            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.ToString());
            }
        }
        //public AjaxResult Update(TbFactoryBatchNeedPlan model, List<TbFactoryBatchNeedPlanItem> items, TbSupplyList supList, List<TbSupplyListDetail> supDetail)
        //{
        //    if (model == null)
        //        return AjaxResult.Warning("参数错误");
        //    var anyRet = AnyInfo(model.ID);
        //    if (anyRet.state.ToString() != ResultType.success.ToString())
        //        return anyRet;
        //    try
        //    {
        //        using (DbTrans trans = Db.Context.BeginTransaction())//使用事务
        //        {
        //            //修改信息
        //            Repository<TbFactoryBatchNeedPlan>.Update(trans, model, p => p.ID == model.ID);
        //            Repository<TbSupplyList>.Update(trans, supList, p => p.ID == supList.ID);

        //            if (items.Count > 0)
        //            {
        //                //删除历史明细信息,添加明细信息
        //                Repository<TbFactoryBatchNeedPlanItem>.Delete(p => p.BatchPlanNum == model.BatchPlanNum);
        //                Repository<TbFactoryBatchNeedPlanItem>.Insert(trans, items);

        //                //供应清单及明细
        //                Repository<TbSupplyListDetail>.Delete(p => p.BatchPlanNum == supList.BatchPlanNum);
        //                Repository<TbSupplyListDetail>.Insert(trans, supDetail);

        //            }
        //            trans.Commit();//提交事务

        //            return AjaxResult.Success();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return AjaxResult.Error(ex.ToString());
        //    }
        //}

        /// <summary>
        /// 判断信息是否可操作
        /// </summary>
        /// <returns></returns>
        public AjaxResult AnyInfo(int keyValue)
        {
            var monthPlan = Repository<TbFactoryBatchNeedPlan>.First(p => p.ID == keyValue);
            if (monthPlan == null)
                return AjaxResult.Warning("信息不存在");
            if (monthPlan.Examinestatus != "未发起")
                return AjaxResult.Warning("信息正在审核中或已审核完成,不能进行此操作");

            return AjaxResult.Success(monthPlan);
        }
        /// <summary>
        /// 获取默认验收人、电话信息
        /// </summary>
        /// <param name="ProcessFactoryCode"></param>
        /// <returns></returns>
        public DataTable GetIsDefault(string ProcessFactoryCode)
        {
            string sql = @"select Acceptor,TbUser.UserName as AcceptorName,ContactWay from TbFactoryBatchNeedPlan 
left join TbUser on TbFactoryBatchNeedPlan.Acceptor=TbUser.UserCode
where ProcessFactoryCode=@ProcessFactoryCode and IsDefault=1";
            var ret = Db.Context.FromSql(sql)
                .AddInParameter("@ProcessFactoryCode", DbType.String, ProcessFactoryCode).ToDataTable();
            return ret;
        }

        public PageModel GetMaterialDataList(RawMaterialStockRequest request)
        {
            string where = "where 1=1";
            if (!string.IsNullOrWhiteSpace(request.keyword))
            {
                where += (" and a.MaterialName like '%" + request.keyword + "%' or a.SpecificationModel like '%" + request.keyword + "%'");
            }
            string sql = @"select a.*,b.DictionaryText as MeasurementUnitText,c.DictionaryText as RebarTypeName,isnull(Tbqk.Qkl,0) as Qkl from TbRawMaterialArchives a
                            left join TbSysDictionaryData b on a.MeasurementUnit=b.DictionaryCode
                            left join TbSysDictionaryData c on a.RebarType=c.DictionaryCode
                            left join (select Tb1.MaterialCode,(isnull(Tb2.WeightSmallPlan,0)-isnull(Tb1.KcDtZl,0)) as Qkl from (select MaterialCode,ProjectId,TbStorage.ProcessFactoryCode,WorkAreaCode,TbCompany.ParentCompanyCode as BranchCode,SUM(LockCount)+SUM(UseCount) as KcDtZl
                            FROM TbRawMaterialStockRecord
                            left join TbStorage on TbRawMaterialStockRecord.StorageCode=TbStorage.StorageCode
                            left join TbCompany on TbRawMaterialStockRecord.WorkAreaCode=TbCompany.CompanyCode where 1=1 and TbRawMaterialStockRecord.ProjectId=@ProjectId and TbRawMaterialStockRecord.WorkAreaCode=@WorkAreaCode and TbStorage.ProcessFactoryCode=@ProcessFactoryCode and TbCompany.ParentCompanyCode=@BranchCode
                            group by MaterialCode,ProjectId,TbStorage.ProcessFactoryCode,WorkAreaCode,TbCompany.ParentCompanyCode) Tb1
                            left join (select od.MaterialCode,od.MaterialName,od.SpecificationModel,isnull(sum(od.WeightSmallPlan),0) as WeightSmallPlan,wo.ProjectId,cp1.ParentCompanyCode as WorkAreaCode,cp2.ParentCompanyCode as BranchCode,wo.ProcessFactoryCode from TbWorkOrder wo 
                            left join TbWorkOrderDetail od on wo.OrderCode=od.OrderCode
                            left join TbCompany cp1 on wo.SiteCode=cp1.CompanyCode
                            left join TbCompany cp2 on cp1.ParentCompanyCode=cp2.CompanyCode
                            where 1=1 and wo.Examinestatus='审核完成' and wo.ProcessingState='Received' and wo.ProjectId=@ProjectId and wo.ProcessFactoryCode=@ProcessFactoryCode and cp1.ParentCompanyCode=@WorkAreaCode and cp2.ParentCompanyCode=@BranchCode
                            group by od.MaterialCode,od.MaterialName,od.SpecificationModel,wo.ProjectId,cp1.ParentCompanyCode,cp2.ParentCompanyCode,wo.ProcessFactoryCode) Tb2 on Tb1.MaterialCode=Tb2.MaterialCode and Tb1.ProcessFactoryCode=Tb2.ProcessFactoryCode and Tb1.BranchCode=Tb2.BranchCode and Tb1.WorkAreaCode=Tb2.WorkAreaCode and Tb1.ProjectId=Tb2.ProjectId) Tbqk on a.MaterialCode=Tbqk.MaterialCode ";
            //参数化
            List<Dos.ORM.Parameter> para = new List<Dos.ORM.Parameter>();
            para.Add(new Parameter("@ProjectId", request.ProjectId, DbType.String, null));
            para.Add(new Parameter("@ProcessFactoryCode", request.ProcessFactoryCode, DbType.String, null));
            para.Add(new Parameter("@WorkAreaCode", request.WorkAreaCode, DbType.String, null));
            para.Add(new Parameter("@BranchCode", request.BranchCode, DbType.String, null));
            var ret = Repository<TbRawMaterialArchives>.FromSqlToPageTable(sql + where, para, request.rows, request.page, "ID", "asc");
            return ret;
        }
        #endregion

        #region 删除数据

        /// <summary>
        /// 删除数据
        /// </summary>
        public AjaxResult Delete(int keyValue)
        {
            try
            {
                //判断信息是否存在
                var anyRet = AnyInfo(keyValue);
                if (anyRet.state.ToString() != ResultType.success.ToString())
                    return anyRet;
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //删除信息,明细信息
                    var count = Repository<TbFactoryBatchNeedPlan>.Delete(trans, p => p.ID == keyValue);
                    Repository<TbFactoryBatchNeedPlanItem>.Delete(trans, p => p.BatchPlanNum == ((TbFactoryBatchNeedPlan)anyRet.data).BatchPlanNum);

                    //供应清单及明细
                    var count1 = Repository<TbSupplyList>.Delete(trans, p => p.ID == keyValue);
                    Repository<TbSupplyListDetail>.Delete(trans, p => p.BatchPlanNum == ((TbFactoryBatchNeedPlan)anyRet.data).BatchPlanNum);

                    trans.Commit();//提交事务

                    return AjaxResult.Success();
                }
            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.ToString());
            }
        }

        #endregion

        #region 导入
        public DataTable GetUnitCode(string FDictionaryCode, string unitName)
        {
            string sql = @"select top 1 DictionaryCode from TbSysDictionaryData where FDictionaryCode=@FDictionaryCode and DictionaryText=@unitName";
            DataTable dt = Db.Context.FromSql(sql)
                .AddInParameter("@FDictionaryCode", DbType.String, FDictionaryCode)
                .AddInParameter("@unitName", DbType.String, unitName).ToDataTable();
            return dt;
        }
        #endregion

        #region 图形报表
        /// <summary>
        /// 图形1
        /// </summary>
        /// <param name="RebarType">钢筋类型</param>
        /// <returns></returns>
        public DataTable Img1(FPiCiXQPlan ent)
        {
            string where = " and a.Examinestatus='审核完成'";
            if (!string.IsNullOrWhiteSpace(ent.ProjectId))
            {
                where += " and a.ProjectId='" + ent.ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(ent.ProcessFactoryCode))
            {
                where += " and a.ProcessFactoryCode='" + ent.ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(ent.SiteCode))
            {
                List<string> WorkAreaList = orderProLogic.GetCompanyWorkAreaOrSiteList(ent.SiteCode, 4);//工区
                string workAreaStr = string.Join("','", WorkAreaList);
                where += " and a.WorkAreaCode in('" + workAreaStr + "')";
            }
            if (!string.IsNullOrWhiteSpace(ent.SteelsTypeCode))
            {
                where += " and a.SteelsTypeCode='" + ent.SteelsTypeCode + "'";
            }
            string sql = @"select TbStateCode.StateCode,isnull(Tb.StateCount,0) as StateCount,isnull(Tb.BatchPlanTotal,0) as BatchPlanTotal from (select '未供货' as StateCode
                            union all
                            select '超期未供货' as StateCode
                            union all
                            select '供货不足' as StateCode
                            union all
                            select '供货完成' as StateCode
                            union all
                            select '供货过多' as StateCode) TbStateCode
                            left join (select TbGh.StateCode,Count(1) as StateCount,isnull(sum(TbGh.BatchPlanTotal),0) as BatchPlanTotal from (select a.BatchPlanNum,a.BatchPlanTotal,DATEADD(DAY,4,A.InsertTime) as SupplyDate,isnull(b.HasSupplierTotal,0) as HasSupplierTotal,b.SupplyCompleteTime,'未供货' as StateCode from TbFactoryBatchNeedPlan a
                            left join TbSupplyList b on a.BatchPlanNum=b.BatchPlanNum
                            where 1=1 " + where + @" and CONVERT(varchar(100),GETDATE(),23)<=CONVERT(varchar(100), DATEADD(DAY,4,A.InsertTime), 23) and b.SupplyCompleteTime is null and ISNULL(b.HasSupplierTotal,0)=0
                            union all
                            select a.BatchPlanNum,a.BatchPlanTotal,DATEADD(DAY,4,A.InsertTime) as SupplyDate,isnull(b.HasSupplierTotal,0) as HasSupplierTotal,b.SupplyCompleteTime,'超期未供货' as StateCode from TbFactoryBatchNeedPlan a
                            left join TbSupplyList b on a.BatchPlanNum=b.BatchPlanNum
                            where 1=1 " + where + @" and CONVERT(varchar(100),GETDATE(),23)>CONVERT(varchar(100), DATEADD(DAY,4,A.InsertTime), 23) and b.SupplyCompleteTime is null and ISNULL(b.HasSupplierTotal,0)=0
                            union all
                            select a.BatchPlanNum,a.BatchPlanTotal,DATEADD(DAY,4,A.InsertTime) as SupplyDate,isnull(b.HasSupplierTotal,0) as HasSupplierTotal,b.SupplyCompleteTime,'供货不足' as StateCode from TbFactoryBatchNeedPlan a
                            left join TbSupplyList b on a.BatchPlanNum=b.BatchPlanNum
                            where 1=1 " + where + @" and ISNULL(b.HasSupplierTotal,0)>0 and b.StateCode='部分供货'
                            union all
                            select a.BatchPlanNum,a.BatchPlanTotal,DATEADD(DAY,4,A.InsertTime) as SupplyDate,isnull(b.HasSupplierTotal,0) as HasSupplierTotal,b.SupplyCompleteTime,'供货过多' as StateCode from TbFactoryBatchNeedPlan a
                            left join TbSupplyList b on a.BatchPlanNum=b.BatchPlanNum
                            where 1=1 " + where + @" and ISNULL(b.HasSupplierTotal,0)>0 and b.StateCode='已供货' and b.BatchPlanTotal+(Isnull(b.BatchPlanTotal,0)*0.05)<b.HasSupplierTotal
                            union all
                            select a.BatchPlanNum,a.BatchPlanTotal,DATEADD(DAY,4,A.InsertTime) as SupplyDate,isnull(b.HasSupplierTotal,0) as HasSupplierTotal,b.SupplyCompleteTime,'供货完成' as StateCode from TbFactoryBatchNeedPlan a
                            left join TbSupplyList b on a.BatchPlanNum=b.BatchPlanNum
                            where 1=1 " + where + @" and ISNULL(b.HasSupplierTotal,0)>0 and b.StateCode='已供货' and b.BatchPlanTotal+(Isnull(b.BatchPlanTotal,0)*0.05)>=b.HasSupplierTotal
                            ) TbGh group by TbGh.StateCode) Tb on TbStateCode.StateCode=Tb.StateCode";
            try
            {
                var ret = Db.Context.FromSql(sql).ToDataTable();
                return ret;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        /// <summary>
        /// 图形2
        /// </summary>
        /// <param name="RebarType">钢筋类型</param>
        /// <returns></returns>
        public DataTable Img2(FPiCiXQPlan ent)
        {
            string where = " and a.Examinestatus='审核完成'";
            //string OrgType = OperatorProvider.Provider.CurrentUser.OrgType;
            //string ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
            //string ProcessFactoryCode = OperatorProvider.Provider.CurrentUser.ProcessFactoryCode;
            //if (!string.IsNullOrWhiteSpace(RebarType))
            //{
            //    where += " and a.SteelsTypeCode=@RebarType";
            //}
            //if (OrgType != "1")
            //{
            //    where += " and a.ProjectId=@ProjectId";
            //}
            //if (!string.IsNullOrWhiteSpace(DemandMonth))
            //{
            //    where += " and CONVERT(varchar(7), DATEADD(DAY,4,A.InsertTime), 120)='" + DemandMonth + "'";
            //}
            //where += " and a.ProcessFactoryCode=@ProcessFactoryCode";
            if (!string.IsNullOrWhiteSpace(ent.ProjectId))
            {
                where += " and a.ProjectId='" + ent.ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(ent.ProcessFactoryCode))
            {
                where += " and a.ProcessFactoryCode='" + ent.ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(ent.SiteCode))
            {
                List<string> WorkAreaList = orderProLogic.GetCompanyWorkAreaOrSiteList(ent.SiteCode, 4);//工区
                string workAreaStr = string.Join("','", WorkAreaList);
                where += " and a.WorkAreaCode in('" + workAreaStr + "')";
            }
            if (!string.IsNullOrWhiteSpace(ent.SteelsTypeCode))
            {
                where += " and a.SteelsTypeCode='" + ent.SteelsTypeCode + "'";
            }
            string sql = @"select isnull(a.BatchPlanTotal,0) as Pcl,isnull(b.HasSupplierTotal,0) as Ygh,isnull(a.BatchPlanTotal,0) as Wgh,'未供货' as GhType from TbFactoryBatchNeedPlan a
                           left join TbSupplyList b on a.BatchPlanNum=b.BatchPlanNum
                           where 1=1 " + where + @" and CONVERT(varchar(100),GETDATE(),23)<=CONVERT(varchar(100), DATEADD(DAY,4,A.InsertTime), 23) and b.SupplyCompleteTime is null and ISNULL(b.HasSupplierTotal,0)=0
                           union all
                           select isnull(a.BatchPlanTotal,0) as Pcl,isnull(b.HasSupplierTotal,0) as Ygh,isnull(a.BatchPlanTotal,0) as Wgh,'超期未供货' as GhType from TbFactoryBatchNeedPlan a
                           left join TbSupplyList b on a.BatchPlanNum=b.BatchPlanNum
                           where 1=1 " + where + @" and CONVERT(varchar(100),GETDATE(),23)>CONVERT(varchar(100), DATEADD(DAY,4,A.InsertTime), 23) and b.SupplyCompleteTime is null and ISNULL(b.HasSupplierTotal,0)=0
                           union all
                           select isnull(b.BatchPlanTotal,0) as Pcl,isnull(b.HasSupplierTotal,0) as Ygh,0 as Wgh,'按时供货' as GhType from TbFactoryBatchNeedPlan a 
                           left join TbSupplyList b on a.BatchPlanNum=b.BatchPlanNum where 1=1 " + where + @" and b.StateCode='已供货' and CONVERT(varchar(100), b.SupplyCompleteTime, 23)<=CONVERT(varchar(100), b.SupplyDate, 23) 
                           union all
                           select isnull(b.BatchPlanTotal,0) as Pcl,isnull(b.HasSupplierTotal,0) as Ygh,0 as Wgh,'超时供货' as GhType from TbFactoryBatchNeedPlan a 
                           left join TbSupplyList b on a.BatchPlanNum=b.BatchPlanNum where 1=1 " + where + @" and b.StateCode='已供货' and CONVERT(varchar(100), b.SupplyCompleteTime, 23)>CONVERT(varchar(100), b.SupplyDate, 23) 
                           union all
                           select sum(Tb.BatchPlanQuantity) Pcl,sum(Tb.HasSupplier) as Ygh,Sum(Tb.Wghl) as Wgh,Tb.GhType from (select Tb.BatchPlanNum,sud.BatchPlanQuantity,sud.HasSupplier,case when (isnull(sud.BatchPlanQuantity,0)-isnull(sud.HasSupplier,0))<0 then 0 when (isnull(sud.BatchPlanQuantity,0)-isnull(sud.HasSupplier,0))>0 then (isnull(sud.BatchPlanQuantity,0)-isnull(sud.HasSupplier,0)) end Wghl,Tb.SupplyDate,Tb.SupplyCompleteTime,Tb.GhType from (
                           select b.BatchPlanNum,b.BatchPlanTotal,b.HasSupplierTotal,b.StateCode,b.SupplyDate,b.SupplyCompleteTime,'超时部分供货' as GhType from TbFactoryBatchNeedPlan a
                           left join TbSupplyList b on a.BatchPlanNum=b.BatchPlanNum where 1=1 " + where + @" and b.StateCode='部分供货' and CONVERT(varchar(100), b.SupplyDate, 23)<= CONVERT(varchar(100), GETDATE(), 23) 
                           union all
                           select b.BatchPlanNum,b.BatchPlanTotal,b.HasSupplierTotal,b.StateCode,b.SupplyDate,b.SupplyCompleteTime,'按时部分供货' as GhType from TbFactoryBatchNeedPlan a
                           left join TbSupplyList b on a.BatchPlanNum=b.BatchPlanNum where 1=1 " + where + @" and b.StateCode='部分供货' and CONVERT(varchar(100), b.SupplyDate, 23)>CONVERT(varchar(100), GETDATE(), 23) ) Tb
                           left join TbSupplyListDetail sud on Tb.BatchPlanNum=sud.BatchPlanNum) Tb group by Tb.GhType";
            try
            {
                var dt = Db.Context.FromSql(sql).ToDataTable();
                decimal wghl = 0;//应供货时间大于当前时间(供货状态=未供货的数据+供货状态=部分供货中剩余未供货的数据)
                decimal cswghl = 0;//应供货时间小于当前时间(供货状态=未供货的数据+供货状态=部分供货中剩余未供货的数据）
                decimal asgh = 0;//供货完成时间大于应供货时间的数据+部分供货中按时供货的数据。
                decimal csgh = 0;//供货完成时间小于应供货时间的数据+部分供货中超时供货的数据。
                decimal pcl = 0;
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        pcl += Convert.ToDecimal(dt.Rows[i]["Pcl"]);
                        if (dt.Rows[i]["GhType"].ToString() == "未供货")
                        {
                            wghl += Convert.ToDecimal(dt.Rows[i]["Wgh"]);//未供货
                        }
                        else if (dt.Rows[i]["GhType"].ToString() == "超时未供货")
                        {
                            cswghl += Convert.ToDecimal(dt.Rows[i]["Wgh"]);//超时未供货
                        }
                        else if (dt.Rows[i]["GhType"].ToString() == "超时供货")
                        {
                            csgh += Convert.ToDecimal(dt.Rows[i]["Ygh"]);//超时供货
                        }
                        else if (dt.Rows[i]["GhType"].ToString() == "按时供货")
                        {
                            asgh += Convert.ToDecimal(dt.Rows[i]["Ygh"]);//按时供货
                        }
                        else if (dt.Rows[i]["GhType"].ToString() == "超时部分供货")
                        {
                            cswghl += Convert.ToDecimal(dt.Rows[i]["Wgh"]);//超时未供货
                            csgh += Convert.ToDecimal(dt.Rows[i]["Ygh"]);//超时已供货
                        }
                        else if (dt.Rows[i]["GhType"].ToString() == "按时部分供货")
                        {
                            wghl += Convert.ToDecimal(dt.Rows[i]["Wgh"]);//未供货
                            asgh += Convert.ToDecimal(dt.Rows[i]["Ygh"]);//按时已供货
                        }
                    }
                }

                decimal yghl = asgh + csgh;//已供货量
                DataTable dtGh = new DataTable();
                DataColumn dc = null;
                dc = dtGh.Columns.Add("wghl", Type.GetType("System.Decimal"));
                dc = dtGh.Columns.Add("cswghl", Type.GetType("System.Decimal"));
                dc = dtGh.Columns.Add("csgh", Type.GetType("System.Decimal"));
                dc = dtGh.Columns.Add("asgh", Type.GetType("System.Decimal"));
                dc = dtGh.Columns.Add("yghl", Type.GetType("System.Decimal"));
                dc = dtGh.Columns.Add("pcl", Type.GetType("System.Decimal"));
                DataRow newRow = dtGh.NewRow();
                newRow["wghl"] = wghl;
                newRow["cswghl"] = cswghl;
                newRow["csgh"] = csgh;
                newRow["asgh"] = asgh;
                newRow["yghl"] = yghl;
                newRow["pcl"] = pcl;
                dtGh.Rows.Add(newRow);
                return dtGh;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public DataTable Img3(FPiCiXQPlan ent)
        {
            string where = "";
            //string OrgType = OperatorProvider.Provider.CurrentUser.OrgType;
            //string ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
            //string ProcessFactoryCode = OperatorProvider.Provider.CurrentUser.ProcessFactoryCode;
            //if (OrgType!="1")
            //{
            //    where += " and s.ProjectId=@ProjectId";
            //}
            //if (!string.IsNullOrWhiteSpace(RebarType))
            //{
            //    where += " and s.SteelsTypeCode=@RebarType";
            //}
            //where += " and s.ProcessFactoryCode=@ProcessFactoryCode";
            if (!string.IsNullOrWhiteSpace(ent.ProjectId))
            {
                where += " and s.ProjectId='" + ent.ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(ent.ProcessFactoryCode))
            {
                where += " and s.ProcessFactoryCode='" + ent.ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(ent.SiteCode))
            {
                List<string> WorkAreaList = orderProLogic.GetCompanyWorkAreaOrSiteList(ent.SiteCode, 4);//工区
                string workAreaStr = string.Join("','", WorkAreaList);
                where += " and s.WorkAreaCode in('" + workAreaStr + "')";
            }
            if (!string.IsNullOrWhiteSpace(ent.SteelsTypeCode))
            {
                where += " and s.SteelsTypeCode='" + ent.SteelsTypeCode + "'";
            }
            string sql = @"select Tb.GhType,sum(Tb.GhWeight) as GhWeight,sum(tb.GhCount) as GhCount from (select TbGhType.GhType,isnull(TbGhInfo.HasSupplier,0) as GhWeight,isnull(TbGhInfo.GhCount,0) as GhCount from (select '供货' as GhType
                            union all
                            select '验收不合格' as GhType
                            union all
                            select '检测不合格' as GhType) TbGhType
                            left join(
                            --供货次数、总量
                            select isnull(sum(Tb.HasSupplier),0) as HasSupplier,isnull(sum(Tb.GhCount),0) as GhCount,'供货' as GhType from (select a.HasSupplier,isnull(b.GhCount,0) as GhCount from (select a.BatchPlanNum,sum(a.HasSupplier) as HasSupplier from TbSupplyListDetail a 
                            left join TbSupplyList s on a.BatchPlanNum=s.BatchPlanNum where 1=1 " + where + @"
                            GROUP BY a.BatchPlanNum) a
                            left join(select Tb.BatchPlanNum,count(1) as GhCount from (select BatchPlanNum,ThisTime from TbSupplyListDetailHistory
                            group by BatchPlanNum,ThisTime) Tb group by Tb.BatchPlanNum) b on a.BatchPlanNum=b.BatchPlanNum) Tb 
                            union all
                            --验收不合格次数、验收不合格重量
                            select sum(TbSum.NoPass) as NoPass,sum(TbSum.YsNoPassCount) as YsNoPassCount,'验收不合格' as GhType from (select count(1) as YsNoPassCount,sum(Tb.NoPass) as NoPass from (select b.BatchPlanCode,c.ThisTime,sum(a.NoPass) as NoPass from TbInOrderItem a
                            left join TbInOrder b on a.InOrderCode=b.InOrderCode
                            left join TbSupplyList s on b.BatchPlanCode=s.BatchPlanNum
                            left join TbSupplyListDetailHistory c on a.BatchPlanItemNewCode=c.BatchPlanItemNewCode
                            where 1=1 " + where + @" and a.BatchPlanItemNewCode is not null and a.NoPass>0 
                            group by b.BatchPlanCode,c.ThisTime) Tb group by Tb.BatchPlanCode) TbSum
                            union all
                            --检测不合格次数、重量
                            select isnull(sum(Tb.JcNoPass),0) as JcNoPass,count(1) as JcNoPassCount,'检测不合格' as GhType from (select b.BatchPlanCode,d.ThisTime,Isnull(sum(c.PassCount),0) as JcNoPass from TbSampleOrderItem a
                            left join TbSampleOrder e on a.SampleOrderCode=e.SampleOrderCode
                            left join TbInOrderItem c on a.InOrderItemId=c.ID
                            left join TbSupplyListDetailHistory d on c.BatchPlanItemNewCode=d.BatchPlanItemNewCode
                            left join TbInOrder b on c.InOrderCode=b.InOrderCode
                            left join TbSupplyList s on b.BatchPlanCode=s.BatchPlanNum
                            where 1=1 " + where + @"  and a.ChackState=2 and c.BatchPlanItemNewCode is not null
                            group by b.BatchPlanCode,d.ThisTime) Tb group by Tb.BatchPlanCode) TbGhInfo on TbGhType.GhType=TbGhInfo.GhType) Tb group by Tb.GhType";

            var dt = Db.Context.FromSql(sql).ToDataTable();
            return dt;
        }

        #endregion

        #region 判断当前登录是否是物贸、分部工区物机部人员

        public DataTable GetUserIsWmOrFbGqWjb()
        {
            string UserId = OperatorProvider.Provider.CurrentUser.UserId;
            string ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
            string sql = @"select * from (select '型钢' as RebarType,UserCode from TbUserRole where DeptId in(select DepartmentId from TbDepartment where DepartmentName='物机部' and (DepartmentType=3 or DepartmentType=4) and (isnull(@ProjectId,'')='' or DepartmentProjectId=@ProjectId))
and Flag=0 group by UserCode
union all
select '建筑钢筋' as RebarType,UserCode  from TbUserRole where RoleCode in(select RoleId from TbRole  where RoleName='物贸') and (isnull(@ProjectId,'')='' or ProjectId=@ProjectId) and Flag=0
group by UserCode) Tb where Tb.UserCode=@UserId";
            var dt = Db.Context.FromSql(sql)
               .AddInParameter("@UserId", DbType.String, UserId)
               .AddInParameter("@ProjectId", DbType.String, ProjectId).ToDataTable();
            return dt;
        }

        #endregion

        #region App获取详情信息

        public DataTable AppDetail(int ID)
        {
            string sql = @"select b.BatchPlanItemId,a.BatchPlanNum,c.CompanyFullName as WorkAreaName,d.CompanyFullName as BranchName,b.RawMaterialNum,b.MaterialName,b.Standard,b.BatchPlanQuantity,b.HasSupplier from TbSupplyList a
left join TbSupplyListDetail b on a.BatchPlanNum=b.BatchPlanNum
left join TbCompany c on a.WorkAreaCode=c.CompanyCode
left join TbCompany d on a.BranchCode=d.CompanyCode
where a.ID=" + ID + "";
            DataTable ret = Db.Context.FromSql(sql).ToDataTable();
            return ret;
        }
        public DataTable AppDetailItem(int BatchPlanItemId)
        {
            string sql = @" select a.ThisTime,a.ThisTimeCount,case when c.ChackState=2 then 0 else b.PassCount end PassCount ,b.NoPass,case when c.ChackState=2 then b.PassCount else 0 end Unqualified,c.Enclosure,convert(decimal(18, 2),ISNULL(a.ThisTimeCount,0)/isnull(d.BatchPlanQuantity,0)*100) as GhBfb from TbSupplyListDetailHistory a
 left join TbInOrderItem b on a.BatchPlanItemId=b.BatchPlanItemId and a.BatchPlanItemNewCode=b.BatchPlanItemNewCode
 left join TbSampleOrderItem c on c.InOrderItemId=b.ID
 left join TbSupplyListDetail d on a.BatchPlanItemId=d.BatchPlanItemId
 where a.BatchPlanItemId="+BatchPlanItemId+"";
            DataTable ret = Db.Context.FromSql(sql).ToDataTable();
            return ret;
        } 
        #endregion
    }
}
