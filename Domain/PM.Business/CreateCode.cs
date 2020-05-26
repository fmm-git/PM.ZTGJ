using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Domain.WebBase
{
    public static class CreateCode
   {
       private static string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["SQLDBCnnString"].ConnectionString;//数据库连接字符串
       public static string GetTableMaxCode(string FileCodeGz, string fileCode, string TableName)
        {
            string NextCode = "";
            string sql = "select MAX(" + fileCode + ") as Code from " + TableName + " where " + fileCode + " like '" + FileCodeGz + "%'";
            SqlConnection conn = new SqlConnection(connStr);
            SqlDataAdapter adapter = new SqlDataAdapter(sql, conn);
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            if (dt != null && dt.Rows.Count > 0)
            {
                //NextCode = GetNo(FileCodeGz, dt.Rows[0][0].ToString());
                NextCode = GetNoNew(FileCodeGz, dt.Rows[0][0].ToString());
            }
            return NextCode;

        }

       private static string GetNo(string FileCodeGz, string MaxCode)
        {
            string no = "";
            DateTime current = DateTime.Now;
            //这里换成你从数据库读取的编号
            string lastNo = MaxCode;
            if (!string.IsNullOrEmpty(lastNo))
            {
                var length = FileCodeGz.Length;
                string year = lastNo.Substring(length, 4);
                string number = lastNo.Substring(length + 4);
                //string dateStr = lastNo.Replace(FileCodeGz, "");
                //string year = dateStr.Substring(0, 4);
                //string number = dateStr.Substring(length + 4, 5);
                int intNum = 0;
                string strNum = "";
                //同年
                if (year == current.ToString("yyyy"))
                {
                    intNum = Int32.Parse(number) + 1;
                    strNum = intNum.ToString();
                    if (strNum.Length == 1)
                    {
                        strNum = "0000" + strNum;
                    }
                    else if (strNum.Length == 2)
                    {
                        strNum = "000" + strNum;
                    }
                    else if (strNum.Length == 3)
                    {
                        strNum = "00" + strNum;
                    }
                    else if (strNum.Length == 4)
                    {
                        strNum = "0" + strNum;
                    }
                    no = FileCodeGz + year + strNum;

                }
                else
                {
                    no = FileCodeGz + current.ToString("yyyy") + "00001";
                }
            }
            else
            {
                no = FileCodeGz + current.ToString("yyyy") + "00001";
            }

            return no;

        }

       private static string GetNoNew(string FileCodeGz, string MaxCode)
       {
           //获取当前序列号
           var currentNumber = MaxCode;

           if (string.IsNullOrEmpty(currentNumber))//为空，根据当前年月生成一个
           {

               var yearMonth = DateTime.Now.Date.ToString("yyyyMM");

               var number = FileCodeGz + yearMonth + "0001";
               return number;

           }

           //不为空的话截取年月部分与当前年月比较
           var length = FileCodeGz.Length;
           var yearMonthPart = currentNumber.Substring(length, 6);

           var dtNow = DateTime.Now.Date.ToString("yyyyMM");

           if (!yearMonthPart.Equals(dtNow))//如果年月不相同，重新生成
           {

               var number = FileCodeGz + dtNow + "0001";
               return number;

           }
           else
           {
               //若年月相同，则根据后面四位序列号+1生成新的序列号

               var num = currentNumber.Substring(length + 6, currentNumber.Length - (length+6));

               var nextNum = GetIndex(num);

               var number = FileCodeGz + yearMonthPart + nextNum;
               return number;
           }

       }

       private static string GetIndex(string num)
       {

           var nextNum = "";

           for (int i = 0; i < num.Length; i++)
           {

               if (num[i] != '0')
               {

                   var number = num.Substring(i, num.Length - i);

                   nextNum = (int.Parse(number) + 1).ToString();

                   var zeroLength = num.Length - nextNum.Length;

                   for (int j = 0; j < zeroLength; j++)
                   {

                       nextNum = "0" + nextNum;

                   }

                   return nextNum;

               }

           }

           return nextNum;

       }
    }
}
