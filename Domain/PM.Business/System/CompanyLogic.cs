using Dos.Common;
using Dos.ORM;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using PM.DataEntity.System.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Business
{
    public class CompanyLogic
    {
        #region 公司管理查询处理

        /// <summary>
        /// 查询公司（全部查询 or 条件查询）
        /// </summary>
        /// <param name="pr"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        //public List<TbCompany> GetAllCompanyOrBySearch(PageSearchRequest pt, string keyword)
        public List<TbCompany> GetAllCompanyOrBySearch(string keyword)
        {
            var listAll = new List<TbCompany>();
            //新建查询where
            var where = new Where<TbCompany>();
            //判断条件查询是否为空，不为空，条件添加进行查询
            if (!string.IsNullOrEmpty(keyword))
            {
                where.And(t => t.CompanyFullName.Like(keyword));
            }
            //where.And(t => t.OrgType != 1);

            List<TbCompany> list1 = Repository<TbCompany>.Query(where, d => d.id, "asc").ToList();//获取所有的组织机构
            List<TbCompany> list3 = list1.Where(s => s.OrgType != 2 && s.OrgType != 1).ToList();//获取除经理部跟加工厂外的组织机构
            listAll.AddRange(list3);
            List<TbCompany> list2 = new List<TbCompany>(); //获取所有项目
            string sql1 = @"select pc.ProjectId,pj.ProjectName from TbProjectCompany pc
left join TbProjectInfo pj on pc.ProjectId=pj.ProjectId where pc.OrgType=2";
            DataTable dt1 = Db.Context.FromSql(sql1).ToDataTable();
            if (dt1 != null)
            {
                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    TbCompany cm = new TbCompany();
                    cm.CompanyCode = dt1.Rows[i]["ProjectId"].ToString();
                    cm.ParentCompanyCode = "0";
                    cm.ParentCompanyName = "";
                    cm.CompanyFullName = dt1.Rows[i]["ProjectName"].ToString();
                    cm.Address = "";
                    cm.OrgType = -1;
                    list2.Add(cm);
                }
                listAll.AddRange(list2);
            }
            List<TbCompany> list4 = list1.Where(s => s.OrgType == 2).ToList();//给所有经理部添加上级编号
            List<TbCompany> list5 = list1.Where(s => s.OrgType == 1).ToList();//给所有加工厂同时把加工厂添加的对应项目中
            List<TbCompany> list6 = new List<TbCompany>();//给所有加工厂同时把加工厂添加的对应项目中
            if (list4.Count > 0)
            {
                for (int i = 0; i < list4.Count; i++)
                {
                    switch (list4[i].CompanyCode)//用户输入的值,和下面的case匹配
                    {
                        case "6247574415609954304":
                            list4[i].ParentCompanyCode = "6245721945602523136";
                            for (int j = 0; j < list5.Count; j++)
                            {
                                TbCompany cm = new TbCompany();
                                cm.CompanyCode = list5[j].CompanyCode + "_" + list4[i].CompanyCode;
                                cm.ParentCompanyCode = list4[i].CompanyCode;
                                cm.ParentCompanyName = "";
                                cm.CompanyFullName = list5[j].CompanyFullName;
                                cm.Address = "";
                                cm.OrgType = 1;
                                list6.Add(cm);
                            }
                            break;
                        case "6247574415609954305":
                            list4[i].ParentCompanyCode = "6245721945602523137";
                            for (int j = 0; j < list5.Count; j++)
                            {
                                TbCompany cm = new TbCompany();
                                cm.CompanyCode = list5[j].CompanyCode + "_" + list4[i].CompanyCode;
                                cm.ParentCompanyCode = list4[i].CompanyCode;
                                cm.ParentCompanyName = "";
                                cm.CompanyFullName = list5[j].CompanyFullName;
                                cm.Address = "";
                                cm.OrgType = 1;
                                list6.Add(cm);
                            }
                            break;
                        case "6247574415609954309":
                            list4[i].ParentCompanyCode = "6245721945602523139";
                            for (int j = 0; j < list5.Count; j++)
                            {
                                TbCompany cm = new TbCompany();
                                cm.CompanyCode = list5[j].CompanyCode + "_" + list4[i].CompanyCode;
                                cm.ParentCompanyCode = list4[i].CompanyCode;
                                cm.ParentCompanyName = "";
                                cm.CompanyFullName = list5[j].CompanyFullName;
                                cm.Address = "";
                                cm.OrgType = 1;
                                list6.Add(cm);
                            }
                            break;
                        case "6247574415609954306":
                            list4[i].ParentCompanyCode = "6422195692059623424";
                            for (int j = 0; j < list5.Count; j++)
                            {
                                TbCompany cm = new TbCompany();
                                cm.CompanyCode = list5[j].CompanyCode + "_" + list4[i].CompanyCode;
                                cm.ParentCompanyCode = list4[i].CompanyCode;
                                cm.ParentCompanyName = "";
                                cm.CompanyFullName = list5[j].CompanyFullName;
                                cm.Address = "";
                                cm.OrgType = 1;
                                list6.Add(cm);
                            }
                            break;
                        default: //如果匹配全不成功则执行下面的代码
                            break;
                    }
                }

                listAll.AddRange(list6);
                listAll.AddRange(list4);
            }
            return listAll;
        }

        public List<TbCompany> GetAllCompanyOrBySearchNew()
        {
            string ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
            var listAll = new List<TbCompany>();
            List<TbCompany> list2 = new List<TbCompany>();
            string sql1 = @"select pc.ProjectId,pj.ProjectName from TbProjectCompany pc
left join TbProjectInfo pj on pc.ProjectId=pj.ProjectId where pc.OrgType=2 and pc.ProjectId='" + ProjectId + "'";
            DataTable dt1 = Db.Context.FromSql(sql1).ToDataTable();
            if (dt1 != null)
            {
                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    TbCompany cm = new TbCompany();
                    cm.CompanyCode = dt1.Rows[i]["ProjectId"].ToString();
                    cm.ParentCompanyCode = "0";
                    cm.CompanyFullName = dt1.Rows[i]["ProjectName"].ToString();
                    cm.OrgType = -1;
                    list2.Add(cm);
                }
                listAll.AddRange(list2);
            }
            string sql = @"select cp.id,cp.CompanyCode,cp.CompanyFullName,cp.ParentCompanyCode,cp.OrgType from TbCompany cp
left join TbProjectCompany pcp on cp.CompanyCode=pcp.CompanyCode where pcp.ProjectId='" + ProjectId + "' order by id asc";
            List<TbCompany> list1 = Db.Context.FromSql(sql).ToList<TbCompany>();
            List<TbCompany> list3 = list1.Where(s => s.OrgType != 2).ToList();
            listAll.AddRange(list3);
            List<TbCompany> list4 = list1.Where(s => s.OrgType == 2).ToList();
            if (list4.Count > 0)
            {
                for (int i = 0; i < list4.Count; i++)
                {
                    switch (list4[i].CompanyCode)//用户输入的值,和下面的case匹配
                    {
                        case "6247574415609954304":
                            list4[i].ParentCompanyCode = "6245721945602523136";
                            break;
                        case "6247574415609954305":
                            list4[i].ParentCompanyCode = "6245721945602523137";
                            break;
                        case "6247574415609954309":
                            list4[i].ParentCompanyCode = "6245721945602523139";
                            break;
                        case "6247574415609954306":
                            list4[i].ParentCompanyCode = "6422195692059623424";
                            break;
                        default: //如果匹配全不成功则执行下面的代码
                            break;
                    }
                }
                listAll.AddRange(list4);
            }
            return listAll;
        }

        public AjaxResult SynchronizationPro()
        {
            try
            {
                string projectCode = "";
                string sql1 = @"select CompanyCode from TbCompany where OrgType=2";
                DataTable dt1 = Db.Context.FromSql(sql1).ToDataTable();
                if (dt1 != null)
                {
                    for (int i = 0; i < dt1.Rows.Count; i++)
                    {
                        var parentCode = dt1.Rows[i]["CompanyCode"].ToString();
                        string sql = @"WITH TREE AS(SELECT * FROM TbCompany WHERE CompanyCode =@parentCode UNION ALL SELECT TbCompany.* FROM TbCompany, TREE WHERE TbCompany.ParentCompanyCode = TREE.CompanyCode) SELECT CompanyCode,ParentCompanyCode,CompanyFullName,OrgType,Address FROM TREE ";
                        DataTable dt = Db.Context.FromSql(sql)
                            .AddInParameter("@parentCode", DbType.String, parentCode).ToDataTable();
                        switch (parentCode)
                        {
                            case "6247574415609954304":
                                projectCode = "6245721945602523136";
                                break;
                            case "6247574415609954305":
                                projectCode = "6245721945602523137";
                                break;
                            case "6247574415609954309":
                                projectCode = "6245721945602523139";
                                break;
                            case "6247574415609954306":
                                projectCode = "6422195692059623424";
                                break;
                            default:
                                break;
                        }
                        if (dt != null)
                        {
                            List<TbProjectCompany> pcList = new List<TbProjectCompany>();
                            for (int j = 0; j < dt.Rows.Count; j++)
                            {
                                TbProjectCompany pcModel = new TbProjectCompany();
                                pcModel.CompanyCode = dt.Rows[j]["CompanyCode"].ToString();
                                pcModel.OrgType = dt.Rows[j]["OrgType"].ToString();
                                pcModel.ProjectId = projectCode;
                                pcList.Add(pcModel);
                            }
                            //List<TbProjectCompany> data = Db.Context.From<TbProjectCompany>().Where(s=>s.ProjectId==projectCode.ToString()).Select(TbProjectCompany._.All).ToList();
                            //var data = Db.Context.From<TbProjectCompany>().Select(TbProjectCompany._.All).Where(s => s.ProjectId == projectCode).ToList();
                            using (DbTrans trans = Db.Context.BeginTransaction())
                            {
                                //if (data!=null&&data.Count>0)
                                //{
                                //    Repository<TbProjectCompany>.Delete(trans, data);
                                //}
                                Repository<TbProjectCompany>.Delete(trans, t => t.ProjectId == projectCode);
                                Repository<TbProjectCompany>.Insert(trans, pcList);
                                trans.Commit();//提交事务
                            }
                        }
                    }
                }

                return AjaxResult.Success();
            }
            catch (Exception e)
            {
                return AjaxResult.Error(e.ToString());
            }
        }

        /// <summary>
        /// 根据编码查询公司
        /// </summary>
        /// <param name="menuCode"></param>
        /// <returns></returns>
        public TbCompany FindEntity(string companyCode)
        {
            var company = Repository<TbCompany>.First(d => d.CompanyCode == companyCode);
            return company;
        }

        /// <summary>
        /// 全部查询
        /// </summary>
        /// <param name="pr"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public List<TbCompany> GetList()
        {
            //OperatorProvider.Provider.CurrentUser.ProjectId
            //根据分页条件查询部分数据
            var list = Db.Context.From<TbCompany>()
              .Select(TbCompany._.All).ToList();
            return list;
        }

        #endregion

        #region 公司管理插入、修改处理

        /// <summary>
        /// 修改公司状态
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public AjaxResult EditIsEnable(string code, int val)
        {
            var count = Db.Context.Update<TbCompany>(TbCompany._.IsEnable, val, TbCompany._.CompanyCode == code);
            if (count > 0)
            {
                return AjaxResult.Success();
            }
            return AjaxResult.Error();
        }

        public int JudgeExist()
        {
            var list = Db.Context.From<TbCompany>().Where(d => d.ParentCompanyCode == "0").ToList();
            return list.Count;
        }

        public int VerificationMethod(TbCompany company, string type)
        {
            if (type == "add")
            {
                var list = Db.Context.From<TbCompany>().Where(d => d.CompanyFullName == company.CompanyFullName && d.CompanyCode == company.CompanyCode).ToList().Count;
                return list;
            }
            else
            {
                //新建查询where
                var where = new Where<TbCompany>();
                where.And(t => t.CompanyFullName == company.CompanyFullName);
                where.And(t => t.CompanyCode != company.CompanyCode);
                where.And(t => t.id != company.id);
                //根据条件查询部分数据
                var list = Repository<TbCompany>.Query(where, d => d.id).ToList().Count;
                return list;
            }

        }

        /// <summary>
        /// 新增公司信息数据
        /// </summary>
        public AjaxResult Insert(TbCompany cpy, bool isApi = false)
        {
            if (cpy == null)
                return AjaxResult.Warning("参数错误");
            if (cpy.ParentCompanyCode == "0")
            {
                var num = JudgeExist();
                if (num > 0)
                {
                    return AjaxResult.Warning("父级(顶级)公司只能存在一个！");
                }
            }
            try
            {
                //var model = MapperHelper.Map<TbCompany, TbCompany>(cpy);
                //var count = Repository<TbCompany>.Insert(model,isApi);
                //if (count <= 0)
                //    return AjaxResult.Error();
                Repository<TbCompany>.Insert(cpy, isApi);
                return AjaxResult.Success();
            }
            catch (Exception e)
            {
                var err = e.ToString();
                return AjaxResult.Error(err);
            }
        }
        /// <summary>
        /// 新增公司信息数据
        /// </summary>
        public AjaxResult InsertNew(List<TbCompany> cpy, bool isApi = false)
        {
            if (cpy == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                List<TbCompany> data = Db.Context.From<TbCompany>().Select(TbCompany._.All).ToList();
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //先删除原来的表
                    //Repository<TbCompany>.Delete(trans, data, isApi);
                    Db.Context.FromSql("truncate table TbCompany").SetDbTransaction(trans).ExecuteNonQuery();
                    //插入从BM那边取过来的数据
                    Repository<TbCompany>.Insert(trans, cpy, isApi);
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
        public AjaxResult Update(TbCompany cyp, bool isApi = false)
        {
            if (cyp == null)
                return AjaxResult.Warning("参数错误");

            try
            {
                //var model = MapperHelper.Map<TbCompany, TbCompany>(cyp);
                //var count = Repository<TbCompany>.Update(model,isApi);
                //if (count <= 0)
                //{
                //    return AjaxResult.Error();
                //}
                Repository<TbCompany>.Update(cyp, d => d.CompanyCode == cyp.CompanyCode, isApi);
                return AjaxResult.Success();
            }
            catch (Exception e)
            {
                var err = e.ToString();
                return AjaxResult.Error(err);
            }
        }

        #endregion

        #region 公司管理删除处理

        /// <summary>
        /// 删除数据
        /// </summary>
        public AjaxResult Delete(string companyCode, bool isApi = false)
        {
            var any = false;
            try
            {
                any = Repository<TbCompany>.Any(p => p.ParentCompanyCode == companyCode);
                if (any)
                {
                    return AjaxResult.Warning("该公司存在子公司，不允许删除！");
                }
                Repository<TbCompany>.Delete(t => t.CompanyCode == companyCode, isApi);
                return AjaxResult.Success();
            }
            catch (Exception e)
            {
                var err = e.ToString();
                return AjaxResult.Error();
            }
        }

        #endregion

        #region 获取工区信息 by ProjectId

        public PageModel GetWorkAreaByProjectId(TbCompanyRequest request)
        {
            var sql = @"select 
                        a.id,
                        a.CompanyCode,
                        a.Address,
                        c.CompanyFullName +'/'+a.CompanyFullName as CompanyFullName,
                        b.ProjectId
                        from TbCompany a
                        left join TbProjectCompany b on a.CompanyCode=b.CompanyCode
                        left join TbCompany c on a.ParentCompanyCode=c.CompanyCode";

            string where = " where b.OrgType=4";
            List<Parameter> parameter = new List<Parameter>();
            if (!string.IsNullOrWhiteSpace(request.ProjectId))
            {
                where += " and  b.ProjectId=@ProjectId";
                parameter.Add(new Parameter("@ProjectId", request.ProjectId, DbType.String, null));
            }

            if (!string.IsNullOrWhiteSpace(request.keyword))
            {
                where += " and a.CompanyFullName like @CompanyFullName";
                parameter.Add(new Parameter("@CompanyFullName", '%' + request.keyword + '%', DbType.String, null));
            }
            if (!string.IsNullOrEmpty(request.ParentCompanyCode))
            {
                where += " and a.ParentCompanyCode like @ParentCompanyCode";
                parameter.Add(new Parameter("@ParentCompanyCode", request.ParentCompanyCode, DbType.String, null));
            }
            try
            {
                var data = Repository<TbCompany>.FromSqlToPageTable(sql + where, parameter, request.rows, request.page, "id");
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
