using Dos.ORM;
using PM.Business.Production;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Business.Distribution
{
    /// <summary>
    /// 运输过程
    /// </summary>
    public class TransportProcessLogic
    {

        private readonly TbWorkOrderLogic _workOrderLogic = new TbWorkOrderLogic();
        /// <summary>
        /// 获取数据列表(车辆配送列表)
        /// </summary>
        public PageModel GetDataListForPage(TransportProcessRequest request, bool IsDriver, string UserCode)
        {
            //var where = new Where<TbDistributionEnt>();
            //if (!string.IsNullOrEmpty(request.ProjectId))
            //    where.And(p => p.ProjectId == request.ProjectId);
            //if (!string.IsNullOrEmpty(request.DistributionCode))
            //    where.And(p => p.DistributionCode.Like(request.DistributionCode));
            //where.And(p => p.FlowState != 6);
            //try
            //{
            //    var data = Db.Context.From<TbDistributionEnt>()
            //        .Select(
            //          TbDistributionEnt._.DistributionCode
            //        , TbDistributionEnt._.FlowState
            //        , TbCarInfo._.CarCph.As("VehicleCode")
            //        , TbCarInfoDetail._.UserName.As("Driver")
            //        , TbCarInfoDetail._.Tel)
            //      .LeftJoin<TbCarInfo>((a, c) => a.VehicleCode == c.CarCode)
            //      .LeftJoin<TbCarInfoDetail>((a, c) => a.Driver == c.UserCode && a.VehicleCode == c.CarCode)
            //      .Where(where)
            //      .OrderByDescending(p => p.ID)
            //      .ToPageList(request.rows, request.page);
            //    return data;
            //}
            //catch (Exception)
            //{
            //    throw;
            //}
            string where = " where 1=1 and Tb.FlowState<5 and Tb.FlowState>0 ";
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
            if (IsDriver == true)//是司机登录
            {
                where += " and Tb.Driver='" + UserCode + "'";
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

            string sql = @"select Tb.*,cp.CompanyFullName as SiteName from (select disEnt.ProjectId,disEnt.ProcessFactoryCode,case when disEntOrder.ID is null then disEnt.ID else disEntOrder.ID end as DisEntOrderId,disEnt.DistributionCode,case when disEntOrder.SiteCode is null then disEnt.SiteCode else disEntOrder.SiteCode end SiteCode,case when disEntOrder.OrderCode is null then disEnt.OrderCode else disEntOrder.OrderCode end OrderCode,disEnt.VehicleCode,car.CarCph as CarNumber,disEnt.Driver,ur.UserName as DriverName,case when disEntOrder.OrderFlowState=1 then disEnt.FlowState when disEntOrder.OrderFlowState is null then disEnt.FlowState else disEntOrder.OrderFlowState end  as FlowState from TbDistributionEnt disEnt
                               left join TbDistributionEntOrder disEntOrder on disEnt.DistributionCode=disEntOrder.DistributionCode
                               left join TbUser ur on disEnt.Driver=ur.UserCode
                               left join TbCarInfo car on  disEnt.VehicleCode=car.CarCode) Tb
                               left join TbCompany cp on Tb.SiteCode=cp.CompanyCode ";

            //参数化
            List<Parameter> parameter = new List<Parameter>();
            var model = Repository<TbDistributionPlanInfo>.FromSqlToPageTable(sql+where, parameter, request.rows, request.page, "DistributionCode", "desc");
            return model;
        }

        /// <summary>
        /// 获取车辆配送路线信息
        /// </summary>
        /// <param name="distributionCode">配送装车单号</param>
        /// <returns></returns>
        public List<TbTransportLine> GetTransportLine(string distributionCode)
        {
            var transportLine = Repository<TbTransportLine>.Query(p => p.DistributionCode == distributionCode).OrderBy(p => p.ID).ToList();
            return transportLine;
        }

        /// <summary>
        /// 新增数据(车辆配送路线)
        /// </summary>
        public AjaxResult Insert(TbTransportLine model)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            model.InsertTime = DateTime.Now;
            try
            {
                //添加信息
                Repository<TbTransportLine>.Insert(model, true);
                return AjaxResult.Success();
            }
            catch (Exception ex)
            {
                return AjaxResult.Error();
            }
        }
    }
}
