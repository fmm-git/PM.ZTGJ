using PM.Common;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic;
using PM.Business;
using PM.DataEntity;
using PM.Business.Production;
using PM.Business.Distribution;
using PM.Business.System;
using PM.DataEntity.System.ViewModel;
using PM.Business.RawMaterial;

namespace PM.Web.Controllers
{
    [HandlerLogin]
    public class HomeController : BaseController
    {

        private readonly UserLogic _User = new UserLogic();
        private readonly TbWorkOrderLogic _workorder = new TbWorkOrderLogic();
        TransportFlowLogic _transportFlow = new TransportFlowLogic();
        private HomeLogic _Home = new HomeLogic();
        private readonly TbRawMaterialMonthDemandPlanLogic _rawMonthDemPlanLogic = new TbRawMaterialMonthDemandPlanLogic();
        public HomeController()
        {
        }

        #region 切换组织机构

        /// <summary>
        /// 切换项目组织机构
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdateProjectOrg()
        {
            ViewBag.ProjectOrgAllId = OperatorProvider.Provider.CurrentUser.ProjectOrgAllId;
            return View();
        }

        public ActionResult GetAllProjectOrg()
        {
            string UserCode = OperatorProvider.Provider.CurrentUser.UserId;
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
            return Content(treeList.TreeGridJson());
        }

        //切换组织机构修改登陆信息中的组织机构相关信息
        public ActionResult SaveProjectOrg(string OrgId, string OrgName, string ProjectId, string OrgType, string ProjectOrgAllId, string ProjectOrgAllName)
        {
            string UserCode = OperatorProvider.Provider.CurrentUser.UserCode;
            var operatorModel = _User.SaveProjectOrg(UserCode, OrgId, OrgName, ProjectId, OrgType, ProjectOrgAllId, ProjectOrgAllName);
            OperatorProvider.Provider.RemoveCurrent();//先移除Cookie或者Session
            OperatorProvider.Provider.AddCurrent(operatorModel);//在添加Cookie或者Session
            Session["username"] = OperatorProvider.Provider.CurrentUser.UserName;
            Session["usercode"] = OperatorProvider.Provider.CurrentUser.UserCode;
            Session["userid"] = OperatorProvider.Provider.CurrentUser.UserId;
            return Content(operatorModel.ToJson());
        }


        #endregion

