using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PM.DataEntity;
using PM.DataEntity.Production.ViewModel;
using Dos.ORM;
using System.Data;
using PM.DataAccess.DbContext;
using PM.Common;
using PM.DataEntity.RawMaterial.ViewModel;
using PM.Business.Production;
using System.Configuration;
namespace PM.Business.RawMaterial
{
    /// <summary>
    /// 逻辑处理层
    /// 供应清单
    /// </summary>
    public class SupplyListLogic
    {
        private readonly TbWorkOrderLogic _workOrderLogic = new TbWorkOrderLogic();
        string IsFormalSystem = ConfigurationManager.AppSettings["IsFormalSystem"];

        #region 判断

        /// <summary>
        /// 判断信息是否可操作
        /// </summary>
        /// <returns></returns>
        public AjaxResult AnyInfo(int keyValue)
        {
            var tbSupplyList = Repository<TbSupplyList>.First(p => p.ID == keyValue);
            if (tbSupplyList == null)
                return AjaxResult.Warning("信息不存在");
            return AjaxResult.Success(tbSupplyList);
        }

        #endregion

        #region 首页查询
        /// <summary>
        /// 
        /// 供应清单 实体=加工厂批次需求计划
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        /// 
        public PageModel GetAllOrBySearch(TSupplyListRequest entity)
        {
            //组装查询语句
            #region 模糊搜索条件
            List<Parameter> parameter = new List<Parameter>();
            string where = " where 1=1";
            if (!string.IsNullOrWhiteSpace(entity.ProcessFactoryCode))
            {
                where += " and a.ProcessFactoryCode=@ProcessFactoryCode";
                parameter.Add(new Parameter("@ProcessFactoryCode", entity.ProcessFactoryCode, DbType.String, null));
            }
            if (!string.IsNullOrWhiteSpace(entity.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(entity.SiteCode, 5);//站点
                List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(entity.SiteCode, 4);//工区
                where += " and (a.SiteCode in('" + string.Join("','", SiteList) + "') or a.WorkAreaCode in('" + string.Join("','", WorkAreaList) + "'))";
            }
            if (!string.IsNullOrWhiteSpace(entity.ProjectId))
            {
                where += " and a.ProjectId=@ProjectId";
                parameter.Add(new Parameter("@ProjectId", entity.ProjectId, DbType.String, null));
            }
            if (!string.IsNullOrWhiteSpace(entity.BatchPlanNum))
            {
                where += " and a.BatchPlanNum like '%" + entity.BatchPlanNum + "%'";
            }
            if (entity.HistoryMonth.HasValue)
            {
                where += " and YEAR(a.InsertTime)=" + entity.HistoryMonth.Value.Year + " and MONTH(a.InsertTime)=" + entity.HistoryMonth.Value.Month;
            }
            if (!string.IsNullOrWhiteSpace(entity.StateCode))
            {
                if (entity.StateCode == "供货超时")
                    where += " and a.SupplyDate<GETDATE() and a.StateCode='未供货'";
                else if (entity.StateCode == "未供货")
                    where += " and a.SupplyDate>=GETDATE() and a.StateCode='未供货'";
                else
                    where += " and a.StateCode='" + entity.StateCode + "'";
            }
            if (!string.IsNullOrWhiteSpace(entity.DemandPlanCode))
            {
                where += " and( a.RawMaterialDemandNum='" + entity.DemandPlanCode + "' or i.DemandPlanCode='" +
                         entity.DemandPlanCode + "')";
            }
            #endregion

            string sql = @"SELECT 
                            a.*,
                            b.CompanyFullName AS BranchName,
                            c.CompanyFullName AS WorkAreaName,
                            d.CompanyFullName AS ProcessFactoryName,
                            e.UserName,
                            f.UserName AS AcceptorName,
                            g.DictionaryText AS RebarTypeNew,
                            h.UnqualifiedTotal,
							case when i.DemandPlanCode is null then a.RawMaterialDemandNum else i.DemandPlanCode end as DemandPlanCode,
							gh.GhCount
                            FROM 
                            TbSupplyList a
                            left join (select COUNT(1) as GhCount,Tb.BatchPlanNum from (select a.ThisTime,b.BatchPlanNum,SUM(a.ThisTimeCount) as ThisTimeCount from TbSupplyListDetailHistory a
left join (select a.BatchPlanNum,case when b.DemandPlanCode is null then a.RawMaterialDemandNum else b.DemandPlanCode end as DemandPlanCode from TbFactoryBatchNeedPlan a
left join TbRawMaterialMonthDemandSupplyPlan b on a.RawMaterialDemandNum=b.SupplyPlanCode) b on a.BatchPlanNum=b.BatchPlanNum
group by a.ThisTime,b.BatchPlanNum) Tb group by Tb.BatchPlanNum) gh on a.BatchPlanNum=gh.BatchPlanNum
                            LEFT JOIN TbCompany b ON a.BranchCode=b.CompanyCode
                            LEFT JOIN TbCompany c ON a.WorkAreaCode=c.CompanyCode
                            LEFT JOIN TbCompany d ON a.ProcessFactoryCode=d.CompanyCode
                            LEFT JOIN TbUser e ON a.InsertUserCode=e.UserCode
                            LEFT JOIN TbUser f ON a.Acceptor=f.UserCode
                            LEFT JOIN TbSysDictionaryData g ON a.SteelsTypeCode=g.DictionaryCode
                            LEFT JOIN
                             (
                             	SELECT ISNULL(SUM(bhgData.PassCount),0) AS UnqualifiedTotal,BatchPlanNum FROM TbFactoryBatchNeedPlanItem tfbnpi
                               LEFT JOIN(
                              SELECT tioi.PassCount,tioi.BatchPlanItemId
                              FROM TbInOrderItem tioi WHERE tioi.ChackState=3
                              ) bhgData ON tfbnpi.ID=bhgData.BatchPlanItemId
                               GROUP BY tfbnpi.BatchPlanNum
                             ) h ON a.BatchPlanNum=h.BatchPlanNum
							left join TbRawMaterialMonthDemandSupplyPlan i on a.RawMaterialDemandNum=i.SupplyPlanCode
";

            try
            {
                var data = Repository<TbSupplyList>.FromSqlToPageTable(sql + where, parameter, entity.rows, entity.page, "BatchPlanNum", "desc");
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// 
        /// 供应清单 实体=加工厂批次需求计划 导出
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        /// 
        public DataTable GetExportList(TSupplyListRequest entity)
        {
            //组装查询语句
            #region 模糊搜索条件
            List<Parameter> parameter = new List<Parameter>();
            string where = " where 1=1";
            if (!string.IsNullOrWhiteSpace(entity.ProcessFactoryCode))
            {
                where += " and a.ProcessFactoryCode=@ProcessFactoryCode";
                parameter.Add(new Parameter("@ProcessFactoryCode", entity.ProcessFactoryCode, DbType.String, null));
            }
            if (!string.IsNullOrWhiteSpace(entity.SiteCode))
            {
                List<string> SiteList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(entity.SiteCode, 5);//站点
                List<string> WorkAreaList = _workOrderLogic.GetCompanyWorkAreaOrSiteList(entity.SiteCode, 4);//工区
                where += " and (a.SiteCode in('" + string.Join("','", SiteList) + "') or a.WorkAreaCode in('" + string.Join("','", WorkAreaList) + "'))";
            }
            if (!string.IsNullOrWhiteSpace(entity.ProjectId))
            {
                where += " and a.ProjectId=@ProjectId";
                parameter.Add(new Parameter("@ProjectId", entity.ProjectId, DbType.String, null));
            }
            if (!string.IsNullOrWhiteSpace(entity.BatchPlanNum))
            {
                where += " and a.BatchPlanNum=@BatchPlanNum";
                parameter.Add(new Parameter("@BatchPlanNum", entity.BatchPlanNum, DbType.String, null));
            }
            if (entity.HistoryMonth.HasValue)
            {
                where += " and YEAR(a.InsertTime)=" + entity.HistoryMonth.Value.Year + " and MONTH(a.InsertTime)=" + entity.HistoryMonth.Value.Month;
            }
            if (!string.IsNullOrWhiteSpace(entity.StateCode))
            {
                if (entity.StateCode == "供货超时")
                    where += " and a.SupplyDate<GETDATE() and a.StateCode='未供货'";
                else if (entity.StateCode == "未供货")
                    where += " and a.SupplyDate>=GETDATE() and a.StateCode='未供货'";
                else
                    where += " and a.StateCode='" + entity.StateCode + "'";
            }
            //排序
            where += " order by BatchPlanNum desc";

            #endregion

            string sql = @"SELECT 
                            a.*,
                            b.CompanyFullName AS BranchName,
                            c.CompanyFullName AS WorkAreaName,
                            d.CompanyFullName AS ProcessFactoryName,
                            e.UserName,
                            f.UserName AS AcceptorName,
                            g.DictionaryText AS RebarTypeNew,
                            h.UnqualifiedTotal
                            FROM 
                            TbSupplyList a
                            LEFT JOIN TbCompany b ON a.BranchCode=b.CompanyCode
                            LEFT JOIN TbCompany c ON a.WorkAreaCode=c.CompanyCode
                            LEFT JOIN TbCompany d ON a.ProcessFactoryCode=d.CompanyCode
                            LEFT JOIN TbUser e ON a.InsertUserCode=e.UserCode
                            LEFT JOIN TbUser f ON a.Acceptor=f.UserCode
                            LEFT JOIN TbSysDictionaryData g ON a.SteelsTypeCode=g.DictionaryCode
                            LEFT JOIN
                             (
                             	SELECT ISNULL(SUM(bhgData.PassCount),0) AS UnqualifiedTotal,BatchPlanNum FROM TbFactoryBatchNeedPlanItem tfbnpi
                               LEFT JOIN(
                              SELECT tioi.PassCount,tioi.BatchPlanItemId
                              FROM TbInOrderItem tioi WHERE tioi.ChackState=3
                              ) bhgData ON tfbnpi.ID=bhgData.BatchPlanItemId
                               GROUP BY tfbnpi.BatchPlanNum
                             ) h ON a.BatchPlanNum=h.BatchPlanNum";

            try
            {
                var data = Db.Context.FromSql(sql + where)
                .AddInParameter("ProcessFactoryCode", DbType.String, entity.ProcessFactoryCode)
                .AddInParameter("ProjectId", DbType.String, entity.ProjectId)
                .AddInParameter("BatchPlanNum", DbType.String, entity.BatchPlanNum).ToDataTable();
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion


        /// <summary>
        /// 以ID查询供应清单
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>

        #region 以ID查询供应清单

        public Tuple<object, DataTable> GetFormJson(int keyValue)
        {
            var ret = Repository<TbSupplyList>.First(p => p.ID == keyValue);
            string sql1 = @"select a.ID,a.RawMaterialNum,a.MaterialName,a.Standard,a.MeasurementUnit,c.DictionaryText as MeasurementUnitText,a.BatchPlanQuantity,a.TechnicalRequirement,a.Remarks,a.HasSupplier,a.BatchPlanNum,a.BatchPlanItemId from TbSupplyListDetail a
left join TbSupplyList b on a.BatchPlanNum=b.BatchPlanNum 
left join TbSysDictionaryData c on a.MeasurementUnit=c.DictionaryCode where b.ID=" + keyValue + "";
            string sql2 = @"select a.ID,a.RawMaterialNum,a.MaterialName,a.Standard,a.MeasurementUnit,c.DictionaryText as MeasurementUnitText,a.BatchPlanQuantity,a.TechnicalRequirement,a.Remarks,a.HasSupplier,a.BatchPlanNum,a.BatchPlanItemId,b.ID as HistoryId,b.ThisTime,b.ThisTimeCount,b.BatchPlanItemNewCode,2 as GhType,e.BatchPlanItemNewCode as InorderId,e.PassCount,e.NoPass,e.NoPassReason,case when f.ChackState=2 then e.PassCount else 0 end Unqualified,f.Enclosure,CONVERT(varchar(100),e.ReturnTime,23) as ReturnTime from TbSupplyListDetail a
left join  TbSupplyListDetailHistory b on a.BatchPlanItemId=b.BatchPlanItemId
left join TbSysDictionaryData c on a.MeasurementUnit=c.DictionaryCode
left join TbSupplyList d on a.BatchPlanNum=d.BatchPlanNum
left join TbInOrderItem e on b.BatchPlanItemNewCode=e.BatchPlanItemNewCode 
left join TbSampleOrderItem f on e.ID=f.InOrderItemId where d.ID=" + keyValue + " and b.ID is not null";
            var items1 = Db.Context.FromSql(sql1).ToDataTable();
            var items2 = Db.Context.FromSql(sql2).ToDataTable();
            for (int i = 0; i < items1.Rows.Count; i++)
            {
                DataRow newRow = items2.NewRow();
                newRow["ID"] = items1.Rows[i]["ID"];
                newRow["RawMaterialNum"] = items1.Rows[i]["RawMaterialNum"];
                newRow["MaterialName"] = items1.Rows[i]["MaterialName"];
                newRow["Standard"] = items1.Rows[i]["Standard"];
                newRow["MeasurementUnit"] = items1.Rows[i]["MeasurementUnit"];
                newRow["MeasurementUnitText"] = items1.Rows[i]["MeasurementUnitText"];
                newRow["BatchPlanQuantity"] = items1.Rows[i]["BatchPlanQuantity"];
                newRow["TechnicalRequirement"] = items1.Rows[i]["TechnicalRequirement"];
                newRow["Remarks"] = items1.Rows[i]["Remarks"];
                newRow["HasSupplier"] = items1.Rows[i]["HasSupplier"];
                newRow["BatchPlanNum"] = items1.Rows[i]["BatchPlanNum"];
                newRow["HistoryId"] = 0;
                newRow["BatchPlanItemId"] = items1.Rows[i]["BatchPlanItemId"];
                newRow["ThisTime"] = DateTime.Now.ToString("yyyy-MM-dd");
                newRow["ThisTimeCount"] = 0;
                newRow["GhType"] = 1;
                newRow["BatchPlanItemNewCode"] = "";
                newRow["InorderId"] = "";
                newRow["PassCount"] = 0;
                items2.Rows.Add(newRow);
            }
            items2.DefaultView.Sort = "ID asc,GhType asc,ThisTime desc";
            items2 = items2.DefaultView.ToTable();
            //查找明细信息
            return new Tuple<object, DataTable>(ret, items2);
        }

        #endregion

        #region 修改数据

        /// <summary>
        /// 修改数据
        /// </summary>
        public AjaxResult Update(TbSupplyList model, List<TbSupplyListDetail> items, List<TbSupplyListDetailHistory> items2)
        {
            using (DbTrans trans = Db.Context.BeginTransaction())//使用事务
            {
                try
                {
                    var modelNew = Repository<TbSupplyList>.First(p => p.ID == model.ID);
                    modelNew.HasSupplierTotal = model.HasSupplierTotal;
                    int count = 0;
                    if (items.Count > 0)
                    {
                        for (int i = 0; i < items.Count; i++)
                        {
                            if (items[i].HasSupplier >= 0 && items[i].HasSupplier < items[i].BatchPlanQuantity)
                            {
                                count += 1;
                            }
                        }
                    }
                    if (count == 0)
                    {
                        model.StateCode = "已供货";
                        model.SupplyCompleteTime = DateTime.Now;//供货完成时间
                        Repository<TbSupplyList>.Update(trans, model, p => p.ID == model.ID);
                        if (items != null && items.Count > 0)
                        {
                            Repository<TbSupplyListDetail>.Delete(trans, p => p.BatchPlanNum == items[0].BatchPlanNum);
                            //添加明细信息
                            Repository<TbSupplyListDetail>.Insert(trans, items);
                            Repository<TbSupplyListDetailHistory>.Delete(trans, p => p.BatchPlanNum == items[0].BatchPlanNum);
                            //添加明细信息
                            Repository<TbSupplyListDetailHistory>.Insert(trans, items2);
                        }
                        //SendMsg(modelNew, trans);
                    }
                    if (count > 0)
                    {
                        model.StateCode = "部分供货";
                        Repository<TbSupplyList>.Update(trans, model, p => p.ID == model.ID);
                        if (items != null && items.Count > 0)
                        {
                            Repository<TbSupplyListDetail>.Delete(trans, p => p.BatchPlanNum == items[0].BatchPlanNum);
                            //添加明细信息
                            Repository<TbSupplyListDetail>.Insert(trans, items);
                            Repository<TbSupplyListDetailHistory>.Delete(trans, p => p.BatchPlanNum == items[0].BatchPlanNum);
                            //添加明细信息
                            Repository<TbSupplyListDetailHistory>.Insert(trans, items2);
                        }
                        //SendMsg(modelNew, trans);
                    }
                    trans.Commit();//提交事务
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return AjaxResult.Error(ex.ToString());
                }
                finally
                {
                    trans.Close();
                }
                return AjaxResult.Success();
            }
        }
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="model"></param>
//        public void SendMsg(TbSupplyList model, DbTrans trans)
//        {
//            try
//            {
//                List<TbFlowPerformMessage> myMsgList = new List<TbFlowPerformMessage>();//'我的消息'推送
//                //供货消息提醒
//                //查找消息模板信息
//                var shortMessageTemplateModel = Repository<TbShortMessageTemplate>.First(p => p.TemplateCode == "MB0012");
//                if (shortMessageTemplateModel != null)
//                {
//                    string sql = @"select tb3.PersonnelSource,tb3.PersonnelCode from TbFormEarlyWarningBegTime tb1
//left join TbFormEarlyWarningNode tb2 on tb1.MenuCode=tb2.MenuCode and tb1.EarlyWarningCode=tb2.EarlyWarningCode
//left join TbFormEarlyWarningNodePersonnel tb3 on tb2.EarlyWarningCode=tb3.EarlyWarningCode and tb2.EWNodeCode=tb3.EWNodeCode and tb2.MenuCode=tb3.MenuCode
//left join TbCompany tb4 on tb1.BranchCode=tb4.CompanyCode
//left join TbCompany tb5 on tb1.WorkAreaCode=tb5.CompanyCode 
//where tb1.MenuCode='SupplyList' and tb1.MsgType=2 
//and tb1.ProcessFactoryCode='" + model.ProcessFactoryCode + "'";
//                    var xxtxitems = Db.Context.FromSql(sql).ToDataTable();
//                    if (xxtxitems.Rows.Count > 0)
//                    {
//                        //获取分部、工区
//                        var Branch = Repository<TbCompany>.First(p => p.CompanyCode == model.BranchCode);
//                        var WorkArea = Repository<TbCompany>.First(p => p.CompanyCode == model.WorkAreaCode);
//                        for (int i = 0; i < xxtxitems.Rows.Count; i++)
//                        {
//                            var content = shortMessageTemplateModel.TemplateContent;
//                            var s = content.Replace("变量：分部/工区", Branch.CompanyFullName + "/" + WorkArea.CompanyFullName);
//                            var a = s.Replace("变量：批次计划编号", model.BatchPlanNum);
//                            var b = a.Replace("变量：已供应总量（kg）", model.HasSupplierTotal + "（kg）");
//                            //'我的消息'推送
//                            var myMsg = new TbFlowPerformMessage()
//                            {
//                                messageID = Guid.NewGuid().ToString(),
//                                messageCreateTime = DateTime.Now,
//                                messageType = 12,
//                                messageTitle = "【" + model.BatchPlanNum + "】供货提醒",
//                                messageContent = b,
//                                IsRead = -1,
//                                UserCode = xxtxitems.Rows[i]["PersonnelCode"].ToString(),
//                                MsgType = "2"
//                            };
//                            myMsgList.Add(myMsg);
//                        }
//                    }
//                    if (myMsgList.Any())
//                    {
//                        //'我的消息'推送
//                        Repository<TbFlowPerformMessage>.Insert(trans, myMsgList);
//                    }
//                }
//            }
//            catch (Exception)
//            {

//                throw;
//            }

//        }

        #endregion

    }


}





