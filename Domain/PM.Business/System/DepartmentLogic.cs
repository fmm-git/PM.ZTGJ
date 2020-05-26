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

namespace PM.Business
{
    /// <summary>
    /// 部门数据处理
    /// </summary>
    public class DepartmentLogic
    {
        #region 部门管理查询处理

        /// <summary>
        /// 获取成本变更编号
        /// </summary>
        /// <returns></returns>
        public string FindEntityNumber(string GSCode)
        {
            var number = "BM";
            var model = Repository<TbDepartment>.GetAll().OrderByDescending(p => p.id);
            if (model.Count() > 0)
            {
                var tem = model.First();
                var tnumber = tem.DepartmentCode.Substring(2, tem.DepartmentCode.Length - 2);
                number += (int.Parse(tnumber) + 1).ToString();
            }
            else
            {
                number += "1";
            }
            return number;
        }
        /// <summary>
        /// 获取岗位编码
        /// </summary>
        /// <returns></returns>
        public string GetPositionNum()
        {
            var positionNum = 0;
            var position = Repository<TbPosition>.GetAll().OrderByDescending(p => p.id).FirstOrDefault();
            if (position != null)
                int.TryParse(position.PositionCode.Replace("DM", ""), out positionNum);
            return "DM" + (positionNum + 1);
        }

        /// <summary>
        /// 部门公司分类导航
        /// </summary>
        public List<TbCompany> GetCompanyMenu()
        {
            var where = new Where<TbCompany>();
            return Repository<TbCompany>.Query(where, d => d.CompanyCode, "asc").ToList();
        }

        /// <summary>
        /// 全部查询OR 条件查询
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public List<TbDepartment> GetAllDepOrBySearch(string keyword)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("select a.id,");
            sb.Append("a.ParentDepartmentCode,");
            sb.Append("a.DepartmentCode,");
            sb.Append("a.DepartmentName,");
            sb.Append("a.CompanyCode,");
            sb.Append("b.CompanyFullName as BelongCompanyCode,");
            sb.Append("a.DepartmentLeader,");
            sb.Append("c.UserName as DepartmentLeaderName,");
            sb.Append("a.DepartmentSecLeader,");
            sb.Append("d.UserName as DepartmentSecLeaderName,");
            sb.Append("a.Telephone,");
            sb.Append("a.WorkSpace,");
            sb.Append("a.Remark");
            sb.Append(" from TbDepartment a left join TbCompany b ");
            sb.Append("on a.CompanyCode=b.CompanyCode left join TbUser c on a.DepartmentLeader=c.UserCode ");
            sb.Append("left join TbUser d on a.DepartmentSecLeader=d.UserCode where 1=1 ");
            if (!string.IsNullOrEmpty(keyword))
            {
                sb.Append(" and (a.DepartmentLeader like @name or a.DepartmentName like @name or a.DepartmentSecLeader like @name)");
            }
            sb.Append(" order by b.CompanyCode,a.DepartmentCode");
            return Db.Context.FromSql(sb.ToString()).AddInParameter(
                "@name", DbType.String, "%" + keyword + "%").ToList<TbDepartment>();
        }

