using Dos.ORM;
using PM.Business.Production;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using PM.DataEntity.System.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace PM.Business.SettlementManage
{
    /// <summary>
    /// 签收对账单
    /// </summary>
    public class SignforDuiZhangLogic
    {
        private readonly TbWorkOrderLogic _workOrderLogic = new TbWorkOrderLogic();

        #region 查询数据

        /// <summary>
        /// 全部查询/条件查询
        /// </summary>
        /// <param name="ent"></param>
        /// <returns></returns>
        public PageModel GetAllSignforDuiZhang(FPiCiXQPlan request)
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
                where += " and a.StartDate>='"+request.KSdatetime+"'";
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
from TbSignforDuiZhang a
left join TbCompany c on a.SiteCode=c.CompanyCode  
left join TbCompany d on a.ProcessFactoryCode=d.CompanyCode  
left join TbUser e on a.InsertUserCode=e.UserCode ";
                //参数化
                List<Dos.ORM.Parameter> para = new List<Dos.ORM.Parameter>();
                var ret = Repository<TbSignforDuiZhang>.FromSqlToPageTable(sql + where, para, request.rows, request.page, "InsertTime", "desc");
                return ret;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 以ID查询签收对账
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public Tuple<object, object,object> GetFormJson(int keyValue)
        {
            var ret = Db.Context.From<TbSignforDuiZhang>()
            .Select(
                    TbSignforDuiZhang._.All
                    , TbCompany._.CompanyFullName.As("SiteName")
                    , TbUser._.UserName)
                  .LeftJoin<TbCompany>((a, c) => a.SiteCode == c.CompanyCode)
                  .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                  .Where(TbCompany._.CompanyCode == TbSignforDuiZhang._.ProcessFactoryCode), "ProcessFactoryName")
                  .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                    .Where(p => p.ID == keyValue).ToDataTable();
            if (ret.Rows.Count > 0)
            {
                var items = Db.Context.From<TbSignforDuiZhangDetail>().Where(p => p.SigninNuber == ret.Rows[0]["SigninNuber"].ToString()).ToDataTable();
                //return new Tuple<object, object>(ret, items);
                //var items1 = Db.Context.From<TbSignforDuiZhangDetail>().Where(p => p.SigninNuber == ret.Rows[0]["SigninNuber"].ToString()&&p.AddType=="手动填报").ToDataTable();
                var items1 = Db.Context.From<TbSignforDuiZhangDetail>()
                           .Select(
                                    TbSignforDuiZhangDetail._.OrderCode).Where(p => p.SigninNuber == ret.Rows[0]["SigninNuber"].ToString() && p.AddType == "手动填报").ToDataTable();
                return new Tuple<object, object,object>(ret, items, items1);
            }
            return new Tuple<object, object, object>(ret, null,null);
        }

        #region 获取站点
        public PageModel GetCompanyCompanyAllSiteList(TbCompanyRequest request,string orgType, string parentCode)
        {
            string sql = "";
            string where = "";
            if (string.IsNullOrWhiteSpace(orgType))
            {
                 orgType = OperatorProvider.Provider.CurrentUser.OrgType;
            }
            if (string.IsNullOrWhiteSpace(parentCode))
            {
                 parentCode = OperatorProvider.Provider.CurrentUser.CompanyId;
            }
            //参数化
            List<Parameter> parameter = new List<Parameter>();
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where += " and ProjectId=@ProjectId";
                parameter.Add(new Parameter("@ProjectId", request.ProjectId, DbType.String, null));
            }
            if (!string.IsNullOrWhiteSpace(request.keyword))
            {
                where += " and CompanyFullName like @CompanyFullName";
                parameter.Add(new Parameter("@CompanyFullName", '%' + request.keyword + '%', DbType.String, null));
            }
            if (orgType == "0" || orgType == "1" || orgType == "2")
            {
                sql = @"select cm.id,cm.CompanyCode,cm.CompanyFullName,cm.OrgType,cm.Address,pc.ProjectId from TbCompany cm
left join TbProjectCompany pc on cm.CompanyCode=pc.CompanyCode where 1=1 and cm.OrgType=5 ";

            }
            else
            {
                sql = @"select * from GetCompanyChild_fun(@parentCode) as cp where 1=1 and cp.OrgType=5 ";
                parameter.Add(new Parameter("@parentCode", parentCode, DbType.String, null));
            }
            var model = Repository<TbCompany>.FromSqlToPageTable(sql + where, parameter, request.rows, request.page, "ID", "asc");
            return model;
        }

        #endregion

        /// <summary>
        /// 签收单弹窗
        /// </summary>
        /// <returns></returns>
        public PageModel GetAllQianShou(FPiCiXQPlan entity, string keyword)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" where 1=1 and TbQsdMx.OrderCode is not null and TbQsdMx.SN not in(select SignForNo from TbSignforDuiZhangDetail) ");
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                sb.Append(" and (TbQsdMx.SN like '%" + keyword + "%' or Tb.DistributionCode like '%" + keyword + "%' )");
            }
            if (!string.IsNullOrWhiteSpace(entity.ProjectId))
            {
                sb.Append(" and TbQsdMx.ProjectId='" + entity.ProjectId + "'");
            }
            if (!string.IsNullOrWhiteSpace(entity.ProcessFactoryCode))
            {
                sb.Append(" and TbQsdMx.ProcessFactoryCode='" + entity.ProcessFactoryCode + "'");
            }
            if (!string.IsNullOrWhiteSpace(entity.CXfbgqzd))
            {
                sb.Append(" and TbQsdMx.SiteCode='" + entity.CXfbgqzd + "'");
            }
            if (!string.IsNullOrWhiteSpace(entity.KSdatetime))
            {
                sb.Append("and TbQsdMx.SigninTime>='" + entity.KSdatetime + "'");
            }
            if (!string.IsNullOrWhiteSpace(entity.JSdatetime))
            {
                sb.Append("and TbQsdMx.SigninTime<='" + entity.JSdatetime + "'");
            }
            try
            {
                //参数化
                string sql = @"select TbQsdMx.SN,TbQsdMx.OrderCode,TbQsdMx.SiteCode,TbQsdMx.SiteName,TbQsdMx.DistributionCode,TbQsdMx.ProjectId,TbQsdMx.ProcessFactoryCode,TbQsdMx.SigninTime,
                                cast(isnull(TbQsdMx.SteelWeight,0)/1000 as decimal(18,5)) as Rebar,
                                cast(isnull(TbQsdMx.GeGouZhongLiWeight,0)/1000 as decimal(18,5)) as MiddlePillar,
                                cast(isnull(TbQsdMx.HSectionSteelWeight,0)/1000 as decimal(18,5)) as HSectionSteel,
                                cast(isnull(TbQsdMx.AGrille,0)/1000 as decimal(18,5)) as AGrille, 
                                cast((isnull(TbQsdMx.SteelWeight,0)+isnull(TbQsdMx.GeGouZhongLiWeight,0)+isnull(TbQsdMx.HSectionSteelWeight,0)+isnull(TbQsdMx.AGrille,0))/1000 as decimal(18,5)) as GratingWeight 
                                from 
                                (
                                	select Tb.*,
                                		case WHEN Tb.OrderCodeH IS NOT NULL THEN 
                                		(
                                			SELECT sum(WeightSmallPlan) AS disWeight  FROM TbWorkOrderDeliveryOverItem twodoi
                                			WHERE twodoi.OrderCode= Tb.OrderCode and twodoi.ProcessingTechnology=2
                                	    )	
                                		ELSE	
                                	   (
                                		select sum(isnull(dei.Number,0)*isnull
                                		(dei.UnitWeight,0)*isnull(dei.SingletonWeight,0)) as disWeight
                                		from TbSiteDischargeCargo sdc
                                		left join TbSiteDischargeCargoDetail sdcd on sdc.DischargeCargoCode=sdcd.DischargeCargoCode
                                		left join TbDistributionEntItem dei on sdcd.DisEntOrderItemId=dei.ID
                                		where  dei.DisEntOrderIdentity is not null and dei.ProcessingTechnology=2 and sdcd.DischargeCargoCode=Tb.DischargeCargoCode
                                	   ) end as SteelWeight,--类型1
                                	    case WHEN Tb.OrderCodeH IS NOT NULL THEN 
                                		(
                                			SELECT sum(WeightSmallPlan) AS disWeight  FROM TbWorkOrderDeliveryOverItem twodoi
                                			WHERE twodoi.OrderCode= Tb.OrderCode and twodoi.ProcessingTechnology=4
                                	    )	
                                		ELSE	
                                		(
                                			select sum(isnull(dei.Number,0)*isnull
                                		(dei.UnitWeight,0)*isnull(dei.SingletonWeight,0)) as disWeight
                                		from TbSiteDischargeCargo sdc
                                		left join TbSiteDischargeCargoDetail sdcd on sdc.DischargeCargoCode=sdcd.DischargeCargoCode
                                		left join TbDistributionEntItem dei on sdcd.DisEntOrderItemId=dei.ID
                                		where  dei.DisEntOrderIdentity is not null and dei.ProcessingTechnology=4 and sdcd.DischargeCargoCode=Tb.DischargeCargoCode
                                		) end as GeGouZhongLiWeight,--类型2
                                		case WHEN Tb.OrderCodeH IS NOT NULL THEN 
                                		(
                                			SELECT sum(WeightSmallPlan) AS disWeight  FROM TbWorkOrderDeliveryOverItem twodoi
                                			WHERE twodoi.OrderCode= Tb.OrderCode and twodoi.ProcessingTechnology in(6,7,8,9,10)
                                	    )	
                                		ELSE	
                                		(
                                			select sum(isnull(dei.Number,0)*isnull
                                		(dei.UnitWeight,0)*isnull(dei.SingletonWeight,0)) as disWeight
                                		from TbSiteDischargeCargo sdc
                                		left join TbSiteDischargeCargoDetail sdcd on sdc.DischargeCargoCode=sdcd.DischargeCargoCode
                                		left join TbDistributionEntItem dei on sdcd.DisEntOrderItemId=dei.ID
                                		where  dei.DisEntOrderIdentity is not null and dei.ProcessingTechnology in(6,7,8,9,10) and sdcd.DischargeCargoCode=Tb.DischargeCargoCode
                                		) end as AGrille,--类型3
                                		case WHEN Tb.OrderCodeH IS NOT NULL THEN 
                                		(
                                			SELECT sum(WeightSmallPlan) AS disWeight  FROM TbWorkOrderDeliveryOverItem twodoi
                                			WHERE twodoi.OrderCode= Tb.OrderCode and twodoi.ProcessingTechnology in(12,13,14,15)
                                	    )	
                                		ELSE	
                                		(
                                			select sum(isnull(dei.Number,0)*isnull
                                		(dei.UnitWeight,0)*isnull(dei.SingletonWeight,0)) as disWeight
                                		from TbSiteDischargeCargo sdc
                                		left join TbSiteDischargeCargoDetail sdcd on sdc.DischargeCargoCode=sdcd.DischargeCargoCode
                                		left join TbDistributionEntItem dei on sdcd.DisEntOrderItemId=dei.ID
                                		where  dei.DisEntOrderIdentity is not null 
                                		and dei.ProcessingTechnology in(12,13,14,15) 
                                		and sdcd.DischargeCargoCode=Tb.DischargeCargoCode
                                		) end as HSectionSteelWeight--类型4
                                from 
                                (select sfs.ID,sfs.OrderCodeH,ISNULL(sfs.OrderCodeH,sdc.OrderCode) AS OrderCode,
                                sfs.SigninNuber as SN,cp.CompanyFullName as SiteName,
                                sfs.SigninTime,sfs.DistributionCode,sfs.DischargeCargoCode,sfs.ProjectId,sfs.SiteCode,sfs.ProcessFactoryCode 
                                 from TbSemiFinishedSign sfs
                                left join TbCompany cp on cp.CompanyCode=sfs.SiteCode
                                left join TbSiteDischargeCargo sdc on sfs.DischargeCargoCode=sdc.DischargeCargoCode
                                ) Tb
                                ) TbQsdMx";
                List<Dos.ORM.Parameter> para = new List<Dos.ORM.Parameter>();
                var ret = Repository<TbSemiFinishedSign>.FromSqlToPageTable(sql + sb.ToString(), para, entity.rows, entity.page, "SN", "desc");
                return ret;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 签收单弹窗
        /// </summary>
        /// <returns></returns>
        public PageModel GetJgWcWpsList(FPiCiXQPlan entity, string keyword,string SigninNuber,string OrderCode)
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                sb.Append(" and (Tb.OrderCode like '%" + keyword + "%' or Tb.TypeCode like '%" + keyword + "%' or Tb.TypeName like '%" + keyword + "%' )");
            }
            if (!string.IsNullOrWhiteSpace(entity.ProjectId))
            {
                sb.Append(" and Tb.ProjectId='" + entity.ProjectId + "'");
            }
            if (!string.IsNullOrWhiteSpace(entity.ProcessFactoryCode))
            {
                sb.Append(" and Tb.ProcessFactoryCode='" + entity.ProcessFactoryCode + "'");
            }
            if (!string.IsNullOrWhiteSpace(entity.CXfbgqzd))
            {
                sb.Append(" and Tb.SiteCode='" + entity.CXfbgqzd + "'");
            }
            if (!string.IsNullOrEmpty(OrderCode))
            {
                sb.Append(" and Tb.OrderCode not in(" + OrderCode + ")");
            }
            try
            {
                //参数化
                string sql = @"select * from(select Tb1.*,isnull(Tb2.SteelWeight,0) as SteelWeight,isnull(Tb3.GeGouZhongLiWeight,0) as GeGouZhongLiWeight,isnull(Tb4.AGrille,0) as AGrille,isnull(Tb5.HSectionSteelWeight,0) as HSectionSteelWeight,isnull(Tb2.SteelWeight,0)+isnull(Tb3.GeGouZhongLiWeight,0)+isnull(Tb4.AGrille,0)+isnull(Tb5.HSectionSteelWeight,0) as GratingWeight from (
select wo.OrderCode,wo.TypeCode,wo.TypeName,wo.ProjectId,wo.ProcessFactoryCode,wo.ProcessingState,wo.SiteCode,wo.InsertTime from TbWorkOrder wo
left join TbWorkOrderDetail wod on  wo.OrderCode=wod.OrderCode
left join (select entOrderItem.OrderCode,entOrderItem.WorkorderdetailId,sum(entOrderItem.Number) as PSNumber from TbDistributionEntOrder entOrder
left join TbDistributionEntItem entOrderItem on entOrder.DisEntOrderIdentity=entOrderItem.DisEntOrderIdentity where entOrderItem.OrderCode is not null  group by entOrderItem.OrderCode,entOrderItem.WorkorderdetailId ) TbPs 
on wo.OrderCode=TbPs.OrderCode and wod.id=TbPs.WorkorderdetailId where (wod.Number-isnull(TbPs.PSNumber,0))>0 and wod.RevokeStart='正常'
group by wo.OrderCode,wo.TypeCode,wo.TypeName,wo.ProjectId,wo.ProcessFactoryCode,wo.ProcessingState,wo.SiteCode,wo.InsertTime) Tb1
left join (select wod.OrderCode,cast(sum(wod.MeasurementUnitZl*wod.ItemUseNum*(wod.Number-isnull(TbPs.PSNumber,0)))/1000 as  decimal(18,5)) as SteelWeight from TbWorkOrder wo
left join TbWorkOrderDetail wod on  wo.OrderCode=wod.OrderCode
left join (select entOrderItem.OrderCode,entOrderItem.WorkorderdetailId,sum(entOrderItem.Number) as PSNumber from TbDistributionEntOrder entOrder
left join TbDistributionEntItem entOrderItem on entOrder.DisEntOrderIdentity=entOrderItem.DisEntOrderIdentity where entOrderItem.OrderCode is not null  group by entOrderItem.OrderCode,entOrderItem.WorkorderdetailId ) TbPs 
on wo.OrderCode=TbPs.OrderCode and wod.id=TbPs.WorkorderdetailId where (wod.Number-isnull(TbPs.PSNumber,0))>0 and wod.RevokeStart='正常' and wod.ProcessingTechnology=2 group by wod.OrderCode) Tb2 on Tb1.OrderCode=Tb2.OrderCode
left join (select wod.OrderCode,cast(sum(wod.MeasurementUnitZl*wod.ItemUseNum*(wod.Number-isnull(TbPs.PSNumber,0)))/1000 as  decimal(18,5)) as GeGouZhongLiWeight from TbWorkOrder wo
left join TbWorkOrderDetail wod on  wo.OrderCode=wod.OrderCode
left join (select entOrderItem.OrderCode,entOrderItem.WorkorderdetailId,sum(entOrderItem.Number) as PSNumber from TbDistributionEntOrder entOrder
left join TbDistributionEntItem entOrderItem on entOrder.DisEntOrderIdentity=entOrderItem.DisEntOrderIdentity where entOrderItem.OrderCode is not null  group by entOrderItem.OrderCode,entOrderItem.WorkorderdetailId ) TbPs 
on wo.OrderCode=TbPs.OrderCode and wod.id=TbPs.WorkorderdetailId where  (wod.Number-isnull(TbPs.PSNumber,0))>0 and wod.RevokeStart='正常' and wod.ProcessingTechnology=4 group by wod.OrderCode) Tb3 on Tb1.OrderCode=Tb3.OrderCode
left join (select wod.OrderCode,cast(sum(wod.MeasurementUnitZl*wod.ItemUseNum*(wod.Number-isnull(TbPs.PSNumber,0)))/1000 as  decimal(18,5)) as AGrille from TbWorkOrder wo
left join TbWorkOrderDetail wod on  wo.OrderCode=wod.OrderCode
left join (select entOrderItem.OrderCode,entOrderItem.WorkorderdetailId,sum(entOrderItem.Number) as PSNumber from TbDistributionEntOrder entOrder
left join TbDistributionEntItem entOrderItem on entOrder.DisEntOrderIdentity=entOrderItem.DisEntOrderIdentity where entOrderItem.OrderCode is not null  group by entOrderItem.OrderCode,entOrderItem.WorkorderdetailId ) TbPs 
on wo.OrderCode=TbPs.OrderCode and wod.id=TbPs.WorkorderdetailId where (wod.Number-isnull(TbPs.PSNumber,0))>0 and wod.RevokeStart='正常' and wod.ProcessingTechnology in(6,7,8,9,10) group by wod.OrderCode) Tb4 on Tb1.OrderCode=Tb4.OrderCode
left join (select wod.OrderCode,cast(sum(wod.MeasurementUnitZl*wod.ItemUseNum*(wod.Number-isnull(TbPs.PSNumber,0)))/1000 as  decimal(18,5)) as HSectionSteelWeight from TbWorkOrder wo
left join TbWorkOrderDetail wod on  wo.OrderCode=wod.OrderCode
left join (select entOrderItem.OrderCode,entOrderItem.WorkorderdetailId,sum(entOrderItem.Number) as PSNumber from TbDistributionEntOrder entOrder
left join TbDistributionEntItem entOrderItem on entOrder.DisEntOrderIdentity=entOrderItem.DisEntOrderIdentity where entOrderItem.OrderCode is not null  group by entOrderItem.OrderCode,entOrderItem.WorkorderdetailId ) TbPs 
on wo.OrderCode=TbPs.OrderCode and wod.id=TbPs.WorkorderdetailId where (wod.Number-isnull(TbPs.PSNumber,0))>0 and wod.RevokeStart='正常' and wod.ProcessingTechnology in(12,13,14,15) group by wod.OrderCode) Tb5 on Tb1.OrderCode=Tb5.OrderCode) Tb
where 1=1 and Tb.ProcessingState='Finishing' and Tb.OrderCode not in(select OrderCode from TbSignforDuiZhangDetail where AddType='手动填报' and SigninNuber!='" + SigninNuber + "') ";
                List<Dos.ORM.Parameter> para = new List<Dos.ORM.Parameter>();
                var ret = Repository<TbWorkOrder>.FromSqlToPageTable(sql + sb.ToString(), para, entity.rows, entity.page, "OrderCode", "desc");
                return ret;
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// 判断信息是否可操作
        /// </summary>
        /// <returns></returns>
        public AjaxResult AnyInfo(int keyValue)
        {
            var monthPlan = Repository<TbSignforDuiZhang>.First(p => p.ID == keyValue);
            if (monthPlan == null)
                return AjaxResult.Warning("信息不存在");
            if (monthPlan.Examinestatus != "未发起")
                return AjaxResult.Warning("信息正在审核中或已审核完成,不能进行此操作");
            return AjaxResult.Success(monthPlan);
        }

        ///// <summary>
        ///// 各站点发料数量分析
        ///// </summary>
        ///// <returns></returns>
        //public DataTable GetSiteAnalysis(PageSearchRequest psr)
        //{
        //    var where = new Where<TbSignforDuiZhang>();
        //    where.And(d => d.Examinestatus == "审核完成");
        //    if (!string.IsNullOrEmpty(psr.ProjectId)) 
        //    {
        //        where.And(d => d.ProjectId==psr.ProjectId);
        //    }
        //    var ret = Db.Context.From<TbSignforDuiZhang>().Select(
        //        TbCompany._.CompanyFullName.As("SiteName"),
        //        TbSignforDuiZhang._.SiteCode,
        //        TbSignforDuiZhang._.HavingAmount.Sum()
        //        ).LeftJoin<TbCompany>((a,c)=>a.SiteCode==c.CompanyCode)
        //        .Where(where).GroupBy(TbSignforDuiZhang._.SiteCode, TbCompany._.CompanyFullName).ToDataTable();
        //    return ret;
        //}

        ///// <summary>
        ///// 各加工类型发料数量分析
        ///// </summary>
        ///// <returns></returns>
        //public DataTable GetMachiningTypeAnalysis(PageSearchRequest psr)
        //{
        //    var where = new Where<TbSignforDuiZhang>();
        //    where.And(d => d.Examinestatus == "审核完成");
        //    if (!string.IsNullOrEmpty(psr.ProjectId))
        //    {
        //        where.And(d => d.ProjectId == psr.ProjectId);
        //    }
        //    var ret = Db.Context.From<TbSignforDuiZhang>().Select(
        //        TbSignforDuiZhang._.MachiningType,
        //        TbSignforDuiZhang._.HavingAmount.Sum()
        //        ).Where(where).GroupBy(TbSignforDuiZhang._.MachiningType).ToDataTable();
        //    return ret;
        //}

        #endregion

        #region 新增/编辑数据

        /// <summary>
        /// 新增数据
        /// </summary>
        public AjaxResult Insert(TbSignforDuiZhang model, List<TbSignforDuiZhangDetail> items, bool isApi = false)
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
                    Repository<TbSignforDuiZhang>.Insert(trans, model, isApi);
                    Repository<TbSignforDuiZhangDetail>.Insert(trans, items, isApi);
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
        public AjaxResult Update(TbSignforDuiZhang model, List<TbSignforDuiZhangDetail> items, bool isApi = false)
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
                    Repository<TbSignforDuiZhang>.Update(trans, model, p => p.ID == model.ID, isApi);

                    if (items.Count > 0)
                    {
                        //删除历史明细信息
                        Repository<TbSignforDuiZhangDetail>.Delete(p => p.SigninNuber == model.SigninNuber);
                        //添加明细信息
                        Repository<TbSignforDuiZhangDetail>.Insert(trans, items, isApi);
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
                    var count = Repository<TbSignforDuiZhang>.Delete(trans, p => p.ID == keyValue);
                    Repository<TbSignforDuiZhangDetail>.Delete(trans, p => p.SigninNuber == ((TbSignforDuiZhang)anyRet.data).SigninNuber);
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
from TbSignforDuiZhang a
left join TbCompany c on a.SiteCode=c.CompanyCode  
left join TbCompany d on a.ProcessFactoryCode=d.CompanyCode  
left join TbUser e on a.InsertUserCode=e.UserCode ";
                //参数化
                List<Dos.ORM.Parameter> para = new List<Dos.ORM.Parameter>();
                var ret = Db.Context.FromSql(sql+where).ToDataTable();
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
