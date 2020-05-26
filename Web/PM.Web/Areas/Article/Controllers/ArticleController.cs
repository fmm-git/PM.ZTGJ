using PM.Business.Article;
using PM.DataEntity;
using PM.DataEntity.Article.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.Article.Controllers
{
    /// <summary>
    /// 交流互动
    /// </summary>
    [HandlerLogin]
    public class ArticleController : BaseController
    {
        private readonly ArticleLogic _article = new ArticleLogic();

        #region 列表

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult test()
        {
            return View();
        }

        /// <summary>
        /// 获取分页列表数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetGridJson(ArticleRequest request)
        {
            request.rows = 15;
            request.UserCode = base.UserCode;
            var data = _article.GetDataListForPage(request);
            return Content(data.ToJson());
        }

        #endregion

        #region 编辑

        /// <summary>
        /// 新增、修改页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Form(string type)
        {
            if (type == "add")
            {
                ViewBag.UserName = base.UserName;
            }
            return View();
        }

        /// <summary>
        /// 查看页面
        /// </summary>
        /// <returns></returns>
        [HandlerLogin(Ignore=false)]
        public ActionResult Details(int keyValue)
        {
            var data = _article.FindEntityForView(keyValue,base.UserCode);
            return View(data);
        }

        /// <summary>
        /// 编辑页获取数据
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>

        [HandlerLogin(Ignore = false)]
        public ActionResult GetFormJson(int keyValue)
        {
            var data = _article.FindEntity(keyValue);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 新增、修改数据提交
        /// </summary>
        /// <param name="request"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SubmitForm(TbArticle model, string type)
        {
            try
            {
                if (type == "add")
                {
                    model.InsertUserCode = base.UserCode;
                    var data = _article.Insert(model);
                    return Content(data.ToJson());
                }
                else
                {
                    var data = _article.Update(model);
                    return Content(data.ToJson());
                }
            }
            catch (Exception)
            {
                return Error("");
            }
        }

        /// <summary>
        /// 数据提交(评论)
        /// </summary>
        /// <param name="request"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SubmitFormForComment(TbArticleComment model)
        {
            try
            {
                model.InsertUserCode = base.UserCode;
                var data = _article.InsertComment(model);
                return Content(data.ToJson());
            }
            catch (Exception)
            {
                return Error("");
            }
        }

        #endregion

        #region 删除

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteForm(int keyValue)
        {
            try
            {
                var data = _article.Delete(keyValue);
                return Content(data.ToJson());
            }
            catch (Exception)
            {
                return Error("");
            }
        }

        #endregion

        #region 收藏
        /// <summary>
        /// 收藏
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Collect(int keyValue)
        {
            try
            {
                var data = _article.Collect(keyValue, base.UserCode);
                return Content(data.ToJson());
            }
            catch (Exception)
            {
                return Error("");
            }
        }
        #endregion

        #region 信息验证

        /// <summary>
        /// 判断信息是否可操作
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult AnyInfo(int keyValue)
        {
            var data = _article.AnyInfo(keyValue);
            return Content(data.ToJson());
        }

        #endregion
    }
}
