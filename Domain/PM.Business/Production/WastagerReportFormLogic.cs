using Dos.ORM;
using PM.Common;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using PM.DataEntity.Production.ViewModel;
using System;
using System.Collections.Generic;

namespace PM.Business.Production
{
    /// <summary>
    /// 耗材管理
    /// </summary>
    public class WastagerReportFormLogic
    {
        #region 获取数据 查询
        /// <summary>
        /// 获取编辑数据
        /// </summary>
        /// <param name="dataID">数据Id</param>
        /// <returns></returns>
        public Tuple<DataTable> FindEntity(int dataID)
        {
            var ret = Db.Context.From<TbWastagerReportForm>()
            .Select(
                    TbWastagerReportForm._.All
                     , TbUser._.UserName
                    , TbSysDictionaryData._.DictionaryText.As("MeasurementUnitNew"))
                    .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                .LeftJoin<TbSysDictionaryData>((a, c) => a.MeasureUnit == c.DictionaryCode)
                    .Where(p => p.ID == dataID).ToDataTable();
            //查找明细信息
            return new Tuple<DataTable>(ret);
        }

        /// <summary>
        /// 获取数据列表(分页)
        /// </summary>
        public PageModel GetDataListForPage(WastagerReportFormRequest request)
        {
            //组装查询语句
            #region 模糊搜索条件

            var where = new Where<TbWastagerReportForm>();
            if (!string.IsNullOrWhiteSpace(request.MaterialName))
            {
                where.And(p => p.MaterialName.Like(request.MaterialName));
            }

            if (!string.IsNullOrWhiteSpace(request.SpecificationType))
            {
                where.And(p => p.SpecificationType.Like(request.SpecificationType));
            }

            if (!string.IsNullOrEmpty(request.ProjectId))
                where.And(p => p.ProjectId == request.ProjectId);

            #endregion
            try
            {
                var ret = Db.Context.From<TbWastagerReportForm>()
              .Select(
                      TbWastagerReportForm._.All
                      , TbUser._.UserName
                      , TbSysDictionaryData._.DictionaryText.As("MeasurementUnitNew"))
                    .LeftJoin<TbSysDictionaryData>((a, c) => a.MeasureUnit == c.DictionaryCode && c.FDictionaryCode == "Unit")
                    .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                      .Where(where).OrderBy(d => d.ID).ToPageList(request.rows, request.page);
                return ret;
            }
            catch (Exception)
            {
                throw;
            }
        }


        #endregion

        #region 新增数据
        /// <summary>
        /// 新增数据
        /// </summary>
        public AjaxResult Insert(TbWastagerReportForm model)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            model.InsertUserCode = OperatorProvider.Provider.CurrentUser.UserCode;
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //添加信息
                    Repository<TbWastagerReportForm>.Insert(trans, model);
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
        public AjaxResult Update(TbWastagerReportForm model)
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
                    Repository<TbWastagerReportForm>.Update(trans, model, p => p.ID == model.ID);
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
        public AjaxResult Delete(int dataID)
        {
            try
            {
                var count = Repository<TbWastagerReportForm>.Delete(p => p.ID == dataID);
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

        #region 判断
        /// <summary>
        /// 判断信息是否可操作
        /// </summary>
        /// <returns></returns>
        public AjaxResult AnyInfo(int keyValue)
        {
            var monthDemandPlan = Repository<TbWastagerReportForm>.First(p => p.ID == keyValue);
            if (monthDemandPlan == null)
                return AjaxResult.Warning("信息不存在");
            //if (monthDemandPlan.Examinestatus != "未发起")
            //    return AjaxResult.Warning("信息正在审核中或已审核完成,不能进行此操作");

            return AjaxResult.Success(monthDemandPlan);
        }

        #endregion

    }
}
