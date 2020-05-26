using PM.Common;
using PM.Common.DataCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using PM.Domain;
using PM.DataEntity;
using PM.Business;
using PM.DataAccess.DbContext;
using PM.DataAccess;

namespace PM.Web.Controllers
{
    public class LoginController : BaseController
    {
        //
        // GET: /Login/

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UserLogin(string userName, string pwd)
        {
            if ((!string.IsNullOrEmpty(userName)) && (!string.IsNullOrEmpty(pwd)))
            {
                //var password = PM.Common.Encryption.EncryptionFactory.EncryptDES(pwd, "QWERTYUIOP");
                var password = PM.Common.Encryption.EncryptionFactory.Md5Encrypt(pwd);
                var user = Db.Context.From<TbUser>().Where(d => d.UserCode == userName && d.UserPwd == password).First();
                if (user != null)
                {
                    //查询是否是离职人员
                    var data = new UserLogic().UserClosedSelect(userName);
                    if (data == "-1")
                    {
                        return JsonMsg(false, "您为离职人员，无法进行登陆");
                    }
                    else
                    {
                        //查找角色信息
                        var operatorModel = new TbUserRoleLogic().FindUserInfo(user.UserCode);
                        operatorModel.LoginTime = DateTime.Now;
                        operatorModel.LoginToken = DESEncrypt.Encrypt(Guid.NewGuid().ToString());
                        if (user.UserName == "100000")
                        {
                            operatorModel.IsSystem = true;
                        }
                        else
                        {
                            operatorModel.IsSystem = false;
                        }
                        //判断是否是司机
                        var isAny = Repository<TbCarInfoDetail>.Any(p => p.UserCode == user.UserCode);
                        if (isAny)
                            operatorModel.IsDriver = true;
                        OperatorProvider.Provider.AddCurrent(operatorModel);
                        Session["username"] = user.UserName;
                        Session["usercode"] = user.UserCode;
                        Session["userid"] = user.UserId;
                        SysLog.inputLog("0", "登录系统");
                        return JsonMsg(true, user);
                    }
                }
                else
                {
                    return JsonMsg(false, "账号或密码错误!");
                }
            }
            else
            {
                var err = "用户名或密码不能为空！";
                return JsonMsg(false, err);
            }
        }
        public ActionResult LogOut()
        {
            SysLog.inputLog("0", "退出系统");
            Session.Abandon();
            Session.Clear();
            OperatorProvider.Provider.RemoveCurrent();
            return View("Index");
        }
        public void inputLog(string code, string type)
        {
            TbSysLog tblog = new TbSysLog();
            tblog.ActionMenu = code;
            tblog.ActionType = type;
            tblog.HostName = Dns.GetHostName();
            tblog.LogDate = DateTime.Now;
            tblog.UserCode = Session["usercode"].ToString();
            IPHostEntry ipEntity = Dns.GetHostEntry(tblog.HostName);
            for (var i = 0; i < ipEntity.AddressList.Length; i++)
            {
                if (ipEntity.AddressList[i].AddressFamily.ToString().Equals("InterNetwork"))
                {
                    tblog.UserIP = ipEntity.AddressList[i].ToString();
                    break;
                }
            }
            tblog.UserName = Session["username"].ToString();
            Db.Context.Insert<TbSysLog>(tblog);
        }
    }
}
