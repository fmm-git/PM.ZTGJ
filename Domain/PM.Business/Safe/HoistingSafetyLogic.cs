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
    /// 吊装安全
    /// </summary>
    public class HoistingSafetyLogic
    {

        #region 新增数据

        /// <summary>
        /// 新增数据
        /// </summary>
        public AjaxResult Insert(TbHoistingSafety model)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                model.Examinestatus = "未发起";
                model.InsertUserCode = OperatorProvider.Provider.CurrentUser.UserCode;
                var count = Repository<TbHoistingSafety>.Insert(model);
                if (count > 0)
                {
                    return AjaxResult.Success();
                }
                return AjaxResult.Error("操作失败");
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
        public AjaxResult Update(TbHoistingSafety model)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                var count = Repository<TbHoistingSafety>.Update(model, p => p.ID == model.ID);
                if (count > 0)
                    return AjaxResult.Success();
                return AjaxResult.Error("操作失败");
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
        public AjaxResult Delete(int dataID)
        {
            try
            {
                var count = Repository<TbHoistingSafety>.Delete(p => p.ID == dataID);
                if (count > 0)
                    return AjaxResult.Success();
                return AjaxResult.Error("操作失败");
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
        public Tuple<DataTable> FindEntity(int dataID)
        {
            var ret = Db.Context.From<TbHoistingSafety>()
               .Select(
                       TbHoistingSafety._.All,
                       TbCompany._.CompanyFullName.As("ProcessFactoryName")
                       , TbSysDictionaryData._.DictionaryText.As("TzWorkZgzNew")
                       , TbUser._.UserName
                       , TbDataManage._.DataName.As("HoistFileName"))
                       .LeftJoin<TbCompany>((a, c) => a.ProcessFactoryCode == c.CompanyCode)
                       .LeftJoin<TbSysDictionaryData>((a, c) => a.TzWorkZgz == c.DictionaryCode && c.FDictionaryCode == "TzWorkZgz")
                       .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                       .LeftJoin<TbDataManage>((a, c) => a.HoistFileCode == c.FileCode && a.ProcessFactoryCode == c.ProcessFactoryCode)
                       .AddSelect(Db.Context.From<TbSysDictionaryData>().Select(p => p.DictionaryText)
                       .Where(TbSysDictionaryData._.DictionaryCode == TbHoistingSafety._.MechanicsIsNormal && TbSysDictionaryData._.FDictionaryCode == "MechanicsIsNormal"), "MechanicsIsNormalNew")
                       .AddSelect(Db.Context.From<TbSysDictionaryData>().Select(p => p.DictionaryText)
                       .Where(TbSysDictionaryData._.DictionaryCode == TbHoistingSafety._.KzIsSolid && TbSysDictionaryData._.FDictionaryCode == "KzIsSolid"), "KzIsSolidNew")
                       .AddSelect(Db.Context.From<TbSysDictionaryData>().Select(p => p.DictionaryText)
                       .Where(TbSysDictionaryData._.DictionaryCode == TbHoistingSafety._.IsProhibition && TbSysDictionaryData._.FDictionaryCode == "IsProhibition"), "IsProhibitionNew")
                        .AddSelect(Db.Context.From<TbUser>().Select(p => p.UserName)
                       .Where(TbUser._.UserCode == TbHoistingSafety._.SuperviseUser), "SuperviseUserName")
                       .Where(p => p.ID == dataID).ToDataTable();
             return new Tuple<DataTable>(ret);
        }

        /// <summary>
        /// 获取数据列表(分页)
        /// </summary>
        public PageModel GetDataListForPage(HoistingSafetyRequest request)
        {
            //组装查询语句
            #region 模糊搜索条件

            var where = new Where<TbHoistingSafety>();
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where.And(p => p.ProcessFactoryCode==request.ProcessFactoryCode);
            }
            if (!string.IsNullOrWhiteSpace(request.SuperviseUser))
            {
                where.And(p => p.SuperviseUser.Like(request.SuperviseUser));
            }
            //if (!string.IsNullOrEmpty(request.ProjectId))
            //    where.And(p => p.ProjectId == request.ProjectId);

            #endregion


            try
            {
                var ret = Db.Context.From<TbHoistingSafety>()
              .Select(
                      TbHoistingSafety._.All,
                      TbCompany._.CompanyFullName.As("ProcessFactoryName")
                      , TbSysDictionaryData._.DictionaryText.As("TzWorkZgzNew")
                      ,TbUser._.UserName
                      ,TbDataManage._.DataName.As("HoistFileName"))
                      .LeftJoin<TbCompany>((a, c) => a.ProcessFactoryCode == c.CompanyCode)
                      .LeftJoin<TbSysDictionaryData>((a, c) => a.TzWorkZgz == c.DictionaryCode && c.FDictionaryCode == "TzWorkZgz")
                      .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                      .LeftJoin<TbDataManage>((a, c) => a.HoistFileCode == c.FileCode&&a.ProcessFactoryCode==c.ProcessFactoryCode)
                      .AddSelect(Db.Context.From<TbSysDictionaryData>().Select(p => p.DictionaryText)
                      .Where(TbSysDictionaryData._.DictionaryCode == TbHoistingSafety._.MechanicsIsNormal && TbSysDictionaryData._.FDictionaryCode == "MechanicsIsNormal"), "MechanicsIsNormalNew")
                      .AddSelect(Db.Context.From<TbSysDictionaryData>().Select(p => p.DictionaryText)
                      .Where(TbSysDictionaryData._.DictionaryCode == TbHoistingSafety._.KzIsSolid && TbSysDictionaryData._.FDictionaryCode == "KzIsSolid"), "KzIsSolidNew")
                      .AddSelect(Db.Context.From<TbSysDictionaryData>().Select(p => p.DictionaryText)
                      .Where(TbSysDictionaryData._.DictionaryCode == TbHoistingSafety._.IsProhibition && TbSysDictionaryData._.FDictionaryCode == "IsProhibition"), "IsProhibitionNew")
                       .AddSelect(Db.Context.From<TbUser>().Select(p => p.UserName)
                      .Where(TbUser._.UserCode == TbHoistingSafety._.SuperviseUser), "SuperviseUserName")
                      .Where(where).OrderBy(d => d.ID).ToPageList(request.rows, request.page);
                return ret;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region 验证

        /// <summary>
        /// 判断信息是否可操作
        /// </summary>
        /// <returns></returns>
        public AjaxResult AnyInfo(int keyValue)
        {
            var hoistingSafety = Repository<TbHoistingSafety>.First(p => p.ID == keyValue);
            if (hoistingSafety == null)
                return AjaxResult.Warning("信息不存在");
            if (hoistingSafety.Examinestatus != "未发起" && hoistingSafety.Examinestatus != "已退回")
                return AjaxResult.Warning("信息正在审核中或已审核完成,不能进行此操作");

            return AjaxResult.Success(hoistingSafety);
        }
        #endregion
    }
}
