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

namespace PM.Business.EarlyWarning
{
    public class TbFormEarlyWarningNodeLogic
    {
        #region 新增数据
        /// <summary>
        /// 新增数据
        /// </summary>
        public AjaxResult Insert(TbFormEarlyWarningNode model, List<TbFormEarlyWarningNodePersonnel> items)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //添加预警节点信息及预警节点人员信息
                    Repository<TbFormEarlyWarningNode>.Insert(trans, model);
                    Repository<TbFormEarlyWarningNodePersonnel>.Insert(trans, items);
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
        public AjaxResult Update(TbFormEarlyWarningNode model, List<TbFormEarlyWarningNodePersonnel> items)
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
                    Repository<TbFormEarlyWarningNode>.Update(trans, model, p => p.ID == model.ID);
                    if (items.Count > 0)
                    {
                        //删除节点人员配置,添加节点人员配置
                        Repository<TbFormEarlyWarningNodePersonnel>.Delete(trans,d => d.MenuCode == model.MenuCode &&d.EarlyWarningCode==model.EarlyWarningCode&& d.EWNodeCode == model.EWNodeCode);
                        Repository<TbFormEarlyWarningNodePersonnel>.Insert(trans, items);
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
                    //删除预警节点、节点人员配置
                    var count = Repository<TbFormEarlyWarningNode>.Delete(trans, p => p.ID == keyValue);
                    Repository<TbFormEarlyWarningNodePersonnel>.Delete(trans, p => p.MenuCode == ((TbFormEarlyWarningNode)anyRet.data).MenuCode && p.EWNodeCode == ((TbFormEarlyWarningNode)anyRet.data).EWNodeCode);
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
            var ret = Db.Context.From<TbFormEarlyWarningNode>()
            .Select(
                    TbFormEarlyWarningNode._.All
                    , TbSysMenu._.MenuName)
                    .LeftJoin<TbSysMenu>((a, c) => a.MenuCode == c.MenuCode)
                    .Where(p => p.ID == dataID).ToDataTable();
            //查找节点人员配置
            string sql = @"select ewnp.*,TbU.UserTypeId,TbU.TypeName,TbU.Name,dp.DepartmentName as DeptName,r.RoleName from TbFormEarlyWarningNodePersonnel ewnp
                           left join TbDepartment dp on ewnp.DeptId=dp.DepartmentId
                           left join TbRole r on ewnp.RoleId=r.RoleId
                           left join(
                           select DepartmentId as UserTypeId,DepartmentName as Name,'Dep' as PersonnelSource,'部门' as TypeName from TbDepartment 
                           union all
                           select r.RoleId as UserTypeId,r.RoleName as Name,'Role' as PersonnelSource,'角色' as TypeName from TbRole r 
                           left join TbDepartment dp on r.DepartmentId=dp.DepartmentId where RoleName<>'系统管理员' 
                           union all
                           select u.UserId as UserTypeId,UserName as Name,'Personnel' as PersonnelSource,'用户' as TypeName from TbUser u 
                           where UserClosed<>'离职') TbU on ewnp.PersonnelCode=TbU.UserTypeId
                           where ewnp.EarlyWarningCode=@EarlyWarningCode and ewnp.MenuCode=@MenuCode and ewnp.EWNodeCode=@EWNodeCode";
            DataTable dt = Db.Context.FromSql(sql)
                .AddInParameter("@EarlyWarningCode", DbType.String, Convert.ToString(ret.Rows[0]["EarlyWarningCode"]))
                .AddInParameter("@MenuCode", DbType.String, Convert.ToString(ret.Rows[0]["MenuCode"]))
                .AddInParameter("@EWNodeCode", DbType.Int32, Convert.ToInt32(ret.Rows[0]["EWNodeCode"]))
                .ToDataTable();
            //var items = Db.Context.From<TbFormEarlyWarningNodePersonnel>().Select(
            //   TbFormEarlyWarningNodePersonnel._.All,
            //   TbRole._.RoleName,
            //   TbDepartment._.DepartmentName)
            //   .LeftJoin<TbRole>((a, c) => a.PersonnelCode == c.RoleCode)
            //   .LeftJoin<TbDepartment>((a, c) => a.DeptId == c.DepartmentId)
            //   .Where(p => p.MenuCode == ret.Rows[0]["MenuCode"].ToString() && p.EWNodeCode == Convert.ToInt32(ret.Rows[0]["EWNodeCode"]) && p.EarlyWarningCode == Convert.ToString(ret.Rows[0]["EarlyWarningCode"])).ToDataTable();
            return new Tuple<DataTable, DataTable>(ret, dt);
        }

        /// <summary>
        /// 获取数据列表(分页)
        /// </summary>
        public PageModel GetDataListForPage(PageSearchRequest request, string EarlyWarningCode)
        {
            //组装查询语句
            #region 模糊搜索条件

            var where = new Where<TbFormEarlyWarningNode>();
            if (!string.IsNullOrWhiteSpace(EarlyWarningCode))
            {
                where.And(p => p.EarlyWarningCode == EarlyWarningCode);
            }
            if (!string.IsNullOrWhiteSpace(OperatorProvider.Provider.CurrentUser.ProjectId))
            {
                where.And(d => d.ProjectId == OperatorProvider.Provider.CurrentUser.ProjectId);
            }
            #endregion

            try
            {
                var ret = Db.Context.From<TbFormEarlyWarningNode>()
              .Select(
                    TbFormEarlyWarningNode._.All
                    , TbSysMenu._.MenuName)
                    .LeftJoin<TbSysMenu>((a, c) => a.MenuCode == c.MenuCode)
                    .Where(where).OrderBy(d => d.ID).ToPageList(request.rows, request.page);
                return ret;
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
            var ewNode = Repository<TbFormEarlyWarningNode>.First(p => p.ID == keyValue);
            if (ewNode == null)
                return AjaxResult.Warning("信息不存在");
            return AjaxResult.Success(ewNode);
        }

        #endregion

        #region 获取菜单
        public PageModel GetMenu(PageSearchRequest request)
        {
            #region 模糊搜索条件

            var where = new Where<TbFormEarlyWarningBegTime>();
            if (!string.IsNullOrWhiteSpace(OperatorProvider.Provider.CurrentUser.ProjectId))
            {
                where.And(d => d.ProjectId == OperatorProvider.Provider.CurrentUser.ProjectId);
            }
            #endregion

            var ret = Db.Context.From<TbFormEarlyWarningBegTime>()
            .Select(
                     TbFormEarlyWarningBegTime._.All
                     , TbSysMenu._.MenuName)
                     .LeftJoin<TbSysMenu>((a, c) => a.MenuCode == c.MenuCode).Where(where).OrderBy(d => d.ID).ToPageList(request.rows, request.page);
            return ret;
        }
        #endregion
    }
}
