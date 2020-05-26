using Dos.Common;
using Dos.ORM;
using PM.Business.Production;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Business.RawMaterial
{
    /// <summary>
    /// 余料库存
    /// </summary>
    public class CloutStockLogic
    {
        private readonly TbWorkOrderLogic _workOrderLogic = new TbWorkOrderLogic();

        public static decimal rubbish = 0.5M;//废料标准

        #region 新增数据

        /// <summary>
        /// 新增数据
        /// </summary>
        public AjaxResult Insert(TbCloutStock model)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            model.State = 1;
            model.InsertTime = DateTime.Now;
            try
            {
                int count = 0;
                //查找库存记录，有修改无添加
                var ret = GetCloutStock(new List<TbCloutStock>() { model });
                if (ret.Item1.Any())
                {
                    count = Repository<TbCloutStock>.Insert(ret.Item1);
                }
                else
                {
                    count = Repository<TbCloutStock>.Update(ret.Item2);
                }
                if (count > 0)
                    return AjaxResult.Success();
                return AjaxResult.Error("操作失败");
            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 新增数据(导入)
        /// </summary>
        public AjaxResult Input(List<TbCloutStock> model, StringBuilder errorMsg,string ProjectId)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            //站点
            var siteNames = model.Select(p => p.SiteCode).ToList();
            //var sites = Repository<TbCompany>.Query(p => p.CompanyFullName.In(siteNames)).ToList();
            var where = new Where<TbCompany>();
            where.And(p => p.CompanyFullName.In(siteNames));
            var projectId = new WhereClip("(TbProjectCompany.ProjectId='" + ProjectId + "')");
            where.And(projectId);
            var sitesNew = Db.Context.From<TbCompany>()
                .Select(TbCompany._.All,
                        TbProjectCompany._.ProjectId)
                .LeftJoin<TbProjectCompany>((a, c) => a.CompanyCode == c.CompanyCode).Where(where).ToList();
            //加工厂
            var processFactoryNames = model.Select(p => p.ProcessFactoryCode).ToList();
            var processFactorys = Repository<TbCompany>.Query(p => p.CompanyFullName.In(processFactoryNames)).ToList();
            //查找原材料
            var specifications = model.Select(p => p.SpecificationModel).Distinct().ToList();
            var rawMaterials = Repository<TbRawMaterialArchives>.Query(p => p.SpecificationModel.In(specifications)).ToList();
            var addList = new List<TbCloutStock>();
            foreach (var item in model)
            {
                //判断原材料是否存在
                var rawMaterial = rawMaterials.FirstOrDefault(p => p.SpecificationModel == item.SpecificationModel);
                if (rawMaterial == null)
                {
                    errorMsg.AppendFormat("第{0}行规格【{1}】信息不存在！", item.IndexNum, item.SpecificationModel);
                    continue;
                }
                if (rawMaterial.MaterialName != "圆钢" && rawMaterial.MaterialName != "螺纹钢")
                {
                    errorMsg.AppendFormat("第{0}行原材料【{1}】信息错误，只能是【圆钢，螺纹钢】！", item.IndexNum, item.SpecificationModel);
                    continue;
                }
                item.MaterialName = rawMaterial.MaterialName;
                item.MaterialCode = rawMaterial.MaterialCode;
                item.SpecificationModel = rawMaterial.SpecificationModel;
                item.MeasurementUnitZl = rawMaterial.MeasurementUnitZl;
                item.Weight = Convert.ToDecimal(Convert.ToDecimal(rawMaterial.MeasurementUnitZl * item.Size * item.Number).ToString("f5"));
                //判断加工厂是否存在
                var processFactory = processFactorys.FirstOrDefault(p => p.CompanyFullName == item.ProcessFactoryCode);
                if (processFactory == null)
                {
                    errorMsg.AppendFormat("第{0}行加工厂【{1}】信息不存在！", item.IndexNum, item.ProcessFactoryCode);
                    continue;
                }
                item.ProcessFactoryCode = processFactory.CompanyCode;
                //判断站点是否存在
                if (sitesNew.Any())
                {

                    var site = sitesNew.FirstOrDefault(p => p.CompanyFullName == item.SiteCode);
                    if (site == null)
                    {
                        errorMsg.AppendFormat("第{0}行站点【{1}】信息不存在！", item.IndexNum, item.SiteCode);
                        continue;
                    }
                    item.SiteCode = site.CompanyCode;
                }
                else
                {
                    item.SiteCode = "-1";
                }
                item.State = 1;
                item.SourceType = 1;
                item.InsertTime = DateTime.Now;
                addList.Add(item);
            }
            if (addList.Count == 0)
                return AjaxResult.Error(errorMsg.ToString());

            //查找库存记录，有修改无添加
            var ret = GetCloutStock(addList);
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    if (ret.Item1.Any())
                    {
                        Repository<TbCloutStock>.Insert(trans, ret.Item1);
                    }
                    if (ret.Item2.Any())
                    {
                        Repository<TbCloutStock>.Update(trans, ret.Item2);
                    }
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
        public AjaxResult Update(TbCloutStock model)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            //验证是否可操作
            var anyRet = AnyInfo(model.ID);
            if (anyRet.state != ResultType.success.ToString())
                return anyRet;
            try
            {
                var count = Repository<TbCloutStock>.Update(model, p => p.ID == model.ID);
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
                var count = Repository<TbCloutStock>.Delete(p => p.ID == keyValue);
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
            var ret = Db.Context.From<TbCloutStock>()
                   .Select(
                       TbCloutStock._.All
                       , TbCompany._.CompanyFullName.As("SiteName"))
                    .AddSelect(Db.Context.From<TbCompany>()
                    .Select(p => p.CompanyFullName)
                    .Where(TbCompany._.CompanyCode == TbCloutStock._.ProcessFactoryCode), "ProcessFactoryName")
                    .LeftJoin<TbCompany>((a, c) => a.SiteCode == c.CompanyCode)
                    .Where(p => p.ID == keyValue).ToDataTable();
            return ret;
        }

        /// <summary>
        /// 获取数据列表(分页)
        /// </summary>
        public PageModel GetDataListForPage(CloutStockRequest request)
        {
            #region 模糊搜索条件

            List<Parameter> parameter = new List<Parameter>();
            string where = " where a.State=1";
            if (!string.IsNullOrWhiteSpace(request.MaterialName))
            {
                where += " and a.MaterialName like @MaterialName";
                parameter.Add(new Parameter("@MaterialName", '%' + request.MaterialName + '%', DbType.String, null));
            }
            if (!string.IsNullOrWhiteSpace(request.SpecificationModel))
            {
                where += " and a.SpecificationModel like @SpecificationModel";
                parameter.Add(new Parameter("@SpecificationModel", '%' + request.SpecificationModel + '%', DbType.String, null));
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);
                where += " and (a.SiteCode in('" + string.Join("','", SiteList) + "') or a.SiteCode='-1' or a.SiteCode is null)";
            }
            if (!string.IsNullOrEmpty(request.ProjectId))
            {
                where += " and a.ProjectId=@ProjectId";
                parameter.Add(new Parameter("@ProjectId", request.ProjectId, DbType.String, null));
            }
            if (!string.IsNullOrEmpty(request.ProcessFactoryCode))
            {
                where += " and a.ProcessFactoryCode=@ProcessFactoryCode";
                parameter.Add(new Parameter("@ProcessFactoryCode", request.ProcessFactoryCode, DbType.String, null));
            }
            //App详情
            if (request.ID != null && request.ID != 0)
            {
                where += " and a.ID=@ID";
                parameter.Add(new Parameter("@ID", request.ID, DbType.Int32, null));
            }

            #endregion

            try
            {
                var sql = @"select 
                                a.ID,
                                a.MaterialName,
                                a.MaterialCode,
                                a.SpecificationModel,
                                a.Size,
                                a.MeasurementUnitZl,
                                a.Number,
                                (a.Number-ISNULL(d.LockCount,0)) as UseNumber,
                                a.Weight,
                                a.Factory,
                                a.BatchNumber,
                                a.SourceType,
                                b.CompanyFullName as SiteName,
                                c.CompanyFullName as ProcessFactoryName,
                                e.ProjectName
                                from TbCloutStock a
                                left join TbCompany b on a.SiteCode=b.CompanyCode
                                left join TbCompany c on a.ProcessFactoryCode=c.CompanyCode
                                left join TbProjectInfo e on a.ProjectId=e.ProjectId
                                left join(
                                select 
                                sum(LockCount) as LockCount,
                                StockRecordId
                                from TbRMProductionMaterialLockCount
                                where SourceType=2
                                group by StockRecordId
                                )d on a.ID=d.StockRecordId";

                string orderBy = " MaterialCode,Size";
                var data = Repository<TbCloutStock>.FromSqlToPageTable(sql + where, parameter, request.rows, request.page, orderBy);
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
            var cloutStock = Repository<TbCloutStock>.First(p => p.ID == keyValue);
            if (cloutStock == null)
                return AjaxResult.Warning("信息不存在");
            return AjaxResult.Success(cloutStock);
        }

        /// <summary>
        /// 获取余料数据
        /// </summary>
        /// <param name="model"></param>
        /// <returns>item1:新增数据 item2:修改数据</returns>
        private Tuple<List<TbCloutStock>, List<TbCloutStock>> GetCloutStock(List<TbCloutStock> model)
        {
            var materialCodes = model.Select(p => p.MaterialCode).Distinct().ToList();
            var processFactoryCodes = model.Select(p => p.ProcessFactoryCode).Distinct().ToList();
            var cloutStockList = Repository<TbCloutStock>.Query(p => p.MaterialCode.In(materialCodes) && p.ProcessFactoryCode.In(processFactoryCodes)).ToList();
            var addList = new List<TbCloutStock>();
            var updateList = new List<TbCloutStock>();
            foreach (var item in model)
            {
                //查找余料库存记录
                var list = cloutStockList.Where(p => p.MaterialCode == item.MaterialCode
                                                     && p.ProcessFactoryCode == item.ProcessFactoryCode
                                                     && p.ProjectId == item.ProjectId
                                                     && p.Size == item.Size).ToList();
                if (!string.IsNullOrEmpty(item.SiteCode))
                    list.Where(p => p.SiteCode == item.SiteCode).ToList();
                if (!string.IsNullOrEmpty(item.Factory))
                    list.Where(p => p.Factory == item.Factory).ToList();
                if (!string.IsNullOrEmpty(item.BatchNumber))
                    list.Where(p => p.BatchNumber == item.BatchNumber).ToList();
                if (list.Any())
                {
                    list[0].Number += item.Number;
                    updateList.Add(list[0]);
                }
                else
                {
                    addList.Add(item);
                }
            }
            return new Tuple<List<TbCloutStock>, List<TbCloutStock>>(addList, updateList);
        }

        #endregion

        #region 余料流向

        /// <summary>
        /// 获取数据列表(分页)
        /// </summary>
        public PageModel GetCloutStockPlaceForPage(CloutStockRequest request)
        {
            #region 模糊搜索条件
            var where = new Where<TbCloutStockPlace>();
            //if (!string.IsNullOrWhiteSpace(request.MaterialName))
            //{
            //    where.And(d => d.MaterialName.Like(request.MaterialName));
            //}
            //if (!string.IsNullOrWhiteSpace(request.SpecificationModel))
            //{
            //    where.And(d => d.SpecificationModel.Like(request.SpecificationModel));
            //}
            if (!string.IsNullOrWhiteSpace(request.MaterialName1))
            {
                where.And(d => d.MaterialName.Like(request.MaterialName1));
            }
            if (!string.IsNullOrWhiteSpace(request.SpecificationModel1))
            {
                where.And(d => d.SpecificationModel.Like(request.SpecificationModel1));
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCodeS))
            {
                where.And(d => d.StartSiteCode == request.SiteCodeS);
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCodeE))
            {
                where.And(d => d.EndSiteCode == request.SiteCodeE);
            }
            if (request.PlaceTimeS.HasValue)
            {
                where.And(d => d.PlaceTime >= request.PlaceTimeS);
            }
            if (request.PlaceTimeE.HasValue)
            {
                where.And(d => d.PlaceTime <= request.PlaceTimeE);
            }
            where.And(d => d.State == 1);
            #endregion

            try
            {
                var data = Db.Context.From<TbCloutStockPlace>()
                    .Select(
                      TbCloutStockPlace._.ID
                    , TbCloutStockPlace._.MaterialName
                    , TbCloutStockPlace._.MaterialCode
                    , TbCloutStockPlace._.SpecificationModel
                    , TbCloutStockPlace._.PlaceTime
                    , TbCloutStockPlace._.Weight
                    , TbCompany._.CompanyFullName.As("StartSiteName")
                    , TbProjectInfo._.ProjectName.As("StartProjectName"))
                    .AddSelect(Db.Context.From<TbCompany>()
                      .Select(p => p.CompanyFullName)
                      .Where(TbCompany._.CompanyCode == TbCloutStockPlace._.EndSiteCode), "EndSiteName")
                    .AddSelect(Db.Context.From<TbProjectInfo>()
                      .Select(p => p.ProjectName)
                      .Where(TbProjectInfo._.ProjectId == TbCloutStockPlace._.EndProjectId), "EndProjectName")
                  .LeftJoin<TbCompany>((a, c) => a.StartSiteCode == c.CompanyCode)
                  .LeftJoin<TbProjectInfo>((a, c) => a.ProjectId == c.ProjectId)
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

        #endregion

        #region 统计报表

        /// <summary>
        /// 余料当前库存量统计
        /// </summary>
        /// <returns></returns>
        public Tuple<DataTable, decimal> GetCloutStockTotalReport(CloutStockReportRequest request)
        {
            string where = " where State=1 ";
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);
                where += " and (SiteCode in('" + string.Join("','", SiteList) + "') or SiteCode='-1' or SiteCode is null)";
            }
            string sql = @"SELECT b.MaterialName,b.SpecificationModel,a.* 
                         FROM 
                         (
                         	SELECT MaterialCode,SUM([Weight]) AS [Weight]
                         	FROM TbCloutStock
                         	@WHERE
                         	GROUP BY MaterialCode
                         ) a
                         LEFT JOIN TbRawMaterialArchives b ON a.MaterialCode=b.MaterialCode
                         ORDER BY a.[Weight] DESC";
            sql = sql.Replace("@WHERE", where);
            var retData = Db.Context.FromSql(sql).ToDataTable();
            decimal count = decimal.Parse(retData.Compute("Sum(Weight)", "true").ToString());
            return new Tuple<DataTable, decimal>(retData, count);
        }

        /// <summary>
        /// 余料产生及使用量统计
        /// </summary>
        /// <returns></returns>
//        public List<ListType> GetCloutStockUseReportNew(CloutStockReportRequest request)
//        {
//            string where = " WHERE 1=1 ";
//            if (!string.IsNullOrEmpty(request.ProjectId))
//            {
//                where += " and a.ProjectId='" + request.ProjectId + "'";
//            }
//            if (!string.IsNullOrEmpty(request.ProcessFactoryCode))
//            {
//                where += " and a.ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
//            }
//            if (request.HistoryMonth.HasValue)
//            {
//                string HistoryMonth = Convert.ToDateTime(request.HistoryMonth).ToString("yyyy-MM");
//                where += " and CONVERT(varchar(7), a.InsertTime, 23)='" + HistoryMonth + "'";
//            }
//            string where1 = "";
//            //if (!string.IsNullOrWhiteSpace(request.SiteCode))
//            //{
//            //      List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);//站点
//            //    string siteStr = string.Join("','", SiteList);
//            //    where += (" and a.SiteCode in('" + siteStr + "')");
//            //}
//            if (!string.IsNullOrWhiteSpace(request.OrgType))
//            {
//                if (request.OrgType == "2")
//                {
//                    where1 += " where Tb.JlbCode='" + request.SiteCode + "'";
//                }
//                else if (request.OrgType == "3")
//                {
//                    where1 += " where Tb.BranchCode='" + request.SiteCode + "'";
//                }
//                else if (request.OrgType == "4")
//                {
//                    where1 += " where Tb.WorkAreaCode='" + request.SiteCode + "'";
//                }
//                else if (request.OrgType == "5")
//                {
//                    where1 += " where Tb.SiteCode='" + request.SiteCode + "'";
//                }
//            }

//            string sql = @"select sum(Tb.Weight) as Weight,Tb.YlType,Tb.SiteCode,Tb.SiteName,Tb.WorkAreaCode,Tb.WorkAreaName,Tb.BranchCode,Tb.BranchName,Tb.JlbCode,tb.JlbName  from (SELECT a.MaterialCode,a.MaterialName,a.SpecificationModel,a.Weight,a.SiteCode,b.CompanyFullName as SiteName,c.CompanyCode as WorkAreaCode,c.CompanyFullName as WorkAreaName,d.CompanyCode as BranchCode,d.CompanyFullName as BranchName,e.CompanyCode as JlbCode,e.CompanyFullName as JlbName,1 as YlType FROM TbCloutStock a 
//left join TbCompany b on a.SiteCode=b.CompanyCode
//left join TbCompany c on c.CompanyCode=b.ParentCompanyCode
//left join TbCompany d on d.CompanyCode=c.ParentCompanyCode
//left join TbCompany e on e.CompanyCode=d.ParentCompanyCode
//" + where + @" and (ISNULL(@MaterialName,'')='' or a.MaterialName=@MaterialName)  and (ISNULL(@SpecificationModel,'')='' or a.SpecificationModel like '%'+@SpecificationModel+'%')
//union all
//select b.MaterialCode,r.MaterialName,r.SpecificationModel,case when isnull(b.WeightSmallPlan,0)-isnull(b.WeightSmallPlanN,0)>=0 then b.WeightSmallPlanN else WeightSmallPlan end as PickCount,a.SiteCode,e.CompanyFullName as SiteName,d.CompanyCode as WorkAreaCode,d.CompanyFullName as WorkAreaName,c.CompanyCode as BranchCode,c.CompanyFullName as BranchName,f.CompanyCode as JlbCode,f.CompanyFullName as JlbName,2 as YlType from TbRMProductionMaterial a
//left join TbRMProductionMaterialPlan b on a.CollarCode=b.CollarCode
//left join TbRawMaterialArchives r on b.MaterialCode=r.MaterialCode
//left join TbCompany c on a.BranchCode=c.CompanyCode
//left join TbCompany d on a.WorkAreaCode=d.CompanyCode
//left join TbCompany e on a.SiteCode=e.CompanyCode
//left join TbCompany f on c.ParentCompanyCode=f.CompanyCode
//" + where + @" and b.RMTypeName='余料' and (ISNULL(@MaterialName,'')='' or r.MaterialName=@MaterialName)  and (ISNULL(@SpecificationModel,'')='' or r.SpecificationModel like '%'+@SpecificationModel+'%')) Tb 
//group by Tb.YlType,Tb.SiteCode,Tb.SiteName,Tb.WorkAreaCode,Tb.WorkAreaName,Tb.BranchCode,Tb.BranchName,Tb.JlbCode,tb.JlbName";
//            var retData = Db.Context.FromSql(sql + where1)
//                .AddInParameter("@MaterialName", DbType.String, request.MaterialName)
//                .AddInParameter("@SpecificationModel", DbType.String, request.SpecificationModel).ToDataTable();
//            List<ListType> list = new List<ListType>();
//            if (retData.Rows.Count > 0)
//            {
//                for (int i = 0; i < retData.Rows.Count; i++)
//                {
//                    if (retData.Rows[i]["YlType"].ToString() == "1")
//                    {
//                        var item = new ListType()
//                        {
//                            jlbName = retData.Rows[0]["JlbName"].ToString(),
//                            fbName = retData.Rows[0]["BranchName"].ToString(),
//                            gqName = retData.Rows[0]["WorkAreaName"].ToString(),
//                            zdName = retData.Rows[0]["SiteName"].ToString(),
//                            ylscl = Convert.ToDecimal(retData.Rows[i]["Weight"]),
//                            lll = 0
//                        };
//                        list.Add(item);
//                    }
//                    else
//                    {
//                        var item = new ListType()
//                        {
//                            jlbName = retData.Rows[0]["JlbName"].ToString(),
//                            fbName = retData.Rows[0]["BranchName"].ToString(),
//                            gqName = retData.Rows[0]["WorkAreaName"].ToString(),
//                            zdName = retData.Rows[0]["SiteName"].ToString(),
//                            ylscl = 0,
//                            lll = Convert.ToDecimal(retData.Rows[i]["Weight"])
//                        };
//                        list.Add(item);
//                    }
//                }
//                if (list.Count>0)
//                {
//                    if (request.OrgType=="2")
//                    {
//                        list.Where()
//                    }
//                }
//            }
//            return list;
//        }

//        public class ListType
//        {
//            public string jlbName { get; set; }
//            public string fbName { get; set; }
//            public string gqName { get; set; }
//            public string zdName { get; set; }
//            public decimal ylscl { get; set; }
//            public decimal lll { get; set; }
//        }
        /// <summary>
        /// 余料产生及使用量统计
        /// </summary>
        /// <returns></returns>
        public DataTable GetCloutStockUseReport(CloutStockReportRequest request)
        {
            string where = " WHERE 1=1 ";
            if (request.Year > 0)
            {
                where += " and YEAR(insertTime)=" + request.Year;
                if (request.Month > 0)
                    where += " and MONTH(insertTime)=" + request.Month;
            }
            string sql = @"SELECT
                        	ISNULL(dbo.GetCompanyParentName_fun(a.CompanyCode),a.CompanyFullName) AS CompanyName, 
                        	 (
                        	 	SELECT  ISNULL(SUM([Weight]),0) AS [Weight]
                        	    FROM TbCloutStock
                        	 	@WHERE  
                        		AND SiteCode IN
                        		(
                        		  SELECT CompanyCode from dbo.GetCompanyChild_fun(a.CompanyCode) WHERE OrgType=5
                        		)
                        	 ) CloutCount, --余料
                        	 (
                        		SELECT 
                        		    ISNULL(SUM(
                        			 CASE 
                        			 WHEN (WeightSmallPlan-WeightSmallPlanN)>=0 THEN WeightSmallPlanN 
                        			 ELSE
                        			 WeightSmallPlan
                        			 END
                        			 ),0) PickCount
                        		FROM TbRMProductionMaterialPlan la
                        		LEFT JOIN TbRMProductionMaterial lb ON la.CollarCode=lb.CollarCode
                        		@WHERE and la.RMTypeName='余料'
                        		AND 
                        		SiteCode IN
                        		(
                        		  SELECT CompanyCode from dbo.GetCompanyChild_fun(a.CompanyCode) WHERE OrgType=5
                        		) 
                        	 )PickCount --领料
                        	FROM TbCompany a
                        	LEFT JOIN TbProjectCompany b ON a.CompanyCode=b.CompanyCode
                        	WHERE 
                        	a.OrgType=@OrgType AND b.ProjectId='@ProjectId'
                        	ORDER BY CloutCount desc";
            sql = sql.Replace("@WHERE", where);
            sql = sql.Replace("@ProjectId", request.ProjectId);
            sql = sql.Replace("@OrgType", request.OrgType);
            var retData = Db.Context.FromSql(sql).ToDataTable();
            return retData;
        }

        #endregion

        #region Private

        /// <summary>
        /// 计算领料件数
        /// </summary>
        private void GetRootNumber(CloutStockRequest request, List<CloutStockResponse> data, bool isyl = true)
        {
            int flag = 1;
            data.ForEach(x =>
            {
                x.ItemUseNum = request.ItemUseNum;//单件用量
                x.WeightSmallPlan = request.WeightSmallPlan;//重量小计
                x.WorkOrderItemId = request.WorkOrderItemId;
                x.CollarCode = request.CollarCode;
                x.Number = request.Number;
                x.MaterialName = request.MaterialName;
                x.PlanIndex = request.PlanIndex;
                x.IsPJOk = request.IsPJOk;
                if (isyl)
                {
                    GetRootNumberForYL(request, x);
                }
                else
                {
                    GetRootNumberForYCL(request, x, flag);
                    flag++;
                }
            });
            if (isyl)
            {
                if (request.IsZX)
                    data.OrderBy(p => p.Weight);
                else
                    data.OrderBy(p => p.NumberH);
            }
        }

        /// <summary>
        /// 计算领料件数(原材料)
        /// </summary>
        private void GetRootNumberForYCL(CloutStockRequest request, CloutStockResponse x, int flag)
        {
            //原材料使用规则：取最省材料的方式
            decimal length = GetEnumKey(x.MaterialName);//原材料尺寸
            x.RMTypeName = "原材料";
            x.StockID = flag;
            x.SizeSelection = length;
            if (x.Weight == 0)
            {
                x.UseNumber = int.MaxValue;
            }
            else
            {
                x.UseNumber = (int)Math.Floor((x.Weight / x.MeasurementUnitZl / length));
            }
            if (length > 0)
            {
                if (!request.IsZX)
                {
                    //计算领料根数及件数
                    //if (request.MaterialName == "圆钢")
                    //{
                    //    GetRootNumberForYG(x);
                    //}
                    //else
                    //{
                    //    GetRootNumber2(x, false);
                    //}
                    GetRootNumber2(x, false);
                    x.WeightSmallPlanN = (length * x.MeasurementUnitZl) * x.RootNumber;
                }
                if (x.Weight == 0)
                {
                    x.UseNumber = 0;
                }

            }
        }

        /// <summary>
        /// 计算领料件数（余料）
        /// </summary>
        /// <param name="request"></param>
        /// <param name="data"></param>
        private void GetRootNumberForYL(CloutStockRequest request, CloutStockResponse x)
        {
            decimal length = x.SizeSelection;//余料尺寸
            x.RMTypeName = "余料";
            if (length > 0)
            {
                if (!request.IsZX)
                {
                    //计算领料根数及件数
                    if (request.MaterialName == "圆钢")
                    {
                        GetRootNumberForYG(x);
                    }
                    else
                    {
                        GetRootNumber2(x);
                    }
                    x.WeightSmallPlanN = (length * x.MeasurementUnitZl) * x.RootNumber;
                }
            }
        }

        /// <summary>
        /// 计算领料件数(圆钢)
        /// </summary>
        /// <param name="request"></param>
        /// <param name="x"></param>
        public static void GetRootNumberForYG(CloutStockResponse x)
        {
            //圆钢使用规则：只能截取，不能拼接
            int rootNumber = 0;
            //计算领料根数及件数
            if (x.ItemUseNum <= x.SizeSelection)
            {
                //一个材料可拼成多少构件
                var num = Math.Floor(x.SizeSelection / x.ItemUseNum);
                //余下的材料长度
                var lg = x.SizeSelection - (num * x.ItemUseNum);
                x.hxcd = lg;
                rootNumber = (int)Math.Ceiling(x.Number / num);
                if (x.UseNumber >= rootNumber)//可用根数>需要领用根数
                {
                    x.RootNumber = rootNumber;
                }
                else
                {
                    x.RootNumber = x.UseNumber;
                }
                x.NumberH = (int)(x.RootNumber * num);
            }
        }

        /// <summary>
        /// 获取枚举值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private int GetEnumKey(string key)
        {
            int value = 0;
            OperationEnum.MaterialType flag;
            if (Enum.TryParse<OperationEnum.MaterialType>(key, true, out flag))
            {
                value = (int)Enum.Parse(typeof(OperationEnum.MaterialType), key);
            }
            return value;
        }

        /// <summary>
        /// 计算领料件数
        /// </summary>
        /// <param name="x"></param>
        /// <param name="rn"></param>
        public static void GetRootNumber2(CloutStockResponse x, bool isyl = true)
        {
            var length = x.SizeSelection;
            int rootNumber = 0;
            int numberH = 0;
            if (x.ItemUseNum >= length)
            {
                x.pjorjq = true;
                //拼接
                //判断一个构件需要多少原材料
                var num = Math.Ceiling(x.ItemUseNum / length);
                var numf = (int)num - 1;
                //判断需要多少根原材料
                var s = x.Number * num;//需要的原材料根数
                //判断余下的材料是否可再次拼接
                var lg = num * length - x.ItemUseNum;
                x.hxcd = lg;
                if (lg >= rubbish && (lg + length * numf) >= x.ItemUseNum)
                {
                    x.ispj = true;
                    //计算需要根数
                    var yldata = YLPJJSForNumber(x.ItemUseNum, length, numf, x.Number);
                    rootNumber = (int)yldata.CountIndex;
                    numberH = (int)yldata.Count;
                    x.hxcd = yldata.PJsize;
                    x.hxcdLast = yldata.PJsizeLast;
                    x.CountLast = yldata.CountLast;
                    if (x.UseNumber < rootNumber && isyl)//可用根数<需要领用根数
                    {
                        rootNumber = x.UseNumber;
                        //一根余料全用完所生成的件数及使用的根数
                        var data = YLPJJSForRootNumber(x.ItemUseNum, length, numf, rootNumber);
                        numberH = data.Countret;
                        x.hxcd = data.PJsize;
                    }
                }
                else
                {
                    //不可拼接
                    rootNumber = (int)s;
                    if (x.UseNumber < rootNumber)//可用根数<需要领用根数
                    {
                        if (isyl)
                            rootNumber = x.UseNumber;
                    }
                    numberH = (int)Math.Floor(rootNumber / num);
                }
            }
            else
            {
                //剪切
                //判断一个原材料可以生成多少构件
                var num = Math.Floor(length / x.ItemUseNum);
                //判断需要多少根原材料
                var s = (int)Math.Ceiling(x.Number / num);//需要的原材料根数
                //判断余下的材料是否可再次拼接
                var lg = length - (num * x.ItemUseNum);
                x.hxcd = lg;
                if (Math.Floor((lg + length) / x.ItemUseNum) > num && lg >= rubbish && x.IsPJOk)
                {
                    x.ispj = true;

                    //计算需要根数
                    var yldata = YLPJJSForNumber(x.ItemUseNum, length, 0, x.Number, false);
                    rootNumber = (int)yldata.CountIndex;
                    numberH = (int)yldata.Count;
                    x.hxcd = yldata.PJsize;
                    x.hxcdLast = yldata.PJsizeLast;
                    x.CountLast = yldata.CountLast;
                    if (x.UseNumber < rootNumber && isyl)//可用根数<需要领用根数
                    {
                        rootNumber = x.UseNumber;
                        //一根余料全用完所生成的件数及使用的根数
                        var data = YLPJJSForRootNumber(x.ItemUseNum, length, 0, rootNumber, false);
                        numberH = data.Countret;
                        x.hxcd = data.PJsize;
                    }
                }
                else
                {
                    //不可拼接
                    rootNumber = s;
                    if (x.UseNumber < rootNumber)//可用根数<需要领用根数
                    {
                        if (isyl)
                            rootNumber = x.UseNumber;
                    }
                    numberH = rootNumber * (int)num;
                }
            }
            x.RootNumber = rootNumber;
            x.NumberH = numberH;
        }

        #region 剪切计算

        /// <summary>
        /// 剪切件数
        /// </summary>
        /// <param name="itemUseNum">单件用量</param>
        /// <param name="size">材料长度</param>
        /// <param name="pjsize">剩余长度</param>
        /// <param name="js">件数</param>
        /// <param name="gs">根数</param>
        /// <param name="rList"></param>
        /// <returns></returns>
        private static List<YLPJData> YCLJQForJS(decimal itemUseNum, decimal size, decimal pjsize, int js, int gs, List<YLPJData> rList)
        {
            YLPJData r = new YLPJData();
            var length = size + pjsize;
            var count = Math.Floor(length / itemUseNum);//生成件数
            js += (int)count;
            var a = length - (count * itemUseNum);//剩余尺寸
            r.Count = js;
            r.CountIndex = gs;
            r.PJsize = a;
            //判断剩余尺寸能否再拼接
            if ((Math.Floor((size + a) / itemUseNum) > Math.Floor(size / itemUseNum)) && a >= rubbish)
            {
                rList.Add(r);
                gs++;
                return YCLJQForJS(itemUseNum, size, a, js, gs, rList);
            }
            else
            {
                r.IsLast = true;
                rList.Add(r);
                return rList;
            }
        }

        #endregion

        #region 拼接计算

        /// <summary>
        ///获取件数使用（根据根数）
        /// </summary>
        /// <param name="itemUseNum"></param>
        /// <param name="size"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        private static YLPJData YLPJJSForRootNumber(decimal itemUseNum, decimal size, int countpj, int num, bool ispj = true)
        {
            var retList = new List<YLPJData>();
            if (ispj)
            {
                YLPJForJS(itemUseNum, size, size, countpj, 1, retList);//拼接
                //最后一个
                var ret = retList.First(p => p.IsLast == true);
                var a = (int)Math.Floor(num / ret.CountIndex);
                if (a > 0 && num % ret.CountIndex == 0)
                {
                    a = a - 1;
                }
                var b = num - (a * ret.CountIndex);//一轮中的位置
                if (b < (countpj + 1))
                {
                    ret.Countret = (a * (int)ret.Count);
                    return ret;
                }
                else
                {
                    int c = (int)Math.Floor((b - (countpj + 1)) / countpj);
                    ret.Countret = (a * (int)ret.Count) + c + 1;
                    return ret;
                }
            }
            else
            {
                YCLJQForJS(itemUseNum, size, 0, 0, 1, retList);//剪切
                return getDataForIndex(retList, num, false);
            }
        }

        /// <summary>
        ///获取根数（根据件数）
        /// </summary>
        /// <param name="itemUseNum"></param>
        /// <param name="size"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        private static YLPJData YLPJJSForNumber(decimal itemUseNum, decimal size, int countpj, int number, bool ispj = true)
        {
            var retList = new List<YLPJData>();
            if (ispj)
            {
                YLPJForJS(itemUseNum, size, size, countpj, 1, retList);//拼接
            }
            else
            {
                YCLJQForJS(itemUseNum, size, 0, 0, 1, retList);//剪切
            }
            return getDataForIndex(retList, number);
        }
        private static YLPJData getDataForIndex(List<YLPJData> retList, decimal number, bool isGetCount = true)
        {
            //最后一个
            var ret = retList.First(p => p.IsLast == true);
            var yljs = ret.Count;
            if (!isGetCount)
                yljs = ret.CountIndex;
            //计算件数所需要的根数
            if (yljs > number)
            {
                var yldata = retList.Where(p => p.Count >= number).OrderBy(p => p.Count).First();
                if (!isGetCount)
                {
                    yldata = retList.First(p => p.CountIndex == number);
                    yldata.Countret = (int)yldata.Count;
                }
                yldata.PJsizeLast = ret.PJsize;
                yldata.CountLast = (int)yljs;
                return yldata;

            }
            else if (yljs == number)
            {
                ret.PJsizeLast = ret.PJsize;
                ret.CountLast = (int)yljs;
                if (!isGetCount)
                {
                    ret.Countret = (int)ret.Count;
                }
                return ret;
            }
            else
            {
                var a = Math.Floor(number / yljs);
                var b = number - (a * yljs);
                var yldataD = retList.Where(p => p.Count >= b).OrderBy(p => p.Count).First();
                var yldata = MapperHelper.Map<YLPJData, YLPJData>(yldataD);
                yldata.Count = a * ret.Count;
                yldata.CountIndex = a * ret.CountIndex;
                if (b > 0)
                {
                    yldata.Count += yldataD.Count;
                    yldata.CountIndex += yldataD.CountIndex;
                }
                if (!isGetCount)
                {
                    if (b > 0)
                    {
                        yldata = retList.First(p => p.CountIndex == b);
                        yldata.Count = a * ret.Count + yldata.Count;
                    }
                    yldata.CountIndex = number;
                    yldata.Countret = (int)yldata.Count;
                }
                yldata.PJsizeLast = ret.PJsize;
                yldata.CountLast = (int)yljs;
                return yldata;
            }
        }

        /// <summary>
        /// 余料拼接件数
        /// </summary>
        /// <param name="itemUseNum"></param>
        /// <param name="size"></param>
        /// <param name="pjsize"></param>
        /// <param name="count"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        private static List<YLPJData> YLPJForJS(decimal itemUseNum, decimal size, decimal pjsize, int countpj, int count, List<YLPJData> rList)
        {
            YLPJData r = new YLPJData();
            var a = size * countpj + pjsize - itemUseNum;
            r.Count = count;
            r.CountIndex = countpj * count + 1;
            r.PJsize = a;
            count++;
            if ((a + size * countpj > itemUseNum) && a >= rubbish)
            {
                rList.Add(r);
                return YLPJForJS(itemUseNum, size, a, countpj, count, rList);
            }
            else if ((a + size * countpj == itemUseNum) && a >= rubbish)
            {
                if (count == 2)
                    rList.Add(r);
                var rnew = new YLPJData();
                rnew.Count = count;
                rnew.CountIndex = countpj * count + 1;
                rnew.PJsize = 0;
                rnew.IsLast = true;
                rList.Add(rnew);
                return rList;
            }
            else
            {
                r.IsLast = true;
                rList.Add(r);
                return rList;
            }
        }

        #endregion

        /// <summary>
        /// 剪切，相同尺寸，原材料拼接
        /// </summary>
        /// <param name="request"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        private List<CloutStockResponse> GetYLForPlan1(CloutStockRequest request, string select, string where, List<Parameter> parameter)
        {
            string sqlStr = "select " + select;
            string orderBy = " order by a.Size";
            var data = Repositorys<CloutStockResponse>.FromSql(sqlStr + where + orderBy, parameter);
            if (data.Any())
            {
                data = GetRetYLData(data, request);
                data = data.Take(3).ToList();
                //计算领料件数
                if (request.PlanIndex == 4)
                {
                    var retList = new List<CloutStockResponse>();
                    //拼接原材料
                    var itemUseNum = request.ItemUseNum;
                    decimal length = GetEnumKey(request.MaterialName);//原材料尺寸
                    if (itemUseNum <= length)
                        return retList;
                    //获取原材料根数(一件)
                    var num = Math.Floor(itemUseNum / length);
                    request.ItemUseNum = length * num;
                    var yclData = GetDataListForPlanYCL(request);
                    if (!yclData.Any())
                        return retList;
                    //余下的所需材料长度
                    var lg = itemUseNum - (length * num);
                    //获取余料根数
                    request.ItemUseNum = lg;
                    GetRootNumber(request, data);
                    foreach (var item in data)
                    {
                        if (item.NumberH < item.Number)
                            continue;
                        var cs = new CloutStockResponse();
                        cs = item;
                        cs.SizeSelectionycl = yclData[0].SizeSelection;
                        cs.UseNumberycl = yclData[0].UseNumber;
                        cs.NumberHycl = yclData[0].NumberH;
                        cs.NumberH = yclData[0].NumberH;
                        cs.NumberHyl = 0;
                        cs.RootNumberycl = yclData[0].RootNumber;
                        cs.WeightSmallPlanNycl = yclData[0].WeightSmallPlanN;
                        cs.RMTypeNameycl = yclData[0].RMTypeName;
                        retList.Add(cs);
                    }
                    return retList;
                }
                else
                {
                    GetRootNumber(request, data);
                }
            }
            return data;
        }

        /// <summary>
        /// 不同尺寸
        /// </summary>
        /// <param name="request"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        private List<CloutStockResponse> GetYLForPlan2(CloutStockRequest request, string select, string where, List<Parameter> parameter)
        {
            string sqlStr = "select " + select;
            string orderBy = " order by a.Size desc";
            var data = Repositorys<CloutStockResponse>.FromSql(sqlStr + where + orderBy, parameter);
            var retList = new List<CloutStockResponse>();
            if (data.Any())
            {
                data = GetRetYLData(data, request);
                var reNumber = request.Number;
                var reItemUseNum = request.ItemUseNum;
                foreach (var item in data)
                {
                    if (retList.Count == 6)
                        break;
                    var ids = retList.Select(p => p.StockID).ToList();
                    if (ids.Contains(item.StockID))
                        continue;
                    //查找与当前尺寸可拼接的余料
                    var lg = request.ItemUseNum - item.SizeSelection;
                    var a = data.Where(p => p.SizeSelection >= lg && p.StockID != item.StockID && !ids.Contains(p.StockID))
                                .OrderBy(p => p.SizeSelection).FirstOrDefault();
                    if (a != null)
                    {
                        //计算当前余料根数
                        request.ItemUseNum = item.SizeSelection;
                        GetRootNumber(request, new List<CloutStockResponse>() { item });
                        var itemNew = MapperHelper.Map<CloutStockResponse, CloutStockResponse>(item);
                        itemNew.LableStr = "yllable_" + (retList.Count + 1);
                        itemNew.StockIDstr = (retList.Count + 1);
                        var itempj = MapperHelper.Map<CloutStockResponse, CloutStockResponse>(a);
                        itempj.LableStr = "yllable_" + (retList.Count + 1);
                        retList.Add(itemNew);
                        //计算拼接余料根数
                        request.ItemUseNum = reItemUseNum - item.SizeSelection;
                        request.Number = item.NumberH;
                        GetRootNumber(request, new List<CloutStockResponse>() { itempj });
                        itempj.StockIDstr = (retList.Count + 1);
                        itempj.NumberH = itemNew.NumberH;
                        retList.Add(itempj);
                        request.ItemUseNum = reItemUseNum;
                        request.Number = reNumber;
                    }
                }
            }
            return retList;
        }

        /// <summary>
        /// 自选余料
        /// </summary>
        /// <param name="request"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        private List<CloutStockResponse> GetYLForPlan3(CloutStockRequest request, string select, string where, List<Parameter> parameter)
        {
            string sqlStr = "select " + select;
            string orderBy = " order by a.Size ";
            var data = Repositorys<CloutStockResponse>.FromSql(sqlStr + where + orderBy, parameter);
            if (data.Any())
            {
                data = GetRetYLData(data, request);
                GetRootNumber(request, data);
            }
            return data;
        }

        /// <summary>
        /// 实时减扣库存数量
        /// </summary>
        /// <param name="data"></param>
        /// <param name="request"></param>
        private List<CloutStockResponse> GetRetYLData(List<CloutStockResponse> data, CloutStockRequest request)
        {
            if (request.DetailsDatas.Any())
            {
                var dList = request.DetailsDatas.Where(p => p.RMTypeName == "余料" && p.WorkOrderItemId != request.WorkOrderItemId).ToList();
                if (dList.Any())
                {
                    data.ForEach(x =>
                    {
                        var d = dList.FirstOrDefault(p => p.StockID == x.StockID);
                        if (d != null)
                        {
                            x.UseNumber -= d.RootNumber;
                        }
                    });
                }
            }
            var dataret = data.Where(p => p.UseNumber > 0).ToList();
            return dataret;
        }

        public static bool ispjok(string specificationModel, decimal itemUseNum)
        {
            bool flag = false;
            //单件用量>=型号*0.07  才能拼接
            var a = specificationModel.Split(' ');
            if (a.Length > 1)
            {
                decimal b = 0.0M;
                decimal.TryParse(a[1].Replace("mm", ""), out b);
                if (b > 0 && itemUseNum >= b * 0.07M)
                {
                    flag = true;
                }
            }
            return flag;
        }

        #endregion

        /// <summary>
        /// 获取领料单推荐方案(弹窗余料)
        /// </summary>
        public List<CloutStockResponse> GetDataListForPlanYL(CloutStockRequest request)
        {
            string sql = @"
                                a.ID as StockID,
                                a.Size as SizeSelection,
                                a.Factory,
                                a.BatchNumber,
                                a.TestReportNo,
                                a.SpecificationModel,
                                a.MeasurementUnitZl,
                                a.Weight,
                                a.MaterialCode,
                                ISNULL(Number-(ISNULL(c.LockCount,0)),0) as UseNumber
                                from TbCloutStock a
                                left join(
                                  select 
                                  StockRecordId,
                                  SUM(LockCount) as LockCount
                                  from TbRMProductionMaterialLockCount
                                  where SourceType=2 and CollarCode!=@CollarCode
                                  group by StockRecordId
                                )c on a.ID=c.StockRecordId ";

            List<Parameter> parameter = new List<Parameter>();
            parameter.Add(new Parameter("@CollarCode", request.CollarCode, DbType.String, null));
            string where = " where ISNULL(Number-(ISNULL(c.LockCount,0)),0)>0 and a.State=1";
            where += " and a.MaterialCode=@MaterialCode";
            parameter.Add(new Parameter("@MaterialCode", request.MaterialCode, DbType.String, null));
            //if (!string.IsNullOrWhiteSpace(request.ProjectId))
            //{
            //    where += " and a.ProjectId=@ProjectId";
            //    parameter.Add(new Parameter("@ProjectId", request.ProjectId, DbType.String, null));
            //}
            if (!string.IsNullOrEmpty(request.ProcessFactoryCode))
            {
                where += " and a.ProcessFactoryCode=@ProcessFactoryCode";
                parameter.Add(new Parameter("@ProcessFactoryCode", request.ProcessFactoryCode, DbType.String, null));
            }
            //单件用量>=型号*0.07  才能拼接
            var isok = ispjok(request.SpecificationModel, request.ItemUseNum);
            request.IsPJOk = isok;
            if (request.Type == 1)
            {
                where += " and a.Size>=@ItemUseNum";
                parameter.Add(new Parameter("@ItemUseNum", request.ItemUseNum, DbType.Decimal, null));
            }
            else if (request.Type == 2)
            {
                if (request.MaterialName == "圆钢" && request.PlanIndex != 4)
                    return new List<CloutStockResponse>();
                //单件用量>=型号*0.07  才能拼接
                if (!request.IsPJOk)
                    return new List<CloutStockResponse>();
                where += " and a.Size<=@ItemUseNum";
                parameter.Add(new Parameter("@ItemUseNum", request.ItemUseNum - rubbish, DbType.Decimal, null));
                if (request.PlanIndex == 2)
                {
                    where += " and a.Size>=@ItemUseNumrfy";
                    parameter.Add(new Parameter("@ItemUseNumrfy", request.ItemUseNum / 2, DbType.Decimal, null));
                }
            }
            try
            {
                if (request.PlanIndex == 2 || request.PlanIndex == 4 || request.PlanIndex == 1)
                {
                    //剪切，相同尺寸，原材料拼接
                    if (request.PlanIndex == 4)
                    {
                        decimal length = GetEnumKey(request.MaterialName);//原材料尺寸
                        var num = Math.Floor(request.ItemUseNum / length);
                        //余下的所需材料长度
                        var lg = request.ItemUseNum - (length * num);
                        //if (request.MaterialName != "圆钢")
                        //{
                        //    where += " and a.Size>=@lg";
                        //    parameter.Add(new Parameter("@lg", lg, DbType.Decimal, null));
                        //}
                        where += " and a.Size>=@lg";
                        parameter.Add(new Parameter("@lg", lg, DbType.Decimal, null));
                    }
                    return GetYLForPlan1(request, sql, where, parameter);
                }
                else if (request.PlanIndex == 3)
                {
                    //不同尺寸
                    return GetYLForPlan2(request, sql, where, parameter);
                }
                else
                {
                    if (request.MaterialName != "圆钢")
                    {
                        //单件用量>=型号*0.07  才能拼接
                        if (!request.IsPJOk)
                        {
                            where += " and a.Size>=@ItemUseNumrfyr";
                            parameter.Add(new Parameter("@ItemUseNumrfyr", request.ItemUseNum, DbType.Decimal, null));
                        }
                        else
                        {
                            where += " and a.Size>=@ItemUseNumrfyr";
                            parameter.Add(new Parameter("@ItemUseNumrfyr", request.ItemUseNum / 2, DbType.Decimal, null));
                        }
                    }
                    else
                    {
                        if (request.PlanIndex == 5)
                        {
                            where += " and a.Size>=@ItemUseNum";
                            parameter.Add(new Parameter("@ItemUseNum", request.ItemUseNum, DbType.Decimal, null));
                        }
                    }
                    //自选余料库
                    return GetYLForPlan3(request, sql, where, parameter);
                }
            }
            catch (Exception)
            {
                return new List<CloutStockResponse>();
            }
        }

        /// <summary>
        /// 获取领料单推荐方案(弹窗原材料)
        /// </summary>
        public List<CloutStockResponse> GetDataListForPlanYCL(CloutStockRequest request)
        {
            try
            {
                string sql = @"select  
                                a.* ,
                                b.SpecificationModel,
                                b.MaterialName,
                                b.MeasurementUnitZl
                              FROM ( 
                                SELECT 
                                 a.MaterialCode,
                                 SUM((a.UseCount+isnull(d.LockCount,0)))as Weight
                                 from TbRawMaterialStockRecord a
                                 LEFT JOIN (
                                	SELECT  StockRecordId,SUM(LockCount) AS LockCount FROM TbRMProductionMaterialLockCount
                                	WHERE SourceType=1 AND CollarCode=@CollarCode AND WorkOrderItemId=@WorkOrderItemId
                                	GROUP BY StockRecordId
                                ) AS d ON a.ID=d.StockRecordId
                                where a.ProjectId=@ProjectId and a.MaterialCode=@MaterialCode and a.WorkAreaCode=@WorkAreaCode and a.ChackState=1
                                GROUP BY a.MaterialCode) AS a
                               left join TbRawMaterialArchives b on a.MaterialCode=b.MaterialCode";
                var data = Db.Context.FromSql(sql)
                    .AddInParameter("@ProjectId", DbType.String, request.ProjectId)
                    .AddInParameter("@MaterialCode", DbType.String, request.MaterialCode)
                    .AddInParameter("@WorkAreaCode", DbType.String, request.WorkAreaCode)
                    .AddInParameter("@CollarCode", DbType.String, request.CollarCode)
                    .AddInParameter("@WorkOrderItemId", DbType.Int32, request.WorkOrderItemId)
                    .ToList<CloutStockResponse>();
                if (data.Any())
                {
                    //var dList = request.DetailsDatas;
                    //if (request.DetailsDatas.Any())
                    //    dList = request.DetailsDatas.Where(p => p.RMTypeName == "原材料" && p.WorkOrderItemId != request.WorkOrderItemId).ToList();
                    data.ForEach(x =>
                    {
                        //if (dList.Any())
                        //{
                        //    x.Weight -= dList.Sum(p => p.WeightSmallPlanN);
                        //}
                        decimal length = GetEnumKey(x.MaterialName);//原材料尺寸
                        var wt = x.MeasurementUnitZl * length;
                        if (x.Weight < wt)
                            x.Weight = 0;
                    });
                    //单件用量>=型号*0.07  才能拼接
                    var isok = ispjok(request.SpecificationModel, request.ItemUseNum);
                    request.IsPJOk = isok;
                    //计算领料件数
                    GetRootNumber(request, data, false);
                }
                return data;
            }
            catch (Exception)
            {
                return new List<CloutStockResponse>();
            }
        }

        /// <summary>
        /// 获取领料单推荐方案(加工订单)
        /// </summary>
        public List<CloutStockResponse> GetDataListForOrderYCL(CloutStockRequest request)
        {
            try
            {
                var MaterialCodeList = request.OrderDetailsDatas.Select(p => p.MaterialCode).Distinct().ToList();
                string MaterialCodeStr = string.Join("','", MaterialCodeList);
                string sql = @"select  
                                a.* ,
                                b.SpecificationModel,
                                b.MaterialName,
                                b.MeasurementUnitZl
                              FROM ( 
                                SELECT 
                                 a.MaterialCode, 
                                 SUM(a.UseCount)as Weight
                                 from TbRawMaterialStockRecord a
                                where a.ProjectId=@ProjectId and a.MaterialCode IN('" + MaterialCodeStr + "') and a.WorkAreaCode=@WorkAreaCode and a.ChackState=1";
                sql += @" GROUP BY a.MaterialCode) AS a
                               left join TbRawMaterialArchives b on a.MaterialCode=b.MaterialCode";
                var data = Db.Context.FromSql(sql)
                    .AddInParameter("@ProjectId", DbType.String, request.ProjectId)
                    .AddInParameter("@WorkAreaCode", DbType.String, request.WorkAreaCode)
                    .ToList<CloutStockResponse>();
                var retList = new List<CloutStockResponse>();
                if (data.Any())
                {
                    var materialList = MapperHelper.Map<OrderDetailsData, CloutStockResponse>(request.OrderDetailsDatas);
                    materialList.ForEach(x =>
                    {
                        var material = data.FirstOrDefault(p => p.MaterialCode == x.MaterialCode);
                        if (material != null)
                        {
                            x.Weight = material.Weight;
                            decimal length = GetEnumKey(x.MaterialName);//原材料尺寸
                            if (length > 0)
                            {
                                var wt = x.MeasurementUnitZl * length;
                                if (x.Weight < wt)
                                    x.Weight = 0;
                                request.SpecificationModel = x.SpecificationModel;
                                request.MaterialName = x.MaterialName;
                                request.MaterialCode = x.MaterialCode;
                                request.ItemUseNum = x.ItemUseNum;
                                request.Number = x.Number;
                                request.WorkOrderItemId = x.WorkOrderItemId;
                                request.PlanIndex = 7;
                                //单件用量>=型号*0.07  才能拼接
                                var isok = ispjok(request.SpecificationModel, request.ItemUseNum);
                                request.IsPJOk = isok;
                                //计算领料件数
                                var dataRootNumber = new List<CloutStockResponse>();
                                dataRootNumber.Add(x);
                                GetRootNumber(request, dataRootNumber, false);
                                retList.AddRange(dataRootNumber);
                            }
                        }
                    });
                }
                return retList;
            }
            catch (Exception)
            {
                return new List<CloutStockResponse>();
            }
        }

        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        public DataTable GetExportList(CloutStockRequest request)
        {

            #region 模糊搜索条件

            //List<Parameter> parameter = new List<Parameter>();
            string where = " where a.State=1";
            if (!string.IsNullOrWhiteSpace(request.MaterialName))
            {
                where += " and a.MaterialName like '%" + request.MaterialName + "%'";
            }
            if (!string.IsNullOrWhiteSpace(request.SpecificationModel))
            {
                where += " and a.SpecificationModel like '%" + request.SpecificationModel + "%'";
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);
                where += " and (a.SiteCode in('" + string.Join("','", SiteList) + "') or a.SiteCode='-1' or a.SiteCode is null)";
            }
            if (!string.IsNullOrEmpty(request.ProjectId))
            {
                where += " and a.ProjectId='" + request.ProjectId + "'";
            }
            if (!string.IsNullOrEmpty(request.ProcessFactoryCode))
            {
                where += " and a.ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
            }
            //App详情
            if (request.ID != null && request.ID != 0)
            {
                where += " and a.ID=" + request.ID + "";
            }

            //排序
            where += " order by MaterialCode,Size asc";

            #endregion

            try
            {
                var sql = @"select 
                                a.ID,
                                a.MaterialName,
                                a.MaterialCode,
                                a.SpecificationModel,
                                a.Size,
                                a.MeasurementUnitZl,
                                a.Number,
                                (a.Number-ISNULL(d.LockCount,0)) as UseNumber,
                                a.Weight,
                                a.Factory,
                                a.BatchNumber,
                                a.SourceType,
                                b.CompanyFullName as SiteName,
                                c.CompanyFullName as ProcessFactoryName,
                                e.ProjectName
                                from TbCloutStock a
                                left join TbCompany b on a.SiteCode=b.CompanyCode
                                left join TbCompany c on a.ProcessFactoryCode=c.CompanyCode
                                left join TbProjectInfo e on a.ProjectId=e.ProjectId
                                left join(
                                select 
                                sum(LockCount) as LockCount,
                                StockRecordId
                                from TbRMProductionMaterialLockCount
                                where SourceType=2
                                group by StockRecordId
                                )d on a.ID=d.StockRecordId";
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
