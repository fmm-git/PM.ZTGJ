using Dos.Common;
using Dos.ORM;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Business
{
    public class TbSysMenuTableLogic
    {
        #region 新增数据

        /// <summary>
        /// 新增数据(单条)
        /// </summary>
        public AjaxResult Insert(TbSysMenuTableRequset request)
        {
            if (request == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                var model = MapperHelper.Map<TbSysMenuTableRequset, TbSysMenuTable>(request);
                var count = Repository<TbSysMenuTable>.Insert(model);
                if (count > 0)
                {
                    return AjaxResult.Success();
                }
                return AjaxResult.Error("操作失败");
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败");
            }

        }

        #endregion

        #region 修改数据

        /// <summary>
        /// 修改数据(单条)
        /// </summary>
        public AjaxResult Update(TbSysMenuTableRequset request)
        {
            if (request == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                var model = MapperHelper.Map<TbSysMenuTableRequset, TbSysMenuTable>(request);
                if (model == null)
                    return AjaxResult.Error("信息不存在"); 
                var count = Repository<TbSysMenuTable>.Update(model);
                if (count > 0)
                {
                    return AjaxResult.Success();
                }
                return AjaxResult.Error();
            }
            catch (Exception)
            {
                return AjaxResult.Error();
            }

        }

        #endregion

        #region 删除数据

        /// <summary>
        /// 删除数据
        /// </summary>
        public AjaxResult Delete(int ID)
        {
            try
            {
                var count = Repository<TbSysMenuTable>.Delete(t => t.ID == ID);
                if (count > 0)
                {
                    return AjaxResult.Success();
                }
                return AjaxResult.Error();
            }
            catch (Exception)
            {
                return AjaxResult.Error();
            }
        }

        #endregion

        #region 获取数据

        public TbSysMenuTable FindEntity(int ID)
        {
            var model = Repository<TbSysMenuTable>.First(d => d.ID == ID);
            return model;
        }

        /// <summary>
        /// 获取数据列表(分页)
        /// </summary>
        public List<TbSysMenuTable> GetDataListForPage(TbSysMenuTableRequset param, string keyword)
        {
            //组装查询语句
            #region 模糊搜索条件

            var where = new Where<TbSysMenuTable>();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                where.And(d => d.MenuCode.Like(keyword));
                where.Or(d => d.TableName.Like(keyword));
            }
            //if (!string.IsNullOrWhiteSpace(param.TableName))
            //{
            //    where.And(d => d.TableName.Like(param.TableName));
            //}

            #endregion
            var orderBy = OrderByOperater.ASC;
            if (param.sord.Equals("desc"))
                orderBy = OrderByOperater.DESC;
            try
            {
                //取总数，以计算共多少页。
                var dateCount = Repository<TbSysMenuTable>.Count(where);
                var orderByClip = new OrderByClip(new Field(param.sidx), orderBy);//排序字段
                var list = Repository<TbSysMenuTable>.Query(where, orderByClip, param.sord, param.rows, param.page).ToList();
                param.records = dateCount;

                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion
    }
}
