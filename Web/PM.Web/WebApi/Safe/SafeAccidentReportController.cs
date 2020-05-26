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
    /// 安全周例会
    /// </summary>
    public class SafeAccidentReportController : ApiController
    {
        /// <summary>
        /// 周例会 处理类
        /// </summary>
        private readonly SafeAccidentReportLogic _sarLogic = new SafeAccidentReportLogic();
        //
        // GET: /SafeAccidentReport/

        /// <summary>
        /// 自动获取周例会  Code
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetTableMaxCode(string UserCode) 
        {
            var MeetingCode = CreateCode.GetTableMaxCode("SAR", "MeetingCode", "TbSafeAccidentReport");
            var us = new TbUserRoleLogic().FindUserInfo(UserCode);
            DataTable dt = new DataTable();
            dt.Columns.Add("MeetingCode", typeof(string));
            dt.Columns.Add("OrgType", typeof(string));
            dt.Columns.Add("ComCode", typeof(string));
            dt.Columns.Add("ComName", typeof(string));
            DataRow dr = dt.NewRow();
            dr["MeetingCode"] = MeetingCode;
            dr["OrgType"] = us.OrgType;
            dr["ComCode"] = us.CompanyId;
            dr["ComName"] = us.ComPanyName;
            dt.Rows.Add(dr);
            return AjaxResult.Success(dt).ToJsonApi();
        }

        /// <summary>
        /// 周例会 自动获取加工厂（以当前登录人查询）Code --取消，已合并到自动获取周例会Code方法体
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage GetProcessFactory(PageSearchRequest psr)
        {
            var data = _sarLogic.GetProcessFactory(psr, "1");
            return AjaxResult.Success(data).ToJsonApi();
        }

        /// <summary>
        /// 周例会 新增、修改数据提交
        /// </summary>
        /// <param name="jdata">表数据和状态</param>
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
                var model = JsonEx.JsonToObj<TbSafeAccidentReport>(modelstr);
                if (type == "add")
                {
                    var data = _sarLogic.Insert(model, true);
                    return data.ToJsonApi();
                }
                else
                {
                    var data = _sarLogic.Update(model, true);
                    return data.ToJsonApi();
                }
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 以ID查询安全周例会信息 点编辑进行查询
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetFormJson(int keyValue)
        {
            try
            {
                var data = _sarLogic.GetFormJson(keyValue);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 查询本年所有 安全周例会信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetAllWeekAccidentReport(string ProcessFactoryCode)
        {
            try
            {
                var data = _sarLogic.GetAllWeekAccidentReport1(ProcessFactoryCode);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 以ID查询周例会 浏览界面 每次+1
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage QueryWeeklyMeeting(int keyValue)
        {
            try
            {
                var data = _sarLogic.Details(keyValue,true);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

    }
}
