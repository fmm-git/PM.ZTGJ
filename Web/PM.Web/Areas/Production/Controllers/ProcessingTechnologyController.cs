using PM.Business.Production;
using PM.Common.Extension;
using PM.Common.Helper;
using PM.DataEntity;
using PM.DataEntity.Production;
using PM.DataEntity.Production.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.Production.Controllers
{
    public class ProcessingTechnologyController : BaseController
    {
        //
        // 加工工艺
        private readonly TbProcessingTechnologyLogic _proTeLogic = new TbProcessingTechnologyLogic();

        #region 列表
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 获取分页列表数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetGridJson(string keyword)
        {
            var data = _proTeLogic.GetList(keyword).ToList();
            var treeList = new List<TreeGridModel>();
            foreach (TbProcessingTechnology item in data)
            {
                TreeGridModel treeModel = new TreeGridModel();
                bool hasChildren = data.Count(t => t.PID == item.ID) == 0 ? false : true;
                treeModel.id = Convert.ToString(item.ID);
                treeModel.text = item.ProcessingTechnologyName;
                if (data.Count(t => t.ID == item.PID) == 0)
                {
                    item.PID = 0;
                }
                treeModel.isLeaf = hasChildren;
                treeModel.parentId = Convert.ToString(item.PID);
                treeModel.expanded = hasChildren;
                treeModel.entityJson = item.ToJson();
                treeList.Add(treeModel);
            }
            return Content(treeList.TreeGridJson());
        }

        public ActionResult GetPaent(string keyword)
        {
            var data = _proTeLogic.GetList(keyword).ToList();
            var treeList = new List<TreeSelectModel>();
            foreach (TbProcessingTechnology item in data)
            {
                TreeSelectModel treeModel = new TreeSelectModel();
                treeModel.id = Convert.ToString(item.ID);
                treeModel.text = item.ProcessingTechnologyName;
                treeModel.parentId = Convert.ToString(item.PID);
                treeList.Add(treeModel);
            }
            return Content(treeList.TreeSelectJson());
        }

        public ActionResult GetChild()
        {
            var data = _proTeLogic.GetChildList();
            return Content(data.ToJson());
        }
        #endregion

        #region 新增、修改、查看

        public ActionResult Form(string type)
        {
            return View();
        }

        [HandlerLogin(Ignore = false)]
        public ActionResult Details()
        {
            return View();
        }

        /// <summary>
        /// 编辑/查看页获取数据
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>

        [HandlerLogin(Ignore = false)]
        public ActionResult GetFormJson(int keyValue)
        {
            var data = _proTeLogic.FindEntity(keyValue);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 新增、修改数据提交
        /// </summary>
        /// <param name="model">主表信息</param>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public ActionResult SubmitForm(TbProcessingTechnology model, string type)
        {
            try
            {

                if (type == "add")
                {
                    var data = _proTeLogic.Insert(model);
                    return Content(data.ToJson());
                }
                else
                {
                    var data = _proTeLogic.Update(model);
                    return Content(data.ToJson());
                }
            }
            catch (Exception)
            {
                throw;
            }
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
            //加工工艺、加工工艺名称
            //PProcessingTechnologyName,ProcessingTechnologyName
            Dictionary<string, string> cellheader = new Dictionary<string, string> { 
                    { "PProcessingTechnologyName", "加工工艺" }, 
                    { "ProcessingTechnologyName", "加工工艺名称" }, 
                };
            var request = JsonEx.JsonToObj<ProcessingTechnologyRequest>(jsonData);
            var data = _proTeLogic.GetExportList(request);
            var fileStream = ExcelHelper.ExportToMemoryStream(cellheader, data, "", "", "加工工艺", "");
            return File(fileStream, "application/vnd.ms-excel", "加工工艺.xls");
        }
        #endregion

    }
}
