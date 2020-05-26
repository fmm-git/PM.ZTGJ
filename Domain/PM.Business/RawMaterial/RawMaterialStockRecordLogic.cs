using Dos.Common;
using Dos.ORM;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Business.RawMaterial
{
    /// <summary>
    /// 原材料库存记录
    /// </summary>
    public class RawMaterialStockRecordLogic
    {
        #region ToDel

        ///// <summary>
        ///// 获取库存记录
        ///// </summary>
        ///// <param name="model"></param>
        ///// <param name="isReduce"></param>
        ///// <returns></returns>
        //public List<TbRawMaterialStockRecord> GetStockRecord<T>(T Data, RawMaterialStockRecordRequest model)
        //{
        //    var dataList = MapperHelper.Map<T, List<TbRawMaterialStockRecord>>(Data);
        //    var materialCodes = dataList.Select(p => p.MaterialCode).Distinct().ToList();
        //    var stockRecords = Repository<TbRawMaterialStockRecord>.Query(p => p.MaterialCode.In(materialCodes)).ToList();
        //    var newStockRecords = new List<TbRawMaterialStockRecord>();
        //    foreach (var x in dataList)
        //    {
        //        TbRawMaterialStockRecord stockRecord = null;
        //        if (stockRecords.Any())
        //        {
        //            stockRecord = stockRecords.First(p => p.SiteCode == x.SiteCode
        //                                               //&& p.TestReportNo == x.TestReportNo
        //                                               //&& p.StorageCode == x.StorageCode
        //                                               && p.ProjectId == x.ProjectId
        //                                               && p.MaterialCode == x.MaterialCode
        //                                               && p.Factory == x.Factory
        //                                               && p.BatchNumber == x.BatchNumber);
        //        }
        //        if (stockRecord == null)
        //        {
        //            if (model.OperationType == OperationEnum.OperationType.Reduce || model.OperationType == OperationEnum.OperationType.CancelAdd)
        //                continue;
        //            stockRecord = x;
        //            stockRecord.ID = 0;
        //            stockRecord.State = 0;
        //            stockRecord.Count = 0;
        //            stockRecord.UseCount = 0;
        //            stockRecord.LockCount = 0;
        //            stockRecord.ProjectId = string.IsNullOrEmpty(x.ProjectId) ? model.ProjectId : x.ProjectId;
        //            //stockRecord.StorageCode = string.IsNullOrEmpty(x.StorageCode) ? model.StorageCode : x.StorageCode;
        //            stockRecord.SiteCode = string.IsNullOrEmpty(x.SiteCode) ? model.SiteCode : x.SiteCode;
        //        }
        //        else
        //        {
        //            decimal? lockCount = stockRecord.LockCount;
        //            switch (model.OperationType)
        //            {
        //                case OperationEnum.OperationType.Add://添加
        //                    lockCount += x.LockCount;
        //                    break;
        //                case OperationEnum.OperationType.Reduce://减少
        //                    lockCount -= x.LockCount;
        //                    if ((stockRecord.Count - lockCount) < 0)
        //                        continue;
        //                    break;
        //                case OperationEnum.OperationType.CancelAdd://取消增加
        //                    lockCount = lockCount - x.OldCount + x.LockCount;
        //                    break;
        //                case OperationEnum.OperationType.CancelReduce://取消减少
        //                    lockCount = lockCount + x.OldCount - x.LockCount;
        //                    if ((stockRecord.Count - lockCount) < 0)
        //                        continue;
        //                    break;
        //                default:
        //                    break;
        //            }
        //            stockRecord.LockCount = lockCount;
        //        }
        //        newStockRecords.Add(stockRecord);
        //    };
        //    return newStockRecords;
        //}

        #endregion

        /// <summary>
        /// 操作库存记录
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool OperationStockRecord(DbTrans trans, List<TbRawMaterialStockRecord> models)
        {
            try
            {
                var insertList = models.Where(p => p.ID == 0).ToList();
                var updateList = models.Where(p => p.ID > 0).ToList();
                //var deleteList = models.Where(p => p.ID > 0 && p.LockCount == 0).ToList();
                if (insertList.Any())
                    Repository<TbRawMaterialStockRecord>.Insert(trans, insertList);
                if (updateList.Any())
                    Repository<TbRawMaterialStockRecord>.Update(trans, updateList);
                //if (deleteList.Any())
                //    Repository<TbRawMaterialStockRecord>.Delete(trans, deleteList);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 获取数据列表(厂家，炉批号，质检报告)
        /// </summary>
        public PageModel GetDataListForPage(InOrderRequest request)
        {
            #region 搜索条件

            List<Parameter> parameter = new List<Parameter>();
            string where = " where 1=1 and Factory IS NOT NULL and BatchNumber IS NOT NULL and (a.UseCount+isnull(d.LockCount,0))>0 and a.ChackState=1";
            if (!string.IsNullOrWhiteSpace(request.keyword))
            {
                where += " and c.CompanyFullName like @keyword";
                parameter.Add(new Parameter("@keyword", '%' + request.keyword + '%', DbType.String, null));
            }
            if (!string.IsNullOrWhiteSpace(request.MaterialCode))
            {
                where += " and a.MaterialCode=@MaterialCode";
                parameter.Add(new Parameter("@MaterialCode", request.MaterialCode, DbType.String, null));
            }
            if (!string.IsNullOrEmpty(request.ProjectId))
            {
                where += " and a.ProjectId=@ProjectId";
                parameter.Add(new Parameter("@ProjectId", request.ProjectId, DbType.String, null));
            }
            if (!string.IsNullOrWhiteSpace(request.WorkAreaCode))
            {
                where += " and a.WorkAreaCode=@WorkAreaCode";
                parameter.Add(new Parameter("@WorkAreaCode", request.WorkAreaCode, DbType.String, null));
            }
            if (!string.IsNullOrWhiteSpace(request.CollarCode))
            {
                parameter.Add(new Parameter("@CollarCode", request.CollarCode, DbType.String, null));
            }
            parameter.Add(new Parameter("@WorkOrderItemId", request.WorkOrderItemId, DbType.Int32, null));
            #endregion

            var sql = @"select 
                                 c.CompanyFullName as InOrderSiteName,
                                 a.InsertTime as InOrderTime,
                                 a.ID as InOrderItemId,
                                 a.Factory,
                                 a.BatchNumber,
                                 a.TestReportNo,
                                 a.Count,
                                 (a.UseCount+isnull(d.LockCount,0)) as PassCount
                                from TbRawMaterialStockRecord a
                                left join TbCompany c on a.SiteCode=c.CompanyCode
                                LEFT JOIN (
                                	SELECT  StockRecordId,SUM(LockCount) AS LockCount FROM TbRMProductionMaterialLockCount
                                	WHERE SourceType=1 AND CollarCode=@CollarCode AND WorkOrderItemId=@WorkOrderItemId
                                	GROUP BY StockRecordId
                                ) AS d ON a.ID=d.StockRecordId";
            try
            {
                var data = Repositorys<RawMaterialStockRecordResponse>.FromSqlToPage(sql + where, parameter, request.rows, request.page, "InOrderTime");
                if (data.total == 0)
                {
                    //查找最近的炉批号
                    var RawMaterial = GetLastRecordForRawMaterial(request);
                    if (RawMaterial != null)
                    {
                        var a = new RawMaterialStockRecordResponse()
                        {
                            InOrderTime = RawMaterial.InsertTime,
                            InOrderItemId = RawMaterial.ID,
                            Factory = RawMaterial.Factory,
                            BatchNumber = RawMaterial.BatchNumber,
                            TestReportNo = RawMaterial.TestReportNo,
                            Count = RawMaterial.Count,
                            PassCount = RawMaterial.UseCountS.Value
                        };
                        var b = Repository<TbCompany>.First(p => p.CompanyCode == RawMaterial.SiteCode);
                        if (b != null)
                            a.InOrderSiteName = b.ParentCompanyName;
                        data.rows = new List<RawMaterialStockRecordResponse>() { a };
                    }
                }
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 查找最后入库的记录
        /// </summary>
        /// <returns></returns>
        public static TbRawMaterialStockRecord GetLastRecordForRawMaterial(InOrderRequest request)
        {
            var stockRecordList = Repository<TbRawMaterialStockRecord>.Query(p => p.MaterialCode == request.MaterialCode
                                                                           && p.ProjectId == request.ProjectId
                                                                           && p.WorkAreaCode == request.WorkAreaCode
                                                                           && p.ChackState == 1
                                                                           )
                                                                  .OrderBy(p => p.SiteCode).ThenByDescending(p => p.InsertTime).ToList();
            if (!stockRecordList.Any())
                return null;
            var stockRecord = stockRecordList[0];
            stockRecord.UseCountS = stockRecordList.Sum(p => p.LockCount) + stockRecordList.Sum(p => p.UseCount);
            return stockRecord;
        }
    }
}
