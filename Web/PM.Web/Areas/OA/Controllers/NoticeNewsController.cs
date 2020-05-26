using PM.Business.OA;
using PM.Common.Extension;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace PM.Web.Areas.OA.Controllers
{
    public class NoticeNewsController : Controller
    {
        //
        // 通知类消息设置
        private readonly NoticeNewsLogic noticeNewsLogic = new NoticeNewsLogic();
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// 新增、修改
        /// </summary>
        /// <returns></returns>
        public ActionResult Form()
        {
            return View();
        }

        #region 推送信息
        public ActionResult GetFormJson(int keyValue)
        {
            var Data = noticeNewsLogic.FindEntity(keyValue);
            return Content(Data.ToJson());
        }
        public ActionResult GetNoticeNewsList(string MenuCode = "")
        {
            var Data = noticeNewsLogic.GetNoticeNewsList(MenuCode);
            return Content(Data.ToJson());
        }
        public ActionResult GetNoticeNewsList2(string OrgType = "", string DeptId = "", string RoleId = "", string PersonnelSource = "", string PersonnelCode = "", string ProjectId = "", int NewsType=1)
        {
            var Data = noticeNewsLogic.GetNoticeNewsList2(OrgType, DeptId, RoleId, PersonnelSource, PersonnelCode, ProjectId, NewsType);
            return Content(Data.ToJson());
        }

        #endregion

        #region 修改状态
        public ActionResult SubmitForm(string model)
        {
            try
            {
                var noticemodel = JsonEx.JsonToObj<TbNoticeNewsSetUp>(model);
                var data = noticeNewsLogic.Update(noticemodel);
                return Content(data.ToJson());

            }
            catch (Exception)
            {
                throw;
            }
        }

        //public ActionResult submitForm1(string postData)
        //{
        //    int IndexofA = postData.IndexOf("[");
        //    int IndexofB = postData.IndexOf("]");
        //    string Ru = postData.Substring(IndexofA, IndexofB - IndexofA + 1);
        //    JavaScriptSerializer Serializer = new JavaScriptSerializer();
        //    List<TbNoticeNewsSetUp> objs = Serializer.Deserialize<List<TbNoticeNewsSetUp>>(Ru);
        //    var data = noticeNewsLogic.UpdateNew(objs);
        //    return Content(data.ToJson());
        //}
        public ActionResult submitForm2(string postData, string OrgType = "", string DeptId = "", string RoleId = "", string PersonnelSource = "", string PersonnelCode = "", string ProjectId = "", int NewsType=1)
        {
            int IndexofA = postData.IndexOf("[");
            int IndexofB = postData.IndexOf("]");
            string Ru = postData.Substring(IndexofA, IndexofB - IndexofA + 1);
            JavaScriptSerializer Serializer = new JavaScriptSerializer();
            List<TbNoticeNewsOrg> objs = Serializer.Deserialize<List<TbNoticeNewsOrg>>(Ru);
            var data = noticeNewsLogic.InsertNew(objs, OrgType, DeptId, RoleId, PersonnelSource, PersonnelCode, ProjectId, NewsType);
            return Content(data.ToJson());
        }
        #endregion
    }
}
