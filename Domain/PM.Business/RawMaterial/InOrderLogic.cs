using Dos.ORM;
using PM.Business.Production;
using PM.Business.ShortMessage;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;

namespace PM.Business.RawMaterial
{
    /// <summary>
    /// 原材料到货入库
    /// </summary>
    public class InOrderLogic
    {
        private readonly TbWorkOrderLogic _workOrderLogic = new TbWorkOrderLogic();
        //发送短信
        CensusdemoTask ct = new CensusdemoTask();

        #region 新增数据

        /// <summary>
        /// 新增数据
        /// </summary>
        public AjaxResult Insert(TbInOrder model, List<TbInOrderItem> items)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            model.InsertUserCode = OperatorProvider.Provider.CurrentUser.UserCode;
            model.Examinestatus = "未发起";
            model.SampleOrderState = "取样未完成";
            model.InOrderState = "完全入库";
            List<TbInOrder> InOrders = new List<TbInOrder>();
            if (!string.IsNullOrEmpty(model.BatchPlanCode))
            {
                var finishRet = IsFinish(model.BatchPlanCode, items);
                if (finishRet.state.ToString() != ResultType.success.ToString())
                    return finishRet;
                model.InOrderState = (bool)finishRet.data ? "完全入库" : "部分入库";
                //完全入库改变所有入库单状态为完全入库 20181224fmm
                if ((bool)finishRet.data)
                {
                    //查找所有批次计划单相关入库单
                    InOrders = Repository<TbInOrder>.Query(p => p.BatchPlanCode == model.BatchPlanCode).ToList();
                    if (InOrders.Any())
                    {
                        InOrders.ForEach(x =>
                        {
                            x.InOrderState = "完全入库";
                        });
                    }
                }
            }
            //查找库存记录
            var inOrderDeductionRecords = GetStockRecord(model, items);
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //添加信息
                    var id = Repository<TbInOrder>.Insert(trans, model);
                    //添加明细信息
                    Repository<TbInOrderItem>.Insert(trans, items);
                    //修改入库单状态为完全入库
                    if (InOrders.Any())
                        Repository<TbInOrder>.Update(trans, InOrders);
                    //入库抵扣记录
                    if (inOrderDeductionRecords.Any())
                        Repository<TbInOrderDeductionRecord>.Insert(trans, inOrderDeductionRecords);
                    trans.Commit();
                    return AjaxResult.Success(id);
                }
            }
            catch (Exception)
            {
                return AjaxResult.Error();
            }
        }

        #endregion

        #region 修改数据

        /// <summary>
        /// 修改数据
        /// </summary>
        public AjaxResult Update(TbInOrder model, List<TbInOrderItem> items)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            var anyRet = AnyInfo(model.ID);
            if (anyRet.state.ToString() != ResultType.success.ToString())
                return anyRet;
            model.SampleOrderState = "取样未完成";
            model.InOrderState = "完全入库";
            List<TbInOrder> InOrders = new List<TbInOrder>();
            if (!string.IsNullOrEmpty(model.BatchPlanCode))
            {
                var finishRet = IsFinish(model.BatchPlanCode, items, model.InOrderCode);
                if (finishRet.state.ToString() != ResultType.success.ToString())
                    return finishRet;
                model.InOrderState = (bool)finishRet.data ? "完全入库" : "部分入库";
                //完全入库改变所有入库单状态为完全入库 20181224fmm
                if ((bool)finishRet.data)
                {
                    //查找所有批次计划单相关入库单
                    InOrders = Repository<TbInOrder>.Query(p => p.BatchPlanCode == model.BatchPlanCode
                                                           && p.InOrderCode != model.InOrderCode).ToList();
                    if (InOrders.Any())
                    {
                        InOrders.ForEach(x =>
                        {
                            x.InOrderState = "完全入库";
                        });
                    }
                }
            }
            //查找库存记录
            var inOrderDeductionRecords = GetStockRecord(model, items);
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())//使用事务
                {
                    //修改信息
                    Repository<TbInOrder>.Update(trans, model, p => p.ID == model.ID);
                    if (items.Count > 0)
                    {
                        //删除历史明细信息
                        Repository<TbInOrderItem>.Delete(trans, p => p.InOrderCode == model.InOrderCode);
                        //删除入库抵扣记录
                        Repository<TbInOrderDeductionRecord>.Delete(trans, p => p.InOrderCode == model.InOrderCode);
                        //添加明细信息
                        Repository<TbInOrderItem>.Insert(trans, items);
                        //修改入库单状态为完全入库
                        if (InOrders.Any())
                            Repository<TbInOrder>.Update(trans, InOrders);
                        //入库抵扣记录
                        if (inOrderDeductionRecords.Any())
                            Repository<TbInOrderDeductionRecord>.Insert(trans, inOrderDeductionRecords);
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
                if (anyRet.state != ResultType.success.ToString())
                    return anyRet;
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //删除信息
                    var count = Repository<TbInOrder>.Delete(trans, p => p.ID == keyValue);
                    //删除明细信息
                    Repository<TbInOrderItem>.Delete(trans, p => p.InOrderCode == ((TbInOrder)anyRet.data).InOrderCode);
                    //删除入库抵扣记录
                    Repository<TbInOrderDeductionRecord>.Delete(trans, p => p.InOrderCode == ((TbInOrder)anyRet.data).InOrderCode);
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
        /// <param name="keyValue">数据Id</param>
        /// <returns></returns>
        public Tuple<object, object> FindEntity(int keyValue)
        {
            var inOrder = Repository<TbInOrder>.First(p => p.ID == keyValue);
            var BranchCode = Repository<TbCompany>.First(p => p.CompanyCode == inOrder.WorkAreaCode);
            var ret = Db.Context.From<TbInOrder>()
                .Select(
                    TbInOrder._.All
                  , TbUser._.UserName.As("InsertUserName")
                  , TbCompany._.CompanyFullName.As("SiteName")
                  , TbSysDictionaryData._.DictionaryText.As("RebarTypeName")
                  , TbStorage._.StorageName)
                  .AddSelect(Db.Context.From<TbUser>().Select(p => p.UserName)
                  .Where(TbUser._.UserCode == TbInOrder._.UserCode), "DeliverUser")
                  .AddSelect(Db.Context.From<TbUser>().Select(p => p.UserName)
                  .Where(TbUser._.UserCode == TbInOrder._.SupplierCode), "SiteUser")
                  .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                  .Where(TbCompany._.CompanyCode == TbInOrder._.WorkAreaCode), "WorkAreaName")
                  .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                  .Where(TbCompany._.CompanyCode == BranchCode.ParentCompanyCode), "BranchName")
                  .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                  .LeftJoin<TbStorage>((a, c) => a.StorageCode == c.StorageCode)
                  .LeftJoin<TbCompany>((a, c) => a.SiteCode == c.CompanyCode)
                  .LeftJoin<TbSysDictionaryData>((a, c) => a.RebarType == c.DictionaryCode)
                  .Where(p => p.ID == keyValue).ToDataTable();
            if (ret == null || ret.Rows.Count == 0)
                return new Tuple<object, object>(null, null);
            //查找明细信息
            string sql = @"select a.*,case when a.ChackState=2 or a.ChackState=3 then a.PassCount else 0 end NoChackCount,d.BatchPlanQuantity,d.HasSupplier,c.DictionaryText as MeasurementUnitText,b.ThisTimeCount as InOrdeCount,b.ThisTime,case when e.SupplierName is null then a.SupplierCode else e.SupplierName end SupplierName from TbInOrderItem a
left join TbSupplyListDetailHistory b on a.BatchPlanItemNewCode=b.BatchPlanItemNewCode
left join TbSupplyListDetail d on a.BatchPlanItemId=d.BatchPlanItemId
left join TbSysDictionaryData c on a.MeasurementUnit=c.DictionaryCode
left join TbSupplier e on a.SupplierCode=e.SupplierCode
where a.InOrderCode=@InOrderCode";

            var items = Db.Context.FromSql(sql)
                //.AddInParameter("BatchPlanNum", DbType.String, ret.Rows[0]["BatchPlanCode"])
                .AddInParameter("InOrderCode", DbType.String, ret.Rows[0]["InOrderCode"])
                .ToDataTable();
            return new Tuple<object, object>(ret, items);
        }

        /// <summary>
        /// 获取数据列表(分页)
        /// </summary>
        public PageModel GetDataListForPage(InOrderRequest request)
        {

            #region 模糊搜索条件


            string where = "where 1=1 and  ino.BatchPlanCode!='' and ino.BatchPlanCode is not null";
            if (!string.IsNullOrWhiteSpace(request.SupplierCode))
            {
                where += (" and ino.SupplierCode='" + request.SupplierCode + "'");
            }
            if (!string.IsNullOrWhiteSpace(request.StorageCode))
            {
                where += (" and ino.StorageCode='" + request.StorageCode + "'");
            }
            if (!string.IsNullOrWhiteSpace(request.SiteName))
            {
                where += (" and cp1.CompanyFullName like '%" + request.SiteName + "%'");
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);//站点
                List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 4);//工区
                string siteStr = string.Join("','", SiteList);
                string workAreaStr = string.Join("','", WorkAreaList);
                where += (" and (ino.SiteCode in('" + siteStr + "') or ino.WorkAreaCode in('" + workAreaStr + "'))");
            }
            if (!string.IsNullOrWhiteSpace(request.InOrderCode))
            {
                where += (" and ino.InOrderCode like '%" + request.InOrderCode + "%'");
            }
            if (!string.IsNullOrEmpty(request.ProjectId))
                where += (" and ino.ProjectId='" + request.ProjectId + "'");

            if (!string.IsNullOrEmpty(request.ProcessFactoryCode))
                where += (" and st.ProcessFactoryCode='" + request.ProcessFactoryCode + "'");
            if (!string.IsNullOrEmpty(request.BatchPlanCode))
            {
                where += (" and ino.BatchPlanCode='" + request.BatchPlanCode + "'");
            }
            if (!string.IsNullOrWhiteSpace(request.SampleOrderState))
            {
                where += (" and ino.SampleOrderState='" + request.SampleOrderState + "'");
            }
            if (!string.IsNullOrWhiteSpace(request.ChackResult))
            {
                if (request.ChackResult != "0")
                {
                    where += " and ino.ChackResult=" + request.ChackResult + "";
                }
            }
            if (request.HistoryMonth.HasValue)
            {
                where += " and  YEAR(ino.InsertTime)=" + request.HistoryMonth.Value.Year + " and MONTH(ino.InsertTime)=" + request.HistoryMonth.Value.Month + "";
            }
            if (!string.IsNullOrWhiteSpace(request.DemandPlanCode))
            {
                where += " and (rmsp.DemandPlanCode='" + request.DemandPlanCode + "' or fbnp.RawMaterialDemandNum='" +
                         request.DemandPlanCode + "')";
            }
            #endregion

            #region 数据权限

            ////数据权限
            //var authorizaModel = new AuthorizeLogic().CheckAuthoriza(new AuthorizationParameterModel("InOrder"));
            //if (authorizaModel.IsAuthorize)
            //{
            //    if (authorizaModel.Ids.Count > 0 && authorizaModel.UserCodes.Count > 0)
            //        where.And(d => d.InsertUserCode.In(authorizaModel.UserCodes) || d.ID.In(authorizaModel.Ids));
            //    else if (authorizaModel.Ids.Count > 0)
            //        where.Or(d => d.ID.In(authorizaModel.Ids));
            //    else if (authorizaModel.UserCodes.Count > 0)
            //        where.And(d => d.InsertUserCode.In(authorizaModel.UserCodes));
            //}
            //if (!string.IsNullOrEmpty(request.ProjectId))
            //    where.And(p => p.ProjectId == request.ProjectId);
            #endregion

            try
            {
                var sql = @"select ino.*,case when ISNULL(ino.ChackResult,0)=0 then '' when ISNULL(ino.ChackResult,0)=1 then '合格' when ISNULL(ino.ChackResult,0)=2 then '不合格' when ISNULL(ino.ChackResult,0)=3 then '部分合格' end ChackResultName,ur.UserName,ur1.UserName as DeliverUser,st.StorageName,sdd.DictionaryText as RebarTypeName,sp.SupplierName,cp1.CompanyFullName as SiteName,cp2.CompanyFullName as WorkAreaName,cp3.CompanyFullName as BranchName,case when rmsp.DemandPlanCode is null then fbnp.RawMaterialDemandNum else rmsp.DemandPlanCode end DemandPlanCode,inoi.IsTh,sap.NoJcBhgCount from TbInOrder ino
							left join(select '退回' as IsTh,InOrderCode from TbInOrderItem where ReturnTime is not null group by InOrderCode) inoi on ino.InOrderCode=inoi.InOrderCode
                            left join TbUser ur on ino.InsertUserCode=ur.UserCode
                            left join TbUser ur1 on ino.UserCode=ur1.UserCode
                            left join TbStorage st on ino.StorageCode=st.StorageCode
                            left join TbSysDictionaryData sdd on ino.RebarType=sdd.DictionaryCode
                            left join TbCompany cp1 on ino.SiteCode=cp1.CompanyCode
                            left join TbCompany cp2 on ino.WorkAreaCode=cp2.CompanyCode
                            left join TbCompany cp3 on cp2.ParentCompanyCode=cp3.CompanyCode
                            left join TbSupplier sp on ino.SupplierCode=sp.SupplierCode
                            left join TbFactoryBatchNeedPlan fbnp on ino.BatchPlanCode=fbnp.BatchPlanNum
							left join TbRawMaterialMonthDemandSupplyPlan rmsp on fbnp.RawMaterialDemandNum=rmsp.SupplyPlanCode
							left join (	select a.InOrderCode,SUM(c.PassCount) as NoJcBhgCount from TbSampleOrder a
							left join TbSampleOrderItem b on a.SampleOrderCode=b.SampleOrderCode
							left join TbInOrderItem c on b.InOrderItemId=c.ID
							where b.ChackState=2 group by a.InOrderCode) sap on ino.InOrderCode=sap.InOrderCode ";
                //参数化
                List<Dos.ORM.Parameter> para = new List<Dos.ORM.Parameter>();
                var data = Repository<TbInOrder>.FromSqlToPageTable(sql + where, para, request.rows, request.page, "InOrderCode", "desc");
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 获取退回数据
        /// </summary>
        /// <returns></returns>
        public DataTable GetReturnDataList(int keyValue)
        {
            var where = new Where<TbInOrderItem>();
            where.And(p => p.ChackState == 2);
            where.And<TbInOrder>((a, c) => c.ID == keyValue);
            var data = Db.Context.From<TbInOrderItem>()
                    .Select(
                      TbInOrderItem._.All
                    , TbSysDictionaryData._.DictionaryText.As("MeasurementUnitText"))
                  .LeftJoin<TbInOrder>((a, c) => a.InOrderCode == c.InOrderCode)
                  .LeftJoin<TbSysDictionaryData>((a, c) => a.MeasurementUnit == c.DictionaryCode)
                  .Where(where).ToDataTable();
            return data;
        }

        /// <summary>
        /// 判断信息是否可操作
        /// </summary>
        /// <returns></returns>
        public AjaxResult AnyInfo(int keyValue)
        {
            var inOrder = Repository<TbInOrder>.First(p => p.ID == keyValue);
            if (inOrder == null)
                return AjaxResult.Warning("信息不存在");
            if (inOrder.Examinestatus != "未发起" && inOrder.Examinestatus != "已退回")
                return AjaxResult.Warning("信息正在审核中或已审核完成,不能进行此操作");

            return AjaxResult.Success(inOrder);
        }

        /// <summary>
        /// 导出数据源
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public DataTable GetExportList(InOrderRequest request)
        {
            #region 模糊搜索条件

            string where = "where 1=1 ";
            if (!string.IsNullOrWhiteSpace(request.SupplierCode))
            {
                where += (" and ino.SupplierCode='" + request.SupplierCode + "'");
            }
            if (!string.IsNullOrWhiteSpace(request.StorageCode))
            {
                where += (" and ino.StorageCode='" + request.StorageCode + "'");
            }
            if (!string.IsNullOrWhiteSpace(request.SiteName))
            {
                where += (" and cp1.CompanyFullName like '%" + request.SiteName + "%'");
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);//站点
                List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 4);//工区
                string siteStr = string.Join("','", SiteList);
                string workAreaStr = string.Join("','", WorkAreaList);
                where += (" and (ino.SiteCode in('" + siteStr + "') or ino.WorkAreaCode in('" + workAreaStr + "'))");
            }
            if (!string.IsNullOrWhiteSpace(request.InOrderCode))
            {
                where += (" and ino.InOrderCode like '%" + request.InOrderCode + "%'");
            }
            if (!string.IsNullOrEmpty(request.ProjectId))
                where += (" and ino.ProjectId='" + request.ProjectId + "'");

            if (!string.IsNullOrEmpty(request.ProcessFactoryCode))
                where += (" and st.ProcessFactoryCode='" + request.ProcessFactoryCode + "'");
            if (!string.IsNullOrEmpty(request.BatchPlanCode))
            {
                where += (" and ino.BatchPlanCode='" + request.BatchPlanCode + "'");
            }
            if (!string.IsNullOrWhiteSpace(request.SampleOrderState))
            {
                where += (" and ino.SampleOrderState='" + request.SampleOrderState + "'");
            }
            if (!string.IsNullOrWhiteSpace(request.ChackResult))
            {
                if (request.ChackResult != "0")
                {
                    where += " and ino.ChackResult=" + request.ChackResult + "";
                }
            }
            if (request.HistoryMonth.HasValue)
            {
                where += " and  YEAR(ino.InsertTime)=" + request.HistoryMonth.Value.Year + " and MONTH(ino.InsertTime)=" + request.HistoryMonth.Value.Month + "";
            }

            //排序
            where += " order by InOrderCode desc";

            #endregion

            try
            {
                var sql = @"select ino.*,ur.UserName,ur1.UserName as DeliverUser,st.StorageName,sdd.DictionaryText as RebarTypeName,sp.SupplierName,cp1.CompanyFullName as SiteName,cp2.CompanyFullName as WorkAreaName,cp3.CompanyFullName as BranchName,case when ino.ChackResult=1 then '合格' when ino.ChackResult=2 then '不合格' when ino.ChackResult=3 then '部分合格' else '' end ChackResultNew from TbInOrder ino
                            left join TbUser ur on ino.InsertUserCode=ur.UserCode
                            left join TbUser ur1 on ino.UserCode=ur1.UserCode
                            left join TbStorage st on ino.StorageCode=st.StorageCode
                            left join TbSysDictionaryData sdd on ino.RebarType=sdd.DictionaryCode
                            left join TbCompany cp1 on ino.SiteCode=cp1.CompanyCode
                            left join TbCompany cp2 on ino.WorkAreaCode=cp2.CompanyCode
                            left join TbCompany cp3 on cp2.ParentCompanyCode=cp3.CompanyCode
                            left join TbSupplier sp on ino.SupplierCode=sp.SupplierCode
                            left join TbFactoryBatchNeedPlan fbnp on ino.BatchPlanCode=fbnp.BatchPlanNum ";
                var data = Db.Context.FromSql(sql + where).ToDataTable();
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region 质检报告

        /// <summary>
        /// 质检报告
        /// </summary>
        public AjaxResult SubmitTestReport(string enclosure, List<TbInOrderItem> items)
        {
            if (items == null || items.Count < 1)
                return AjaxResult.Warning("参数错误");
            var inOrderCode = items[0].InOrderCode;
            //查找入库单
            var inOrder = Repository<TbInOrder>.First(p => p.InOrderCode == inOrderCode);
            if (inOrder == null)
                return AjaxResult.Warning("入库单信息不存在");
            if (!string.IsNullOrEmpty(inOrder.Enclosure))
                enclosure = "," + enclosure;
            inOrder.Enclosure += enclosure;
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //修改入库单附件信息
                    Repository<TbInOrder>.Update(trans, inOrder);
                    //修改明细信息
                    Repository<TbInOrderItem>.Update(trans, items);
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

        #region 退回处理

        /// <summary>
        /// 退回处理
        /// </summary>
        public AjaxResult ReturnSubmit(List<TbInOrderItem> items)
        {
            if (items == null || items.Count < 1)
                return AjaxResult.Warning("参数错误");
            var inOrderCode = items[0].InOrderCode;
            //查找入库单
            var inOrderItem = Repository<TbInOrderItem>.Query(p => p.InOrderCode == inOrderCode && p.ChackState == 2).ToList();
            if (!inOrderItem.Any())
                return AjaxResult.Warning("所选入库批次中无可退回的‘不合格’材料");
            inOrderItem.ForEach(x =>
            {
                var t = items.FirstOrDefault(p => p.ID == x.ID);
                if (t != null)
                {
                    x.ReturnTime = t.ReturnTime;
                    x.ChackState = 3;
                }
            });
            //原材料到货入库不合格材料退回通知
            var inOrderModel = Repository<TbInOrder>.First(p => p.InOrderCode == inOrderCode);
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //修改明细信息
                    Repository<TbInOrderItem>.Update(trans, inOrderItem);
                    trans.Commit();
                    //调用短信通知消息
                    InOrderNoPassSendNotice(inOrderModel.ID, inOrderCode);
                    return AjaxResult.Success();
                }
            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.ToString());
            }
        }

        #endregion

        #region Private

        /// <summary>
        /// 判断是否完全入库
        /// </summary>
        /// <param name="batchPlanCode"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        private AjaxResult IsFinish(string batchPlanCode, List<TbInOrderItem> items, string inOrderCode = "")
        {
            //查找批次需求计划信息
            var batchNeedPlan = Repository<TbFactoryBatchNeedPlan>.First(p => p.BatchPlanNum == batchPlanCode);
            if (batchNeedPlan == null)
                return AjaxResult.Error("批次需求计划信息不存在");
            //查找批次计划相关的入库单
            decimal count = 0;//历史入库数量
            var inOrders = Repository<TbInOrder>.Query(p => p.BatchPlanCode == batchPlanCode).ToList();
            if (inOrders.Any())
            {
                if (!string.IsNullOrEmpty(inOrderCode))
                {
                    count = inOrders.Where(p => p.InOrderCode != inOrderCode).Sum(p => p.InCount);
                }
                else
                {
                    count = inOrders.Sum(p => p.InCount);
                }
            }
            var batchPlanQuantity = batchNeedPlan.BatchPlanTotal;//批次计划数量
            var passCount = items.Sum(p => p.PassCount);//当前入库数量
            if (batchPlanQuantity > (passCount + count))
                return AjaxResult.Success(false);
            return AjaxResult.Success(true);
        }

        /// <summary>
        /// 判断是否有库存记录
        /// </summary>
        /// <param name="items"></param>
        public List<TbInOrderDeductionRecord> GetStockRecord(TbInOrder model, List<TbInOrderItem> items)
        {
            var deductionRecordList = new List<TbInOrderDeductionRecord>();
            var materialCodeList = items.Select(p => p.MaterialCode).Distinct().ToList();
            var stockRecordList = Repository<TbRawMaterialStockRecord>.Query(p => p.MaterialCode.In(materialCodeList)
                                                            && p.ProjectId == model.ProjectId
                                                            && p.WorkAreaCode == model.WorkAreaCode).ToList();
            var stockRecordListTep = stockRecordList.Where(p => (p.LockCount + p.UseCount) < 0).ToList();
            int index = 1;
            items.ForEach(x =>
            {
                x.StockRecordId = 0;
                x.DeductionCount = x.PassCount;
                x.ItemId = model.InOrderCode + "_" + index++;
                if (stockRecordListTep.Any())
                {
                    var stockRecordIds = deductionRecordList.Select(p => p.StockRecordId).ToList();
                    var stockRecord = stockRecordListTep.Where(p => p.MaterialCode == x.MaterialCode && !stockRecordIds.Contains(p.ID)).ToList();
                    if (stockRecord.Any())
                    {
                        x.StockRecordId = -1;
                        //入库时无论填写了站点是否，先抵扣站点，再抵扣工区
                        var stockRecordTep = stockRecord.OrderByDescending(p => p.SiteCode).ThenByDescending(p => p.InsertTime).ToList();
                        if (!string.IsNullOrEmpty(model.SiteCode))
                        {
                            stockRecordTep = stockRecord.Where(p => p.SiteCode == model.SiteCode).ToList();
                            stockRecordTep.AddRange(stockRecord.Where(p => p.SiteCode != model.SiteCode).OrderByDescending(p => p.SiteCode).ThenByDescending(p => p.InsertTime).ToList());
                        }
                        //查找到可抵扣的原材料库存
                        decimal? deductionCount = x.PassCount;
                        foreach (var item in stockRecordTep)
                        {
                            var deductionRecord = new TbInOrderDeductionRecord()
                            {
                                InOrderCode = x.InOrderCode,
                                StockRecordId = item.ID,
                                InOrderItemId = x.ItemId,
                                IsDel = 0
                            };
                            var surplusCount = deductionCount + (item.UseCount + item.LockCount.Value);//剩余数量
                            if (surplusCount <= 0)
                            {
                                deductionRecord.DeductionCount = deductionCount;
                                if (surplusCount < 0)
                                {
                                    //抵扣后还为负数炉批号替换掉
                                    if (!string.IsNullOrEmpty(item.SiteCode))
                                    {
                                        deductionRecord.BatchNumber = x.BatchNumber;
                                    }
                                }
                                deductionRecordList.Add(deductionRecord);
                                deductionCount = surplusCount;
                                break;
                            }
                            else
                            {
                                deductionRecord.DeductionCount = Math.Abs(item.UseCount + item.LockCount.Value);
                                deductionRecordList.Add(deductionRecord);
                            }
                            deductionCount = surplusCount;
                        }
                        // 剩余重量放入工区
                        if (deductionCount > 0)
                        {
                            x.StockRecordId = -2;
                            x.DeductionCount = deductionCount;
                        }
                    }
                }
                //有批次计划的入库单先锁定
                if (!string.IsNullOrEmpty(model.BatchPlanCode))
                {
                    x.StockRecordId = -3;
                    x.ChackState = 0;
                }
            });
            return deductionRecordList;
        }

        #endregion

        /// <summary>
        /// 获取数据列表(供应商)
        /// </summary>
        public PageModel GetSupplierDataList(InOrderRequest request)
        {
            #region 模糊搜索条件

            var where = new Where<TbSupplier>();
            if (!string.IsNullOrWhiteSpace(request.keyword))
            {
                where.And(p => p.SupplierName.Like(request.keyword));
            }
            #endregion
            try
            {
                var data = Repository<TbSupplier>.QueryPage(where, p => p.ID, "desc", request.rows, request.page);
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 获取数据列表(批次计划)
        /// </summary>
        //public PageModel GetBatchPlanDataList(InOrderRequest request)
        //{
        //    #region 模糊搜索条件

        //    var where = new Where<TbFactoryBatchNeedPlan>();
        //    if (!string.IsNullOrWhiteSpace(request.keyword))
        //    {
        //        where.And(p => p.BatchPlanNum.Like(request.keyword));
        //    }
        //    where.And(p => p.Examinestatus == "审核完成");
        //    where.And(TbFactoryBatchNeedPlan._.BatchPlanNum
        //        .SubQueryNotIn(Db.Context.From<TbInOrder>().Select(p => p.BatchPlanCode)
        //        .Where(p => p.InOrderState == "完全入库")));
        //    where.And(TbFactoryBatchNeedPlan._.BatchPlanNum
        //        .SubQueryNotIn(Db.Context.From<TbSupplyList>().Select(p => p.BatchPlanNum)
        //        .Where(p => p.StateCode == "未供货")));
        //    if (!string.IsNullOrEmpty(request.ProjectId))
        //        where.And(p => p.ProjectId == request.ProjectId);

        //    if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
        //    {
        //        where.And(p => p.ProcessFactoryCode == request.ProcessFactoryCode);
        //    }
        //    if (!string.IsNullOrWhiteSpace(request.ProjectId))
        //    {
        //        where.And(p => p.ProjectId == request.ProjectId);
        //    }
        //    if (!string.IsNullOrWhiteSpace(request.SiteCode))
        //    {
        //        List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);//站点
        //        List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 4);//工区
        //        where.And(p => p.SiteCode.In(SiteList) || p.WorkAreaCode.In(WorkAreaList));
        //    }

        //    #endregion

        //    try
        //    {
        //        var data = Db.Context.From<TbFactoryBatchNeedPlan>().Select(
        //            TbFactoryBatchNeedPlan._.BatchPlanNum,
        //            TbFactoryBatchNeedPlan._.SiteCode,
        //            TbFactoryBatchNeedPlan._.SteelsTypeCode,
        //            TbFactoryBatchNeedPlan._.ContactWay,
        //            TbFactoryBatchNeedPlan._.DeliveryPlace,
        //            TbFactoryBatchNeedPlan._.Acceptor,
        //            TbFactoryBatchNeedPlan._.SupplierCode,
        //            TbFactoryBatchNeedPlan._.ProcessFactoryCode,
        //            TbFactoryBatchNeedPlan._.WorkAreaCode,
        //            TbFactoryBatchNeedPlan._.BranchCode,
        //            TbSysDictionaryData._.DictionaryText.As("RebarTypeName"),
        //            TbCompany._.CompanyFullName.As("SiteName"),
        //            TbUser._.UserName.As("DeliverUser"),
        //            TbSupplier._.SupplierName,
        //            TbStorage._.StorageCode,
        //            TbStorage._.StorageName)
        //            .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
        //            .Where(TbCompany._.CompanyCode == TbFactoryBatchNeedPlan._.WorkAreaCode), "WorkAreaName")
        //            .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
        //            .Where(TbCompany._.CompanyCode == TbFactoryBatchNeedPlan._.BranchCode), "BranchName")
        //            .LeftJoin<TbSysDictionaryData>((a, c) => a.SteelsTypeCode == c.DictionaryCode)
        //            .LeftJoin<TbUser>((a, c) => a.Acceptor == c.UserCode)
        //            .LeftJoin<TbCompany>((a, c) => a.SiteCode == c.CompanyCode)
        //            .LeftJoin<TbSupplier>((a, c) => a.SupplierCode == c.SupplierCode)
        //            .LeftJoin<TbStorage>((a, c) => a.ProcessFactoryCode == c.ProcessFactoryCode)
        //            .Where(where)
        //            .ToPageList(request.rows, request.page);
        //        return data;

        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        public PageModel GetBatchPlanDataList(InOrderRequest request)
        {
            string where = "where 1=1 and a.Examinestatus='审核完成'";
            if (!string.IsNullOrWhiteSpace(request.keyword))
                where += (" and a.BatchPlanNum like '%" + request.keyword + "%'");
            if (!string.IsNullOrEmpty(request.ProjectId))
                where += (" and a.ProjectId='" + request.ProjectId + "'");

            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
                where += (" and a.ProcessFactoryCode='" + request.ProcessFactoryCode + "'");
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);//站点
                List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 4);//工区
                string siteStr = string.Join("','", SiteList);
                string workAreaStr = string.Join("','", WorkAreaList);
                where += (" and (a.SiteCode in('" + siteStr + "') or a.WorkAreaCode in('" + workAreaStr + "'))");
            }
            //排除完全入库的排除编号
            string rksql = @"select BatchPlanCode from TbInOrder where InOrderState='完全入库' and BatchPlanCode not in(select a.BatchPlanNum from TbSupplyListDetailHistory a
