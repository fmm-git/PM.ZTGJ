using Dos.Common;
using Dos.ORM;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace PM.Business
{
    /// <summary>
    /// 岗位管理
    /// </summary>
    public class PositionLogic
    {
        private readonly AuthorizeLogic _authorizeImp = new AuthorizeLogic();
        #region 新增数据

        /// <summary>
        /// 新增数据
        /// </summary>
        public AjaxResult Insert(PositionRequest request)
        {
            if (request == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                //判断编号是否重复
                var isAny = Repository<TbPosition>.Any(p => p.PositionCode == request.PositionCode);
                if (isAny)
                    return AjaxResult.Warning("编号已存在");
                //查找部门信息
                var department = Repository<TbDepartment>.First(p => p.DepartmentCode == request.DepartmentCode);
                if (department == null)
                    return AjaxResult.Warning("部门信息不存在");
                //判断岗位名称在所属公司是否重复
                isAny = Repository<TbPosition>.Any(p => p.PositionName == request.PositionName && p.CompanyCode == department.CompanyCode);
                if (isAny)
                    return AjaxResult.Warning("岗位名称重复");

                var model = MapperHelper.Map<PositionRequest, TbPosition>(request);
                model.CreateTime = DateTime.Now;
                model.CreateUser = OperatorProvider.Provider.CurrentUser.UserCode;
                model.CompanyCode = department.CompanyCode;
                //查找数据库岗位最新数据Id 生成流水号
                //model.PositionCode = "GW" + model.DepartmentCode + "";
                //查找所有关联的公司部门岗位编号Code
                var allCode = GetAllCode(model.DepartmentCode, model.ParentPositionCode);
                model.ParentPositionCode_F = (model.PositionCode + "." + allCode.Item1).TrimEnd('.');
                model.FullCode = (model.PositionCode + "." + allCode.Item2).TrimEnd('.');
                model.ParentPositionCode = model.ParentPositionCode ?? "0";
                var count = Repository<TbPosition>.Insert(model);
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
        public AjaxResult Update(PositionRequest request)
        {
            if (request == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                //查找部门信息
                var department = Repository<TbDepartment>.First(p => p.DepartmentCode == request.DepartmentCode);
                if (department == null)
                    return AjaxResult.Warning("部门信息不存在");
                //判断岗位名称在所属公司是否重复
                var isAny = Repository<TbPosition>.Any(p => p.PositionName == request.PositionName && p.CompanyCode == department.CompanyCode && p.id != request.id);
                if (isAny)
                    return AjaxResult.Warning("岗位名称重复");

                request.ParentPositionCode = request.ParentPositionCode ?? "0";
                var model = Repository<TbPosition>.First(p => p.PositionCode == request.PositionCode);
                if (model == null)
                    return AjaxResult.Warning("信息不存在");
                if (request.PositionCode == request.ParentPositionCode)
                    return AjaxResult.Error("上级岗位错误");
                //判断是否非法继承
                var modelT = Repository<TbPosition>.First(p => p.PositionCode == request.ParentPositionCode);
                if (modelT != null)
                {
                    if (modelT.ParentPositionCode == request.PositionCode)
                        return AjaxResult.Error("信息错误,上下级关系错误");
                }
                model.DepartmentCode = request.DepartmentCode;
                model.ParentPositionCode = request.ParentPositionCode;
                model.PositionName = request.PositionName;
                model.Remark = request.Remark;
                model.CompanyCode = department.CompanyCode;
                //查找所有关联的公司部门岗位编号Code
                var allCode = GetAllCode(model.DepartmentCode, model.ParentPositionCode);
                model.ParentPositionCode_F = (model.PositionCode + "." + allCode.Item1).TrimEnd('.');
                model.FullCode = (model.PositionCode + "." + allCode.Item2).TrimEnd('.');

                //查找下级岗位，修改部门信息
                var childPositions = Repository<TbPosition>.Query(p => p.ParentPositionCode == model.PositionCode).ToList();
                if (childPositions.Count > 0)
                {
                    foreach (var item in childPositions)
                    {
                        item.DepartmentCode = model.DepartmentCode;
                    }
                }
                using (DbTrans trans = Db.Context.BeginTransaction())//使用事务
                {
                    //修改岗位信息
                    Repository<TbPosition>.Update(trans, model);
                    //修改下级岗位信息
                    if (childPositions.Count > 0)
                        Repository<TbPosition>.Update(trans, childPositions);
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
        public AjaxResult Delete(string keyValue)
        {
            try
            {
                var position = Repository<TbPosition>.First(p => p.PositionCode == keyValue);
                if (position == null)
                    return AjaxResult.Warning("信息不存在");
                //判断岗位是否有子集
                var Any = Repository<TbPosition>.Any(p => p.ParentPositionCode == position.PositionCode);
                if (Any)
                    return AjaxResult.Warning("该岗位有下级岗位,不能删除");
                var count = Repository<TbPosition>.Delete(p => p.PositionCode == keyValue);
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

        public TbPosition FindEntity(string keyValue)
        {
            var model = Repository<TbPosition>.First(p => p.PositionCode == keyValue);
            return model;
        }

        /// <summary>
        /// 获取数据列表
        /// </summary>
        public List<PositionResponse> GetListBySearch(PositionSearchRequest request)
        {
            string sql = @"select 
                            tp.PositionCode,
                            tp.CreateUser,
                            ISNULL(tp.ParentPositionCode,0) AS ParentPositionCode,
                            tp.PositionName,
                            tp.DepartmentCode,
                            td.DepartmentName,
                            td.CompanyCode,
                            tu.UserName,
                            tc.CompanyFullName
                            from TbPosition tp 
                         left join TbDepartment td on tp.DepartmentCode=td.DepartmentCode
                         left join TbCompany tc on td.CompanyCode=tc.CompanyCode
                         left join TbUser tu on tu.UserCode=tp.CreateUser
                         where 1=1";
            if (!string.IsNullOrWhiteSpace(request.PositionName))
                sql += " and  tp.PositionName like @name or tp.PositionCode like @name";

            if (!string.IsNullOrWhiteSpace(request.FullCode))
            {
                //sql += " and (tc.CompanyCode in (select CompanyCode from TbCompany where CompanyCode=@fullCode or ParentCompanyCode=@fullCode)";
                //sql += " or td.DepartmentCode in (select DepartmentCode from TbDepartment where DepartmentCode=@fullCode or ParentDepartmentCode=@fullCode))";
                sql += " and (tp.CompanyCode =@fullCode";
                sql += " or tp.DepartmentCode=@fullCode)";
            }
            if(!string.IsNullOrEmpty(request.DepartmentCode) &&!string.IsNullOrEmpty(request.PDepartmentCode))
            {
                sql += " and (tp.DepartmentCode =@departmentCode";
                sql += " or tp.DepartmentCode=@pdepartmentCode)";
            }
            //if(!string.IsNullOrEmpty(request.PositionCode))
            //    sql += " and tp.PositionCode!=@code";


            sql += " order by td.CompanyCode";
            var list = Db.Context.FromSql(sql)
                .AddInParameter("@name", DbType.String, "%" + request.PositionName + "%")
                .AddInParameter("@fullCode", DbType.String, request.FullCode)
                .AddInParameter("@departmentCode", DbType.String, request.DepartmentCode)
                .AddInParameter("@pdepartmentCode", DbType.String, request.PDepartmentCode)
                .AddInParameter("@code", DbType.String, request.PositionCode)
                .ToList<PositionResponse>();
            return list;
        }

        /// <summary>
        /// 获取数据列表
        /// </summary>
        public List<TbDepartment> GetDepartmentList(string code = "")
        {
            if (string.IsNullOrEmpty(code))
            {
                var list = Repository<TbDepartment>.GetAll();
                return list;
            }
            else
            {
                var list = Repository<TbDepartment>.Query(p => p.CompanyCode == code);
                if (list.Count < 1)
                {
                    list = Repository<TbDepartment>.Query(p => p.ParentDepartmentCode == code || p.DepartmentCode == code);
                }
                return list;
            }
        }

        /// <summary>
        /// 获取岗位列表组织机构信息(tree)
        /// </summary>
        /// <returns></returns>
        public List<PositionTreeResponse> GetPositionTree(int userPostion = 0)
        {
            try
            {
                var sql = @"select 
                            1 as isCompany,
                             CompanyCode as Code,
                             CompanyFullName as Name,
                             ParentCompanyCode as ParentCode 
                             from TbCompany";
                if (userPostion > 0)
                {
                    sql += @" union all
                             select 
                            -1 as isCompany,
                             PositionCode as Code,
                             PositionName as Name, 
                             case ParentPositionCode when '0' then CompanyCode else ParentPositionCode end as ParentCode  
                             from TbPosition";
                }
                else {
                    sql += @" union all
                            select 
                            0 as isCompany,
                            DepartmentCode as Code,
                            DepartmentName as Name,
                            case when ParentDepartmentCode='0'then CompanyCode else ParentDepartmentCode end as ParentCode
                            from TbDepartment";
                }
                var list = Db.Context.FromSql(sql).ToList<PositionTreeResponse>().OrderBy(p=>p.isCompany).ToList();
                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 获取岗位编码
        /// </summary>
        /// <returns></returns>
        public string GetPositionNum()
        {
            var positionNum = 0;
            var position = Repository<TbPosition>.GetAll().OrderByDescending(p=>p.id).FirstOrDefault();
            if (position != null)
                int.TryParse(position.PositionCode.Replace("DM",""), out positionNum);
            return "DM"+(positionNum + 1);
        }

        #endregion

        #region Private

        /// <summary>
        /// 查找所有关联的公司部门岗位编号
        /// </summary>
        /// <param name="departmentCode">部门编号</param>
        /// <param name="parentPositionCode">上级岗位编号</param>
        /// item1:所有上级岗位编号 item2：所有关联的公司部门编号
        /// <returns></returns>
        private Tuple<string, string> GetAllCode(string departmentCode, string parentPositionCode)
        {
            var allParentPositionCode = "";
            var fullCode = "";//所有上级公司+部门Code
            if (!string.IsNullOrEmpty(parentPositionCode))
            {
                //查找所有上级岗位Code
                var parentPosition = Repository<TbPosition>.First(p => p.PositionCode == parentPositionCode);
                if (parentPosition != null)
                {
                    allParentPositionCode = parentPosition.ParentPositionCode_F;
                    fullCode = parentPosition.FullCode;
                }
            }
            else
            {
                //查找部门信息
                var department = Repository<TbDepartment>.First(p => p.DepartmentCode == departmentCode);
                if (department != null)
                {
                    fullCode = department.FullCode;
                }
            }
            return new Tuple<string, string>(allParentPositionCode, fullCode);
        }

        #endregion
    }
}
