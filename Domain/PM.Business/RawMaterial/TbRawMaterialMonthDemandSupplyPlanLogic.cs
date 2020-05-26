using Dos.ORM;
using PM.Business.Production;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using PM.DataEntity.Production.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Business.RawMaterial
{
    /// <summary>
    /// 原材料月度需求补充计划
    /// </summary>
    public class TbRawMaterialMonthDemandSupplyPlanLogic
    {
        string ProjectId = GetProjectId();

        public static string GetProjectId()
        {
            string ProjectId = "";
            bool Flog = OperatorProvider.Provider.CurrentUser == null;
            if (!Flog)
            {
                ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
            }
            return ProjectId;
        }
        private readonly TbWorkOrderLogic _workOrderLogic = new TbWorkOrderLogic();

        #region 新增数据

        /// <summary>
        /// 新增数据
        /// </summary>
        public AjaxResult Insert(TbRawMaterialMonthDemandSupplyPlan model, List<TbRawMaterialMonthDemandSupplyPlanDetail> items)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            var anyRetType = AnyInfoType(model.DemandPlanCode, model.ID);
            if (anyRetType.state.ToString() != ResultType.success.ToString())
                return anyRetType;
            model.InsertUserCode = OperatorProvider.Provider.CurrentUser.UserCode;
            //var RawDemandPlan = Repository<TbRawMaterialMonthDemandPlan>.First(p => p.DemandPlanCode==model.DemandPlanCode);
            //if (RawDemandPlan!=null)
            //{
            //    if (RawDemandPlan.RebarType == "SectionSteel")
            //    {
            //        model.Examinestatus = "审核完成";
            //    }
            //    else
            //    {
            model.Examinestatus = "未发起";
            //    }
            //}
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //添加信息
                    var id = Repository<TbRawMaterialMonthDemandSupplyPlan>.Insert(trans, model);
                    //添加明细信息
                    Repository<TbRawMaterialMonthDemandSupplyPlanDetail>.Insert(trans, items);
                    trans.Commit();
                    return AjaxResult.Success(id);
                }
            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.ToString());
            }
        }

        #endregion

        #region 修改数据

        /// <summary>
        /// 修改数据
        /// </summary>
        public AjaxResult Update(TbRawMaterialMonthDemandSupplyPlan model, List<TbRawMaterialMonthDemandSupplyPlanDetail> items)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            var anyRet = AnyInfo(model.ID);
            if (anyRet.state.ToString() != ResultType.success.ToString())
                return anyRet;
            var anyRetType = AnyInfoType(model.DemandPlanCode, model.ID);
            if (anyRetType.state.ToString() != ResultType.success.ToString())
                return anyRetType;
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())//使用事务
                {
                    //修改信息
                    Repository<TbRawMaterialMonthDemandSupplyPlan>.Update(trans, model, p => p.ID == model.ID);
                    if (items.Count > 0)
                    {
                        //删除历史明细信息
                        Repository<TbRawMaterialMonthDemandSupplyPlanDetail>.Delete(trans, p => p.SupplyPlanCode == model.SupplyPlanCode);
                        //添加明细信息
                        Repository<TbRawMaterialMonthDemandSupplyPlanDetail>.Insert(trans, items);
                    }
                    trans.Commit();//提交事务

                    return AjaxResult.Success(model.ID);
                }
            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.ToString());
            }
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
                    //删除信息
                    var count = Repository<TbRawMaterialMonthDemandSupplyPlan>.Delete(trans, p => p.ID == keyValue);
                    //删除明细信息
                    Repository<TbRawMaterialMonthDemandSupplyPlanDetail>.Delete(trans, p => p.SupplyPlanCode == ((TbRawMaterialMonthDemandSupplyPlan)anyRet.data).SupplyPlanCode);
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

        #region 获取数据

        /// <summary>
        /// 获取编辑数据
        /// </summary>
        /// <param name="dataID">数据Id</param>
        /// <returns></returns>
        public Tuple<DataTable, DataTable> FindEntity(int dataID,string dataStr)
        {
            var ret = Db.Context.From<TbRawMaterialMonthDemandSupplyPlan>()
            .Select(
                    TbRawMaterialMonthDemandSupplyPlan._.All
                    , TbCompany._.CompanyFullName.As("BranchName")
                    , TbSupplier._.SupplierName
                    , TbUser._.UserName
                    , TbRawMaterialMonthDemandPlan._.RebarType)
                  .LeftJoin<TbCompany>((a, c) => a.BranchCode == c.CompanyCode)
                  .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                  .Where(TbCompany._.CompanyCode == TbRawMaterialMonthDemandSupplyPlan._.SiteCode), "SiteName")
                  .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                  .Where(TbCompany._.CompanyCode == TbRawMaterialMonthDemandSupplyPlan._.WorkAreaCode), "WorkAreaName")
                  .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                  .Where(TbCompany._.CompanyCode == TbRawMaterialMonthDemandSupplyPlan._.ProcessFactoryCode), "ProcessFactoryName")
                  .LeftJoin<TbSupplier>((a, c) => a.SupplierCode == c.SupplierCode)
                  .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                  .LeftJoin<TbRawMaterialMonthDemandPlan>((a, c) => a.DemandPlanCode == c.DemandPlanCode)
                    .Where(p => p.ID == dataID||p.DemandPlanCode==dataStr).ToDataTable();
            //查找明细信息
            var items = Db.Context.From<TbRawMaterialMonthDemandSupplyPlanDetail>().Select(
               TbRawMaterialMonthDemandSupplyPlanDetail._.All,
               TbSysDictionaryData._.DictionaryText.As("MeasurementUnitText"))
           .LeftJoin<TbSysDictionaryData>((a, c) => a.MeasurementUnit == c.DictionaryCode)
           .Where(p => p.SupplyPlanCode == ret.Rows[0]["SupplyPlanCode"].ToString()).ToDataTable();
            return new Tuple<DataTable, DataTable>(ret, items);
        }
        /// <summary>
        /// 获取编辑数据
        /// </summary>
        /// <param name="dataID">数据Id</param>
        /// <returns></returns>
        public DataTable GetDemSupplyPlanData(string SupplyPlanCode)
        {
            var ret = Db.Context.From<TbRawMaterialMonthDemandSupplyPlan>()
            .Select(
                    TbRawMaterialMonthDemandSupplyPlan._.All
                    , TbCompany._.CompanyFullName.As("BranchName")
                    , TbSupplier._.SupplierName
                    , TbUser._.UserName
                    , TbRawMaterialMonthDemandPlan._.RebarType)
                  .LeftJoin<TbCompany>((a, c) => a.BranchCode == c.CompanyCode)
                  .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                  .Where(TbCompany._.CompanyCode == TbRawMaterialMonthDemandSupplyPlan._.SiteCode), "SiteName")
                  .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                  .Where(TbCompany._.CompanyCode == TbRawMaterialMonthDemandSupplyPlan._.WorkAreaCode), "WorkAreaName")
                  .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                  .Where(TbCompany._.CompanyCode == TbRawMaterialMonthDemandSupplyPlan._.ProcessFactoryCode), "ProcessFactoryName")
                  .LeftJoin<TbSupplier>((a, c) => a.SupplierCode == c.SupplierCode)
                  .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                  .LeftJoin<TbRawMaterialMonthDemandPlan>((a, c) => a.DemandPlanCode == c.DemandPlanCode)
                    .Where(p => p.SupplyPlanCode == SupplyPlanCode).ToDataTable();
            return ret;
        }

        /// <summary>
        /// 获取数据列表(分页)
        /// </summary>
        public PageModel GetDataListForPage(RawMonthDemSupplyPlanRequest request)
        {
            //组装查询语句
            #region 模糊搜索条件

            var where = new Where<TbRawMaterialMonthDemandSupplyPlan>();
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where.And(p => p.ProcessFactoryCode == request.ProcessFactoryCode);
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);//站点
                List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 4);//工区
                where.And(p => p.SiteCode.In(SiteList) || p.WorkAreaCode.In(WorkAreaList));
            }
            if (!string.IsNullOrEmpty(request.ProjectId))
            {
                where.And(p => p.ProjectId == request.ProjectId);
            }
            if (!string.IsNullOrWhiteSpace(request.DemandPlanCode))
            {
                where.And(p => p.DemandPlanCode.StartsWith(request.DemandPlanCode));
            }
            if (!string.IsNullOrWhiteSpace(request.SupplyPlanCode))
            {
                where.And(p => p.SupplyPlanCode.StartsWith(request.SupplyPlanCode));
            }
            if (!string.IsNullOrWhiteSpace(request.RebarType))
            {
                where.And(p => p.RebarType==request.RebarType);
            }
            if (request.HistoryMonth.HasValue)
            {
                var historyMonth = new WhereClip("YEAR(TbRawMaterialMonthDemandSupplyPlan.InsertTime)=" + request.HistoryMonth.Value.Year + " and MONTH(TbRawMaterialMonthDemandSupplyPlan.InsertTime)=" + request.HistoryMonth.Value.Month);
                where.And(historyMonth);
            }
            #endregion

            try
            {
                var ret = Db.Context.From<TbRawMaterialMonthDemandSupplyPlan>()
              .Select(
                      TbRawMaterialMonthDemandSupplyPlan._.All
                      , TbCompany._.CompanyFullName.As("BranchName")
                      , TbSysDictionaryData._.DictionaryText.As("RebarTypeNew")
                      , TbSupplier._.SupplierName
                      , TbUser._.UserName)
                    .LeftJoin<TbCompany>((a, c) => a.BranchCode == c.CompanyCode)
                    .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                    .Where(TbCompany._.CompanyCode == TbRawMaterialMonthDemandSupplyPlan._.SiteCode), "SiteName")
                    .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                    .Where(TbCompany._.CompanyCode == TbRawMaterialMonthDemandSupplyPlan._.ProcessFactoryCode), "ProcessFactoryName")
                    .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                    .Where(TbCompany._.CompanyCode == TbRawMaterialMonthDemandSupplyPlan._.WorkAreaCode), "WorkAreaName")
                    .LeftJoin<TbSysDictionaryData>((a, c) => a.RebarType == c.DictionaryCode && c.FDictionaryCode == "RebarType")
                    .LeftJoin<TbSupplier>((a, c) => a.SupplierCode == c.SupplierCode)
                    .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                      .Where(where).OrderByDescending(d => d.ID).ToPageList(request.rows, request.page);
                return ret;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public class CompanyModel
        {
            /// <summary>
            /// 用户编号
            /// </summary>
            public List<string> CompanyCode { get; set; }
        }
        /// <summary>
        /// 获取导出数据列表
        /// </summary>
        public DataTable GetExportList(RawMonthDemSupplyPlanRequest request) 
        {
            //组装查询语句
            #region 模糊搜索条件

            var where = new Where<TbRawMaterialMonthDemandSupplyPlan>();
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where.And(p => p.ProcessFactoryCode == request.ProcessFactoryCode);
            }
            //if (!string.IsNullOrWhiteSpace(request.SiteCode))
            //{
            //    List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 4);//工区
            //    where.And(p => p.WorkAreaCode.In(WorkAreaList));
            //}
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);//站点
                List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 4);//工区
                where.And(p => p.SiteCode.In(SiteList) || p.WorkAreaCode.In(WorkAreaList));
            }
            if (!string.IsNullOrEmpty(request.ProjectId))
            {
                where.And(p => p.ProjectId == request.ProjectId);
            }
            if (!string.IsNullOrWhiteSpace(request.DemandPlanCode))
            {
                where.And(p => p.DemandPlanCode.StartsWith(request.DemandPlanCode));
            }
            if (!string.IsNullOrWhiteSpace(request.SupplyPlanCode))
            {
                where.And(p => p.SupplyPlanCode.StartsWith(request.SupplyPlanCode));
            }
            if (request.HistoryMonth.HasValue)
            {
                var historyMonth = new WhereClip("YEAR(TbRawMaterialMonthDemandSupplyPlan.InsertTime)=" + request.HistoryMonth.Value.Year + " and MONTH(TbRawMaterialMonthDemandSupplyPlan.InsertTime)=" + request.HistoryMonth.Value.Month);
                where.And(historyMonth);
            }
            #endregion

            try
            {
                var ret = Db.Context.From<TbRawMaterialMonthDemandSupplyPlan>()
              .Select(
                      TbRawMaterialMonthDemandSupplyPlan._.All
                      , TbCompany._.CompanyFullName.As("BranchName")
                      , TbSysDictionaryData._.DictionaryText.As("RebarTypeNew")
                      , TbSupplier._.SupplierName
                      , TbUser._.UserName)
                    .LeftJoin<TbCompany>((a, c) => a.BranchCode == c.CompanyCode)
                    .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                    .Where(TbCompany._.CompanyCode == TbRawMaterialMonthDemandSupplyPlan._.SiteCode), "SiteName")
                    .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                    .Where(TbCompany._.CompanyCode == TbRawMaterialMonthDemandSupplyPlan._.ProcessFactoryCode), "ProcessFactoryName")
                    .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                    .Where(TbCompany._.CompanyCode == TbRawMaterialMonthDemandSupplyPlan._.WorkAreaCode), "WorkAreaName")
                    .LeftJoin<TbSysDictionaryData>((a, c) => a.RebarType == c.DictionaryCode && c.FDictionaryCode == "RebarType")
                    .LeftJoin<TbSupplier>((a, c) => a.SupplierCode == c.SupplierCode)
                    .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                      .Where(where).OrderByDescending(d => d.ID).ToDataTable();
                return ret;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region 判断
        /// <summary>
        /// 判断信息是否可操作
        /// </summary>
        /// <returns></returns>
        public AjaxResult AnyInfo(int keyValue)
        {
            var monthDemandPlan = Repository<TbRawMaterialMonthDemandSupplyPlan>.First(p => p.ID == keyValue);
            if (monthDemandPlan == null)
                return AjaxResult.Warning("信息不存在");
            if (monthDemandPlan.Examinestatus != "未发起" && monthDemandPlan.Examinestatus != "已退回")
                return AjaxResult.Warning("信息正在审核中或已审核完成,不能进行此操作");

            return AjaxResult.Success(monthDemandPlan);
        }
        public AjaxResult AnyInfoType(string DemandPlanCode, int keyValue)
        {
            try
            {
                string sql = @"select COUNT(1) as Count1 from TbRawMaterialMonthDemandSupplyPlan where DemandPlanCode=@DemandPlanCode and ID!=@keyValue";
                var model = Db.Context.FromSql(sql).AddInParameter("@DemandPlanCode", DbType.String, DemandPlanCode).AddInParameter("@keyValue", DbType.String, keyValue).ToDataTable();

                if (Convert.ToInt32(model.Rows[0]["Count1"]) > 0)
                    return AjaxResult.Warning("该站点在当月已经录入月度需求补充计划，请不要重复录入");
                return AjaxResult.Success();
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region 获取可以选择的原需求计划

        /// <summary>
        /// 获取可以选择的原需求计划(分页)
        /// </summary>
        public DataTable GetRawMaterialMonthDemandPlanList(RawMonthDemPlanRequest request)
        {
            try
            {
                string where = "";
                if (!string.IsNullOrWhiteSpace(request.keyword))
                {
                    where += " and DemandPlanCode like '%" + request.keyword + "%' or RebarType='" + request.keyword + "' ";
                }
                if (!string.IsNullOrWhiteSpace(request.ProjectId))
                {
                    where += " and mdp.ProjectId='" + request.ProjectId + "' ";
                }
                if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
                {
                    where += " and mdp.ProcessFactoryCode='" + request.ProcessFactoryCode + "' ";
                }
                if (!string.IsNullOrWhiteSpace(request.SiteCode))
                {
                    List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);//站点
                    List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 4);//工区
                    string siteStr = string.Join("','", SiteList);
                    string workAreaStr = string.Join("','", WorkAreaList);
                    where += " and (mdp.SiteCode in('" + siteStr + "') or mdp.WorkAreaCode in('" + workAreaStr + "'))";
                }
                string sql = @"select mdp.DemandPlanCode,mdp.RebarType,sd.DictionaryText as RebarTypeName,mdp.BranchCode,c1.CompanyFullName as BranchName,mdp.WorkAreaCode,c4.CompanyFullName as WorkAreaName,mdp.SiteCode,c2.CompanyFullName as SiteName,mdp.ProcessFactoryCode,c3.CompanyFullName as ProcessFactoryName,mdp.SupplierCode,sup.SupplierName,mdp.InsertUserCode,mdp.DeliveryAdd,mdp.ProjectId from TbRawMaterialMonthDemandPlan mdp
left join TbCompany c1 on mdp.BranchCode=c1.CompanyCode
left join TbCompany c2 on mdp.SiteCode=c2.CompanyCode
left join TbCompany c3 on mdp.ProcessFactoryCode=c3.CompanyCode
left join TbCompany c4 on mdp.WorkAreaCode=c4.CompanyCode
left join TbSupplier sup on mdp.SupplierCode=sup.SupplierCode
left join TbUser us on mdp.InsertUserCode=us.UserCode
left join TbSysDictionaryData sd on mdp.RebarType=sd.DictionaryCode and sd.FDictionaryCode='RebarType'
where DATEPART(m,mdp.InsertTime)=DATEPART(m,GETDATE()) and mdp.DemandPlanCode not in (select DemandPlanCode from TbRawMaterialMonthDemandSupplyPlan) and mdp.Examinestatus!='未发起' ";
                var model = Db.Context.FromSql(sql + where).ToDataTable();
                return model;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 获取原材料月度需求计划明细信息
        /// </summary>
        public DataTable GetRawMonthDemandPlanDetail(string DemandPlanCode)
        {
            try
            {
                //查找明细信息
                var items = Db.Context.From<TbRawMaterialMonthDemandPlanDetail>().Select(
                   TbRawMaterialMonthDemandPlanDetail._.All,
                   TbSysDictionaryData._.DictionaryText.As("MeasurementUnitText"))
               .LeftJoin<TbSysDictionaryData>((a, c) => a.MeasurementUnit == c.DictionaryCode)
               .Where(p => p.DemandPlanCode == DemandPlanCode).ToDataTable();
                return items;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
    }
}
