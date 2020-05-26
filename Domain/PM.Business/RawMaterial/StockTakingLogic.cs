using Dos.ORM;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using PM.DataEntity.RawMaterial.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Business.RawMaterial
{
    /// <summary>
    /// 库存盘点
    /// </summary>
    public class StockTakingLogic
    {
        #region 新增数据

        /// <summary>
        /// 新增数据
        /// </summary>
        public AjaxResult Insert(TbStockTaking model, List<TbStockTakingItem> items)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            model.InsertUserCode = OperatorProvider.Provider.CurrentUser.UserCode;
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //添加信息
                    var id = Repository<TbStockTaking>.Insert(trans, model);
                    //添加明细信息
                    Repository<TbStockTakingItem>.Insert(trans, items);
                    trans.Commit();
                    return AjaxResult.Success(id);
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
        public AjaxResult Update(TbStockTaking model, List<TbStockTakingItem> items)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            var anyRet = AnyInfo(model.ID);
            if (anyRet.state != ResultType.success.ToString())
                return anyRet;
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())//使用事务
                {
                    //修改信息
                    Repository<TbStockTaking>.Update(trans, model, p => p.ID == model.ID);
                    if (items.Count > 0)
                    {
                        //删除历史明细信息
                        Repository<TbStockTakingItem>.Delete(trans, p => p.TakNum == model.TakNum);
                        //添加明细信息
                        Repository<TbStockTakingItem>.Insert(trans, items);
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
                    var count = Repository<TbStockTaking>.Delete(trans, p => p.ID == keyValue);
                    //删除明细信息
                    Repository<TbStockTakingItem>.Delete(trans, p => p.TakNum == ((TbStockTaking)anyRet.data).TakNum);
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
            var ret = Db.Context.From<TbStockTaking>()
                .Select(
                       TbStockTaking._.All
                    , TbUser._.UserName
                    , TbCompany._.CompanyFullName.As("FactoryName")
                    , TbSysDictionaryData._.DictionaryText.As("WarehouseTypeName"))
                  .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                  .LeftJoin<TbCompany>((a, c) => a.ProcessFactoryCode == c.CompanyCode)
                  .LeftJoin<TbSysDictionaryData>((a, c) => a.WarehouseType == c.DictionaryCode)
                  .Where(p => p.ID == keyValue).ToDataTable();
            if (ret == null || ret.Rows.Count == 0)
                return new Tuple<object, object>(null, null);
            //查找明细信息
            var items = Db.Context.From<TbStockTakingItem>().Select(
                TbStockTakingItem._.All,
                TbSysDictionaryData._.DictionaryText.As("RebarTypeName"))
            .LeftJoin<TbSysDictionaryData>((a, c) => a.RebarType == c.DictionaryCode)
            .Where(p => p.TakNum == Convert.ToString(ret.Rows[0]["TakNum"]))
            .ToDataTable();
            return new Tuple<object, object>(ret, items);
        }

        /// <summary>
        /// 获取数据列表(分页)
        /// </summary>
        public PageModel GetDataListForPage(StockTakingRequest request)
        {

            #region 模糊搜索条件

            var where = new Where<TbStockTaking>();
            if (request.TakDayS.HasValue)
            {
                where.And(p => p.TakDay >= request.TakDayS);
            }
            if (request.TakDayE.HasValue)
            {
                where.And(p => p.TakDay <= request.TakDayE);
            }
            if (!string.IsNullOrWhiteSpace(request.WarehouseType))
            {
                where.And(p => p.WarehouseType == request.WarehouseType);
            }
            if (!string.IsNullOrWhiteSpace(request.TakNum))
            {
                where.And(p => p.TakNum.Like(request.TakNum));
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where.And(p => p.ProcessFactoryCode == request.ProcessFactoryCode);
            }
            #endregion

            try
            {
                var data = Db.Context.From<TbStockTaking>()
                    .Select(
                      TbStockTaking._.ID
                    , TbStockTaking._.TakNum
                    , TbStockTaking._.TakDay
                    , TbStockTaking._.TotalInventory
                    , TbStockTaking._.TotalTak
                    , TbStockTaking._.TotalEarnOrLos
                    , TbStockTaking._.EarnOrLos
                    , TbStockTaking._.Remarks
                    , TbStockTaking._.InsertUserCode
                    , TbStockTaking._.InsertTime
                    , TbUser._.UserName
                    , TbCompany._.CompanyFullName.As("FactoryName")
                    , TbSysDictionaryData._.DictionaryText.As("WarehouseTypeName"))
                  .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                  .LeftJoin<TbSysDictionaryData>((a, c) => a.WarehouseType == c.DictionaryCode)
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
        /// 判断信息是否可操作
        /// </summary>
        /// <returns></returns>
        public AjaxResult AnyInfo(int keyValue)
        {
            var StockTaking = Repository<TbStockTaking>.First(p => p.ID == keyValue);
            if (StockTaking == null)
                return AjaxResult.Warning("信息不存在");
            return AjaxResult.Success(StockTaking);
        }
        #endregion

        /// <summary>
        /// 获取明细表数据
        /// </summary>
        /// <param name="warehouseType"></param>
        /// <returns></returns>
        public DataTable GetItemData(string warehouseType, string factoryCode)
        {
            string sql = @"SELECT 
                              yclData.MaterialCode,
                              rma.MaterialName,
                              Convert(decimal(18,5),sum(yclData.lnventoryWeight)) AS lnventoryWeight,
                              rma.SpecificationModel,
                              rma.RebarType,
                              sdd.DictionaryText AS RebarTypeName
                              FROM(
                              SELECT  
                               MaterialCode,
                               ProcessFactoryCode,
                               sum(rmsr.COUNT*0.001) AS lnventoryWeight
                              FROM TbRawMaterialStockRecord rmsr
                              LEFT JOIN TbStorage ts ON rmsr.StorageCode=ts.StorageCode
                              WHERE ts.ProcessFactoryCode=@FactoryCode
                             GROUP BY  MaterialCode,ts.ProcessFactoryCode) yclData 
                             JOIN TbRawMaterialArchives rma ON yclData.MaterialCode=rma.MaterialCode
                             LEFT JOIN TbSysDictionaryData sdd ON rma.RebarType=sdd.DictionaryCode
                             GROUP BY yclData.ProcessFactoryCode,sdd.DictionaryText,yclData.MaterialCode,rma.SpecificationModel,rma.RebarType,rma.MaterialName
                             ORDER BY rma.MaterialName";
          
            if (warehouseType == "ResidualMaterial")  //余料
            {
                sql = @"SELECT 
                           yclData.MaterialCode,
                           rma.MaterialName,
                           ISNULL(Convert(decimal(18,5),sum(cs.[Weight]*0.001)),0) AS lnventoryWeight,
                           rma.SpecificationModel,
                           rma.RebarType,
                           sdd.DictionaryText AS RebarTypeName
                           FROM(
                           SELECT  
                            MaterialCode,
                            ProcessFactoryCode
                           FROM TbRawMaterialStockRecord rmsr
                           LEFT JOIN TbStorage ts ON rmsr.StorageCode=ts.StorageCode
                           WHERE ts.ProcessFactoryCode=@FactoryCode
                          GROUP BY  MaterialCode,ts.ProcessFactoryCode) yclData
                          LEFT JOIN TbCloutStock cs ON yclData.MaterialCode=cs.MaterialCode AND cs.ProcessFactoryCode=@FactoryCode 
                          JOIN TbRawMaterialArchives rma ON yclData.MaterialCode=rma.MaterialCode
                          LEFT JOIN TbSysDictionaryData sdd ON rma.RebarType=sdd.DictionaryCode
                          GROUP BY yclData.ProcessFactoryCode,sdd.DictionaryText,yclData.MaterialCode,rma.SpecificationModel,rma.RebarType,rma.MaterialName
                          ORDER BY rma.MaterialName";
            }
            var retData = Db.Context.FromSql(sql)
               .AddInParameter("FactoryCode", DbType.String, factoryCode)
               .ToDataTable();
            return retData;
        }

        #region 导出
        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public DataTable GetExportList(StockTakingRequest request)
        {

            #region 模糊搜索条件

            var where = new Where<TbStockTaking>();
            if (request.TakDayS.HasValue)
            {
                where.And(p => p.TakDay >= request.TakDayS);
            }
            if (request.TakDayE.HasValue)
            {
                where.And(p => p.TakDay <= request.TakDayE);
            }
            if (!string.IsNullOrWhiteSpace(request.WarehouseType))
            {
                where.And(p => p.WarehouseType == request.WarehouseType);
            }
            if (!string.IsNullOrWhiteSpace(request.TakNum))
            {
                where.And(p => p.TakNum.Like(request.TakNum));
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where.And(p => p.ProcessFactoryCode == request.ProcessFactoryCode);
            }
            #endregion

            try
            {
                var data = Db.Context.From<TbStockTaking>()
                    .Select(
                      TbStockTaking._.ID
                    , TbStockTaking._.TakNum
                    , TbStockTaking._.TakDay
                    , TbStockTaking._.TotalInventory
                    , TbStockTaking._.TotalTak
                    , TbStockTaking._.TotalEarnOrLos
                    , TbStockTaking._.EarnOrLos
                    , TbStockTaking._.Remarks
                    , TbStockTaking._.InsertUserCode
                    , TbStockTaking._.InsertTime
                    , TbUser._.UserName
                    , TbCompany._.CompanyFullName.As("FactoryName")
                    , TbSysDictionaryData._.DictionaryText.As("WarehouseTypeName"))
                  .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                  .LeftJoin<TbSysDictionaryData>((a, c) => a.WarehouseType == c.DictionaryCode)
                  .LeftJoin<TbCompany>((a, c) => a.ProcessFactoryCode == c.CompanyCode)
                  .Where(where)
                  .OrderByDescending(p => p.ID).ToDataTable();
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion
    }
}
