using Dos.ORM;
using PM.Business.Production;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using PM.DataEntity.Distribution.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace PM.Business.Distribution
{
    /// <summary>
    /// 半成品签收
    /// </summary>
    public class SemiFinishedSignLogic
    {
        private readonly TbWorkOrderLogic _workOrderLogic = new TbWorkOrderLogic();

        #region 获取数据

        /// <summary>
        /// 获取编辑数据
        /// </summary>
        /// <param name="dataID">数据Id</param>
        /// <returns></returns>
        public Tuple<DataTable, DataTable> FindEntity(int dataID)
        {
            //var ret = Db.Context.From<TbSemiFinishedSign>()
            //   .Select(
            //           TbSemiFinishedSign._.All
            //           , TbCompany._.CompanyFullName.As("SiteName")
            //           , TbSysDictionaryData._.DictionaryText.As("DischargeTypeNew")
            //           , TbDistributionEnt._.All
            //           , TbUser._.UserName
            //          , TbCarInfo._.CarCph)
            //         .LeftJoin<TbCompany>((a, c) => a.SiteCode == c.CompanyCode)
            //         .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
            //         .Where(TbCompany._.CompanyCode == TbSemiFinishedSign._.ProcessFactoryCode), "ProcessFactoryName")
            //         .AddSelect(Db.Context.From<TbUser>().Select(p => p.UserName)
            //         .Where(TbUser._.UserCode == TbSemiFinishedSign._.Contacts), "ContactsName")
            //        .AddSelect(Db.Context.From<TbUser>().Select(p => p.UserName)
            //        .Where(TbUser._.UserCode == TbSemiFinishedSign._.Driver), "CarUser")
            //         .LeftJoin<TbDistributionEnt>((a, c) => a.DistributionCode == c.DistributionCode)
            //         .LeftJoin<TbSysDictionaryData>((a, c) => a.DischargeType == c.DictionaryCode && c.FDictionaryCode == "DischargeType")
            //         .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
            //        .LeftJoin<TbCarInfo>((a, c) => a.VehicleCode == c.CarCode)
            //        .Where(p => p.ID == dataID).ToDataTable();
            string sqlZB = @"select a.*,s.CompanyFullName as SiteName,case when a.OrderCodeH is null then c.OrderCode else a.OrderCodeH end as OrderCode,b.VehicleCode,d.CarCph,b.Driver,e.UserName as CarUser,f.DictionaryText as DischargeTypeNew from TbSemiFinishedSign a
left join TbDistributionEnt b on a.DistributionCode=b.DistributionCode
left join TbSiteDischargeCargo c on a.DistributionCode=c.DistributionCode and a.DischargeCargoCode=c.DischargeCargoCode
left join TbCarInfo d on b.VehicleCode=d.CarCode
left join TbUser e on b.Driver=e.UserCode
left join TbSysDictionaryData f on a.DischargeType=f.DictionaryCode
left join TbCompany s on a.SiteCode=s.CompanyCode
where a.ID=" +dataID+"";
            var ret = Db.Context.FromSql(sqlZB).ToDataTable();
            string sql = "";
            if (ret.Rows.Count>0)
            {
                if (!string.IsNullOrWhiteSpace(ret.Rows[0]["DistributionCode"].ToString()))
                {
                    sql = @"select disEntItem.*,wo.SiteCode,cp.CompanyFullName as SiteName,wo.TypeCode,wo.TypeName,pt.ProcessingTechnologyName,sd.DictionaryText as MeasurementUnitName from TbDistributionEntItem disEntItem
left join TbDistributionEntOrder disEntOrder on disEntItem.DisEntOrderIdentity=disEntOrder.DisEntOrderIdentity
left join TbWorkOrder wo on disEntItem.OrderCode=wo.OrderCode
left join TbProcessingTechnology pt on disEntItem.ProcessingTechnology=pt.ID
left join TbSysDictionaryData sd on disEntItem.MeteringUnit=sd.DictionaryCode and sd.FDictionaryCode='Unit'
left join TbCompany cp on wo.SiteCode=cp.CompanyCode 
left join TbSiteDischargeCargo xh on disEntItem.DistributionCode=xh.DistributionCode and disEntOrder.ID=xh.DisEntOrderId
where 1=1 and disEntItem.DistributionCode='" + ret.Rows[0]["DistributionCode"].ToString() + @"' 
and xh.DischargeCargoCode='" + ret.Rows[0]["DischargeCargoCode"].ToString() + "'";
                }
                else
                {
                    sql = @"select wod.ID,wod.ComponentName,wod.MaterialCode,wod.MaterialName,wod.SpecificationModel as Standard,wod.MeasurementUnit as MeteringUnit,wod.MeasurementUnit as UnitWeight,wod.MeasurementUnitZl as SingletonWeight,wod.Number,wod.WeightSmallPlan as WeightGauge,wod.Manufactor,wod.HeatNo,wod.HeatNo,'' as DistributionCode,'' as PlanItemID,0 as PSAmount,2 as DataType,wod.ProcessingTechnology,wod.PackNumber,wod.PackageNumber as PackagesNumber,wod.Number as GjNumber,'',wod.OrderCode,wod.ID as WorkorderdetailId,1 as CanLoadingPackCount,1 as ThisTimePackCount,wo.SiteCode,cp.CompanyFullName as SiteName,wo.TypeCode,wo.TypeName,sd1.DictionaryText as MeasurementUnitName,pt.ProcessingTechnologyName from TbWorkOrderDetail wod 
left join TbWorkOrder wo on wod.OrderCode=wo.OrderCode
left join TbCompany cp on wo.SiteCode=cp.CompanyCode
left join TbSysDictionaryData sd1 on wod.MeasurementUnit=sd1.DictionaryCode
left join TbProcessingTechnology pt on wod.ProcessingTechnology=pt.ID
where 1=1 and wod.RevokeStart='正常' and wod.OrderCode='" + ret.Rows[0]["OrderCodeH"].ToString() + "'";
                }
            }
            var items = Db.Context.FromSql(sql).ToDataTable();
            return new Tuple<DataTable, DataTable>(ret, items);
        }

        /// <summary>
        /// 获取数据列表(分页)
        /// </summary>
        public PageModel GetDataListForPage(SemiFinishedSignRequest request)
        {
            string where = " where 1=1 ";
            StringBuilder sbSiteCode = new StringBuilder();
            #region 数据权限新

            if (!string.IsNullOrWhiteSpace(request.SigninNuber))
            {
                where += " and Tb.SigninNuber like '%" + request.SigninNuber + "%'";
            }
            if (!string.IsNullOrWhiteSpace(request.DischargeCargoCode))
            {
                where += " and Tb.DistributionCode like '%" + request.DischargeCargoCode + "%'";
            }
            if (!string.IsNullOrWhiteSpace(request.SigninTime))
            {
                where += " and Tb.SigninTime=" + Convert.ToDateTime(request.SigninTime) + "";
            }
            if (!string.IsNullOrWhiteSpace(request.OperateState))
            {
                where += " and Tb.OperateState='" + request.OperateState + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where += " and Tb.ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where += " and Tb.ProjectId='" + request.ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.CarCph))
            {
                where += " and Tb.CarCph like '%" + request.CarCph + "%'";
            }
            if (!string.IsNullOrWhiteSpace(request.OrderCode))
            {
                where += " and (Tb.OrderCode like '%" + request.OrderCode + "%' or Tb.OrderCodeH like '%" + request.OrderCode + "%')";
            }
            if (!string.IsNullOrWhiteSpace(request.TypeCode))
            {
                where += " and Tb.TypeCode like '%" + request.TypeCode + "%'";
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
                    where += " and Tb.SiteCode in(" + sbSiteCode + ")";
                }
            }

            #endregion

            string sql = @"select * from (select sfs.*,case when sfs.DistributionCode is not null and sfs.DistributionCode!='' then '系统配送' else '线下登记' end DistributionType,sdc.OrderCode,ur1.UserName as CarUser,ur2.UserName as ContactsName,cp1.CompanyFullName as SiteName,cp2.CompanyFullName as ProcessFactoryName,car.CarCph 
                             ,
                            	(
                            		stuff((select ',' + CONVERT(varchar(100),DeliveryCompleteTime, 23) from TbDistributionPlanInfo 
		                            where CHARINDEX(OrderCode,sdc.OrderCode)>0 OR OrderCode=sfs.OrderCodeH
		                            for xml path('')),1,1,'')
                            	) AS DeliveryCompleteTime
                            from TbSemiFinishedSign sfs
                            left join (select sdc.DischargeCargoCode,case when sdc.OrderCode is null then dis.OrderCode else sdc.OrderCode end OrderCode from TbSiteDischargeCargo sdc 
                            left join TbDistributionEnt dis on sdc.DistributionCode=dis.DistributionCode) sdc on sfs.DischargeCargoCode=sdc.DischargeCargoCode
                            left join TbUser ur1 on sfs.Driver=ur1.UserCode
                            left join TbUser ur2 on sfs.Contacts=ur2.UserCode
                            left join TbCarInfo car on sfs.VehicleCode=car.CarCode
                            left join TbCompany cp1 on sfs.SiteCode=cp1.CompanyCode
                            left join TbCompany cp2 on sfs.ProcessFactoryCode=cp2.CompanyCode) Tb ";

            //参数化
            List<Parameter> parameter = new List<Parameter>();
            var model = Repository<TbSemiFinishedSign>.FromSqlToPageTable(sql + where, parameter, request.rows, request.page, "SigninNuber", "desc");
            return model;

            ////组装查询语句
            //#region 模糊搜索条件
            //var where = new Where<TbSemiFinishedSign>();
            //if (!string.IsNullOrWhiteSpace(request.SigninTime))
            //{
            //    DateTime sigTime = Convert.ToDateTime(request.SigninTime);
            //    where.And(p => p.SigninTime == sigTime);
            //}

            //if (!string.IsNullOrWhiteSpace(request.SiteCode))
            //{
            //    List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(request.SiteCode, 5);
            //    where.And(p => p.SiteCode.In(SiteList));
            //}
            //if (!string.IsNullOrWhiteSpace(request.VehicleCode))
            //{
            //    where.And(p => p.VehicleCode == request.VehicleCode);
            //}
            //if (!string.IsNullOrWhiteSpace(request.SigninNuber))
            //{
            //    where.And(p => p.SigninNuber.StartsWith(request.SigninNuber));
            //}


            //#endregion

            //#region 数据权限

            ////数据权限
            //var authorizaModel = new AuthorizeLogic().CheckAuthoriza(new AuthorizationParameterModel() { FormCode = "SemiFinishedSign", UserCode = request.UserCode });
            //if (authorizaModel.IsAuthorize)
            //{
            //    if (authorizaModel.Ids.Count > 0 && authorizaModel.UserCodes.Count > 0)
            //        where.And(d => d.InsertUserCode.In(authorizaModel.UserCodes) || d.ID.In(authorizaModel.Ids));
            //    else if (authorizaModel.Ids.Count > 0)
            //        where.Or(d => d.ID.In(authorizaModel.Ids));
            //    else if (authorizaModel.UserCodes.Count > 0)
            //        where.And(d => d.InsertUserCode.In(authorizaModel.UserCodes));
            //}
            //if (!string.IsNullOrEmpty(request.ProjectId))
            //    where.And(p => p.ProjectId == request.ProjectId);
            //#endregion

            //try
            //{
            //    var ret = Db.Context.From<TbSemiFinishedSign>()
            //  .Select(
            //          TbSemiFinishedSign._.All
            //          , TbCompany._.CompanyFullName.As("SiteName")
            //          , TbDistributionEntOrder._.OrderCode
            //          , TbUser._.UserName
            //          , TbCarInfo._.CarCph)
            //        .LeftJoin<TbCompany>((a, c) => a.SiteCode == c.CompanyCode)
            //        .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
            //        .Where(TbCompany._.CompanyCode == TbSemiFinishedSign._.ProcessFactoryCode), "ProcessFactoryName")
            //        .AddSelect(Db.Context.From<TbUser>().Select(p => p.UserName)
            //        .Where(TbUser._.UserCode == TbSemiFinishedSign._.Contacts), "ContactsName")
            //        .AddSelect(Db.Context.From<TbUser>().Select(p => p.UserName)
            //        .Where(TbUser._.UserCode == TbSemiFinishedSign._.Driver), "CarUser")
            //        .LeftJoin<TbDistributionEntOrder>((a, c) => a.DistributionCode == c.DistributionCode)
            //        .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
            //        .LeftJoin<TbCarInfo>((a, c) => a.VehicleCode == c.CarCode)
            //        .Where(where).OrderBy(d => d.ID).ToPageList(request.rows, request.page);
            //    return ret;
            //}
            //catch (Exception e)
            //{
            //    throw e;
            //}
        }

        #endregion

        #region 确认签收（系统配送）

        /// <summary>
        /// 确认签收
        /// </summary>
        public AjaxResult Update(int Id, string userCode = "", bool isApi = false)
        {
            if (Id <= 0)
                return AjaxResult.Warning("参数错误");
            var anyRet = AnyInfo(Id);
            if (anyRet.state.ToString() != ResultType.success.ToString())
                return anyRet;
            var model = (TbSemiFinishedSign)anyRet.data;
            TbDistributionEntOrder modelEnt = null;
            var zdxhModel = Repository<TbSiteDischargeCargo>.First(p => p.DischargeCargoCode == model.DischargeCargoCode);
            if (zdxhModel != null)
            {
                //查询配送装车数据
                modelEnt = Repository<TbDistributionEntOrder>.First(p => p.ID == zdxhModel.DisEntOrderId);
            }
            else
            {
                if (string.IsNullOrEmpty(model.OrderCodeH))
                    return AjaxResult.Warning("获取站点卸货信息失败");
            }
            if (!string.IsNullOrEmpty(userCode))
            {
                model.InsertUserCode = userCode;//签收人编号
            }
            else
            {
                model.InsertUserCode = OperatorProvider.Provider.CurrentUser.UserCode;//签收人编号
            }
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())//使用事务
                {
                    model.OperateState = "已签收";
                    model.SigninTime = DateTime.Now;
                    //修改信息
                    Repository<TbSemiFinishedSign>.Update(trans, model, p => p.ID == model.ID, isApi);
                    //当点击签收是，判断是否存在预警信息，如果存在就把预警信息状态改为撤销
                    Db.Context.FromSql("update TbFormEarlyWarningNodeInfo set EWStart=2 where MenuCode='SemiFinishedSign' and EWFormDataCode=" + Id + " and EWStart=0 and MsgType=2").SetDbTransaction(trans).ExecuteNonQuery();
                    //回写配送装车里面的签收状态
                    if (modelEnt != null)
                    {
                        modelEnt.SignState = "已签收";//签收状态
                        modelEnt.SignStateTime = DateTime.Now;//签收完成时间
                        Repository<TbDistributionEntOrder>.Update(trans, modelEnt, p => p.ID == modelEnt.ID, isApi);
                    }
                    trans.Commit();//提交事务

                    return AjaxResult.Success();
                }
            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.ToString());
            }
            //if (Id <= 0)
            //    return AjaxResult.Warning("参数错误");
            //var anyRet = AnyInfo(Id);
            //if (anyRet.state.ToString() != ResultType.success.ToString())
            //    return anyRet;
            //var model = (TbSemiFinishedSign)anyRet.data;
            ////查询配送装车数据
            //TbDistributionEnt modelEnt = Repository<TbDistributionEnt>.First(p => p.DistributionCode == model.DistributionCode);
            //if (!string.IsNullOrEmpty(userCode))
            //{
            //    model.InsertUserCode = userCode;//签收人编号
            //}
            //else
            //{
            //    model.InsertUserCode = OperatorProvider.Provider.CurrentUser.UserCode;//签收人编号
            //}
            //try
            //{
            //    using (DbTrans trans = Db.Context.BeginTransaction())//使用事务
            //    {
            //        model.OperateState = "已签收";
            //        model.SigninTime = DateTime.Now;
            //        //修改信息
            //        Repository<TbSemiFinishedSign>.Update(trans, model, p => p.ID == model.ID, isApi);
            //        //回写配送装车里面的签收状态
            //        if (modelEnt != null)
            //        {
            //            modelEnt.SignState = "已签收";//签收状态
            //            Repository<TbDistributionEnt>.Update(trans, modelEnt, p => p.ID == modelEnt.ID, isApi);
            //        }
            //        trans.Commit();//提交事务

            //        return AjaxResult.Success();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    return AjaxResult.Error(ex.ToString());
            //}
        }

        #endregion

        #region 判断
        /// <summary>
        /// 判断信息是否可操作
        /// </summary>
        /// <returns></returns>
        public AjaxResult AnyInfo(int keyValue)
        {
            var monthDemandPlan = Repository<TbSemiFinishedSign>.First(p => p.ID == keyValue);
            if (monthDemandPlan == null)
                return AjaxResult.Warning("信息不存在");

            return AjaxResult.Success(monthDemandPlan);
        }

        #endregion

        #region 导出

        /// <summary>
        /// 数据列表信息查询
        /// </summary>
        /// <returns></returns>
        public DataTable GetExportList(SemiFinishedSignRequest request)
        {
            string where = " where 1=1 ";
            StringBuilder sbSiteCode = new StringBuilder();
            #region 数据权限新
            if (!string.IsNullOrWhiteSpace(request.SigninNuber))
            {
                where += " and Tb.SigninNuber like '%" + request.SigninNuber + "%'";
            }
            if (!string.IsNullOrWhiteSpace(request.DischargeCargoCode))
            {
                where += " and Tb.DistributionCode like '%" + request.DischargeCargoCode + "%'";
            }
            if (!string.IsNullOrWhiteSpace(request.SigninTime))
            {
                where += " and Tb.SigninTime=" + Convert.ToDateTime(request.SigninTime) + "";
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where += " and Tb.ProcessFactoryCode='" + request.ProcessFactoryCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where += " and Tb.ProjectId='" + request.ProjectId + "'";
            }
            if (!string.IsNullOrWhiteSpace(request.CarCph))
            {
                where += " and Tb.CarCph like '%" + request.CarCph + "%'";
            }
            if (!string.IsNullOrWhiteSpace(request.OrderCode))
            {
                where += " and (Tb.OrderCode like '%" + request.OrderCode + "%' or Tb.OrderCodeH like '%" + request.OrderCode + "%')";
            }
            if (!string.IsNullOrWhiteSpace(request.TypeCode))
            {
                where += " and Tb.TypeCode like '%" + request.TypeCode + "%'";
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
                    where += " and Tb.SiteCode in(" + sbSiteCode + ")";
                }
            }

            where += " order by Tb.SigninNuber desc";

            #endregion

            string sql = @"select * from (select sfs.*,sdc.OrderCode,ur1.UserName as CarUser,ur2.UserName as ContactsName,cp1.CompanyFullName as SiteName,cp2.CompanyFullName as ProcessFactoryName,car.CarCph 
                             ,
                            	(
                            		stuff((select ',' + CONVERT(varchar(100),DeliveryCompleteTime, 23) from TbDistributionPlanInfo 
		                            where CHARINDEX(OrderCode,sdc.OrderCode)>0 OR OrderCode=sfs.OrderCodeH
		                            for xml path('')),1,1,'')
                            	) AS DeliveryCompleteTime
                            from TbSemiFinishedSign sfs
                            left join (select sdc.DischargeCargoCode,case when sdc.OrderCode is null then dis.OrderCode else sdc.OrderCode end OrderCode from TbSiteDischargeCargo sdc 
                            left join TbDistributionEnt dis on sdc.DistributionCode=dis.DistributionCode) sdc on sfs.DischargeCargoCode=sdc.DischargeCargoCode
                            left join TbUser ur1 on sfs.Driver=ur1.UserCode
                            left join TbUser ur2 on sfs.Contacts=ur2.UserCode
                            left join TbCarInfo car on sfs.VehicleCode=car.CarCode
                            left join TbCompany cp1 on sfs.SiteCode=cp1.CompanyCode
                            left join TbCompany cp2 on sfs.ProcessFactoryCode=cp2.CompanyCode) Tb ";
            var model = Db.Context.FromSql(sql + where).ToDataTable();
            return model;
        }

        #endregion

        #region 确认签收（线下补录）

        /// <summary>
        /// 确认签收
        /// </summary>
        public AjaxResult SemiFinishedSignNew(string OrderCode)
        {
            if (string.IsNullOrWhiteSpace(OrderCode))
            {
                return AjaxResult.Warning("参数错误");
            }
            string sql = @"select a.ID,a.OrderCodeH,b.OrderCode,b.DisEntOrderId from TbSemiFinishedSign a
left join TbSiteDischargeCargo b on a.DischargeCargoCode=b.DischargeCargoCode
where (a.DischargeCargoCode is not null and a.OperateState is null and b.OrderCode='" + OrderCode + "') or (OrderCodeH='" + OrderCode + "')";
            DataTable dt = Db.Context.FromSql(sql).ToDataTable();
            List<TbSemiFinishedSign> listsf = new List<TbSemiFinishedSign>();
            List<TbDistributionEntOrder> listEnt = new List<TbDistributionEntOrder>();
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    //半成品签收
                    TbSemiFinishedSign model = Repository<TbSemiFinishedSign>.First(p => p.ID == Convert.ToInt32(dt.Rows[i]["ID"]));
                    model.OperateState = "已签收";
                    model.SigninTime = DateTime.Now;
                    model.InsertUserCode = OperatorProvider.Provider.CurrentUser.UserCode;//签收人编号
                    listsf.Add(model);


                    if (!string.IsNullOrWhiteSpace(dt.Rows[i]["DisEntOrderId"].ToString()))
                    {
                        TbDistributionEntOrder modelEnt = Repository<TbDistributionEntOrder>.First(p => p.ID == Convert.ToInt32(dt.Rows[i]["DisEntOrderId"]));
                        modelEnt.SignState = "已签收";//签收状态
                        modelEnt.SignStateTime = DateTime.Now;//签收完成时间
                        listEnt.Add(modelEnt);
                    }
                }
            }

            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())//使用事务
                {
                    if (listsf.Count > 0)
                    {
                        Repository<TbSemiFinishedSign>.Update(trans, listsf,true);//半成品签收
                    }
                    if (listEnt.Count > 0)
                    {
                        Repository<TbDistributionEntOrder>.Update(trans, listEnt,true);//配送装车订单
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

    }
}
