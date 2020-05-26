using PM.Common.Helper;
using System.Configuration;
using PM.Common.Extension;
namespace PM.Common
{
    public class OperatorProvider
    {
        public static OperatorProvider Provider
        {
            get { return new OperatorProvider(); }
        }
        private string LoginUserKey = "nfine_loginuserkey_2018";
        private string LoginProvider = Configs.GetValue("LoginProvider");


        #region 当前用户
        /// <summary>
        /// 当前用户
        /// </summary>
        public CurrentUserInfo CurrentUser
        {
            get
            {
                CurrentUserInfo operatorModel = new CurrentUserInfo();
                if (Provider.LoginProvider == "Cookie")
                {
                    operatorModel = DESEncrypt.Decrypt(WebHelper.GetCookie(Provider.LoginUserKey).ToString()).JsonToObj<CurrentUserInfo>();
                }
                else
                {
                    var sessionStr=WebHelper.GetSession(Provider.LoginUserKey);
                    if (sessionStr != null)
                    {
                        operatorModel = DESEncrypt.Decrypt(sessionStr.ToString()).JsonToObj<CurrentUserInfo>();
                    }
                }
                return operatorModel;

                //缓存失效，从数据库查询
            }
        }
        #endregion

        public CurrentUserInfo GetCurrent()
        {
            CurrentUserInfo operatorModel = new CurrentUserInfo();
            if (LoginProvider == "Cookie")
            {
                operatorModel = DESEncrypt.Decrypt(WebHelper.GetCookie(LoginUserKey).ToString()).JsonToObj<CurrentUserInfo>();
            }
            else
            {
                operatorModel = DESEncrypt.Decrypt(WebHelper.GetSession(LoginUserKey).ToString()).JsonToObj<CurrentUserInfo>();
            }
            return operatorModel;
        }
        public void AddCurrent(CurrentUserInfo operatorModel)
        {
            if (LoginProvider == "Cookie")
            {
                WebHelper.WriteCookie(LoginUserKey, DESEncrypt.Encrypt(operatorModel.ToJson()), 90);
            }
            else
            {
                WebHelper.WriteSession(LoginUserKey, DESEncrypt.Encrypt(operatorModel.ToJson()));
            }
            WebHelper.WriteCookie("nfine_mac", Md5.md5(Net.GetMacByNetworkInterface().ToJson(), 90));
            WebHelper.WriteCookie("nfine_licence", Licence.GetLicence());
        }
        public void RemoveCurrent()
        {
            if (LoginProvider == "Cookie")
            {
                WebHelper.RemoveCookie(LoginUserKey.Trim());
            }
            else
            {
                WebHelper.RemoveSession(LoginUserKey.Trim());
            }
        }
    }
}
