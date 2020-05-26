using Dos.ORM;
using PM.Business.Production;
using PM.Common;
using PM.Common.Helper;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using PM.DataEntity.Production.ViewModel;
using PM.DataEntity.System.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;

namespace PM.Business.RawMaterial
{
    public class TbRawMaterialMonthDemandPlanLogic
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
        public AjaxResult Insert(TbRawMaterialMonthDemandPlan model, List<TbRawMaterialMonthDemandPlanDetail> items)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            //判断信息是否存在
            var anyRet = AnyInfoType(model.RebarType, model.DemandPlanCode, model.WorkAreaCode, model.DemandMonth);
            if (anyRet.state.ToString() != ResultType.success.ToString())
                return anyRet;
            model.InsertUserCode = OperatorProvider.Provider.CurrentUser.UserCode;
            model.Examinestatus = "未发起";
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //添加信息
                    Repository<TbRawMaterialMonthDemandPlan>.Insert(trans, model);
                    //添加明细信息
                    Repository<TbRawMaterialMonthDemandPlanDetail>.Insert(trans, items);
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

        #region 修改数据

        /// <summary>
        /// 修改数据
        /// </summary>
        public AjaxResult Update(TbRawMaterialMonthDemandPlan model, List<TbRawMaterialMonthDemandPlanDetail> items)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            var anyRet = AnyInfo(model.ID);
            if (anyRet.state.ToString() != ResultType.success.ToString())
                return anyRet;
            var anyRetType = AnyInfoType(model.RebarType, model.DemandPlanCode, model.WorkAreaCode, model.DemandMonth);
            if (anyRetType.state.ToString() != ResultType.success.ToString())
                return anyRetType;
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())//使用事务
                {
                    //修改信息
                    Repository<TbRawMaterialMonthDemandPlan>.Update(trans, model, p => p.ID == model.ID);
                    if (items.Count > 0)
                    {
                        //删除历史明细信息
                        Repository<TbRawMaterialMonthDemandPlanDetail>.Delete(trans, p => p.DemandPlanCode == model.DemandPlanCode);
                        //添加明细信息
                        Repository<TbRawMaterialMonthDemandPlanDetail>.Insert(trans, items);
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
                    var count = Repository<TbRawMaterialMonthDemandPlan>.Delete(trans, p => p.ID == keyValue);
                    //删除明细信息
                    Repository<TbRawMaterialMonthDemandPlanDetail>.Delete(trans, p => p.DemandPlanCode == ((TbRawMaterialMonthDemandPlan)anyRet.data).DemandPlanCode);
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
        public Tuple<DataTable, DataTable> FindEntity(int dataID)
        {
            var ret = Db.Context.From<TbRawMaterialMonthDemandPlan>()
            .Select(
                    TbRawMaterialMonthDemandPlan._.All
                    , TbCompany._.CompanyFullName.As("BranchName")
                    , TbSupplier._.SupplierName
                    , TbUser._.UserName
                    , TbRawMaterialMonthDemandSupplyPlan._.SupplyPlanNum
                    , TbRawMaterialMonthDemandSupplyPlan._.InsertTime.As("SupplyInsertTime")
                    , TbSysDictionaryData._.DictionaryText.As("RebarTypeNew"))
                  .LeftJoin<TbCompany>((a, c) => a.BranchCode == c.CompanyCode)
                  .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                  .Where(TbCompany._.CompanyCode == TbRawMaterialMonthDemandPlan._.SiteCode), "SiteName")
                  .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                  .Where(TbCompany._.CompanyCode == TbRawMaterialMonthDemandPlan._.WorkAreaCode), "WorkAreaName")
                  .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                  .Where(TbCompany._.CompanyCode == TbRawMaterialMonthDemandPlan._.ProcessFactoryCode), "ProcessFactoryName")
                  .LeftJoin<TbSupplier>((a, c) => a.SupplierCode == c.SupplierCode)
                  .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                  .LeftJoin<TbRawMaterialMonthDemandSupplyPlan>((a, c) => a.DemandPlanCode == c.DemandPlanCode)
                  .LeftJoin<TbSysDictionaryData>((a, c) => a.RebarType == c.DictionaryCode)
                    .Where(p => p.ID == dataID).ToDataTable();
            //查找明细信息
            var items = Db.Context.From<TbRawMaterialMonthDemandPlanDetail>().Select(
               TbRawMaterialMonthDemandPlanDetail._.All,
               TbSysDictionaryData._.DictionaryText.As("MeasurementUnitText"))
           .LeftJoin<TbSysDictionaryData>((a, c) => a.MeasurementUnit == c.DictionaryCode)
           .Where(p => p.DemandPlanCode == ret.Rows[0]["DemandPlanCode"].ToString()).ToDataTable();
            return new Tuple<DataTable, DataTable>(ret, items);
        }

        /// <summary>
        /// 获取数据列表(分页)
        /// </summary>
        public PageModel GetDataListForPage(RawMonthDemPlanRequest request)
        {
            //组装查询语句
            #region 模糊搜索条件

            string where1 = " where 1=1";
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where1 += " and Tb.ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.DemandPlanCode))
            {
                where1 += " and Tb.DemandPlanCode like '%" + request.DemandPlanCode + "%'";
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);//站点
                List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 4);//工区
                string siteStr = string.Join("','", SiteList);
                string workAreaStr = string.Join("','", WorkAreaList);
                where1 += (" and (Tb.SiteCode in('" + siteStr + "') or Tb.WorkAreaCode in('" + workAreaStr + "'))");
            }
            if (!string.IsNullOrEmpty(request.ProjectId))
            {
                where1 += " and Tb.ProjectId='" + request.ProjectId + "'";
            }
            if (!string.IsNullOrEmpty(request.RebarType))
            {
                where1 += " and Tb.RebarType='" + request.RebarType + "'";
            }
            if (request.HistoryMonth.HasValue)
            {
                string DemandMonth = Convert.ToDateTime(request.HistoryMonth).ToString("yyyy-MM");
                where1 += " and Tb.DemandMonth='" + DemandMonth + "'";

            }
            if (!string.IsNullOrWhiteSpace(request.PlanType) && request.PlanType == "月度补充计划")
            {
                where1 += " and Tb.SupplyPlanCode is not null";
            }
            if (!string.IsNullOrWhiteSpace(request.PlanIsAmple))
            {
                where1 += " and Tb.PlanIsAmple='" + request.PlanIsAmple + "' and Tb.Examinestatus!='已退回' and Tb.Examinestatus!='已撤销'";
            }
            if (!string.IsNullOrWhiteSpace(request.SupplyAmple))
            {
                where1 += " and Tb.SupplyAmple='" + request.SupplyAmple + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.IsAsTj))
            {
                where1 += " and Tb.IsAsTj='" + request.IsAsTj + "' and Tb.Examinestatus!='已退回' and Tb.Examinestatus!='已撤销'";
            }
            #endregion

            try
            {
                string sql = @"select Tb.* from (select rmp.*,case when CONVERT(varchar(100), rmp.InsertTime, 23)<=DATEADD(MONTH,-1,rmp.DemandMonth+'-19') then '按时提交' else '超时提交' end IsAsTj,rmsp.SupplyPlanCode,rmsp.ID as SupplyID,isnull(rmsp.SupplyPlanNum,0) as SupplyPlanNum,rmsp.Examinestatus as SupplyExaminestatus,rmsp.InsertTime as SupplyInsertTime,isnull(tbPcJh.PcJhCount,0) as PcJhCount,isnull(TbPcJh.BatchPlanTotal,0) as BatchPlanTotal,isnull(TbGh.GhCount,0) as GhCount,isnull(TbGh.HasSupplierTotal,0) as HasSupplierTotal,isnull(TbRk.RkCount,0) as RkCount,isnull(TbRk.InCount,0) as InCount,isnull(TbQy.QyCount,0) as QyCount,isnull(TbQy.BhgZl,0) as BhgZl,(isnull(TbPcJh.BatchPlanTotal,0)+(isnull(TbPcJh.BatchPlanTotal,0)*0.1)) as JhSx,(isnull(TbPcJh.BatchPlanTotal,0)-(isnull(TbPcJh.BatchPlanTotal,0)*0.1)) as JhXx,cp1.CompanyFullName as BranchName,cp2.CompanyFullName as WorkAreaName,cp3.CompanyFullName as ProcessFactoryName,sd.DictionaryText as RebarTypeNew,case when (isnull(rmp.PlanTotal,0)+isnull(rmsp.SupplyPlanNum,0))<(isnull(TbPcJh.BatchPlanTotal,0)-(isnull(TbPcJh.BatchPlanTotal,0)*0.1)) then '计划不足' when (isnull(rmp.PlanTotal,0)+isnull(rmsp.SupplyPlanNum,0))>(isnull(TbPcJh.BatchPlanTotal,0)+(isnull(TbPcJh.BatchPlanTotal,0)*0.1)) then '计划过多' else '计划合理' end as PlanIsAmple,case when isnull(TbGh.HasSupplierTotal,0)-isnull(TbPcJh.BatchPlanTotal,0)<0 then '供货不足' else '供货完成' end as SupplyAmple,(isnull(TbGh.HasSupplierTotal,0)-isnull(TbPcJh.BatchPlanTotal,0)) as GhCe from TbRawMaterialMonthDemandPlan rmp
left join TbRawMaterialMonthDemandSupplyPlan rmsp on rmp.DemandPlanCode=rmsp.DemandPlanCode
left join (select TbPcJh.DemandPlanCode,count(1) as PcJhCount,sum(TbPcJh.BatchPlanTotal) as BatchPlanTotal from (select  fbnp.BatchPlanNum,fbnp.BatchPlanTotal,case when rmdsp.DemandPlanCode is null then fbnp.RawMaterialDemandNum else rmdsp.DemandPlanCode end DemandPlanCode from TbFactoryBatchNeedPlan fbnp
left join TbRawMaterialMonthDemandSupplyPlan rmdsp on fbnp.RawMaterialDemandNum=rmdsp.SupplyPlanCode) TbPcJh group by TbPcJh.DemandPlanCode) TbPcJh on rmp.DemandPlanCode=TbPcJh.DemandPlanCode
left join (select TbGh.DemandPlanCode,count(1) as GhCount,sum(TbGh.HasSupplierTotal) as HasSupplierTotal from (select a.ThisTime,b.DemandPlanCode,SUM(a.ThisTimeCount) as HasSupplierTotal from TbSupplyListDetailHistory a
left join (select a.BatchPlanNum,case when b.DemandPlanCode is null then a.RawMaterialDemandNum else b.DemandPlanCode end as DemandPlanCode from TbFactoryBatchNeedPlan a
left join TbRawMaterialMonthDemandSupplyPlan b on a.RawMaterialDemandNum=b.SupplyPlanCode) b on a.BatchPlanNum=b.BatchPlanNum
group by a.ThisTime,b.DemandPlanCode) TbGh group by TbGh.DemandPlanCode) TbGh on rmp.DemandPlanCode=TbGh.DemandPlanCode
left join (select TbRk.DemandPlanCode,COUNT(1) as RkCount,sum(TbRk.InCount) as InCount from (select case when rmdsp.DemandPlanCode is null then fbnp.RawMaterialDemandNum else rmdsp.DemandPlanCode end as DemandPlanCode,ino.InCount from TbInOrder ino
left join  TbFactoryBatchNeedPlan fbnp on ino.BatchPlanCode=fbnp.BatchPlanNum
left join TbRawMaterialMonthDemandSupplyPlan rmdsp on  fbnp.RawMaterialDemandNum=rmdsp.SupplyPlanCode) TbRk group by TbRk.DemandPlanCode) TbRk on rmp.DemandPlanCode=TbRk.DemandPlanCode
left join(select TbQy.DemandPlanCode,COUNT(1) as QyCount,sum(TbQy.BhgZl) as BhgZl from (select so.SampleOrderCode,fbnp.RawMaterialDemandNum,case when rmdsp.DemandPlanCode is null then fbnp.RawMaterialDemandNum else rmdsp.DemandPlanCode end as DemandPlanCode,isnull(soi.BhgZl,0) as BhgZl from TbSampleOrder so
left join TbInOrder ino on so.InOrderCode=ino.InOrderCode
left join TbFactoryBatchNeedPlan fbnp on ino.BatchPlanCode=fbnp.BatchPlanNum
left join TbRawMaterialMonthDemandSupplyPlan rmdsp on fbnp.RawMaterialDemandNum=rmdsp.SupplyPlanCode
left join (select soi.SampleOrderCode,sum(soi.Total) as BhgZl from TbSampleOrderItem soi where soi.ChackState=2 group by soi.SampleOrderCode) soi on  so.SampleOrderCode=soi.SampleOrderCode) TbQy group by TbQy.DemandPlanCode) TbQy on rmp.DemandPlanCode=TbQy.DemandPlanCode
left join TbCompany cp1 on rmp.BranchCode=cp1.CompanyCode
left join TbCompany cp2 on rmp.WorkAreaCode=cp2.CompanyCode
left join TbCompany cp3 on rmp.ProcessFactoryCode=cp3.CompanyCode
left join TbSysDictionaryData sd on rmp.RebarType=sd.DictionaryCode) Tb  ";
                //参数化
                List<Parameter> para = new List<Parameter>();
                var data = new PageModel();
                if (request.IsOutput)
                {
                    data.rows = Repository<TbFactoryBatchNeedPlan>.FromSqlToDataTable(sql + where1, para, "DemandPlanCode", "desc");
                }
                else
                {
                    data = Repository<TbFactoryBatchNeedPlan>.FromSqlToPageTable(sql + where1, para, request.rows, request.page, "DemandPlanCode", "desc");

                }
                //型钢总量
                string where2 = "";
                decimal SectionSteelSum = 0;//需求型钢总量
                decimal SectionSteelBCSum = 0;//需求型钢总量补充总量
                if (string.IsNullOrEmpty(request.RebarType))
                {
                    where2 += " and Tb.RebarType='SectionSteel'";
                }
                DataTable dt2 = Db.Context.FromSql(sql + where1 + where2).ToDataTable();
                if (dt2.Rows.Count > 0)
                {
                    SectionSteelSum = Convert.ToDecimal(dt2.Compute("sum(PlanTotal)", ""));
                    SectionSteelBCSum = Convert.ToDecimal(dt2.Compute("sum(SupplyPlanNum)", ""));
                }
                //建筑钢筋
                string where3 = "";
                decimal BuildingSteelSum = 0;//需求建筑钢筋总量
                decimal BuildingSteelBCSum = 0;//需求建筑钢筋补充总量
                if (string.IsNullOrEmpty(request.RebarType))
                {
                    where3 += " and Tb.RebarType='BuildingSteel'";
                }
                DataTable dt3 = Db.Context.FromSql(sql + where1 + where3).ToDataTable();
                if (dt3.Rows.Count > 0)
                {
                    BuildingSteelSum = Convert.ToDecimal(dt3.Compute("sum(PlanTotal)", ""));
                    BuildingSteelBCSum = Convert.ToDecimal(dt3.Compute("sum(SupplyPlanNum)", ""));
                }

                data.userdata = new SumRowData { BuildingSteel = BuildingSteelSum + BuildingSteelBCSum, SectionSteel = SectionSteelSum + SectionSteelBCSum };
                return data;
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
        /// 获取组织机构列表
        /// </summary>
        /// <param name="type">组织机构类型</param>
        /// <returns></returns>
        public PageModel GetCompanyList(int type, TbCompanyRequest request)
        {
            //参数化
            List<Parameter> parameter = new List<Parameter>();
            string where = " where 1=1 and cp.OrgType=@type";
            parameter.Add(new Parameter("@type", type, DbType.String, null));
            if (!string.IsNullOrWhiteSpace(request.keyword))
            {
                where += " and cp.CompanyFullName like @CompanyFullName";
                parameter.Add(new Parameter("@CompanyFullName", '%' + request.keyword + '%', DbType.String, null));
            }
            string sql = @"select cp.id,cp.CompanyCode,cp.CompanyFullName,cp.ParentCompanyCode,cp.Address,cp.OrgType,pc.ProjectId,pro.ProjectName from  TbCompany cp
left join TbProjectCompany pc on cp.CompanyCode=pc.CompanyCode  
left join TbProjectInfo pro on pc.ProjectId=pro.ProjectId ";
            if (type != 1)
            {
                where += " and (ISNULL(@ProjectId,'')='' or pc.ProjectId=@ProjectId)";
                parameter.Add(new Parameter("@ProjectId", request.ProjectId, DbType.String, null));
            }
            var model = Repository<TbCompany>.FromSqlToPageTable(sql + where, parameter, request.rows, request.page, "ID", "asc");
            return model;
        }

        /// <summary>
        /// 获取组织机构列表
        /// </summary>
        /// <param name="type">组织机构类型</param>
        /// <returns></returns>
        public PageModel GetCompanyWorkAreaOrSiteList(TbCompanyRequest request, string parentCode, int type)
        {
            //参数化
            List<Parameter> parameter = new List<Parameter>();
            string where = " where 1=1 and TREE.OrgType=@type ";
            parameter.Add(new Parameter("@type", type, DbType.Int32, null));
            if (!string.IsNullOrWhiteSpace(request.keyword))
            {
                where += " and TREE.CompanyFullName like @CompanyFullName";
                parameter.Add(new Parameter("@CompanyFullName", '%' + request.keyword + '%', DbType.String, null));
            }
            string sql = @"select * from GetCompanyChild_fun(@parentCode) as TREE";
            parameter.Add(new Parameter("@parentCode", parentCode, DbType.String, null));
            var model = Repository<TbCompany>.FromSqlToPageTable(sql + where, parameter, request.rows, request.page, "ID", "asc");
            return model;
        }

        public List<TbCompany> GetCompany()
        {
            ////新建查询where
            //var where = new Where<TbCompany>();
            //where.And(t => t.OrgType == 3 || t.OrgType == 4 || t.OrgType == 5);
            ////根据条件查询部分数据
            //return Repository<TbCompany>.Query(where, d => d.id, "asc").ToList();
            //string ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
            string sql = @"select * from  TbCompany cp
                           left join TbProjectCompany pc on cp.CompanyCode=pc.CompanyCode where pc.ProjectId=@ProjectId and (cp.OrgType=3 or cp.OrgType=4 or cp.OrgType=5 or cp.OrgType=2 or cp.OrgType=1) order by cp.id asc";
            DataTable ret = Db.Context.FromSql(sql).AddInParameter("@ProjectId", DbType.String, ProjectId).ToDataTable();
            List<TbCompany> model = ModelConvertHelper<TbCompany>.ToList(ret);
            return model;
        }

        /// <summary>
        /// 获取该组织机构的所有父级
        /// </summary>
        /// <param name="CompanyCode">组织机构编号</param>
        /// <returns></returns>
        public DataTable GetParentCompany(string CompanyCode)
        {
            string sql = @"with CityTree AS
                        (
                            SELECT CompanyCode,CompanyFullName,ParentCompanyCode,OrgType from TbCompany where CompanyCode =@CompanyCode
                            UNION ALL 
                            SELECT TbCompany.CompanyCode,TbCompany.CompanyFullName,TbCompany.ParentCompanyCode,TbCompany.OrgType from CityTree
                            JOIN TbCompany on CityTree.ParentCompanyCode= TbCompany.CompanyCode
                        )
                        SELECT * from CityTree where OrgType=3 or OrgType=4 or OrgType=5 or OrgType=2 order by OrgType;";
            DataTable dt = Db.Context.FromSql(sql)
                .AddInParameter("@CompanyCode", DbType.String, CompanyCode).ToDataTable();
            return dt;
        }

        /// <summary>
        /// 获取数据列表(原材料)
        /// </summary>
        public PageModel GetMaterialDataList(RawMaterialStockRequest request, string RebarType)
        {
            #region 模糊搜索条件

            var where = new Where<TbRawMaterialArchives>();
            if (!string.IsNullOrWhiteSpace(request.keyword))
            {
                where.And(p => p.MaterialName.Like(request.keyword) || p.SpecificationModel.Like(request.keyword) || p.MaterialCode.Like(request.keyword));
            }
            if (!string.IsNullOrWhiteSpace(RebarType))
            {
                where.And(p => p.RebarType == RebarType);
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

        public string GetCompanyAdd(string CompanyCode)
        {
            var ret = Repository<TbCompany>.First(p => p.CompanyCode == CompanyCode);
            return ret.Address;
        }
        #endregion

        #region 判断
        /// <summary>
        /// 判断信息是否可操作
        /// </summary>
        /// <returns></returns>
        public AjaxResult AnyInfo(int keyValue)
        {
            var monthDemandPlan = Repository<TbRawMaterialMonthDemandPlan>.First(p => p.ID == keyValue);
            if (monthDemandPlan == null)
                return AjaxResult.Warning("信息不存在");
            if (monthDemandPlan.Examinestatus != "未发起" && monthDemandPlan.Examinestatus != "已退回")
                return AjaxResult.Warning("信息正在审核中或已审核完成,不能进行此操作");

            return AjaxResult.Success(monthDemandPlan);
        }

        public AjaxResult AnyInfoType(string RebarType, string DemandPlanCode, string WorkAreaCode, string DemandMonth)
        {
            try
            {
                string sql = @"select RebarType,SiteCode from TbRawMaterialMonthDemandPlan where RebarType=@RebarType and WorkAreaCode=@WorkAreaCode and DemandMonth=@DemandMonth and DemandPlanCode!=@DemandPlanCode and Examinestatus != '已退回'";
                var model = Db.Context.FromSql(sql)
                    .AddInParameter("@RebarType", DbType.String, RebarType)
                    .AddInParameter("@WorkAreaCode", DbType.String, WorkAreaCode)
                    .AddInParameter("@DemandMonth", DbType.String, DemandMonth)
                    .AddInParameter("@DemandPlanCode", DbType.String, DemandPlanCode).ToDataTable();
                string RebarTypeName = "";
                if (RebarType == "SectionSteel")
                {
                    RebarTypeName = "型钢";
                }
                else
                {
                    RebarTypeName = "建筑钢筋";
                }
                if (model.Rows.Count > 0)
                    return AjaxResult.Warning("该站点在当月已经录入" + RebarTypeName + "的月度需求计划，请不要重复录入");
                return AjaxResult.Success();
            }
            catch (Exception)
            {
                throw;
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

        #region  获取当前登录人所属组织机构跟下级组织机构

        public List<TbCompany> GetLoginUserAllCompany(bool isNoSite = true)
        {
            string CompanyCode = OperatorProvider.Provider.CurrentUser.CompanyId;
            string OrgType = OperatorProvider.Provider.CurrentUser.OrgType;
            var listAll = new List<TbCompany>();
            string noSiteWhere = "";
            if (!isNoSite)
            {
                noSiteWhere = " and T.OrgType!=5";
            }
            if (OrgType == "1")//当前登录人是加工厂人员（加载所以线的组织机构数据）
            {
                string sqlJgc = @"select T.id,T.CompanyCode,T.ParentCompanyCode,T.CompanyFullName,T.Address,T.OrgType,pc.ProjectId from  TbCompany T
                           left join TbProjectCompany pc on T.CompanyCode=pc.CompanyCode where T.OrgType!=1" + noSiteWhere + " order by T.id asc";
                DataTable retjgc = Db.Context.FromSql(sqlJgc).ToDataTable();
                List<TbCompany> jgcList = ModelConvertHelper<TbCompany>.ToList(retjgc);
                listAll.AddRange(jgcList);
            }
            else// 当前登录人是经理部、分部、工区、站点人员（加载所属跟下级）
            {
                //获取当前登录人的所有上级除本身
                string sqlParentCompany = @"WITH T
                                        AS( 
                                            SELECT id,CompanyCode,ParentCompanyCode,CompanyFullName,Address,OrgType FROM TbCompany WHERE CompanyCode=@CompanyCode 
                                            UNION ALL 
                                            SELECT a.id,a.CompanyCode,a.ParentCompanyCode,a.CompanyFullName,a.Address,a.OrgType 
                                            FROM TbCompany a INNER JOIN T ON a.CompanyCode=T.ParentCompanyCode  
                                        ) 
                                        SELECT * FROM T 
                                        left join TbProjectCompany tpc on T.CompanyCode=tpc.CompanyCode where tpc.ProjectId=@ProjectId and T.CompanyCode!=@CompanyCode and T.OrgType!=1 " + noSiteWhere + " order by T.id asc;";
                DataTable retParent = Db.Context.FromSql(sqlParentCompany).AddInParameter("@CompanyCode", DbType.String, CompanyCode).AddInParameter("@ProjectId", DbType.String, ProjectId).ToDataTable();
                List<TbCompany> parentList = ModelConvertHelper<TbCompany>.ToList(retParent);
                listAll.AddRange(parentList);
                //获取当前登录人的所有下级包括本身
                string sqlSonCompany = @"WITH T
                                     AS( 
                                         SELECT id,CompanyCode,ParentCompanyCode,CompanyFullName,Address,OrgType FROM TbCompany WHERE CompanyCode=@CompanyCode 
                                         UNION ALL 
                                         SELECT a.id,a.CompanyCode,a.ParentCompanyCode,a.CompanyFullName,a.Address,a.OrgType  FROM TbCompany a INNER JOIN T ON a.ParentCompanyCode=T.CompanyCode  
                                     ) 
                                     SELECT T.*,tpc.ProjectId FROM T 
                                     left join TbProjectCompany tpc on T.CompanyCode=tpc.CompanyCode where tpc.ProjectId=@ProjectId and T.OrgType!=1" + noSiteWhere + " order by T.id asc;";
                DataTable retSon = Db.Context.FromSql(sqlSonCompany).AddInParameter("@CompanyCode", DbType.String, CompanyCode).AddInParameter("@ProjectId", DbType.String, ProjectId).ToDataTable();
                List<TbCompany> sonList = ModelConvertHelper<TbCompany>.ToList(retSon);
                listAll.AddRange(sonList);
            }
            return listAll;
        }

        public List<TbCompanyNew> GetLoginUserAllCompanyNew(bool isNoSite = true, string formMenuCode = "", string RebarType = "", string HistoryMonth = "")
        {
            string CompanyCode = OperatorProvider.Provider.CurrentUser.CompanyId;
            string OrgType = OperatorProvider.Provider.CurrentUser.OrgType;
            var listAll = new List<TbCompanyNew>();
            string noSiteWhere = "";
            if (!isNoSite)
            {
                noSiteWhere = " and T.OrgType!=5";
            }
            if (OrgType == "1")//当前登录人是加工厂人员（加载所以线的组织机构数据）
            {
                string sqlJgc = @"select T.id,T.CompanyCode,T.ParentCompanyCode,T.CompanyFullName,T.Address,T.OrgType,pc.ProjectId from  TbCompany T
                           left join TbProjectCompany pc on T.CompanyCode=pc.CompanyCode where T.OrgType!=1" + noSiteWhere + " order by T.id asc";
                DataTable retjgc = Db.Context.FromSql(sqlJgc).ToDataTable();
                List<TbCompanyNew> jgcList = ModelConvertHelper<TbCompanyNew>.ToList(retjgc);
                listAll.AddRange(jgcList);
            }
            else// 当前登录人是经理部、分部、工区、站点人员（加载所属跟下级）
            {
                //获取当前登录人的所有上级除本身
                string sqlParentCompany = @"WITH T
                                        AS( 
                                            SELECT id,CompanyCode,ParentCompanyCode,CompanyFullName,Address,OrgType FROM TbCompany WHERE CompanyCode=@CompanyCode 
                                            UNION ALL 
                                            SELECT a.id,a.CompanyCode,a.ParentCompanyCode,a.CompanyFullName,a.Address,a.OrgType 
                                            FROM TbCompany a INNER JOIN T ON a.CompanyCode=T.ParentCompanyCode  
                                        ) 
                                        SELECT * FROM T 
                                        left join TbProjectCompany tpc on T.CompanyCode=tpc.CompanyCode where tpc.ProjectId=@ProjectId and T.CompanyCode!=@CompanyCode and T.OrgType!=1 " + noSiteWhere + " order by T.id asc;";
                DataTable retParent = Db.Context.FromSql(sqlParentCompany).AddInParameter("@CompanyCode", DbType.String, CompanyCode).AddInParameter("@ProjectId", DbType.String, ProjectId).ToDataTable();
                List<TbCompanyNew> parentList = ModelConvertHelper<TbCompanyNew>.ToList(retParent);
                listAll.AddRange(parentList);
                //获取当前登录人的所有下级包括本身
                string sqlSonCompany = @"WITH T
                                     AS( 
                                         SELECT id,CompanyCode,ParentCompanyCode,CompanyFullName,Address,OrgType FROM TbCompany WHERE CompanyCode=@CompanyCode 
                                         UNION ALL 
                                         SELECT a.id,a.CompanyCode,a.ParentCompanyCode,a.CompanyFullName,a.Address,a.OrgType  FROM TbCompany a INNER JOIN T ON a.ParentCompanyCode=T.CompanyCode  
                                     ) 
                                     SELECT T.*,tpc.ProjectId FROM T 
                                     left join TbProjectCompany tpc on T.CompanyCode=tpc.CompanyCode where tpc.ProjectId=@ProjectId and T.OrgType!=1" + noSiteWhere + " order by T.id asc;";
                DataTable retSon = Db.Context.FromSql(sqlSonCompany).AddInParameter("@CompanyCode", DbType.String, CompanyCode).AddInParameter("@ProjectId", DbType.String, ProjectId).ToDataTable();
                List<TbCompanyNew> sonList = ModelConvertHelper<TbCompanyNew>.ToList(retSon);
                listAll.AddRange(sonList);
            }
            if (!string.IsNullOrWhiteSpace(formMenuCode))
            {
                DataTable dtWorkArea = GetStartWorkArea(HistoryMonth);
                if (dtWorkArea.Rows.Count > 0)
                {
                    List<string> FbList = new List<string>();//获取所有的分部
                    DateTime d1 = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));//当前时间
                    DateTime d2 = Convert.ToDateTime(Convert.ToDateTime(HistoryMonth).AddMonths(-1).ToString("yyyy-MM" + "-19"));
                    for (int i = 0; i < dtWorkArea.Rows.Count; i++)
                    {
                        TbCompanyNew compModel = listAll.Find(a => a.CompanyCode == dtWorkArea.Rows[i]["CompanyCode"].ToString());
                        if (compModel != null)
                        {
                            if (!FbList.Contains(compModel.ParentCompanyCode))
                            {
                                FbList.Add(compModel.ParentCompanyCode);
                            }
                            //查找这个工区是否提交了需求计划等信息
                            DataTable dt = GetWorkAreaInfo(HistoryMonth, RebarType, compModel.CompanyCode);

                            if (dt.Rows.Count > 0)
                            {
                                if (Convert.ToDateTime(dt.Rows[0]["InsertTime"]) > d2)
                                {
                                    compModel.Cstj = 1;
                                    if (dt.Rows[0]["PcJhType"].ToString() == "不足")
                                    {
                                        compModel.JhBz = 1;
                                    }
                                    else if (dt.Rows[0]["PcJhType"].ToString() == "过多")
                                    {
                                        compModel.JhGd = 1;
                                    }
                                    else if (dt.Rows[0]["PcJhType"].ToString() == "合理")
                                    {
                                        compModel.JhHl = 1;
                                    }
                                }
                                else
                                {
                                    compModel.Astj = 1;
                                    if (dt.Rows[0]["PcJhType"].ToString() == "不足")
                                    {
                                        compModel.JhBz = 1;
                                    }
                                    else if (dt.Rows[0]["PcJhType"].ToString() == "过多")
                                    {
                                        compModel.JhGd = 1;
                                    }
                                    else if (dt.Rows[0]["PcJhType"].ToString() == "合理")
                                    {
                                        compModel.JhHl = 1;
                                    }
                                }
                            }
                            else
                            {
                                if (d1 > d2)
                                {
                                    compModel.Cswtj = 1;
                                }
                                else
                                {
                                    compModel.Wtj = 1;
                                }
                            }
                        }
                    }
                    //循环所有的分部
                    for (int f = 0; f < FbList.Count; f++)
                    {
                        int wtj = 0;
                        int cswtj = 0;
                        int astj = 0;
                        int cstj = 0;
                        int hl = 0;
                        int bz = 0;
                        int gd = 0;
                        List<TbCompanyNew> compList1 = listAll.FindAll(a => a.ParentCompanyCode == FbList[f]);//获取分部下所有的工区
                        if (compList1.Count > 0)
                        {
                            for (int g = 0; g < compList1.Count; g++)
                            {
                                wtj += compList1[g].Wtj;
                                cswtj += compList1[g].Cswtj;
                                astj += compList1[g].Astj;
                                cstj += compList1[g].Cstj;
                                hl += compList1[g].JhHl;
                                bz += compList1[g].JhBz;
                                gd += compList1[g].JhGd;
                            }
                            TbCompanyNew fbModel = listAll.Find(a => a.CompanyCode == FbList[f]);
                            if (fbModel != null)
                            {
                                fbModel.Wtj = wtj;
                                fbModel.Cswtj = cswtj;
                                fbModel.Astj = astj;
                                fbModel.Cstj = cstj;
                                fbModel.JhHl = hl;
                                fbModel.JhBz = bz;
                                fbModel.JhGd = gd;
                            }

                        }
                    }
                    List<TbCompany> JlbList = Repository<TbCompany>.Query(d => d.OrgType == 2);//获取所有经理部
                    if (JlbList.Count > 0)
                    {
                        for (int j = 0; j < JlbList.Count; j++)
                        {
                            int wtj = 0;
                            int cswtj = 0;
                            int astj = 0;
                            int cstj = 0;
                            int hl = 0;
                            int bz = 0;
                            int gd = 0;
                            List<TbCompanyNew> compList1 = listAll.FindAll(a => a.ParentCompanyCode == JlbList[j].CompanyCode);//获取经理部下所有的工分部
                            for (int f = 0; f < compList1.Count; f++)
                            {
                                wtj += compList1[f].Wtj;
                                cswtj += compList1[f].Cswtj;
                                astj += compList1[f].Astj;
                                cstj += compList1[f].Cstj;
                                hl += compList1[f].JhHl;
                                bz += compList1[f].JhBz;
                                gd += compList1[f].JhGd;
                            }
                            TbCompanyNew jlbModel = listAll.Find(a => a.CompanyCode == JlbList[j].CompanyCode);//获取经理部下所有的分部
                            if (jlbModel != null)
                            {
                                jlbModel.Wtj = wtj;
                                jlbModel.Cswtj = cswtj;
                                jlbModel.Astj = astj;
                                jlbModel.Cstj = cstj;
                                jlbModel.JhHl = hl;
                                jlbModel.JhBz = bz;
                                jlbModel.JhGd = gd;
                            }
                        }
                    }
                }
            }
            return listAll;
        }

        public DataTable GetWorkAreaInfo(string HistoryMonth = "", string RebarType = "", string WorkAreaCode = "")
        {
            string sql = @"select a.DemandPlanCode,CONVERT(varchar(100), a.InsertTime, 23) as InsertTime ,case when (isnull(a.PlanTotal,0)+isnull(c.SupplyPlanNum,0))<(isnull(sum(b.BatchPlanTotal),0)-isnull(sum(b.BatchPlanTotal),0)*0.1) then '不足' when  (isnull(a.PlanTotal,0)+isnull(c.SupplyPlanNum,0))>(isnull(sum(b.BatchPlanTotal),0)+isnull(sum(b.BatchPlanTotal),0)*0.1) then '过多' else '合理' end PcJhType from TbRawMaterialMonthDemandPlan a
left join TbRawMaterialMonthDemandSupplyPlan c on a.DemandPlanCode=c.DemandPlanCode
left join (select fbnp.BatchPlanTotal,case when rmdsp.DemandPlanCode is null then fbnp.RawMaterialDemandNum else rmdsp.DemandPlanCode end DemandPlanCode from TbFactoryBatchNeedPlan fbnp
left join TbRawMaterialMonthDemandSupplyPlan rmdsp on fbnp.RawMaterialDemandNum=rmdsp.SupplyPlanCode) b on a.DemandPlanCode=b.DemandPlanCode
where a.DemandMonth='" + HistoryMonth + @"' and a.RebarType='" + RebarType + @"' and a.WorkAreaCode='" + WorkAreaCode + @"' and a.Examinestatus!='已退回' and a.Examinestatus!='已撤销'	group by a.DemandPlanCode,a.PlanTotal,c.SupplyPlanNum,CONVERT(varchar(100), a.InsertTime, 23) ";
            DataTable dt = Db.Context.FromSql(sql).ToDataTable();
            return dt;
        }
        #endregion

        #region 图形报表

        /// <summary>
        /// 图形1
        /// </summary>
        /// <param name="RebarType">钢筋类型</param>
        /// <param name="DemandMonth">需求月份</param>
        /// <returns></returns>
        public DataTable Img1(RawMonthDemPlanRequest request)
        {
            string where = "";
            string DemandMonth = Convert.ToDateTime(DateTime.Now).ToString("yyyy-MM");
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where += " and a.ProjectId='" + request.ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where += " and a.ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 4);//工区
                string workAreaStr = string.Join("','", WorkAreaList);
                where += " and a.WorkAreaCode in('" + workAreaStr + "')";
            }
            if (!string.IsNullOrWhiteSpace(request.RebarType))
            {
                where += " and a.RebarType='" + request.RebarType + "'";
            }
            if (request.HistoryMonth.HasValue)
            {
                DemandMonth = Convert.ToDateTime(request.HistoryMonth).ToString("yyyy-MM");
            }
            where += " and a.DemandMonth='" + DemandMonth + "'";
            string sql = @"select isnull(sum(a.PlanTotal),0)+isnull(SUM(d.SupplyPlanNum),0) as PlanTotal,isnull(sum(b.BatchPlanTotal),0) as BatchPlanTotal,isnull(sum(c.HasSupplierTotal),0) as HasSupplierTotal,(isnull(sum(a.PlanTotal),0)+isnull(SUM(d.SupplyPlanNum),0)-isnull(sum(b.BatchPlanTotal),0)) as xqjhce,(isnull(sum(c.HasSupplierTotal),0)-isnull(sum(b.BatchPlanTotal),0)) as ghce,case when isnull(sum(a.PlanTotal),0)<isnull(sum(b.BatchPlanTotal),0)-(isnull(sum(b.BatchPlanTotal),0)*0.1) then '计划不足' when isnull(sum(a.PlanTotal),0)>isnull(sum(b.BatchPlanTotal),0)+(isnull(sum(b.BatchPlanTotal),0)*0.1) then '计划过多' else '计划合理' end as IsJh from TbRawMaterialMonthDemandPlan a
left join (select a.DemandPlanCode,sum(isnull(a.BatchPlanTotal,0)) as BatchPlanTotal from (select case when b.DemandPlanCode is null then a.RawMaterialDemandNum else b.DemandPlanCode end DemandPlanCode,a.BatchPlanTotal from TbFactoryBatchNeedPlan a
left join TbRawMaterialMonthDemandSupplyPlan b on a.RawMaterialDemandNum=b.SupplyPlanCode) a group by a.DemandPlanCode) b on a.DemandPlanCode=b.DemandPlanCode
left join (select a.DemandPlanCode,sum(isnull(a.HasSupplierTotal,0)) as HasSupplierTotal from (select case when b.DemandPlanCode is null then a.RawMaterialDemandNum else b.DemandPlanCode end DemandPlanCode,a.HasSupplierTotal from TbSupplyList a
left join TbRawMaterialMonthDemandSupplyPlan b on a.RawMaterialDemandNum=b.SupplyPlanCode) a group by a.DemandPlanCode) c on a.DemandPlanCode=c.DemandPlanCode
left join TbRawMaterialMonthDemandSupplyPlan d on a.DemandPlanCode=d.DemandPlanCode
where 1=1 and a.Examinestatus='审核完成' ";
            DataTable dt = Db.Context.FromSql(sql + where).ToDataTable();
            return dt;

        }
        /// <summary>
        /// 图形2
        /// </summary>
        /// <param name="RebarType">钢筋类型</param>
        /// <param name="DemandMonth">需求月份</param>
        /// <returns></returns>
        public DataTable Img2(RawMonthDemPlanRequest request)
        {
            string where = "";
            string DemandMonth = Convert.ToDateTime(DateTime.Now).ToString("yyyy-MM");
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where += " and ProjectId='" + request.ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where += " and ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 4);//工区
                string workAreaStr = string.Join("','", WorkAreaList);
                where += " and WorkAreaCode in('" + workAreaStr + "')";
            }
            if (!string.IsNullOrWhiteSpace(request.RebarType))
            {
                where += " and RebarType='" + request.RebarType + "'";
            }
            if (request.HistoryMonth.HasValue)
            {
                DemandMonth = Convert.ToDateTime(request.HistoryMonth).ToString("yyyy-MM");
            }
            where += " and DemandMonth='" + DemandMonth + "'";

            string sql = @"select * from (select WorkAreaCode,RebarType,ProcessFactoryCode,ProjectId,DemandMonth,'1' as IsAs,Examinestatus from TbRawMaterialMonthDemandPlan where CONVERT(varchar(100), InsertTime, 23)>DATEADD(MONTH,-1,DemandMonth+'-19')
                           union all
                           select WorkAreaCode,RebarType,ProcessFactoryCode,ProjectId,DemandMonth,'2' as IsAs,Examinestatus from TbRawMaterialMonthDemandPlan where CONVERT(varchar(100), InsertTime, 23)<=DATEADD(MONTH,-1,DemandMonth+'-19')) Tb where 1=1 and Examinestatus!='已撤销' and Examinestatus!='已退回'";
            DataTable dt = Db.Context.FromSql(sql + where).ToDataTable();

            int gqs = 0;//工区数
            gqs = GetStartWorkAreaNew(request).Rows.Count;
            int astj = 0;//按时提交数
            int cstj = 0;//超时提交数
            //List<string> ytjgq = new List<string>();//所有已经提交了工区
            //int cswtj = 0;//超时未提交
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    //ytjgq.Add(dt.Rows[i]["WorkAreaCode"].ToString());
                    if (dt.Rows[i]["IsAs"].ToString() == "2")
                    {
                        astj += 1;
                    }
                    else
                    {
                        cstj += 1;
                    }
                }

                //    DataTable dtGq = GetStartWorkAreaNew(request);
                //    DateTime datetime1 = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));//系统当前时间
                //    DateTime datetime2 = Convert.ToDateTime(Convert.ToDateTime(DemandMonth).ToString("yyyy-MM" + "-19"));
                //    for (int w = 0; w < dtGq.Rows.Count; w++)
                //    {
                //        if (!ytjgq.Contains(dtGq.Rows[w]["CompanyCode"].ToString()))
                //        {
                //            //判断当前时间是否超过了当月的19号
                //            if (datetime1 > datetime2)
                //            {
                //                cswtj += 1;
                //            }
                //        }
                //    }
            }

            ////未提交=所有启动业务的工区数-超时提交的工区数-按时提交的工区数-超时未提交的工区数
            //int wtj = gqs - astj - cstj - cswtj;
            //超时未提交=所有启动业务的工区数-超时提交的工区数-按时提交的工区数
            int cswtj = gqs - astj - cstj;
            DataTable dtImg2 = new DataTable();
            DataRow dr = dtImg2.NewRow();
            dtImg2.Rows.Add(dr);
            DataColumn dc1 = new DataColumn("gqs", typeof(int));
            dc1.DefaultValue = gqs;
            dtImg2.Columns.Add(dc1);
            //DataColumn dc2 = new DataColumn("wtj", typeof(int));
            //dc2.DefaultValue = wtj;
            //dtImg2.Columns.Add(dc2);
            DataColumn dc3 = new DataColumn("astj", typeof(int));
            dc3.DefaultValue = astj;
            dtImg2.Columns.Add(dc3);
            DataColumn dc4 = new DataColumn("cstj", typeof(int));
            dc4.DefaultValue = cstj;
            dtImg2.Columns.Add(dc4);
            DataColumn dc5 = new DataColumn("cswtj", typeof(int));
            dc5.DefaultValue = cswtj;
            dtImg2.Columns.Add(dc5);
            return dtImg2;
        }

        /// <summary>
        /// 图形3
        /// </summary>
        /// <param name="RebarType">钢筋类型</param>
        /// <param name="DemandMonth">需求月份</param>
        /// <returns></returns>
        public DataTable Img3(RawMonthDemPlanRequest request)
        {
            string where = "";
            string DemandMonth = Convert.ToDateTime(DateTime.Now).ToString("yyyy-MM");
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where += " and rmp.ProjectId='" + request.ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where += " and rmp.ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 4);//工区
                string workAreaStr = string.Join("','", WorkAreaList);
                where += " and rmp.WorkAreaCode in('" + workAreaStr + "')";
            }
            if (!string.IsNullOrWhiteSpace(request.RebarType))
            {
                where += " and rmp.RebarType='" + request.RebarType + "'";
            }
            if (request.HistoryMonth.HasValue)
            {
                DemandMonth = Convert.ToDateTime(request.HistoryMonth).ToString("yyyy-MM");
            }
            where += " and rmp.DemandMonth='" + DemandMonth + "'";

            string sql = @"select TbJhType.JhType,isnull(TbJh.JhTypeCount,0) as JhTypeCount from (select '计划不足' as JhType
                           union all
                           select '计划过多' as JhType
                           union all
                           select '计划合理' as JhType) TbJhType
                           left join  (select TbJh.JhType,COUNT(1) as JhTypeCount from (select case when (rmp.PlanTotal+isnull(rmsp.SupplyPlanNum,0))<(isnull(TbPcJh.BatchPlanTotal,0)-(isnull(TbPcJh.BatchPlanTotal,0)*0.1)) then '计划不足' when (isnull(rmp.PlanTotal,0)+isnull(rmsp.SupplyPlanNum,0))>(isnull(TbPcJh.BatchPlanTotal,0)+(isnull(TbPcJh.BatchPlanTotal,0)*0.1)) then '计划过多' else '计划合理' end JhType from TbRawMaterialMonthDemandPlan rmp
                           left join TbRawMaterialMonthDemandSupplyPlan rmsp on rmp.DemandPlanCode=rmsp.DemandPlanCode
                           left join (select TbPcJh.DemandPlanCode,count(1) as PcJhCount,sum(TbPcJh.BatchPlanTotal) as BatchPlanTotal from (select  fbnp.BatchPlanNum,fbnp.BatchPlanTotal,case when rmdsp.DemandPlanCode is null then fbnp.RawMaterialDemandNum else rmdsp.DemandPlanCode end DemandPlanCode from TbFactoryBatchNeedPlan fbnp
                           left join TbRawMaterialMonthDemandSupplyPlan rmdsp on fbnp.RawMaterialDemandNum=rmdsp.SupplyPlanCode) TbPcJh group by TbPcJh.DemandPlanCode) TbPcJh on rmp.DemandPlanCode=TbPcJh.DemandPlanCode
                           where 1=1 and rmp.Examinestatus!='已退回' and rmp.Examinestatus!='已撤销' " + where + ") TbJh group by TbJh.JhType) TbJh on TbJhType.JhType=TbJh.JhType";
            DataTable dt = Db.Context.FromSql(sql).ToDataTable();
            return dt;
        }

        /// 图形4
        /// </summary>
        /// <param name="RebarType">钢筋类型</param>
        /// <param name="DemandMonth">需求月份</param>
        /// <returns></returns>
        public DataTable Img4(RawMonthDemPlanRequest request)
        {
            string where = "";
            string DemandMonth = Convert.ToDateTime(DateTime.Now).ToString("yyyy-MM");
            if (!string.IsNullOrWhiteSpace(request.RebarType))
            {
                where += " and a.SteelsTypeCode='" + request.RebarType + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where += " and a.ProjectId='" + request.ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where += " and a.ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 4);//工区
                string workAreaStr = string.Join("','", WorkAreaList);
                where += " and a.WorkAreaCode in('" + workAreaStr + "')";
            }
            if (request.HistoryMonth.HasValue)
            {
                DemandMonth = Convert.ToDateTime(request.HistoryMonth).ToString("yyyy-MM");
            }

            where += " and CONVERT(varchar(7), DATEADD(DAY,4,a.InsertTime), 120)='" + DemandMonth + "'";

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

        #endregion

        #region 获取所有启动业务的工区

        public DataTable GetStartWorkArea(string DemandMonth = "")
        {
            string dateTime1 = "";
            string dateTime2 = "";//上月
            string dateTime3 = "";//上上月
            if (!string.IsNullOrWhiteSpace(DemandMonth))
            {
                dateTime1 = Convert.ToDateTime(DemandMonth).ToString("yyyy-MM");
                dateTime2 = Convert.ToDateTime(DemandMonth).AddMonths(-1).ToString("yyyy-MM");
                dateTime3 = Convert.ToDateTime(DemandMonth).AddMonths(-2).ToString("yyyy-MM");
            }
            else
            {
                dateTime1 = DateTime.Now.ToString("yyyy-MM");//当月
                dateTime2 = DateTime.Now.AddMonths(-1).ToString("yyyy-MM");//上月
                dateTime3 = DateTime.Now.AddMonths(-2).ToString("yyyy-MM");//上上月
            }
            string OrgType = OperatorProvider.Provider.CurrentUser.OrgType;
            string ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
            string ProcessFactoryCode = OperatorProvider.Provider.CurrentUser.ProcessFactoryCode;
            //string sql = @"select * from (select cp2.CompanyCode,cp2.CompanyFullName from (select SiteCode from TbWorkOrder where 
            //(CONVERT(varchar(7) ,InsertTime, 120)=CONVERT(varchar(7) ,DATEADD(MONTH,-1,getdate()), 120) or CONVERT(varchar(7) ,InsertTime, 120)=CONVERT(varchar(7) ,DATEADD(MONTH,-2,getdate()), 120) or CONVERT(varchar(7) ,InsertTime, 120)=CONVERT(varchar(7) ,getdate(), 120)) and (ISNULL(@ProjectId,'')='' or ProjectId=@ProjectId) and (ISNULL(@ProcessFactoryCode,'')='' or ProcessFactoryCode=@ProcessFactoryCode)
            //group by SiteCode) TbWO
            //left join TbCompany cp on TBWO.SiteCode=cp.CompanyCode
            //left join TbCompany cp2 on cp2.CompanyCode=cp.ParentCompanyCode
            //group by cp2.CompanyCode,cp2.CompanyFullName
            //union all
            //select rmdp.WorkAreaCode,cp.CompanyFullName
            //from TbRawMaterialMonthDemandPlan rmdp
            //left join TbCompany cp on cp.CompanyCode=rmdp.WorkAreaCode
            //where (rmdp.DemandMonth=CONVERT(varchar(7) ,DATEADD(MONTH,-1,getdate()), 120) or rmdp.DemandMonth=CONVERT(varchar(7) ,DATEADD(MONTH,-2,getdate()), 120) or rmdp.DemandMonth=CONVERT(varchar(7) ,getdate(), 120)) and (ISNULL(@ProjectId,'')='' or ProjectId=@ProjectId) and (ISNULL(@ProcessFactoryCode,'')='' or ProcessFactoryCode=@ProcessFactoryCode)) TbGq group by TbGq.CompanyCode,TbGq.CompanyFullName";
            string sql = @"select * from (select cp2.CompanyCode,cp2.CompanyFullName from (select SiteCode from TbWorkOrder where 1=1 and Examinestatus!='已撤销' and Examinestatus!='已退回' and
            (CONVERT(varchar(7) ,InsertTime, 120)='" + dateTime2 + @"' or CONVERT(varchar(7) ,InsertTime, 120)='" + dateTime3 + @"' or CONVERT(varchar(7) ,InsertTime, 120)='" + dateTime1 + @"') and (ISNULL(@ProjectId,'')='' or ProjectId=@ProjectId) and (ISNULL(@ProcessFactoryCode,'')='' or ProcessFactoryCode=@ProcessFactoryCode)
            group by SiteCode) TbWO
            left join TbCompany cp on TBWO.SiteCode=cp.CompanyCode
            left join TbCompany cp2 on cp2.CompanyCode=cp.ParentCompanyCode
            group by cp2.CompanyCode,cp2.CompanyFullName
            union all
            select rmdp.WorkAreaCode,cp.CompanyFullName
            from TbRawMaterialMonthDemandPlan rmdp
            left join TbCompany cp on cp.CompanyCode=rmdp.WorkAreaCode
            where 1=1 and rmdp.Examinestatus!='已撤销' and rmdp.Examinestatus!='已退回' and (rmdp.DemandMonth='" + dateTime2 + @"' or rmdp.DemandMonth='" + dateTime3 + @"' or rmdp.DemandMonth='" + dateTime1 + @"') and (ISNULL(@ProjectId,'')='' or ProjectId=@ProjectId) and (ISNULL(@ProcessFactoryCode,'')='' or ProcessFactoryCode=@ProcessFactoryCode)) TbGq group by TbGq.CompanyCode,TbGq.CompanyFullName";
            DataTable dt = Db.Context.FromSql(sql)
                .AddInParameter("@ProjectId", DbType.String, ProjectId)
                .AddInParameter("@ProcessFactoryCode", DbType.String, ProcessFactoryCode).ToDataTable();
            return dt;
        }

        public DataTable GetStartWorkAreaNew(RawMonthDemPlanRequest request)
        {
            string where = "";
            string dateTime1 = "";
            string dateTime2 = "";//上月
            string dateTime3 = "";//上上月
            string DemandMonth = Convert.ToDateTime(DateTime.Now).ToString("yyyy-MM");
            if (request.HistoryMonth.HasValue)
            {
                DemandMonth = Convert.ToDateTime(request.HistoryMonth).ToString("yyyy-MM");
            }
            if (!string.IsNullOrWhiteSpace(DemandMonth))
            {
                dateTime1 = Convert.ToDateTime(DemandMonth).ToString("yyyy-MM");//当月
                dateTime2 = Convert.ToDateTime(DemandMonth).AddMonths(-1).ToString("yyyy-MM");//上月
                dateTime3 = Convert.ToDateTime(DemandMonth).AddMonths(-2).ToString("yyyy-MM");//上上月
            }
            if (request.OrgType == "3")//分部
            {
                where += " and TbGq.ParentCompanyCode='" + request.SiteCode + "'";
            }
            if (request.OrgType == "4")//工区
            {
                where += " and TbGq.CompanyCode='" + request.SiteCode + "'";
            }
            string sql = @"select * from (select cp2.CompanyCode,cp2.CompanyFullName,cp2.ParentCompanyCode from (select SiteCode from TbWorkOrder where 
            (CONVERT(varchar(7) ,InsertTime, 120)='" + dateTime2 + @"' or CONVERT(varchar(7) ,InsertTime, 120)='" + dateTime3 + @"' or CONVERT(varchar(7) ,InsertTime, 120)='" + dateTime1 + @"') and (ISNULL(@ProjectId,'')='' or ProjectId=@ProjectId) and (ISNULL(@ProcessFactoryCode,'')='' or ProcessFactoryCode=@ProcessFactoryCode)
            group by SiteCode) TbWO
            left join TbCompany cp on TBWO.SiteCode=cp.CompanyCode
            left join TbCompany cp2 on cp2.CompanyCode=cp.ParentCompanyCode
            group by cp2.CompanyCode,cp2.CompanyFullName,cp2.ParentCompanyCode
            union all
            select rmdp.WorkAreaCode,cp.CompanyFullName,cp.ParentCompanyCode
            from TbRawMaterialMonthDemandPlan rmdp
            left join TbCompany cp on cp.CompanyCode=rmdp.WorkAreaCode
            where (rmdp.DemandMonth='" + dateTime2 + @"' or rmdp.DemandMonth='" + dateTime3 + @"' or rmdp.DemandMonth='" + dateTime1 + @"') and (ISNULL(@ProjectId,'')='' or ProjectId=@ProjectId) and (ISNULL(@ProcessFactoryCode,'')='' or ProcessFactoryCode=@ProcessFactoryCode)) TbGq where 1=1 " + where + @" group by TbGq.CompanyCode,TbGq.CompanyFullName,TbGq.ParentCompanyCode";
            DataTable dt = Db.Context.FromSql(sql)
                .AddInParameter("@ProjectId", DbType.String, request.ProjectId)
                .AddInParameter("@ProcessFactoryCode", DbType.String, request.ProcessFactoryCode).ToDataTable();
            return dt;
        }

        #endregion


        #region App接口

        /// <summary>
        /// 获取钢筋类型
        /// </summary>
        /// <returns></returns>
        public DataTable GetRebarType()
        {
            string sql = @"select DictionaryCode as RebarType,DictionaryText as RebarTypeName from TbSysDictionaryData where FDictionaryCode='RebarType'";
            DataTable dt = Db.Context.FromSql(sql).ToDataTable();
            return dt;
        }

        public DataTable Img1New(RawMonthDemPlanRequest request)
        {
            string where = "";
            string DemandMonth = Convert.ToDateTime(DateTime.Now).ToString("yyyy-MM");
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where += " and a.ProjectId='" + request.ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where += " and a.ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 4);//工区
                string workAreaStr = string.Join("','", WorkAreaList);
                where += " and a.WorkAreaCode in('" + workAreaStr + "')";
            }
            if (request.HistoryMonth.HasValue)
            {
                DemandMonth = Convert.ToDateTime(request.HistoryMonth).ToString("yyyy-MM");
            }
            where += " and a.DemandMonth='" + DemandMonth + "'";
            string sql = @"select Tb.*,ISNULL(Tb.PlanTotal,0)-ISNULL(Tb.BatchPlanTotal,0) as PlanCe,ISNULL(Tb.HasSupplierTotal,0)-ISNULL(Tb.BatchPlanTotal,0) as SupplierCe,case when ISNULL(Tb.PlanTotal,0)<(isnull(Tb.BatchPlanTotal,0)-ISNULL(Tb.BatchPlanTotal,0)*0.1) then '计划不足' when ISNULL(Tb.PlanTotal,0)>(isnull(Tb.BatchPlanTotal,0)+ISNULL(Tb.BatchPlanTotal,0)*0.1) then '计划过多' else '计划合理' end IsPlanType,case when ISNULL(Tb.HasSupplierTotal,0)-ISNULL(Tb.BatchPlanTotal,0)<0 then '供货不足' when ISNULL(Tb.HasSupplierTotal,0)>=ISNULL(Tb.BatchPlanTotal,0) and ISNULL(Tb.HasSupplierTotal,0)<=(ISNULL(Tb.BatchPlanTotal,0)*0.05) then '供货合理' when ISNULL(Tb.HasSupplierTotal,0)>(ISNULL(Tb.BatchPlanTotal,0)*0.05) then '供货过多' end IsSupplierType  from (select '建筑钢筋' as RebarTypeName,isnull(sum(a.PlanTotal),0)+isnull(SUM(d.SupplyPlanNum),0) as PlanTotal,isnull(sum(e.PcJhCount),0) as PcJhCount,isnull(sum(b.BatchPlanTotal),0) as BatchPlanTotal,isnull(sum(f.GhCount),0) as GhCount,isnull(sum(c.HasSupplierTotal),0) as HasSupplierTotal from TbRawMaterialMonthDemandPlan a
