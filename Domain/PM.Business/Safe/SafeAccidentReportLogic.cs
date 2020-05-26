using Dos.ORM;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Business.Safe
{
    /// <summary>
    /// 逻辑处理层
    /// 安全管理：安全周例会
    /// </summary>
    public class SafeAccidentReportLogic
    {
        #region 查询数据

        /// <summary>
        /// 查询本年所有信息
        /// </summary>
        /// <returns></returns>
        public DataTable GetAllWeekAccidentReport(PageSearchRequest psr)
        {
            DateTime now = DateTime.Now;
            string year = now.Year.ToString();
            //本年第一天
            DateTime FirstDay = Convert.ToDateTime(year + "-01-01" + " 00:00:00");
            //本年最后一天
            DateTime LastDay = Convert.ToDateTime(year + "-12-31" + " 23:59:59");
            var where = new Where<TbSafeAccidentReport>();
            where.And(d => d.StartTime >= FirstDay && d.StartTime <= LastDay);
            if (!string.IsNullOrWhiteSpace(psr.ProcessFactoryCode))
            {
                where.And(d => d.ProcessFactoryCode == psr.ProcessFactoryCode);
            }
            var ret = Db.Context.From<TbSafeAccidentReport>().Select(
                TbSafeAccidentReport._.All
                      , TbCompany._.CompanyFullName.As("ProcessFactoryName")
                      , TbUser._.UserName.As("InsertUserName"))
                    .LeftJoin<TbCompany>((a, c) => a.ProcessFactoryCode == c.CompanyCode)
                    .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode
                ).Where(where).ToDataTable();
            if (ret.Rows.Count > 0)
            {
                ret.Columns.Add("ParticipantsName", typeof(string));
                for (var j = 0; j < ret.Rows.Count; j++)
                {
                    var code = ret.Rows[j]["Participants"].ToString();
                    var user = Db.Context.From<TbUser>().Where("UserCode in ('" + code + "')").ToList();
                    StringBuilder sb = new StringBuilder();
                    for (var i = 0; i < user.Count; i++)
                    {
                        sb.Append(user[i].UserName + ",");
                    }
                    ret.Rows[j]["ParticipantsName"] = sb;
                    var data = ret.Rows[j]["MeetingContent"].ToString().Replace("&lt;", "<").Replace("&gt;", ">");
                    ret.Rows[j]["MeetingContent"] = data;
                }
            }
            else
            {
                return ret;
            }
            return ret;
        }

        /// <summary>
        /// 查询本年所有信息(app)
        /// </summary>
        /// <returns></returns>
        public DataTable GetAllWeekAccidentReport1(string ProcessFactoryCode)
        {
            DateTime now = DateTime.Now;
            string year = now.Year.ToString();

            //本年第一天
            DateTime FirstDay = Convert.ToDateTime(year + "-01-01" + " 00:00:00");
            //本年最后一天
            DateTime LastDay = Convert.ToDateTime(year + "-12-31" + " 23:59:59");
            var where = new Where<TbSafeAccidentReport>();
            where.And(d => d.StartTime >= FirstDay && d.StartTime <= LastDay);
            if (!string.IsNullOrWhiteSpace(ProcessFactoryCode))
            {
                where.And(d => d.ProcessFactoryCode == ProcessFactoryCode);
            }
            var ret = Db.Context.From<TbSafeAccidentReport>().Select(
                TbSafeAccidentReport._.All
                      , TbCompany._.CompanyFullName.As("ProcessFactoryName")
                      , TbUser._.UserName.As("InsertUserName"))
                    .LeftJoin<TbCompany>((a, c) => a.ProcessFactoryCode == c.CompanyCode)
                    .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode
                ).Where(where).ToDataTable();
            if (ret.Rows.Count > 0)
            {
                ret.Columns.Add("ParticipantsName", typeof(string));
                for (var j = 0; j < ret.Rows.Count; j++)
                {
                    var code = ret.Rows[j]["Participants"].ToString();
                    var user = Db.Context.From<TbUser>().Where("UserCode in ('" + code + "')").ToList();
                    StringBuilder sb = new StringBuilder();
                    for (var i = 0; i < user.Count; i++)
                    {
                        sb.Append(user[i].UserName + ",");
                    }
                    ret.Rows[j]["ParticipantsName"] = sb;
                    var data = ret.Rows[j]["MeetingContent"].ToString().Replace("&lt;", "<").Replace("&gt;", ">");
                    ret.Rows[j]["MeetingContent"] = data;
                }
            }
            else
            {
                return ret;
            }
            return ret;
        }

        /// <summary>
        /// 以本周查询周例会
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public DataTable GetWeek(PageSearchRequest psr, string sdate, string edate)
        {
            var KTime = DateTime.Parse(sdate + " 00:00:00");
            var JTime = DateTime.Parse(edate + " 23:59:59");
            var ret = Db.Context.From<TbSafeAccidentReport>()
              .Select(
                      TbSafeAccidentReport._.All
                      , TbCompany._.CompanyFullName.As("ProcessFactoryName")
                      , TbUser._.UserName.As("InsertUserName"))
                    .LeftJoin<TbCompany>((a, c) => a.ProcessFactoryCode == c.CompanyCode)
                    .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                    .Where(p => p.StartTime >= KTime && p.StartTime <= JTime && p.ProjectId == psr.ProjectId).ToDataTable();
            if (ret.Rows.Count == 0)
            {
                return ret;
            }
            ret.Columns.Add("ParticipantsName", typeof(string));
            var code = ret.Rows[0]["Participants"].ToString();
            var user = Db.Context.From<TbUser>().Where("UserCode in ('" + code + "')").ToList();
            StringBuilder sb = new StringBuilder();
            for (var i = 0; i < user.Count; i++)
            {
                sb.Append(user[i].UserName + ",");
            }
            ret.Rows[0]["ParticipantsName"] = sb;
            if (ret.Rows.Count > 0)
            {
                var data = ret.Rows[0]["MeetingContent"].ToString().Replace("&lt;", "<").Replace("&gt;", ">");
                ret.Rows[0]["MeetingContent"] = data;
            }
            return ret;
        }

        /// <summary>
        /// 以本周查询周例会(app)
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public DataTable GetWeek1(string psr, string sdate, string edate)
        {
            var KTime = DateTime.Parse(sdate + " 00:00:00");
            var JTime = DateTime.Parse(edate + " 23:59:59");
            var ret = Db.Context.From<TbSafeAccidentReport>()
              .Select(
                      TbSafeAccidentReport._.All
                      , TbCompany._.CompanyFullName.As("ProcessFactoryName")
                      , TbUser._.UserName.As("InsertUserName"))
                    .LeftJoin<TbCompany>((a, c) => a.ProcessFactoryCode == c.CompanyCode)
                    .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                    .Where(p => p.StartTime >= KTime && p.StartTime <= JTime && p.ProjectId == psr).ToDataTable();
            if (ret.Rows.Count == 0)
            {
                return ret;
            }
            ret.Columns.Add("ParticipantsName", typeof(string));
            var code = ret.Rows[0]["Participants"].ToString();
            var user = Db.Context.From<TbUser>().Where("UserCode in ('" + code + "')").ToList();
            StringBuilder sb = new StringBuilder();
            for (var i = 0; i < user.Count; i++)
            {
                sb.Append(user[i].UserName + ",");
            }
            ret.Rows[0]["ParticipantsName"] = sb;
            if (ret.Rows.Count > 0)
            {
                var data = ret.Rows[0]["MeetingContent"].ToString().Replace("&lt;", "<").Replace("&gt;", ">");
                ret.Rows[0]["MeetingContent"] = data;
            }
            return ret;
        }

        /// <summary>
        /// 以往周查询周例会
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public DataTable GetWWeek(PageSearchRequest psr, string sdate, string edate, int num)
        {
            var KTime = DateTime.Parse(sdate + " 00:00:00");
            var JTime = DateTime.Parse(edate + " 23:59:59");
            if (num == 0)
            {
                KTime = KTime.AddDays(-7);
                JTime = JTime.AddDays(-7);
            }
            var where = new Where<TbSafeAccidentReport>();
            where.And(d => d.StartTime >= KTime && d.StartTime <= JTime);
            if (!string.IsNullOrWhiteSpace(psr.ProcessFactoryCode))
            {
                where.And(d => d.ProcessFactoryCode == psr.ProcessFactoryCode);
            }
            var ret = Db.Context.From<TbSafeAccidentReport>()
              .Select(
                      TbSafeAccidentReport._.All
                      , TbCompany._.CompanyFullName.As("ProcessFactoryName")
                      , TbUser._.UserName.As("InsertUserName"))
                    .LeftJoin<TbCompany>((a, c) => a.ProcessFactoryCode == c.CompanyCode)
                    .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                    .Where(where).ToDataTable();
            if (ret.Rows.Count == 0)
            {
                return ret;
            }
            ret.Columns.Add("ParticipantsName", typeof(string));
            var code = ret.Rows[0]["Participants"].ToString();
            var user = Db.Context.From<TbUser>().Where("UserCode in ('" + code + "')").ToList();
            StringBuilder sb = new StringBuilder();
            for (var i = 0; i < user.Count; i++)
            {
                sb.Append(user[i].UserName + ",");
            }
            ret.Rows[0]["ParticipantsName"] = sb;
            if (ret.Rows.Count > 0)
            {
                var data = ret.Rows[0]["MeetingContent"].ToString().Replace("&lt;", "<").Replace("&gt;", ">");
                ret.Rows[0]["MeetingContent"] = data;
            }
            return ret;
        }

        /// <summary>
        /// 以往周查询周例会(app)
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public DataTable GetWWeek1(string psr, string sdate, string edate, int num)
        {
            var KTime = DateTime.Parse(sdate + " 00:00:00");
            var JTime = DateTime.Parse(edate + " 23:59:59");
            if (num == 0)
            {
                KTime = KTime.AddDays(-7);
                JTime = JTime.AddDays(-7);
            }
            var ret = Db.Context.From<TbSafeAccidentReport>()
              .Select(
                      TbSafeAccidentReport._.All
                      , TbCompany._.CompanyFullName.As("ProcessFactoryName")
                      , TbUser._.UserName.As("InsertUserName"))
                    .LeftJoin<TbCompany>((a, c) => a.ProcessFactoryCode == c.CompanyCode)
                    .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                    .Where(p => p.StartTime >= KTime && p.StartTime <= JTime && p.ProjectId == psr).ToDataTable();
            if (ret.Rows.Count == 0)
            {
                return ret;
            }
            ret.Columns.Add("ParticipantsName", typeof(string));
            var code = ret.Rows[0]["Participants"].ToString();
            var user = Db.Context.From<TbUser>().Where("UserCode in ('" + code + "')").ToList();
            StringBuilder sb = new StringBuilder();
            for (var i = 0; i < user.Count; i++)
            {
                sb.Append(user[i].UserName + ",");
            }
            ret.Rows[0]["ParticipantsName"] = sb;
            if (ret.Rows.Count > 0)
            {
                var data = ret.Rows[0]["MeetingContent"].ToString().Replace("&lt;", "<").Replace("&gt;", ">");
                ret.Rows[0]["MeetingContent"] = data;
            }
            return ret;
        }

        /// <summary>
        /// 以ID查询周例会
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public Tuple<DataTable> GetFormJson(int keyValue)
        {
            var ret = Db.Context.From<TbSafeAccidentReport>()
              .Select(
                      TbSafeAccidentReport._.All
                      , TbCompany._.CompanyFullName.As("ProcessFactoryName")
                      , TbUser._.UserName.As("InsertUserName"))
                    .LeftJoin<TbCompany>((a, c) => a.ProcessFactoryCode == c.CompanyCode)
                    .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                    .Where(p => p.ID == keyValue).ToDataTable();
            if (ret.Rows.Count == 0)
            {
                return new Tuple<DataTable>(ret);
            }
            ret.Columns.Add("ParticipantsName", typeof(string));
            var code = ret.Rows[0]["Participants"].ToString();
            var user = Db.Context.From<TbUser>().Where("UserCode in ('" + code + "')").ToList();
            StringBuilder sb = new StringBuilder();
            for (var i = 0; i < user.Count; i++)
            {
                sb.Append(user[i].UserName + ",");
            }
            ret.Rows[0]["ParticipantsName"] = sb;
            if (ret.Rows.Count > 0)
            {
                var data = ret.Rows[0]["MeetingContent"].ToString().Replace("&lt;", "<").Replace("&gt;", ">");
                ret.Rows[0]["MeetingContent"] = data;
            }
            return new Tuple<DataTable>(ret);
        }


        public DataTable Details(int keyValue, bool isApi = false)
        {
            var ret = Db.Context.From<TbSafeAccidentReport>()
              .Select(
                      TbSafeAccidentReport._.All
                      , TbCompany._.CompanyFullName.As("ProcessFactoryName")
                      , TbUser._.UserName.As("InsertUserName"))
                    .LeftJoin<TbCompany>((a, c) => a.ProcessFactoryCode == c.CompanyCode)
                    .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                    .Where(p => p.ID == keyValue).ToDataTable();
            if (ret.Rows.Count == 0)
            {
                return ret;
            }
            //修改浏览数
            var article = Repository<TbSafeAccidentReport>.First(p => p.ID == keyValue);
            article.BrowseCount += 1;
            Repository<TbSafeAccidentReport>.Update(article, p => p.ID == keyValue, isApi);

            ret.Columns.Add("ParticipantsName", typeof(string));
            var code = ret.Rows[0]["Participants"].ToString();
            var user = Db.Context.From<TbUser>().Where("UserCode in ('" + code + "')").ToList();
            StringBuilder sb = new StringBuilder();
            for (var i = 0; i < user.Count; i++)
            {
                sb.Append(user[i].UserName + ",");
            }
            ret.Rows[0]["ParticipantsName"] = sb;
            if (ret.Rows.Count > 0)
            {
                var data = ret.Rows[0]["MeetingContent"].ToString().Replace("&lt;", "<").Replace("&gt;", ">");
                ret.Rows[0]["MeetingContent"] = data;
            }
            return ret;
        }

        /// <summary>
        /// 查询用户所属加工厂
        /// </summary>
        /// <param name="userCode"></param>
        /// <returns></returns>
        public PageModel GetProcessFactory(PageSearchRequest psr, string type)
        {
            //参数化
            List<Parameter> parameter = new List<Parameter>();
            string where = " where 1=1 and cp.OrgType=@type";
            parameter.Add(new Parameter("@type", type, DbType.String, null));
            string sql = @"select cp.id,cp.CompanyCode,cp.CompanyFullName,cp.ParentCompanyCode,cp.Address,cp.OrgType from  TbCompany cp
left join TbProjectCompany pc on cp.CompanyCode=pc.CompanyCode ";
            var model = Repository<TbCompany>.FromSqlToPageTable(sql + where, parameter, psr.rows, psr.page, "ID", "asc");
            return model;
        }
        #endregion

        #region （新增、编辑）数据

        /// <summary>
        /// 新增数据
        /// </summary>
        public AjaxResult Insert(TbSafeAccidentReport model, bool isApi = false)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            if (isApi == false)
            {
                model.BrowseCount = 0;
                model.InsertUserCode = OperatorProvider.Provider.CurrentUser.UserCode;
            }
            model.ProjectId = "";//项目id不保存
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //添加信息
                    Repository<TbSafeAccidentReport>.Insert(trans, model, isApi);
                    trans.Commit();
                    return AjaxResult.Success();
                }
            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 修改数据
        /// </summary>
        public AjaxResult Update(TbSafeAccidentReport model, bool isApi = false)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            var anyRet = AnyInfo(model.ID);
            if (anyRet.state.ToString() != ResultType.success.ToString())
                return anyRet;
            model.ProjectId = "";//项目id不保存
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())//使用事务
                {
                    //修改信息
                    Repository<TbSafeAccidentReport>.Update(trans, model, p => p.ID == model.ID, isApi);
                    trans.Commit();//提交事务

                    return AjaxResult.Success();
                }
            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 判断信息是否可操作
        /// </summary>
        /// <returns></returns>
        public AjaxResult AnyInfo(int keyValue)
        {
            var monthPlan = Repository<TbSafeAccidentReport>.First(p => p.ID == keyValue);
            if (monthPlan == null)
                return AjaxResult.Warning("信息不存在");
            return AjaxResult.Success(monthPlan);
        }

        #endregion
    }
}
