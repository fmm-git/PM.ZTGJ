using Dos.ORM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.DataAccess.DbContext
{
    public partial class Repository<T>
    {

        /// <summary>
        /// 分页查询(自定义sql查询)
        /// </summary>
        public static List<T> FromSql(string sql, List<Parameter> Parameter, string orderBy = "", string ascOrDesc = "asc", int? pageSize = null, int? pageIndex = null)
        {
            StringBuilder sqlStr = new StringBuilder();
            //组装where
            var length = 0;
            var param = new DbParameter[0];
            length = Parameter.Count();
            param = new DbParameter[length];
            string orderByString = "ID";
            for (int i = 0; i < length; i++)
            {
                param[i] = Db.Context.Db.DbProviderFactory.CreateParameter();
                param[i].DbType = Parameter[i].ParameterDbType.Value;
                param[i].ParameterName = Parameter[i].ParameterName;
                param[i].Value = Parameter[i].ParameterValue;
                orderByString = param[i].ParameterName;
            }
            if (!string.IsNullOrEmpty(orderBy))
            {
                orderByString = orderBy + " " + ascOrDesc;
            }
            sqlStr.Append("with temptbl as (SELECT ROW_NUMBER() OVER (order by " + orderByString + ")AS Row, * from (" + sql + ") as a) ");
            sqlStr.Append(" SELECT * FROM temptbl");

            if (pageIndex != null && pageSize != null)
            {
                var start = ((pageIndex.Value - 1) * pageSize.Value) + 1;
                var end = ((pageIndex.Value - 1) * pageSize.Value) + pageSize.Value;
                sqlStr.Append(" where Row between " + start + " and " + end);
            }
            var dataList = Db.Context.FromSql(sqlStr.ToString()).AddParameter(param).ToList<T>();
            return dataList;
        }
        
        /// <summary>
        /// 分页查询(自定义sql查询)
        /// </summary>
        public static PageModel FromSqlToPageTable(string sql, List<Parameter> Parameter, int pageSize, int pageIndex, string orderBy = "", string ascOrDesc = "asc")
        {
            StringBuilder sqlStr = new StringBuilder();
            //组装where
            var length = 0;
            var param = new DbParameter[0];
            var param2 = new DbParameter[0];
            length = Parameter.Count();
            param = new DbParameter[length];
            param2 = new DbParameter[length];
            string orderByString = "ID";
            for (int i = 0; i < length; i++)
            {
                param[i] = Db.Context.Db.DbProviderFactory.CreateParameter();
                param[i].DbType = Parameter[i].ParameterDbType.Value;
                param[i].ParameterName = Parameter[i].ParameterName;
                param[i].Value = Parameter[i].ParameterValue;
                orderByString = param[i].ParameterName;
                param2[i] = Db.Context.Db.DbProviderFactory.CreateParameter();
                param2[i].DbType = Parameter[i].ParameterDbType.Value;
                param2[i].ParameterName = Parameter[i].ParameterName;
                param2[i].Value = Parameter[i].ParameterValue;
            }
            var count =Db.Context.FromSql(sql).AddParameter(param).ToDataTable().Rows;
            if (!string.IsNullOrEmpty(orderBy))
            {
                orderByString = orderBy + " " + ascOrDesc;
            }
            sqlStr.Append("with temptbl as (SELECT ROW_NUMBER() OVER (order by " + orderByString + ")AS Row, * from (" + sql + ") as a) ");
            sqlStr.Append(" SELECT * FROM temptbl");
            var start = ((pageIndex - 1) * pageSize) + 1;
            var end = ((pageIndex - 1) * pageSize) + pageSize;
            sqlStr.Append(" where Row between " + start + " and " + end);
            var dataList = Db.Context.FromSql(sqlStr.ToString()).AddParameter(param2).ToDataTable();
            var pageMode = new PageModel(pageIndex, pageSize, count.Count, dataList);
            return pageMode;
        }

        /// <summary>
        /// DataTable
        /// </summary>
        public static DataTable FromSqlToDataTable(string sql, List<Parameter> Parameter, string orderBy = "", string ascOrDesc = "asc")
        {
            StringBuilder sqlStr = new StringBuilder();
            //组装where
            var length = 0;
            var param = new DbParameter[0];
            length = Parameter.Count();
            param = new DbParameter[length];
            string orderByString = "ID";
            for (int i = 0; i < length; i++)
            {
                param[i] = Db.Context.Db.DbProviderFactory.CreateParameter();
                param[i].DbType = Parameter[i].ParameterDbType.Value;
                param[i].ParameterName = Parameter[i].ParameterName;
                param[i].Value = Parameter[i].ParameterValue;
                orderByString = param[i].ParameterName;
            }
            if (!string.IsNullOrEmpty(orderBy))
            {
                orderByString = orderBy + " " + ascOrDesc;
            }
            sqlStr.Append("with temptbl as (SELECT ROW_NUMBER() OVER (order by " + orderByString + ")AS Row, * from (" + sql + ") as a) ");
            sqlStr.Append(" SELECT * FROM temptbl");
            var dataList = Db.Context.FromSql(sqlStr.ToString()).AddParameter(param).ToDataTable();
            return dataList;
        }
    }

    public partial class Repositorys<T>
    {
        /// <summary>
        /// 分页查询(自定义sql查询)
        /// </summary>
        public static PageModel FromSqlToPage(string sql, List<Parameter> Parameter, int pageSize, int pageIndex, string orderBy = "", string ascOrDesc = "asc")
        {
            StringBuilder sqlStr = new StringBuilder();
            //组装where
            var length = 0;
            var param = new DbParameter[0];
            var param2 = new DbParameter[0];
            length = Parameter.Count();
            param = new DbParameter[length];
            param2 = new DbParameter[length];
            string orderByString = "ID";
            for (int i = 0; i < length; i++)
            {
                param[i] = Db.Context.Db.DbProviderFactory.CreateParameter();
                param[i].DbType = Parameter[i].ParameterDbType.Value;
                param[i].ParameterName = Parameter[i].ParameterName;
                param[i].Value = Parameter[i].ParameterValue;
                orderByString = param[i].ParameterName;
                param2[i] = Db.Context.Db.DbProviderFactory.CreateParameter();
                param2[i].DbType = Parameter[i].ParameterDbType.Value;
                param2[i].ParameterName = Parameter[i].ParameterName;
                param2[i].Value = Parameter[i].ParameterValue;
            }
            var count = Db.Context.FromSql(sql).AddParameter(param).ToList<T>().Count;
            if (!string.IsNullOrEmpty(orderBy))
            {
                orderByString = orderBy + " " + ascOrDesc;
            }
            sqlStr.Append("with temptbl as (SELECT ROW_NUMBER() OVER (order by " + orderByString + ")AS Row, * from (" + sql + ") as a) ");
            sqlStr.Append(" SELECT * FROM temptbl");
            var start = ((pageIndex - 1) * pageSize) + 1;
            var end = ((pageIndex - 1) * pageSize) + pageSize;
            sqlStr.Append(" where Row between " + start + " and " + end);
            var dataList = Db.Context.FromSql(sqlStr.ToString()).AddParameter(param2).ToList<T>();
            var pageMode = new PageModel(pageIndex, pageSize, count, dataList);
            return pageMode;
        }

        /// <summary>
        /// 分页查询(自定义sql查询)
        /// </summary>
        public static List<T> FromSql(string sql, List<Parameter> Parameter, string orderBy = "", string ascOrDesc = "asc", int? pageSize = null, int? pageIndex = null)
        {
            StringBuilder sqlStr = new StringBuilder();
            //组装where
            var length = 0;
            var param = new DbParameter[0];
            length = Parameter.Count();
            param = new DbParameter[length];
            string orderByString = "ID";
            for (int i = 0; i < length; i++)
            {
                param[i] = Db.Context.Db.DbProviderFactory.CreateParameter();
                param[i].DbType = Parameter[i].ParameterDbType.Value;
                param[i].ParameterName = Parameter[i].ParameterName;
                param[i].Value = Parameter[i].ParameterValue;
                orderByString = param[i].ParameterName;
            }
            if (!string.IsNullOrEmpty(orderBy))
            {
                orderByString = orderBy + " " + ascOrDesc;
            }
            sqlStr.Append("with temptbl as (SELECT ROW_NUMBER() OVER (order by " + orderByString + ")AS Row, * from (" + sql + ") as a) ");
            sqlStr.Append(" SELECT * FROM temptbl");

            if (pageIndex != null && pageSize != null)
            {
                var start = ((pageIndex.Value - 1) * pageSize.Value) + 1;
                var end = ((pageIndex.Value - 1) * pageSize.Value) + pageSize.Value;
                sqlStr.Append(" where Row between " + start + " and " + end);
            }
            var dataList = Db.Context.FromSql(sqlStr.ToString()).AddParameter(param).ToList<T>();
            return dataList;
        }

        /// <summary>
        /// 自定义sql查询
        /// </summary>
        public static List<T> FromSql(string sql, List<Parameter> Parameter)
        {
            StringBuilder sqlStr = new StringBuilder();
            //组装where
            var length = 0;
            var param = new DbParameter[0];
            length = Parameter.Count();
            param = new DbParameter[length];
            for (int i = 0; i < length; i++)
            {
                param[i] = Db.Context.Db.DbProviderFactory.CreateParameter();
                param[i].DbType = Parameter[i].ParameterDbType.Value;
                param[i].ParameterName = Parameter[i].ParameterName;
                param[i].Value = Parameter[i].ParameterValue;
            }
            var dataList = Db.Context.FromSql(sql).AddParameter(param).ToList<T>();
            return dataList;
        }
    }
}
