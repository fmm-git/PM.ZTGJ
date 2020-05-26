using Dos.ORM;
using PM.Common;
using PM.Common.Helper;
using PM.Common.DataCache;
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
    public class TbAreaAdministrationLogic
    {
        public List<TbAreaAdministration> GetList(string keyword)
        {
            #region 模糊搜索条件

            var where = new Where<TbAreaAdministration>();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                //if (keyword.Trim()=="省"||keyword.Trim()=="市"||keyword.Trim()=="区"||keyword.Trim()=="县")
                //{
                //    return AjaxResult.Warning("输入关键字不正确");
                //}
                where.And(d => d.AreaCode.Like(keyword));
                where.Or(d => d.AreaName.Like(keyword));
            }

            #endregion
            try
            {

                return Repository<TbAreaAdministration>.Query(where, d => d.Sort, "asc").ToList();
            }
            catch (Exception)
            {
                return new List<TbAreaAdministration>();
            }
        }

        /// <summary>
        /// 获取数据列表(tree)
        /// </summary>
        public List<TbAreaAdministrationTreeReauset> GetListTreeBySearch(TbAreaAdministrationReauset request, string keyword)
        {
            string sql = @"select 
                             AreaCode
                            ,AreaCode as code
                            ,FK_AreaCode
                            ,AreaName
                            ,AreaName as name
                            ,FK_AreaCode_F
                            ,(select COUNT(*) from TbAreaAdministration where FK_AreaCode= a.AreaCode) as ChildCount 
                            from TbAreaAdministration a 
                     where 1=1";
            if (string.IsNullOrWhiteSpace(keyword))
            {
                if (!string.IsNullOrWhiteSpace(request.code))
                {
                    sql += " and a.FK_AreaCode=@pcode ";
                }
                else
                {
                    sql += " and a.FK_AreaCode is null ";
                }
                sql += "order by a.Sort";
                var list = Db.Context.FromSql(sql)
               .AddInParameter("@name", DbType.String, "%" + request.AreaName + "%")
               .AddInParameter("@pcode", DbType.String, request.code)
               .ToList<TbAreaAdministrationTreeReauset>();
                return list;
            }
            else {
                var area = Repository<TbAreaAdministration>.First(p => p.AreaCode == keyword || p.AreaName == keyword);
                if (area != null)
                {
                    //查找第一级code
                    var codeArry = area.FK_AreaCode_F.Split('.');
                    sql = @"select 
                             a.AreaCode
                            ,a.AreaCode as code
                            ,a.FK_AreaCode
                            ,a.AreaName
                            ,a.AreaName as name
                            ,a.FK_AreaCode_F
                            ,a.Sort
                             from TbAreaAdministration a 
                             where (a.AreaCode=@keyword or a.AreaCode=@keyword2 or a.FK_AreaCode_F like @keyword3)";
                    var list = Db.Context.FromSql(sql)
                  .AddInParameter("@keyword", DbType.String, codeArry[0])
                  .AddInParameter("@keyword2", DbType.String, area.FK_AreaCode)
                  .AddInParameter("@keyword3", DbType.String, area.FK_AreaCode_F+"%")
                  .ToList<TbAreaAdministrationTreeReauset>();
                    return list;
                }
                else { return null; }
            }
        }
        public List<TbAreaAdministration> GetParentAreaJosn(string AreaCode)
        {
            string sql = @"select * from TbAreaAdministration where FK_AreaCode is null or FK_AreaCode='" + AreaCode + "'";
            var model = Db.Context.FromSql(sql).ToDataTable();
            var modelList = ModelConvertHelper<TbAreaAdministration>.ToList(model);
            return modelList;
        }
        /// <summary>
        /// 获取当前选中区域的所有上级区域
        /// </summary>
        /// <param name="AreaCode"></param>
        /// <returns></returns>
        public AjaxResult GetAllParentAreaJosn(string AreaCode)
        {
            try
            {
                string AreaName = "";
                string a = AreaCode.TrimEnd('.');
                string[] b = a.Split('.');
                string c = "";
                if (b != null && b.Length > 0)
                {
                    for (int i = 0; i < b.Length; i++)
                    {
                        c += b[i] + ",";
                    }
                }
                string d = c.TrimEnd(',');

                string sql = @"select AreaName from TbAreaAdministration where 1=1 and AreaCode in(" + d + ") order by AreaCode asc";
                var model = Db.Context.FromSql(sql).ToDataTable();
                if (model != null && model.Rows.Count > 0)
                {
                    for (int i = 0; i < model.Rows.Count; i++)
                    {
                        AreaName += model.Rows[i]["AreaName"];
                    }
                }
                return AjaxResult.Success(AreaName);
            }
            catch (Exception)
            {
                return AjaxResult.Success("");
            }

        }

    }
}
