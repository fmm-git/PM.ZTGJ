using PM.Business.System;
using PM.Common;
using PM.Common.Extension;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace PM.Web.Areas.SystemManage.Controllers
{
    public class ShortMessageTemplateController : BaseController
    {
        private readonly ShortMessageTemplateLogic _smtLogic = new ShortMessageTemplateLogic();
        //
        // GET: /SystemManage/ShortMessageTemplate/

        /// <summary>
        /// 首页
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// 编辑页
        /// </summary>
        /// <returns></returns>
        public ActionResult Form()
        {
            return View();
        }
        /// <summary>
        /// 查看界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Details()
        {
            return View();
        }

        /// <summary>
        /// 查询全部绑定短信模板数据
        /// </summary>
        /// <param name="ent"></param>
        /// <returns></returns>
        public ActionResult GetAllTemplate(FPiCiXQPlan ent)
        {
            var data = _smtLogic.GetAllTemplate(ent);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 模板弹窗
        /// </summary>
        /// <param name="ent">分页</param>
        /// <returns></returns>
        public ActionResult selectTemplate(FPiCiXQPlan ent, string keyword)
        {
            var data = _smtLogic.selectTemplate(ent, keyword);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 判断信息是否可操作
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult AnyInfo(string keyValue)
        {
            var data = _smtLogic.AnyInfo(keyValue);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 编辑/查看页获取数据
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult GetFormJson(int keyValue)
        {
            var data = _smtLogic.FindEntity(keyValue);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 新增/编辑【绑定】
        /// </summary>
        /// <param name="type"></param>
        /// <param name="MCode"></param>
        /// <param name="TCode"></param>
        /// <returns></returns>
        public ActionResult SubmitForm(string model, string type)
        {
            //try
            //{
            //    var data = _smtLogic.SubmitForm(MCode, TCode);
            //    return Content(data.ToJson());
            //}
            //catch (Exception)
            //{
            //    throw;
            //}
            try
            {
                var megModel = JsonEx.JsonToObj<TbShortMessageTemplate>(model);

                if (type == "add")
                {
                    var data = _smtLogic.Insert(megModel);
                    return Content(data.ToJson());
                }
                else
                {
                    var data = _smtLogic.Update(megModel);
                    return Content(data.ToJson());
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
