using Dos.ORM;
using Newtonsoft.Json.Linq;
using PM.Business;
using PM.Business.Safe;
using PM.Common;
using PM.Common.Extension;
using PM.DataEntity;
using PM.Domain.WebBase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace PM.Web.WebApi.Safe
{
    /// <summary>
    /// 班前讲话控制器
    /// </summary>
    public class SafeDatumManageController : ApiController
    {
        /// <summary>
        /// 班前讲话 处理类
        /// </summary>
        public readonly SafeDatumManageLogic _sdmLogic = new SafeDatumManageLogic();
        //
        // GET: /SafeDatumManage/

        /// <summary>
        /// 自动获取班前讲话  Code
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetTableMaxCode(string UserCode)
        {
            var ContentCode = CreateCode.GetTableMaxCode("SDM", "ContentCode", "TbSafeDatumManage");
            var us = new TbUserRoleLogic().FindUserInfo(UserCode);
            DataTable dt = new DataTable();
            dt.Columns.Add("ContentCode", typeof(string));
            dt.Columns.Add("OrgType", typeof(string));
            dt.Columns.Add("ComCode", typeof(string));
            dt.Columns.Add("ComName", typeof(string));
            DataRow dr = dt.NewRow();
            dr["ContentCode"] = ContentCode;
            dr["OrgType"] = us.OrgType;
            dr["ComCode"] = us.CompanyId;
            dr["ComName"] = us.ComPanyName;
            dt.Rows.Add(dr);
            return AjaxResult.Success(dt).ToJsonApi();
        }

        /// <summary>
        /// 自动获取加工厂（以当前登录人查询）  Code  --取消，已合并到自动获取班前讲话Code方法体
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage GetTableJGCCode(string UserCode)
        {
            var data = _sdmLogic.GetUser(UserCode);
            return AjaxResult.Success(data).ToJsonApi();
        }

        /// <summary>
        /// 班前讲话 新增、修改数据提交
        /// </summary>
        /// <param name="jdata">表数据和状态</param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage SubmitForm([FromBody]JObject jdata)
        {
            string modelstr = jdata["model"] == null ? "" : jdata["model"].ToString();
            string type = jdata["type"] == null ? "" : jdata["type"].ToString();
            if (string.IsNullOrWhiteSpace(modelstr) || string.IsNullOrWhiteSpace(type))
                return AjaxResult.Error("参数错误").ToJsonApi();
            var model = JsonEx.JsonToObj<TbSafeDatumManage>(modelstr);
            if (type == "add")
            {
                var data = _sdmLogic.Insert(model,true);
                return data.ToJsonApi();
            }
            else
            {
                var data = _sdmLogic.Update(model,true);
                return data.ToJsonApi();
            }
        }

        /// <summary>
        /// 以ID查询班前讲话信息 点编辑进行查询
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetFormJson(int keyValue)
        {
            try
            {
                var data = _sdmLogic.GetFormJson(keyValue);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 查询本年本月所有 班前讲话信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetAllDayDatum(string ProcessFactoryCode)
        {
            try
            {
                var data = _sdmLogic.GetAllDayDatum1(ProcessFactoryCode);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 以ID查询班前讲话 浏览界面 每次+1
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage QueryDatumManage(int keyValue)
        {
            try
            {
                var data = _sdmLogic.Details(keyValue,true);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception e)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }
    }
}
