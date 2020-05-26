using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PM.Common.Extension.ListEx;
using System.Data;
using Dos.ORM;
using Dos.Common;
using PM.Business.Production;
using PM.Common.Helper;

namespace PM.Business.RawMaterial
{
    /// <summary>
    /// 原材料初期库存
    /// </summary>
    public class RawMaterialStockLogic
    {
        private readonly TbWorkOrderLogic _workOrderLogic = new TbWorkOrderLogic();

        #region 新增数据

        /// <summary>
        /// 新增数据
        /// </summary>
        public AjaxResult Insert(List<TbRawMaterialStockRecord> model)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //添加信息
                    Repository<TbRawMaterialStockRecord>.Insert(trans, model);
                    trans.Commit();
                    return AjaxResult.Success();
                }
            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 新增数据(导入)
        /// </summary>
        public AjaxResult Input(List<TbRawMaterialStockRecord> model, StringBuilder errorMsg)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            //仓库
            var storageNames = model.Select(p => p.StorageCode).ToList();
            var storages = Repository<TbStorage>.Query(p => p.StorageName.In(storageNames)).ToList();
            //分部
            var branchNames = model.Select(p => p.BranchCode).Distinct().ToList();
            var branchs = Db.Context.From<TbCompany>().Select(
                TbCompany._.CompanyFullName,
                TbCompany._.CompanyCode)
                .LeftJoin<TbProjectCompany>((a, c) => a.CompanyCode == c.CompanyCode)
                .Where(TbProjectCompany._.ProjectId == model[0].ProjectId && TbCompany._.CompanyFullName.In(branchNames))
                .ToList();
            //工区
            var workAreaNames = model.Select(p => p.WorkAreaCode).Distinct().ToList();
            var workAreas = Repository<TbCompany>.Query(p => p.CompanyFullName.In(workAreaNames)).ToList();
            //站点
            var siteNames = model.Select(p => p.SiteCode).Distinct().ToList();
            var sites = Repository<TbCompany>.Query(p => p.CompanyFullName.In(siteNames)).ToList();
            //原材料
            var specifications = model.Select(p => p.SpecificationModel).Distinct().ToList();
            var rawMaterials = Repository<TbRawMaterialArchives>.Query(p => p.SpecificationModel.In(specifications)).ToList();
            var userCode = OperatorProvider.Provider.CurrentUser.UserCode;
            foreach (var item in model)
            {
                item.InsertUserCode = userCode;
                item.InsertTime = DateTime.Now;
                //判断原材料是否存在
                var rawMaterial = rawMaterials.FirstOrDefault(p => p.SpecificationModel == item.SpecificationModel);
                if (rawMaterial == null)
                {
                    errorMsg.AppendFormat("第{0}行规格【{1}】信息不存在！", item.IndexNum, item.SpecificationModel);
                    continue;
                }
                item.MaterialName = rawMaterial.MaterialName;
                item.MaterialCode = rawMaterial.MaterialCode;
                item.SpecificationModel = rawMaterial.SpecificationModel;
                item.MeasurementUnit = rawMaterial.MeasurementUnit;
                item.RebarType = rawMaterial.RebarType;
                //判断仓库是否存在
                var storage = storages.FirstOrDefault(p => p.StorageName == item.StorageCode);
                if (storage == null)
                {
                    errorMsg.AppendFormat("第{0}行仓库【{1}】信息不存在！", item.IndexNum, item.StorageCode);
                    continue;
                }
                item.StorageCode = storage.StorageCode;
                //判断分部是否存在
                var branch = branchs.FirstOrDefault(p => p.CompanyFullName == item.BranchCode);
                if (branch == null)
                {
                    errorMsg.AppendFormat("第{0}行分部【{1}】信息不存在！", item.IndexNum, item.BranchCode);
                    continue;
                }
                //判断工区是否存在
                var workArea = workAreas.FirstOrDefault(p => p.CompanyFullName == item.WorkAreaCode && p.ParentCompanyCode == branch.CompanyCode);
                if (workArea == null)
                {
                    errorMsg.AppendFormat("第{0}行工区【{1}】信息不存在！", item.IndexNum, item.WorkAreaCode);
                    continue;
                }
                //判断站点是否存在
                if (sites.Any())
                {
                    var site = sites.Where(p => p.CompanyFullName == item.SiteCode).ToList();
                    if (site.Any())
                    {
                        //判断工区站点是否匹配
                        var siteret = site.FirstOrDefault(p => p.CompanyFullName == item.SiteCode && p.ParentCompanyCode == workArea.CompanyCode);
                        if (siteret == null)
                        {
                            errorMsg.AppendFormat("第{0}行工区【{1}】信息和站点【{2}】信息不匹配！", item.IndexNum, item.WorkAreaCode, item.SiteCode);
                            continue;
                        }
                        item.SiteCode = siteret.CompanyCode;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(item.SiteCode))
                        {
                            errorMsg.AppendFormat("第{0}行站点【{1}】信息不存在！", item.IndexNum, item.SiteCode);
                            continue;
                        }
                        item.SiteCode = "";
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(item.SiteCode))
                    {
                        errorMsg.AppendFormat("第{0}行站点【{1}】信息不存在！", item.IndexNum, item.SiteCode);
                        continue;
                    }
                    item.SiteCode = "";
                }
                item.WorkAreaCode = workArea.CompanyCode;
            }
            //查询库存记录
            var ret = GetStockRecord(model);
            var addList = ret.Item1;
            var updateList = ret.Item2;
            if (!addList.Any() && !updateList.Any())
                return AjaxResult.Error(errorMsg.ToString());
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //添加信息
                    if (addList.Any())
                        Repository<TbRawMaterialStockRecord>.Insert(trans, addList);
                    //修改信息
                    if (updateList.Any())
                        Repository<TbRawMaterialStockRecord>.Update(trans, updateList);
                    trans.Commit();//提交事务
                    return AjaxResult.Success(errorMsg.ToString());
                }
            }
            catch (Exception ex)
            {
                return AjaxResult.Error("操作失败");
            }
        }

        #endregion

        #region 修改数据

        /// <summary>
        /// 修改数据
        /// </summary>
        public AjaxResult Update(List<TbRawMaterialStockRecord> model)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //修改信息
                    Repository<TbRawMaterialStockRecord>.Update(trans, model);
                    trans.Commit();
                    return AjaxResult.Success();
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

            var anyRet = AnyInfo(keyValue);
            if (anyRet.state != ResultType.success.ToString())
                return anyRet;
            try
            {
                var count = Repository<TbRawMaterialStockRecord>.Delete(p => p.ID == keyValue);
                if (count > 0)
                    return AjaxResult.Success();
                return AjaxResult.Error("操作失败");
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
        /// <param name="keyValue">数据Id</param>
        /// <returns></returns>
        public DataTable FindEntity(int keyValue)
        {
            var ret = Db.Context.From<TbRawMaterialStockRecord>()
                   .Select(
                       TbRawMaterialStockRecord._.ID
                       , TbRawMaterialStockRecord._.Enclosure
                       , TbRawMaterialStockRecord._.ProjectId
                       , TbRawMaterialStockRecord._.MaterialCode
                       , TbRawMaterialStockRecord._.MaterialName
                       , TbRawMaterialStockRecord._.Factory
                       , TbRawMaterialStockRecord._.BatchNumber
                       , TbRawMaterialStockRecord._.Count
                       , TbRawMaterialStockRecord._.UseCount.As("UseCountS")
                       , TbRawMaterialStockRecord._.StorageCode
                       , TbRawMaterialStockRecord._.WorkAreaCode
                       , TbRawMaterialStockRecord._.SiteCode
                       , TbRawMaterialStockRecord._.InsertUserCode
                       , TbRawMaterialStockRecord._.InsertTime
                       , TbUser._.UserName
                       , TbStorage._.StorageName
                       , TbCompany._.CompanyFullName.As("SiteName")
                       , TbStorage._.StorageName
                       , TbStorage._.StorageName
                       , TbRawMaterialArchives._.SpecificationModel
                       , TbRawMaterialArchives._.MeasurementUnit
                       , TbRawMaterialArchives._.RebarType)
                    .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                    .Where(TbCompany._.CompanyCode == TbRawMaterialStockRecord._.WorkAreaCode), "WorkAreaName")
                    .AddSelect(Db.Context.From<TbSysDictionaryData>().Select(p => p.DictionaryText)
                    .Where(TbSysDictionaryData._.DictionaryCode == TbRawMaterialArchives._.MeasurementUnit), "MeasurementUnitText")
                    .AddSelect(Db.Context.From<TbSysDictionaryData>().Select(p => p.DictionaryText)
                    .Where(TbSysDictionaryData._.DictionaryCode == TbRawMaterialArchives._.RebarType), "RebarTypeName")
                    .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                    .LeftJoin<TbStorage>((a, c) => a.StorageCode == c.StorageCode)
                    .LeftJoin<TbCompany>((a, c) => a.SiteCode == c.CompanyCode)
                    .LeftJoin<TbRawMaterialArchives>((a, c) => a.MaterialCode == c.MaterialCode)
                    .Where(p => p.ID == keyValue).ToDataTable();
            return ret;
        }

        /// <summary>
        /// 获取数据列表(分页)
        /// </summary>
        public PageModel GetDataListForPage(RawMaterialStockRequest request)
        {

            #region 模糊搜索条件
            string where = " where 1=1 ";
            if (!string.IsNullOrWhiteSpace(request.MaterialName))
            {
                where += (" and b.MaterialName like '%" + request.MaterialName + "%'");
            }

            if (!string.IsNullOrWhiteSpace(request.SpecificationModel))
            {
                where += (" and b.SpecificationModel like '%" + request.SpecificationModel + "%'");
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);//站点
                List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 4);//工区
                string siteStr = string.Join("','", SiteList);
                string workAreaStr = string.Join("','", WorkAreaList);
                where += (" and (rms.SiteCode in('" + siteStr + "') or rms.WorkAreaCode in('" + workAreaStr + "'))");
            }
            if (!string.IsNullOrEmpty(request.ProjectId))
                where += (" and rms.ProjectId='" + request.ProjectId + "'");
            if (!string.IsNullOrEmpty(request.ProcessFactoryCode))
                where += (" and c.ProcessFactoryCode='" + request.ProcessFactoryCode + "'");
            if (!string.IsNullOrEmpty(request.RebarType))
            {
                where += (" and b.RebarType='" + request.RebarType + "'");
            }
            if (!string.IsNullOrWhiteSpace(request.IsQkl))
            {
                if (request.IsQkl=="是")
                {
                    where += "and -1*((isnull(rms.LockCount,0)+isnull(rms.UseCount,0))-isnull(tbwork.WeightSmallPlan,0))>0";
                }
                else
                {
                    where += "and (isnull(rms.LockCount,0)+isnull(rms.UseCount,0))-isnull(tbwork.WeightSmallPlan,0)>=0";
                }
            }
            #endregion
            try
            {
                var sql = @"select 
                                rms.*,
                                b.MaterialName,
                                b.SpecificationModel,
								c.ProcessFactoryCode,
                                c.StorageName,
                                cp1.CompanyFullName as SiteName,
                                cp2.CompanyFullName as WorkAreaName,
                                cp3.CompanyFullName as BranchName,
                                sdd2.DictionaryText as RebarTypeText,
                                sdd3.DictionaryText as MeasurementUnitText,
								case when (isnull(rms.LockCount,0)+isnull(rms.UseCount,0))-isnull(tbwork.WeightSmallPlan,0)>=0 then 0 else -1*((isnull(rms.LockCount,0)+isnull(rms.UseCount,0))-isnull(tbwork.WeightSmallPlan,0)) end as QKl
                                from (
                                select 
                                MaterialCode,
                                ProjectId,
                                SiteCode,
                                StorageCode,
                                WorkAreaCode,
                                SUM(COUNT) as COUNT,
                                SUM(LockCount) as LockCount,
                                SUM(UseCount) as UseCount
                                FROM TbRawMaterialStockRecord
                                group by MaterialCode,SiteCode,WorkAreaCode,StorageCode,ProjectId
                                )as rms
                            left join TbCompany cp1 on rms.SiteCode=cp1.CompanyCode
                            left join TbCompany cp2 on rms.WorkAreaCode=cp2.CompanyCode
                            left join TbCompany cp3 on cp2.ParentCompanyCode=cp3.CompanyCode
                            left join TbStorage c on c.StorageCode=rms.StorageCode
                            left join TbRawMaterialArchives b on b.MaterialCode=rms.MaterialCode
                            left join TbSysDictionaryData sdd2 on b.RebarType=sdd2.DictionaryCode
                            left join TbSysDictionaryData sdd3 on b.MeasurementUnit=sdd3.DictionaryCode
							left join(select od.MaterialCode,od.MaterialName,od.SpecificationModel,isnull(sum(od.WeightSmallPlan),0) as WeightSmallPlan,wo.ProjectId,cp1.ParentCompanyCode as WorkAreaCode,cp2.ParentCompanyCode as BranchCode,wo.ProcessFactoryCode from TbWorkOrder wo 
							left join TbWorkOrderDetail od on wo.OrderCode=od.OrderCode
							left join TbCompany cp1 on wo.SiteCode=cp1.CompanyCode
							left join TbCompany cp2 on cp1.ParentCompanyCode=cp2.CompanyCode
							where wo.Examinestatus='审核完成' and wo.ProcessingState='Received'
							group by od.MaterialCode,od.MaterialName,od.SpecificationModel,wo.ProjectId,cp1.ParentCompanyCode,cp2.ParentCompanyCode,wo.ProcessFactoryCode) tbwork on rms.MaterialCode=tbwork.MaterialCode and b.MaterialName=tbwork.MaterialName and b.SpecificationModel=tbwork.SpecificationModel and rms.WorkAreaCode=tbwork.WorkAreaCode
							and c.ProcessFactoryCode=tbwork.ProcessFactoryCode ";
                //参数化
                List<Dos.ORM.Parameter> para = new List<Dos.ORM.Parameter>();
                var data = Repositorys<RawMaterialStockResponse>.FromSqlToPage(sql + where, para, request.rows, request.page, "MaterialCode", "desc");
                string sqljzgj = @"select 
                                rms.SurplusNumber
                                from (
                                select 
                                MaterialCode,
                                ProjectId,
                                SiteCode,
                                StorageCode,
                                WorkAreaCode,
                                SUM(COUNT) as COUNT,
                                SUM(LockCount) as LockCount,
                                SUM(UseCount) as UseCount,
                                case when SUM(LockCount)+SUM(UseCount)<0 then 0 else SUM(LockCount)+SUM(UseCount) end as SurplusNumber
                                FROM TbRawMaterialStockRecord
                                group by MaterialCode,SiteCode,WorkAreaCode,StorageCode,ProjectId
                                )as rms
                            left join TbCompany cp1 on rms.SiteCode=cp1.CompanyCode
                            left join TbCompany cp2 on rms.WorkAreaCode=cp2.CompanyCode
                            left join TbCompany cp3 on cp2.ParentCompanyCode=cp3.CompanyCode
                            left join TbStorage c on c.StorageCode=rms.StorageCode
                            left join TbRawMaterialArchives b on b.MaterialCode=rms.MaterialCode
                            left join TbSysDictionaryData sdd2 on b.RebarType=sdd2.DictionaryCode
                            left join TbSysDictionaryData sdd3 on b.MeasurementUnit=sdd3.DictionaryCode
							left join(select od.MaterialCode,od.MaterialName,od.SpecificationModel,isnull(sum(od.WeightSmallPlan),0) as WeightSmallPlan,wo.ProjectId,cp1.ParentCompanyCode as WorkAreaCode,cp2.ParentCompanyCode as BranchCode,wo.ProcessFactoryCode from TbWorkOrder wo 
							left join TbWorkOrderDetail od on wo.OrderCode=od.OrderCode
							left join TbCompany cp1 on wo.SiteCode=cp1.CompanyCode
							left join TbCompany cp2 on cp1.ParentCompanyCode=cp2.CompanyCode
							where wo.Examinestatus='审核完成' and wo.ProcessingState='Received'
							group by od.MaterialCode,od.MaterialName,od.SpecificationModel,wo.ProjectId,cp1.ParentCompanyCode,cp2.ParentCompanyCode,wo.ProcessFactoryCode) tbwork on rms.MaterialCode=tbwork.MaterialCode and b.MaterialName=tbwork.MaterialName and b.SpecificationModel=tbwork.SpecificationModel and rms.WorkAreaCode=tbwork.WorkAreaCode
							and c.ProcessFactoryCode=tbwork.ProcessFactoryCode " + where + @" and  b.RebarType='BuildingSteel'";
                decimal BuildingSteelSum = 0;
                DataTable dtjzgj = Db.Context.FromSql(sqljzgj).ToDataTable();
                if (dtjzgj.Rows.Count > 0)
                {
                    BuildingSteelSum = Convert.ToDecimal(dtjzgj.Compute("sum(SurplusNumber)", ""));
                }
                string sqlxg = @"select 
                                rms.SurplusNumber
                                from (
                                select 
                                MaterialCode,
                                ProjectId,
                                SiteCode,
                                StorageCode,
                                WorkAreaCode,
                                SUM(COUNT) as COUNT,
                                SUM(LockCount) as LockCount,
                                SUM(UseCount) as UseCount,
                                case when SUM(LockCount)+SUM(UseCount)<0 then 0 else SUM(LockCount)+SUM(UseCount) end as SurplusNumber
                                FROM TbRawMaterialStockRecord
                                group by MaterialCode,SiteCode,WorkAreaCode,StorageCode,ProjectId
                                )as rms
                            left join TbCompany cp1 on rms.SiteCode=cp1.CompanyCode
                            left join TbCompany cp2 on rms.WorkAreaCode=cp2.CompanyCode
                            left join TbCompany cp3 on cp2.ParentCompanyCode=cp3.CompanyCode
                            left join TbStorage c on c.StorageCode=rms.StorageCode
                            left join TbRawMaterialArchives b on b.MaterialCode=rms.MaterialCode
                            left join TbSysDictionaryData sdd2 on b.RebarType=sdd2.DictionaryCode
                            left join TbSysDictionaryData sdd3 on b.MeasurementUnit=sdd3.DictionaryCode
							left join(select od.MaterialCode,od.MaterialName,od.SpecificationModel,isnull(sum(od.WeightSmallPlan),0) as WeightSmallPlan,wo.ProjectId,cp1.ParentCompanyCode as WorkAreaCode,cp2.ParentCompanyCode as BranchCode,wo.ProcessFactoryCode from TbWorkOrder wo 
							left join TbWorkOrderDetail od on wo.OrderCode=od.OrderCode
							left join TbCompany cp1 on wo.SiteCode=cp1.CompanyCode
							left join TbCompany cp2 on cp1.ParentCompanyCode=cp2.CompanyCode
							where wo.Examinestatus='审核完成' and wo.ProcessingState='Received'
							group by od.MaterialCode,od.MaterialName,od.SpecificationModel,wo.ProjectId,cp1.ParentCompanyCode,cp2.ParentCompanyCode,wo.ProcessFactoryCode) tbwork on rms.MaterialCode=tbwork.MaterialCode and b.MaterialName=tbwork.MaterialName and b.SpecificationModel=tbwork.SpecificationModel and rms.WorkAreaCode=tbwork.WorkAreaCode
							and c.ProcessFactoryCode=tbwork.ProcessFactoryCode " + where + @" and  b.RebarType='SectionSteel'";
                decimal SectionSteelSum = 0;
                DataTable dtxg = Db.Context.FromSql(sqlxg).ToDataTable();
                if (dtxg.Rows.Count > 0)
                {
                    SectionSteelSum = Convert.ToDecimal(dtxg.Compute("sum(SurplusNumber)", ""));
                }
                data.userdata = new { BuildingSteel = BuildingSteelSum, SectionSteel = SectionSteelSum };
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>                          
        /// 获取数据列表(明细)
        /// </summary>
        public PageModel GetDataDetialForPage(RawMaterialStockRequest request)
        {
            #region 模糊搜索条件

            string where = " where 1=1 ";
            where += " and a.MaterialCode='" + request.MaterialCode + "'";
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
                where += " and a.SiteCode='" + request.SiteCode + "'";
            else
                where += " and a.SiteCode=''";
            where += " and a.WorkAreaCode='" + request.WorkAreaCode + "'";
            if (!string.IsNullOrEmpty(request.ProjectId))
                where += " and a.ProjectId='" + request.ProjectId + "'";
            if (!string.IsNullOrEmpty(request.ProcessFactoryCode))
            {
                where += " and s.ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
            }
            if (request.IsOld)
            {
                where += " and ((a.LockCount+a.UseCount=0 and a.LockCount!=0) or a.ChackState>1)";
            }
            else
            {
                where += " and ((a.LockCount+a.UseCount!=0 or a.LockCount=0) and a.ChackState<=1)";
            }
            #endregion

            try
            {
                var sql = @"select
                                a.ID,
                                a.Factory,
                                a.BatchNumber,
                                a.InsertTime,
                                a.Count,
                                a.LockCount,
                                a.UseCount,
                                a.HistoryCount,
                                a.ChackState,
                                tsoi.Enclosure,
                                tsoi.TestReportNo
                                from 
                                TbRawMaterialStockRecord a
                                left join TbStorage s on a.StorageCode=s.StorageCode
                                LEFT JOIN TbSampleOrderItem tsoi ON tsoi.InOrderItemId=a.InOrderitemId";
                //参数化
                List<Dos.ORM.Parameter> para = new List<Dos.ORM.Parameter>();
                var data = Repositorys<RawMaterialStockItemResponse>.FromSqlToPage(sql + where, para, request.rows, request.page, "ID", "desc");
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 判断信息是否可操作
        /// </summary>
        /// <returns></returns>
        public AjaxResult AnyInfo(int keyValue)
        {
            var rawMaterialStock = Repository<TbRawMaterialStockRecord>.First(p => p.ID == keyValue);
            if (rawMaterialStock == null)
                return AjaxResult.Warning("信息不存在");
            return AjaxResult.Success(rawMaterialStock);
        }
        /// <summary>
        /// 判断信息是否已存在库存信息
        /// </summary>
        /// <param name="keyValue">Id</param>
        /// <param name="materialCode">原材料编号</param>
        /// <param name="storageCode">仓库编号</param>
        /// <param name="storageCode">站点编号</param>
        /// <returns></returns>
        public AjaxResult AnyAdd(TbRawMaterialStockRecord model)
        {
            var rawMaterialStock = Repository<TbRawMaterialStockRecord>.First(p => p.MaterialCode == model.MaterialCode
                                                                        && p.StorageCode == model.StorageCode
                                                                        && p.Factory == model.Factory
                                                                        && p.BatchNumber == model.BatchNumber
                                                                        && p.WorkAreaCode == model.WorkAreaCode
                                                                        && p.SiteCode == model.SiteCode);
            if (model.ID == 0)
            {
                if (rawMaterialStock == null)
                    return AjaxResult.Success();
            }
            else
            {
                if (rawMaterialStock == null || rawMaterialStock.ID == model.ID)
                    return AjaxResult.Success();
            }
            return AjaxResult.Warning("操作失败,该原材所在仓库及站点库存已存在");
        }

        #endregion

        #region APP统计信息

        /// <summary>
        /// 列表统计信息
        /// </summary>
        public DataTable GetIndexReportInfo(RawMaterialStockRequest request)
        {
            #region 模糊搜索条件

            string where = " ";
            string where2 = " ";
            if (!string.IsNullOrWhiteSpace(request.MaterialName))
            {
                where += (" and MaterialName like '%" + request.MaterialName + "%'");
            }

            if (!string.IsNullOrWhiteSpace(request.SpecificationModel))
            {
                where += (" and SpecificationModel like '%" + request.SpecificationModel + "%'");
            }
            if (!string.IsNullOrEmpty(request.ProjectId))
                where += (" and ProjectId='" + request.ProjectId + "'");
            if (!string.IsNullOrEmpty(request.ProcessFactoryCode))
                where += (" and ProcessFactoryCode='" + request.ProcessFactoryCode + "'");

            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                where2 += where;
                List<string> siteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);//站点
                string siteStr = string.Join("','", siteList);
                where2 += (" and SiteCode in('" + siteStr + "')");

                List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 4);//工区
                string workAreaStr = string.Join("','", WorkAreaList);
                where += (" and WorkAreaCode in('" + workAreaStr + "')");
               
            }

            #endregion
            string sql = @"SELECT b.RebarType,
                           SUM(a.UseCount) AS UseCount, 
                           SUM(iSNULL(c.WeightSmallPlan,0)) AS WeightSmallPlan,
                           FLOOR(((SUM(a.UseCount)-SUM(iSNULL(c.WeightSmallPlan,0)))/SUM(a.UseCount))*100) AS Point
                           FROM 
                           (
                           SELECT MaterialCode,ISNULL(SUM(UseCount),0)AS UseCount
                           FROM TbRawMaterialStockRecord sr
                           LEFT JOIN TbStorage ts ON sr.StorageCode=ts.StorageCode
                           WHERE ChackState=1  @WHERE1
                           GROUP BY MaterialCode
                           ) a
                           LEFT JOIN 
                           (
                           SELECT twod.MaterialCode,SUM(twod.WeightSmallPlan) AS WeightSmallPlan 
                           				FROM TbWorkOrderDetail twod
                           				INNER JOIN TbWorkOrder two ON two.OrderCode = twod.OrderCode
                           				and ProcessingState='Received'
                                        where 1=1  @WHERE2
                           GROUP BY twod.MaterialCode
                           ) c ON a.MaterialCode=c.MaterialCode
                           INNER JOIN TbRawMaterialArchives b ON a.MaterialCode=b.MaterialCode
                           GROUP BY b.RebarType";
            sql = sql.Replace("@WHERE1", where);
            sql = sql.Replace("@WHERE2", where2);
            var retData = Db.Context.FromSql(sql).ToDataTable();
            return retData;

        }

        /// <summary>
        /// 列表信息
        /// </summary>
        public PageModel GetIndexListInfo(RawMaterialStockRequest request)
        {
            #region 模糊搜索条件

            string where = " where 1=1 ";
            string where2 = " where 1=1 ";
            if (!string.IsNullOrWhiteSpace(request.MaterialName))
            {
                where += (" and MaterialName like '%" + request.MaterialName + "%'");
            }

            if (!string.IsNullOrWhiteSpace(request.SpecificationModel))
            {
                where += (" and SpecificationModel like '%" + request.SpecificationModel + "%'");
            }
            if (!string.IsNullOrEmpty(request.ProjectId))
                where += (" and ProjectId='" + request.ProjectId + "'");
            if (!string.IsNullOrEmpty(request.ProcessFactoryCode))
                where += (" and ProcessFactoryCode='" + request.ProcessFactoryCode + "'");
            if (request.IsEarly)
                where2 += " and Point>50";

            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 4);//工区
                string workAreaStr = string.Join("','", WorkAreaList);
                where2 += (" and WorkAreaCode in('" + workAreaStr + "')");
            }

            #endregion

            string sql = @"SELECT 
                            MaterialCode,
                            MaterialName,
                            SpecificationModel,
                            WorkAreaCode,
                            WorkAreaName, 
                            BranchName, 
                            Total,
                            WeightSmallPlan, 
                            Point  
                            FROM (
                            select 
                                rms.MaterialCode,
                                rms.WorkAreaCode,
                                CASE WHEN (rms.LockCount +rms.UseCount)>0
                                THEN
                                 	rms.LockCount +rms.UseCount
                                ELSE
                                	0
                                END AS Total,
                                isnull(c.WeightSmallPlan,0)AS WeightSmallPlan,
                                CASE
                                WHEN (rms.LockCount +rms.UseCount)>0
                                THEN
                                 	Convert(decimal(18,2),isnull((WeightSmallPlan/(rms.LockCount +rms.UseCount)*100),0))
                                WHEN isnull(WeightSmallPlan,0)=0
                                THEN
                                	0
                                ELSE
                                	100
                                END AS Point,
                                b.MaterialName,
                                b.SpecificationModel,
                                cp2.CompanyFullName as WorkAreaName,
                                cp3.CompanyFullName as BranchName
                                from (
                                select 
                                trmsr.MaterialCode,
                                trmsr.WorkAreaCode,
                                SUM(trmsr.LockCount) as LockCount,
                                SUM(trmsr.UseCount) as UseCount
                                FROM TbRawMaterialStockRecord trmsr
                                INNER join TbStorage d on d.StorageCode=trmsr.StorageCode
                                @WHERE
                                group by trmsr.MaterialCode,trmsr.WorkAreaCode
                                )as rms
                                LEFT JOIN  
                                (
                                	   SELECT 
                                	   tcpy.ParentCompanyCode,
                                	   twod.MaterialCode,SUM(twod.WeightSmallPlan) AS WeightSmallPlan 
                           				FROM TbWorkOrderDetail twod
                           				INNER JOIN TbWorkOrder two ON two.OrderCode = twod.OrderCode
                           				INNER JOIN TbCompany tcpy ON two.SiteCode=tcpy.CompanyCode
                           				and ProcessingState='Received'
                           				@WHERE
                                        GROUP BY 
                                        tcpy.ParentCompanyCode,
                                        twod.MaterialCode
                                ) as c ON rms.WorkAreaCode=c.ParentCompanyCode 
                                and rms.MaterialCode=c.MaterialCode
                            left join TbCompany cp2 on rms.WorkAreaCode=cp2.CompanyCode
                            left join TbCompany cp3 on cp2.ParentCompanyCode=cp3.CompanyCode
                            INNER join TbRawMaterialArchives b on b.MaterialCode=rms.MaterialCode
                        ) retdata";
            sql = sql.Replace("@WHERE", where);
            //参数化
            List<Dos.ORM.Parameter> para = new List<Dos.ORM.Parameter>();
            var data = Repository<TbRawMaterialStockRecord>.FromSqlToPageTable(sql + where2, para, request.rows, request.page, "Point", "desc");
            return data;

        }

        #endregion

        #region Private

        /// <summary>
        /// 获取要抵扣的库存记录
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private Tuple<List<TbRawMaterialStockRecord>, List<TbRawMaterialStockRecord>> GetStockRecord(List<TbRawMaterialStockRecord> model)
        {
            List<TbRawMaterialStockRecord> addList = new List<TbRawMaterialStockRecord>();//定义添加集合
            List<TbRawMaterialStockRecord> updateList = new List<TbRawMaterialStockRecord>();//定义修改集合
            //查找库存数据
            var materialCodeList = model.Select(p => p.MaterialCode).Distinct().ToList();
            var workAreaCodeList = model.Select(p => p.WorkAreaCode).Distinct().ToList();
            var projectId = model.Select(p => p.ProjectId).First();
            var stockRecordAllList = Repository<TbRawMaterialStockRecord>.Query(p => p.ProjectId == projectId
                                                            && (p.MaterialCode.In(materialCodeList)
                                                            || p.WorkAreaCode.In(workAreaCodeList))).ToList();

            model.ForEach(x =>
            {
                //查找可以抵扣数据
                var stockRecordList = stockRecordAllList.Where(p => p.MaterialCode == x.MaterialCode
                                                             && p.WorkAreaCode == x.WorkAreaCode).ToList();
                var stockRecordListTep = stockRecordList.Where(p => (p.LockCount + p.UseCount) < 0).ToList();
                if (stockRecordListTep.Any())
                {
                    var stockRecordIds = updateList.Select(p => p.ID).ToList();
                    var stockRecord = stockRecordListTep.Where(p => !stockRecordIds.Contains(p.ID) || p.IsGQDK == true).ToList();
                    if (stockRecord.Any())
                    {
                        //无论是否填写了站点，先抵扣站点，再抵扣工区
                        var stockRecordTep = stockRecord.OrderByDescending(p => p.SiteCode).ThenByDescending(p => p.InsertTime).ToList();
                        if (!string.IsNullOrEmpty(x.SiteCode))
                        {
                            stockRecordTep = stockRecord.Where(p => p.SiteCode == x.SiteCode).ToList();
                            stockRecordTep.AddRange(stockRecord.Where(p => p.SiteCode != x.SiteCode).OrderByDescending(p => p.SiteCode).ThenByDescending(p => p.InsertTime).ToList());
                        }
                        //查找到可抵扣的原材料库存
                        decimal deductionCount = x.Count;
                        foreach (var item in stockRecordTep)
                        {
                            var surplusCount = deductionCount + (item.UseCount + item.LockCount.Value);//剩余数量
                            if (surplusCount <= 0)
                            {
                                item.LockCount += deductionCount;
                                if (surplusCount < 0)
                                {
                                    //抵扣后还为负数炉批号替换掉
                                    item.BatchNumber = x.BatchNumber;
                                    item.Factory = x.Factory;
                                    item.IsGQDK = true;
                                }
                                updateList.Add(item);
                                deductionCount = surplusCount;
                                break;
                            }
                            else
                            {
                                item.LockCount += Math.Abs(item.UseCount + item.LockCount.Value);
                                updateList.Add(item);
                            }
                            deductionCount = surplusCount;
                        }
                        // 剩余重量放入工区
                        if (deductionCount > 0)
                        {
                            var newgq = MapperHelper.Map<TbRawMaterialStockRecord, TbRawMaterialStockRecord>(x);
                            newgq.Count = deductionCount;
                            newgq.SiteCode = "";
                            addList.Add(newgq);
                        }
                    }
                    else
                    {
                        addList.Add(x);
                    }
                }
                else
                {
                    addList.Add(x);
                }
            });
            return new Tuple<List<TbRawMaterialStockRecord>, List<TbRawMaterialStockRecord>>(addList, updateList);
        }

        #endregion

        /// <summary>
        /// 获取数据列表(原材料)
        /// </summary>
        public PageModel GetMaterialDataList(RawMaterialStockRequest request)
        {
            #region 模糊搜索条件

            var where = new Where<TbRawMaterialArchives>();
            if (!string.IsNullOrWhiteSpace(request.keyword))
            {
                where.And(p => p.MaterialName.Like(request.keyword) || p.SpecificationModel.Like(request.keyword));
            }
            if (request.IsYL)
            {
                string a = typeof(OperationEnum.MaterialType).GetEnumName(OperationEnum.MaterialType.圆钢);
                string b = typeof(OperationEnum.MaterialType).GetEnumName(OperationEnum.MaterialType.螺纹钢);
                where.And(p => p.MaterialName == a || p.MaterialName == b);
            }
            #endregion

            try
            {
                var data = Db.Context.From<TbRawMaterialArchives>().Select(
                    TbRawMaterialArchives._.MaterialCode,
                    TbRawMaterialArchives._.MaterialName,
                    TbRawMaterialArchives._.SpecificationModel,
                    TbRawMaterialArchives._.MeasurementUnit,
                    TbRawMaterialArchives._.RebarType,
                    TbSysDictionaryData._.DictionaryText.As("MeasurementUnitText"),
                    TbRawMaterialArchives._.MeasurementUnitZl)
                    .AddSelect(Db.Context.From<TbSysDictionaryData>()
                    .Select(p => p.DictionaryText)
                    .Where(TbSysDictionaryData._.DictionaryCode == TbRawMaterialArchives._.RebarType), "RebarTypeName")
                    .LeftJoin<TbSysDictionaryData>((a, c) => a.MeasurementUnit == c.DictionaryCode)
                    .Where(where).OrderBy(TbRawMaterialArchives._.MaterialCode)
                    .ToPageList(request.rows, request.page);
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 获取数据列表(仓库)
        /// </summary>
        public PageModel GetStorageDataList(RawMaterialStockRequest request)
        {
            #region 模糊搜索条件

            var where = new Where<TbStorage>();
            if (!string.IsNullOrWhiteSpace(request.keyword))
            {
                where.And(p => p.StorageName.Like(request.keyword));
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where.And(p => p.ProcessFactoryCode == request.ProcessFactoryCode);
            }
            if (!string.IsNullOrEmpty(request.MaterialType))
            {
                where.And(p => p.StorageAttribute == request.MaterialType);
            }
            #endregion
            try
            {
                var data = Db.Context.From<TbStorage>().Select(
                    TbStorage._.ID,
                    TbStorage._.StorageCode,
                    TbStorage._.StorageName,
                    TbStorage._.StorageAdd,
                    TbCompany._.CompanyFullName.As("StorageAscription"))
                    .LeftJoin<TbCompany>((a, c) => a.ProcessFactoryCode == c.CompanyCode)
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
        /// 导出
        /// </summary>
        /// <returns></returns>
        public DataTable GetExportList(RawMaterialStockRequest request) 
        {
            #region 模糊搜索条件
            string where = " where 1=1 ";
            if (!string.IsNullOrWhiteSpace(request.MaterialName))
            {
                where += (" and b.MaterialName like '%" + request.MaterialName + "%'");
            }

            if (!string.IsNullOrWhiteSpace(request.SpecificationModel))
            {
                where += (" and b.SpecificationModel like '%" + request.SpecificationModel + "%'");
            }

            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);//站点
                List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 4);//工区
                string siteStr = string.Join("','", SiteList);
                string workAreaStr = string.Join("','", WorkAreaList);
                where += (" and (rms.SiteCode in('" + siteStr + "') or rms.WorkAreaCode in('" + workAreaStr + "'))");
            }
            if (!string.IsNullOrEmpty(request.ProjectId))
                where += (" and rms.ProjectId='" + request.ProjectId + "'");
            if (!string.IsNullOrEmpty(request.ProcessFactoryCode))
                where += (" and c.ProcessFactoryCode='" + request.ProcessFactoryCode + "'");
            //排序
            where += " order by MaterialCode desc";
            #endregion
            try
            {
                var sql = @"select 
                                rms.*,
                                b.MaterialName,
                                b.SpecificationModel,
                                c.StorageName,
                                cp1.CompanyFullName as SiteName,
                                cp2.CompanyFullName as WorkAreaName,
                                cp3.CompanyFullName as BranchName,
                                sdd2.DictionaryText as RebarTypeText,
                                sdd3.DictionaryText as MeasurementUnitText,
                                rms.LockCount + rms.UseCount as PassCount,
                                case when  rms.LockCount + rms.UseCount>=0 then rms.LockCount + rms.UseCount else 0 end SurplusNumber 
                                from (
                                select 
                                MaterialCode,
                                ProjectId,
                                SiteCode,
                                StorageCode,
                                WorkAreaCode,
                                SUM(COUNT) as COUNT,
                                SUM(LockCount) as LockCount,
                                SUM(UseCount) as UseCount
                                FROM TbRawMaterialStockRecord
                                group by MaterialCode,SiteCode,WorkAreaCode,StorageCode,ProjectId
                                )as rms
                            left join TbCompany cp1 on rms.SiteCode=cp1.CompanyCode
                            left join TbCompany cp2 on rms.WorkAreaCode=cp2.CompanyCode
                            left join TbCompany cp3 on cp2.ParentCompanyCode=cp3.CompanyCode
                            left join TbStorage c on c.StorageCode=rms.StorageCode
                            left join TbRawMaterialArchives b on b.MaterialCode=rms.MaterialCode
                            left join TbSysDictionaryData sdd2 on b.RebarType=sdd2.DictionaryCode
                            left join TbSysDictionaryData sdd3 on b.MeasurementUnit=sdd3.DictionaryCode ";
                var data = Db.Context.FromSql(sql + where).ToDataTable();
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
