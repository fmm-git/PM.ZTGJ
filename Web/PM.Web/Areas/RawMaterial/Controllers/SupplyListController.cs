using Dos.Common;
using PM.Business.RawMaterial;
using PM.Common.Extension;
using PM.Common.Helper;
using PM.DataEntity;
using PM.DataEntity.RawMaterial.ViewModel;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace PM.Web.Areas.RawMaterial.Controllers
{
    /// <summary>
    /// 供应清单
    /// </summary>
    [HandlerLogin]
    public class SupplyListController : Controller
    {
        //
        private readonly SupplyListLogic _suplistLogic = new SupplyListLogic();

        #region  视图
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 新增/编辑页
        /// </summary>
        /// <returns></returns>
        public ActionResult Form()
        {

            return View();
        }

        /// <summary>
        /// 查询页
        /// </summary>
        /// <returns></returns>
        [HandlerLogin(Ignore = false)]
        public ActionResult Details()
        {
            return View();
        }

        #endregion


        #region （编辑）数据
        /// <summary>
        /// 修改数据提交
        /// </summary>
        /// <param name="model">主表信息</param>
        /// <param name="itemModel">明细信息</param>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public ActionResult SubmitForm(string model, string detail, string detail2, string type)
        {

            try
            {
                Object data = new Object();
                var supplyList = JsonEx.JsonToObj<TbSupplyList>(model);
                if (type == "Detail")//点击的供货按钮
                {
                    var supplyLDetail = JsonEx.JsonToObj<List<TbSupplyListDetail>>(detail);
                    var supplyLDetail2 = JsonEx.JsonToObj<List<TbSupplyListDetailHistory>>(detail2);
                    data = _suplistLogic.Update(supplyList, supplyLDetail, supplyLDetail2);
                }
                else
                {   //点击的供货完成
                    var suplistLogic = PM.DataAccess.DbContext.Db.Context.From<TbSupplyListDetail>()
                                          .Select(TbSupplyListDetail._.All)
                                          .Where(p => p.BatchPlanNum == supplyList.BatchPlanNum).ToList();
                    if (suplistLogic.Count > 0)
                    {
                        for (int i = 0; i < suplistLogic.Count; i++)
                        {
                            suplistLogic[i].HasSupplier = suplistLogic[i].BatchPlanQuantity;
                            //suplistLogic[i].ThisTimeCount = suplistLogic[i].BatchPlanQuantity;
                        }
                    }
                    var suplistLogich = PM.DataAccess.DbContext.Db.Context.From<TbSupplyListDetailHistory>()
                        .Select(TbSupplyListDetailHistory._.All)
                        .Where(p => p.BatchPlanNum == supplyList.BatchPlanNum).ToList();
                    var modeld = MapperHelper.Map<TbSupplyListDetail, TbSupplyListDetail>(suplistLogic);
                    var modeldh = MapperHelper.Map<TbSupplyListDetailHistory, TbSupplyListDetailHistory>(suplistLogich);
                    data = _suplistLogic.Update(supplyList, modeld, modeldh);
                }
                return Content(data.ToJson());
            }
            catch (Exception ex)
            {
                return Content(ex.ToString()); ;
            }

        }

        #endregion


        #region 查询页面 --初始页面,根据ID查询
        public ActionResult GetAllOrBySearch(TSupplyListRequest entity)
        {
            var data = _suplistLogic.GetAllOrBySearch(entity);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 以ID查询批次需求
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>

        [HandlerLogin(Ignore = false)]
        public ActionResult GetFormJson(int keyValue)
        {
            var data = _suplistLogic.GetFormJson(keyValue);
            return Content(data.ToJson());
        }
        #endregion

        #region 信息验证

        /// <summary>
        /// 判断信息是否可操作
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult AnyInfo(int keyValue)
        {
            var data = _suplistLogic.AnyInfo(keyValue);
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

            //批次计划编号、状态、分部名称、工区名称、供货日期、批次计划总量、已供应总量、不合格数量、验收人、联系电话、加工厂名称、交货地点
            //BatchPlanNum,StateCode,BranchName,WorkAreaName,SupplyDate,BatchPlanTotal,HasSupplierTotal,UnqualifiedTotal,AcceptorName,ContactWay,ProcessFactoryName,DeliveryPlace
            //导出数据列
            Dictionary<string, string> cellheader = new Dictionary<string, string> {
                    { "BatchPlanNum", "批次计划编号" },
                    { "StateCode", "状态" },
                    { "BranchName", "分部名称" },
                    { "WorkAreaName", "工区名称" },
                    { "SupplyDate", "供货日期" },
                    { "BatchPlanTotal", "批次计划总量(kg)" },
                    { "HasSupplierTotal", "已供应总量(kg)" },
                    { "UnqualifiedTotal", "不合格数量(kg)" },
                    { "AcceptorName", "验收人" },
                    { "ContactWay", "联系电话" },
                    { "ProcessFactoryName", "加工厂名称" },
                    { "DeliveryPlace", "交货地点" },
                };
            var request = JsonEx.JsonToObj<TSupplyListRequest>(jsonData);
            var data = _suplistLogic.GetExportList(request);
            decimal pcjhzlhj = 0;
            decimal ygyzlhj = 0;
            decimal bhgslhj = 0;
            if (data.Rows.Count > 0)
            {
                pcjhzlhj = Convert.ToDecimal(data.Compute("sum(BatchPlanTotal)", "true"));
                ygyzlhj = Convert.ToDecimal(data.Compute("sum(HasSupplierTotal)", "true"));
                bhgslhj = Convert.ToDecimal(data.Compute("sum(UnqualifiedTotal)", "true"));
            }
            string hzzfc = "批次计划总量合计(KG):" + pcjhzlhj + ",已供应总量合计(KG):" + ygyzlhj + "、不合格数量合计(KG):" + bhgslhj;
            var fileStream = ExcelHelper.ExportToMemoryStream(cellheader, data, "", "", "供应清单", hzzfc);
            return File(fileStream, "application/vnd.ms-excel", "供应清单.xls");
        }

        #endregion

    }

}
