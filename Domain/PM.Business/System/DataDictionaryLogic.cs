using Dos.Common;
using Dos.ORM;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PM.Common;

namespace PM.Business
{
    /// <summary>
    /// 数据字典处理
    /// </summary>
    public class DataDictionaryLogic
    {
        #region 数据字典查询处理

        /// <summary>
        /// 数据字典分类导航
        /// </summary>
        public List<TbSysDictionaryType> GetDataDictionary(string dataCode)
        {
            var where = new Where<TbSysDictionaryType>();
            if(!string.IsNullOrEmpty(dataCode))
            {
                where.And(t => t.DictionaryCode == dataCode);
            }
            return Repository<TbSysDictionaryType>.Query(where, d => d.DictionaryCode, "asc").ToList();
        }

        /// <summary>
        /// 根据分类编码进行查询字典信息
        /// </summary>
        /// <param name="vd"></param>
        /// <returns></returns>
        public List<TbSysDictionaryData> GetDicByCode(string dicCode)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("select * from TbSysDictionaryData where 1=1");
            if (!string.IsNullOrEmpty(dicCode))
            {
                sb.Append(" and FDictionaryCode=@name");
            }

            sb.Append(" order by DictionaryOrder asc");
            return Db.Context.FromSql(sb.ToString()).AddInParameter(
                "@name", DbType.String, dicCode).ToList<TbSysDictionaryData>();
        }