        /// <summary>
        /// 获取数据列表(分页)
        /// </summary>
        public PageModel GetDataListForPage(TbDepartmentRequest request)
        {
            //组装查询语句
            #region 模糊搜索条件

            var where = new Where<TbDepartment>();
            if (!string.IsNullOrWhiteSpace(request.DepartmentName))
            {
                where.And(p => p.DepartmentName.Contains(request.DepartmentName));
            }
            if (!string.IsNullOrWhiteSpace(request.DepartmentType))
            {
                where.And(p => p.DepartmentType == request.DepartmentType);
            }
            if (!string.IsNullOrWhiteSpace(request.DepartmentProjectId))
            {
                where.And(p => p.DepartmentProjectId==request.DepartmentProjectId);
            }

            #endregion

            try
            {
                var ret = Db.Context.From<TbDepartment>()
              .Select(
                      TbDepartment._.DepartmentCode
                      , TbDepartment._.DepartmentId
                      , TbDepartment._.DepartmentName
                      , TbDepartment._.DepartmentType
                      , TbDepartment._.DepartmentProjectId
                      , TbProjectInfo._.ProjectName)
                    .LeftJoin<TbProjectInfo>((a, c) => a.DepartmentProjectId == c.ProjectId)
                    .Where(where).OrderByDescending(d => d.DepartmentId).ToPageList(request.rows, request.page);
                return ret;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 根据公司编码进行查询部门信息
        /// </summary>
        /// <param name="vd"></param>
        /// <returns></returns>
        public List<TbDepartment> GetDepByCompany(string vd)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("select a.id,");
            sb.Append("a.ParentDepartmentCode,");
            sb.Append("a.DepartmentCode,");
            sb.Append("a.DepartmentName,");
            sb.Append("a.CompanyCode,");
            sb.Append("b.CompanyFullName as BelongCompanyCode,");
            sb.Append("a.DepartmentLeader,");
            sb.Append("c.UserName as DepartmentLeaderName,");
            sb.Append("a.DepartmentSecLeader,");
            sb.Append("d.UserName as DepartmentSecLeaderName,");
            sb.Append("a.Telephone,");
            sb.Append("a.WorkSpace,");
            sb.Append("a.Remark");
            sb.Append(" from TbDepartment a left join TbCompany b ");
            sb.Append("on a.CompanyCode=b.CompanyCode left join TbUser c on a.DepartmentLeader=c.UserCode ");
            sb.Append("left join TbUser d on a.DepartmentSecLeader=d.UserCode where 1=1 ");
            if (!string.IsNullOrEmpty(vd))
            {
                sb.Append(" and a.CompanyCode in (select CompanyCode from TbCompany where CompanyCode=@name)");
            }
            sb.Append(" order by b.CompanyCode,a.DepartmentCode");
            return Db.Context.FromSql(sb.ToString()).AddInParameter(
                "@name", DbType.String, vd).ToList<TbDepartment>();
        }

        public List<TbDepartment> GetListByCode(string code)
        {
            string sql = @"WITH TREE AS(SELECT * FROM TbCompany WHERE CompanyCode = @Code UNION ALL SELECT TbCompany.* FROM TbCompany, TREE WHERE TbCompany.ParentCompanyCode = TREE.CompanyCode) SELECT CompanyCode FROM TREE;";
            var company = Db.Context.FromSql(sql).AddInParameter("@Code", DbType.String, code).ToList<TbCompany>();
            string sbuilder = "";
            if (!string.IsNullOrEmpty(code))
            {
                if (company.Count > 0)
                {
                    foreach (var item in company)
                    {
                        sbuilder += "'" + item.CompanyCode + "',";
                    }
                    if (sbuilder != "")
                        sbuilder = sbuilder.Substring(0, sbuilder.Length - 1);
                }
                else
                {
                    sbuilder = "'" + code + "'";
                }
            }
            var list = Db.Context.FromSql("select * from TbDepartment where CompanyCode in (" + sbuilder + ");").ToList<TbDepartment>();
            return list;
        }

        /// <summary>
        /// 根据编码查询部门
        /// </summary>
        /// <param name="menuCode"></param>
        /// <returns></returns>
        public TbDepartment FindEntity(string depCode, string ComCode)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("select a.id,");
            sb.Append("a.ParentDepartmentCode,");
            sb.Append("a.DepartmentCode,");
            sb.Append("a.DepartmentName,");
            sb.Append("a.CompanyCode,");
            sb.Append("b.CompanyFullName as BelongCompanyCode,");
            sb.Append("a.DepartmentLeader,");
            sb.Append("c.UserName as DepartmentLeaderName,");
            sb.Append("a.DepartmentSecLeader,");
            sb.Append("d.UserName as DepartmentSecLeaderName,");
            sb.Append("a.Telephone,");
            sb.Append("a.WorkSpace,");
            sb.Append("a.Remark");
            sb.Append(" from TbDepartment a left join TbCompany b ");
            sb.Append("on a.CompanyCode=b.CompanyCode left join TbUser c on a.DepartmentLeader=c.UserCode");
            sb.Append(" left join TbUser d on a.DepartmentSecLeader=d.UserCode where 1=1 ");
            if (!string.IsNullOrEmpty(depCode))
            {
                sb.Append(" and a.DepartmentCode = @dcode and a.CompanyCode=@ccode");
            }
            sb.Append(" order by a.DepartmentCode");
            return Db.Context.FromSql(sb.ToString()).AddInParameter(
                "@dcode", DbType.String, depCode).AddInParameter(
                "@ccode", DbType.String, ComCode).ToFirst<TbDepartment>();
        }

