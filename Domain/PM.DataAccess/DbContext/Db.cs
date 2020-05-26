using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dos.Common;
using Dos.ORM;

namespace PM.DataAccess.DbContext
{
    public class Db
    {
        public static readonly DbSession Context = new DbSession("SQLDBCnnString");
        public static readonly DbSession ContextGJ = new DbSession("SQLDBCnnStringGJ");//设备管理
        static Db()
        {
            Context.RegisterSqlLogger(delegate (string sql)
            {
                //在此可以记录sql日志
                //写日志会影响性能，建议开发版本记录sql以便调试，发布正式版本不要记录
                //LogHelper.Debug(sql, "SQL日志");
                LogHelper.Error(sql, "操作日志");
            });
        }
    }
}
