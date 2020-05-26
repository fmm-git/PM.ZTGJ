using Dos.ORM;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using PM.DataEntity.Production;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Business.Production
{
    public class TbProcessingTechnologyLogic
    {
        #region 新增数据

        /// <summary>
        /// 新增数据
        /// </summary>
        public AjaxResult Insert(TbProcessingTechnology model)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            var ProTeList = Repository<TbProcessingTechnology>.Query(p => p.ProcessingTechnologyName == model.ProcessingTechnologyName && p.PID == model.PID);
            if (ProTeList.Count >= 1)
                return AjaxResult.Warning("改加工工艺已经存在");
            try
            {
                //添加信息及明细信息
                Repository<TbProcessingTechnology>.Insert(model);
                return AjaxResult.Success();
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
        public AjaxResult Update(TbProcessingTechnology model)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            var anyRet = AnyInfo(model.ID);
            if (anyRet.state.ToString() != ResultType.success.ToString())
                return anyRet;
            var ProTeList = Repository<TbProcessingTechnology>.Query(p => p.ID != model.ID && p.ProcessingTechnologyName == model.ProcessingTechnologyName && p.PID == model.PID);
            if (ProTeList.Count >= 1)
                return AjaxResult.Warning("改加工工艺已经存在");
            try
            {

                //修改信息
                Repository<TbProcessingTechnology>.Update(model, p => p.ID == model.ID);
                return AjaxResult.Success();
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
                Repository<TbProcessingTechnology>.Delete(p => p.ID == keyValue);
                return AjaxResult.Success();
            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.ToString());
            }
        }

        #region 判断

        /// <summary>
        /// 判断信息是否可操作
        /// </summary>
        /// <returns></returns>
        public AjaxResult AnyInfo(int keyValue)
        {
            var ProTeList = Repository<TbProcessingTechnology>.First(p => p.ID == keyValue);
            if (ProTeList == null)
                return AjaxResult.Warning("信息不存在");
            return AjaxResult.Success(ProTeList);
        }

        #endregion

        #endregion

        #region 获取数据

        /// <summary>
        /// 获取编辑数据
        /// </summary>
        /// <param name="dataID">数据Id</param>
        /// <returns></returns>
        public Tuple<DataTable> FindEntity(int dataID)
        {
            var ret = Db.Context.From<TbProcessingTechnology>()
            .Select(
                    TbProcessingTechnology._.All).Where(p => p.ID == dataID).ToDataTable();
            return new Tuple<DataTable>(ret);
        }

        /// <summary>
        /// 获取数据列表(分页)
        /// </summary>
        public PageModel GetDataListForPage(ProcessingTechnologyRequest request)
        {
            //组装查询语句
            #region 模糊搜索条件

            var where = new Where<TbProcessingTechnology>();
            if (!string.IsNullOrWhiteSpace(request.ProcessingTechnologyName))
            {
                where.And(p => p.ProcessingTechnologyName == request.ProcessingTechnologyName);
            }

            #endregion

            try
            {
                var ret = Db.Context.From<TbProcessingTechnology>()
           .Select(
                   TbProcessingTechnology._.All
                   , TbProcessingTechnology._.ProcessingTechnologyName.As("PProcessingTechnologyName"))
                 .LeftJoin<TbProcessingTechnology>((a, c) => a.PID == c.ID).Where(where).OrderByDescending(d => d.ID).ToPageList(request.rows, request.page);
                return ret;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<TbProcessingTechnology> GetChildList()
        {
            string sql = @"select * from (select * from TbProcessingTechnology where PID!=0 
                            union all
                            select 0,-1,0,'请选择') Tb order by Tb.Sort asc";
            var ret = Db.Context.FromSql(sql).ToList<TbProcessingTechnology>();
            return ret;
        }
        public List<TbProcessingTechnology> GetList(string keyword)
        {
            #region 模糊搜索条件

            var where = new Where<TbProcessingTechnology>();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                where.And(d => d.ProcessingTechnologyName.Like(keyword));
            }

            #endregion

            try
            {
                return Repository<TbProcessingTechnology>.Query(where, d => d.Sort, "asc").ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region 导出

        public DataTable GetExportList(ProcessingTechnologyRequest request) 
        {
            string sql = @"select Tb1.ProcessingTechnologyName as PProcessingTechnologyName,Tb2.ProcessingTechnologyName,Tb1.childCount from (select tb1.ID,tb1.ProcessingTechnologyName,tb1.Sort,COUNT(tb1.ID) as childCount from TbProcessingTechnology tb1
left join TbProcessingTechnology tb2 on tb1.ID=tb2.PID
where tb2.ID is not null group by tb1.ID,tb1.ProcessingTechnologyName,tb1.Sort) Tb1
left join TbProcessingTechnology Tb2 on Tb1.ID=Tb2.PID order by Tb1.Sort,Tb2.Sort asc";
            var data = Db.Context.FromSql(sql).ToDataTable();
            return data;
        }

        #endregion
    }
}
