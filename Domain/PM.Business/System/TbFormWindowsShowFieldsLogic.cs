using Dos.Common;
using Dos.ORM;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Business
{
    public class TbFormWindowsShowFieldsLogic
    {
        #region 获取列表

        /// <summary>
        /// 获取不分页数据列表
        /// </summary>
        public List<TbFormWindowsShowFields> GetNoPageGridList()
        {
            string sqlStr = @"select PhysicalTableName,FormWindowsTableName from TbFormWindowsShowFields where FormWindowsTableName is not null
group by PhysicalTableName,FormWindowsTableName Order by PhysicalTableName asc";
            try
            {
                var model = Db.Context.FromSql(sqlStr).ToList<TbFormWindowsShowFields>();
                return model;
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// 获取单条数据
        /// </summary>
        /// <param name="RoleCode"></param>
        /// <returns></returns>
        public TbFormWindowsShowFields FindEntity(string keyValue, string keyValue1)
        {
            var model = Repository<TbFormWindowsShowFields>.First(d => d.PhysicalTableName == keyValue&&d.FieldCode==keyValue1);
            return model;
        }

        /// <summary>
        /// 获取数据列表(分页)
        /// </summary>
        public PageModel GetDataListForPage(PageSearchRequest pt, string PhysicalTable, string keyword)
        {
            string sqlStr = @"select * from TbFormWindowsShowFields";
            string whereStr = " where 1=1 and FormWindowsTableName is not null and (ISNULL('" + PhysicalTable + "','')='' or PhysicalTableName='" + PhysicalTable + "')";

            #region 模糊搜索条件
            var wherep = new List<Dos.ORM.Parameter>();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                whereStr += " and (PhysicalTableName like '%'+@keyword+'%' or FieldName like '%'+@keyword+'%' or FieldCode like '%'+@keyword+'%')";
                var p = new Dos.ORM.Parameter("@keyword", keyword, DbType.String, null);
                wherep.Add(p);
            }
            #endregion

            try
            {
                //取总数，以计算共多少页。
                var count = Repository<TbFormWindowsShowFields>.FromSql(sqlStr + whereStr, wherep).Count();
                var model = Repository<TbFormWindowsShowFields>.FromSql(sqlStr + whereStr, wherep, "PhysicalTableName,FieldShowOrder", pt.sord,pt.rows,pt.page).ToList();
                var model1 = new PageModel(pt.page, pt.rows, count, model);
                return model1;
            }
            catch (Exception)
            {
                throw;
            }
           
        }

        /// <summary>
        /// 获取不分页数据列表
        /// </summary>
        public List<TbFormWindowsShowFields> GetGridHeadList(string tableName)
        {
            var where = new Where<TbFormWindowsShowFields>();
            if (!string.IsNullOrEmpty(tableName))
            {
                where.And(d => d.PhysicalTableName == tableName);
            }
            return Repository<TbFormWindowsShowFields>.Query(where, d => d.FieldShowOrder, "asc").ToList();
        }

        #endregion

        #region 新增数据

        /// <summary>
        /// 新增数据(单条)
        /// </summary>
        public AjaxResult Insert(TbFormWindowsShowFields request)
        {
            if (request == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                var model = MapperHelper.Map<TbFormWindowsShowFields, TbFormWindowsShowFields>(request);
                var count = Repository<TbFormWindowsShowFields>.Insert(model);
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

        #region 修改数据

        /// <summary>
        /// 修改数据(单条)
        /// </summary>
        public AjaxResult Update(TbFormWindowsShowFields request)
        {
            if (request == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                var model = Repository<TbFormWindowsShowFields>.First(p => p.PhysicalTableName == request.PhysicalTableName&&p.FieldCode==request.FieldCode);
                if (model == null)
                    return AjaxResult.Error("信息不存在");
                model.FieldCode = request.FieldCode;
                model.FieldName = request.FieldName;
                model.FieldShowOrder = request.FieldShowOrder;
                model.PhysicalTableName = request.PhysicalTableName;
                model.FieldIsShow = request.FieldIsShow;
                model.FieldWidth = request.FieldWidth;
                model.FormWindowsTableName = request.FormWindowsTableName;
                var count = Repository<TbFormWindowsShowFields>.Update(model);
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
        public AjaxResult Delete(string PhysicalTableName, string FieldCode)
        {
            try
            {
                var count = Repository<TbFormWindowsShowFields>.Delete(t => t.PhysicalTableName == PhysicalTableName&&t.FieldCode== FieldCode);
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
    }
}
