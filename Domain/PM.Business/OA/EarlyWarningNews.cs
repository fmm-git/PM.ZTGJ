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

namespace PM.Business.OA
{
    public class EarlyWarningNews
    {
        #region 新增数据

        /// <summary>
        /// 新增数据
        /// </summary>
        public AjaxResult Insert(TbEarlyWarningSetUp model)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                //添加信息
                Repository<TbEarlyWarningSetUp>.Insert(model);
                return AjaxResult.Success();
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
        public AjaxResult Update(TbEarlyWarningSetUp model)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                //修改信息
                Repository<TbEarlyWarningSetUp>.Update(model, p => p.ID == model.ID);
                return AjaxResult.Success();

            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.ToString());
            }
        }

        #endregion

        #region 列表信息
        public DataTable GetEarlyWarningNewsList(string MenuCode)
        {
            var where = new Where<TbEarlyWarningSetUp>();
            if (!string.IsNullOrWhiteSpace(MenuCode))
            {
                where.And(p => p.MenuCode.Like(MenuCode));
            }
            var ret = Db.Context.From<TbEarlyWarningSetUp>()
              .Select(
                      TbEarlyWarningSetUp._.All
                      , TbSysMenu._.MenuName)
                    .LeftJoin<TbSysMenu>((a, c) => a.MenuCode == c.MenuCode)
                    .Where(where).OrderBy(p=>p.MenuCode)
                    .ToDataTable();
            return ret;

        }

        public DataTable GetEarlyWarningNewsList2(string OrgType, string DeptId, string RoleId, string PersonnelSource, string PersonnelCode, string ProjectId, int NewsType)
        {
            string sql = @"select a.*,2 as NewsType,(select COUNT(1) as IsXz from TbNoticeNewsOrg b where b.NewsType=@NewsType and a.EarlyWarningNewsCode=b.NoticeNewsCode and  b.OrgType=@OrgType and b.DeptId=@DeptId and b.RoleId=@RoleId and b.PersonnelSource=@PersonnelSource and b.PersonnelCode=@PersonnelCode and b.ProjectId=@ProjectId) as IsXz from TbEarlyWarningSetUp a
where 1=1 and a.IsStart=1 order by a.MenuCode asc";
            DataTable dt = Db.Context.FromSql(sql)
                    .AddInParameter("@NewsType", DbType.Int32, NewsType)
                    .AddInParameter("@OrgType", DbType.String, OrgType)
                    .AddInParameter("@DeptId", DbType.String, DeptId)
                    .AddInParameter("@RoleId", DbType.String, RoleId)
                    .AddInParameter("@PersonnelSource", DbType.String, PersonnelSource)
                    .AddInParameter("@PersonnelCode", DbType.String, PersonnelCode)
                    .AddInParameter("@ProjectId", DbType.String, ProjectId)
                    .ToDataTable();
            return dt;

        }

        /// <summary>
        /// 获取编辑数据
        /// </summary>
        /// <param name="keyValue">数据Id</param>
        /// <returns></returns>
        public DataTable FindEntity(int keyValue)
        {
            var ret = Db.Context.From<TbEarlyWarningSetUp>()
                .Select(
                       TbEarlyWarningSetUp._.All
                    , TbSysMenu._.MenuName)
                  .LeftJoin<TbSysMenu>((a, c) => a.MenuCode == c.MenuCode)
                  .Where(p => p.ID == keyValue).ToDataTable();
            return ret;
        }

        #endregion
    }
}
