using Dos.ORM;
using Newtonsoft.Json.Linq;
using PM.Business.BIExhibition;
using PM.Common;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace PM.Web.WebApi.BIExhibition
{
    /// <summary>
    /// 质量指标
    /// </summary>
    public class QualityIndexController : ApiController
    {
        /// <summary>
        /// 质量指标 处理类
        /// </summary>
        public readonly QualityIndexLogic _qiLogic = new QualityIndexLogic();
        //
        // GET: /QualityIndex/

        /// <summary>
        /// 加工厂当月原材料到货分析
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage JGCYCLArrival(string OrgType, string ProjectId, string HistoryMonth1)
        {
            try
            {
                var data = _qiLogic.JGCYCLArrival(OrgType, ProjectId, HistoryMonth1);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
           
        }
        /// <summary>
        /// 当月原材料到货合格数量分析
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage YCLArrivalQualified(string OrgType, string ProjectId)
        {
            try
            {
                var data = _qiLogic.YCLArrivalQualified(OrgType, ProjectId);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }

        }
        /// <summary>
        /// 加工厂当月配送到站各类问题次数统计
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage JGCPSXHAnalysis(string OrgType, string ProjectId, string HistoryMonth2)
        {
            try
            {
                var data = _qiLogic.JGCPSXHAnalysis(OrgType, ProjectId, HistoryMonth2);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }

        }
        /// <summary>
        /// 加工厂当月配送到站问题明细
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetPsDzWtMxList(string Jgc, string PsType, string OrgType, string ProjectId)
        {
            try
            {
                string HistoryMonth2 = "";
                var data = _qiLogic.GetPsDzWtMxList(Jgc, PsType, OrgType, ProjectId, HistoryMonth2);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }

        }
        /// <summary>
        /// 一号加工厂历史订单分析
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetOneJgc(string OrgType, string ProjectId)
        {
            try
            {
                var data = _qiLogic.GetOneJgc(OrgType, ProjectId);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }
        /// <summary>
        /// 二号加工厂历史订单分析
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetTwoJgc(string OrgType, string ProjectId)
        {
            try
            {
                var data = _qiLogic.GetTwoJgc(OrgType, ProjectId);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 三号加工厂历史订单分析
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetThreeJgc(string OrgType, string ProjectId)
        {
            try
            {
                var data = _qiLogic.GetThreeJgc(OrgType, ProjectId);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }
    }
}
