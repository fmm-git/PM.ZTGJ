using Dos.ORM;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using PM.DataEntity.CostManage.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;

namespace PM.Business.CostManage
{
    public class TbMonthCostHeSuanLogic
    {
        #region 新增数据

        /// <summary>
        /// 新增数据
        /// </summary>
        public AjaxResult Insert(TbMonthCostHeSuan model, TbMonthCostHeSuanIncome incomeModel, List<TbMonthCostHeSuanCost> costModel, List<TbMonthCostHeSuanOtherCost> otherCostModel)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            //判断信息是否存在
            var anyRet = AnyInfoType(model.HeSuanMonth, model.ProcessFactoryCode, model.HeSuanCode);
            if (anyRet.state.ToString() != ResultType.success.ToString())
                return anyRet;
            model.InsertUserCode = OperatorProvider.Provider.CurrentUser.UserCode;
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //添加信息
                    Repository<TbMonthCostHeSuan>.Insert(trans, model);
                    //添加信息
                    Repository<TbMonthCostHeSuanIncome>.Insert(trans, incomeModel);
                    //添加明细信息
                    Repository<TbMonthCostHeSuanCost>.Insert(trans, costModel);
                    if (otherCostModel.Count>0)
                    {
                        //添加明细信息
                        Repository<TbMonthCostHeSuanOtherCost>.Insert(trans, otherCostModel);
                    }
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
        public AjaxResult Update(TbMonthCostHeSuan model, TbMonthCostHeSuanIncome incomeModel, List<TbMonthCostHeSuanCost> costModel, List<TbMonthCostHeSuanOtherCost> otherCostModel)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            var anyRet = AnyInfo(model.ID);
            if (anyRet.state.ToString() != ResultType.success.ToString())
                return anyRet;
            var anyRetType = AnyInfoType(model.HeSuanMonth, model.ProcessFactoryCode, model.HeSuanCode);
            if (anyRetType.state.ToString() != ResultType.success.ToString())
                return anyRetType;
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())//使用事务
                {
                    //修改信息
                    Repository<TbMonthCostHeSuan>.Update(trans, model, p => p.ID == model.ID);
                    //修改信息
                    Repository<TbMonthCostHeSuanIncome>.Update(trans, incomeModel, p => p.HeSuanCode == model.HeSuanCode);
                    if (costModel.Count > 0)
                    {
                        //删除历史明细信息
                        Repository<TbMonthCostHeSuanCost>.Delete(trans, p => p.HeSuanCode == model.HeSuanCode);
                        //添加明细信息
                        Repository<TbMonthCostHeSuanCost>.Insert(trans, costModel);
                    }
                    if (otherCostModel.Count > 0)
                    {
                        //删除历史明细信息
                        Repository<TbMonthCostHeSuanOtherCost>.Delete(trans, p => p.HeSuanCode == model.HeSuanCode);
                        //添加明细信息
                        Repository<TbMonthCostHeSuanOtherCost>.Insert(trans, otherCostModel);
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
                    Repository<TbMonthCostHeSuan>.Delete(trans, p => p.ID == keyValue);
                    Repository<TbMonthCostHeSuanIncome>.Delete(trans, p => p.HeSuanCode == ((TbMonthCostHeSuan)anyRet.data).HeSuanCode);
                    Repository<TbMonthCostHeSuanCost>.Delete(trans, p => p.HeSuanCode == ((TbMonthCostHeSuan)anyRet.data).HeSuanCode);
                    Repository<TbMonthCostHeSuanOtherCost>.Delete(trans, p => p.HeSuanCode == ((TbMonthCostHeSuan)anyRet.data).HeSuanCode);
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

        #region 判断
        /// <summary>
        /// 判断信息是否可操作
        /// </summary>
        /// <returns></returns>
        public AjaxResult AnyInfo(int keyValue)
        {
            var monthCostHeSuan = Repository<TbMonthCostHeSuan>.First(p => p.ID == keyValue);
            if (monthCostHeSuan == null)
                return AjaxResult.Warning("信息不存在");
            return AjaxResult.Success(monthCostHeSuan);
        }

        public AjaxResult AnyInfoType(int? HeSuanMonth, string ProcessFactoryCode, string HeSuanCode)
        {
            try
            {
                var monthCostHeSuan = Repository<TbMonthCostHeSuan>.First(p => p.HeSuanMonth == HeSuanMonth && p.ProcessFactoryCode == ProcessFactoryCode && p.HeSuanCode != HeSuanCode);
                if (monthCostHeSuan != null)
                    return AjaxResult.Warning("该加工厂在当月已经录入月成本核算信息，请不要重复录入");
                return AjaxResult.Success();
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region 获取数据

        /// <summary>
        /// 获取编辑数据
        /// </summary>
        /// <param name="dataID">数据Id</param>
        /// <returns></returns>
        public Tuple<DataTable, DataTable, DataTable, DataTable> FindEntity(int dataID)
        {
            //月成本核算主表
            var ret = Db.Context.From<TbMonthCostHeSuan>()
            .Select(
                    TbMonthCostHeSuan._.All
                    , TbCompany._.CompanyFullName.As("ProcessFactoryName")
                    , TbUser._.UserName)
                  .LeftJoin<TbCompany>((a, c) => a.ProcessFactoryCode == c.CompanyCode)
                  .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                  .Where(p => p.ID == dataID).ToDataTable();
            //月成本收入表
            var incomeModel = Db.Context.From<TbMonthCostHeSuanIncome>().Select(
               TbMonthCostHeSuanIncome._.All)
           .Where(p => p.HeSuanCode == ret.Rows[0]["HeSuanCode"].ToString()).ToDataTable();
            //月成本核算成本表
            var costModel = Db.Context.From<TbMonthCostHeSuanCost>().Select(
               TbMonthCostHeSuanCost._.All,
               TbSysDictionaryData._.DictionaryText.As("CostProjectName"))
               .LeftJoin<TbSysDictionaryData>((a, c) => a.CostType ==c.DictionaryCode&&c.FDictionaryCode== "CostProject")
           .Where(p => p.HeSuanCode == ret.Rows[0]["HeSuanCode"].ToString()).ToDataTable();
            //月成本核算其他成本表
            var otherCostModel = Db.Context.From<TbMonthCostHeSuanOtherCost>().Select(
               TbMonthCostHeSuanOtherCost._.All)
           .Where(p => p.HeSuanCode == ret.Rows[0]["HeSuanCode"].ToString()).ToDataTable();
            return new Tuple<DataTable, DataTable, DataTable, DataTable>(ret, incomeModel, costModel, otherCostModel);
        }

        /// <summary>
        /// 获取数据列表(分页)
        /// </summary>
        public PageModel GetDataListForPage(MonthCostHeSuanRequest request)
        {
            //组装查询语句
            #region 模糊搜索条件

            var where = new Where<TbMonthCostHeSuan>();

            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where.And(p => p.ProcessFactoryCode == request.ProcessFactoryCode);
            }
            if (!string.IsNullOrWhiteSpace(request.HeSuanCode))
            {
                where.And(p => p.HeSuanCode == request.HeSuanCode);
            }

            #endregion

            try
            {
                var ret = Db.Context.From<TbMonthCostHeSuan>()
                .Select(
                      TbMonthCostHeSuan._.All
                      , TbCompany._.CompanyFullName.As("ProcessFactoryName")
                      , TbUser._.UserName)
                      .LeftJoin<TbCompany>((a, c) => a.ProcessFactoryCode == c.CompanyCode)
                      .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                      .Where(where).OrderByDescending(d => d.ID).ToPageList(request.rows, request.page);
                return ret;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 获取成本项目
        /// </summary>
        public DataTable GetCostDataList()
        {
            var where = new Where<TbSysDictionaryData>();
            where.And(p => p.FDictionaryCode == "CostProject");
            try
            {
                var data = Db.Context.From<TbSysDictionaryData>()
                     .Select(
                       TbSysDictionaryData._.id
                     , TbSysDictionaryData._.DictionaryCode.As("CostType")
                     , TbSysDictionaryData._.DictionaryText.As("CostProjectName"))
                     .Where(where)
                     .ToDataTable();
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Tuple<DataTable> GetOtherCostList(string HeSuanCode, int? addType) 
        {
            if (addType==9)
            {
                addType = 1;
            }
            else
            {
                addType = 2;
            }
            //月成本核算其他成本表
            var otherCostModel = Db.Context.From<TbMonthCostHeSuanOtherCost>().Select(
               TbMonthCostHeSuanOtherCost._.All)
           .Where(p => p.HeSuanCode == HeSuanCode&&p.CostType==addType).ToDataTable();
            return new Tuple<DataTable>(otherCostModel);
        }

        #endregion

        #region 导出

        /// <summary>
        /// 获取数据列表(分页)
        /// </summary>
        public DataTable GetExportList(MonthCostHeSuanRequest request)
        {
            //组装查询语句
            #region 模糊搜索条件

            var where = new Where<TbMonthCostHeSuan>();

            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where.And(p => p.ProcessFactoryCode == request.ProcessFactoryCode);
            }
            if (!string.IsNullOrWhiteSpace(request.HeSuanCode))
            {
                where.And(p => p.HeSuanCode == request.HeSuanCode);
            }

            #endregion

            try
            {
                var ret = Db.Context.From<TbMonthCostHeSuan>()
                .Select(
                      TbMonthCostHeSuan._.All
                      , TbCompany._.CompanyFullName.As("ProcessFactoryName")
                      , TbUser._.UserName)
                      .LeftJoin<TbCompany>((a, c) => a.ProcessFactoryCode == c.CompanyCode)
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
    }
}
