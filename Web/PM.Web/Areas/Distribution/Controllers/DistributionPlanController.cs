using Dos.Common;
using PM.Business.Distribution;
using PM.Common.Extension;
using PM.Common.Helper;
using PM.DataEntity;
using PM.DataEntity.Distribution.ViewModel;
using PM.Web.Models.ExcelModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.Distribution.Controllers
{
    [HandlerLogin]
    public class DistributionPlanController : BaseController
    {
        private readonly DistributionPlan _Plan = new DistributionPlan();
        //
        // GET: /Distribution/DistributionPlan/
        //配送管理-配送计划

        #region 数据列表页
        public ActionResult Index()
        {
            ViewBag.InsertCode = base.UserCode;
            return View();
        }

        /// <summary>
        /// 高级查询类型名称查询
        /// </summary>
        /// <returns></returns>
        public ActionResult TypeNameSelect() 
        {
            var data = _Plan.TypeNameSelect();
            return Content(data.ToJson());
        }

        /// 原材料名称查询
        public ActionResult MaterialNameSelect() 
        {
            var data = _Plan.MaterialNameSelect();
            return Content(data.ToJson());
        }

        /// <summary>
        /// 数据列表信息查询
        /// </summary>
        /// <returns></returns>
        public ActionResult GetGridJson(TbDistributionPlanInfoRequest request) 
        {
            var data = _Plan.GetGridJson(request);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 加载报表数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetDistributionForm(TbDistributionPlanInfoRequest request)
        {
            var data = _Plan.GetDistributionForm(request);
            return Content(data.ToJson());
        }
        #endregion

        #region 查看页面
        [HandlerLogin(Ignore = false)]
        public ActionResult Details() 
        {
            return View();
        }

        /// <summary>
        /// 查看页面数据查询
        /// </summary>
        /// <returns></returns>

        [HandlerLogin(Ignore = false)]
        public ActionResult GetFormJson(int keyValue) 
        {
            var data = _Plan.GetFormJson(keyValue);
            return Content(data.ToJson());
        }
        #endregion

        #region 修改页面
        public ActionResult Form(string keyValue, DateTime DistributionTime)
        {
            if (string.IsNullOrWhiteSpace(keyValue) || DistributionTime!=null) 
            {
                ViewBag.keyValue = keyValue;
                ViewBag.DistributionTime = DistributionTime;
            }
            return View();
        }

        //修改计划配送时间
        public ActionResult SubmitForm(int ID, DateTime DistributionTime)
        {
            var data = _Plan.SubmitForm(ID, DistributionTime);
            return Content(data.ToJson());
        }
        #endregion

        #region 导出
        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="ProjectCode"></param>
        /// <returns></returns>
        public ActionResult OutputExcel(string jsonData)
        {
            var request = JsonEx.JsonToObj<TbDistributionPlanInfoRequest>(jsonData);
            request.IsOutPut = true;
            var ret = _Plan.GetGridJson(request);
            decimal zlhj = 0;
            var data = (List<DistributionPlanInfoModel>)ret.rows;
            if (data.Count > 0)
            {
                zlhj = data.Sum(p=>p.WeightTotal.Value);
            }
            string hzzfc = "总量合计(KG):" + zlhj;
            var dataList = MapperHelper.Map<DistributionPlanInfoModel, DistributionPlanExcel> (data);
            var fileStream = ExcelHelper.EntityListToExcelStream<DistributionPlanExcel>(dataList, "配送计划", hzzfc);
            return File(fileStream, "application/vnd.ms-excel", "配送计划.xls");
        }

        #endregion

        #region 统计报表

        public ActionResult Img1(TbDistributionPlanInfoRequest request)
        {
            var data = _Plan.Img1(request);
            return Content(data.ToJson());
        }

        public ActionResult Img2(TbDistributionPlanInfoRequest request)
        {
            var data = _Plan.Img2(request);
            return Content(data.ToJson());
        }

        public ActionResult Img3(TbDistributionPlanInfoRequest request)
        {
            var data = _Plan.Img3(request);
            return Content(data.ToJson());
        }

        #endregion

    }
}
