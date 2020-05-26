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

namespace PM.Business.Distribution
{
    /// <summary>
    /// 站点卸货
    /// </summary>
    public class SiteDischargeCargoLogic
    {
        private readonly TbWorkOrderLogic _workOrderLogic = new TbWorkOrderLogic();

        #region 新增数据

        /// <summary>
        /// 新增数据
        /// </summary>
        public AjaxResult Insert(TbSiteDischargeCargo model, List<TbSiteDischargeCargoDetail> items)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            model.InsertUserCode = OperatorProvider.Provider.CurrentUser.UserCode;
            model.Examinestatus = "未发起";
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //添加信息,明细信息
                    Repository<TbSiteDischargeCargo>.Insert(trans, model);
                    Repository<TbSiteDischargeCargoDetail>.Insert(trans, items);
                    trans.Commit();
                    return AjaxResult.Success();
                }
            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.ToString());
            }
        }

        #endregion

        #region 修改数据

        /// <summary>
        /// 修改数据
        /// </summary>
        public AjaxResult Update(TbSiteDischargeCargo model, List<TbSiteDischargeCargoDetail> items)
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
                    Repository<TbSiteDischargeCargo>.Update(trans, model, p => p.ID == model.ID);
                    if (items.Count > 0)
                    {
                        //删除历史明细信息
                        Repository<TbSiteDischargeCargoDetail>.Delete(p => p.DischargeCargoCode == model.DischargeCargoCode);
                        //添加明细信息
                        Repository<TbSiteDischargeCargoDetail>.Insert(trans, items);
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
                    //删除信息,明细信息
                    var count = Repository<TbSiteDischargeCargo>.Delete(trans, p => p.ID == keyValue);
                    Repository<TbSiteDischargeCargoDetail>.Delete(trans, p => p.DischargeCargoCode == ((TbSiteDischargeCargo)anyRet.data).DischargeCargoCode);
                    var count1 = Repository<TbSemiFinishedSign>.Delete(trans, p => p.ID == keyValue);
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

        #region 获取数据

        /// <summary>
        /// 获取编辑数据
        /// </summary>
        /// <param name="dataID">数据Id</param>
        /// <returns></returns>
        public Tuple<DataTable, DataTable> FindEntity(int dataID)
        {
            var ret = Db.Context.From<TbSiteDischargeCargo>()
               .Select(
                       TbSiteDischargeCargo._.All
                       , TbCompany._.CompanyFullName.As("SiteName")
                       , TbSysDictionaryData._.DictionaryText.As("DischargeTypeNew")
                       , TbUser._.UserName
                      , TbCarInfo._.CarCph)
                     .LeftJoin<TbCompany>((a, c) => a.SiteCode == c.CompanyCode)
                     .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                     .Where(TbCompany._.CompanyCode == TbSiteDischargeCargo._.ProcessFactoryCode), "ProcessFactoryName")
                     .AddSelect(Db.Context.From<TbUser>().Select(p => p.UserName)
                     .Where(TbUser._.UserCode == TbSiteDischargeCargo._.Contacts), "ContactsName")
                    .AddSelect(Db.Context.From<TbUser>().Select(p => p.UserName)
                    .Where(TbUser._.UserCode == TbSiteDischargeCargo._.Driver), "CarUser")
                     .LeftJoin<TbSysDictionaryData>((a, c) => a.DischargeType == c.DictionaryCode && c.FDictionaryCode == "DischargeType")
                     .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                    .LeftJoin<TbCarInfo>((a, c) => a.VehicleCode == c.CarCode)
                    .Where(p => p.ID == dataID).ToDataTable();
            //查找明细信息
            var items = Db.Context.From<TbSiteDischargeCargoDetail>().Select(
               TbSiteDischargeCargoDetail._.All,
               TbSysDictionaryData._.DictionaryText.As("MeasurementUnitText"))
           .LeftJoin<TbSysDictionaryData>((a, c) => a.MeasurementUnit == c.DictionaryCode)
           .AddSelect(Db.Context.From<TbSysDictionaryData>().Select(p => p.DictionaryText)
           .Where(TbSysDictionaryData._.DictionaryCode == TbSiteDischargeCargoDetail._.MeasurementUnit && TbSysDictionaryData._.FDictionaryCode == "Unit"), "MeasurementUnitNew")
           .Where(p => p.DischargeCargoCode == ret.Rows[0]["DischargeCargoCode"].ToString()).ToDataTable();
            return new Tuple<DataTable, DataTable>(ret, items);
        }

        /// <summary>
        /// 获取数据列表(分页)
        /// </summary>
        public PageModel GetDataListForPage(SiteDischargeCargoRequest request)
        {
            ////组装查询语句
            //#region 模糊搜索条件

            //var where = new Where<TbSiteDischargeCargo>();
            //if (!string.IsNullOrWhiteSpace(request.DistributionTime))
            //{
            //    where.And(p => p.DistributionTime.ToString() == request.DistributionTime);
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
            //if (!string.IsNullOrEmpty(request.DischargeType))
            //{
            //    where.And(p => p.DischargeType == request.DischargeType);
            //}
            //if (!string.IsNullOrEmpty(request.ProjectId))
            //    where.And(p => p.ProjectId == request.ProjectId);
            //if (!string.IsNullOrEmpty(request.ProcessFactoryCode))
            //    where.And(p => p.ProcessFactoryCode == request.ProcessFactoryCode);

            //#endregion

            //#region 数据权限

            ////数据权限
            //var authorizaModel = new AuthorizeLogic().CheckAuthoriza(new AuthorizationParameterModel() { FormCode = "SiteDischargeCargo", UserCode = request.UserCode });
            //if (authorizaModel.IsAuthorize)
            //{
            //    if (authorizaModel.Ids.Count > 0 && authorizaModel.UserCodes.Count > 0)
            //        where.And(d => d.InsertUserCode.In(authorizaModel.UserCodes) || d.ID.In(authorizaModel.Ids));
            //    else if (authorizaModel.Ids.Count > 0)
            //        where.Or(d => d.ID.In(authorizaModel.Ids));
            //    else if (authorizaModel.UserCodes.Count > 0)
            //        where.And(d => d.InsertUserCode.In(authorizaModel.UserCodes));
            //}
            //#endregion

            //try
            //{
            //    var ret = Db.Context.From<TbSiteDischargeCargo>()
            //  .Select(
            //          TbSiteDischargeCargo._.All
            //          , TbCompany._.CompanyFullName.As("SiteName")
            //          , TbSysDictionaryData._.DictionaryText.As("DischargeTypeNew")
            //          , TbUser._.UserName
            //          , TbCarInfo._.CarCph
            //          , TbDistributionEnt._.OrderCode)
            //        .LeftJoin<TbCompany>((a, c) => a.SiteCode == c.CompanyCode)
            //        .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
            //        .Where(TbCompany._.CompanyCode == TbSiteDischargeCargo._.ProcessFactoryCode), "ProcessFactoryName")
            //        .AddSelect(Db.Context.From<TbUser>().Select(p => p.UserName)
            //        .Where(TbUser._.UserCode == TbSiteDischargeCargo._.Contacts), "ContactsName")
            //        .AddSelect(Db.Context.From<TbUser>().Select(p => p.UserName)
            //        .Where(TbUser._.UserCode == TbSiteDischargeCargo._.Driver), "CarUser")
            //        .LeftJoin<TbSysDictionaryData>((a, c) => a.DischargeType == c.DictionaryCode && c.FDictionaryCode == "DischargeType")
            //        .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
            //        .LeftJoin<TbCarInfo>((a, c) => a.VehicleCode == c.CarCode)
            //        .LeftJoin<TbDistributionEnt>((a, c) => a.DistributionCode == c.DistributionCode)
            //          .Where(where).OrderBy(d => d.ID).ToPageList(request.rows, request.page);
            //    return ret;
            //}
            //catch (Exception)
            //{
            //    throw;
            //}
            try
            {
                string where = " where 1=1 ";
                StringBuilder sbSiteCode = new StringBuilder();
                #region 数据权限新

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
                if (!string.IsNullOrWhiteSpace(request.TypeCode))
                {
                    where += " and Tb.TypeCode like '%" + request.TypeCode + "%'";
                }
                if (!string.IsNullOrWhiteSpace(request.OrderCode))
                {
                    where += " and Tb.OrderCode like '%" + request.OrderCode + "%'";
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

                string sql = @"select * from (select siteDc.ID,siteDc.DischargeCargoCode,siteDc.DistributionCode,siteDc.SiteCode,siteDc.ProcessFactoryCode,siteDc.ProjectId,sd.DictionaryText as DischargeTypeNew,case when siteDc.OrderCode is null then disEnt.OrderCode else siteDc.OrderCode end as OrderCode,siteDc.TypeCode,siteDc.TypeName,cp.CompanyFullName as SiteName,ur1.UserName as CarUser,siteDc.ContactWay,ur2.UserName as UserName,ur3.UserName as  ContactsName,siteDc.DistributionAddress,siteDc.DistributionTime,siteDc.SumTotal,car.CarCph,siteDc.InsertTime from TbSiteDischargeCargo siteDc
left join TbDistributionEnt disEnt on siteDc.DistributionCode=disEnt.DistributionCode
left join TbSysDictionaryData sd on siteDc.DischargeType=sd.DictionaryCode and sd.FDictionaryCode='DischargeType'
left join TbCompany cp on siteDc.SiteCode=cp.CompanyCode
left join TbUser ur1 on siteDc.Driver=ur1.UserCode
left join TbUser ur2 on siteDc.InsertUserCode=ur2.UserCode
left join TbUser ur3 on siteDc.Contacts=ur3.UserCode
left join TbCarInfo car on siteDc.VehicleCode=car.CarCode) Tb";

                //参数化
                List<Parameter> parameter = new List<Parameter>();
                var model = Repository<TbSiteDischargeCargo>.FromSqlToPageTable(sql + where, parameter, request.rows, request.page, "DischargeCargoCode", "desc");
                return model;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region 判断
        /// <summary>
        /// 判断信息是否可操作
        /// </summary>
        /// <returns></returns>
        public AjaxResult AnyInfo(int keyValue)
        {
            var siteDischarge = Repository<TbSiteDischargeCargo>.First(p => p.ID == keyValue);
            if (siteDischarge == null)
                return AjaxResult.Warning("信息不存在");
            if (siteDischarge.Examinestatus != "未发起" && siteDischarge.Examinestatus != "已退回")
                return AjaxResult.Warning("信息正在审核中或已审核完成,不能进行此操作");

            return AjaxResult.Success(siteDischarge);
        }

        #endregion

        #region 获取配送装车列表数据
        /// <summary>
        /// 获取配送装车列表数据
        /// </summary>
        /// <returns></returns>
        public PageModel GetDistributionEntList(TbDistributionEntRequest request)
        {
            //参数化
            List<Parameter> parameter = new List<Parameter>();
            string where = " where 1=1 and de.ProjectId=@ProjectId";
            parameter.Add(new Parameter("@ProjectId", request.ProjectId, DbType.String, null));
            if (!string.IsNullOrWhiteSpace(request.keyword))
            {
                where += " and cp.CompanyFullName like @keyword or de.DistributionCode like @keyword or de.TypeCode like @keyword or de.TypeName like @keyword or de.UsePart like @keyword";
                parameter.Add(new Parameter("@keyword", '%' + request.keyword + '%', DbType.String, null));
            }
            string sql = @"select de.*,cp.CompanyFullName as SiteName,ur.UserName,car.CarCph,card.UserName as CarUser from TbDistributionEnt de
left join TbCompany cp on de.SiteCode=cp.CompanyCode
left join TbUser ur on de.InsertUserCode=ur.UserCode
left join TbCarInfo car on de.VehicleCode=car.CarCode
left join TbCarInfoDetail card on de.Driver=card.UserCode ";
            var model = Repository<TbDistributionEnt>.FromSqlToPageTable(sql + where, parameter, request.rows, request.page, "ID", "asc");
            return model;
            //var ret = Db.Context.From<TbDistributionEnt>()
            //   .Select(
            //           TbDistributionEnt._.All
            //           , TbCompany._.CompanyFullName.As("SiteName")
            //           , TbUser._.UserName
            //          , TbCarInfo._.CarCph
            //          , TbCarInfoDetail._.UserName.As("CarUser"))
            //         .LeftJoin<TbCompany>((a, c) => a.SiteCode == c.CompanyCode)
            //         .AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
            //         .Where(TbCompany._.CompanyCode == TbDistributionEnt._.ProcessFactoryCode), "ProcessFactoryName")
            //         .AddSelect(Db.Context.From<TbUser>().Select(p => p.UserName)
            //         .Where(TbUser._.UserCode == TbDistributionEnt._.Contacts), "ContactsName")
            //         .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
            //        .LeftJoin<TbCarInfo>((a, c) => a.VehicleCode == c.CarCode)
            //        .LeftJoin<TbCarInfoDetail>((a, c) => a.Driver == c.UserCode)
            //         .ToDataTable();
            //return ret;
        }
        /// <summary>
        /// 获取该配送单号下的明细信息
        /// </summary>
        /// <param name="DistributionCode">配送单号</param>
        /// <returns></returns>
        public DataTable GetDistributionEntItemList(string DistributionCode)
        {
            var ret = Db.Context.From<TbDistributionEntItem>()
               .Select(
                       TbDistributionEntItem._.All
                       , TbSysDictionaryData._.DictionaryText.As("MeasurementUnitText"))
                     .LeftJoin<TbSysDictionaryData>((a, c) => a.MeteringUnit == c.DictionaryCode && c.FDictionaryCode == "Unit")
                     .Where(p => p.DistributionCode == DistributionCode)
                     .ToDataTable();
            return ret;
        }

        #endregion

        #region 确认卸货

        /// <summary>
        /// 确认卸货
        /// </summary>
        /// <param name="dataID"></param>
        /// <returns></returns>
        public AjaxResult DischargeCargoConfirm(int dataID, bool isApi = false)
        {
            //查找卸货单信息
            var dischargeCargo = Repository<TbSiteDischargeCargo>.First(p => p.ID == dataID);
            if (dischargeCargo == null)
                return AjaxResult.Error("卸货单信息不存在");
            if (dischargeCargo.DischargeType == "YesComplete")
                return AjaxResult.Error("卸货单已卸货完成");
            dischargeCargo.DischargeType = "YesComplete";//已完成
            dischargeCargo.DistributionTime = DateTime.Now;
            //查找配送装车订单信息
            var distribution = Repository<TbDistributionEnt>.First(p => p.DistributionCode == dischargeCargo.DistributionCode);
            if (distribution == null)
                return AjaxResult.Error("配送装车单信息不存在");
            //查找配送装车订单信息
            var distributionOrder = Repository<TbDistributionEntOrder>.First(p => p.DistributionCode == dischargeCargo.DistributionCode && p.ID == dischargeCargo.DisEntOrderId);
            if (distributionOrder == null)
                return AjaxResult.Error("配送装车订单信息不存在");
            distributionOrder.UnloadingState = "已完成";//卸货状态
            distributionOrder.UnloadingStateTiem = DateTime.Now;//卸货完成时间

            //定义一个list集合存放修改后的配送计划状态
            var disPanlList = new List<TbDistributionPlanInfo>();

            //定义一个list集合存放修改后的配送计划明细
            var disEntPlanItemList = new List<TbDistributionPlanDetailInfo>();

            #region 修改配送计划中的配送状态开始

            //1、获取该卸货单的所有明细
            string sql = @"select sdc.DischargeCargoCode,sdc.DischargeType,sdcd.OrderCode,dei.WorkorderdetailId,sdcd.XhNumber from TbSiteDischargeCargo sdc 
left join TbSiteDischargeCargoDetail sdcd on sdc.DischargeCargoCode=sdcd.DischargeCargoCode
left join TbDistributionEntItem dei on sdcd.DisEntOrderItemId=dei.ID and sdc.DistributionCode=dei.DistributionCode where sdc.DischargeCargoCode='" + dischargeCargo.DischargeCargoCode + "'";
            //参数化
            List<Parameter> parameter = new List<Parameter>();
            var items = Repository<SiteDischargeCargoDetailRequest>.FromSql(sql, parameter, "DischargeCargoCode", "desc").ToList();
            if (items.Count > 0)
            {
                List<string> listCode = new List<string>();
                for (int i = 0; i < items.Count; i++)
                {
                    if (!listCode.Contains(items[i].OrderCode))
                    {
                        listCode.Add(items[i].OrderCode);
                    }
                }

                for (int i = 0; i < listCode.Count; i++)
                {
                    int? a = 0;//该订单所有的件数
                    int b = 0;//该订单卸货的件数
                    //查询配送计划主表信息
                    var retPlan = Db.Context.From<TbDistributionPlanInfo>()
                     .Select(TbDistributionPlanInfo._.All).Where(p => p.OrderCode == listCode[i]).First();
                    if (retPlan != null)
                    {
                        //修改配送时间状态
                        if (retPlan.DistributionTiemStart == "暂无")
                        {
                            DateTime time1 = Convert.ToDateTime(Convert.ToDateTime(distribution.LoadCompleteTime).ToString("yyyy-MM-dd"));
                            DateTime time2 = Convert.ToDateTime(Convert.ToDateTime(retPlan.DistributionTime).ToString("yyyy-MM-dd"));
                            if (time1 < time2)
                            {
                                retPlan.DistributionTiemStart = "提前配送";
                            }
                            if (time1 > time2)
                            {
                                retPlan.DistributionTiemStart = "延后配送";
                            }
                            if (time1 == time2)
                            {
                                retPlan.DistributionTiemStart = "正常配送";
                            }
                        }
                        string sql1 = @"select OrderCode,sum(Number) as Number from TbOrderProgressDetail
where OrderCode='" + listCode[i] + "' and RevokeStart='正常' group by OrderCode";
                        var items1 = Repository<TbOrderProgressDetail>.FromSql(sql1, parameter, "OrderCode", "asc").ToList();
                        if (items1.Count > 0)
                        {
                            a = items1[0].Number;
                            string sql2 = @"select sdcd.OrderCode,Isnull(sum(sdcd.XhNumber),0) as XhNumber from TbSiteDischargeCargo sdc 
left join TbSiteDischargeCargoDetail sdcd on sdc.DischargeCargoCode=sdcd.DischargeCargoCode
left join TbDistributionEntItem dei on sdcd.DisEntOrderItemId=dei.ID and sdc.DistributionCode=dei.DistributionCode 
where sdc.DischargeType='YesComplete' and sdcd.OrderCode='" + listCode[i] + "' group by sdcd.OrderCode";
                            var items2 = Repository<SiteDischargeCargoDetailRequest>.FromSql(sql2, parameter, "OrderCode", "asc").ToList();
                            if (items2.Count > 0)
                            {
                                b += items2[0].XhNumber;
                            }
                            //获取本次卸货的订单
                            var BcXh = items.Where(p => p.OrderCode == listCode[i]).ToList();
                            if (BcXh != null)
                            {
                                b += BcXh.Sum(p => p.XhNumber);
                            }
                            for (int x = 0; x < BcXh.Count; x++)
                            {
                                //查询该订单配送计划里面的配送明细
                                var oItem = Db.Context.From<TbDistributionPlanDetailInfo>().Select(TbDistributionPlanDetailInfo._.All).Where(d => d.WorkorderdetailId == BcXh[x].WorkorderdetailId).First();
                                if (oItem != null)
                                {
                                    int? a1 = oItem.PSAmount - BcXh[x].XhNumber;
                                    oItem.PSAmount = a1;
                                    disEntPlanItemList.Add(oItem);
                                }
                            }
                        }
                        //1、配送完成
                        //a、第一步先判断该条配送计划是否都配送装车咯
                        //查询子表信息

                        if (a == b)
                        {
                            //b、第二步在判断配送装车是否都卸货完成
                            retPlan.DistributionStart = "配送完成";
                            retPlan.DeliveryCompleteTime = DateTime.Now;
                        }
                        //2、部分配送
                        //a、第一步先判断该条配送计划是否存在配送装车、同时该条配送计划的明细在配送装车中只有部分已经装车了。
                        else if (b > 0 && b < a)
                        {
                            //b、第二步判断配送装车中是否存在一条卸货状态为已完成的状态
                            retPlan.DistributionStart = "部分配送";

                        }

                        disPanlList.Add(retPlan);
                    }
                }

            }

            #endregion 修改配送计划中的配送状态结束

            using (DbTrans trans = Db.Context.BeginTransaction())
            {
                Repository<TbSiteDischargeCargo>.Update(trans, dischargeCargo, isApi);
                Repository<TbDistributionEntOrder>.Update(trans, distributionOrder, isApi);
                //修改配送计划中的配送状态
                if (disPanlList != null && disPanlList.Count > 0)
                {
                    Repository<TbDistributionPlanInfo>.Update(trans, disPanlList, isApi);
                }
                //修改配送计划明细中的配送件数
                if (disEntPlanItemList != null && disEntPlanItemList.Count > 0)
                {
                    Repository<TbDistributionPlanDetailInfo>.Update(trans, disEntPlanItemList, isApi);
                }
                trans.Commit();
                return AjaxResult.Success();
            }
        }
        //public AjaxResult DischargeCargoConfirm(int dataID, bool isApi = false)
        //{
        //    //查找卸货单信息
        //    var dischargeCargo = Repository<TbSiteDischargeCargo>.First(p => p.ID == dataID);
        //    if (dischargeCargo == null)
        //        return AjaxResult.Error("卸货单信息不存在");
        //    if (dischargeCargo.DischargeType == "YesComplete")
        //        return AjaxResult.Error("卸货单已卸货完成");
        //    dischargeCargo.DischargeType = "YesComplete";//已完成
        //    dischargeCargo.DistributionTime = DateTime.Now;
        //    //查找配送装车单
        //    var distribution = Repository<TbDistributionEnt>.First(p => p.DistributionCode == dischargeCargo.DistributionCode);
        //    if (distribution == null)
        //        return AjaxResult.Error("配送装车单信息不存在");
        //    distribution.UnloadingState = "已完成";

        //    #region 修改配送计划中的配送状态开始

        //    //定义一个list集合存放修改后的配送计划状态
        //    var disPanlList = new List<TbDistributionPlanInfo>();
        //    //判断该条配送装车单是否选择了多个配送计划
        //    if (!string.IsNullOrWhiteSpace(distribution.DistributionPlanCode))
        //    {
        //        string[] aPlanCode = distribution.DistributionPlanCode.Split('，');
        //        List<string> listCode = new List<string>();
        //        if (aPlanCode.Length > 0)
        //        {
        //            for (int i = 0; i < aPlanCode.Length; i++)
        //            {
        //                listCode.Add(aPlanCode[i]);
        //            }
        //            var where = new Where<TbDistributionPlanInfo>();
        //            if (listCode.Count > 0)
        //            {
        //                where.And(p => p.DistributionPlanCode.In(listCode));
        //            }
        //            //查询配送计划主表信息
        //            var ret = Db.Context.From<TbDistributionPlanInfo>()
        //             .Select(TbDistributionPlanInfo._.All).Where(where).ToList();
        //            if (ret.Count > 0)
        //            {
        //                for (int i = 0; i < ret.Count; i++)
        //                {
        //                    //修改配送时间状态
        //                    if (ret[i].DistributionTiemStart == "暂无")
        //                    {
        //                        DateTime time1 = Convert.ToDateTime(Convert.ToDateTime(distribution.LoadCompleteTime).ToString("yyyy-MM-dd"));
        //                        DateTime time2 = Convert.ToDateTime(Convert.ToDateTime(ret[i].DistributionTime).ToString("yyyy-MM-dd"));
        //                        if (time1 < time2)
        //                        {
        //                            ret[i].DistributionTiemStart = "提前配送";
        //                        }
        //                        if (time1 > time2)
        //                        {
        //                            ret[i].DistributionTiemStart = "延后配送";
        //                        }
        //                        if (time1 == time2)
        //                        {
        //                            ret[i].DistributionTiemStart = "正常配送";
        //                        }
        //                        disPanlList.Add(ret[i]);
        //                    }

        //                    //1、配送完成
        //                    //a、第一步先判断该条配送计划是否都配送装车咯
        //                    //查询子表信息
        //                    //查询配送计划主表信息
        //                    var retItem = Db.Context.From<TbDistributionPlanDetailInfo>()
        //                     .Select(TbDistributionPlanDetailInfo._.All).Where(p => p.OrderCode == ret[i].OrderCode).ToList();
        //                    int wpsNum = retItem.Sum(p => p.PSAmount);
        //                    int Num = retItem.Sum(p => p.Number);
        //                    if (wpsNum == 0)
        //                    {
        //                        //b、第二步在判断配送装车是否都卸货完成
        //                        var retEnt = Db.Context.From<TbDistributionEnt>()
        //            .Select(TbDistributionEnt._.All).Where(p => p.OrderCode.Like(ret[i].OrderCode) && p.UnloadingState != "已完成" && p.DistributionCode != distribution.DistributionCode).ToList();
        //                        if (retEnt == null || retEnt.Count == 0)
        //                        {
        //                            ret[i].DistributionStart = "配送完成";
        //                            disPanlList.Add(ret[i]);
        //                        }
        //                    }
        //                    //2、部分配送
        //                    //a、第一步先判断该条配送计划是否存在配送装车、同时该条配送计划的明细在配送装车中只有部分已经装车了。
        //                    else if (wpsNum > 0 && wpsNum < Num)
        //                    {
        //                        //b、第二步判断配送装车中是否存在一条卸货状态为已完成的状态
        //                        var retEnt = Db.Context.From<TbDistributionEnt>()
        //           .Select(TbDistributionEnt._.All).Where(p => p.OrderCode.Like(ret[i].OrderCode) && (p.UnloadingState == "已完成" || p.DistributionCode == distribution.DistributionCode)).ToList();
        //                        if (retEnt != null || retEnt.Count > 0)
        //                        {
        //                            ret[i].DistributionStart = "部分配送";
        //                            disPanlList.Add(ret[i]);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    #endregion 修改配送计划中的配送状态结束

        //    using (DbTrans trans = Db.Context.BeginTransaction())
        //    {
        //        Repository<TbSiteDischargeCargo>.Update(trans, dischargeCargo, isApi);
        //        Repository<TbDistributionEnt>.Update(trans, distribution, isApi);
        //        //修改配送计划中的配送状态
        //        if (disPanlList != null && disPanlList.Count > 0)
        //        {
        //            Repository<TbDistributionPlanInfo>.Update(trans, disPanlList, isApi);
        //        }
        //        trans.Commit();
        //        return AjaxResult.Success();
        //    }
        //}

        #endregion
    }
}
