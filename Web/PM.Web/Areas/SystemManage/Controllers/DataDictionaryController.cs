using PM.Business;
using PM.DataEntity;
using PM.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.SystemManage.Controllers
{   
    /// <summary>
    /// 数据字典控制器
    /// </summary>
    /// <returns></returns>
    public class DataDictionaryController : BaseController
    {
        DataDictionaryLogic ddi = new DataDictionaryLogic();

        //
        // GET: /SystemManage/DataDictionary/

        /// <summary>
        /// 数据字典首页
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// 数据字典内容界面
        /// </summary>
        /// <returns></returns>
        public ActionResult FormData() 
        {
            return View();
        }
        /// <summary>
        /// 数据字典分类页面
        /// </summary>
        /// <returns></returns>
        public ActionResult FormType()
        {
            return View();
        }
        /// <summary>
        /// 查询界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Details() 
        {
            return View();
        }

        /// <summary>
        /// 数据字典分类导航
        /// </summary>
        /// <returns></returns>
        public ActionResult GetDataDictionary(string dataCode)
        {
            var data = ddi.GetDataDictionary(dataCode).ToList();
            var treeList = new List<TreeGridModel>();
            foreach (TbSysDictionaryType item in data)
            {
                TreeGridModel treeModel = new TreeGridModel();
                bool hasChildren = data.Count(t => t.PDictionaryCode == item.DictionaryCode) == 0 ? false : true;
                treeModel.id = item.DictionaryCode;
                treeModel.isLeaf = hasChildren;
                treeModel.parentId = item.PDictionaryCode;
                treeModel.expanded = false;
                treeModel.entityJson = item.ToJson();
                treeList.Add(treeModel);
            }
            return Content(treeList.TreeGridJson());
        }

        /// <summary>
        /// 数据字典select
        /// </summary>
        /// <returns></returns>
        public ActionResult GetSelectJson(string dataCode)
        {
            var data = ddi.GetDataDictionary(dataCode).ToList();
            var List = new List<TreeSelectModel>();
            foreach (TbSysDictionaryType item in data)
            {
                TreeSelectModel Model = new TreeSelectModel();
                Model.id = item.DictionaryCode;
                Model.text = item.DictionaryType;
                Model.parentId = item.PDictionaryCode;
                List.Add(Model);
            }
            return Content(List.TreeSelectJson());
        }

        /// <summary>
        /// 根据编码进行查询信息
        /// </summary>
        /// <param name="dicCode"></param>
        /// <returns></returns>
        public ActionResult GetDicByCode(string dicCode)
        {
            var data = ddi.GetDicByCode(dicCode).ToList();
            return Content(data.ToJson());
        }

        /// <summary>
        /// 根据编辑编码进行查询信息
        /// </summary>
        /// <param name="dicCode"></param>
        /// <returns></returns>
        public ActionResult GetDicJsonByCode(string dicCode, string CodeType, string CodeText)
        {
            var data = ddi.GetDicJsonByCode(dicCode, CodeType, CodeText);
            return Content(data.ToJson());
        }

        /// <summary>
        ///提交数据字典类型
        /// </summary>
        /// <param name="dicType"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public ActionResult SubmitFormType(TbSysDictionaryType dicType,string type) 
        {
            if (type == "add")
            {
                var data = ddi.InsertType(dicType);
                return Content(data.ToJson());
            }
            else
            {
                var data = ddi.UpdateType(dicType);
                return Content(data.ToJson());
            }
        }

        public ActionResult GetFormJsonType(string keyValue) 
        {
            var data = ddi.GetFormJsonType(keyValue);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 提交数据字典
        /// </summary>
        /// <param name="dicData"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public ActionResult SubmitFormData(TbSysDictionaryData dicData, string type)
        {
            if (type == "add")
            {
                var data = ddi.InsertData(dicData);
                return Content(data.ToJson());
            }
            else
            {
                var data = ddi.UpdateData(dicData);
                return Content(data.ToJson());
            }
        }

        /// <summary>
        /// 删除字典类型信息
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteFormType(string keyValue)
        {
            var result = ddi.DeleteType(keyValue);
            if (result)
            {
                return Success("删除成功!");
            }
            else
            {
                return Error("操作失败!");
            }
        }

        /// <summary>
        /// 删除字典信息
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteFormData(string keyValue)
        {
            var result = ddi.DeleteData(keyValue);
            if (result)
            {
                return Success("删除成功!");
            }
            else
            {
                return Error("操作失败!");
            }
        }

        /// <summary>
        /// 判断数据字典是否有下级
        /// </summary>
        /// <returns></returns>
        public ActionResult IsTrue(string keyValue) 
        {
            var data = ddi.IsTrue(keyValue);
            return Error(data.ToString());
        }
    }
}
