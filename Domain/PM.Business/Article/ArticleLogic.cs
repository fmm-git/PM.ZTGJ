using Dos.ORM;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using PM.DataEntity.Article.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace PM.Business.Article
{
    /// <summary>
    /// 交流互动
    /// </summary>
    public class ArticleLogic
    {

        #region 新增数据

        /// <summary>
        /// 新增数据(文章)
        /// </summary>
        public AjaxResult Insert(TbArticle model, bool isApi = false)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            model.CommentCount = 0;
            model.BrowseCount = 0;
            model.CollectCount = 0;
            model.Type = 1;
            try
            {
                //添加信息
                Repository<TbArticle>.Insert(model, isApi);
                return AjaxResult.Success();
            }
            catch (Exception)
            {
                return AjaxResult.Error();
            }
        }

        /// <summary>
        /// 新增数据(评论)
        /// </summary>
        public AjaxResult InsertComment(TbArticleComment model, bool isApi = false)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            model.InsertTime = DateTime.Now;
            model.Type = 1;
            if (model.CommentID != 0)
            {
                model.Type = 2;
            }
            //修改浏览数
            var article = Repository<TbArticle>.First(p => p.ID == model.ArticleID);
            article.CommentCount += 1;
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //添加信息
                    Repository<TbArticleComment>.Insert(trans, model, isApi);
                    //修改浏览数
                    Repository<TbArticle>.Update(trans, article, p => p.ID == model.ArticleID, isApi);
                    trans.Commit();
                    return AjaxResult.Success();
                }
            }
            catch (Exception)
            {
                return AjaxResult.Error();
            }
        }

        #endregion

        #region 修改数据

        /// <summary>
        /// 修改数据
        /// </summary>
        public AjaxResult Update(TbArticle model, bool isApi = false)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            var anyRet = AnyInfo(model.ID);
            if (anyRet.state != ResultType.success.ToString())
                return anyRet;
            try
            {
                Repository<TbArticle>.Update(model, p => p.ID == model.ID, isApi);
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
        public AjaxResult Delete(int keyValue, bool isApi = false)
        {
            try
            {
                //判断信息是否存在
                var anyRet = AnyInfo(keyValue);
                if (anyRet.state != ResultType.success.ToString())
                    return anyRet;
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //删除信息
                    var count = Repository<TbArticle>.Delete(trans, p => p.ID == keyValue, isApi);
                    //删除评论信息
                    Repository<TbArticleComment>.Delete(trans, p => p.ArticleID == keyValue, isApi);
                    //删除收藏信息
                    Repository<TbArticleCollect>.Delete(trans, p => p.ArticleID == keyValue, isApi);
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

        #region 收藏

        /// <summary>
        /// 收藏
        /// </summary>
        public AjaxResult Collect(int keyValue, string userCode, bool isApi = false)
        {
            try
            {
                //判断信息是否存在
                var anyRet = AnyInfo(keyValue);
                if (anyRet.state != ResultType.success.ToString())
                    return anyRet;
                //判断信息是否已收藏
                var collect = Repository<TbArticleCollect>.First(p => p.ArticleID == keyValue && p.InsertUserCode == userCode);
                if (collect == null)
                {
                    var newCollect = new TbArticleCollect()
                    {
                        ArticleID = keyValue,
                        InsertUserCode = userCode,
                        InsertTime = DateTime.Now
                    };
                    ((TbArticle)anyRet.data).CollectCount += 1;
                    using (DbTrans trans = Db.Context.BeginTransaction())
                    {
                        //添加收藏信息
                        Repository<TbArticleCollect>.Insert(trans, newCollect, isApi);
                        //修改收藏数
                        Repository<TbArticle>.Update(trans, (TbArticle)anyRet.data, p => p.ID == keyValue, isApi);
                        trans.Commit();
                        return AjaxResult.Success("收藏成功");
                    }
                }
                else
                {
                    ((TbArticle)anyRet.data).CollectCount -= 1;
                    using (DbTrans trans = Db.Context.BeginTransaction())
                    {
                        //删除收藏信息
                        Repository<TbArticleCollect>.Delete(trans, collect, isApi);
                        //修改收藏数
                        Repository<TbArticle>.Update(trans, (TbArticle)anyRet.data, p => p.ID == keyValue, isApi);
                        trans.Commit();
                        return AjaxResult.Success("取消成功");
                    }
                }
                return AjaxResult.Success();
            }
            catch (Exception)
            {
                return AjaxResult.Error();
            }
        }

        #endregion

        #region 获取数据

        /// <summary>
        /// 获取数据(编辑)
        /// </summary>
        /// <param name="keyValue">数据Id</param>
        /// <returns></returns>
        public TbArticle FindEntity(int keyValue)
        {
            var ret = Db.Context.From<TbArticle>()
                .Select(
                       TbArticle._.All
                    , TbUser._.UserName)
                  .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                  .Where(p => p.ID == keyValue).First();
            if (ret == null)
                return null;
            var data = ret.Content.Replace("&lt;", "<").Replace("&gt;", ">");
            ret.Content = data;
            return ret;
        }

        /// <summary>
        /// 获取数据(查看)
        /// </summary>
        /// <param name="keyValue">数据Id</param>
        /// <returns></returns>
        public Tuple<TbArticle, List<TbArticleComment>> FindEntityForView(int keyValue, string userCode, bool isApi = false)
        {
            try
            {
                var ret = Db.Context.From<TbArticle>()
                    .Select(
                           TbArticle._.All
                        , TbUser._.UserName
                        , TbArticleCollect._.ID.As("CollectID"))
                      .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                      .LeftJoin<TbArticleCollect>((a, c) => a.ID == c.ArticleID && c.InsertUserCode == "" + userCode + "")
                      .Where(p => p.ID == keyValue).First();
                if (ret == null)
                    return new Tuple<TbArticle, List<TbArticleComment>>(null, null);
                //查找评论信息
                var items = Db.Context.From<TbArticleComment>().Select(
                    TbArticleComment._.All
                        , TbUser._.UserName)
                   .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                   .Where(p => p.ArticleID == ret.ID)
                   .ToList();
                //修改浏览数
                var article = Repository<TbArticle>.First(p => p.ID == keyValue);
                article.BrowseCount += 1;
                Repository<TbArticle>.Update(article, p => p.ID == keyValue, isApi);

                var data = ret.Content.Replace("&lt;", "<").Replace("&gt;", ">");
                ret.Content = data;
                return new Tuple<TbArticle, List<TbArticleComment>>(ret, items);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 获取数据列表(分页)
        /// </summary>
        public PageModel GetDataListForPage(ArticleRequest request)
        {

            #region 模糊搜索条件
            var where = new Where<TbArticle>();
            if (!string.IsNullOrWhiteSpace(request.Title))
            {
                where.And(d => d.Title.Like(request.Title) || d.Code.StartsWith(request.Title));
            }
            if (request.type > 0)
            {
                if (request.type == 3)
                {
                    where.And(TbArticle._.ID.SubQueryIn(Db.Context.From<TbArticleCollect>()
                    .Where(p => p.InsertUserCode == request.UserCode)
                    .Select(p => p.ArticleID)));
                }
                else
                {
                    where.And(d => d.Type == request.type);
                }
            }
            if (!string.IsNullOrWhiteSpace(request.OrderCode))
            {
                StringBuilder sb = new StringBuilder();
                string sql = @"select a.ID,b.OrderCode from TbDistributionEntOrder a
left join TbDistributionEntItem b on a.DisEntOrderIdentity=b.DisEntOrderIdentity where b.OrderCode='" + request.OrderCode + "' group by a.ID,b.OrderCode";
                DataTable dt = Db.Context.FromSql(sql).ToDataTable();
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (dt.Rows.Count-1==i)
                        {
                            sb.Append(dt.Rows[i]["ID"]);
                        }
                        else
                        {
                            sb.Append(dt.Rows[i]["ID"] + ",");
                        }
                    }
                    if (request.type==2)
                    {
                        where.And(p => p.DisEntOrderId.In(sb.ToString()));
                    }
                }
            }
            //if (!string.IsNullOrEmpty(request.ProjectId))
            //    where.And(p => p.ProjectId == request.ProjectId);
            #endregion

            try
            {
                var data = Db.Context.From<TbArticle>()
                    .Select(
                      TbArticle._.ID
                    , TbArticle._.Title
                    , TbArticle._.ProjectId
                    , TbArticle._.Abstract
                    , TbArticle._.BrowseCount
                    , TbArticle._.CommentCount
                    , TbArticle._.CollectCount
                    , TbArticle._.Type
                    , TbArticle._.InsertUserCode
                    , TbArticle._.InsertTime
                    , TbUser._.UserName)
                  .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                  .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                  .Where(where)
                  .OrderByDescending(TbArticle._.BrowseCount, TbArticle._.CommentCount, TbArticle._.CollectCount, TbArticle._.ID)
                  .ToPageList(request.rows, request.page);
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 判断信息是否可操作
        /// </summary>
        /// <returns></returns>
        public AjaxResult AnyInfo(int keyValue)
        {
            var article = Repository<TbArticle>.First(p => p.ID == keyValue);
            if (article == null)
                return AjaxResult.Warning("信息不存在");
            return AjaxResult.Success(article);
        }
        #endregion
    }
}
