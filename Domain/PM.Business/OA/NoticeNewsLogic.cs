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
    public class NoticeNewsLogic
    {
        #region 修改
        public AjaxResult Update(TbNoticeNewsSetUp model)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                //修改信息
                Repository<TbNoticeNewsSetUp>.Update(model, p => p.ID == model.ID);
                return AjaxResult.Success();

            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.ToString());
            }
        }
        //public AjaxResult UpdateNew(List<TbNoticeNewsSetUp> noticeNewsList)
        //{
        //    if (noticeNewsList == null)
        //        return AjaxResult.Warning("参数错误");
        //    var noticeNewsListNew = new List<TbNoticeNewsSetUp>();
        //    var ret = Db.Context.From<TbNoticeNewsSetUp>().Select(TbNoticeNewsSetUp._.All).ToList();
        //    if (noticeNewsList.Count > 0)
        //    {
        //        for (int i = 0; i < noticeNewsList.Count; i++)
        //        {
        //            var model = ret.Where(p => p.NoticeNewsCode == noticeNewsList[i].NoticeNewsCode).First();
        //            model.IsStart = noticeNewsList[i].IsStart;
        //            model.App = noticeNewsList[i].App;
        //            model.Pc = noticeNewsList[i].Pc;
        //            noticeNewsListNew.Add(model);
        //        }
        //    }
        //    try
        //    {
        //        if (noticeNewsListNew.Count > 0)
        //        {
        //            Repository<TbNoticeNewsSetUp>.Update(noticeNewsListNew);
        //        }
        //        return AjaxResult.Success();
        //    }

        //    catch (Exception)
        //    {
        //        return AjaxResult.Error("操作失败");
        //    }

        //}
        public AjaxResult InsertNew(List<TbNoticeNewsOrg> noticeNewsOrgList, string OrgType, string DeptId, string RoleId, string PersonnelSource, string PersonnelCode, string ProjectId, int NewsType)
        {
            if (noticeNewsOrgList == null)
                return AjaxResult.Warning("参数错误");
            using (DbTrans trans = Db.Context.BeginTransaction())
            {
                try
                {
                    //先删除原来的数据
                    Repository<TbNoticeNewsOrg>.Delete(trans, p => p.OrgType == OrgType && p.DeptId == DeptId && p.PersonnelCode == PersonnelCode && p.PersonnelSource == PersonnelSource && p.ProjectId == ProjectId && p.NewsType==NewsType);
                    Repository<TbNoticeNewsOrg>.Insert(trans, noticeNewsOrgList);
                    trans.Commit();
                    return AjaxResult.Success();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return AjaxResult.Error(ex.ToString());
                }
                finally
                {
                    trans.Close();
                }
            }

        }

        #endregion
        public DataTable GetNoticeNewsList(string MenuCode)
        {
            var where = new Where<TbNoticeNewsSetUp>();
            if (!string.IsNullOrWhiteSpace(MenuCode))
            {
                where.And(p => p.MenuCode == MenuCode);
            }
            var ret = Db.Context.From<TbNoticeNewsSetUp>()
              .Select(
                      TbNoticeNewsSetUp._.All
                      , TbSysMenu._.MenuName)
                    .LeftJoin<TbSysMenu>((a, c) => a.MenuCode == c.MenuCode)
                    .Where(where)
                    .ToDataTable();
            return ret;

        }
        public DataTable GetNoticeNewsList2(string OrgType, string DeptId, string RoleId, string PersonnelSource, string PersonnelCode, string ProjectId, int NewsType)
        {
            string sql = @"select a.*,1 as NewsType,(select COUNT(1) as IsXz from TbNoticeNewsOrg b where b.NewsType=@NewsType and a.NoticeNewsCode=b.NoticeNewsCode and  b.OrgType=@OrgType and b.DeptId=@DeptId and b.RoleId=@RoleId and b.PersonnelSource=@PersonnelSource and b.PersonnelCode=@PersonnelCode and b.ProjectId=@ProjectId) as IsXz from TbNoticeNewsSetUp a
where 1=1 and a.IsStart=1";
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
            var ret = Db.Context.From<TbNoticeNewsSetUp>()
                .Select(
                       TbNoticeNewsSetUp._.All
                    , TbSysMenu._.MenuName)
                  .LeftJoin<TbSysMenu>((a, c) => a.MenuCode == c.MenuCode)
                  .Where(p => p.ID == keyValue).ToDataTable();
            return ret;
        }
    }
}
