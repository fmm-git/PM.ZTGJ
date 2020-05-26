using Dos.ORM;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
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
    /// 安全管理：班前讲话
    /// </summary>
    public class SafeDatumManageLogic
    {
        #region 查询数据

        /// <summary>
        /// 以ID查询讲话
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public Tuple<DataTable> GetFormJson(int keyValue)
        {
            var ret = Db.Context.From<TbSafeDatumManage>()
              .Select(
                      TbSafeDatumManage._.All
                      , TbCompany._.CompanyFullName.As("ProcessFactoryName")
                      , TbUser._.UserName.As("InsertUserName"))
                    .LeftJoin<TbCompany>((a, c) => a.ProcessFactoryCode == c.CompanyCode)
                    .AddSelect(Db.Context.From<TbUser>().Select(p => p.UserName)
                    .Where(TbUser._.UserCode == TbSafeDatumManage._.SpeechUser), "SpeechUserName")
                    .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                    .Where(p => p.ID == keyValue).ToDataTable();
            if (ret.Rows.Count > 0)
            {
                var data = ret.Rows[0]["SpeechContent"].ToString().Replace("&lt;", "<").Replace("&gt;", ">");
                ret.Rows[0]["SpeechContent"] = data;
            }
            return new Tuple<DataTable>(ret);
        }

        /// <summary>
        /// 查询页
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public DataTable Details(int keyValue, bool isApi = false)
        {
            var ret = Db.Context.From<TbSafeDatumManage>()
              .Select(
                      TbSafeDatumManage._.All
                      , TbCompany._.CompanyFullName.As("ProcessFactoryName")
                      , TbUser._.UserName.As("InsertUserName"))
                    .LeftJoin<TbCompany>((a, c) => a.ProcessFactoryCode == c.CompanyCode)
                    .AddSelect(Db.Context.From<TbUser>().Select(p => p.UserName)
                    .Where(TbUser._.UserCode == TbSafeDatumManage._.SpeechUser), "SpeechUserName")
                    .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                    .Where(p => p.ID == keyValue).ToDataTable();

            //修改浏览数
            var article = Repository<TbSafeDatumManage>.First(p => p.ID == keyValue);
            article.BrowseCount += 1;
            Repository<TbSafeDatumManage>.Update(article, p => p.ID == keyValue, isApi);

            if (ret.Rows.Count > 0)
            {
                var data = ret.Rows[0]["SpeechContent"].ToString().Replace("&lt;", "<").Replace("&gt;", ">");
                ret.Rows[0]["SpeechContent"] = data;
            }
            return ret;
        }

        /// <summary>
        /// 查询用户 拿到所属加工厂
        /// </summary>
        /// <param name="userCode"></param>
        /// <returns></returns>
        public DataTable GetUser(string userCode)
        {
            var ret = Db.Context.From<TbUser>().Select(
                TbUser._.All, TbCompany._.CompanyFullName.As("CompanyName")
                ).LeftJoin<TbCompany>((a, c) => a.CompanyCode == c.CompanyCode).Where(p => p.UserCode == userCode).ToDataTable();
            return ret;
        }

        /// <summary>
        /// 以当日查询班前讲话
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public DataTable GetToday(PageSearchRequest psr, string keyValue)
        {
            var KTime = DateTime.Parse(keyValue + " 00:00:00");
            var JTime = DateTime.Parse(keyValue + " 23:59:59");
            var where = new Where<TbSafeDatumManage>();
            where.And(d => d.StartTime >= KTime && d.StartTime <= JTime);
            //if (!string.IsNullOrWhiteSpace(psr.ProjectId))
            //{
            //    where.And(d => d.ProjectId == psr.ProjectId);
            //}
            if (!string.IsNullOrWhiteSpace(psr.ProcessFactoryCode))
            {
                where.And(d => d.ProcessFactoryCode == psr.ProcessFactoryCode);
            }
            var ret = Db.Context.From<TbSafeDatumManage>()
              .Select(
                      TbSafeDatumManage._.All
                      , TbCompany._.CompanyFullName.As("ProcessFactoryName")
                      , TbUser._.UserName.As("InsertUserName"))
                    .LeftJoin<TbCompany>((a, c) => a.ProcessFactoryCode == c.CompanyCode)
                    .AddSelect(Db.Context.From<TbUser>().Select(p => p.UserName)
                    .Where(TbUser._.UserCode == TbSafeDatumManage._.SpeechUser), "SpeechUserName")
                    .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                    .Where(where).ToDataTable();
            if (ret.Rows.Count > 0)
            {
                var data = ret.Rows[0]["SpeechContent"].ToString().Replace("&lt;", "<").Replace("&gt;", ">");
                ret.Rows[0]["SpeechContent"] = data;
            }
            return ret;
        }

        /// <summary>
        /// 查询本年本月所有信息
        /// </summary>
        /// <returns></returns>
        public DataTable GetAllDayDatum(PageSearchRequest psr,int month)
        {
            DateTime now = DateTime.Now;
            //本月第一天
            DateTime FirstDay = new DateTime(now.Year, month, 1);
            //本月最后一天
            DateTime LastDay = Convert.ToDateTime(FirstDay.AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd") + " 23:59:59");
            var where = new Where<TbSafeDatumManage>();
            where.And(d => d.StartTime >= FirstDay && d.StartTime <= LastDay);
            if (!string.IsNullOrWhiteSpace(psr.ProcessFactoryCode))
            {
                where.And(d => d.ProcessFactoryCode == psr.ProcessFactoryCode);
            }
            var ret = Db.Context.From<TbSafeDatumManage>().Select(
                      TbSafeDatumManage._.All
                      , TbCompany._.CompanyFullName.As("ProcessFactoryName")
                      , TbUser._.UserName.As("InsertUserName"))
                    .LeftJoin<TbCompany>((a, c) => a.ProcessFactoryCode == c.CompanyCode)
                    .AddSelect(Db.Context.From<TbUser>().Select(p => p.UserName)
                    .Where(TbUser._.UserCode == TbSafeDatumManage._.SpeechUser), "SpeechUserName")
                    .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                    .Where(where).ToDataTable();
            if (ret.Rows.Count > 0)
            {
                for (var i = 0; i < ret.Rows.Count; i++)
                {
                    var data = ret.Rows[i]["SpeechContent"].ToString().Replace("&lt;", "<").Replace("&gt;", ">");
                    ret.Rows[i]["SpeechContent"] = data;
                }
            }
            return ret;
        }

        /// <summary>
        /// 查询本年本月所有信息
        /// </summary>
        /// <returns></returns>
        public DataTable GetAllDayDatum1(string ProcessFactoryCode)
        {
            DateTime now = DateTime.Now;
            //本月第一天
            DateTime FirstDay = new DateTime(now.Year, now.Month, 1);
            //本月最后一天
            DateTime LastDay = Convert.ToDateTime(FirstDay.AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd") + " 23:59:59");
            var where = new Where<TbSafeDatumManage>();
            where.And(d => d.StartTime >= FirstDay && d.StartTime <= LastDay);
            if (!string.IsNullOrWhiteSpace(ProcessFactoryCode))
            {
                where.And(d => d.ProcessFactoryCode == ProcessFactoryCode);
            }
            var ret = Db.Context.From<TbSafeDatumManage>().Select(
                      TbSafeDatumManage._.All
                      , TbCompany._.CompanyFullName.As("ProcessFactoryName")
                      , TbUser._.UserName.As("InsertUserName"))
                    .LeftJoin<TbCompany>((a, c) => a.ProcessFactoryCode == c.CompanyCode)
                    .AddSelect(Db.Context.From<TbUser>().Select(p => p.UserName)
                    .Where(TbUser._.UserCode == TbSafeDatumManage._.SpeechUser), "SpeechUserName")
                    .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode).Where(where).ToDataTable();
            if (ret.Rows.Count > 0)
            {
                for (var i = 0; i < ret.Rows.Count; i++)
                {
                    var data = ret.Rows[i]["SpeechContent"].ToString().Replace("&lt;", "<").Replace("&gt;", ">");
                    ret.Rows[i]["SpeechContent"] = data;
                }
            }
            return ret;
        }
        #endregion

        #region （新增、编辑）数据

        /// <summary>
        /// 新增数据
        /// </summary>
        public AjaxResult Insert(TbSafeDatumManage model, bool isApi = false)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            model.ProjectId = "";//不保存项目id
            if (isApi == false)
            {
                model.BrowseCount = 0;
                model.InsertUserCode = OperatorProvider.Provider.CurrentUser.UserCode;
            }
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //添加信息
                    Repository<TbSafeDatumManage>.Insert(trans, model, isApi);
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
        public AjaxResult Update(TbSafeDatumManage model, bool isApi = false)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            var anyRet = AnyInfo(model.ID);
            if (anyRet.state.ToString() != ResultType.success.ToString())
                return anyRet;
            model.ProjectId = "";//不保存项目id
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())//使用事务
                {
                    //修改信息
                    Repository<TbSafeDatumManage>.Update(trans, model, p => p.ID == model.ID, isApi);
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
            var monthPlan = Repository<TbSafeDatumManage>.First(p => p.ID == keyValue);
            if (monthPlan == null)
                return AjaxResult.Warning("信息不存在");
            return AjaxResult.Success(monthPlan);
        }

        #endregion

    }
}
