using PM.DataAccess.DbContext;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using PM.Domain.WebBase;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace PM.Business
{
    /// <summary>
    /// 系统操作日志处理
    /// </summary>
    public class SysLogLogic
    {
        public List<TbSysLog> GetAllLog(PageSearchRequest pr, JObject queryParam)
        {
            string selectName = "";
            string selectDate = "";
            if (queryParam.Count!=0)
            {
                selectName = queryParam["keyword"].ToString();
                selectDate = queryParam["timeType"].ToString();
            }
            var time = DateTime.Now;
            var endTime = time.ToString("yyyy-MM-dd") + " 23:59:59";
            var startime = time.AddDays(-7).ToString("yyyy-MM-dd") + " 00:00:00";
            switch (selectDate)
            {
                case "1":
                    startime = time.ToString("yyyy-MM-dd") + " 00:00:00";
                    break;
                case "2":
                    startime = time.AddDays(-7).ToString("yyyy-MM-dd") + " 00:00:00";
                    break;
                case "3":
                    startime = time.AddMonths(-1).ToString("yyyy-MM-dd") + " 00:00:00";
                    break;
                case "4":
                    startime = time.AddMonths(-3).ToString("yyyy-MM-dd") + " 00:00:00";
                    break;
            }
            StringBuilder sb = new StringBuilder();
            sb.Append("select ");
            sb.Append("a.id,a.LogDate,a.UserIP,a.UserCode,b.UserName as UserName,");
            sb.Append("a.ActionMenu,a.HostName,c.MenuName as ActionMenuName,a.ActionType");
            sb.Append(" from TbSysLog a left join TbUser b on a.UserCode = b.UserCode left join TbTableMenu c on a.ActionMenu=c.TableName where 1=1");
            if (!string.IsNullOrEmpty(selectName))
            {
                sb.Append(" and (b.UserName like @name or c.MenuName like @name or a.ActionType like @name)");
            }
            sb.Append(" and (a.LogDate BETWEEN '" + startime + "' and '" + endTime + "')");
            var list = Db.Context.FromSql(sb.ToString()).AddInParameter("@name", DbType.String, "%" + selectName + "%").ToList<TbSysLog>();
            pr.records = list.Count();
            List<Dos.ORM.Parameter> para = new List<Dos.ORM.Parameter>();
            para.Add(new Dos.ORM.Parameter("@name", "%" + selectName + "%", DbType.String, null));
            var model = Repository<TbSysLog>.FromSql(sb.ToString(), para, "LogDate", "desc", pr.rows, pr.page).ToList();
            return model;
        }
    }
}
