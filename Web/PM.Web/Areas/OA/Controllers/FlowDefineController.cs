using Dos.Common;
using Dos.ORM;
using PM.Common;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PM.Business;

namespace PM.Web.Areas.OA.Controllers
{
    public class FlowDefineController : BaseController
    {
        FlowDefineLogic fdi = new FlowDefineLogic();

        #region 视图界面
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// 新增、修改页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Form(string type)
        {
            if (type == "add")
            {
                ViewBag.UserName = base.UserName;
                ViewBag.UserCode = base.UserCode;
                ViewBag.FlowNumber = fdi.GetFlowNumber();
            }
            return View();
        }
        /// <summary>
        /// 查看界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Detail()
        {
            return View();
        }
        /// <summary>
        /// 流程设计界面
        /// </summary>
        /// <returns></returns>
        public ActionResult FlowDesign()
        {
            return View();
        }
        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <returns></returns>
        public ActionResult ProcessDefinition()
        {
            return View();
        }
        #endregion

        #region 表单提交
        /// <summary>
        /// 新增、修改
        /// </summary>
        /// <param name="fd">实体</param>
        /// <param name="type">操作类型</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SubmitForm(TbFlowDefine fd, string type)
        {
            if (type == "add")
            {
                var data = fdi.Add(fd);
                return Content(data.ToJson());
            }
            else 
            {
                var data = fdi.Update(fd);
                return Content(data.ToJson());
            }
        }
        #endregion

        #region 查询
       
        /// <summary>
        /// 获取明细
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns> 
        [HttpGet]
        public ActionResult GetFlowDefineJson(string key)
        {
            var Data = fdi.GetFlowDefine(key);
            return Content(Data.ToJson());
        }
        /// <summary>
        /// 获取流程列表
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ActionResult GetList(string key)
        {
            var where = new Where<TbFlowDefine>();
            where.And(d=>d.FormCode==key);
            where.And(d => d.FlowType == "New");
            if (!string.IsNullOrWhiteSpace(OperatorProvider.Provider.CurrentUser.ProjectId))
            {
                where.And(d => d.ProjectId == OperatorProvider.Provider.CurrentUser.ProjectId);
            }
            var list = FlowDefineLogic.Query(where, new OrderByClip(new Field("FlowName")), "asc");
            return Content(list.ToJson());
        }

        /// <summary>
        /// 获取树形列表数据
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetTreeGridJson()
        {
            var data = new FlowDefineLogic().GetMenu();
            var treeList = new List<TreeGridModel>();
            foreach (TbSysMenu item in data)
            {
                TreeGridModel treeModel = new TreeGridModel();
                bool hasChildren = data.Count(t => t.MenuPCode == item.MenuCode) == 0 ? false : true;
                treeModel.id = item.MenuCode;
                treeModel.text = item.MenuName;
                if (data.Count(t => t.MenuCode == item.MenuPCode) == 0)
                {
                    item.MenuPCode = "0";
                }
                treeModel.isLeaf = hasChildren;
                treeModel.parentId = item.MenuPCode;
                treeModel.expanded = false;
                treeModel.entityJson = item.ToJson();
                treeList.Add(treeModel);
            }
            return Content(treeList.TreeGridJson());
        }

        /// <summary>
        /// 获取树形列表数据
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetTreeGridJsonNew()
        {
            var data = new FlowDefineLogic().GetMenuNew();
            var treeList = new List<TreeGridModel>();
            foreach (TbSysMenu item in data)
            {
                TreeGridModel treeModel = new TreeGridModel();
                bool hasChildren = data.Count(t => t.MenuPCode == item.MenuCode) == 0 ? false : true;
                treeModel.id = item.MenuCode;
                treeModel.text = item.MenuName;
                if (data.Count(t => t.MenuCode == item.MenuPCode) == 0)
                {
                    item.MenuPCode = "0";
                }
                treeModel.isLeaf = hasChildren;
                treeModel.parentId = item.MenuPCode;
                treeModel.expanded = false;
                treeModel.entityJson = item.ToJson();
                treeList.Add(treeModel);
            }
            return Content(treeList.TreeGridJson());
        }
        #endregion

        #region 删除
        public ActionResult Delete(string key)
        {
             int flag = FlowDefineLogic.Delete(d=>d.FlowCode==key);
             if (flag > 0)
                 return Content(AjaxResult.Success().ToJson());
             else return Content(AjaxResult.Error("操作失败").ToJson());
        }
        #endregion
    }
}
