using Dos.ORM;
using Newtonsoft.Json.Linq;
using PM.Business.Article;
using PM.Common;
using PM.Common.Extension;
using PM.DataEntity;
using PM.DataEntity.Article.ViewModel;
using PM.Web.WebApi.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace PM.Web.WebApi.Article
{
    /// <summary>
    /// 互动交流Api
    /// </summary>
    public class ArticleController : BaseApiController
    {
        private readonly ArticleLogic _article = new ArticleLogic();

        #region 列表

        /// <summary>
        /// 获取分页列表数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage GetGridJson([FromBody]ArticleRequest request)
        {
            try
            {
                request.Title = HttpUtility.UrlDecode(request.Title);
                var data = _article.GetDataListForPage(request);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        #endregion

        #region 编辑

        /// <summary>
        /// 编辑页获取数据
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetFormJson(int keyValue)
        {
            try
            {
                var data = _article.FindEntity(keyValue);
                var str = data.Content.Replace("\"", "'");
                data.Content = str;
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 查看页获取数据
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetViewJson(int keyValue, string userCode)
        {
            try
            {
                var data = _article.FindEntityForView(keyValue, userCode, true);
                var str = data.Item1.Content.Replace("\"", "'");
                data.Item1.Content = str;
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 新增、修改数据提交
        /// </summary>
        /// <param name="request"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage SubmitForm([FromBody]JObject jdata)
        {
            try
            {
                string modelstr = jdata["model"] == null ? "" : jdata["model"].ToString();
                string type = jdata["type"] == null ? "" : jdata["type"].ToString();
                if (string.IsNullOrWhiteSpace(modelstr) || string.IsNullOrWhiteSpace(type))
                    return AjaxResult.Error("参数错误").ToJsonApi();
                var model = JsonEx.JsonToObj<TbArticle>(modelstr);
                if (type == "add")
                {
                    model.InsertTime = DateTime.Now;
                    var data = _article.Insert(model, true);
                    return data.ToJsonApi();
                }
                else
                {
                    var data = _article.Update(model, true);
                    return data.ToJsonApi();
                }
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 数据提交(评论)
        /// </summary>
        /// <param name="request"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage SubmitFormForComment([FromBody]TbArticleComment model)
        {
            var data = _article.InsertComment(model, true);
            return data.ToJsonApi();
        }

        #endregion

        #region 删除

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage DeleteForm(int keyValue)
        {
            var data = _article.Delete(keyValue, true);
            return data.ToJsonApi();
        }

        #endregion

        #region 收藏
        /// <summary>
        /// 收藏
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage Collect(int keyValue, string userCode)
        {
            var data = _article.Collect(keyValue, userCode, true);
            return data.ToJsonApi();
        }
        #endregion

        #region 上传图片

        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name="quePara"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage UploadFile([FromBody]JObject quePara)
        {
            if (quePara == null)
                return AjaxResult.Error("文件不能为空").ToJsonApi();
            string file = quePara["file"] == null ? "" : quePara["file"].ToString();
            string fileName = quePara["fileName"] == null ? "" : quePara["fileName"].ToString();
            if (string.IsNullOrEmpty(file))
                return AjaxResult.Error("文件不能为空").ToJsonApi();
            try
            {
                var files = new List<FileData>();
                files.Add(new FileData { file = file, fileName = fileName });
                var retFile = base.UploadFileData(files);
                if (retFile == null)
                    return AjaxResult.Error("图片上传失败").ToJsonApi();
                return AjaxResult.Success(retFile.Item1).ToJsonApi();
            }
            catch (Exception ex)
            {
                return AjaxResult.Error("发生错误").ToJsonApi();
            }
        }

        #endregion
    }
}
