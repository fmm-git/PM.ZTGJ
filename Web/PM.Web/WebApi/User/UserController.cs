using Newtonsoft.Json.Linq;
using PM.Business;
using PM.Business.Flow;
using PM.Business.Production;
using PM.Common;
using PM.Common.Extension;
using PM.DataAccess;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using PM.DataEntity.System.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace PM.Web.WebApi.User
{
    /// <summary>
    /// 账户信息Api
    /// </summary>
    public class UserController : ApiController
    {
        private readonly UserLogic _User = new UserLogic();
        private readonly FlowPerformLogic _flowperform = new FlowPerformLogic();
        private readonly TbFlowEarlyWarningConditionLogic _earlyWarning = new TbFlowEarlyWarningConditionLogic();
        private readonly TbWorkOrderLogic _workorder = new TbWorkOrderLogic();

        #region 登录

        [HttpGet]
        public HttpResponseMessage UserLogin(string userName, string pwd)
        {
            if ((!string.IsNullOrEmpty(userName)) && (!string.IsNullOrEmpty(pwd)))
            {
                var password = PM.Common.Encryption.EncryptionFactory.Md5Encrypt(pwd);
                var user = Db.Context.From<TbUser>().Where(d => d.UserCode == userName && d.UserPwd == password).First();
                if (user != null)
                {
                    //查询是否是离职人员
                    var data = new UserLogic().UserClosedSelect(userName);
                    if (data == "-1")
                    {
                        return AjaxResult.Error("您为离职人员，无法进行登陆").ToJsonApi();
                    }
                    else
                    {
                        //查找角色信息
                        var operatorModel = new TbUserRoleLogic().FindUserInfo(user.UserCode);
                        operatorModel.LoginTime = DateTime.Now;
                        operatorModel.LoginToken = DESEncrypt.Encrypt(Guid.NewGuid().ToString());
                        if (user.UserName == "100000")
                        {
                            operatorModel.IsSystem = true;
                        }
                        else
                        {
                            operatorModel.IsSystem = false;
                        }
                        //判断是否是司机
                        var isAny = Repository<TbCarInfoDetail>.Any(p => p.UserCode == user.UserCode);
                        if (isAny)
                            operatorModel.IsDriver = true;
                        return AjaxResult.Success(operatorModel).ToJsonApi();

                    }
                }
                else
                {
                    return AjaxResult.Error("账号或密码错误!").ToJsonApi();
                }
            }
            else
            {
                return AjaxResult.Error("用户名或密码不能为空！").ToJsonApi();
            }
        }

        #endregion

        #region App加工厂切换、组织机构切换
        /// <summary>
        /// 获取用户下的所有组织机构
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetAllProjectOrg(string UserId)
        {
            string UserCode = UserId;
            if (!string.IsNullOrWhiteSpace(UserCode))
            {
                var list = _User.GetAllProjectOrg(UserCode);
                var treeList = new List<TreeGridModel>();
                foreach (var item in list)
                {
                    bool hasChildren = list.Count(p => p.ProjectId == item.OrgId) == 0 ? false : true;
                    TreeGridModel treeModel = new TreeGridModel();
                    treeModel.id = item.OrgId;
                    treeModel.text = item.OrgName;
                    treeModel.isLeaf = hasChildren;
                    treeModel.parentId = item.ProjectId;
                    treeModel.expanded = true;
                    treeModel.entityJson = item.ToJson();
                    treeList.Add(treeModel);
                }
                JObject jo = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(treeList.TreeGridJson());//将string转换为json对象
                return AjaxResult.Success(jo).ToJsonApi();
            }
            else
            {
                return AjaxResult.Error("接口信息错误:参数错误").ToJsonApi();
            }

        }

        /// <summary>
        /// 获取所有加工厂
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetAllProjectJgc()
        {
            PageSearchRequest request = new PageSearchRequest();
            var data = _User.GetAllProjectJgc(request);
            return AjaxResult.Success(data).ToJsonApi();
        }

        [HttpPost]
        public HttpResponseMessage AppSaveJgcOrOrg([FromBody]JObject jdata)
        {
            //{
            //"model": {
            //        "OrgType": "2",
            //        "ComPanyName": "十一号线经理部",
            //        "CompanyId": "6247574415609954304",
            //        "ProjectOrgAllId": "6247574415609954304",
            //        "ProjectOrgAllName": "十一号线经理部",
            //        "ProjectId": "6245721945602523136",
            //        "RoleCode": "6352967650544590848",
            //        "UserCode": "102787",
            //        "UserId": "6487716734823440384",
            //        "UserName": "李良博",
            //        "LoginTime": "2019-04-25 15:31:11",
            //        "LoginToken": "9BD8717A3C93C93C5CAEE8FBA126D353AD4562C10CE22F487CA47731EB6E842E3B6064B1DC8A4B75",
            //        "IsSystem": false,
            //        "IsDriver": false,
            //        "ProcessFactoryCode": "",
            //        "ProcessFactoryName": "所有加工厂"
            //    }
            //,type:"Jgc"
            //}
            string modelstr = jdata["model"] == null ? "" : jdata["model"].ToString();
            string type = jdata["type"] == null ? "" : jdata["type"].ToString();
            if (string.IsNullOrWhiteSpace(modelstr) || string.IsNullOrWhiteSpace(type))
                return AjaxResult.Error("参数错误").ToJsonApi();
            var model = JsonEx.JsonToObj<CurrentUserInfo>(modelstr);
            CurrentUserInfo operatorModel = null;
            if (type == "Jgc")
            {
                operatorModel = _User.AppSaveProcessFactoryCode(model);
            }
            else if (type == "Org")
            {
                operatorModel = _User.AppSaveProjectOrg(model);
            }
            OperatorProvider.Provider.RemoveCurrent();//先移除Cookie或者Session
            OperatorProvider.Provider.AddCurrent(operatorModel);//在添加Cookie或者Session
            return operatorModel.ToJsonApi();

        }

        #endregion


        #region 首页当月报警类型

        [HttpGet]
        /// 原材料名称查询
        public HttpResponseMessage GetAlarmMessage(string ProjectId, string ProcessFactoryCode, string SiteCode, string Year, string Month)
        {
            var request = new HomeRequest(ProjectId, ProcessFactoryCode, SiteCode, Year, Month);
            var data = _User.GetAlarmMessage(request);
            return AjaxResult.Success(data).ToJsonApi();
        }

        #region 预警类型明细

        [HttpGet]
        /// 审批超时
        public HttpResponseMessage GetSpCsYjList(string ProjectId, string ProcessFactoryCode, string SiteCode, string Year, string Month)
        {
            var request = new HomeRequest(ProjectId, ProcessFactoryCode, SiteCode, Year, Month);
            var data = _User.GetSpCsYjList(request);
            return AjaxResult.Success(data).ToJsonApi();
        }

        [HttpGet]
        /// 未按时提报月度需求计划
        public HttpResponseMessage GetWbydxqjhYjList(string ProjectId, string ProcessFactoryCode, string SiteCode, string Year, string Month)
        {
            var request = new HomeRequest(ProjectId, ProcessFactoryCode, SiteCode, Year, Month);
            var data = _User.GetWbydxqjhYjList(request);
            return AjaxResult.Success(data).ToJsonApi();
        }

        [HttpGet]
        /// 原材料供货超时
        public HttpResponseMessage GetYclghCsYjList(string ProjectId, string ProcessFactoryCode, string SiteCode, string Year, string Month)
        {
            var request = new HomeRequest(ProjectId, ProcessFactoryCode, SiteCode, Year, Month);
            var data = _User.GetYclghCsYjList(request);
            return AjaxResult.Success(data).ToJsonApi();
        }

        [HttpGet]
        /// 加急订单个数
        public HttpResponseMessage GetJjOrderYjList(string ProjectId, string ProcessFactoryCode, string SiteCode, string Year, string Month)
        {
            var request = new HomeRequest(ProjectId, ProcessFactoryCode, SiteCode, Year, Month);
            var data = _User.GetJjOrderYjList(request);
            return AjaxResult.Success(data).ToJsonApi();
        }

        [HttpGet]
        /// 加工进度滞后
        public HttpResponseMessage GetJgOrderZhYjList(string ProjectId, string ProcessFactoryCode, string SiteCode, string Year, string Month)
        {
            var request = new HomeRequest(ProjectId, ProcessFactoryCode, SiteCode, Year, Month);
            var data = _User.GetJgOrderZhYjList(request);
            return AjaxResult.Success(data).ToJsonApi();
        }

        [HttpGet]
        /// 接收配送超期
        public HttpResponseMessage GetPsCsYjList(string ProjectId, string ProcessFactoryCode, string SiteCode, string Year, string Month)
        {
            var request = new HomeRequest(ProjectId, ProcessFactoryCode, SiteCode, Year, Month);
            var data = _User.GetPsCsYjList(request);
            return AjaxResult.Success(data).ToJsonApi();
        }

        [HttpGet]
        /// 卸货超时
        public HttpResponseMessage GetXhCsYjList(string ProjectId, string ProcessFactoryCode, string SiteCode, string Year, string Month)
        {
            var request = new HomeRequest(ProjectId, ProcessFactoryCode, SiteCode, Year, Month);
            var data = _User.GetXhCsYjList(request);
            return AjaxResult.Success(data).ToJsonApi();
        }

        [HttpGet]
        /// 签收超时
        public HttpResponseMessage GetQsCsYjList(string ProjectId, string ProcessFactoryCode, string SiteCode, string Year, string Month)
        {
            var request = new HomeRequest(ProjectId, ProcessFactoryCode, SiteCode, Year, Month);
            var data = _User.GetJjOrderYjList(request);
            return AjaxResult.Success(data).ToJsonApi();
        }
        #endregion

        #endregion

        #region 首页产能填报

        [HttpGet]
        /// 产能填报
        public HttpResponseMessage GetCapacityNum(string ProcessFactoryCode, string CapacityMonth)
        {

            var data = _workorder.GetCapacityNum(ProcessFactoryCode, CapacityMonth);
            return AjaxResult.Success(data).ToJsonApi();
        }

        #endregion

        #region 首页预警、消息、审批信息的次数

        [HttpGet]
        /// 首页预警、信息、审批的次数
        public HttpResponseMessage GetAppCount(string UserId, string ProjectId = "")
        {

            var data = _flowperform.GetAppCount(UserId, ProjectId);
            return AjaxResult.Success(data).ToJsonApi();
        }
        [HttpGet]
        /// <summary>
        /// 我的审批状态
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="SDT"></param>
        /// <param name="EDT"></param>
        /// <returns></returns>
        public HttpResponseMessage GetAppMyApprovalState(string UserId, string ProjectId = "", DateTime? SDT = null, DateTime? EDT = null)
        {
            var data = _flowperform.GetAppMyApprovalState(UserId, ProjectId, SDT, EDT);
            return AjaxResult.Success(data).ToJsonApi();
        }
        [HttpGet]
        /// <summary>
        /// 我的审批列表
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="UserId"></param>
        /// <param name="state"></param>
        /// <param name="SDT"></param>
        /// <param name="EDT"></param>
        /// <returns></returns>
        public HttpResponseMessage GetAppMyApproval([FromUri]PageSearchRequest pt, string UserId, string ProjectId = "", int state = -1, DateTime? SDT = null, DateTime? EDT = null)
        {

            var data = _flowperform.GetAppMyApproval(pt, UserId, ProjectId, state, SDT, EDT);
            return AjaxResult.Success(data).ToJsonApi();
        }

        [HttpGet]
        /// <summary>
        /// 我的消息状态
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="SDT"></param>
        /// <param name="EDT"></param>
        /// <returns></returns>
        public HttpResponseMessage GetAppMessageState(string UserId,string ProjectId, DateTime? SDT = null, DateTime? EDT = null)
        {
            var data = _flowperform.GetMessageStateNew(UserId, ProjectId, SDT, EDT);
            return AjaxResult.Success(data).ToJsonApi();
        }
        [HttpGet]
        /// <summary>
        /// 我的消息列表
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="UserId"></param>
        /// <param name="state"></param>
        /// <param name="SDT"></param>
        /// <param name="EDT"></param>
        /// <returns></returns>
        public HttpResponseMessage GetMyMessage([FromUri]OARequest pt)
        {

            var data = _flowperform.GetMessageStateList(pt, pt.UserId, pt.ProjectId, pt.state, pt.SDT, pt.EDT);
            return AjaxResult.Success(data).ToJsonApi();
        }

        [HttpGet]
        /// <summary>
        /// 我的预警状态
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="SDT"></param>
        /// <param name="EDT"></param>
        /// <returns></returns>
        public HttpResponseMessage GetAppMyEarlyWarningState(string UserId, string ProjectId = "", DateTime? SDT = null, DateTime? EDT = null)
        {
            var data = _earlyWarning.GetMyEarlyWarningState(UserId, ProjectId, SDT, EDT);
            return AjaxResult.Success(data).ToJsonApi();
        }
        [HttpGet]
        /// <summary>
        /// 流程预警消息列表
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="UserId"></param>
        /// <param name="state"></param>
        /// <param name="SDT"></param>
        /// <param name="EDT"></param>
        /// <returns></returns>
        public HttpResponseMessage GetEarlyWarningInfo([FromUri]OARequest pt)
        {
            var data = _earlyWarning.GetEarlyWarningInfo(pt, pt.UserId, pt.ProjectId, pt.state, pt.SDT, pt.EDT);
            return AjaxResult.Success(data).ToJsonApi();
        }
        [HttpGet]
        /// <summary>
        /// 表单预警消息列表
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="UserId"></param>
        /// <param name="state"></param>
        /// <param name="SDT"></param>
        /// <param name="EDT"></param>
        /// <returns></returns>
        public HttpResponseMessage GetEarlyWarningFormInfo([FromUri]OARequest pt)
        {
            var data = _earlyWarning.GetEarlyWarningFormInfo(pt, pt.UserId, pt.ProjectId, pt.state, pt.SDT, pt.EDT);
            return AjaxResult.Success(data).ToJsonApi();
        }
        [HttpPost]
        /// <summary>
        /// 处理预警消息
        /// </summary>
        /// <param name="ID">预警消息ID</param>
        /// <param name="EarlyType">预警类型</param>
        /// <returns></returns>
        public HttpResponseMessage HandleEarlyWarning(int ID, string EarlyType)
        {
            var data = _earlyWarning.HandleEarlyWarning(ID,EarlyType).ToJson();
            return AjaxResult.Success(data).ToJsonApi();
        }
        #endregion

    }
}