left join (select a.DemandPlanCode,sum(isnull(a.BatchPlanTotal,0)) as BatchPlanTotal from (select case when b.DemandPlanCode is null then a.RawMaterialDemandNum else b.DemandPlanCode end DemandPlanCode,a.BatchPlanTotal from TbFactoryBatchNeedPlan a
left join TbRawMaterialMonthDemandSupplyPlan b on a.RawMaterialDemandNum=b.SupplyPlanCode) a group by a.DemandPlanCode) b on a.DemandPlanCode=b.DemandPlanCode
left join (select a.DemandPlanCode,sum(isnull(a.HasSupplierTotal,0)) as HasSupplierTotal from (select case when b.DemandPlanCode is null then a.RawMaterialDemandNum else b.DemandPlanCode end DemandPlanCode,a.HasSupplierTotal from TbSupplyList a
left join TbRawMaterialMonthDemandSupplyPlan b on a.RawMaterialDemandNum=b.SupplyPlanCode) a group by a.DemandPlanCode) c on a.DemandPlanCode=c.DemandPlanCode
left join TbRawMaterialMonthDemandSupplyPlan d on a.DemandPlanCode=d.DemandPlanCode
left join (select COUNT(1) PcJhCount,e.RawMaterialDemandNum from TbFactoryBatchNeedPlan e where e.Examinestatus!='已退回' and e.Examinestatus!='已撤销' group by e.RawMaterialDemandNum) e on e.RawMaterialDemandNum=a.DemandPlanCode
left join (select COUNT(1) as GhCount,a.RawMaterialDemandNum from TbSupplyList a
left join(select ThisTime,BatchPlanNum from TbSupplyListDetailHistory group by ThisTime,BatchPlanNum) b on a.BatchPlanNum=b.BatchPlanNum
where b.ThisTime is not null and a.RawMaterialDemandNum!='' group by a.RawMaterialDemandNum) f on a.DemandPlanCode=f.RawMaterialDemandNum
where 1=1 and a.Examinestatus='审核完成' and a.RebarType='BuildingSteel' " + where + @"
union all
select '型钢' as RebarTypeName,isnull(sum(a.PlanTotal),0)+isnull(SUM(d.SupplyPlanNum),0) as PlanTotal,isnull(sum(e.PcJhCount),0) as PcJhCount,isnull(sum(b.BatchPlanTotal),0) as BatchPlanTotal,isnull(sum(f.GhCount),0) as GhCount,isnull(sum(c.HasSupplierTotal),0) as HasSupplierTotal from TbRawMaterialMonthDemandPlan a
left join (select a.DemandPlanCode,sum(isnull(a.BatchPlanTotal,0)) as BatchPlanTotal from (select case when b.DemandPlanCode is null then a.RawMaterialDemandNum else b.DemandPlanCode end DemandPlanCode,a.BatchPlanTotal from TbFactoryBatchNeedPlan a
left join TbRawMaterialMonthDemandSupplyPlan b on a.RawMaterialDemandNum=b.SupplyPlanCode) a group by a.DemandPlanCode) b on a.DemandPlanCode=b.DemandPlanCode
left join (select a.DemandPlanCode,sum(isnull(a.HasSupplierTotal,0)) as HasSupplierTotal from (select case when b.DemandPlanCode is null then a.RawMaterialDemandNum else b.DemandPlanCode end DemandPlanCode,a.HasSupplierTotal from TbSupplyList a
left join TbRawMaterialMonthDemandSupplyPlan b on a.RawMaterialDemandNum=b.SupplyPlanCode) a group by a.DemandPlanCode) c on a.DemandPlanCode=c.DemandPlanCode
left join TbRawMaterialMonthDemandSupplyPlan d on a.DemandPlanCode=d.DemandPlanCode
left join (select COUNT(1) PcJhCount,e.RawMaterialDemandNum from TbFactoryBatchNeedPlan e where e.Examinestatus!='已退回' and e.Examinestatus!='已撤销' group by e.RawMaterialDemandNum) e on e.RawMaterialDemandNum=a.DemandPlanCode
left join (select COUNT(1) as GhCount,a.RawMaterialDemandNum from TbSupplyList a
left join(select ThisTime,BatchPlanNum from TbSupplyListDetailHistory group by ThisTime,BatchPlanNum) b on a.BatchPlanNum=b.BatchPlanNum
where b.ThisTime is not null and a.RawMaterialDemandNum!='' group by a.RawMaterialDemandNum) f on a.DemandPlanCode=f.RawMaterialDemandNum
where 1=1 and a.Examinestatus='审核完成' and a.RebarType='SectionSteel' " + where + @") Tb";
            DataTable dt = Db.Context.FromSql(sql + where).ToDataTable();
            return dt;
        }
        #endregion
    }
}