        #region 切换加工厂
        /// <summary>
        /// 切换项目组织机构
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdateProjectJgc()
        {
            ViewBag.ProcessFactoryCode = OperatorProvider.Provider.CurrentUser.ProcessFactoryCode;
            ViewBag.ProcessFactoryName = OperatorProvider.Provider.CurrentUser.ProcessFactoryName;
            return View();
        }
        public ActionResult GetAllProjectJgc(PageSearchRequest request)
        {
            request.rows = 50;
            var data = _User.GetAllProjectJgc(request);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 切换加工厂信息，将当前切换的加工厂信息存到登陆信息中
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveProcessFactoryCode(string ProcessFactoryCode, string ProcessFactoryName)
        {
            var operatorModel = _User.SaveProcessFactoryCode(ProcessFactoryCode, ProcessFactoryName);
            OperatorProvider.Provider.RemoveCurrent();//先移除Cookie或者Session
            OperatorProvider.Provider.AddCurrent(operatorModel);//在添加Cookie或者Session
            Session["username"] = OperatorProvider.Provider.CurrentUser.UserName;
            Session["usercode"] = OperatorProvider.Provider.CurrentUser.UserCode;
            Session["userid"] = OperatorProvider.Provider.CurrentUser.UserId;
            return Content(operatorModel.ToJson());
        }
        #endregion

        #region App下载

        public ActionResult AppDown()
        {
            return View();
        }

        #endregion

        /// <summary>
        /// 修改密码窗体
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdatePwd()
        {
            ViewBag.UserCode = base.UserCode;
            return View();
        }

        /// <summary>
        /// 确认原密码，如果原密码输入错误，禁止用户操作
        /// </summary>
        /// <returns></returns>
        public ActionResult GetPwd(string UserCode)
        {
            var data = _User.SelectPwd(UserCode);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 修改密码按钮事件
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdatePwdA(UserRequest result, string UserCode)
        {
            var data = _User.UpDatePwd(result, UserCode);
            return Content(data.ToJson());
        }

        public ActionResult Index()
        {
            //return View(OperatorProvider.Provider.CurrentUser);
            ViewBag.UserName = base.UserName;
            ViewBag.OrgName = OperatorProvider.Provider.CurrentUser.ComPanyName;
            ViewBag.OrgId = OperatorProvider.Provider.CurrentUser.CompanyId;
            ViewBag.ProjectOrgAllName = OperatorProvider.Provider.CurrentUser.ProjectOrgAllName;
            ViewBag.OrgType = OperatorProvider.Provider.CurrentUser.OrgType;
            ViewBag.ProcessFactoryCode = OperatorProvider.Provider.CurrentUser.ProcessFactoryCode;
            ViewBag.ProcessFactoryName = OperatorProvider.Provider.CurrentUser.ProcessFactoryName;
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Main()
        {
            return View();
        }
        public ActionResult NoRight()
        {
            return View();
        }
        //欢迎首页
        [HttpGet]
        public ActionResult Default()
        {
            ViewBag.UserName = base.UserName;
            ViewBag.OrgType = OperatorProvider.Provider.CurrentUser.OrgType;
            ViewBag.ProcessFactoryCode = OperatorProvider.Provider.CurrentUser.ProcessFactoryCode;
            return View();
        }

        #region 首页库存、加工完成量、配送量、签收量统计
        public ActionResult GetTotelNum(int Year, int Month)
        {
            string ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
            string ProcessFactoryCode = OperatorProvider.Provider.CurrentUser.ProcessFactoryCode;
            string CompanyId = OperatorProvider.Provider.CurrentUser.CompanyId;
            string OrgType = OperatorProvider.Provider.CurrentUser.OrgType;
            var data = _workorder.GetTotelNum(ProjectId, ProcessFactoryCode, CompanyId, OrgType, Year, Month);
            var a = new
            {
                StockCount = data.Rows[0]["TotelNum"],
                OrderCount = data.Rows[3]["TotelNum"],
                OrderScheduleCount = data.Rows[1]["TotelNum"],
                YiDeliveryCount = data.Rows[2]["TotelNum"]
            };
            return Json(a, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 产能信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetCapacityNum(string CapacityMonth)
        {
            try
            {
                string ProcessFactoryCode = OperatorProvider.Provider.CurrentUser.ProcessFactoryCode;
                string OrgType = OperatorProvider.Provider.CurrentUser.OrgType;
                var data = _workorder.GetCapacityNum(ProcessFactoryCode, CapacityMonth);
                if (data != null && data.Rows.Count > 0)
                {
                    var a = new
                    {
                        ycl = data.Rows[0]["Capacity"],
                        sjcz = data.Rows[0]["WeightSmallPlan"],
                        sjfh = data.Rows[0]["ActualLoadNew"]
                    };
                    return Json(a, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var a = new
                    {
                        ycl = 0,
                        sjcz = 0,
                        sjfh = 0
                    };
                    return Json(a, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        #endregion


        #region 首页数据展示

        /// <summary>
        /// 报警饼图信息分析
        /// </summary>
        /// <param name="Year"></param>
        /// <param name="Month"></param>
        /// <returns></returns>
        public ActionResult GetAlarmMessage(HomeRequest request)
        {
            var data = _User.GetAlarmMessage(request);
            return Content(data.ToJson());
        }

        #region 报警饼图明细列表

        /// <summary>
        /// 审批超时
        /// </summary>
        /// <param name="Year"></param>
        /// 
        /// <param name="Month"></param>
        /// <returns></returns>
        public ActionResult GetSpCsYjList(HomeRequest request)
        {
            var data = _User.GetSpCsYjList(request);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 未按时提报月度需求计划
        /// </summary>
        /// <param name="DateType"></param>
        /// <returns></returns>
        public ActionResult GetWbydxqjhYjList(HomeRequest request)
        {
            var data = _User.GetWbydxqjhYjList(request);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 原材料供货超时
        /// </summary>
        /// <param name="DateType"></param>
        /// <returns></returns>
        public ActionResult GetYclghCsYjList(HomeRequest request)
        {
            var data = _User.GetYclghCsYjList(request);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 加急订单个数
        /// </summary>
        /// <param name="DateType"></param>
        /// <returns></returns>
        public ActionResult GetJjOrderYjList(HomeRequest request)
        {
            var data = _User.GetJjOrderYjList(request);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 加工进度滞后
        /// </summary>
        /// <param name="DateType"></param>
        /// <returns></returns>
        public ActionResult GetJgOrderZhYjList(HomeRequest request)
        {
            var data = _User.GetJgOrderZhYjList(request);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 配送超期
        /// </summary>
        /// <param name="DateType"></param>
        /// <returns></returns>
        public ActionResult GetPsCsYjList(HomeRequest request)
        {
            var data = _User.GetPsCsYjList(request);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 卸货超期
        /// </summary>
        /// <param name="DateType"></param>
        /// <returns></returns>
        public ActionResult GetXhCsYjList(HomeRequest request)
        {
            var data = _User.GetXhCsYjList(request);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 签收超期
        /// </summary>
        /// <param name="DateType"></param>
        /// <returns></returns>
        public ActionResult GetQsCsYjList(HomeRequest request)
        {
            var data = _User.GetQsCsYjList(request);
            return Content(data.ToJson());
        }

        #endregion

        public ActionResult GetJiaoLiu(PageSearchRequest psr)
        {
            var data = _User.GetJiaoLiu(psr);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 延时供货列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetDelayedSupplyList()
        {
            var data = _workorder.GetDelayedSupplyList(OperatorProvider.Provider.CurrentUser.ProjectId, OperatorProvider.Provider.CurrentUser.ProcessFactoryCode, OperatorProvider.Provider.CurrentUser.CompanyId, OperatorProvider.Provider.CurrentUser.OrgType);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 加急订单
        /// </summary>
        /// <returns></returns>
        public ActionResult GetUrgentWorkOrderList()
        {
            var data = _workorder.GetUrgentWorkOrderList(OperatorProvider.Provider.CurrentUser.ProjectId, OperatorProvider.Provider.CurrentUser.ProcessFactoryCode, OperatorProvider.Provider.CurrentUser.CompanyId, OperatorProvider.Provider.CurrentUser.OrgType);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 卸货过程时间展示
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetCarReportGridJson(TransportProcessRequest request, string Year, string Month)
        {
            if (string.IsNullOrWhiteSpace(Year))
            {
                Year = DateTime.Now.Year.ToString();
            }
            if (string.IsNullOrWhiteSpace(Month))
            {
                Month = DateTime.Now.Month.ToString();
            }
            var data = _User.GetCarReportGridJson(request, Year, Month);
            return Content(data.ToJson());
        }

        #endregion

        #region 接口测试页面
        public ActionResult InterfaceTestPage()
        {
            return View();
        }
        #endregion

        #region 首页数据展示 NEW

        /// <summary>
        /// 加工厂引导页
        /// </summary>
        /// <returns></returns>
        public ActionResult GuidePage()
        {
            ViewBag.OrgType = OperatorProvider.Provider.CurrentUser.OrgType;
            ViewBag.ProcessFactoryCode = OperatorProvider.Provider.CurrentUser.ProcessFactoryCode;
            ViewBag.ProcessFactoryName = OperatorProvider.Provider.CurrentUser.ProcessFactoryName;
            return View();
        }
        /// <summary>
        /// 订单(更多)
        /// </summary>
        /// <returns></returns>
        public ActionResult OrderMore()
        {
            return View();
        }
        /// <summary>
        /// 需求量(更多)
        /// </summary>
        /// <returns></returns>
        public ActionResult SupplyrMore()
        {
            return View();
        }

        /// <summary>
        /// 加工厂数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetFactoryInfo(HomeRequest request)
        {
            var data = _Home.GetFactoryInfo(request);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 获取年月日数据
        /// </summary>
        /// <returns></returns>
        public ActionResult GetDMYData(HomeRequest request)
        {
            var data = _Home.GetDMYData(request);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 订单信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetOrderData(HomeRequest request)
        {
            var data = _Home.GetOrderData(request);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 获取库存信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetRawMaterialStockData(HomeRequest request)
        {
            var data = _Home.GetRawMaterialStockData(request);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 获取生产计划阶段数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetRawMaterialStockPlanData(HomeRequest request)
        {
            var data = _Home.GetRawMaterialStockPlanData(request);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 月度计划统计图
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetMonthPlanReport(HomeRequest request)
        {
            var data = _Home.GetMonthPlanReport(request);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 加工订单统计图
        /// </summary>
        /// <returns></returns>
        public ActionResult GetOrderInfoReport(HomeRequest request)
        {
            var data = _Home.GetOrderInfoReport(request);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 供货单统计图
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetSupplyDataReport(HomeRequest request)
        {
            var data = _Home.GetSupplyDataReport(request);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 组织机构
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetOrgLabDataForFactory(HomeRequest request)
        {
            var data = _Home.GetOrgLabDataForFactory(request);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 组织机构标签(经理部)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetOrgLabDataForJLB(HomeRequest request)
        {
            var data = _Home.GetOrgLabDataForJLB(request);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 历史接单量统计
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetOrderMore(HomeRequest request)
        {
            var data = _Home.GetOrderMore(request);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 供应量需求量统计
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetSupplyrMore(HomeRequest request)
        {
            var data = _Home.GetSupplyrMore(request);
            return Content(data.ToJson());
        }
        #endregion
    }
}