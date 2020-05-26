using PM.Common;
using PM.DataAccess.DbContext;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Business
{
    /// <summary>
    /// 页面加载按钮控制
    /// </summary>
    public class MenuPointsLogic
    {
        /// <summary>
        /// 获取当前登陆用户所拥有的该页面权限
        /// </summary>
        /// <param name="strUri">当前页面路径</param>
        /// <returns></returns>

        public DataTable GetDataMenuPoints(string UserCode, string menuURI)
        {
            string sql = @"SELECT mm.MenuUrl, pp.UserCode, pp.menuCode, pp.OperationView, pp.OperationAdd
	                        , pp.OperationEdit, pp.OperationDel, pp.OperationExamination, pp.OperationOutput, pp.OperationOther1
	                        , mm.OperationOther1Fun, mm.OperationOther1IconCls, pp.OperationOther2, mm.OperationOther2Fun, mm.OperationOther2IconCls
	                        , pp.OperationOther3, mm.OperationOther3Fun, mm.OperationOther3IconCls, pp.OperationOther4, mm.OperationOther4Fun
	                        , mm.OperationOther4IconCls, pp.OperationOther5, mm.OperationOther5Fun, mm.OperationOther5IconCls
                        FROM (
	                        SELECT UserCode, menuCode, MAX(OperationView) AS OperationView
		                        , MAX(OperationAdd) AS OperationAdd, MAX(OperationEdit) AS OperationEdit
		                        , MAX(OperationDel) AS OperationDel, MAX(OperationExamination) AS OperationExamination
		                        , MAX(OperationOutput) AS OperationOutput, MAX(OperationOther1) AS OperationOther1
		                        , MAX(OperationOther2) AS OperationOther2, MAX(OperationOther3) AS OperationOther3
		                        , MAX(OperationOther4) AS OperationOther4, MAX(OperationOther5) AS OperationOther5
	                        FROM (
		                        SELECT UserCode, menuCode, OperationView, OperationAdd, OperationEdit
			                        , OperationDel, OperationExamination, OperationOutput, OperationOther1, OperationOther2
			                        , OperationOther3, OperationOther4, OperationOther5
		                        FROM (
			                        SELECT a.UserCode, b.menuCode, MAX(b.OperationView) AS OperationView
				                        , MAX(b.OperationAdd) AS OperationAdd, MAX(b.OperationEdit) AS OperationEdit
				                        , MAX(b.OperationDel) AS OperationDel, MAX(b.OperationExamination) AS OperationExamination
				                        , MAX(b.OperationOutput) AS OperationOutput, MAX(b.OperationOther1) AS OperationOther1
				                        , MAX(b.OperationOther2) AS OperationOther2, MAX(b.OperationOther3) AS OperationOther3
				                        , MAX(b.OperationOther4) AS OperationOther4, MAX(b.OperationOther5) AS OperationOther5
			                        FROM dbo.TbUserRole a
				                        INNER JOIN dbo.TbRoleMenu b ON a.RoleCode = b.RoleCode
                                        where b.RoleCode in("+ OperatorProvider.Provider.CurrentUser.RoleCode+@")
			                        GROUP BY a.UserCode, b.menuCode
			                        UNION ALL
			                        SELECT UserCode, menuCode, OperationView, OperationAdd, OperationEdit
				                        , OperationDel, OperationExamination, OperationOutput, OperationOther1, OperationOther2
				                        , OperationOther3, OperationOther4, OperationOther5
			                        FROM dbo.TbUserMenu
			                        union all
			                        SELECT p.UserCode, d.menuCode, MAX(d.OperationView) AS OperationView
				                        , MAX(d.OperationAdd) AS OperationAdd, MAX(d.OperationEdit) AS OperationEdit
				                        , MAX(d.OperationDel) AS OperationDel, MAX(d.OperationExamination) AS OperationExamination
				                        , MAX(d.OperationOutput) AS OperationOutput, MAX(d.OperationOther1) AS OperationOther1
				                        , MAX(d.OperationOther2) AS OperationOther2, MAX(d.OperationOther3) AS OperationOther3
				                        , MAX(d.OperationOther4) AS OperationOther4, MAX(d.OperationOther5) AS OperationOther5
			                        FROM dbo.TbPositionUser p
				                        INNER JOIN dbo.TbPositionMenu d ON p.PositionCode = d.PositionCode
			                        GROUP BY p.UserCode, d.menuCode
			
		                        ) c
	                        ) aa
	                        GROUP BY UserCode, menuCode
                        ) pp
                        INNER JOIN dbo.TbSysMenu mm ON pp.menuCode = mm.menuCode 
                        left join dbo.TbUser u on pp.UserCode=u.UserId
                        where u.UserCode=@UserCode And mm.MenuUrl=@menuURI";
            var model = Db.Context.FromSql(sql)
                .AddInParameter("@UserCode", DbType.String, UserCode).AddInParameter("@menuURI", DbType.String, menuURI).ToDataTable();
            return model;
        }
    }
}
