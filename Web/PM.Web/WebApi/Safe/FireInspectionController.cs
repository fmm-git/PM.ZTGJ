using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PM.Business;
using PM.Business.Safe;
using PM.Common;
using PM.Common.Extension;
using PM.DataEntity;
using PM.DataEntity.Safe.ViewModel;
using PM.Domain.WebBase;
using PM.Web.WebApi.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;
namespace PM.Web.WebApi.Safe
{
    public class FireInspectionController : BaseApiController
    {
        //
        // 消防检查
        private readonly TbFireInspectionLogic _fireInspection = new TbFireInspectionLogic();
        private readonly TbAttachmentLogic _attachmentImp = new TbAttachmentLogic();
        private string HostAddress = Configs.GetValue("HostAddress");

        #region 列表

        /// <summary>
        /// 获取分页列表数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetGridJson([FromUri]FireInspectionRequest request)
        {
            try
            {
                var data = _fireInspection.GetDataListForPage(request);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 获取检查人员
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetCheckUserList()
        {
            try
            {
                var data = _fireInspection.GetCheckUserList();
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 获取加工厂
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetCompanyList()
        {
            try
            {
                var data = _fireInspection.GetCompanyList();
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 获取附件地址
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetFileUrl(string FileId)
        {
            try
            {
                var data = base.GetEnclosureUrl(FileId);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        #endregion

        #region 新增、编辑、查看

        /// <summary>
        /// 获取消防检查Code
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetTableMaxCode()
        {
            var CheckCode = CreateCode.GetTableMaxCode("FIC", "CheckCode", "TbFireInspection");
            DataTable dt = new DataTable();
            dt.Columns.Add("CheckCode", typeof(string));
            DataRow dr = dt.NewRow();
            dr["CheckCode"] = CheckCode;
            dt.Rows.Add(dr);
            return AjaxResult.Success(dt).ToJsonApi();
        }

        /// <summary>
        /// 编辑、查看页获取数据
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetFormJson(int keyValue)
        {
            try
            {
                var data = _fireInspection.FindEntity(keyValue);
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
                var model = JsonEx.JsonToObj<TbFireInspection>(modelstr);
                //保存图片文件
                if (!string.IsNullOrWhiteSpace(model.Enclosure))
                {
                    var files = JsonEx.JsonToObj<List<FileData>>(model.Enclosure);
                    //保存图片文件
                    if (files.Any())
                    {
                        base.UserCode = model.InsertUserCode;
                        var retFile = base.UploadFileData(files, true);
                        if (retFile == null)
                            return AjaxResult.Error("图片上传失败").ToJsonApi();
                        model.Enclosure = retFile.Item2;
                    }
                }
                if (type == "add")
                {
                    model.InsertTime = DateTime.Now;
                    var data = _fireInspection.Insert(model, true);
                    return data.ToJsonApi();
                }
                else
                {
                    var data = _fireInspection.Update(model, true);
                    return data.ToJsonApi();
                }
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
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
            var data = _fireInspection.Delete(keyValue, true);
            return data.ToJsonApi();
        }

        #endregion

    }
}
