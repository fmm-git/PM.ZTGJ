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

namespace PM.Business.EarlyWarning
{
    public class TbFormEarlyWarningBegTimeLogic
    {
        #region 新增数据

        /// <summary>
        /// 新增数据
        /// </summary>
        public AjaxResult Insert(TbFormEarlyWarningBegTime model)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                var count = Repository<TbFormEarlyWarningBegTime>.Insert(model);
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

        #endregion

        #region 修改数据

        /// <summary>
        /// 修改数据
        /// </summary>
        public AjaxResult Update(TbFormEarlyWarningBegTime model)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                var count = Repository<TbFormEarlyWarningBegTime>.Update(model, p => p.ID == model.ID);
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
                var count = Repository<TbFormEarlyWarningBegTime>.Delete(p => p.ID == dataID);
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
            var ret = Db.Context.From<TbFormEarlyWarningBegTime>()
              .Select(
                      TbFormEarlyWarningBegTime._.All
                      , TbCompany._.CompanyFullName.As("BranchName"))
                      .LeftJoin<TbSysMenu>((a, c) => a.MenuCode == c.MenuCode)
                      .LeftJoin<TbCompany>((a, c) => a.BranchCode == c.CompanyCode)
                      .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                      .Where(TbCompany._.CompanyCode == TbFormEarlyWarningBegTime._.SiteCode), "SiteName")
                      .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                      .Where(TbCompany._.CompanyCode == TbFormEarlyWarningBegTime._.WorkAreaCode), "WorkAreaName")
                      .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                      .Where(TbCompany._.CompanyCode == TbFormEarlyWarningBegTime._.ProcessFactoryCode), "ProcessFactoryName")
                      .Where(p => p.ID == dataID).ToDataTable();
            return new Tuple<DataTable>(ret);
        }

        /// <summary>
        /// 获取数据列表(分页)
        /// </summary>
        public PageModel GetDataListForPage(PageSearchRequest request, string MenuCode)
        {
            //组装查询语句
            #region 模糊搜索条件

            var where = new Where<TbFormEarlyWarningBegTime>();
            if (!string.IsNullOrWhiteSpace(MenuCode))
            {
                where.And(p => p.MenuCode.Like(MenuCode));
            }
            if (!string.IsNullOrWhiteSpace(OperatorProvider.Provider.CurrentUser.ProjectId))
            {
                where.And(d => d.ProjectId == OperatorProvider.Provider.CurrentUser.ProjectId);
            }

            #endregion

            try
            {
                var ret = Db.Context.From<TbFormEarlyWarningBegTime>()
              .Select(
                      TbFormEarlyWarningBegTime._.All
                      , TbSysMenu._.MenuName
                      , TbCompany._.CompanyFullName.As("BranchName"))
                      .LeftJoin<TbSysMenu>((a, c) => a.MenuCode == c.MenuCode)
                      .LeftJoin<TbCompany>((a, c) => a.BranchCode == c.CompanyCode)
                      .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                      .Where(TbCompany._.CompanyCode == TbFormEarlyWarningBegTime._.SiteCode), "SiteName")
                      .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                      .Where(TbCompany._.CompanyCode == TbFormEarlyWarningBegTime._.WorkAreaCode), "WorkAreaName")
                      .Where(where).OrderBy(d => d.ID).ToPageList(request.rows, request.page);
                return ret;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region 获取菜单
        public DataTable GetMenu()
        {
            var ret = Db.Context.From<TbSysMenuTable>()
            .Select(
                     TbSysMenuTable._.All
                     , TbSysMenu._.MenuName)
                     .LeftJoin<TbSysMenu>((a, c) => a.MenuCode == c.MenuCode).OrderBy(d => d.ID).ToDataTable();
            return ret;
        }
        #endregion
    }
}
