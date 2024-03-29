﻿using System;
using System.Configuration;
using System.Web;

namespace PM.Common
{
    public sealed class Licence
    {
        public static bool IsLicence(string key)
        {
            string host = HttpContext.Current.Request.Url.Host.ToLower();
            if (host.Equals("localhost"))
                return true;
            string licence = ConfigurationManager.AppSettings["LicenceKey"];
            if (licence != null && licence == Md5.md5(key, 32))
                return true;

            return false;
        }
        public static string GetLicence()
        {
            var licence = ConfigurationManager.AppSettings["LicenceKey"]; ;
            if (string.IsNullOrEmpty(licence))
            {
                licence = Guid.NewGuid().ToString();
                Configs.SetValue("LicenceKey", licence);
            }
            return Md5.md5(licence, 32);
        }
    }
}
