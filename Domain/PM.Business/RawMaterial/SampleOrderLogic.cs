using Dos.Common;
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
using System.Threading.Tasks;

namespace PM.Business.RawMaterial
{
    /// <summary>
    /// 原材料取样订单
    /// </summary>
    public class SampleOrderLogic
    {
        private readonly TbWorkOrderLogic _workOrderLogic = new TbWorkOrderLogic();
        private readonly InOrderLogic _InOrderLogic = new InOrderLogic();
        //发送短信
        CensusdemoTask ct = new CensusdemoTask();

        #region 新增数据

        /// <summary>
        /// 新增数据
        /// </summary>
        public AjaxResult Insert(TbSampleOrder model, List<TbSampleOrderItem> items)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            model.InsertUserCode = OperatorProvider.Provider.CurrentUser.UserCode;
            model.Examinestatus = "未发起";
            model.CheckStatus = "未检测";
            model.IsUpLoad = 0;
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //添加信息
                    Repository<TbSampleOrder>.Insert(trans, model);
                    //添加明细信息
                    Repository<TbSampleOrderItem>.Insert(trans, items);
                    trans.Commit();
                    return AjaxResult.Success();
                }
            }
            catch (Exception ex)
            {
                return AjaxResult.Error();
            }
        }

        #endregion

        #region 修改数据

        /// <summary>
        /// 修改数据
        /// </summary>
        public AjaxResult Update(TbSampleOrder model, List<TbSampleOrderItem> items)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            var anyRet = AnyInfo(model.ID);
            if (anyRet.state.ToString() != ResultType.success.ToString())
                return anyRet;
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())//使用事务
                {
                    //修改信息
                    Repository<TbSampleOrder>.Update(trans, model, p => p.ID == model.ID);
                    if (items.Count > 0)
                    {
                        //删除历史明细信息
                        Repository<TbSampleOrderItem>.Delete(trans, p => p.SampleOrderCode == model.SampleOrderCode);
                        //添加明细信息
                        Repository<TbSampleOrderItem>.Insert(trans, items);
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
                if (anyRet.state != ResultType.success.ToString())
                    return anyRet;
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //删除信息
                    var count = Repository<TbSampleOrder>.Delete(trans, p => p.ID == keyValue);
                    //删除明细信息
                    Repository<TbSampleOrderItem>.Delete(trans, p => p.SampleOrderCode == ((TbSampleOrder)anyRet.data).SampleOrderCode);
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

        #region 获取数据

        /// <summary>
        /// 获取编辑数据
        /// </summary>
        /// <param name="keyValue">数据Id</param>
        /// <returns></returns>
        public Tuple<object, object> FindEntity(int keyValue)
        {
            var ret = Db.Context.From<TbSampleOrder>()
                .Select(
                       TbSampleOrder._.All
                    , TbUser._.UserName
                    , TbCompany._.CompanyFullName.As("SiteName")
                    , TbSysDictionaryData._.DictionaryText.As("RebarTypeName"))
                    .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                    .Where(TbCompany._.CompanyCode == TbSampleOrder._.WorkAreaCode), "WorkAreaName")
                    .AddSelect(Db.Context.From<TbUser>().Select(p => p.UserName)
                    .Where(TbUser._.UserCode == TbSampleOrder._.UserCode), "ContactUser")
                    .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                    .Where(TbCompany._.CompanyCode == TbSampleOrder._.ProcessFactoryCode), "ProcessFactoryName")
                  .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                  .LeftJoin<TbCompany>((a, c) => a.SiteCode == c.CompanyCode)
                  .LeftJoin<TbSysDictionaryData>((a, c) => a.RebarType == c.DictionaryCode)
                  .Where(p => p.ID == keyValue).ToDataTable();
            if (ret == null || ret.Rows.Count == 0)
                return new Tuple<object, object>(null, null);
            //查找明细信息
            var items = Db.Context.From<TbSampleOrderItem>().Select(
                TbSampleOrderItem._.All,
                TbRawMaterialArchives._.MeasurementUnitZl.As("Weight"),
                TbInOrderItem._.PassCount)
            .LeftJoin<TbInOrderItem>((a, c) => a.MaterialCode == c.MaterialCode && a.InOrderItemId == c.ID && c.InOrderCode == Convert.ToString(ret.Rows[0]["InOrderCode"]))
            .LeftJoin<TbRawMaterialArchives>((a, c) => a.MaterialCode == c.MaterialCode)
            .Where(p => p.SampleOrderCode == Convert.ToString(ret.Rows[0]["SampleOrderCode"]))
            .ToDataTable();
            return new Tuple<object, object>(ret, items);
        }

        /// <summary>
        /// 获取数据列表(分页)
        /// </summary>
        public PageModel GetDataListForPage(SampleOrderRequest request)
        {

            #region 模糊搜索条件

            string where = " where 1=1 ";
            if (!string.IsNullOrWhiteSpace(request.CheckStatus))
            {
                where += (" and so.CheckStatus='" + request.CheckStatus + "'");
            }
            if (!string.IsNullOrWhiteSpace(request.Code))
            {
                where += (" and so.InOrderCode like '%" + request.Code + "%' or so.SampleOrderCode like '%" + request.Code + "%'");
            }
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where += (" and so.ProjectId='" + request.ProjectId + "'");
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where += (" and so.ProcessFactoryCode='" + request.ProcessFactoryCode + "'");
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);//站点
                List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 4);//工区
                string siteStr = string.Join("','", SiteList);
                string workAreaStr = string.Join("','", WorkAreaList);
                where += (" and (so.SiteCode in('" + siteStr + "') or so.WorkAreaCode in('" + workAreaStr + "'))");
            }
            if (request.ProcessingState > 0)
                where += " and so.ProcessingState=" + request.ProcessingState;
            if (request.ChackResult > 0)
                where += " and so.ChackResult=" + request.ChackResult;
            if (request.HistoryMonth.HasValue)
            {
                where += " and YEAR(so.SampleTime)=" + request.HistoryMonth.Value.Year + " and MONTH(so.SampleTime)=" + request.HistoryMonth.Value.Month;
            }
            if (!string.IsNullOrWhiteSpace(request.DemandPlanCode))
            {
                where += " and (fbnp.RawMaterialDemandNum='" + request.DemandPlanCode + "' or rmsp.DemandPlanCode='" +
                         request.DemandPlanCode + "') and (so.ChackResult=2 or so.ChackResult=3)";
            }
            if (!string.IsNullOrWhiteSpace(request.BatchPlanNum))
            {
                where += " and fbnp.BatchPlanNum='" + request.BatchPlanNum + "' and (so.ChackResult=2 or so.ChackResult=3)";
            }
            if (!string.IsNullOrWhiteSpace(request.RebarType))
            {
                where += " and so.RebarType='" + request.RebarType + "'";
            }
            #endregion

            try
            {
                string sql = @"select so.*,case when so.ProcessingState=0 then '未加工' when so.ProcessingState=1 then '加工中' when so.ProcessingState=2 then '加工完成' end ProcessingStateName,case when so.ChackResult=1 then '合格' when so.ChackResult=2 then '不合格' when so.ChackResult=3 then '部分合格' when so.ChackResult=4 then '部分不合格' else '未检测' end ChackResultName,ino.InsertTime as InOrderTime,ur.UserName,ur1.UserName as ContactUser,cp1.CompanyFullName as ProcessFactoryName,cp2.CompanyFullName as SiteName,cp3.CompanyFullName as WorkAreaName,cp4.CompanyFullName as BranchName,case when rmsp.DemandPlanCode is null then fbnp.RawMaterialDemandNum else rmsp.DemandPlanCode end as DemandPlanCode,sd.DictionaryText as RebarTypeName from TbSampleOrder so
                               left join TbUser ur on so.InsertUserCode=ur.UserCode
                               left join TbUser ur1 on so.UserCode=ur1.UserCode
                               left join TbCompany cp1 on so.ProcessFactoryCode=cp1.CompanyCode
                               left join TbCompany cp2 on so.SiteCode=cp2.CompanyCode
                               left join TbCompany cp3 on so.WorkAreaCode=cp3.CompanyCode
                               left join TbCompany cp4 on cp3.ParentCompanyCode=cp4.CompanyCode
							   left join TbInOrder ino on ino.InOrderCode=so.InOrderCode
							   left join TbFactoryBatchNeedPlan fbnp on fbnp.BatchPlanNum=ino.BatchPlanCode
							   left join TbRawMaterialMonthDemandSupplyPlan rmsp on rmsp.SupplyPlanCode=fbnp.RawMaterialDemandNum 
                               left join TbSysDictionaryData sd on so.RebarType=sd.DictionaryCode ";
                //参数化
                List<Dos.ORM.Parameter> para = new List<Dos.ORM.Parameter>();
                var data = new PageModel();
                if (request.IsOutPut)
                {
                    data.rows = Repository<TbSampleOrder>.FromSqlToDataTable(sql + where, para, "ID", "desc");
                }
                else
                {
                    data = Repository<TbSampleOrder>.FromSqlToPageTable(sql + where, para, request.rows, request.page, "ID", "desc");
                }
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
            var sampleOrder = Repository<TbSampleOrder>.First(p => p.ID == keyValue);
            if (sampleOrder == null)
                return AjaxResult.Warning("信息不存在");
            if (sampleOrder.Examinestatus != "未发起" && sampleOrder.Examinestatus != "已退回")
                return AjaxResult.Warning("信息正在审核中或已审核完成,不能进行此操作");

            return AjaxResult.Success(sampleOrder);
        }
        #endregion

        #region 质检结果

        /// <summary>
        /// 质检结果
        /// </summary>
        public AjaxResult SubmitTestReport(List<TbSampleOrderItem> items)
        {
            if (items == null || items.Count < 1)
                return AjaxResult.Warning("参数错误");
            //定义一个list集合存放不合格明细的ID
            List<int> lista = new List<int>();
            var sampleOrderCode = items[0].SampleOrderCode;
            //查找取样订单
            var sampleOrder = Repository<TbSampleOrder>.First(p => p.SampleOrderCode == sampleOrderCode);
            if (sampleOrder == null)
                return AjaxResult.Warning("取样订单信息不存在");
            //sampleOrder.CheckStatus = "检测完成";
            //查找取样订单明细
            var inOrderItemIds = items.Select(p => p.InOrderItemId).ToList();
            var sampleOrderItem = Repository<TbSampleOrderItem>.Query(p => p.SampleOrderCode == sampleOrder.SampleOrderCode && p.InOrderItemId.In(inOrderItemIds)).ToList();
            sampleOrderItem.ForEach(x =>
            {
                var t = items.FirstOrDefault(p => p.InOrderItemId == x.InOrderItemId);
                if (t.ChackStateH != 0 && t.ChackState == 0)
                {
                    if (t.ChackStateH == 2)
                    {
                        lista.Add(t.ID);
                    }
                    x.TestReportNo = t.TestReportNo;
                    x.ChackState = t.ChackStateH;
                    x.Enclosure = t.Enclosure;
                    x.ChackItemTiem = DateTime.Now;
                }
            });
            //查找入库单
            var inOrder = Repository<TbInOrder>.First(p => p.InOrderCode == sampleOrder.InOrderCode);
            if (inOrder == null)
                return AjaxResult.Warning("入库单信息不存在");
            //判断质检文件是否上传
            var isUpLoadList = sampleOrderItem.Where(p => !string.IsNullOrEmpty(p.Enclosure)).ToList();
            if (isUpLoadList.Count > 0)
                sampleOrder.IsUpLoad = 1;
            //判断检测结果
            int totalCount = sampleOrderItem.Count;
            int isOkList = sampleOrderItem.Where(p => p.ChackState == 1).Count();
            int isNotList = sampleOrderItem.Where(p => p.ChackState == 2).Count();
            int isNoList = sampleOrderItem.Where(p => p.ChackState == 0).Count();//目前数据都为0
            if (isNoList == totalCount)
                sampleOrder.ChackResult = 0; //未检测
            else if (isOkList == totalCount)
                sampleOrder.ChackResult = 1; //合格
            //else if (isNotList == totalCount || (isNoList + isNotList == totalCount))
            //    sampleOrder.ChackResult = 2; //不合格
            else if (isNotList == totalCount)
                sampleOrder.ChackResult = 2; //不合格 
            //else if (isOkList > 0)
            //    sampleOrder.ChackResult = 3; //部分合格
            else if (isOkList > 0 && isNotList == 0)
                sampleOrder.ChackResult = 3; //部分合格
            else if (isNotList > 0)
            {
                sampleOrder.ChackResult = 4; //部分不合格
            }
            if (isNoList > 0)
            {
                sampleOrder.CheckStatus = "检测中";
            }
            else
            {
                sampleOrder.CheckStatus = "检测完成";
                sampleOrder.ChackTiem = DateTime.Now;
            }
            //---------修改检测结果tq
            var sampleOrderList = Repository<TbSampleOrder>.Query(p => p.InOrderCode == sampleOrder.InOrderCode).ToList();
            if (sampleOrderList.Count > 0)
            {
                int list1 = sampleOrderList.Where(p => p.ChackResult == 0 && p.SampleOrderCode != sampleOrder.SampleOrderCode).Count();//未检测
                int list2 = sampleOrderList.Where(p => p.ChackResult == 1 && p.SampleOrderCode != sampleOrder.SampleOrderCode).Count();//合格
                int list3 = sampleOrderList.Where(p => p.ChackResult == 2 && p.SampleOrderCode != sampleOrder.SampleOrderCode).Count();//不合格
                int list4 = sampleOrderList.Where(p => p.ChackResult == 3 && p.SampleOrderCode != sampleOrder.SampleOrderCode).Count();//部分合格
                switch (sampleOrder.ChackResult)
                {
                    case 0:
                        list1 += 1;
                        break;
                    case 1:
                        list2 += 1;
                        if (list3 > 0 || list4 > 0)
                        {
                            inOrder.ChackResult = 3;//部分合格
                        }
                        else
                        {
                            inOrder.ChackResult = 1;//合格
                        }
                        break;
                    case 2:
                        list3 += 1;
                        if (list2 > 0 || list4 > 0)
                        {
                            inOrder.ChackResult = 3;//部分合格
                        }
                        else
                        {
                            inOrder.ChackResult = 2;//不合格
                        }
                        break;
                    case 3:
                        list4 += 1;
                        inOrder.ChackResult = 3;//部分合格
                        break;
                    default:
                        break;
                }
            }
            else
            {
                inOrder.ChackResult = sampleOrder.ChackResult;
            }
            //---------修改检测结果tq

            //查找入库单明细(检测报告编号赋值)
            var inOrderItem = Repository<TbInOrderItem>.Query(p => p.ID.In(inOrderItemIds)).ToList();
            var inOrderItemTep = MapperHelper.Map<TbInOrderItem, TbInOrderItem>(inOrderItem);
            if (inOrderItem.Any())
            {
                inOrderItem.ForEach(x =>
                {
                    var i = sampleOrderItem.FirstOrDefault(p => p.InOrderItemId == x.ID);
                    if (i != null)
                    {
                        x.TestReportNo = i.TestReportNo;
                        x.ChackState = i.ChackState;
                        var t = inOrderItemTep.FirstOrDefault(p => p.ID == x.ID);
                        t.PassCount -= i.Total;//如果
                    }
                });
            }
            var delRecord = new List<TbInOrderDeductionRecord>();
            var addRecord = new List<TbInOrderDeductionRecord>();
            var delStock = new List<TbRawMaterialStockRecord>();
            var updateStock = new List<TbRawMaterialStockRecord>();
            //赛选出第一次改变检测状态的数据
            var retInOrderItemIds = items.Where(p => p.ChackState == 0).Select(p => p.InOrderItemId).ToList();
            if (retInOrderItemIds.Any())
            {
                inOrderItemTep = inOrderItemTep.Where(p => retInOrderItemIds.Contains(p.ID)).ToList();
                //查询库存记录改变检测结果(合格：能抵扣的做出相应抵扣，不合格：删除入库抵扣记录)
                var deductionRecord = _InOrderLogic.GetStockRecord(inOrder, inOrderItemTep);
                ////查询库存记录
                //var inOrderItemIdList = inOrderItem.Select(p => p.ID).ToList();
                updateStock = Repository<TbRawMaterialStockRecord>.Query(p => p.InOrderitemId.In(retInOrderItemIds)).ToList();
                //查询入库抵扣记录
                var deductionRecordList = Repository<TbInOrderDeductionRecord>.Query(p => p.InOrderCode == inOrder.InOrderCode).ToList();
                updateStock.ForEach(x =>
                {
                    var i = items.FirstOrDefault(p => p.InOrderItemId == x.InOrderitemId);
                    if (i.ChackState == 0)
                    {
                        var obj = inOrderItem.FirstOrDefault(p => p.ID == x.InOrderitemId);
                        x.ChackState = obj.ChackState;
                        if (x.ChackState == 1)
                        {
                            //合格：有抵扣的做出相应抵扣
                            if (x.Count > x.HistoryCount)
                            {
                                //有抵扣
                                var qyc = x.UseCount;//取样后数量 4
                                var dkc = x.Count - x.HistoryCount;//抵扣的数量 3
                                //判断取样后的数据时候还可继续抵扣
                                if (qyc > dkc) //不够抵扣
                                {
                                    //删除原来的抵扣数据
                                    var delRed = deductionRecordList.Where(p => p.InOrderItemId == obj.ItemId).ToList();
                                    if (delRed.Any())
                                        delRecord.AddRange(delRed);
                                    //新的库存抵扣
                                    if (deductionRecord.Any())
                                    {
                                        var addRed = deductionRecord.Where(p => p.InOrderItemId == obj.ItemId).ToList();
                                        if (addRed.Any())
                                            addRecord.AddRange(addRed);
                                    }
                                    var tep = inOrderItemTep.FirstOrDefault(p => p.ItemId == obj.ItemId);
                                    if (tep.DeductionCount == 0)
                                        delStock.Add(x);
                                    else
                                        x.Count = Convert.ToDecimal(tep.DeductionCount);
                                }
                                else if (qyc == dkc) //刚好抵扣
                                {
                                    delStock.Add(x);
                                }
                                else
                                {
                                    x.Count = qyc - dkc;
                                }
                                x.UseCount = x.Count;
                                x.HistoryCount = x.Count;
                            }
                            else
                            {
                                //无抵扣
                                x.HistoryCount = x.UseCount;
                                x.Count = x.UseCount;
                            }
                        }
                        else if (x.ChackState == 2)
                        {
                            //不合格：删除入库抵扣记录
                            if (deductionRecordList.Any())
                            {
                                var delRed = deductionRecordList.FirstOrDefault(p => p.InOrderItemId == obj.ItemId);
                                if (delRed != null)
                                    delRecord.Add(delRed);
                            }
                            x.UseCount = 0;
                            x.Count = 0;
                        }
                    }
                });
                if (delRecord.Any())
                {
                    var ids = delRecord.Select(p => p.ID).ToList();
                    updateStock = updateStock.Where(p => !ids.Contains(p.ID)).ToList();
                }
            }
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //修改取样订单附件信息
                    Repository<TbSampleOrder>.Update(trans, sampleOrder);
                    //修改明细信息
                    Repository<TbSampleOrderItem>.Update(trans, sampleOrderItem);
                    //修改入库单信息
                    Repository<TbInOrder>.Update(trans, inOrder);
                    //修改入库单明细质检结果编号
                    if (inOrderItem.Any())
                        Repository<TbInOrderItem>.Update(trans, inOrderItem);
                    //删除入库抵扣记录
                    if (delRecord.Any())
                        Repository<TbInOrderDeductionRecord>.Delete(trans, delRecord);
                    //添加入库抵扣记录
                    if (addRecord.Any())
                        Repository<TbInOrderDeductionRecord>.Insert(trans, addRecord);
                    //删除库存记录
                    if (delStock.Any())
                        Repository<TbRawMaterialStockRecord>.Delete(trans, delStock);
                    //修改库存记录
                    if (updateStock.Any())
                        Repository<TbRawMaterialStockRecord>.Update(trans, updateStock);

                    var updateSQL = @"update TbRawMaterialStockRecord 
		                             set 
		                                 LockCount=(LockCount+b.DeductionCount)
		                             from TbRawMaterialStockRecord as a,
		                             (
		                            	 select 
		                            	 StockRecordId,
		                            	 DeductionCount
		                            	 from TbInOrderDeductionRecord
		                            	 where InOrderCode='" + inOrder.InOrderCode + "' AND DeductionCount>0 and IsDel=0";
                    updateSQL += @")as b 
		                             where a.ID=b.StockRecordId;";

                    updateSQL += " UPDATE TbInOrderDeductionRecord SET IsDel=1 WHERE InOrderCode='" + inOrder.InOrderCode + "';";
                    Db.Context.FromSql(updateSQL).SetDbTransaction(trans).ExecuteNonQuery();
                    trans.Commit();
                    //取样订单检测不合格预警
                    ChackNoPassEarly(sampleOrder.ID, lista);
                    return AjaxResult.Success();
                }
            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.ToString());
            }
        }

        #endregion

        #region 取样订单检测不合格预警

        public bool ChackNoPassEarly(int ID, List<int> lista)
        {
            try
            {
                List<TbFormEarlyWarningNodeInfo> myMsgList = new List<TbFormEarlyWarningNodeInfo>();//Pc端推送
                List<TbSMSAlert> myDxList = new List<TbSMSAlert>();//App端

                //查找消息模板信息
                var shortMessageTemplateModel = Repository<TbShortMessageTemplate>.First(p => p.TemplateCode == "MB0018");
                if (shortMessageTemplateModel != null)
                {
                    if (lista.Count > 0)
                    {
                        string itemId = "";
                        for (int i = 0; i < lista.Count; i++)
                        {
                            if (i == lista.Count - 1)
                            {
                                itemId += lista[i];
                            }
                            else
                            {
                                itemId += lista[i] + ",";
                            }
                        }
                        string sql = @"select a.ID,ino.RebarType,a.SampleOrderCode,b.MaterialCode,b.MaterialName,b.SpecificationModel,tb1.CompanyCode as WorkAreaCode,tb1.CompanyFullName as WorkAreaName,tb2.CompanyCode as BranchCode,tb2.CompanyFullName as BranchName,a.ProcessFactoryCode from TbSampleOrder a
left join TbSampleOrderItem b on a.SampleOrderCode=b.SampleOrderCode
left join TbCompany tb1 on a.WorkAreaCode=tb1.CompanyCode
left join TbCompany tb2 on tb1.ParentCompanyCode=tb2.CompanyCode
left join TbInOrder ino on a.InOrderCode=ino.InOrderCode
where a.ID=" + ID + " and b.ID in(" + itemId + ")";
                        var xxtxitems = Db.Context.FromSql(sql).ToDataTable();
                        if (xxtxitems.Rows.Count > 0)
                        {
                            for (int i = 0; i < xxtxitems.Rows.Count; i++)
                            {
                                var NoticeModel = new TbEarlyWarningSetUp();
                                if (xxtxitems.Rows[i]["RebarType"].ToString() == "BuildingSteel")
                                {
                                    NoticeModel = Repository<TbEarlyWarningSetUp>.First(p => p.MenuCode == "SampleOrder2" && p.EarlyMenuCodeNode == 1 && p.IsStart == 1);
                                }
                                else
                                {
                                    NoticeModel = Repository<TbEarlyWarningSetUp>.First(p => p.MenuCode == "SampleOrder2" && p.EarlyMenuCodeNode == 2 && p.IsStart == 1);
                                }
                                if (NoticeModel != null)
                                {
                                    var content = shortMessageTemplateModel.TemplateContent;
                                    var s = content.Replace("变量：分部/工区", xxtxitems.Rows[i]["BranchName"].ToString() + "/" + xxtxitems.Rows[i]["WorkAreaName"].ToString());
                                    var a = s.Replace("变量：取样单号", xxtxitems.Rows[i]["SampleOrderCode"].ToString());
                                    var ShortContent = a.Replace("变量：材料名称、规格", xxtxitems.Rows[0]["MaterialName"].ToString() + "、" + xxtxitems.Rows[i]["SpecificationModel"].ToString());
                                    //获取要发送短信通知消息的用户
                                    List<CensusdemoTask.NotiecUser> listUser = ct.GetSendUser("SampleOrder", NoticeModel.EarlyWarningNewsCode, ID);
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
                                                    DataCode = xxtxitems.Rows[i]["SampleOrderCode"].ToString(),
                                                    ShortContent = ShortContent,
                                                    FromCode = "SampleOrder",
                                                    MsgType = "4"
                                                };
                                                myDxList.Add(myDxMsg);
                                            }
                                        }
                                        if (NoticeModel.Pc == 1)
                                        {
                                            var myFormEarlyMsg = new TbFormEarlyWarningNodeInfo()
                                            {

                                                MenuCode = xxtxitems.Rows[i]["MenuCode"].ToString(),
                                                EWNodeCode = Convert.ToInt32(xxtxitems.Rows[i]["ID"]),
                                                EWUserCode = listUser[u].PersonnelCode,
                                                ProjectId = xxtxitems.Rows[i]["ProjectId"].ToString(),
                                                EarlyWarningCode = NoticeModel.EarlyWarningNewsCode,
                                                EWFormDataCode = ID,
                                                CompanyCode = xxtxitems.Rows[i]["BranchCode"].ToString(),
                                                WorkArea = xxtxitems.Rows[i]["WorkAreaCode"].ToString(),
                                                SiteCode = "",
                                                MsgType = "2",
                                                EWContent = ShortContent,
                                                EWStart = 0,
                                                EWTime = DateTime.Now,
                                                ProcessFactoryCode = xxtxitems.Rows[i]["ProcessFactoryCode"].ToString()
                                            };
                                            myMsgList.Add(myFormEarlyMsg);
                                        }
                                    }
                                }
                            }
                        }
                        //调用短信接口发送短信
                        for (int m = 0; m < myDxList.Count; m++)
                        {
                            //var dx = ct.ShortMessagePC(myDxList[m].UserTel, myDxList[m].ShortContent);
                            var dx = ct.ShortMessagePC("15756321745", myDxList[m].ShortContent);
                            var jObject = Newtonsoft.Json.Linq.JObject.Parse(dx);
                            var logmsg = jObject["data"][0]["code"].ToString();
                            myDxList[m].DXType = logmsg;
                        }
                    }
                }
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    if (myMsgList.Any())
                    {
                        //添加表单预警信息
                        Repository<PM.DataEntity.TbFormEarlyWarningNodeInfo>.Insert(trans, myMsgList, true);
                    }

                    if (myDxList.Any())
                    {
                        //向短信信息表中插入数据
                        Repository<TbSMSAlert>.Insert(trans, myDxList, true);
                    }
                    //调用手机端消息推送方法
                    trans.Commit();//提交事务
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region 加工完成

        /// <summary>
        /// 加工完成
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public AjaxResult ProcessingOver(int keyValue)
        {
            var sampleOrder = Repository<TbSampleOrder>.First(p => p.ID == keyValue);
            if (sampleOrder == null)
                return AjaxResult.Warning("信息不存在");
            if (sampleOrder.ProcessingState == 0)
                return AjaxResult.Warning("订单还未加工，不能进行此操作");
            else if (sampleOrder.ProcessingState == 2)
                return AjaxResult.Warning("订单已加工完成，不能进行此操作");
            sampleOrder.ProcessingState = 2;
            sampleOrder.CheckStatus = "检测中";
            sampleOrder.ProcessingStateTime = DateTime.Now;
            //查找库存记录进行减扣
            var sampleOrderItemList = Repository<TbSampleOrderItem>.Query(p => p.SampleOrderCode == sampleOrder.SampleOrderCode).ToList();
            var itemId = sampleOrderItemList.Select(p => p.InOrderItemId).ToList();
            var stockRecordList = Repository<TbRawMaterialStockRecord>.Query(p => p.InOrderitemId.In(itemId)).ToList();
            if (!stockRecordList.Any())
                return AjaxResult.Warning("库存数据错误");
            stockRecordList.ForEach(x =>
            {
                var tmp = sampleOrderItemList.FirstOrDefault(p => p.InOrderItemId == x.InOrderitemId);
                x.UseCount -= Convert.ToDecimal(tmp.Total);
            });
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //修改取样订单信息
                    Repository<TbSampleOrder>.Update(trans, sampleOrder);
                    //修改库存信息
                    Repository<TbRawMaterialStockRecord>.Update(trans, stockRecordList);
                    trans.Commit();
                    //调用短信通知消息
                    QyDdJgWcSendNotice(keyValue);
                    return AjaxResult.Success();
                }
            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.ToString());
            }
        }

        #endregion

        #region 取样订单加工完成通知
        public bool QyDdJgWcSendNotice(int keyValue)
        {
            try
            {
                //取样订单加工完成通知 短信、信息
                List<TbFormEarlyWarningNodeInfo> myMsgList = new List<TbFormEarlyWarningNodeInfo>();//Pc端推送
                List<TbSMSAlert> myDxList = new List<TbSMSAlert>();//App推送
                var NoticeModel = Repository<TbNoticeNewsSetUp>.First(p => p.NoticeNewsCode == "XXTZ0004" && p.IsStart == 1);
                if (NoticeModel != null)
                {
                    //查找消息模板信息
                    var shortMessageTemplateModel = Repository<TbShortMessageTemplate>.First(p => p.TemplateCode == "MB0013");
                    if (shortMessageTemplateModel != null)
                    {
                        string sql = @"select so.ID,so.SampleOrderCode,so.InsertUserCode,so.ProjectId,tb1.ParentCompanyCode as ManagerDepartmentCode,tb1.CompanyCode as WorkAreaCode,tb1.CompanyFullName as WorkAreaName,tb2.CompanyCode as BranchCode,tb2.CompanyFullName as BranchName,tb3.CompanyCode as ProcessFactoryCode,tb3.CompanyFullName as ProcessFactoryName from (select so.ID,so.SampleOrderCode,so.WorkAreaCode,so.ProjectId,so.ProcessFactoryCode,so.InsertUserCode from TbSampleOrder so
                               union all
                               select so.ID,so.SampleOrderCode,so.WorkAreaCode,so.ProjectId,so.ProcessFactoryCode,ino.SupplierCode from TbSampleOrder so
                               left join TbInOrder ino on so.InOrderCode=ino.InOrderCode) so 
                               left join TbCompany tb1 on so.WorkAreaCode=tb1.CompanyCode
                               left join TbCompany tb2 on tb1.ParentCompanyCode=tb2.CompanyCode
                               left join TbCompany tb3 on so.ProcessFactoryCode=tb3.CompanyCode
                               where so.ID=" + keyValue + "";
                        var xxtxitems = Db.Context.FromSql(sql).ToDataTable();
                        if (xxtxitems.Rows.Count > 0)
                        {
                            var content = shortMessageTemplateModel.TemplateContent;
                            var s = content.Replace("变量：分部/工区", xxtxitems.Rows[0]["BranchName"].ToString() + "/" + xxtxitems.Rows[0]["WorkAreaName"].ToString());
                            var a = s.Replace("变量：取样单号", xxtxitems.Rows[0]["SampleOrderCode"].ToString());
                            var ShortContent = a.Replace("变量：加工厂名称", xxtxitems.Rows[0]["ProcessFactoryName"].ToString());
                            for (int i = 0; i < xxtxitems.Rows.Count; i++)
                            {
                                //获取用户userId
                                string UserId = ct.GetUserId(xxtxitems.Rows[i]["InsertUserCode"].ToString()).Rows[0]["UserId"].ToString();
                                if (!string.IsNullOrWhiteSpace(UserId))
                                {
                                    if (NoticeModel.App == 1)
                                    {
                                        //调用获取电话号码接口
                                        string userInfo = ct.up(UserId);
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
                                                UserCode = UserId,
                                                UserTel = tel,
                                                DXType = "",
                                                BusinessCode = shortMessageTemplateModel.TemplateCode,
                                                DataCode = xxtxitems.Rows[i]["SampleOrderCode"].ToString(),
                                                ShortContent = ShortContent,
                                                FromCode = "SampleOrder",
                                                MsgType = "1"

                                            };
                                            myDxList.Add(myDxMsg);
                                        }
                                    }
                                    if (NoticeModel.Pc == 1)
                                    {
                                        var myFormEarlyMsg = new TbFormEarlyWarningNodeInfo()
                                        {

                                            MenuCode = "SampleOrder",
                                            EWNodeCode = NoticeModel.ID,
                                            EWUserCode = UserId,
                                            ProjectId = xxtxitems.Rows[i]["ProjectId"].ToString(),
                                            EarlyWarningCode = NoticeModel.NoticeNewsCode,
                                            EWFormDataCode = Convert.ToInt32(xxtxitems.Rows[i]["ID"]),
                                            CompanyCode = xxtxitems.Rows[i]["BranchCode"].ToString(),
                                            WorkArea = xxtxitems.Rows[i]["WorkAreaCode"].ToString(),
                                            SiteCode = "",
                                            MsgType = "1",
                                            EWContent = ShortContent,
                                            EWStart = 0,
                                            EWTime = DateTime.Now,
                                            ProcessFactoryCode = xxtxitems.Rows[i]["ProcessFactoryCode"].ToString(),
                                            DataCode = xxtxitems.Rows[i]["SampleOrderCode"].ToString(),
                                            EarlyTitle = "【" + xxtxitems.Rows[i]["SampleOrderCode"].ToString() + "】" + NoticeModel.NoticeNewsName
                                        };
                                        myMsgList.Add(myFormEarlyMsg);
                                    }
                                }

                            }
                            //调用短信接口发送短信
                            for (int m = 0; m < myDxList.Count; m++)
                            {
                                //var dx = ct.ShortMessagePC(myDxList[m].UserTel, myDxList[m].ShortContent);
                                var dx = ct.ShortMessagePC("15756321745", myDxList[m].ShortContent);
                                var jObject = Newtonsoft.Json.Linq.JObject.Parse(dx);
                                var logmsg = jObject["data"][0]["code"].ToString();
                                myDxList[m].DXType = logmsg;
                            }
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
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

        }
        #endregion

        /// <summary>
        /// 获取数据列表(入库单)
        /// </summary>
        public PageModel GetInOrderDataList(SampleOrderRequest request)
        {
            #region 模糊搜索条件

            //var where = new Where<TbInOrder>();
            //if (!string.IsNullOrWhiteSpace(request.keyword))
            //{
            //    where.And(p => p.InOrderCode.Like(request.keyword));
            //}
            //if (!string.IsNullOrEmpty(request.ProjectId))
            //    where.And(p => p.ProjectId == request.ProjectId);
            //if (!string.IsNullOrWhiteSpace(request.SiteCode))
            //{
            //    List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);//站点
            //    List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 4);//工区
            //    where.And(p => p.SiteCode.In(SiteList) || p.WorkAreaCode.In(WorkAreaList));
            //}
            //where.And(p => p.Examinestatus == "审核完成" && p.SampleOrderState == "取样未完成");
            //where.And(TbInOrder._.InOrderCode
            //    .SubQueryNotIn(Db.Context.From<TbSampleOrder>().Select(TbSampleOrder._.InOrderCode)));
            string where = " where 1=1 and ino.Examinestatus='审核完成' and ino.BatchPlanCode!='' and wqy.InOrderCode is not null ";
            if (!string.IsNullOrWhiteSpace(request.keyword))
            {
                where += (" and ino.InOrderCode like '%" + request.keyword + "%' ");
            }
            if (!string.IsNullOrEmpty(request.ProjectId))
                where += (" and ino.ProjectId='" + request.ProjectId + "'");
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);//站点
                List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 4);//工区
                string siteStr = string.Join("','", SiteList);
                string workAreaStr = string.Join("','", WorkAreaList);
                where += (" and (ino.SiteCode in('" + siteStr + "') or ino.WorkAreaCode in('" + workAreaStr + "'))");
            }

            #endregion
            try
            {
                //var data = Db.Context.From<TbInOrder>().Select(
                //    TbInOrder._.InOrderCode,
                //    TbInOrder._.WorkAreaCode,
                //    TbInOrder._.SiteCode,
                //    TbCompany._.CompanyFullName.As("SiteName"),
                //    TbStorage._.StorageName)
                //    //.AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                //    //.Where(TbCompany._.CompanyCode == TbInOrder._.ba), "BranchName")
                //    .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                //    .Where(TbCompany._.CompanyCode == TbInOrder._.WorkAreaCode), "WorkAreaName")
                //    .LeftJoin<TbStorage>((a, c) => a.StorageCode == c.StorageCode)
                //    .LeftJoin<TbCompany>((a, c) => a.SiteCode == c.CompanyCode)
                //    .Where(where)
                //    .OrderByDescending(p => p.ID)
                //    .ToPageList(request.rows, request.page);
                string sql = @"select ino.ID,ino.ProjectId,ino.InOrderCode,ino.WorkAreaCode,cp2.CompanyFullName as WorkAreaName,ino.SiteCode,ino.InsertTime as InOrderTime,cp1.CompanyFullName as SiteName,cp3.CompanyCode as BranchCode,cp3.CompanyFullName as BranchName,st.StorageName,st.ProcessFactoryCode,cp4.CompanyFullName as ProcessFactoryName,ino.RebarType,sd.DictionaryText as RebarTypeName from TbInOrder ino 
                               left join(select a.InOrderCode from TbInOrderItem a left join TbSampleOrderItem b on a.ID=b.InOrderitemId where b.ID is null group by a.InOrderCode) wqy on ino.InOrderCode=wqy.InOrderCode
                               left join TbCompany cp1 on ino.SiteCode=cp1.CompanyCode
                               left join TbCompany cp2 on ino.WorkAreaCode=cp2.CompanyCode
                               left join TbCompany cp3 on cp2.ParentCompanyCode=cp3.CompanyCode
                               left join TbStorage st on ino.StorageCode=st.StorageCode
                               left join TbCompany cp4 on st.ProcessFactoryCode=cp4.CompanyCode
                               left join TbSysDictionaryData sd on ino.RebarType=sd.DictionaryCode ";
                //参数化
                List<Dos.ORM.Parameter> para = new List<Dos.ORM.Parameter>();
                var data = Repository<TbInOrder>.FromSqlToPageTable(sql + where, para, request.rows, request.page, "ID", "desc");
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 获取数据列表(入库单明细)
        /// </summary>
        public PageModel GetInOrderItemDataList(SampleOrderRequest request)
        {
            #region 模糊搜索条件

            var where = new Where<TbInOrderItem>();
            if (!string.IsNullOrWhiteSpace(request.keyword))
            {
                where.And(p => p.MaterialCode.Like(request.keyword));
            }
            if (!string.IsNullOrWhiteSpace(request.InOrderCode))
            {
                where.And(p => p.InOrderCode == request.InOrderCode);
                //排除取样过的明细
                var sampleOrderCode = Repository<TbSampleOrder>.Query(p => p.InOrderCode == request.InOrderCode).Select(p => p.SampleOrderCode).ToList();
                if (sampleOrderCode.Any())
                {
                    var inOrderItemId = Repository<TbSampleOrderItem>.Query(p => p.SampleOrderCode.In(sampleOrderCode)).Select(p => p.InOrderItemId).ToList();
                    if (inOrderItemId.Any())
                    {
                        where.And(p => p.ID.NotIn(inOrderItemId));
                    }
                }
            }

            #endregion
            try
            {
                var data = Db.Context.From<TbInOrderItem>().Select(
                    TbInOrderItem._.ID.As("InOrderItemId"),
                    TbInOrderItem._.SpecificationModel,
                    TbInOrderItem._.MaterialCode,
                    TbInOrderItem._.PassCount,
                    TbInOrderItem._.BatchNumber,
                    TbInOrderItem._.Factory,
                    TbRawMaterialArchives._.MaterialName,
                    TbRawMaterialArchives._.MeasurementUnitZl.As("Weight"))
                    .LeftJoin<TbRawMaterialArchives>((a, c) => a.MaterialCode == c.MaterialCode)
                    .Where(where)
                    .ToPageList(request.rows, request.page);
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region 统计报表
        public DataTable Img1(SampleOrderRequest request)
        {
            string where = "";
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where += " and a.ProjectId='" + request.ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where += " and a.ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
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
            string sql = @"select TbType.spoType,ISNULL(TbData.spoCount,0) as spoCount,ISNULL(TbData.PassCount,0) as PassCount from (
                           select '报告上传超时材料' as spoType
                           union all
                           select '取样材料种类总量' as spoType
                           union all
                           select '检测不合格种类总量' as spoType) TbType
                           left join (
                           select '报告上传超时材料' as spoType,COUNT(1) spoCount,0 as PassCount from TbSampleOrder a
                           where a.Examinestatus='审核完成' and a.IsUpLoad=0
                           and DATEADD(DAY,7,a.ProcessingStateTime)<GETDATE()
                           union all
                           select '取样材料种类总量' as spoType ,COUNT(1) as spoCount,SUM(TbData.PassCount) as PassCount from (select a.SampleOrderCode,isnull(SUM(c.PassCount),0) as PassCount from TbSampleOrder a
                           left join TbSampleOrderItem b on a.SampleOrderCode=b.SampleOrderCode
                           left join TbInOrderItem c on b.InOrderItemId=c.ID
                           where a.Examinestatus='审核完成'  group by a.SampleOrderCode) TbData
                           union all
                           select '检测不合格种类总量' as spoType ,COUNT(1) as spoCount,SUM(TbData.PassCount) as PassCount from (select a.SampleOrderCode,isnull(SUM(c.PassCount),0) as PassCount from TbSampleOrder a
                           left join TbSampleOrderItem b on a.SampleOrderCode=b.SampleOrderCode
                           left join TbInOrderItem c on b.InOrderItemId=c.ID
                           where a.Examinestatus='审核完成' and b.ChackState=2 group by a.SampleOrderCode) TbData) TbData on TbType.spoType=TbData.spoType";
            DataTable ret = Db.Context.FromSql(sql).ToDataTable();
            ////获取合格率
            decimal hgl = Convert.ToDecimal(ret.Rows[1]["PassCount"]) - Convert.ToDecimal(ret.Rows[2]["PassCount"]);
            string hglbfb = (hgl / Convert.ToDecimal(ret.Rows[1]["PassCount"])).ToString("0.00");
            DataRow newRow = ret.NewRow();
            newRow["spoType"] = "总量合格率";
            newRow["spoCount"] = 0;
            newRow["PassCount"] = Convert.ToDecimal(hglbfb) * 100;
            ret.Rows.Add(newRow);
            return ret;
        }
        public DataTable Img2(SampleOrderRequest request)
        {
            string where = "";
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where += " and a.ProjectId='" + request.ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where += " and a.ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
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
            string sql = @"select TbType.spoType,ISNULL(TbData.TypeCount,0) as TypeCount from (
                           select '取样订单数' spoType
                           union all
                           select '加工中' spoType
                           union all
                           select '加工完成(未检测)' spoType
                           union all
                           select '检测完成(待上传附件)' spoType
                           union all
                           select '检测完成' spoType) TbType
                           left join (
                           select '取样订单数' spoType,COUNT(1) as TypeCount from TbSampleOrder a 
                           where  a.Examinestatus='审核完成' " + where + @"
                           union all
                           select '加工中' spoType,COUNT(1) as TypeCount from TbSampleOrder a 
                           where  a.Examinestatus='审核完成' and a.ProcessingState=1 " + where + @"
                           union all
                           select '加工完成(未检测)' spoType,COUNT(1) as TypeCount from TbSampleOrder a 
                           where  a.Examinestatus='审核完成' and a.ProcessingState=2 and (a.CheckStatus='未检测' or a.CheckStatus='检测中') " + where + @"
                           union all
                           select '检测完成(待上传附件)' spoType,COUNT(1) as TypeCount from TbSampleOrder a 
                           where  a.Examinestatus='审核完成' and a.ProcessingState=2 and a.CheckStatus='检测完成' and a.IsUpLoad=0 " + where + @"
                           union all
                           select '检测完成' spoType,COUNT(1) as TypeCount from TbSampleOrder a 
                           where  a.Examinestatus='审核完成' and a.ProcessingState=2 and a.CheckStatus='检测完成' and a.IsUpLoad=1 " + where + @") TbData on TbType.spoType=TbData.spoType";
            DataTable ret = Db.Context.FromSql(sql).ToDataTable();
            return ret;
        }
        public DataTable Img3(SampleOrderRequest request)
        {
            string where = "";
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where += " and a.ProjectId='" + request.ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where += " and a.ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
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
            string sql = @"select TbType.spoType,isnull(TbData.TypeCount,0) as TypeCount from (select '合格' as spoType
                           union all
                           select '不合格' as spoType
                           union all
                           select '部分合格' as spoType) TbType
                           left join (
                           select '合格' as spoType,COUNT(1) as TypeCount from TbSampleOrder a
                           where a.Examinestatus='审核完成' and a.ChackResult=1 " + where + @"
                           union all
                           select '不合格' as spoType,COUNT(1) as TypeCount from TbSampleOrder a
                           where a.Examinestatus='审核完成' and a.ChackResult=2 " + where + @"
                           union all
                           select '部分合格' as spoType,COUNT(1) as TypeCount from TbSampleOrder a
                           where a.Examinestatus='审核完成' and (a.ChackResult=3 or a.ChackResult=4) " + where + @"
                           ) TbData on TbType.spoType=TbData.spoType";
            DataTable ret = Db.Context.FromSql(sql).ToDataTable();
            return ret;
        }
        #endregion
    }
}