left join TbInOrderItem b on a.BatchPlanItemNewCode=b.BatchPlanItemNewCode where b.ID is null
group by a.BatchPlanNum) group by BatchPlanCode";
            DataTable dtrk = Db.Context.FromSql(rksql).ToDataTable();
            if (dtrk.Rows.Count > 0)
            {
                StringBuilder sbrk = new StringBuilder();
                for (int i = 0; i < dtrk.Rows.Count; i++)
                {
                    if (dtrk.Rows.Count - 1 == i)
                    {
                        sbrk.Append("'" + dtrk.Rows[i]["BatchPlanCode"] + "'");
                    }
                    else
                    {
                        sbrk.Append("'" + dtrk.Rows[i]["BatchPlanCode"] + "',");
                    }
                }

                where += " and a.BatchPlanNum not in(" + sbrk + ")";
            }
            //排除未供货的批次
            string ghsql = "select BatchPlanNum from TbSupplyList where StateCode='未供货' group by BatchPlanNum";
            DataTable dtgh = Db.Context.FromSql(ghsql).ToDataTable();
            if (dtgh.Rows.Count > 0)
            {
                StringBuilder sbgh = new StringBuilder();
                for (int i = 0; i < dtgh.Rows.Count; i++)
                {
                    if (dtgh.Rows.Count - 1 == i)
                    {
                        sbgh.Append("'" + dtgh.Rows[i]["BatchPlanNum"] + "'");
                    }
                    else
                    {
                        sbgh.Append("'" + dtgh.Rows[i]["BatchPlanNum"] + "',");
                    }
                }

                where += " and a.BatchPlanNum not in(" + sbgh + ")";
            }
            //排除供货完成的明细的排除计划编号
            string ghrksql = @"select a.BatchPlanNum from  TbSupplyListDetailHistory a
            left join TbInOrderItem d on a.BatchPlanItemNewCode = d.BatchPlanItemNewCode
            where d.BatchPlanItemNewCode is null group by a.BatchPlanNum";
            DataTable dtghrk = Db.Context.FromSql(ghrksql).ToDataTable();
            if (dtghrk.Rows.Count > 0)
            {
                StringBuilder sbghrk = new StringBuilder();
                for (int i = 0; i < dtghrk.Rows.Count; i++)
                {
                    if (dtghrk.Rows.Count - 1 == i)
                    {
                        sbghrk.Append("'" + dtghrk.Rows[i]["BatchPlanNum"] + "'");
                    }
                    else
                    {
                        sbghrk.Append("'" + dtghrk.Rows[i]["BatchPlanNum"] + "',");
                    }
                }

                where += " and a.BatchPlanNum in(" + sbghrk + ")";
            }
            where += " and isnull(r.Rkl,0)<k.HasSupplierTotal";
            //string ghrksql1 = @"select a.BatchPlanNum from  TbSupplyListDetailHistory a
            //left join TbInOrderItem d on a.BatchPlanItemNewCode = d.BatchPlanItemNewCode
            //where d.BatchPlanItemNewCode is null group by a.BatchPlanNum";
            //DataTable dtghrk1 = Db.Context.FromSql(ghrksql1).ToDataTable();
            //if (dtghrk.Rows.Count > 0)
            //{
            //    StringBuilder sbghrk = new StringBuilder();
            //    for (int i = 0; i < dtghrk1.Rows.Count; i++)
            //    {
            //        if (dtghrk1.Rows.Count - 1 == i)
            //        {
            //            sbghrk.Append("'" + dtghrk1.Rows[i]["BatchPlanNum"] + "'");
            //        }
            //        else
            //        {
            //            sbghrk.Append("'" + dtghrk1.Rows[i]["BatchPlanNum"] + "',");
            //        }
            //    }

            //    where += " or a.BatchPlanNum in(" + sbghrk + ")";
            //}
            string sql = @"select a.*,i.DictionaryText as RebarTypeName,h.UserName as DeliverUser,j.SupplierName,g.StorageCode,g.StorageName,e.CompanyFullName as BranchName,f.CompanyFullName as WorkAreaName from TbFactoryBatchNeedPlan a
