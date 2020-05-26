using Newtonsoft.Json.Linq;
using PM.Business;
using PM.Business.RawMaterial;
using PM.Business.Safe;
using PM.Common;
using PM.Common.Extension;
using PM.DataEntity;
using PM.DataEntity.Production.ViewModel;
using PM.DataEntity.Safe.ViewModel;
using PM.Domain.WebBase;
using PM.Web.WebApi.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace PM.Web.WebApi.Safe
{
    /// <summary>
    /// 安全检查
    /// </summary>
    public class SafeCheckController : BaseApiController
    {
        private readonly SafeCheckLogic _safeCheck = new SafeCheckLogic();
        private readonly RMProductionMaterialMark _RM = new RMProductionMaterialMark();
        private readonly DataDictionaryLogic ddi = new DataDictionaryLogic();

        #region 列表

        /// <summary>
        /// 获取分页列表数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetGridJson([FromUri]SafeCheckRequest request)
        {
            try
            {
                var data = _safeCheck.GetDataListForPage(request);
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
                var data = _safeCheck.FindEntity(keyValue);
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
                string itemModelstr = jdata["itemModel"] == null ? "" : jdata["itemModel"].ToString();
                string type = jdata["type"] == null ? "" : jdata["type"].ToString();
                if (string.IsNullOrWhiteSpace(modelstr) || string.IsNullOrWhiteSpace(type))
                    return AjaxResult.Error("参数错误").ToJsonApi();
                var model = JsonEx.JsonToObj<TbSafeCheck>(modelstr);
                var item = JsonEx.JsonToObj<List<TbSafeCheckItem>>(itemModelstr);
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
                    var data = _safeCheck.Insert(model, item, true);
                    return data.ToJsonApi();
                }
                else
                {
                    var data = _safeCheck.Update(model, item, true);
                    return data.ToJsonApi();
                }
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        #endregion

        #region 自动生成编号

        [HttpGet]
        public HttpResponseMessage GetTableMaxCode()
        {
            try
            {
                var SafeCheckCode = CreateCode.GetTableMaxCode("AQJC", "SafeCheckCode", "TbSafeCheck");
                return AjaxResult.Success(SafeCheckCode).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        #endregion

        #region 获取人员信息

        [HttpGet]
        public HttpResponseMessage GetUserList([FromUri]RMProductionMaterialRequest request)
        {
            try
            {
                var data = _RM.InsertUserNameSelect(request);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        #endregion

        #region 获取检查类型

        /// <summary>
        /// 获取检查类型
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetCheckType()
        {
            try
            {
                var data = ddi.GetDicByCode("CheckType");
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        #endregion

        #region 获取检查项模板

        /// <summary>
        /// 获取检查项模板
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetCheckItemList()
        {
            try
            {
                var data = _safeCheck.GetSafeCheckItemDataList(new SafeCheckRequest());
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        #endregion
    }
}