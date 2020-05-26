using PM.Business;
using PM.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Controllers
{
    /// <summary>
    /// 公用页面 （导入 等）
    /// </summary>
    public class CommonController : BaseController
    {
        public ActionResult Index()
        {
            return Content("");
        }

        #region 导入
        
        /// <summary>
        /// 导入页面
        /// </summary>
        /// <param name="submitUrl">数据导入地址</param>
        /// <param name="exclName">excl文件名称</param>
        /// <returns></returns>
        public ActionResult Input(string submitUrl, string exclName)
        {
            ViewBag.SubmitUrl = submitUrl;
            ViewBag.ExclName = exclName;
            return View();
        }

        #endregion

        #region 导入新

        /// <summary>
        /// 导入页面
        /// </summary>
        /// <param name="submitUrl">数据导入地址</param>
        /// <param name="exclName">excl文件名称</param>
        /// <returns></returns>
        public ActionResult InputNew(string submitUrl, string exclName)
        {
            ViewBag.SubmitUrl = submitUrl;
            ViewBag.ExclName = exclName;
            return View();
        }

        #endregion

        #region 弹框
        public virtual ActionResult SelectForm()
        {
            return View();
        }

        public ActionResult SelectFormNew()
        {
            ViewBag.CompanyId = OperatorProvider.Provider.CurrentUser.CompanyId;
            ViewBag.ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
            return View();
        }

        #endregion

        #region 加载页面按钮权限

        public string FunToolBarHtml = "";
        /// <summary>
        /// 加载页面权限
        /// <param name="IsCheck">是否有生成查看按钮</param>
        /// </summary>
        public string LoadPermissions(string strUri, bool IsCheck = true)
        {
            DataTable dsMenuPoints = new MenuPointsLogic().GetDataMenuPoints(base.UserCode,strUri);
            if (IsHasRow(dsMenuPoints))//是否有权限数据
            {
                if (IsCanRead(dsMenuPoints))//是否可读
                {
                    CreateFunToolbarHtml(dsMenuPoints, IsCheck);//运行创建功能栏Html的JS函数
                    return FunToolBarHtml;
                }
                else
                {
                    Error("很抱歉！您的权限不足，访问被拒绝");
                    return "";
                }
            }
            else
            {
                Error("很抱歉！您的权限不足，访问被拒绝");
                return "";
            }
        }
        /// <summary>
        /// 是否有权限数据
        /// </summary>
        /// <param name="dsMenuPoints"></param>
        /// <returns></returns>
        protected bool IsHasRow(DataTable dsMenuPoints)
        {
            return dsMenuPoints != null && dsMenuPoints.Rows.Count > 0;
        }

        /// <summary>
        /// 是否有读取权限
        /// </summary>
        /// <param name="dsMenuPoints"></param>
        /// <returns></returns>
        protected bool IsCanRead(DataTable dsMenuPoints)
        {
            return dsMenuPoints.Rows[0]["OperationView"].ToString() == "2";
        }

        /// <summary>
        /// 创建功能栏Html并赋值到全局变量
        /// </summary>
        /// <param name="dsMenuPoints"></param>
        protected void CreateFunToolbarHtml(DataTable dsMenuPoints, bool IsCheck = true)
        {
            DataRow row = dsMenuPoints.Rows[0];
            string funHtml = string.Empty;
            //默认
            if (IsCheck)
            {
                //查询
                funHtml += CreateFunBtnHtml(row["OperationView"].ToString(), "NF-Details", "fa fa-search-plus", "btn_details", "查看");
            }
            funHtml += CreateFunBtnHtml(row["OperationAdd"].ToString(), "NF-add", "fa fa-plus", "btn_add", "添加");
            funHtml += CreateFunBtnHtml(row["OperationEdit"].ToString(), "NF-edit", "fa fa-pencil-square-o", "btn_edit", "修改");
            funHtml += CreateFunBtnHtml(row["OperationDel"].ToString(), "NF-delete", "fa fa-trash-o", "btn_delete", "删除");
            funHtml += CreateFunBtnHtml(row["OperationOutput"].ToString(), "NF_output", "fa fa-mail-reply", "btn_output", "导出");
            funHtml += CreateFunBtnHtml(row["OperationExamination"].ToString(), "NF_examination", "fa fa-cc-mastercard", "btn_examination", "审批流程");
            //自定义
            funHtml += CreateFunBtnHtml(row["OperationOther1"].ToString(), "NF_other1", row["OperationOther1IconCls"], "btn_other1", row["OperationOther1Fun"]);
            funHtml += CreateFunBtnHtml(row["OperationOther2"].ToString(), "NF_other2", row["OperationOther2IconCls"], "btn_other2", row["OperationOther2Fun"]);
            funHtml += CreateFunBtnHtml(row["OperationOther3"].ToString(), "NF_other3", row["OperationOther3IconCls"], "btn_other3", row["OperationOther3Fun"]);
            funHtml += CreateFunBtnHtml(row["OperationOther4"].ToString(), "NF_other4", row["OperationOther4IconCls"], "btn_other4", row["OperationOther4Fun"]);
            funHtml += CreateFunBtnHtml(row["OperationOther5"].ToString(), "NF_other5", row["OperationOther5IconCls"], "btn_other5", row["OperationOther5Fun"]);

            FunToolBarHtml = funHtml;
        }

        //创建功能按钮Html
        protected string CreateFunBtnHtml(string permission, string id, object iconCls, string clickFunction, object text)
        {
            string html = string.Empty;
            if (permission == "2")
            {
                html = "<div class=\"btn-group\" style=\"margin-left: 5px\"><a id=\"" + id + "\" authorize=\"yes\" class=\"btn btn-primary dropdown-text\" onclick=\"" + clickFunction + "()\"><i class=\"" + iconCls + "\"></i>" + text.ToString() + "</a></div>";
            }
            return html;
        }

        #endregion
    }
}
