using Dos.ORM;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Business.RawMaterial
{
    /// <summary>
    /// 车辆管理
    /// </summary>
    public class CarInfoLogic
    {

        #region 新增数据

        /// <summary>
        /// 新增数据
        /// </summary>
        public AjaxResult Insert(TbCarInfo model, List<TbCarInfoDetail> items)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            model.InsertUserCode = OperatorProvider.Provider.CurrentUser.UserCode;
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //添加信息
                    var id = Repository<TbCarInfo>.Insert(trans, model);
                    //添加明细信息
                    Repository<TbCarInfoDetail>.Insert(trans, items);
                    trans.Commit();
                    return AjaxResult.Success(id);
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
        public AjaxResult Update(TbCarInfo model, List<TbCarInfoDetail> items)
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
                    Repository<TbCarInfo>.Update(trans, model, p => p.ID == model.ID);
                    if (items.Count > 0)
                    {
                        //删除历史明细信息
                        Repository<TbCarInfoDetail>.Delete(trans, p => p.CarCode == model.CarCode);
                        //添加明细信息
                        Repository<TbCarInfoDetail>.Insert(trans, items);
                    }
                    trans.Commit();//提交事务

                    return AjaxResult.Success(model.ID);
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
                    var count = Repository<TbCarInfo>.Delete(trans, p => p.ID == keyValue);
                    //删除明细信息
                    Repository<TbCarInfoDetail>.Delete(trans, p => p.CarCode == ((TbCarInfo)anyRet.data).CarCode);
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
        /// <param name="keyValue">数据Id</param>
        /// <returns></returns>
        public Tuple<object, object> FindEntity(int keyValue)
        {
            var ret = Db.Context.From<TbCarInfo>()
                .Select(
                      TbCarInfo._.All
                    , TbUser._.UserName.As("InsertUserName")
                    , TbSupplier._.SupplierName
                    , TbCompany._.CompanyFullName.As("ProcessFactoryName"))
                  .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                  .LeftJoin<TbSupplier>((a, c) => a.SupplierCode == c.SupplierCode)
                  .LeftJoin<TbCompany>((a, c) => a.ProcessFactoryCode == c.CompanyCode)
                  .Where(p => p.ID == keyValue).ToDataTable();
            if (ret == null || ret.Rows.Count == 0)
                return new Tuple<object, object>(null, null);
            //查找明细信息
            var items = Db.Context.From<TbCarInfoDetail>()
                .Select(TbCarInfoDetail._.All)
                .Where(p => p.CarCode == ret.Rows[0]["CarCode"])
                .ToDataTable();
            return new Tuple<object, object>(ret, items);
        }

        /// <summary>
        /// 获取数据列表(分页)
        /// </summary>
        public PageModel GetDataListForPage(CarInfoRequest request)
        {

            #region 模糊搜索条件

            var where = new Where<TbCarInfo>();
            if (!string.IsNullOrWhiteSpace(request.CarCode))
            {
                where.And(p => p.CarCode.Like(request.CarCode));
            }
            if (!string.IsNullOrWhiteSpace(request.CarCph))
            {
                where.And(p => p.CarCph.Like(request.CarCph));
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where.And(p => p.ProcessFactoryCode==request.ProcessFactoryCode);
            }

            #endregion

            try
            {
                var data = Db.Context.From<TbCarInfo>()
                    .Select(
                      TbCarInfo._.All
                    , TbUser._.UserName
                    , TbSupplier._.SupplierName
                    , TbCompany._.CompanyFullName.As("ProcessFactoryName"))
                  .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                  .LeftJoin<TbSupplier>((a, c) => a.SupplierCode == c.SupplierCode)
                  .LeftJoin<TbCompany>((a,c)=>a.ProcessFactoryCode==c.CompanyCode)
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
        /// 获取数据列表(明细数据)
        /// </summary>
        public PageModel GetDataItemListForPage(CarInfoRequest request)
        {
            var where = new Where<TbCarInfoDetail>();
            if (!string.IsNullOrWhiteSpace(request.keyword))
            {
                where.And(p => p.UserCode.Like(request.keyword) || p.UserName.Like(request.keyword));
            }
            if (!string.IsNullOrWhiteSpace(request.CarCode))
            {
                where.And(p => p.CarCode==request.CarCode);
            }
            try
            {
                var data = Db.Context.From<TbCarInfoDetail>()
                    .Select(
                      TbCarInfoDetail._.UserCode.As("CarUserCode"),
                      TbCarInfoDetail._.UserName.As("CarUserName"),
                      TbCarInfoDetail._.Tel)
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
            var carInfo = Repository<TbCarInfo>.First(p => p.ID == keyValue);
            if (carInfo == null)
                return AjaxResult.Warning("信息不存在");

            return AjaxResult.Success(carInfo);
        }
        #endregion

        /// <summary>
        /// 获取司机信息（只能选择加工厂通用人员）
        /// </summary>
        public PageModel GetJgcTyUser(TbUserRequest request, string ProcessFactoryCode)
        {

            string where = "";
            if (!string.IsNullOrWhiteSpace(request.keyword))
            {
                where += " and u.UserCode like '%" + request.keyword + "%' or u.UserName like '%" + request.keyword + "%'";
            }
            if (!string.IsNullOrWhiteSpace(ProcessFactoryCode))
            {
                 where += " and OrgId='"+ ProcessFactoryCode +"' ";
            }
            try
            {
                string sql = @"select distinct u.UserCode,u.UserId,u.UserName,u.UserSex as CollarState,u.UserClosed as InsertUserCode,OrgId,ProjectId from TbUserRole 
left join TbUser u on u.UserId=TbUserRole.UserCode
where DeptId in(select DepartmentId from TbDepartment where DepartmentName='加工厂部门（通用）') 
and ProjectId='6245721945602523136' "+where+@"
group by u.UserCode,u.UserId,u.UserName,u.UserSex,u.UserClosed,OrgId,ProjectId";
                List<Parameter> parameter = new List<Parameter>();
                var model = Repository<TbUser>.FromSqlToPageTable(sql, parameter, request.rows, request.page, "UserCode", "asc");
                return model;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region 导出
        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public DataTable GetExportList(CarInfoRequest request)
        {

            #region 模糊搜索条件

            var where = new Where<TbCarInfo>();
            if (!string.IsNullOrWhiteSpace(request.CarCode))
            {
                where.And(p => p.CarCode.Like(request.CarCode));
            }
            if (!string.IsNullOrWhiteSpace(request.CarCph))
            {
                where.And(p => p.CarCph.Like(request.CarCph));
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where.And(p => p.ProcessFactoryCode == request.ProcessFactoryCode);
            }

            #endregion

            try
            {
                var data = Db.Context.From<TbCarInfo>()
                    .Select(
                      TbCarInfo._.All
                    , TbUser._.UserName
                    , TbSupplier._.SupplierName
                    , TbCompany._.CompanyFullName.As("ProcessFactoryName"))
                  .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                  .LeftJoin<TbSupplier>((a, c) => a.SupplierCode == c.SupplierCode)
                  .LeftJoin<TbCompany>((a, c) => a.ProcessFactoryCode == c.CompanyCode)
                  .Where(where)
                  .OrderByDescending(p => p.ID).ToDataTable();
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