left join TbCompany e on a.BranchCode=e.CompanyCode
left join TbCompany f on a.WorkAreaCode=f.CompanyCode
left join TbStorage g on a.ProcessFactoryCode=g.ProcessFactoryCode
left join TbUser h on a.Acceptor=h.UserCode
left join TbSysDictionaryData i on a.SteelsTypeCode=i.DictionaryCode
left join TbSupplier j on a.SupplierCode=j.SupplierCode
left join TbSupplyList k on a.BatchPlanNum=k.BatchPlanNum
left join (select BatchPlanCode,sum(isnull(TbInOrderItem.PassCount,0)+isnull(TbInOrderItem.NoPass,0)) as Rkl from TbInOrder
left join TbInOrderItem on TbInOrder.InOrderCode=TbInOrderItem.InOrderCode
group by BatchPlanCode) r on k.BatchPlanNum=r.BatchPlanCode ";

            try
            {
                List<Parameter> parameter = new List<Parameter>();
                var data = Repository<TbInOrderItem>.FromSqlToPageTable(sql + where, parameter, request.rows, request.page, "BatchPlanNum");
                return data;
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// 获取数据列表(批次计划明细)
        /// </summary>
        public PageModel GetBatchPlanItemDataList(InOrderRequest request)
        {
            //#region 搜索条件

            //List<Parameter> parameter = new List<Parameter>();
            //string where = " where 1=1 and HasSupplier>0 and HasSupplier!=TotalPassCount and TotalPassCount>=0";
            //if (!string.IsNullOrWhiteSpace(request.keyword))
            //{
            //    where += " and RawMaterialNum like @RawMaterialNum";
            //    parameter.Add(new Parameter("@RawMaterialNum", '%' + request.keyword + '%', DbType.String, null));
            //}
            //if (!string.IsNullOrWhiteSpace(request.BatchPlanCode))
            //{
            //    where += " and BatchPlanNum=@BatchPlanCode";
            //    parameter.Add(new Parameter("@BatchPlanCode", request.BatchPlanCode, DbType.String, null));
            //}

            //#endregion

            //var sql = @"select * from (select
            //            a.ID as BatchPlanItemId,
            //            a.BatchPlanNum,
            //            a.Standard as SpecificationModel,
            //            a.RawMaterialNum as MaterialCode,
            //            a.MeasurementUnit,
            //            a.BatchPlanQuantity,
            //            b.DictionaryText as MeasurementUnitText,
            //            c.MaterialName,
            //            isnull(d.HasSupplier,0) as HasSupplier,
            //            isnull(e.PassCount,0)as TotalPassCount
            //            from TbFactoryBatchNeedPlanItem a
            //            left join TbSysDictionaryData b on a.MeasurementUnit=b.DictionaryCode
            //            left join TbRawMaterialArchives c on a.RawMaterialNum=c.MaterialCode
            //            left join TbSupplyListDetail d on a.RawMaterialNum=d.RawMaterialNum and a.BatchPlanNum=d.BatchPlanNum
            //            left join
            //            (
            //            select
            //            MaterialCode,
            //            isnull(SUM(PassCount),0)as PassCount
            //            from TbInOrderItem
            //            where InOrderCode in(select InOrderCode from TbInOrder where BatchPlanCode=@BatchPlanCode)
            //            group by MaterialCode
            //            ) e on a.RawMaterialNum=e.MaterialCode) Tb";
            string where = "";
            List<Parameter> parameter = new List<Parameter>();
            if (!string.IsNullOrWhiteSpace(request.keyword))
            {
                where += " and a.RawMaterialNum like @RawMaterialNum";
                parameter.Add(new Parameter("@RawMaterialNum", '%' + request.keyword + '%', DbType.String, null));
            }
            if (!string.IsNullOrWhiteSpace(request.BatchPlanCode))
            {
                where += " and a.BatchPlanNum=@BatchPlanCode";
                parameter.Add(new Parameter("@BatchPlanCode", request.BatchPlanCode, DbType.String, null));
            }
            //供货信息已经入库的排除
            string sqlin = @"select b.BatchPlanItemNewCode from TbInOrder a 
left join TbInOrderItem b on a.InOrderCode=b.InOrderCode where a.BatchPlanCode='" + request.BatchPlanCode + "' group by b.BatchPlanItemNewCode";
            var datain = Db.Context.FromSql(sqlin).ToDataTable();
            if (datain.Rows.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < datain.Rows.Count; i++)
                {
                    if (datain.Rows.Count - 1 == i)
                    {
                        sb.Append("'" + datain.Rows[i]["BatchPlanItemNewCode"] + "'");
                    }
                    else
                    {
                        sb.Append("'" + datain.Rows[i]["BatchPlanItemNewCode"] + "',");
                    }
                }
                where += " and b.BatchPlanItemNewCode not in(" + sb.ToString() + ")";
            }
            string sql = @"select a.BatchPlanItemId,a.RawMaterialNum as MaterialCode,a.MaterialName,a.Standard as SpecificationModel,a.MeasurementUnit,d.DictionaryText as MeasurementUnitText,a.BatchPlanQuantity,a.HasSupplier,b.ThisTimeCount as InOrdeCount,b.ThisTimeCount as PassCount,0 as NoPass,'' as NoPassReason,b.ID as HistoryID,b.ThisTime,b.ThisTimeCount,b.BatchPlanItemNewCode from TbSupplyListDetail a
left join TbSupplyListDetailHistory b on a.BatchPlanItemId=b.BatchPlanItemId 
left join TbSysDictionaryData d on a.MeasurementUnit=d.DictionaryCode where b.ID is not null  ";
            try
            {
                var data = Repository<TbInOrderItem>.FromSqlToPageTable(sql + where, parameter, request.rows, request.page, "BatchPlanItemId");
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// 获取实验室人员
        /// </summary>
        /// <param name="BranchCode">分部</param>
        /// <param name="WorkAreaCode">工区</param>
        /// <returns></returns>
        public DataTable GetSysUser(string ProjectId, string BranchCode, string WorkAreaCode)
        {
            string sql = @"select u.UserCode,u.UserId,u.UserName,b.DepartmentName,r.RoleName from TbUserRole a
left join TbDepartment b on a.DeptId=b.DepartmentId
left join TbCompany c on a.OrgId=c.CompanyCode
left join TbUser u on a.UserCode=u.UserId
left join TbRole r on a.RoleCode=r.RoleId
where (a.OrgType=3 or a.OrgType=4) and b.DepartmentName='试验室'
and a.ProjectId='" + ProjectId + "' and (a.OrgId='" + BranchCode + "' or a.OrgId='" + WorkAreaCode + "')";
            var data = Db.Context.FromSql(sql).ToDataTable();
            return data;
        }

        public DataTable GetInOrderData(string InOrderCode)
        {
            var ret = Db.Context.From<TbInOrder>().Select(TbInOrder._.All).Where(p => p.InOrderCode == InOrderCode).ToDataTable();
            return ret;
        }

        #region 原材料到货入库通知
        public AjaxResult SendNoticeNews(string InOrderCode)
        {
            try
            {
                //发送短信信息
                List<TbFormEarlyWarningNodeInfo> myMsgList = new List<TbFormEarlyWarningNodeInfo>();//Pc端推送
                List<TbSMSAlert> myDxList = new List<TbSMSAlert>();//App端
                var NoticeModel = Repository<TbNoticeNewsSetUp>.First(p => p.NoticeNewsCode == "XXTZ0002" && p.IsStart == 1);
                if (NoticeModel != null)
                {
                    //判断短信模板是否存在
                    var shortMessageTemplateModel =
                        Repository<TbShortMessageTemplate>.First(p => p.TemplateCode == "MB0004");
                    if (shortMessageTemplateModel != null)
                    {
                        var inOrderModel = Repository<TbInOrder>.First(p => p.InOrderCode == InOrderCode);
                        if (inOrderModel != null)
                        {
                            //获取用户userId
                            string UserId = ct.GetUserId(inOrderModel.SupplierCode).Rows[0]["UserId"].ToString();
                            if (!string.IsNullOrWhiteSpace(UserId))
                            {
                                string userInfo = ct.up(UserId);
                                var jObject = Newtonsoft.Json.Linq.JObject.Parse(userInfo);
                                string tel = jObject["data"][0]["MobilePhone"].ToString();
                                DataTable dt = ct.GetParentCompany(inOrderModel.WorkAreaCode);
                                if (dt.Rows.Count > 0)
                                {
                                    string ManagerDepartmentCode = "";
                                    string BranchCode = "";
                                    string BranchName = "";
                                    string WorkAreaCode = "";
                                    string WorkAreaName = "";
                                    for (int i = 0; i < dt.Rows.Count; i++)
                                    {
                                        if (dt.Rows[i]["OrgType"].ToString() == "2")
                                        {
                                            ManagerDepartmentCode = dt.Rows[i]["CompanyCode"].ToString();
                                        }
                                        else if (dt.Rows[i]["OrgType"].ToString() == "3")
                                        {
                                            BranchCode = dt.Rows[i]["CompanyCode"].ToString();
                                            BranchName = dt.Rows[i]["CompanyFullName"].ToString();
                                        }
                                        else if (dt.Rows[i]["OrgType"].ToString() == "4")
                                        {
                                            WorkAreaCode = dt.Rows[i]["CompanyCode"].ToString();
                                            WorkAreaName = dt.Rows[i]["CompanyFullName"].ToString();
                                        }
                                    }
                                    //短信、消息内容
                                    string content = shortMessageTemplateModel.TemplateContent;
                                    var bl = BranchName + "/" + WorkAreaName;
                                    var ShortContent = content.Replace("变量：分部/工区", bl);
                                    if (NoticeModel.App == 1)
                                    {
                                        if (!string.IsNullOrWhiteSpace(tel))
                                        {
                                            var myDxMsg = new TbSMSAlert()
                                              {
                                                  InsertTime = DateTime.Now,
                                                  ManagerDepartment = ManagerDepartmentCode,
                                                  Branch = BranchCode,
                                                  WorkArea = WorkAreaCode,
                                                  Site = "",
                                                  UserCode = UserId,
                                                  UserTel = tel,
                                                  DXType = "",
                                                  DataCode = inOrderModel.InOrderCode,
                                                  MsgType = "1",
                                                  FromCode = "InOrder",
                                                  ShortContent = ShortContent,
                                                  BusinessCode = shortMessageTemplateModel.TemplateCode
                                              };
                                            myDxList.Add(myDxMsg);
                                        }
                                    }
                                    if (NoticeModel.Pc == 1)
                                    {
                                        var myFormEarlyMsg = new TbFormEarlyWarningNodeInfo()
                                        {

                                            MenuCode = "InOrder",
                                            EWNodeCode = NoticeModel.ID,
                                            EWUserCode = UserId,
                                            ProjectId =inOrderModel.ProjectId,
                                            EarlyWarningCode = NoticeModel.NoticeNewsCode,
                                            EWFormDataCode = inOrderModel.ID,
                                            CompanyCode =BranchCode,
                                            WorkArea = WorkAreaCode,
                                            SiteCode = "",
                                            MsgType = "1",
                                            EWContent = ShortContent,
                                            EWStart = 0,
                                            EWTime = DateTime.Now,
                                            ProcessFactoryCode = "",
                                            DataCode = inOrderModel.InOrderCode,
                                            EarlyTitle = "【" + inOrderModel.InOrderCode + "】" +NoticeModel.NoticeNewsName
                                        };
                                        myMsgList.Add(myFormEarlyMsg);
                                    }
                                }
                            }
                        }
                        for (int i = 0; i < myDxList.Count; i++)
                        {
                            //调用短信发送接口
                            //string dx = ct.ShortMessagePC(myDxList[i].UserTel, myDxList[i].ShortContent);
                            string dx = ct.ShortMessagePC("15756321745", myDxList[i].ShortContent);
                            var jObject1 = Newtonsoft.Json.Linq.JObject.Parse(dx);
                            var logmsg = jObject1["data"][0]["code"].ToString();
                            myDxList[i].DXType = logmsg;
                        }
                    }

                }

                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    if (myMsgList.Any())
                    {
                        //添加表单预警信息
                        Repository<TbFormEarlyWarningNodeInfo>.Insert(trans, myMsgList, true);
                    }
                    if (myDxList.Any())
                    {
                        //添加短信信息
                        Repository<TbSMSAlert>.Insert(trans, myDxList, true);
                    }
                    trans.Commit();//提交事务 
                    return AjaxResult.Success();
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        #endregion

        #region 原材料到货入库不合格材料退回通知
        public bool InOrderNoPassSendNotice(int ID, string inOrderCode)
        {
            try
            {
                //原材料到货入库不合格材料退回通知
                List<TbFormEarlyWarningNodeInfo> myMsgList = new List<TbFormEarlyWarningNodeInfo>();//Pc端推送
                List<TbSMSAlert> myDxList = new List<TbSMSAlert>();//App端

                //查找消息模板信息
                var shortMessageTemplateModel = Repository<TbShortMessageTemplate>.First(p => p.TemplateCode == "MB0014");
                if (shortMessageTemplateModel != null)
                {
                    string sql = @"select a.ID,a.InOrderCode,a.RebarType,a.ProjectId,a.BatchPlanCode,b.MaterialName,b.SpecificationModel,b.PassCount,a.WorkAreaCode,c.ParentCompanyCode as BranchCode,d.ParentCompanyCode as ManagerDepartmentCode from TbInOrder a
left join TbInOrderItem b on a.InOrderCode=b.InOrderCode
left join TbCompany c on a.WorkAreaCode=c.CompanyCode
left join TbCompany d on c.ParentCompanyCode=d.CompanyCode
where a.InOrderCode='" + inOrderCode + "' and b.ChackState=3";
                    var xxtxitems = Db.Context.FromSql(sql).ToDataTable();
                    if (xxtxitems.Rows.Count > 0)
                    {
                        for (int i = 0; i < xxtxitems.Rows.Count; i++)
                        {
                            var NoticeModel = new TbNoticeNewsSetUp();
                            if (xxtxitems.Rows[i]["RebarType"].ToString() == "BuildingSteel")
                            {
                                NoticeModel = Repository<TbNoticeNewsSetUp>.First(p => p.NoticeNewsCode == "XXTZ0003" && p.IsStart == 1);
                            }
                            else
                            {
                                NoticeModel = Repository<TbNoticeNewsSetUp>.First(p => p.NoticeNewsCode == "XXTZ0011" && p.IsStart == 1);
                            }
                            if (NoticeModel != null)
                            {
                                var content = shortMessageTemplateModel.TemplateContent;
                                var s = content.Replace("变量：批次计划编号", xxtxitems.Rows[i]["BatchPlanCode"].ToString());
                                var a = s.Replace("变量：材料名称、规格", xxtxitems.Rows[i]["MaterialName"].ToString() + "、" + xxtxitems.Rows[i]["SpecificationModel"].ToString());
                                var ShortContent = a.Replace("变量：不合格重量", xxtxitems.Rows[i]["PassCount"].ToString());
                                //获取要发送短信通知消息的用户
                                List<CensusdemoTask.NotiecUser> listUser = ct.GetSendUser("InOrder", NoticeModel.NoticeNewsCode, ID);
                                if (listUser.Any())
                                {
                                    for (int u = 0; u < listUser.Count; u++)
                                    {
                                        if (NoticeModel.App == 1)
                                        {
                                            //调用BIM获取人员电话或者身份证号码的的接口
                                            string userInfo = ct.up(listUser[u].PersonnelCode);
                                            var jObject = Newtonsoft.Json.Linq.JObject.Parse(userInfo);
                                            string tel = jObject["data"][0]["MobilePhone"].ToString();
                                            if (!string.IsNullOrWhiteSpace(tel))
                                            {
                                                var myDxMsg = new TbSMSAlert()
                                                {
                                                    InsertTime = DateTime.Now,
                                                    ManagerDepartment = xxtxitems.Rows[i]["ManagerDepartmentCode"].ToString(),
                                                    Branch = xxtxitems.Rows[i]["BranchCode"].ToString(),
                                                    WorkArea = xxtxitems.Rows[i]["WorkAreaCode"].ToString(),
                                                    Site = "",
                                                    UserCode = listUser[u].PersonnelCode,
                                                    UserTel = tel,
                                                    DXType = "",
                                                    BusinessCode = shortMessageTemplateModel.TemplateCode,
                                                    DataCode = xxtxitems.Rows[i]["InOrderCode"].ToString(),
                                                    ShortContent = ShortContent,
                                                    FromCode = "InOrder",
                                                    MsgType = "4"

                                                };
                                                myDxList.Add(myDxMsg);
                                            }
                                        }
                                        if (NoticeModel.Pc == 1)
                                        {
                                            var myFormEarlyMsg = new TbFormEarlyWarningNodeInfo()
                                            {

                                                MenuCode = "InOrder",
                                                EWNodeCode = NoticeModel.ID,
                                                EWUserCode = listUser[u].PersonnelCode,
                                                ProjectId = xxtxitems.Rows[i]["ProjectId"].ToString(),
                                                EarlyWarningCode = NoticeModel.NoticeNewsCode,
                                                EWFormDataCode =Convert.ToInt32(xxtxitems.Rows[i]["ID"]),
                                                CompanyCode = xxtxitems.Rows[i]["BranchCode"].ToString(),
                                                WorkArea = xxtxitems.Rows[i]["WorkAreaCode"].ToString(),
                                                SiteCode = "",
                                                MsgType = "1",
                                                EWContent = ShortContent,
                                                EWStart = 0,
                                                EWTime = DateTime.Now,
                                                ProcessFactoryCode = "",
                                                DataCode = xxtxitems.Rows[i]["InOrderCode"].ToString(),
                                                EarlyTitle = "【" + xxtxitems.Rows[i]["InOrderCode"].ToString() + "】" + NoticeModel.NoticeNewsName
                                            };
                                            myMsgList.Add(myFormEarlyMsg);
                                        }

                                    }
                                }
                            }
                        }

                        for (int i = 0; i < myDxList.Count; i++)
                        {
                            //调用短信发送接口
                            //string dx = ct.ShortMessagePC(myDxList[i].UserTel, myDxList[i].ShortContent);
                            string dx = ct.ShortMessagePC("15756321745", myDxList[i].ShortContent);
                            var jObject1 = Newtonsoft.Json.Linq.JObject.Parse(dx);
                            var logmsg = jObject1["data"][0]["code"].ToString();
                            myDxList[i].DXType = logmsg;
                        }
                    }
                }

                using (DbTrans trans = Db.Context.BeginTransaction())
                {

                    if (myMsgList.Any())
                    {
                        //'我的消息'推送
                        Repository<TbFormEarlyWarningNodeInfo>.Insert(trans, myMsgList);
                    }
                    if (myDxList.Any())
                    {
                        //'短信'推送
                        Repository<TbSMSAlert>.Insert(trans, myDxList);
                    }
                    trans.Commit();
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion

        #region 图形报表

        public DataTable Img1(InOrderRequest request) 
        {
            string where = "";
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where += " and a.ProjectId='" + request.ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where += " and b.ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.RebarType))
            {
                 where += " and a.RebarType='" + request.RebarType + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 4);//工区
                string workAreaStr = string.Join("','", WorkAreaList);
                where += " and a.WorkAreaCode in('" + workAreaStr + "')";
            }
            string sql = @"select Tb.DataMonth,ISNULL(TbData.InorderCount,0) as InCount,ISNULL(TbData.InCount,0) as InWeight from (select datepart(MONTH,dateadd(mm,-t.number,getdate())) as DataMonth
from
(select number from master..spt_values where type='P') t
where year(dateadd(mm,-t.number,getdate()))=year(getdate())) Tb 
left join (select COUNT(1) as InorderCount,SUM(a.InCount) as InCount,a.RebarType,datepart(year,a.InsertTime) InsertYear,datepart(MONTH,a.InsertTime) InsertMonth from TbInOrder a 
left join TbStorage b on a.StorageCode=b.StorageCode
where a.Examinestatus!='已退回' 
and a.Examinestatus!='已撤销' 
and datepart(year,a.InsertTime)=datepart(year,GETDATE()) "+where+@"
group by a.RebarType,datepart(year,a.InsertTime),datepart(MONTH,a.InsertTime)) TbData on  Tb.DataMonth=TbData.InsertMonth
order by Tb.DataMonth asc";
            DataTable ret = Db.Context.FromSql(sql).ToDataTable();
            return ret;
        }

        public DataTable Img2(InOrderRequest request) 
        {
            string where = "";
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where += " and a.ProjectId='" + request.ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where += " and b.ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.RebarType))
            {
                where += " and a.RebarType='" + request.RebarType + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 4);//工区
                string workAreaStr = string.Join("','", WorkAreaList);
                where += " and a.WorkAreaCode in('" + workAreaStr + "')";
            }
            if (request.HistoryMonth.HasValue)
            {
                string Month = Convert.ToDateTime(request.HistoryMonth).ToString("yyyy-MM");
                where += " and CONVERT(varchar(7),a.InsertTime, 120 )='" + Month + "'";
            }
            string sql = @"select TbType.InType,isnull(TbData.InCount,0) as InCount from (select '入库单数' as InType
                           union all
                           select '未取样' as InType
                           union all
                           select '验收不合格' as InType
                           union all
                           select '检测部分合格' as InType
                           union all
                           select '检测不合格' as InType) TbType
                           left join(select '入库单数' as InType,COUNT(1) as InCount from TbInOrder a
                           left join TbStorage b on a.StorageCode=b.StorageCode 
                           where 1=1 and a.BatchPlanCode !='' "+where+@"
                           union all
                           select '未取样' as InType,COUNT(1) as InCount from TbInOrder a
                           left join TbStorage b on a.StorageCode=b.StorageCode
                           where 1=1 and a.SampleOrderState='取样未完成' and a.BatchPlanCode !='' " + where + @"
                           union all
                           select '验收不合格' as InType,COUNT(1) as InCount from TbInOrder a
                           left join TbStorage b on a.StorageCode=b.StorageCode
                           where 1=1  and NoPassTotal>0 and a.BatchPlanCode !='' " + where + @"
                           union all
                           select '检测部分合格' as InType,COUNT(1) as InCount from TbInOrder a
                           left join TbStorage b on a.StorageCode=b.StorageCode
                           where 1=1  and ChackResult=3 and a.BatchPlanCode !='' " + where + @"
                           union all
                           select '检测不合格' as InType,COUNT(1) as InCount from TbInOrder a
                           left join TbStorage b on a.StorageCode=b.StorageCode
                           where 1=1  and ChackResult=2 and a.BatchPlanCode !='' " + where + @") TbData on TbType.InType=TbData.InType";
            DataTable ret = Db.Context.FromSql(sql).ToDataTable();
            return ret;
        }

        #endregion
    }
}
