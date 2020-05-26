using Dos.ORM;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using PM.DataEntity.DataManage.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Business.DataManage
{
    public class FileTypeBuessic
    {
        //资料分类归档

        #region 首页查询
        /// <summary>
        /// 列表数据查询
        /// </summary>
        /// <returns></returns>
        public PageModel GetGridJson(TbDataManageRequest request)
        {
            #region 模糊搜索条件
            var where = new Where<TbDataManage>();
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where.And(d => d.ProcessFactoryCode == request.ProcessFactoryCode);
            }
            if (!string.IsNullOrWhiteSpace(request.TypeName))
            {
                where.And(d => d.TypeName == request.TypeName);
            }
            #endregion
            
            try
            {
                var data = Db.Context.From<TbDataManage>().Select(
                        TbDataManage._.ID,
                        TbDataManage._.FileCode,
                        TbDataManage._.TypeName,
                        TbDataManage._.DataName,
                        TbDataManage._.DataContent,
                        TbDepartment._.DepartmentName,
                        TbDataManage._.InsertTime,
                        TbDataManage._.Remark,
                        TbDataManage._.InsertUserCode,
                        TbUser._.UserName
                   ).AddSelect(Db.Context.From<TbCompany>().Select(p => p.CompanyFullName)
                   .Where(TbCompany._.CompanyCode == TbDataManage._.ProcessFactoryCode), "ProcessFactoryName")
                   .LeftJoin<TbDepartment>((a, c) => a.DepartmentCode == c.DepartmentId)
                   .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
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
        #endregion

        #region 添加/修改
        /// <summary>
        /// 查询部门信息
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<TbDepartment> GetDepartmentCodeJson(string keyword)
        {
            //#region 模糊查询条件
            //var where = new Where<TbDepartment>();
            //if (!string.IsNullOrWhiteSpace(keyword))
            //{
            //    where.And(d => d.DepartmentName.Like(keyword));
            //}
            //#endregion
            try
            {
                string sql = @"
                    select ur.ID,cp.CompanyFullName BelongCompanyCode,r.RoleName CompanyCode,dp.DepartmentId DepartmentCode,dp.DepartmentName from TbUserRole ur
                    left join TbRole r on ur.RoleCode=r.RoleId
                    left join TbDepartment dp on ur.DeptId=dp.DepartmentId
                    left join TbCompany cp on cp.CompanyCode=ur.OrgId
                     where (ISNULL(@UserCode,'')='' OR UserCode=@UserCode) 
                     AND (ISNULL(@ProjectId,'')='' OR ProjectId=@ProjectId) 
                     AND Flag=0  
                     AND  (ISNULL(@keyword,'')='' OR dp.DepartmentName LIKE @keyword)
                ";
                return Db.Context.FromSql(sql).AddInParameter("keyword", DbType.String, "%" + keyword + "%").AddInParameter("UserCode", DbType.String, OperatorProvider.Provider.CurrentUser.UserId).AddInParameter("ProjectId", DbType.String, OperatorProvider.Provider.CurrentUser.ProjectId).ToList<TbDepartment>();
                //return Db.Context.From<TbDepartment>().Select(
                //    TbDepartment._.id,
                //    TbDepartment._.DepartmentName,
                //    TbDepartment._.DepartmentId.As("DepartmentCode"),
                //    TbDepartment._.DepartmentProjectId,
                //    TbDepartment._.DepartmentLeader,
                //    TbDepartment._.DepartmentSecLeader
                // ).Where(where).ToList();
            }
            catch (Exception)
            {

                throw;
            }
        }


        /// <summary>
        /// 查询当前登录人员下的加工厂信息
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<TbRMProductionMaterial> ProcessFactoryNameSelect(string UserCode)
        {
            try
            {
                string sql = @"SELECT C.id,C.CompanyFullName UserCode,c.CompanyCode ProcessFactoryCode FROM TbUser U
                                INNER JOIN TbCompany C ON C.CompanyCode=U.CompanyCode
                                WHERE (ISNULL(@UserCode,'')='' OR U.UserCode=@UserCode)";
                return Db.Context.FromSql(sql).AddInParameter("UserCode", DbType.String, "" + UserCode + "").ToList<TbRMProductionMaterial>();
            }
            catch (Exception)
            {

                throw;
            }
        }

        //添加操作
        public AjaxResult Insert(TbDataManage data)
        {
            if (data == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                //添加信息
                Repository<TbDataManage>.Insert(data);
                return AjaxResult.Success();
            }
            catch (Exception ex)
            {
                return AjaxResult.Error();
            }
        }

        //修改操作
        public AjaxResult Update(TbDataManage data)
        {
            if (data == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                var count = Repository<TbDataManage>.Update(data, p => p.ID == data.ID);
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

        #region 删除

        //删除
        public AjaxResult DeleteForm(int keyValue)
        {
            try
            {
                //判断信息是否存在
                var anyRet = AnyInfo(keyValue);
                if (anyRet.state != ResultType.success.ToString())
                    return anyRet;
                //删除信息
                var count = Repository<TbDataManage>.Delete(p => p.ID == keyValue);
                return AjaxResult.Success();
            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.ToString());
            }
        }

        //是否可删除，判断
        public AjaxResult AnyInfo(int keyValue)
        {
            var ID = Repository<TbDataManage>.First(p => p.ID == keyValue);
            if (ID == null)
                return AjaxResult.Warning("信息不存在");
            return AjaxResult.Success(ID);
        }
        #endregion

        #region 资料分类归档信息查看
        public Tuple<DataTable> GetFormJson(int keyValue)
        {

            try
            {
                var ret = Db.Context.From<TbDataManage>()
              .Select(
                      TbDataManage._.All,
                      TbDataManage._.TypeName.As("TypeNameSelected")
                      , TbCompany._.CompanyFullName.As("ProcessFactoryName")
                      , TbDepartment._.DepartmentName
                      , TbUser._.UserName.As("InsertUserName")
                      )
                    .LeftJoin<TbCompany>((a, c) => a.ProcessFactoryCode == c.CompanyCode)
                    .LeftJoin<TbDepartment>((a, c) => a.DepartmentCode == c.DepartmentId)
                    .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                    .Where(p => p.ID == keyValue).ToDataTable();
                return new Tuple<DataTable>(ret);
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region 资料分类统计
        /// <summary>
        /// 获取菜单-模块/表单二级树形列表数据
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public List<TbSysMenu> GetTwoNveProInfo()
        {
            string sql = @"SELECT * FROM (
                                --*************************************模块信息****************************************
                                SELECT ID,
	                                   MenuCode,
	                                   MenuName,
	                                   MenuPCode,
	                                   ROW_NUMBER() OVER(ORDER BY Sort ASC) Sort
	                                   FROM TbSysMenu WHERE MenuPCode='0'
	                                    AND MenuCode NOT IN ('SysManage','BIExhibition','SettlementManage','Interactionn','OA')
                                UNION ALL
                                SELECT TOP 1 1 AS ID,
									   'PlanManage' MenuCode,
									   '计划管理' MenuName,
									   'RawMaterial' MenuPCode,
									   NULL Sort 
							    UNION ALL
                                SELECT TOP 1 2 AS ID,
                                       'StatisticsReportForm' MenuCode,
                                        '统计报表' MenuName,
                                        'RawMaterial' MenuPCode,
                                        NULL Sort 
                                UNION ALL
                                --*************************模块下具体的业务表单信息****************************************
                                SELECT A.ID,
	                                   A.MenuCode,
	                                   MenuName,
	                                   MenuPCode,
	                                   NULL Sort
	                                   FROM TbSysMenu  A
	                                   INNER JOIN TbSysMenuTable b ON A.MenuCode=B.MenuCode
	                                   WHERE MenuPCode!='0') A";
            return Db.Context.FromSql(sql).ToList<TbSysMenu>();
        }
        #endregion

        //资料分类统计数据查询
        public List<TbAttachment> GetDataList(TbDataClassRequest request)
        {
            try
            {
                string sql = @"--************************查询各业务表单里面的附件**********************************
                                DECLARE @TabName VARCHAR(32) SET @TabName=(SELECT TableName FROM TbSysMenuTable WHERE MenuCode=(
				                                SELECT TOP 1 MenuCode FROM TbSysMenu WHERE (ISNULL(@code,'')='' OR MenuCode=@code))) 
                                 EXEC
                                 (
	                                'SELECT F.*,U.UserName FunModule
	                                FROM '+@TabName+' A OUTER  APPLY  dbo.StringToRows(A.ID,A.Enclosure) b
	                                INNER JOIN TbAttachment f on B.StrField=f.FileID
                                    LEFT JOIN TbUser U ON U.UserCode=F.UserCode'
                                  )";
                SqlParameter[] par = new SqlParameter[] {
                                    new SqlParameter("@code",request.code)//菜单编号
                                };
                var list = Db.Context.FromSql(sql).AddParameter(par).ToList<TbAttachment>();
                //查询总数
                request.records = list.Count();
                //参数化
                return list;
                //List<Dos.ORM.Parameter> para = new List<Dos.ORM.Parameter>();
                ////将左侧导航分类的项目编号Code加入参数中
                //para.Add(new Dos.ORM.Parameter("@code", request.code, DbType.String, null));
                ////将model返回给前台
                //var model = Repository<TbAttachment>.FromSql(sql, para, "id", "desc", request.rows, request.page).ToList<TbAttachment>();
                //return model;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
