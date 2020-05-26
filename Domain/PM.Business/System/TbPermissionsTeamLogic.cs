using Dos.Common;
using Dos.ORM;
using PM.Common;
using PM.Common.Helper;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Business
{
    /// 团队管理
    /// </summary>
    /// </summary>
    public class TbPermissionsTeamLogic : Repository<TbPermissionsTeam>
    {
        /// <summary>
        /// 新增数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public AjaxResult Add(PermissionsTeamRequest request)
        {
            try
            {
                if (First(p => p.TeamName == request.TeamName && p.MenuCode == request.MenuCode) != null)
                    return AjaxResult.Error("该表单已存在该团队名称");
                var model = MapperHelper.Map<PermissionsTeamRequest, TbPermissionsTeam>(request);
                var items = MapperHelper.Map<PermissionsTeamMemberRequest, TbPermissionsTeamMember>(request.PermissionsTeamMember);
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //添加团队信息
                    Insert(trans, model);
                    //添加团队成员信息
                    Repository<TbPermissionsTeamMember>.Insert(trans, items);
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
        /// 修改数据
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public AjaxResult UpdateTeam(PermissionsTeamRequest request)
        {
            try
            {
                TbPermissionsTeam team = First(p => p.TeamNumber == request.TeamNumber);
                if (team == null)
                    return AjaxResult.Error("数据不存在");
                if (First(p => p.TeamName == request.TeamName && p.MenuCode == request.MenuCode && p.TeamNumber != request.TeamNumber) != null)
                    return AjaxResult.Error("该表单已存在该团队名称");

                var model = MapperHelper.Map<PermissionsTeamRequest, TbPermissionsTeam>(request);
                var items = MapperHelper.Map<PermissionsTeamMemberRequest, TbPermissionsTeamMember>(request.PermissionsTeamMember);

                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //修改团队信息
                    Update(trans, model, p => p.ID == model.ID);
                    //删除历史团队成员信息
                    Repository<TbPermissionsTeamMember>.Delete(p => p.TeamNumber == model.TeamNumber);
                    //添加团队成员信息
                    Repository<TbPermissionsTeamMember>.Insert(trans, items);
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
        /// 删除数据
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public AjaxResult DeleteTeam(string keyValue)
        {
            var team = First(p => p.TeamNumber == keyValue);
            if (team == null)
                return AjaxResult.Error("数据不存在");
            try
            {
                 using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //删除团队信息
                    var count = Repository<TbPermissionsTeam>.Delete(trans, p => p.TeamNumber == keyValue);
                    //删除分包合同明细
                    Repository<TbPermissionsTeamMember>.Delete(trans, p => p.TeamNumber == keyValue);

                    trans.Commit();//提交事务

                    return AjaxResult.Success();
                }
            }
            catch (Exception)
            {
                return AjaxResult.Error();
            }
        }

        public DataTable GetList(string key, string menuCode)
        {
            string sql = @"select t.*,m.MenuName from TbPermissionsTeam t left join TbSysMenu m on t.MenuCode=m.MenuCode where ((ISNULL(@MenuCode,'')='') or (m.MenuCode=@MenuCode)) and (t.TeamName like '%'+@Key+'%' or t.TeamNumber like '%'+@Key+'%')";
            return Db.Context.FromSql(sql)
                .AddInParameter("@Key", DbType.String, key)
                .AddInParameter("@MenuCode", DbType.String, menuCode).ToDataTable();
        }
        public PermissionsTeamResponse GetModel(string key)
        {
            var dt = Db.Context.From<TbPermissionsTeam>()
               .Select(
                       TbPermissionsTeam._.All,
                       TbSysMenu._.MenuName)
               .LeftJoin<TbSysMenu>((a, c) => a.MenuCode == c.MenuCode).Where(p => p.TeamNumber == key).ToDataTable();
            var model = ModelConvertHelper<PermissionsTeamResponse>.ToList(dt);
            var modelRet = model[0];
            //查找成员信息
            var itemRet = Db.Context.From<TbPermissionsTeamMember>()
               .Select(
                       TbPermissionsTeamMember._.All,
                       TbUser._.UserName)
               .LeftJoin<TbUser>((a, c) => a.UserCode == c.UserCode).Where(p => p.TeamNumber == modelRet.TeamNumber).ToList<PermissionsTeamMemberRequest>();
            modelRet.PermissionsTeamMember = itemRet;
            return modelRet;
        }
        public List<TbSysMenu> GetMenuList()
        {
            string sql = @"with cte as
                            (
                            select * from TbSysMenu where DataAuthority='0'
                            union all
                            select c.* from cte P inner join TbSysMenu c on p.MenuPCode=c.MenuCode 
                            )
                            select distinct * from cte order by Sort";
            return Db.Context.FromSql(sql).ToList<TbSysMenu>();
        }
    }
}
