using Dos.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PM.Business;
using PM.Business.Distribution;
using PM.Business.System;
using PM.Common;
using PM.Common.Extension;
using PM.Common.Helper;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using PM.DataEntity.System.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace PM.Web.WebApi.SystemManage
{
    public class SystemManageController : ApiController
    {
        //
        // GET: /SystemManage/
        private string strBMUrl = System.Configuration.ConfigurationManager.ConnectionStrings["strBMUrl"].ConnectionString;
        private string DefaultUserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";
        private string contentType = "application/x-www-form-urlencoded";

        private TbProjectInfoLogic _project = new TbProjectInfoLogic();
        private CompanyLogic cit = new CompanyLogic();
        private DepartmentLogic dit = new DepartmentLogic();
        private UserLogic ui = new UserLogic();
        private TbRoleLogic _role = new TbRoleLogic();
        private TbUserRoleLogic _userRole = new TbUserRoleLogic();
        private TbSysMenuLogic _sysMenu = new TbSysMenuLogic();
        private DistributionPlan _Plan = new DistributionPlan();
        private DataDictionaryLogic ddi = new DataDictionaryLogic();

        #region 项目信息
        /// <summary>
        /// 获取项目地址
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public HttpResponseMessage GetProjectInfoList(string url)
        {
            try
            {
                var dataJson = Get(strBMUrl + url);
                JObject jo = (JObject)JsonConvert.DeserializeObject(dataJson);
                string modelstr = jo["data"] == null ? "" : jo["data"].ToString();
                if (string.IsNullOrWhiteSpace(modelstr))
                    return AjaxResult.Error("参数错误").ToJsonApi();
                List<TbProjectInfo> model = JsonEx.JsonToObj<List<TbProjectInfo>>(modelstr);
                var data = _project.InsertNew(model, true);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        #endregion

        #region 组织机构
        ///// <summary>
        ///// 新增、修改
        ///// </summary>
        ///// <param name="jdata"></param>
        ///// <returns></returns>
        //[HttpGet]
        //public HttpResponseMessage SaveCompanyData(string jdata)
        //{
        //    try
        //    {
        //        //{"type":"add","model":{"CompanyCode":"001003002","ParentCompanyCode":"001003","CompanyFullName":"二工区","Address":"广州市","IsEnable": 0,"OrgType":3}}
        //        JObject jo = (JObject)JsonConvert.DeserializeObject(jdata);
        //        string modelstr = jo["model"] == null ? "" : jo["model"].ToString();
        //        string type = jo["type"] == null ? "" : jo["type"].ToString();
        //        if (string.IsNullOrWhiteSpace(modelstr) || string.IsNullOrWhiteSpace(type))
        //            return AjaxResult.Error("参数错误").ToJsonApi();
        //        var model = JsonEx.JsonToObj<TbCompany>(modelstr);
        //        if (type == "add")
        //        {
        //            var data = cit.Insert(model, true);
        //            return data.ToJsonApi();
        //        }
        //        else
        //        {
        //            var data = cit.Update(model, true);
        //            return data.ToJsonApi();
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        return AjaxResult.Error("操作失败").ToJsonApi();
        //    }
        //}

        ///// <summary>
        ///// 删除
        ///// </summary>
        ///// <param name="keyValue"></param>
        ///// <returns></returns>
        //[HttpGet]
        //public HttpResponseMessage DeleteCompanyData(string companyCode)
        //{
        //    var data = cit.Delete(companyCode, true);
        //    return data.ToJsonApi();
        //}

        /// <summary>
        /// 获取组织机构地址
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage GetCompanyList(string url)
        {
            try
            {
                var dataJson = Get(strBMUrl + url);
                JObject jo = (JObject)JsonConvert.DeserializeObject(dataJson);
                string modelstr = jo["data"] == null ? "" : jo["data"].ToString();
                if (string.IsNullOrWhiteSpace(modelstr))
                    return AjaxResult.Error("参数错误").ToJsonApi();
                DataTable dt = ToDataTable(modelstr);
                dt.Columns["OrgId"].ColumnName = "CompanyCode";
                dt.Columns["OrgName"].ColumnName = "CompanyFullName";
                dt.Columns["ParentOrgId"].ColumnName = "ParentCompanyCode";
                dt.Columns["Address"].ColumnName = "Address";
                dt.Columns["OrgType"].ColumnName = "OrgType";
                DataColumn dc = new DataColumn("IsEnable", typeof(int));
                dc.DefaultValue = 0;
                dt.Columns.Add(dc);
                List<TbCompany> model = ModelConvertHelper<TbCompany>.ToList(dt);
                var data = cit.InsertNew(model, true);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        #endregion

        #region 部门

        ///// <summary>
        ///// 新增、修改
        ///// </summary>
        ///// <param name="jdata"></param>
        ///// <returns></returns>
        //[HttpGet]
        //public HttpResponseMessage SaveDepData(string jdata)
        //{
        //    try
        //    {
        //        //{"type":"add","model":{"DepartmentCode":"BM17","DepartmentName":"测试部门","ParentDepartmentCode":"","CompanyCode":"001003002","SortNumber":1,"DepartmentLeader":"", "DepartmentSecLeader":"", "Address":"测试"}}
        //        JObject jo = (JObject)JsonConvert.DeserializeObject(jdata);
        //        string modelstr = jo["model"] == null ? "" : jo["model"].ToString();
        //        string type = jo["type"] == null ? "" : jo["type"].ToString();
        //        if (string.IsNullOrWhiteSpace(modelstr) || string.IsNullOrWhiteSpace(type))
        //            return AjaxResult.Error("参数错误").ToJsonApi();
        //        var model = JsonEx.JsonToObj<TbDepartment>(modelstr);
        //        model.BelongCompanyCode = model.CompanyCode;
        //        if (type == "add")
        //        {
        //            var data = dit.Insert(model, true);
        //            return data.ToJsonApi();
        //        }
        //        else
        //        {
        //            var data = dit.Update(model, true);
        //            return data.ToJsonApi();
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        return AjaxResult.Error("操作失败").ToJsonApi();
        //    }
        //}

        ///// <summary>
        ///// 删除
        ///// </summary>
        ///// <param name="keyValue"></param>
        ///// <returns></returns>
        //[HttpGet]
        //public HttpResponseMessage DeleteDepData(string depCode)
        //{
        //    var data = dit.DeleteNew(depCode, true);
        //    return data.ToJsonApi();
        //}

        /// <summary>
        /// 获取部门地址
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage GetDepartmentList(string url)
        {
            try
            {
                var dataJson = Get(strBMUrl + url);
                JObject jo = (JObject)JsonConvert.DeserializeObject(dataJson);
                string modelstr = jo["data"] == null ? "" : jo["data"].ToString();
                if (string.IsNullOrWhiteSpace(modelstr))
                    return AjaxResult.Error("参数错误").ToJsonApi();
                List<TbDepartment> model = JsonEx.JsonToObj<List<TbDepartment>>(modelstr);
                var data = dit.InsertNew(model, true);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        #endregion

        #region 角色

        ///// <summary>
        ///// 新增、修改
        ///// </summary>
        ///// <param name="jdata"></param>
        ///// <returns></returns>
        //[HttpGet]
        //public HttpResponseMessage SaveRoleData(string jdata)
        //{
        //    try
        //    {
        //        //{"type":"add","model":{"RoleCode":"R010","RoleName":"测试角色","DepartmentCode":"BM17","State":"启用"}}
        //        JObject jo = (JObject)JsonConvert.DeserializeObject(jdata);
        //        string modelstr = jo["model"] == null ? "" : jo["model"].ToString();
        //        string type = jo["type"] == null ? "" : jo["type"].ToString();
        //        if (string.IsNullOrWhiteSpace(modelstr) || string.IsNullOrWhiteSpace(type))
        //            return AjaxResult.Error("参数错误").ToJsonApi();
        //        var model = JsonEx.JsonToObj<TbRole>(modelstr);
        //        if (type == "add")
        //        {
        //            var data = _role.InsertNew(model, true);
        //            return data.ToJsonApi();
        //        }
        //        else
        //        {
        //            var data = _role.UpdateNew(model, true);
        //            return data.ToJsonApi();
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        return AjaxResult.Error("操作失败").ToJsonApi();
        //    }
        //}

        ///// <summary>
        ///// 删除
        ///// </summary>
        ///// <param name="keyValue"></param>
        ///// <returns></returns>
        //[HttpGet]
        //public HttpResponseMessage DeleteRoleData(string roleCode)
        //{
        //    var data = _role.Delete(roleCode, true);
        //    return data.ToJsonApi();
        //}

        /// <summary>
        /// 获取角色地址
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage GetRoleList(string url)
        {
            try
            {
                var dataJson = Get(strBMUrl + url);
                JObject jo = (JObject)JsonConvert.DeserializeObject(dataJson);
                string modelstr = jo["data"] == null ? "" : jo["data"].ToString();
                if (string.IsNullOrWhiteSpace(modelstr))
                    return AjaxResult.Error("参数错误").ToJsonApi();
                List<TbRole> model = JsonEx.JsonToObj<List<TbRole>>(modelstr);
                var data = _role.InsertNew1(model, true);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        #endregion

        #region 用户

        ///// <summary>
        ///// 新增、修改
        ///// </summary>
        ///// <param name="jdata"></param>
        ///// <returns></returns>
        //[HttpGet]
        //public HttpResponseMessage SaveUserData(string jdata)
        //{
        //    try
        //    {
        //        //{"type":"edit","model":{"UserCode":"zhangqiang","UserName":"张强","UserPwd":"dEu7vwkYjTA=","UserSex":"男","UserClosed":"在职","MobilePhone":"15726831456","IDNumber":"500228199210264578","CompanyCode ":"001003002","DepartmentCode":"BM17"}}
        //        JObject jo = (JObject)JsonConvert.DeserializeObject(jdata);
        //        string modelstr = jo["model"] == null ? "" : jo["model"].ToString();
        //        string type = jo["type"] == null ? "" : jo["type"].ToString();
        //        if (string.IsNullOrWhiteSpace(modelstr) || string.IsNullOrWhiteSpace(type))
        //            return AjaxResult.Error("参数错误").ToJsonApi();
        //        var model = JsonEx.JsonToObj<TbUser>(modelstr);
        //        if (type == "add")
        //        {
        //            var data = ui.Insert(model, true);
        //            return data.ToJsonApi();
        //        }
        //        else
        //        {
        //            var data = ui.Update(model, true);
        //            return data.ToJsonApi();
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        return AjaxResult.Error("操作失败").ToJsonApi();
        //    }
        //}

        ///// <summary>
        ///// 删除
        ///// </summary>
        ///// <param name="keyValue"></param>
        ///// <returns></returns>
        //[HttpGet]
        //public HttpResponseMessage DeleteUserData(string userCode)
        //{
        //    var data = ui.DeleteNew(userCode, true);
        //    return data.ToJsonApi();
        //}

        /// <summary>
        /// 获取用户地址
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage GetUserList(string url)
        {
            try
            {
                var dataJson = Get(strBMUrl + url);
                //JObject jo = (JObject)JsonConvert.DeserializeObject(dataJson);
                //string modelstr = jo["data"] == null ? "" : jo["data"].ToString();
                //if (string.IsNullOrWhiteSpace(modelstr))
                //    return AjaxResult.Error("参数错误").ToJsonApi();
                //DataTable dt = ToDataTable(modelstr);
                //dt.Columns["UserAccount"].ColumnName = "UserCode";
                //dt.Columns["type"].ColumnName = "UserType";
                //DataColumn dc = new DataColumn("UserClosed", typeof(string));
                //dc.DefaultValue = "在职";
                //dt.Columns.Add(dc);
                //DataColumn dc1 = new DataColumn("CreateTime", typeof(DateTime));
                //dc1.DefaultValue = DateTime.Now;
                //dt.Columns.Add(dc1);
                //List<TbUser> model = ModelConvertHelper<TbUser>.ToList(dt);

                var requestUserList = JsonEx.JsonToObj<RequestUserModel>(dataJson);
                if (requestUserList.data == null)
                    return AjaxResult.Success().ToJsonApi();
                var model = MapperHelper.Map<RequestUserListModel, TbUser>(requestUserList.data);
                var data = ui.InsertNew(model, true);
                return data.ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        public class RequestUserModel
        {
            public RequestStateModel state { get; set; }
            public List<RequestUserListModel> data { get; set; }
        }
        public class RequestUserListModel
        {
            public string UserPwd { get; set; }
            public string UserName { get; set; }
            public string UserId { get; set; }
            public string UserSex { get; set; }
            public string UserAccount { get; set; }
            public string UserCode { get { return this.UserAccount; } }
            public string type { get; set; }
            public string UserType { get { return string.IsNullOrEmpty(this.type) ? "0" : this.type; } }
            public string UserClosed { get { return "在职"; } }
            public DateTime CreateTime { get { return DateTime.Now; } }
        }
        public class RequestStateModel
        {
            public string code { get; set; }
            public string msg { get; set; }
            public string debugmsg { get; set; }
            public string editDate { get; set; }
        }
        #endregion

        #region 角色人员
        ///// <summary>
        ///// 新增、修改
        ///// </summary>
        ///// <param name="jdata"></param>
        ///// <returns></returns>
        //[HttpGet]
        //public HttpResponseMessage SaveRoleUserData(string jdata)
        //{
        //    try
        //    {
        //        //{"type":"add","model":[{"RoleCode":"R010","UserCode":"zhangqiang"},{"RoleCode":"R010","UserCode":"lisi"}]}
        //        JObject jo = (JObject)JsonConvert.DeserializeObject(jdata);
        //        string modelstr = jo["model"] == null ? "" : jo["model"].ToString();
        //        string type = jo["type"] == null ? "" : jo["type"].ToString();
        //        if (string.IsNullOrWhiteSpace(modelstr) || string.IsNullOrWhiteSpace(type))
        //            return AjaxResult.Error("参数错误").ToJsonApi();
        //        var model = JsonEx.JsonToObj<List<TbUserRole>>(modelstr);
        //        //if (type == "add")
        //        //{
        //        var data = _userRole.InsertNew(model, true);
        //        return data.ToJsonApi();
        //        //}
        //    }
        //    catch (Exception)
        //    {
        //        return AjaxResult.Error("操作失败").ToJsonApi();
        //    }
        //}

        ///// <summary>
        ///// 删除
        ///// </summary>
        ///// <param name="keyValue"></param>
        ///// <returns></returns>
        //[HttpGet]
        //public HttpResponseMessage DeleteRoleUserData(string roleCode, string UserCode)
        //{
        //    var data = _userRole.Delete(roleCode, UserCode, true);
        //    return data.ToJsonApi();
        //}
        /// <summary>
        /// 获取用户角色地址
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public HttpResponseMessage GetUserRoleList(string url)
        {
            try
            {
                var dataJson = Get(strBMUrl + url);
                JObject jo = (JObject)JsonConvert.DeserializeObject(dataJson);
                string modelstr = jo["data"] == null ? "" : jo["data"].ToString();
                if (string.IsNullOrWhiteSpace(modelstr))
                    return AjaxResult.Error("参数错误").ToJsonApi();
                DataTable dt = ToDataTable(modelstr);
                dt.Columns["UserId"].ColumnName = "UserCode";
                dt.Columns["RoleId"].ColumnName = "RoleCode";
                List<TbUserRole> model = ModelConvertHelper<TbUserRole>.ToList(dt);
                var data = _userRole.InsertNew(model, true);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }
        #endregion

        #region 其他公共方法

        /// <summary>
        /// 通过Get方式获取BM接口
        /// </summary>
        /// <param name="Url"></param>
        /// <returns></returns>
        public string Get(string Url)
        {
            string strResult = "";

            try
            {
                HttpWebRequest request = WebRequest.Create(Url) as HttpWebRequest;
                request.Method = "GET";
                request.Accept = "text/html, application/xhtml+xml, */*";
                request.Headers.Add("Accept-Language", "zh-CN");
                request.ContentType = contentType;
                request.UserAgent = DefaultUserAgent;
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8);
                string retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();

                if (response != null)
                {
                    response.Close();
                }
                if (request != null)
                {
                    request.Abort();
                }

                return retString;
            }
            catch (Exception ex)
            {
                strResult = "错误：" + ex.Message;
            }
            return strResult;
        }

        /// <summary>
        /// Json 字符串 转换为 DataTable数据集合
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public DataTable ToDataTable(string json)
        {
            DataTable dataTable = new DataTable();  //实例化
            DataTable result;
            try
            {
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                javaScriptSerializer.MaxJsonLength = Int32.MaxValue; //取得最大数值
                ArrayList arrayList = javaScriptSerializer.Deserialize<ArrayList>(json);
                if (arrayList.Count > 0)
                {
                    foreach (Dictionary<string, object> dictionary in arrayList)
                    {
                        if (dictionary.Keys.Count<string>() == 0)
                        {
                            result = dataTable;
                            return result;
                        }
                        if (dataTable.Columns.Count == 0)
                        {
                            foreach (string current in dictionary.Keys)
                            {
                                dataTable.Columns.Add(current, dictionary[current].GetType());
                            }
                        }
                        DataRow dataRow = dataTable.NewRow();
                        foreach (string current in dictionary.Keys)
                        {
                            dataRow[current] = dictionary[current];
                        }

                        dataTable.Rows.Add(dataRow); //循环添加行到DataTable中
                    }
                }
            }
            catch
            {
            }
            result = dataTable;
            return result;
        }

        #endregion

        #region 短信提醒

        /// <summary>
        /// 装卸车短信提醒
        /// </summary>
        /// <param name="SiteName"></param>
        /// <param name="TypeCode"></param>
        /// <param name="Iph"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage ImpShortMessage(string SiteName, string TypeCode, string Iph)
        {
            var dt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var content = "你【" + SiteName + "】的【" + TypeCode + "】构件订单已于【" + dt + "】出加工厂，请做好卸货准备！";
            var textResponse = JsonEx.JsonToObj<ShortMess>(ShortMessagePC(Iph, content)).data;
            if (textResponse[0].code != 2)
            {
                var val = MessageStr(textResponse[0].code);
                if (val != "成功")
                {
                    return AjaxResult.Error(val).ToJsonApi();
                }
                else
                {
                    return AjaxResult.Success(val).ToJsonApi();
                }
            }
            return AjaxResult.Success("成功").ToJsonApi();
        }

        /// <summary>
        /// 加工厂转发卸货等待信息—给工区工程部
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetUnloadingWaiting(string dcode)
        {
            try
            {
                ShortMessageTemplateLogic _smtLogic = new ShortMessageTemplateLogic();
                var data = _smtLogic.UnloadingWaiting(dcode);
                if (data.Rows.Count != 0)
                {
                    if (data.Rows[0]["Phone"].ToString() != "")
                    {
                        var NTime = DateTime.Now;
                        var dcTime = DateTime.Parse(data.Rows[0]["InsertTime"].ToString());
                        TimeSpan ts = NTime - dcTime;
                        var Hours = ts.Minutes / 60;
                        var textResponse = JsonEx.JsonToObj<ShortMess>(ShortMessagePC(data.Rows[0]["Phone"].ToString(), "你【" + data.Rows[0]["CompanyFullName"].ToString() + "】的【" + data.Rows[0]["TypeCode"].ToString() + "】构件订单已于【" + data.Rows[0]["InsertTime"].ToString() + "】配送到场，但卸货等待时间已超过【" + Hours + "】小时，请及时处理！")).data;
                        if (textResponse[0].code != 2)
                        {
                            var val = MessageStr(textResponse[0].code);
                            if (val != "成功")
                            {
                                return AjaxResult.Error(val).ToJsonApi();
                            }
                            else
                            {
                                return AjaxResult.Success(val).ToJsonApi();
                            }
                        }
                    }
                    return AjaxResult.Warning("通知人员电话错误！").ToJsonApi();
                }
                return AjaxResult.Success("成功").ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Success("错误").ToJsonApi();
            }
        }

        /// <summary>
        /// 短信提示
        /// </summary>
        /// <param name="phones">提醒的手机号，多个以","隔开</param>
        /// <param name="content">发送的内容</param>
        /// <returns></returns>
        public string ShortMessagePC(string phones, string content)
        {
            //发送短信网址
            var url = "http://124.42.243.98:8089/EPCPMS/sys/sendMsg.json";
            string textResponse = "";
            try
            {
                string postData = string.Format("phones={0}&content={1}", phones, content); // 要发放的数据
                byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(postData);
                HttpWebRequest objWebRequest = (HttpWebRequest)WebRequest.Create(url); //发送地址
                objWebRequest.Method = "POST";//提交方式
                objWebRequest.ContentType = "application/x-www-form-urlencoded";
                objWebRequest.ContentLength = byteArray.Length;
                Stream newStream = objWebRequest.GetRequestStream(); // Send the data.
                newStream.Write(byteArray, 0, byteArray.Length); //写入参数
                newStream.Close();
                HttpWebResponse response = (HttpWebResponse)objWebRequest.GetResponse();//获取响应
                StreamReader sr = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8);
                //textResponse = sr.ReadToEnd() + "返回数据"; // 返回的数据
                textResponse = sr.ReadToEnd();
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            return textResponse;
        }

        /// <summary>
        /// 短信状态返回判断
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public string MessageStr(int code)
        {
            switch (code)
            {
                case 2:
                    return "成功";
                case 400:
                    return "非法IP访问";
                case 401:
                    return "账号不能为空";
                case 403:
                    return "手机不能为空";
                case 4030:
                    return "手机号码已被列入黑名单";
                case 404:
                    return "短信内容不能为空";
                case 405:
                    return "APIID 或 API KEY 不正确";
                case 4050:
                    return "账号被冻结";
                case 40501:
                    return "动态密码已过期";
                case 40502:
                    return "动态密码校验失败";
                case 4051:
                    return "剩余条数不足";
                case 4052:
                    return "访问IP与备案IP不符";
                case 406:
                    return "手机号码格式不正确";
                case 407:
                    return "短信内容喊有敏感字符串";
                case 4070:
                    return "签名格式不正确";
                case 4071:
                    return "没有提示备案模板";
                case 4072:
                    return "提交的短信内容与审核通过的模板内容不匹配";
                case 40722:
                    return "变量的内容超过指定的长度【8】";
                case 4073:
                    return "短信内容超出长度限制";
                case 4074:
                    return "短信内容包含emoji符号";
                case 4075:
                    return "签名未通过审核";
                default:
                    return "";
            }
        }

        #endregion

        #region App菜单
        [HttpGet]
        public HttpResponseMessage GetMenuList(string RoleCode, string UserId)
        {
            try
            {
                var list = new List<TbSysMenuApp>();
                //判断人员角色菜单
                if (!string.IsNullOrEmpty(RoleCode))
                {
                    var roleMenuList = _sysMenu.GetCurrentMenuListByRoleCodeApp(RoleCode);
                    if (roleMenuList.Count > 0)
                        list.AddRange(roleMenuList);
                }
                //判断人员菜单
                if (!string.IsNullOrEmpty(UserId))
                {
                    var userMenuList = _sysMenu.GetCurrentMenuListByUserCodeApp(UserId);
                    if (userMenuList.Count > 0)
                        list.AddRange(userMenuList);
                }
                //去重
                list = list.Where((x, firstId) => list.FindIndex(z => z.MenuCode == x.MenuCode) == firstId).OrderBy(x => x.Sort).ToList();
                string a = ToMenuJson(list, "0");
                JArray ja = (JArray)JsonConvert.DeserializeObject(a);
                return AjaxResult.Success(ja).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("获取App菜单失败").ToJsonApi();
            }

        }
        private string ToMenuJson(List<TbSysMenuApp> data, string parentId)
        {
            StringBuilder sbJson = new StringBuilder();
            sbJson.Append("[");
            List<TbSysMenuApp> entitys = data.FindAll(t => t.MenuPCode == parentId);
            if (entitys.Count > 0)
            {
                foreach (var item in entitys)
                {
                    string strJson = item.ToJson();
                    strJson = strJson.Insert(strJson.Length - 1, ",\"ChildNodes\":" + ToMenuJson(data, item.MenuCode) + "");
                    sbJson.Append(strJson + ",");
                }
                sbJson = sbJson.Remove(sbJson.Length - 1, 1);
            }
            sbJson.Append("]");
            return sbJson.ToString();
        }
        #endregion

        #region  获取App当前登录人所属组织机构跟下级组织机构
        public HttpResponseMessage GetLoginUserAllCompany(string CompanyCode, string OrgType, string ProjectId)
        {
            var listAll = new List<TbCompanyApp>();
            if (OrgType == "1")//当前登录人是加工厂人员（加载所以线的组织机构数据）
            {
                string sqlJgc = @"select cp.id,cp.CompanyCode,cp.ParentCompanyCode,cp.CompanyFullName,cp.Address,cp.OrgType,pc.ProjectId from  TbCompany cp
                           left join TbProjectCompany pc on cp.CompanyCode=pc.CompanyCode where cp.OrgType!=1 order by cp.id asc";
                DataTable retjgc = Db.Context.FromSql(sqlJgc).ToDataTable();
                List<TbCompanyApp> jgcList = ModelConvertHelper<TbCompanyApp>.ToList(retjgc);
                listAll.AddRange(jgcList);
            }
            else// 当前登录人是经理部、分部、工区、站点人员（加载所属跟下级）
            {
                //获取当前登录人的所有上级除本身
                string sqlParentCompany = @"WITH T
                                        AS( 
                                            SELECT id,CompanyCode,ParentCompanyCode,CompanyFullName,Address,OrgType FROM TbCompany WHERE CompanyCode=@CompanyCode 
                                            UNION ALL 
                                            SELECT a.id,a.CompanyCode,a.ParentCompanyCode,a.CompanyFullName,a.Address,a.OrgType 
                                            FROM TbCompany a INNER JOIN T ON a.CompanyCode=T.ParentCompanyCode  
                                        ) 
                                        SELECT * FROM T 
                                        left join TbProjectCompany tpc on T.CompanyCode=tpc.CompanyCode where tpc.ProjectId=@ProjectId and T.CompanyCode!=@CompanyCode and T.OrgType!=1
                                        order by T.id asc;";
                DataTable retParent = Db.Context.FromSql(sqlParentCompany).AddInParameter("@CompanyCode", DbType.String, CompanyCode).AddInParameter("@ProjectId", DbType.String, ProjectId).ToDataTable();
                List<TbCompanyApp> parentList = ModelConvertHelper<TbCompanyApp>.ToList(retParent);
                listAll.AddRange(parentList);
                //获取当前登录人的所有下级包括本身
                string sqlSonCompany = @"WITH T
                                     AS( 
                                         SELECT id,CompanyCode,ParentCompanyCode,CompanyFullName,Address,OrgType FROM TbCompany WHERE CompanyCode=@CompanyCode 
                                         UNION ALL 
                                         SELECT a.id,a.CompanyCode,a.ParentCompanyCode,a.CompanyFullName,a.Address,a.OrgType  FROM TbCompany a INNER JOIN T ON a.ParentCompanyCode=T.CompanyCode  
                                     ) 
                                     SELECT T.*,tpc.ProjectId FROM T 
                                     left join TbProjectCompany tpc on T.CompanyCode=tpc.CompanyCode where tpc.ProjectId=@ProjectId and T.OrgType!=1
                                     order by T.id asc;";
                DataTable retSon = Db.Context.FromSql(sqlSonCompany).AddInParameter("@CompanyCode", DbType.String, CompanyCode).AddInParameter("@ProjectId", DbType.String, ProjectId).ToDataTable();
                List<TbCompanyApp> sonList = ModelConvertHelper<TbCompanyApp>.ToList(retSon);
                listAll.AddRange(sonList);
            }
            return AjaxResult.Success(listAll).ToJsonApi();
        }

        #endregion

        #region 获取原材料

        [HttpGet]
        /// 原材料名称查询
        public HttpResponseMessage MaterialNameSelect()
        {
            var data = _Plan.MaterialNameSelect();
            return AjaxResult.Success(data).ToJsonApi();
        }

        #endregion

        #region 获取数据字典
        [HttpGet]
        /// <summary>
        /// 根据编码进行查询信息
        /// </summary>
        /// <param name="dicCode"></param>
        /// <returns></returns>
        public HttpResponseMessage GetDicByCode(string dicCode)
        {
            var data = ddi.GetDicByCode(dicCode).ToList();
            return AjaxResult.Success(data).ToJsonApi();
        }

        #endregion

        #region 获取站点联系人电话
        /// <summary>
        /// 获取站点联系人电话
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetUserPhone(string userId)
        {
            try
            {
                string url = "server/byServer/queryUserInfo.json?userId=" + userId;
                var dataJson = Get(strBMUrl + url);
                JObject jo = (JObject)JsonConvert.DeserializeObject(dataJson);
                string modelstr = jo["data"] == null ? "" : jo["data"].ToString();
                if (string.IsNullOrWhiteSpace(modelstr))
                    return AjaxResult.Error("参数错误").ToJsonApi();

                return AjaxResult.Success(jo["data"]).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }
        #endregion
    }
}
