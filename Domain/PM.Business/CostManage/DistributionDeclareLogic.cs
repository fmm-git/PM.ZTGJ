using Dos.ORM;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using PM.DataEntity.CostManage.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Business.CostManage
{
    /// <summary>
    /// 配送费用申报
    /// </summary>
    public class DistributionDeclareLogic
    {

        #region 新增数据

        /// <summary>
        /// 新增数据
        /// </summary>
        public AjaxResult Insert(TbDistributionDeclare model, List<TbDistributionDeclareItem> items)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            model.InsertUserCode = OperatorProvider.Provider.CurrentUser.UserCode;
            model.Examinestatus = "未发起";
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //添加信息
                    Repository<TbDistributionDeclare>.Insert(trans, model);
                    //添加明细信息
                    Repository<TbDistributionDeclareItem>.Insert(trans, items);
                    trans.Commit();
                    return AjaxResult.Success();
                }
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
        public AjaxResult Update(TbDistributionDeclare model, List<TbDistributionDeclareItem> items)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            var anyRet = AnyInfo(model.ID);
            if (anyRet.state != ResultType.success.ToString())
                return anyRet;
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())//使用事务
                {
                    //修改信息
                    Repository<TbDistributionDeclare>.Update(trans, model, p => p.ID == model.ID);
                    if (items.Count > 0)
                    {
                        //删除历史明细信息
                        Repository<TbDistributionDeclareItem>.Delete(trans, p => p.DistributionDeclareCode == model.DistributionDeclareCode);
                        //添加明细信息
                        Repository<TbDistributionDeclareItem>.Insert(trans, items);
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
                if (anyRet.state != ResultType.success.ToString())
                    return anyRet;
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //删除信息
                    var count = Repository<TbDistributionDeclare>.Delete(trans, p => p.ID == keyValue);
                    //删除明细信息
                    Repository<TbDistributionDeclareItem>.Delete(trans, p => p.DistributionDeclareCode == ((TbDistributionDeclare)anyRet.data).DistributionDeclareCode);
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

        #region 获取数据

        /// <summary>
        /// 获取编辑数据
        /// </summary>
        /// <param name="keyValue">数据Id</param>
        /// <returns></returns>
        public Tuple<object, object> FindEntity(int keyValue)
        {
            var ret = Db.Context.From<TbDistributionDeclare>()
                .Select(
                       TbDistributionDeclare._.All
                    , TbUser._.UserName
                    , TbCompany._.CompanyFullName.As("ProcessFactoryName"))
                    .AddSelect(Db.Context.From<TbUser>()
                      .Select(p => p.UserName)
                      .Where(TbUser._.UserCode == TbDistributionDeclare._.DeclareUserCode), "DeclareUserName")
                  .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                  .LeftJoin<TbCompany>((a, c) => a.ProcessFactoryCode == c.CompanyCode)
                  .Where(p => p.ID == keyValue).ToDataTable();
            if (ret == null || ret.Rows.Count == 0)
                return new Tuple<object, object>(null, null);
            //查找明细信息
            var items = Db.Context.From<TbDistributionDeclareItem>().Select(
                TbDistributionDeclareItem._.All,
                TbCompany._.CompanyFullName.As("SiteName"))
                .LeftJoin<TbCompany>((a,c)=>a.SiteCode==c.CompanyCode)
            .Where(p => p.DistributionDeclareCode == ret.Rows[0]["DistributionDeclareCode"])
            .ToDataTable();
            return new Tuple<object, object>(ret, items);
        }

        /// <summary>
        /// 获取数据列表(分页)
        /// </summary>
        public PageModel GetDataListForPage(DistributionDeclareRequest request)
        {

            #region 模糊搜索条件
            var where = new Where<TbDistributionDeclare>();
            if (!string.IsNullOrWhiteSpace(request.DistributionDeclareCode))
            {
                where.And(d => d.DistributionDeclareCode == request.DistributionDeclareCode);
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where.And(d => d.ProcessFactoryCode == request.ProcessFactoryCode);
            }
            #endregion

            #region 数据权限

            //数据权限
            var authorizaModel = new AuthorizeLogic().CheckAuthoriza(new AuthorizationParameterModel("DistributionDeclare"));
            if (authorizaModel.IsAuthorize)
            {
                if (authorizaModel.Ids.Count > 0 && authorizaModel.UserCodes.Count > 0)
                    where.And(d => d.InsertUserCode.In(authorizaModel.UserCodes) || d.ID.In(authorizaModel.Ids));
                else if (authorizaModel.Ids.Count > 0)
                    where.Or(d => d.ID.In(authorizaModel.Ids));
                else if (authorizaModel.UserCodes.Count > 0)
                    where.And(d => d.InsertUserCode.In(authorizaModel.UserCodes));
            }
            if (!string.IsNullOrEmpty(request.ProjectId))
                where.And(p => p.ProjectId == request.ProjectId);
            #endregion

            try
            {
                var data = Db.Context.From<TbDistributionDeclare>()
                    .Select(
                      TbDistributionDeclare._.ID
                    , TbDistributionDeclare._.Examinestatus
                    , TbDistributionDeclare._.DistributionDeclareCode
                    , TbDistributionDeclare._.DeclareTime
                    , TbDistributionDeclare._.TotalAmount
                    , TbDistributionDeclare._.InsertUserCode
                    , TbDistributionDeclare._.InsertTime
                    , TbDistributionDeclare._.ProjectId
                    , TbUser._.UserName
                    , TbCompany._.CompanyFullName.As("ProcessFactoryName"))
                    .AddSelect(Db.Context.From<TbUser>()
                      .Select(p => p.UserName)
                      .Where(TbUser._.UserCode == TbDistributionDeclare._.DeclareUserCode), "DeclareUserName")
                  .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                  .LeftJoin<TbCompany>((a, c) => a.ProcessFactoryCode == c.CompanyCode)
                  .Where(where)
                  .OrderByDescending(p => p.ID)
                  .ToPageList(request.rows, request.page);
                return data;
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
            var inOrder = Repository<TbDistributionDeclare>.First(p => p.ID == keyValue);
            if (inOrder == null)
                return AjaxResult.Warning("信息不存在");
            if (inOrder.Examinestatus != "未发起" && inOrder.Examinestatus != "已退回")
                return AjaxResult.Warning("信息正在审核中或已审核完成,不能进行此操作");

            return AjaxResult.Success(inOrder);
        }
        #endregion
    }
}