        /// <summary>
        /// 根据部门Code查询用户信息
        /// </summary>
        /// <param name="psr">分页</param>
        /// <param name="keyword">手动输入查询条件</param>
        /// <param name="DCode">条件参数部门Code</param>
        /// <param name="CCode">条件参数公司Code</param>
        /// <returns></returns>
        public List<TbUser> GetDepartmentUserList(PageSearchRequest psr, string keyword, string DCode, string CCode)
        {
            string sql = @"select A.ID,A.UserCode,A.UserName,A.UserSex,A.UserClosed from TbUser A inner join (select a.PositionCode,a.PositionName,a.CompanyCode,a.DepartmentCode,b.UserCode from TbPosition a inner join TbPositionUser b on a.PositionCode=b.PositionCode where a.DepartmentCode=@DCode and a.CompanyCode=@CCode) B on A.UserCode=B.UserCode where A.UserClosed='在职'";
            if (!string.IsNullOrEmpty(keyword))
            {
                sql += " and A.UserName like @name";
            }
            sql += " group by A.ID,A.UserCode,A.UserName,A.UserSex,A.UserClosed";
            var data = Db.Context.FromSql(sql).AddInParameter("@DCode", DbType.String, DCode).AddInParameter("@CCode", DbType.String, CCode).AddInParameter("@name", DbType.String, "%" + keyword + "%").ToList<TbUser>();
            psr.records = data.Count();
            return data;
        }

        /// <summary>
        /// 查询公司是否存在部门
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public AjaxResult GetIsExistDep(string code)
        {
            var Count = Db.Context.From<TbDepartment>().Where(d => d.CompanyCode == code).ToList().Count;
            if (Count > 0)
            {
                return AjaxResult.Warning("失败");
            }
            return AjaxResult.Warning("成功");
        }

