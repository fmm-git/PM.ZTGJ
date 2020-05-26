using Dos.ORM;
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
    public class TbRawMaterialArchivesLogic
    {
        #region 新增数据

        /// <summary>
        /// 新增数据
        /// </summary>
        public AjaxResult Insert(TbRawMaterialArchives model)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                var count = Repository<TbRawMaterialArchives>.Insert(model);
                if (count > 0)
                {
                    return AjaxResult.Success();
                }
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
        public AjaxResult Input(List<TbRawMaterialArchives> model, StringBuilder errorMsg)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");

            var addList = new List<TbRawMaterialArchives>();
            foreach (var item in model)
            {  
                //原材料
                var materialCodes = model.Select(p => p.MaterialCode).ToList();
                var rawMaterials = Repository<TbRawMaterialArchives>.Query(p => p.MaterialCode.In(materialCodes)).ToList();
                //判断原材料是否存在
                var rawMaterial = rawMaterials.FirstOrDefault(p => p.MaterialCode == item.MaterialCode);
                if (rawMaterial != null)
                {
                    errorMsg.AppendFormat("第{0}行原材料【{1}】信息已经存在！", item.IndexNum, item.MaterialName);
                    continue;
                }
                addList.Add(item);
            }
            if (addList.Count == 0)
                return AjaxResult.Error(errorMsg.ToString());
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //添加信息
                    if (addList.Count > 0)
                        Repository<TbRawMaterialArchives>.Insert(trans, addList);
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
        public AjaxResult Update(TbRawMaterialArchives model)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                var count = Repository<TbRawMaterialArchives>.Update(model);
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
        public AjaxResult Delete(int dataID)
        {
            try
            {
                var count = Repository<TbRawMaterialArchives>.Delete(p => p.ID == dataID);
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
        /// <param name="dataID">数据Id</param>
        /// <returns></returns>
        public Tuple<DataTable> FindEntity(int dataID)
        {
            var ret = Db.Context.From<TbRawMaterialArchives>()
              .Select(
                      TbRawMaterialArchives._.All
                      , TbSysDictionaryData._.DictionaryText.As("MeasurementUnitNew"))
                      .LeftJoin<TbSysDictionaryData>((a, c) => a.MeasurementUnit == c.DictionaryCode && c.FDictionaryCode == "Unit")
                      .AddSelect(Db.Context.From<TbSysDictionaryData>().Select(p => p.DictionaryText)
                      .Where(TbSysDictionaryData._.DictionaryCode == TbRawMaterialArchives._.RebarType && TbSysDictionaryData._.FDictionaryCode == "RebarType"), "RebarTypeNew")
                      .Where(p => p.ID == dataID).ToDataTable();
            return new Tuple<DataTable>(ret);
        }

        /// <summary>
        /// 获取数据列表(分页)
        /// </summary>
        public PageModel GetDataListForPage(RawMarchivesRequest request)
        {
            //组装查询语句
            #region 模糊搜索条件

            var where = new Where<TbRawMaterialArchives>();
            if (!string.IsNullOrWhiteSpace(request.MaterialCode))
            {
                where.And(p => p.MaterialCode.Like(request.MaterialCode));
            }
            if (!string.IsNullOrWhiteSpace(request.MaterialName))
            {
                where.And(p => p.MaterialName.Like(request.MaterialName));
            }
            if (!string.IsNullOrWhiteSpace(request.SpecificationModel))
            {
                where.And(p => p.SpecificationModel.Like(request.SpecificationModel));
            }
            if (!string.IsNullOrWhiteSpace(request.RebarType))
            {
                 where.And(p => p.RebarType==request.RebarType);
            }
            #endregion
            try
            {
                var ret = Db.Context.From<TbRawMaterialArchives>()
              .Select(
                      TbRawMaterialArchives._.All
                      , TbSysDictionaryData._.DictionaryText.As("MeasurementUnitNew"))
                      .LeftJoin<TbSysDictionaryData>((a, c) => a.MeasurementUnit == c.DictionaryCode && c.FDictionaryCode == "Unit")
                      .AddSelect(Db.Context.From<TbSysDictionaryData>().Select(p => p.DictionaryText)
                      .Where(TbSysDictionaryData._.DictionaryCode == TbRawMaterialArchives._.RebarType && TbSysDictionaryData._.FDictionaryCode == "RebarType"), "RebarTypeNew")
                      .Where(where).OrderBy(d => d.ID).ToPageList(request.rows, request.page);
                return ret;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region 导出

        public DataTable GetExportList(RawMarchivesRequest request) 
        {
            //组装查询语句
            #region 模糊搜索条件

            var where = new Where<TbRawMaterialArchives>();
            if (!string.IsNullOrWhiteSpace(request.MaterialCode))
            {
                where.And(p => p.MaterialCode.Like(request.MaterialCode));
            }
            if (!string.IsNullOrWhiteSpace(request.MaterialName))
            {
                where.And(p => p.MaterialName.Like(request.MaterialName));
            }
            if (!string.IsNullOrWhiteSpace(request.SpecificationModel))
            {
                where.And(p => p.SpecificationModel.Like(request.SpecificationModel));
            }
            if (!string.IsNullOrWhiteSpace(request.RebarType))
            {
                where.And(p => p.RebarType == request.RebarType);
            }
            #endregion
            try
            {
                var ret = Db.Context.From<TbRawMaterialArchives>()
              .Select(
                      TbRawMaterialArchives._.All
                      , TbSysDictionaryData._.DictionaryText.As("MeasurementUnitNew"))
                      .LeftJoin<TbSysDictionaryData>((a, c) => a.MeasurementUnit == c.DictionaryCode && c.FDictionaryCode == "Unit")
                      .AddSelect(Db.Context.From<TbSysDictionaryData>().Select(p => p.DictionaryText)
                      .Where(TbSysDictionaryData._.DictionaryCode == TbRawMaterialArchives._.RebarType && TbSysDictionaryData._.FDictionaryCode == "RebarType"), "RebarTypeNew")
                      .Where(where).OrderBy(d => d.ID).ToDataTable();
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