        /// <summary>
        /// 根据内容进行查询字典信息
        /// </summary>
        /// <param name="vd"></param>
        /// <returns></returns>
        public TbSysDictionaryData GetDicJsonByCode(string dicCode, string CodeType, string CodeText)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("select ");
            sb.Append("a.FDictionaryCode,");
            sb.Append("a.DictionaryCode,");
            sb.Append("a.DictionaryOrder,");
            sb.Append("a.DictionaryText,");
            sb.Append("a.id,");
            sb.Append("b.DictionaryType as DictionaryValue ");
            sb.Append(" from TbSysDictionaryData a left join TbSysDictionaryType b ");
            sb.Append("on a.FDictionaryCode=b.DictionaryCode where 1=1");
            if (!string.IsNullOrEmpty(dicCode))
            {
                sb.Append(" and a.DictionaryText = @name and a.FDictionaryCode=@type and a.DictionaryCode=@text");
            }
            return Db.Context.FromSql(sb.ToString()).AddInParameter(
                "@name", DbType.String, dicCode).AddInParameter("@type", DbType.String, CodeType).AddInParameter("@text", DbType.String, CodeText).ToFirst<TbSysDictionaryData>();
        }

        /// <summary>
        /// 根据编码查询字典分类
        /// </summary>
        /// <param name="menuCode"></param>
        /// <returns></returns>
        public TbSysDictionaryType GetFormJsonType(string keyValue)
        {
            var model = Repository<TbSysDictionaryType>.First(d => d.DictionaryCode == keyValue);
            return model;
        }

        #endregion

        #region 数据字典新增/编辑处理

        /// <summary>
        /// 新增字典类型数据
        /// </summary>
        public AjaxResult InsertType(TbSysDictionaryType tbDic)
        {
            if (tbDic == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                var any = Repository<TbSysDictionaryType>.Any(p=>p.DictionaryCode==tbDic.DictionaryCode);
                if(any)
                {
                    return AjaxResult.Warning("分类编码'"+tbDic.DictionaryCode+"'重复！");
                }
                any = Repository<TbSysDictionaryType>.Any(p => p.DictionaryType == tbDic.DictionaryType);
                if (any)
                {
                    return AjaxResult.Warning("分类名称'" + tbDic.DictionaryType + "'重复！");
                }
                var model = MapperHelper.Map<TbSysDictionaryType, TbSysDictionaryType>(tbDic);
                var count = Repository<TbSysDictionaryType>.Insert(model);
                if (count <= 0)
                    return AjaxResult.Error("数据错误,保存失败！");
                return AjaxResult.Success("保存成功");
            }
            catch (Exception e)
            {
                var err = e.ToString();
                return AjaxResult.Error(err); ;
            }
        }

        /// <summary>
        /// 修改字典类型数据
        /// </summary>
        public AjaxResult UpdateType(TbSysDictionaryType tbDic)
        {
            if (tbDic == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                var any = Repository<TbSysDictionaryType>.Any(p => p.DictionaryCode == tbDic.DictionaryCode && p.id != tbDic.id);
                if (any)
                {
                    return AjaxResult.Warning("分类编码'" + tbDic.DictionaryCode + "'重复！");
                }
                any = Repository<TbSysDictionaryType>.Any(p => p.DictionaryType == tbDic.DictionaryType && p.id != tbDic.id);
                if (any)
                {
                    return AjaxResult.Warning("分类名称'" + tbDic.DictionaryType + "'重复！");
                }
                var model = MapperHelper.Map<TbSysDictionaryType, TbSysDictionaryType>(tbDic);
                var count = Repository<TbSysDictionaryType>.Update(model);
                if (count <= 0)
                    return AjaxResult.Error("数据错误,编辑失败！");
                return AjaxResult.Success("修改成功");
            }
            catch (Exception e)
            {
                var err = e.ToString();
                return AjaxResult.Error("编辑失败！" + err);
            }
        }

        /// <summary>
        /// 新增字典数据
        /// </summary>
        public AjaxResult InsertData(TbSysDictionaryData tbDic)
        {
            if (tbDic == null)
                return AjaxResult.Warning("参数错误");
            var entity = Db.Context.From<TbSysDictionaryData>().Where(d => d.FDictionaryCode == tbDic.FDictionaryCode && d.DictionaryText == tbDic.DictionaryText).ToList().Count;
            if (entity > 0)
                return AjaxResult.Warning("字典内容重复");
            entity = Db.Context.From<TbSysDictionaryData>().Where(d => d.DictionaryCode == tbDic.DictionaryCode&&d.FDictionaryCode==tbDic.FDictionaryCode).ToList().Count;
            if (entity > 0)
                return AjaxResult.Warning("字典编码重复");
            try
            {
                var model = MapperHelper.Map<TbSysDictionaryData, TbSysDictionaryData>(tbDic);
                var count = Repository<TbSysDictionaryData>.Insert(model);
                if (count <= 0)
                    return AjaxResult.Error();
                return AjaxResult.Success();
            }
            catch (Exception e)
            {
                var err = e.ToString();
                return AjaxResult.Error(err);
            }
        }

        /// <summary>
        /// 修改字典数据
        /// </summary>
        public AjaxResult UpdateData(TbSysDictionaryData tbDic)
        {
            if (tbDic == null)
                return AjaxResult.Warning("参数错误");
            var entity = Db.Context.From<TbSysDictionaryData>().Where(d => d.FDictionaryCode == tbDic.FDictionaryCode && d.id != tbDic.id && d.DictionaryText == tbDic.DictionaryText).ToList().Count;
            if (entity > 0)
                return AjaxResult.Warning("字典内容重复");
            entity = Db.Context.From<TbSysDictionaryData>().Where(d => d.DictionaryCode == tbDic.DictionaryCode && d.FDictionaryCode == tbDic.FDictionaryCode && d.id != tbDic.id).ToList().Count;
            if (entity > 0)
                return AjaxResult.Warning("字典编码重复");
            try
            {
                var model = MapperHelper.Map<TbSysDictionaryData, TbSysDictionaryData>(tbDic);
                var count = Repository<TbSysDictionaryData>.Update(model);
                if (count <= 0)
                {
                    return AjaxResult.Error();
                }
                return AjaxResult.Success();
            }
            catch (Exception e)
            {
                var err = e.ToString();
                return AjaxResult.Error(err);
            }
        }

        #endregion

        #region 数据字典删除处理


        /// <summary>
        /// 删除数据
        /// </summary>
        public bool DeleteType(string depCode)
        {
            try
            {
                var count = Repository<TbSysDictionaryType>.Delete(t => t.DictionaryCode == depCode || t.PDictionaryCode==depCode);
                if (count <= 0)
                    return false;
                return true;
            }
            catch (Exception e)
            {
                var err = e.ToString();
                return false;
            }
        }

        /// <summary>
        /// 判断数据字典是否有下级
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public int IsTrue(string keyValue)
        {
            //新建查询where
            var where = new Where<TbSysDictionaryType>();
            //判断条件查询是否为空，不为空，条件添加进行查询
            if (!string.IsNullOrEmpty(keyValue))
            {
                where.And(t => t.PDictionaryCode == keyValue);
            }
            //查询总数
            var count = Repository<TbSysDictionaryType>.Count(where);
            return count;
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        public bool DeleteData(string depCode)
        {
            try
            {
                var count = Repository<TbSysDictionaryData>.Delete(t => t.id == Convert.ToInt32(depCode));
                if (count <= 0)
                    return false;
                return true;
            }
            catch (Exception e)
            {
                var err = e.ToString();
                return false;
            }
        }

        #endregion

    }
}