        /// <summary>
        /// 复制部门
        /// </summary>
        /// <param name="code">复制源公司code</param>
        /// <param name="CCode">插入部门公司Code</param>
        /// <returns></returns>
        public AjaxResult CopeDepartment(string code, string CCode, string CreateUser)
        {
            var Deplist = Db.Context.From<TbDepartment>().Where(d => d.CompanyCode == code).ToList();
            var Poslist = Db.Context.From<TbPosition>().Where(d => d.CompanyCode == code).ToList();
            var PosMenulist = new List<TbPositionMenu>();
            List<TbDepartment> listdep = new List<TbDepartment>();
            List<TbPosition> listpos = new List<TbPosition>();
            if (Deplist.Count > 0)
            {
                var model = MapperHelper.Map<TbDepartment, TbDepartment>(Deplist);
                var Lcode = FindEntityNumber("");
                var Ccode = "";
                foreach (var item in model)
                {
                    item.LFCode = item.DepartmentCode;
                    item.DepartmentCode = Lcode;
                    listdep.Add(item);
                    Ccode = Lcode.Replace("BM", "");
                    Lcode = "BM" + (Convert.ToInt32(Ccode) + 1);
                }
                for (var i = 0; i < listdep.Count; i++)
                {
                    if (listdep[i].ParentDepartmentCode == "0")
                    {
                        listdep[i].BelongCompanyCode = CCode;
                        listdep[i].CompanyCode = CCode;
                        listdep[i].DepartmentLeader = "";
                        listdep[i].DepartmentSecLeader = "";
                        listdep[i].id = 0;
                        listdep[i].Remark = "";
                        listdep[i].Telephone = "";
                        listdep[i].WorkSpace = "";
                    }
                    else
                    {
                        listdep[i].BelongCompanyCode = CCode;
                        listdep[i].CompanyCode = CCode;
                        listdep[i].DepartmentLeader = "";
                        listdep[i].DepartmentSecLeader = "";
                        listdep[i].id = 0;
                        listdep[i].Remark = "";
                        listdep[i].Telephone = "";
                        listdep[i].WorkSpace = "";
                        foreach (var item in listdep)
                        {
                            if (listdep[i].ParentDepartmentCode == item.LFCode)
                            {
                                listdep[i].ParentDepartmentCode = item.DepartmentCode;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                return AjaxResult.Error("未找到部门数据！");
            }
            if (Poslist.Count > 0)
            {
                var model = MapperHelper.Map<TbPosition, TbPosition>(Poslist);
                var Lcode = GetPositionNum();
                var Ccode = "";
                foreach (var item in model)
                {
                    item.LsCode = item.PositionCode;
                    item.PositionCode = Lcode;
                    listpos.Add(item);
                    Ccode = Lcode.Replace("DM", "");
                    Lcode = "DM" + (Convert.ToInt32(Ccode) + 1);
                }
                for (var i = 0; i < listpos.Count; i++)
                {
                    if (listpos[i].ParentPositionCode == "0")
                    {
                        listpos[i].CompanyCode = CCode;
                        listpos[i].CreateTime = DateTime.Now;
                        listpos[i].id = 0;
                        listpos[i].Remark = "";
                        listpos[i].CreateUser = CreateUser;
                    }
                    else
                    {
                        listpos[i].CompanyCode = CCode;
                        listpos[i].CreateTime = DateTime.Now;
                        listpos[i].id = 0;
                        listpos[i].Remark = "";
                        listpos[i].CreateUser = CreateUser;
                        foreach (var item in listpos)
                        {
                            if (listpos[i].ParentPositionCode == item.LsCode)
                            {
                                listpos[i].ParentPositionCode = item.PositionCode;
                                break;
                            }
                        }
                    }
                    foreach (var item in listdep)
                    {
                        if (listpos[i].DepartmentCode == item.LFCode)
                        {
                            listpos[i].DepartmentCode = item.DepartmentCode;
                            break;
                        }
                    }
                }
                for (var j = 0; j < listpos.Count; j++)
                {
                    var pos = Db.Context.From<TbPositionMenu>().Where(d => d.PositionCode == listpos[j].LsCode).ToList();
                    if (pos.Count > 0)
                    {
                        for (var k = 0; k < pos.Count; k++)
                        {
                            pos[k].ID = 0;
                            pos[k].PositionCode = listpos[j].PositionCode;
                            PosMenulist.Add(pos[k]);
                        }
                    }
                }
            }
            try
            {
                var posm = MapperHelper.Map<TbPositionMenu, TbPositionMenu>(PosMenulist);
                using (DbTrans trans = Db.Context.BeginTransaction())//使用事务
                {
                    //添加部门
                    Repository<TbDepartment>.Insert(trans, listdep);
                    if (listpos.Count > 0)
                    {
                        //添加岗位
                        Repository<TbPosition>.Insert(trans, listpos);
                        if (PosMenulist.Count > 0)
                        {
                            //添加岗位权限
                            Repository<TbPositionMenu>.Insert(trans, posm);
                        }
                    }
                    else
                    {
                        return AjaxResult.Error();
                    }
                    trans.Commit();//提交事务
                    return AjaxResult.Success();
                }
            }
            catch (Exception ex)
            {
                return AjaxResult.Error();
            }
        }

        #endregion

        #region 部门管理新增、修改处理

        /// <summary>
        /// 新增部门信息数据
        /// </summary>
        public AjaxResult Insert(TbDepartment dep, bool isApi = false)
        {
            if (dep == null)
                return AjaxResult.Warning("参数错误");
            var BmCodeCount = Db.Context.From<TbDepartment>().Where(d => d.DepartmentCode == dep.DepartmentCode).ToList().Count;
            if (BmCodeCount > 0)
                return AjaxResult.Warning("部门编码重复！");
            var BmNameCount = Db.Context.From<TbDepartment>().Where(d => d.DepartmentName == dep.DepartmentName && d.CompanyCode == dep.CompanyCode).ToList().Count;
            if (BmNameCount > 0)
                return AjaxResult.Warning("部门名称重复！");
            try
            {
                //dep.BelongCompanyCode = dep.CompanyCode;
                //var model = MapperHelper.Map<TbDepartment, TbDepartment>(dep);
                //var count = Repository<TbDepartment>.Insert(model, isApi);
                //if (count <= 0)
                //    return AjaxResult.Error();
                Repository<TbDepartment>.Insert(dep, isApi);
                return AjaxResult.Success();
            }
            catch (Exception e)
            {
                var err = e.ToString();
                return AjaxResult.Error(err);
            }
        }

        /// <summary>
        /// 新增部门信息数据
        /// </summary>
        public AjaxResult InsertNew(List<TbDepartment> dep, bool isApi = false)
        {
            try
            {
                List<TbDepartment> data = Db.Context.From<TbDepartment>().Select(TbDepartment._.All).ToList();
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //先删除原来的表
                    Db.Context.FromSql("truncate table TbDepartment").SetDbTransaction(trans).ExecuteNonQuery();
                    //插入从BM那边取过来的数据
                    Repository<TbDepartment>.Insert(trans, dep, isApi);
                    trans.Commit();//提交事务
                    return AjaxResult.Success();
                }
            }
            catch (Exception e)
            {
                var err = e.ToString();
                return AjaxResult.Error(err);
            }
        }

        /// <summary>
        /// 修改公司信息数据
        /// </summary>
        public AjaxResult Update(TbDepartment dep, bool isApi = false)
        {
            if (dep == null)
                return AjaxResult.Warning("参数错误");
            var BmNameCount = Db.Context.From<TbDepartment>().Where(d => d.DepartmentName == dep.DepartmentName && d.DepartmentCode != dep.DepartmentCode && d.CompanyCode == dep.CompanyCode).ToList().Count;
            if (BmNameCount > 0)
                return AjaxResult.Warning("部门名称重复！");
            try
            {
                //var model = MapperHelper.Map<TbDepartment, TbDepartment>(dep);
                //var count = Repository<TbDepartment>.Update(model,isApi);
                //if (count <= 0)
                //{
                //    return AjaxResult.Error();
                //}
                Repository<TbDepartment>.Update(dep, p => p.DepartmentCode == dep.DepartmentCode, isApi);
                return AjaxResult.Success();
            }
            catch (Exception e)
            {
                return AjaxResult.Error(e.ToString());
            }
        }

        #endregion

        #region 部门管理删除处理

        /// <summary>
        /// 删除数据
        /// </summary>
        public AjaxResult Delete(string depCode, string ComCode)
        {
            try
            {
                //查找信息
                var model = Repository<TbDepartment>.First(p => p.DepartmentCode == depCode && p.CompanyCode == ComCode);
                if (model == null)
                    return AjaxResult.Error("信息不存在");
                var Position = Db.Context.FromSql("select * from TbPosition where DepartmentCode=@dcode and CompanyCode=@ccode").AddInParameter(
                "@dcode", DbType.String, depCode).AddInParameter(
                "@ccode", DbType.String, ComCode).ToList<TbPosition>();
                if (Position != null && Position.Count > 0)
                {
                    return AjaxResult.Warning("部门存在岗位信息，请处理后再删除！");
                }
                var Dep = Db.Context.FromSql("select * from TbDepartment where ParentDepartmentCode=@dcode and CompanyCode=@ccode").AddInParameter(
                "@dcode", DbType.String, depCode).AddInParameter(
                "@ccode", DbType.String, ComCode).ToList<TbDepartment>();
                if (Dep != null && Dep.Count > 0)
                {
                    return AjaxResult.Warning("部门存在下级部门信息，请处理后再删除！");
                }
                var count = Repository<TbDepartment>.Delete(t => t.DepartmentCode == depCode);
                if (count <= 0)
                    return AjaxResult.Error();
                return AjaxResult.Success();
            }
            catch (Exception e)
            {
                var err = e.ToString();
                return AjaxResult.Error(err);
            }
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        public AjaxResult DeleteNew(string depCode, bool isApi = false)
        {
            try
            {
                //查找信息
                var model = Repository<TbDepartment>.First(p => p.DepartmentCode == depCode);
                if (model == null)
                    return AjaxResult.Error("信息不存在");
                var count = Repository<TbDepartment>.Delete(t => t.DepartmentCode == depCode, isApi);
                if (count <= 0)
                    return AjaxResult.Error();
                return AjaxResult.Success();
            }
            catch (Exception e)
            {
                var err = e.ToString();
                return AjaxResult.Error(err);
            }
        }

        #endregion

    }
}
