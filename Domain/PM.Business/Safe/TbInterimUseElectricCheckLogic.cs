using Dos.ORM;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using PM.DataEntity.Safe.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Business.Safe
{
    /// <summary>
    /// 临时用电检查
    /// </summary>
    public class TbInterimUseElectricCheckLogic
    {
        #region 获取列表
        /// <summary>
        /// 获取数据列表(分页)
        /// </summary>
        public PageModel GetDataListForPage(InterimUseElectricCheckRequest request)
        {

            #region 模糊搜索条件

            var where = new Where<TbInterimUseElectricCheck>();
            if (!string.IsNullOrWhiteSpace(request.CheckCode))
            {
                where.And(d => d.CheckCode == request.CheckCode);
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where.And(d => d.ProcessFactoryCode == request.ProcessFactoryCode);
            }
            //if (!string.IsNullOrWhiteSpace(request.ProjectId))
            //{
            //    where.And(d => d.ProjectId == request.ProjectId);
            //}
            if (request.CheckTimeS.HasValue)
            {
                where.And(d => d.CheckDate >= request.CheckTimeS);
            }
            if (request.CheckTimeE.HasValue)
            {
                where.And(d => d.CheckDate <= request.CheckTimeE);
            }
            #endregion

            try
            {
                var data = Db.Context.From<TbInterimUseElectricCheck>()
                    .Select(
                      TbInterimUseElectricCheck._.All
                    , TbCompany._.CompanyFullName.As("ProcessFactoryName")
                    , TbUser._.UserName)
                  .LeftJoin<TbCompany>((a, c) => a.ProcessFactoryCode == c.CompanyCode)
                  .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                  .Where(where)
                  .OrderByDescending(TbInterimUseElectricCheck._.CheckCode)
                  .ToPageList(request.rows, request.page);
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 获取数据(编辑、查看)
        /// </summary>
        /// <param name="keyValue">数据Id</param>
        /// <returns></returns>
        public TbInterimUseElectricCheck FindEntity(int keyValue)
        {
            var ret = Db.Context.From<TbInterimUseElectricCheck>()
                .Select(
                       TbInterimUseElectricCheck._.All
                    , TbCompany._.CompanyFullName.As("ProcessFactoryName")
                    ,TbUser._.UserName.As("UserName"))
                  .LeftJoin<TbCompany>((a, c) => a.ProcessFactoryCode == c.CompanyCode)
                  .LeftJoin<TbUser>((a,c)=>a.InsertUserCode==c.UserCode)
                  .Where(p => p.ID == keyValue).First();
            return ret;
        }

        #endregion

        #region 新增数据

        /// <summary>
        /// 新增数据
        /// </summary>
        public AjaxResult Insert(TbInterimUseElectricCheck model, bool isApi = false)
        {
            TbFireInspectionLogic filogic = new TbFireInspectionLogic();
            if (model == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                ////保存附件
                //string str = filogic.UploadFile(model.Enclosure, model.InsertUserCode);
                //model.Enclosure = str.TrimEnd(',');
                //添加信息
                Repository<TbInterimUseElectricCheck>.Insert(model, isApi);
                return AjaxResult.Success();
            }
            catch (Exception ex)
            {
                return AjaxResult.Error();
            }
        }

        #endregion

        #region 修改数据

        /// <summary>
        /// 修改数据
        /// </summary>
        public AjaxResult Update(TbInterimUseElectricCheck model, bool isApi = false)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            var anyRet = AnyInfo(model.ID);
            if (anyRet.state.ToString() != ResultType.success.ToString())
                return anyRet;
            try
            {
                Repository<TbInterimUseElectricCheck>.Update(model, p => p.ID == model.ID, isApi);
                return AjaxResult.Success();
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
        public AjaxResult Delete(int keyValue, bool isApi = false)
        {
            try
            {
                //判断信息是否存在
                var anyRet = AnyInfo(keyValue);
                if (anyRet.state.ToString() != ResultType.success.ToString())
                    return anyRet;
                //删除信息
                var count = Repository<TbInterimUseElectricCheck>.Delete(p => p.ID == keyValue, isApi);
                return AjaxResult.Success();
            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.ToString());
            }
        }

        #endregion

        /// <summary>
        /// 判断信息是否可操作
        /// </summary>
        /// <returns></returns>
        public AjaxResult AnyInfo(int keyValue)
        {
            var article = Repository<TbInterimUseElectricCheck>.First(p => p.ID == keyValue);
            if (article == null)
                return AjaxResult.Warning("信息不存在");
            return AjaxResult.Success(article);
        }

        #region 导出
        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public DataTable GetExportList(InterimUseElectricCheckRequest request)
        {

            #region 模糊搜索条件

            var where = new Where<TbInterimUseElectricCheck>();
            if (!string.IsNullOrWhiteSpace(request.CheckCode))
            {
                where.And(d => d.CheckCode == request.CheckCode);
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where.And(d => d.ProcessFactoryCode == request.ProcessFactoryCode);
            }
            if (request.CheckTimeS.HasValue)
            {
                where.And(d => d.CheckDate >= request.CheckTimeS);
            }
            if (request.CheckTimeE.HasValue)
            {
                where.And(d => d.CheckDate <= request.CheckTimeE);
            }
            #endregion

            try
            {
                var data = Db.Context.From<TbInterimUseElectricCheck>()
                    .Select(
                      TbInterimUseElectricCheck._.All
                    , TbCompany._.CompanyFullName.As("ProcessFactoryName")
                    , TbUser._.UserName)
                  .LeftJoin<TbCompany>((a, c) => a.ProcessFactoryCode == c.CompanyCode)
                  .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                  .Where(where)
                  .OrderByDescending(TbInterimUseElectricCheck._.CheckCode).ToDataTable();
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion
    }
}
