using Dos.ORM;
using PM.Business.Production;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace PM.Business.SettlementManage
{
    /// <summary>
    /// 结算单处理类
    /// </summary>
    public class SettlementOrderLogic
    {

        private readonly TbWorkOrderLogic _workOrderLogic = new TbWorkOrderLogic();

        #region 查询数据

        /// <summary>
        /// 首页查询
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public PageModel GetAllSettlement(FPiCiXQPlan request)
        {
            string where = " where 1=1 ";
            StringBuilder sbSiteCode = new StringBuilder();
            //组装查询语句
            #region 数据权限新

            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where += " and a.ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
            }
            //if (!string.IsNullOrWhiteSpace(request.ProjectId))
            //{
            //    where += " and a.ProjectId='" + request.ProjectId + "'";
            //}
            if (!string.IsNullOrWhiteSpace(request.KSdatetime))
            {
                where += " and a.StartDate>='" + request.KSdatetime + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.JSdatetime))
            {
                where += " and a.EndDate<='" + request.JSdatetime + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);
                for (int i = 0; i < SiteList.Count; i++)
                {
                    if (i == (SiteList.Count - 1))
                    {
                        sbSiteCode.Append("'" + SiteList[i] + "'");
                    }
                    else
                    {
                        sbSiteCode.Append("'" + SiteList[i] + "',");
                    }
                }
                if (SiteList.Count > 0)
                {
                    where += " and a.SiteCode in(" + sbSiteCode + ")";
                }
            }

            #endregion

            try
            {
                var sql = @"select 
a.*,c.CompanyFullName SiteName,d.CompanyFullName ProcessFactoryName,
e.UserName 
from TbSettlementOrder a 
left join TbCompany c on a.SiteCode=c.CompanyCode 
left join TbCompany d on a.ProcessFactoryCode=d.CompanyCode 
left join TbUser e on a.InsertUserCode=e.UserCode ";
                //参数化
                List<Dos.ORM.Parameter> para = new List<Dos.ORM.Parameter>();
                var ret = Repository<TbSettlementOrder>.FromSqlToPageTable(sql + where, para, request.rows, request.page, "InsertTime", "desc");
                return ret;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 以ID查询结算
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public Tuple<object, object> GetFormJson(int keyValue)
        {
            var ret = Db.Context.From<TbSettlementOrder>()
            .Select(
                    TbSettlementOrder._.All
                    , TbCompany._.CompanyFullName.As("SiteName")
                    , TbUser._.UserName)
                  .LeftJoin<TbCompany>((a, c) => a.SiteCode == c.CompanyCode)
                  .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                  .Where(TbCompany._.CompanyCode == TbSettlementOrder._.ProcessFactoryCode), "ProcessFactoryName")
                  .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                    .Where(p => p.ID == keyValue).ToDataTable();
            if (ret.Rows.Count > 0)
            {
                var items = Db.Context.From<TbSettlementOrderDetail>().Where(p => p.SettlementCode == ret.Rows[0]["SettlementCode"].ToString()).ToDataTable();
                return new Tuple<object, object>(ret, items);
            }
            return new Tuple<object, object>(ret, null);
        }

        /// <summary>
        /// 新增明细对账弹窗
        /// </summary>
        /// <returns></returns>
        public PageModel GetBalanceOfAccountOrder(FPiCiXQPlan request, string keyword)
        {
            string where = "";
            StringBuilder sbSiteCode = new StringBuilder();

            #region 数据权限新

            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where += " and a.ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
            }
            //if (!string.IsNullOrWhiteSpace(request.ProjectId))
            //{
            //    where += " and a.ProjectId='" + request.ProjectId + "'";
            //}
            if (!string.IsNullOrWhiteSpace(request.CXfbgqzd))
            {
                where += (" and a.SiteCode='" + request.CXfbgqzd + "'");
            }
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                where += " and a.SigninNuber like '%" + keyword + "%'";
            }
            if (!string.IsNullOrWhiteSpace(request.KSdatetime))
            {
                where += ("and a.InsertTime>='" + request.KSdatetime + "'");
            }
            if (!string.IsNullOrWhiteSpace(request.JSdatetime))
            {
                where += ("and a.InsertTime<='" + request.JSdatetime + "'");
            }

            #endregion

            string sql = @"select 
                           a.SigninNuber,
                           a.InsertTime,
                           a.SignforTime,
                           a.ProjectId,
                           a.SiteCode,
                           a.ProcessFactoryCode,
                           sum(b.SteelWeight)/1000 as SteelWeight,
                           sum(b.GeGouZhongLiWeight)/1000 as GeGouZhongLiWeight,
                           sum(b.HSectionSteelWeight)/1000 as HSectionSteelWeight,
                           sum(b.AGrille)/1000 as  AGrille
                           from TbSignforDuiZhang a 
                           left join TbSignforDuiZhangDetail b on a.SigninNuber=b.SigninNuber 
                           where a.SigninNuber not in(select BalanceOfAccountCode from TbSettlementOrderDetail) "+ where +@"
                           group by a.SigninNuber,a.InsertTime,a.SignforTime, a.ProjectId,a.SiteCode,a.ProcessFactoryCode ";

            //参数化
            List<Dos.ORM.Parameter> para = new List<Dos.ORM.Parameter>();
            var ret = Repository<TbSettlementOrder>.FromSqlToPageTable(sql, para, request.rows, request.page, "SigninNuber", "desc");
            return ret;
        }

        /// <summary>
        /// 判断信息是否可操作
        /// </summary>
        /// <returns></returns>
        public AjaxResult AnyInfo(int keyValue)
        {
            var monthPlan = Repository<TbSettlementOrder>.First(p => p.ID == keyValue);
            if (monthPlan == null)
                return AjaxResult.Warning("信息不存在");
            if (monthPlan.Examinestatus != "未发起")
                return AjaxResult.Warning("信息正在审核中或已审核完成,不能进行此操作");
            return AjaxResult.Success(monthPlan);
        }

        /// <summary>
        /// 图形数据查询
        /// </summary>
        /// <returns></returns>
        public DataTable GetAnalysis(PageSearchRequest psr)
        {
            string sql = @"select a.BranchCode,b.CompanyFullName,SUM(a.OrderMachiningAmount) JiaGong,SUM(a.AmountOfMoney) JinE from TbSettlementOrder a 
left join TbCompany b on a.BranchCode=b.CompanyCode where DateDiff(month, a.InsertTime, GetDate()) = 0 ";
            if (!string.IsNullOrEmpty(psr.ProjectId))
            {
                sql += " and a.ProjectId='" + psr.ProjectId + "'";
            }
            sql += " group by a.BranchCode,b.CompanyFullName";
            var ret = Db.Context.FromSql(sql).ToDataTable();
            return ret;
        }

        #endregion

        #region 新增数据

        /// <summary>
        /// 新增数据
        /// </summary>
        public AjaxResult Insert(TbSettlementOrder model, List<TbSettlementOrderDetail> items, bool isApi = false)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            model.Examinestatus = "未发起";
            model.InsertUserCode = OperatorProvider.Provider.CurrentUser.UserCode;
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //添加信息
                    Repository<TbSettlementOrder>.Insert(trans, model, isApi);
                    Repository<TbSettlementOrderDetail>.Insert(trans, items, isApi);
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
        public AjaxResult Update(TbSettlementOrder model, List<TbSettlementOrderDetail> items, bool isApi = false)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            var anyRet = AnyInfo(model.ID);
            if (anyRet.state.ToString() != ResultType.success.ToString())
                return anyRet;
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())//使用事务
                {
                    //修改信息
                    Repository<TbSettlementOrder>.Update(trans, model, p => p.ID == model.ID, isApi);

                    if (items.Count > 0)
                    {
                        //删除历史明细信息
                        Repository<TbSettlementOrderDetail>.Delete(p => p.SettlementCode == model.SettlementCode);
                        //添加明细信息
                        Repository<TbSettlementOrderDetail>.Insert(trans, items, isApi);
                    }

                    trans.Commit();//提交事务

                    return AjaxResult.Success();
                }
            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.ToString());
            }
        }

        #endregion

        #region 删除数据

        /// <summary>
        /// 删除数据
        /// </summary>
        public AjaxResult Delete(int keyValue)
        {
            try
            {
                //判断信息是否存在
                var anyRet = AnyInfo(keyValue);
                if (anyRet.state.ToString() != ResultType.success.ToString())
                    return anyRet;
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //删除信息
                    var count = Repository<TbSettlementOrder>.Delete(trans, p => p.ID == keyValue);
                    Repository<TbSettlementOrderDetail>.Delete(trans, p => p.SettlementCode == ((TbSettlementOrder)anyRet.data).SettlementCode);
                    trans.Commit();//提交事务

                    return AjaxResult.Success();
                }
            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.ToString());
            }
        }

        #endregion

        #region 导出

        /// <summary>
        /// 获取数据列表(分页)
        /// </summary>
        public DataTable GetExportList(FPiCiXQPlan request)
        {
            string where = " where 1=1 ";
            StringBuilder sbSiteCode = new StringBuilder();
            //组装查询语句
            #region 数据权限新

            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where += " and a.ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.KSdatetime))
            {
                where += " and a.StartDate>='" + request.KSdatetime + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.JSdatetime))
            {
                where += " and a.EndDate<='" + request.JSdatetime + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);
                for (int i = 0; i < SiteList.Count; i++)
                {
                    if (i == (SiteList.Count - 1))
                    {
                        sbSiteCode.Append("'" + SiteList[i] + "'");
                    }
                    else
                    {
                        sbSiteCode.Append("'" + SiteList[i] + "',");
                    }
                }
                if (SiteList.Count > 0)
                {
                    where += " and a.SiteCode in(" + sbSiteCode + ")";
                }
            }

            where += " order by a.InsertTime desc";

            #endregion

            try
            {
                var sql = @"select 
a.*,c.CompanyFullName SiteName,d.CompanyFullName ProcessFactoryName,
e.UserName 
from TbSettlementOrder a 
left join TbCompany c on a.SiteCode=c.CompanyCode 
left join TbCompany d on a.ProcessFactoryCode=d.CompanyCode 
left join TbUser e on a.InsertUserCode=e.UserCode ";
                var ret = Db.Context.FromSql(sql + where).ToDataTable();
                return ret;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion
    }
}
