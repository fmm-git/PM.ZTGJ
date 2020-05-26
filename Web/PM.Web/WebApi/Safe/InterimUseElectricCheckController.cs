using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    public class InterimUseElectricCheckController : BaseApiController
    {
        //
        //临时用电检查

        private readonly TbInterimUseElectricCheckLogic _iueclogic = new TbInterimUseElectricCheckLogic();

        #region 列表

        /// <summary>
        /// 获取分页列表数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetGridJson([FromUri]InterimUseElectricCheckRequest request)
        {
            try
            {
                var data = _iueclogic.GetDataListForPage(request);
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
        /// 获取临时用电检查Code
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetTableMaxCode()
        {
            var CheckCode = CreateCode.GetTableMaxCode("IUEC", "CheckCode", "TbInterimUseElectricCheck");
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
                var data = _iueclogic.FindEntity(keyValue);
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
                var model = JsonEx.JsonToObj<TbInterimUseElectricCheck>(modelstr);
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
                    var data = _iueclogic.Insert(model, true);
                    return data.ToJsonApi();
                }
                else
                {
                    var data = _iueclogic.Update(model, true);
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
            var data = _iueclogic.Delete(keyValue, true);
            return data.ToJsonApi();
        }

        #endregion

    }
}
