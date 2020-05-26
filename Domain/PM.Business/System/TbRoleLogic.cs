using Dos.Common;
using Dos.ORM;
using PM.Common;
using PM.Common.Helper;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Business
{
    public class TbRoleLogic
    {
        #region 新增数据

        /// <summary>
        /// 新增数据(单条)
        /// </summary>
        public AjaxResult Insert(TbRoleRequset request)
        {
            if (request == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                var model1 = Repository<TbRole>.First(d => d.RoleName == request.RoleName);
                if (model1 != null)
                {
                    return AjaxResult.Error("角色名称重复");
                }
                var model = MapperHelper.Map<TbRoleRequset, TbRole>(request);
                model.RoleCode = request.RoleCode;
                model.RoleName = request.RoleName;
                model.RoleDetail = request.RoleDetail;
                model.State = "启用";
                var count = Repository<TbRole>.Insert(model);
                if (count > 0)
                {
                    return AjaxResult.Success();
                }
                return AjaxResult.Error();
            }
            catch (Exception)
            {
                return AjaxResult.Error();
            }

        }

        /// <summary>
        /// 新增数据(单条)
        /// </summary>
        public AjaxResult InsertNew(TbRole model, bool isApi = false)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            var model1 = Repository<TbRole>.First(d => d.RoleName == model.RoleName && d.DepartmentId == model.DepartmentId);
            if (model1 != null)
            {
                return AjaxResult.Error("角色名称重复");
            }
            try
            {
                Repository<TbRole>.Insert(model, isApi);
                return AjaxResult.Success();
            }
            catch (Exception)
            {
                return AjaxResult.Error();
            }

        }

        /// <summary>
        /// 新增数据(单条)
        /// </summary>
        public AjaxResult InsertNew1(List<TbRole> model, bool isApi = false)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");

            try
            {
                List<TbRole> data = Db.Context.From<TbRole>().Select(TbRole._.All).ToList();
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //先删除原来的表
                    //Repository<TbRole>.Delete(trans, data, isApi);
                    Db.Context.FromSql("truncate table TbRole").SetDbTransaction(trans).ExecuteNonQuery();
                    //插入从BM那边取过来的数据
                    Repository<TbRole>.Insert(trans, model, isApi);
                    trans.Commit();//提交事务
                    return AjaxResult.Success();
                }
            }
            catch (Exception)
            {
                return AjaxResult.Error();
            }

        }

        /// <summary>
        /// 新增数据(单条)
        /// </summary>
        public AjaxResult CopyRole(TbRoleRequset request, string roleCode)
        {
            if (request == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                var model1 = Repository<TbRole>.First(d => d.RoleName == request.RoleName);
                if (model1 != null)
                {
                    return AjaxResult.Error("角色名称重复");
                }
                var model = MapperHelper.Map<TbRoleRequset, TbRole>(request);
                model.RoleCode = request.RoleCode;
                model.RoleName = request.RoleName;
                model.RoleDetail = request.RoleDetail;
                model.State = "启用";
                //获取要拷贝的角色的角色权限
                var roleMenu = Repository<TbRoleMenu>.Query(p => p.RoleCode == roleCode).ToList();
                List<TbRoleMenu> roleMenuUpdate = new List<TbRoleMenu>();
                if (roleMenu != null && roleMenu.Count > 0)
                {
                    for (int i = 0; i < roleMenu.Count; i++)
                    {
                        var roleMenu1 = MapperHelper.Map<TbRoleMenu, TbRoleMenu>(roleMenu[i]);
                        roleMenu1.RoleCode = model.RoleCode;
                        roleMenuUpdate.Add(roleMenu1);
                    }
                }
                using (DbTrans trans = Db.Context.BeginTransaction())//使用事务
                {
                    Repository<TbRole>.Insert(trans, model);
                    if (roleMenuUpdate != null && roleMenuUpdate.Count > 0)
                    {
                        Repository<TbRoleMenu>.Insert(trans, roleMenuUpdate);//拷贝角色权限
                    }
                    trans.Commit();//提交事务
                }
                return AjaxResult.Success();

            }
            catch (Exception)
            {
                return AjaxResult.Error();
            }

        }

        /// <summary>
        /// 获取角色编号
        /// </summary>
        /// <returns></returns>
        public string FindEntityNumber()
        {
            var number = "RN";
            string sql = @"select top 1 * from TbRole where RoleCode like 'RN%' order by id desc";
            var dt = Db.Context.FromSql(sql).ToDataTable();
            var model = ModelConvertHelper<TbRole>.ToList(dt);
            if (model.Count() > 0)
            {
                var tem = model.First();
                var cnumber = tem.RoleCode.Replace("RN", "");
                number += (int.Parse(cnumber) + 1).ToString("").PadLeft(2, '0'); ;
            }
            else
            {
                number += "01";
            }
            return number;
        }

        #endregion

        #region 修改数据

        /// <summary>
        /// 修改数据(单条)
        /// </summary>
        public AjaxResult Update(TbRoleRequset request)
        {
            if (request == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                var model = Repository<TbRole>.First(p => p.RoleCode == request.RoleCode);
                if (model == null)
                    return AjaxResult.Error("信息不存在");
                var model1 = Repository<TbRole>.First(d => d.RoleName == request.RoleName && d.RoleCode != request.RoleCode);
                if (model1 != null)
                {
                    return AjaxResult.Error("角色名称重复");
                }
                model.RoleCode = request.RoleCode;
                model.RoleName = request.RoleName;
                model.RoleDetail = request.RoleDetail;
                var count = Repository<TbRole>.Update(model);
                if (count > 0)
                {
                    return AjaxResult.Success();
                }
                return AjaxResult.Error();
            }
            catch (Exception)
            {
                return AjaxResult.Error();
            }

        }

        /// <summary>
        /// 修改数据(单条)
        /// </summary>
        public AjaxResult UpdateNew(TbRole model, bool isApi = false)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");

            var model1 = Repository<TbRole>.First(d => d.RoleName == model.RoleName && d.DepartmentId == model.DepartmentId && d.RoleCode != model.RoleCode);
            if (model1 != null)
            {
                return AjaxResult.Error("角色名称重复");
            }
            try
            {
                Repository<TbRole>.Update(model, isApi);
                return AjaxResult.Success();
            }
            catch (Exception)
            {
                return AjaxResult.Error();
            }
        }

        /// <summary>
        /// 修改角色状态
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public AjaxResult EditIsStart(string code, string val)
        {
            var count = Db.Context.Update<TbRole>(TbRole._.State, val, TbRole._.RoleCode == code);
            if (count > 0)
            {
                return AjaxResult.Success();
            }
            return AjaxResult.Error();
        }

        #endregion

        #region 删除数据

        /// <summary>
        /// 删除数据
        /// </summary>
        public AjaxResult Delete(string RoleCode, bool isApi = false)
        {
            try
            {
                var model1 = Repository<TbRoleMenu>.First(p => p.RoleCode == RoleCode);
                if (model1 != null)
                {
                    return AjaxResult.Error("该角色已经分配权限，不能删除");
                }
                var model2 = Repository<TbUserRole>.First(p => p.RoleCode == RoleCode);
                if (model2 != null)
                {
                    return AjaxResult.Error("该角色已经分配用户，不能删除");
                }
                var count = Repository<TbRole>.Delete(t => t.RoleCode == RoleCode, isApi);
                if (count > 0)
                {
                    return AjaxResult.Success();
                }
                return AjaxResult.Error();
            }
            catch (Exception)
            {
                return AjaxResult.Error();
            }

        }

        #endregion

        #region 获取数据

        public TbRole FindEntity(string RoleCode)
        {
            var model = Repository<TbRole>.First(d => d.RoleCode == RoleCode);
            return model;
        }

        ///// <summary>
        ///// 获取数据列表(分页)
        ///// </summary>
        //public List<TbRole> GetDataListForPage(TbRoleRequset param, string keyword)
        //{
        //    //组装查询语句
        //    #region 模糊搜索条件

        //    var where = new Where<TbRole>();
        //    if (!string.IsNullOrEmpty(keyword))
        //    {
        //        where.And(d => d.RoleCode.Like(keyword));
        //        where.Or(d => d.RoleName.Like(keyword));
        //    }

        //    #endregion
        //    var orderBy = OrderByOperater.ASC;
        //    if (param.sord.Equals("desc"))
        //        orderBy = OrderByOperater.DESC;
        //    try
        //    {
        //        //取总数，以计算共多少页。
        //        var dateCount = Repository<TbRole>.Count(where);
        //        var orderByClip = new OrderByClip(new Field(param.sidx), orderBy);//排序字段
        //        var list = Repository<TbRole>.Query(where, orderByClip, param.sord, param.rows, param.page).ToList();
        //        param.records = dateCount;
        //        return list;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        /// <summary>
        /// 获取数据列表(分页)
        /// </summary>
        public PageModel GetDataListForPage(TbRoleRequset request)
        {
            //组装查询语句
            #region 模糊搜索条件

            var where = new Where<TbRole>();
            if (!string.IsNullOrWhiteSpace(request.RoleName))
            {
                where.And(p => p.RoleName.Contains(request.RoleName));
            }
            if (!string.IsNullOrWhiteSpace(request.DepartmentId))
            {
                 where.And(p => p.DepartmentId==request.DepartmentId);
            }
            #endregion

            try
            {
                var ret = Db.Context.From<TbRole>()
              .Select(TbRole._.All).Where(where).OrderByDescending(d => d.RoleId).ToPageList(request.rows, request.page);
                return ret;
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// 获取不分页数据列表
        /// </summary>
        public List<TbRole> GetNoPageGridList()
        {
            var where = new Where<TbRole>();
            //where.And(d => d.State == "启用");
            return Repository<TbRole>.Query(where, d => d.RoleCode, "asc").ToList();
        }
        #endregion
    }
}
