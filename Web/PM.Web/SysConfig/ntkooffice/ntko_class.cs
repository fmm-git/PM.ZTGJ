using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

namespace WebApplication1
{
    public class ntko_class
    {
        /// <summary>
        /// 数据库联接字符串
        /// </summary>
        public SqlConnection connstr; //连接字符串 

        /// <summary>
        ///定义示例程序文档保存为其它格式文档路径
        /// </summary>
        public string demotitle, officepath, htmlpath, pdfpath, attachpath;

        /// <summary>
        /// 获取数据库联接字符串
        /// </summary>
        public string getconnstr()
        {
            string constr;
            constr = System.Configuration.ConfigurationManager.AppSettings["ntko_dbconn"];
            return constr;
        }
        public void open() //打开数据库
        {
            string constr;
            constr = getconnstr();
            connstr = new SqlConnection(constr);
            connstr.Open();
        }
        public void close() //关闭数据库
        {
            connstr.Dispose();
            connstr.Close();
        }

        /// <summary>
        /// 执行SQL语句
        /// </summary>
        /// <param name="sql">SQL语句</param>
        public void execsql(string sql)
        {
            open();
            SqlCommand cmd = new SqlCommand(sql, connstr);
            cmd.ExecuteNonQuery();
            close();
        }

        /// <summary>
        /// 返回DataSet对象
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public DataSet dataset(string sql)
        {
            open();
            SqlDataAdapter rs = new SqlDataAdapter(sql, connstr);
            DataSet ds = new DataSet();
            rs.Fill(ds);
            return ds;
        }

        /// <summary>
        /// 返回DataView对象
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public DataView dataview(string sql)
        {
            DataSet ds = new DataSet();
            ds = dataset(sql);
            DataView dv = new DataView(ds.Tables[0]);
            return dv;
        }

        /// <summary>
        /// 返回DataReader对象
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public SqlDataReader datareader(string sql)
        {
            open();
            SqlCommand cmd = new SqlCommand(sql, connstr);
            SqlDataReader dr = cmd.ExecuteReader();
            return dr;
        }
        /// <summary>
        /// 获取示例程序标题
        /// </summary>
        /// <returns></returns>
        public string getdemotitle()
        {
            demotitle = System.Configuration.ConfigurationManager.AppSettings["ntko_demotitle"];
            return demotitle;
        }
        public string getofficepath()//获取保存文件路径
        {
            officepath = System.Configuration.ConfigurationManager.AppSettings["officePath"];
            return officepath;
        }
        public string gethtmlpath()//获取html文件路径
        {
            htmlpath = System.Configuration.ConfigurationManager.AppSettings["htmlpath"];
            return htmlpath;
        }
        public string getpdfpath()//获取pdf文件路径
        {
            pdfpath = System.Configuration.ConfigurationManager.AppSettings["pdfpath"];
            return pdfpath;
        }
        public string getattachpath()//获取pdf文件路径
        {
            attachpath = System.Configuration.ConfigurationManager.AppSettings["attachpath"];
            return attachpath;
        }
    }
}
