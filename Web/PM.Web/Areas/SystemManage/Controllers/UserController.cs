using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using PM.Business;
using PM.DataEntity;

namespace PM.Web.Areas.SystemManage.Controllers
{
    public class UserController : BaseController
    {
        private UserLogic ui = new UserLogic();

        private readonly TbUserMenuLogic _userMenu = new TbUserMenuLogic();
        //
        // GET: /SystemManage/User/

        /// <summary>
        /// 用户首页
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 个人信息页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Info() 
        {
            var UserCode = base.UserCode;
            var data = _userMenu.GetUserJson(UserCode);
            ViewBag.UserInfo = data[0];
            var Position = _userMenu.GetPosition_UserCode(UserCode);
            ViewData["Position"] = Position;
            var Role = _userMenu.GetRole_UserCode(UserCode);
            ViewData["Role"] = Role;
            return View();
        }       

        /// <summary>
        /// 新增/编辑页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Form() 
        {
            ViewBag.UserName = base.UserName;
            ViewBag.UserCode = base.UserCode;
            return View();
        }

        /// <summary>
        /// 查看详细信息界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Details() 
        {
            return View();
        }

        /// <summary>
        /// 查询全部公司
        /// 左侧分类
        /// </summary>
        /// <param name="postData"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetAllCompany()
        {
            var data = ui.GetAllCompany().ToList();
            var treeList = new List<TreeGridModel>();
            foreach (TbCompany item in data)
            {
                TreeGridModel treeModel = new TreeGridModel();
                bool hasChildren = data.Count(t => t.ParentCompanyCode == item.CompanyCode) == 0 ? false : true;
                treeModel.id = item.CompanyCode;
                treeModel.text = item.CompanyFullName;
                treeModel.isLeaf = hasChildren;
                treeModel.parentId = item.ParentCompanyCode;
                treeModel.expanded = hasChildren;
                treeModel.entityJson = item.ToJson();
                treeList.Add(treeModel);
            }
            return Content(treeList.TreeGridJson());
        }

        /// <summary>
        /// 根据左侧公司Code查询员工信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetUserByCompanyCode(string ComCode) 
        {
            var data = ui.GetUserByCompanyCode(ComCode).ToList();
            return Content(data.ToJson());
        }

        /// <summary>
        /// 根据编码查询用户
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult GetAllOrBySearchUser(string keyValue)
        {
            var data = ui.GetAllOrBySearchUser(keyValue);
            return Content(data.ToJson());
        }
        public ActionResult GetGridJson(TbUserRequest request)
        {
            var data = ui.GetDataListForPage(request);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 获取该组织机构下的所有用户
        /// </summary>
        /// <returns></returns>
        public ActionResult GetCompanyUser(TbUserRequest request,string UserName,string CompanyCode, string DepartmentProjectId) 
        {
            var data = ui.GetCompanyUser(request, UserName, CompanyCode, DepartmentProjectId);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 获取在职用户
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult GetIncumbencyUser(string keyword)
        {
            var data = ui.GetIncumbencyUser(keyword);
            return Content(data.ToJson());
        }
        public ActionResult GetFormJson(string keyValue) 
        {
            var data = ui.GetFormJson(keyValue);
            return Content(data.ToJson());
        }

        /// <summary>
        /// select 绑定
        /// </summary>
        /// <param name="dataCode"></param>
        /// <returns></returns>
        public ActionResult GetSelect(string dataCode) 
        {
            var data = ui.GetList(dataCode).ToList();
            var List = new List<TreeSelectModel>();
            foreach (TbSysDictionaryData item in data)
            {
                TreeSelectModel Model = new TreeSelectModel();
                Model.id = item.DictionaryText;
                Model.text = item.DictionaryText;
                Model.parentId = "0";
                List.Add(Model);
            }
            return Content(List.TreeSelectJson());
        }
        /// <summary>
        /// 查询用户所有存在的岗位
        /// </summary>
        /// <param name="usercode"></param>
        /// <returns></returns>
        public ActionResult GetUserPosition(string usercode) 
        {
            var data = ui.GetUserPosition(usercode);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 查询用户所有权限
        /// </summary>
        /// <param name="usercode"></param>
        /// <returns></returns>
        public ActionResult GetUserRole(string usercode)
        {
            var data = ui.GetUserRole(usercode);
            return Content(data.ToJson());
        }

        public ActionResult SubmitForm(TbUser user, string type) 
        {
            if (type == "add")
            {
                var data = ui.Insert(user);
                return Content(data.ToJson());
            }
            else
            {
                var data = ui.Update(user);
                return Content(data.ToJson());
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            var data = ui.Delete(keyValue);
            return Content(data.ToJson());
        }

        #region 用户菜单按钮权限

        public ActionResult UserAuthority()
        {
            return View();
        }

        /// <summary>
        /// 获取所有的用户信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetUserList() 
        {
            var data = _userMenu.GetUserGridList();
            return Content(data.ToJson());
        }
        /// <summary>
        /// 保存角色菜单
        /// </summary>
        /// <param name="postData"></param>
        /// <param name="RoleCode"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult submitForm1(string postData, string UserCode)
        {
            int IndexofA = postData.IndexOf("[");
            int IndexofB = postData.IndexOf("]");
            string Ru = postData.Substring(IndexofA, IndexofB - IndexofA + 1);
            JavaScriptSerializer Serializer = new JavaScriptSerializer();
            List<TbUserMenu> objs = Serializer.Deserialize<List<TbUserMenu>>(Ru);
            var data = _userMenu.Insert(objs, UserCode);
            return Content(data.ToJson());
        }
        [HttpGet]
        public ActionResult GetGridUserMenuJson(string UserCode)
        {
            var data = _userMenu.GetList(UserCode);
            return Content(data.ToJson());
        }

        #endregion


        #region 个人信息修改页面
        public ActionResult UserForm(string UserCode) {
            ViewBag.UserCode = UserCode;
            return View();
        }
        #endregion

        #region 根据用户编码查询出对应的个人信息
        public ActionResult UserFormSelect(string UserCode)
        {
            var data = ui.UserFormSelectList(UserCode);
            return Content(data.ToJson());
        }
        #endregion

        #region 根据用户编码修改个人信息
        public ActionResult UserUpdateSubmitForm(TbUser user) 
        {
            var data = ui.UserUpdateSubmitForm(user);
            return Content(data.ToJson());
        }
        #endregion
    }
}
