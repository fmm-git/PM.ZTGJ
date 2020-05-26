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

namespace PM.Business.System
{
    public class ShortMessageTemplateLogic
    {
        /// <summary>
        /// 查询全部绑定数据
        /// </summary>
        /// <param name="ent"></param>
        /// <returns></returns>
        public PageModel GetAllTemplate(FPiCiXQPlan request) 
        {
            //var ret = Db.Context.From<TbShortMessageTemplate>().Select(
            //    TbSysMenu._.ID,
            //    TbSysMenu._.MenuCode,
            //    TbSysMenu._.MenuName,
            //    TbShortMessageTemplate._.TemplateCode,
            //    TbShortMessageTemplate._.TemplateContent,
            //    TbShortMessageTemplate._.TemplateType
            //    ).InnerJoin<TbSysMenu>((a, c) => a.TemplateCode == c.DuanXinTemplate).OrderByDescending(d => d.ID).ToPageList(ent.rows, ent.page);
            //return ret;
            //组装查询语句
            #region 模糊搜索条件

            var where = new Where<TbShortMessageTemplate>();

            #endregion

            try
            {
                var ret = Db.Context.From<TbShortMessageTemplate>()
              .Select(TbShortMessageTemplate._.All)
                      .Where(where).OrderByDescending(d => d.ID).ToPageList(request.rows, request.page);
                return ret;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 模板弹窗
        /// </summary>
        /// <param name="ent">分页</param>
        /// <returns></returns>
        public PageModel selectTemplate(FPiCiXQPlan ent, string keyword)
        {
            var where = new Where<TbShortMessageTemplate>();
            if(!string.IsNullOrEmpty(keyword))
            {
                where.And(c => c.TemplateCode.Like(keyword));
            }
            var ret = Db.Context.From<TbShortMessageTemplate>().Select(
                TbShortMessageTemplate._.All
                ).Where(where).OrderByDescending(d => d.ID).ToPageList(ent.rows, ent.page);
            return ret;
        }

        /// <summary>
        /// 判断信息是否可操作
        /// </summary>
        /// <returns></returns>
        public AjaxResult AnyInfo(string keyValue)
        {
            var md = Repository<TbSysMenu>.First(p => p.MenuPCode == keyValue);
            if (md != null)
                return AjaxResult.Warning("此选项绑定失败，请重新选择！");
            return AjaxResult.Success(md);
        }

        /// <summary>
        /// 获取编辑数据
        /// </summary>
        /// <param name="dataID">数据Id</param>
        /// <returns></returns>
        public DataTable FindEntity(int dataID)
        {
            var ret = Db.Context.From<TbShortMessageTemplate>()
            .Select(TbShortMessageTemplate._.All).Where(p => p.ID == dataID).ToDataTable(); ;
            return ret;
        }

        /// <summary>
        /// 编辑数据
        /// </summary>
        public AjaxResult SubmitForm(string MCode, string TCode)
        {
            try
            {
                var md = Db.Context.From<TbSysMenu>().Where(d => d.MenuCode == MCode).First();
                md.DuanXinTemplate = TCode;
                var count = Repository<TbSysMenu>.Update(md, p => p.ID == md.ID);
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


        #region 新增数据
        /// <summary>
        /// 新增数据
        /// </summary>
        public AjaxResult Insert(TbShortMessageTemplate model)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            using (DbTrans trans = Db.Context.BeginTransaction())
            {
                try
                {
                    //添加信息及明细信息
                    Repository<TbShortMessageTemplate>.Insert(trans, model);
                    trans.Commit();
                    return AjaxResult.Success();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return AjaxResult.Error(ex.ToString());
                }
                finally
                {
                    trans.Close();
                }
            }
        }

        #endregion

        #region 修改数据

        /// <summary>
        /// 修改数据
        /// </summary>
        public AjaxResult Update(TbShortMessageTemplate model)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                using (DbTrans trans = Db.Context.BeginTransaction())//使用事务
                {
                    //修改信息
                    Repository<TbShortMessageTemplate>.Update(trans, model, p => p.ID == model.ID);
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

        /// <summary>
        /// 查询需求计划填报
        /// </summary>
        /// <returns></returns>
        public DataTable GetRawMaterialXQPlan() 
        {
            string ym = DateTime.Now.Year + "-" + DateTime.Now.Month;
            var ret = Db.Context.FromSql("select SiteCode from TbRawMaterialMonthDemandPlan where (select CONVERT(varchar(7), InsertTime, 120))='" + ym + "' and Examinestatus<>'未发起'").ToDataTable();
            return ret;
        }

        /// <summary>
        /// 查询十一号线项目 公司
        /// </summary>
        /// <returns></returns>
        public DataTable GetElevenPC()
        {
            var ret = Db.Context.FromSql("select b.CompanyCode,b.CompanyFullName,c.ProjectId,c.ProjectName from TbProjectCompany a inner join TbCompany b on a.CompanyCode=b.CompanyCode inner join TbProjectInfo c on a.ProjectId=c.ProjectId where a.CompanyCode='6247574415609954304'").ToDataTable();
            return ret;
        }

        /// <summary>
        /// 查询所属项目所有站点
        /// </summary>
        /// <returns></returns>
        public DataTable GetInCode(string code)
        {
            var ret = Db.Context.FromSql("WITH TREE AS(SELECT * FROM TbCompany WHERE CompanyCode = '" + code + "' UNION ALL SELECT TbCompany.* FROM TbCompany, TREE WHERE TbCompany.ParentCompanyCode = TREE.CompanyCode) SELECT a.CompanyCode,a.CompanyFullName FROM TREE a inner join TbCompany b on a.CompanyCode=b.CompanyCode where a.CompanyCode not in ('6386683214299275264','6386683729561128960','6386683947165814784') and b.OrgType='5';").ToDataTable();
            return ret;
        }

        /// <summary>
        /// 查询已发信息
        /// </summary>
        /// <returns></returns>
        public TbSMSAlert GetYF(string bcode,string site)
        {
            var ret = Db.Context.FromSql("select * from TbSMSAlert where BusinessCode='" + bcode + "' and SiteCode='" + site + "' and DXType <> '10' order by InsertTime desc").ToFirst<TbSMSAlert>();
            return ret;
        }

        /// <summary>
        /// 查询成员
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public DataTable GetUser(string code) 
        {
            string sql = @"select a.ProjectId,a.ProjectName,b.CompanyCode,g.CompanyFullName,b.OrgType,c.DepartmentCode,c.DepartmentId,
c.DepartmentName,d.RoleId,d.RoleName,e.UserCode,f.UserName,f.MobilePhone 
from TbProjectInfo a inner join TbProjectCompany b on a.ProjectId=b.ProjectId 
inner join TbDepartment c on a.ProjectId=c.DepartmentProjectId 
inner join TbRole d on c.DepartmentId=d.DepartmentId 
inner join TbUserRole e on d.RoleId=e.RoleCode and e.DeptId=c.DepartmentId 
and e.OrgId=b.CompanyCode and e.ProjectId=a.ProjectId 
inner join TbUser f on e.UserCode=f.UserId inner join TbCompany g on b.CompanyCode=g.CompanyCode 
where b.CompanyCode='" + code+@"' and a.ProjectId='6245721945602523136' and b.OrgType='5' 
and c.DepartmentType='5' and c.DepartmentCode='B40050' and e.OrgType='5'";
            var ret = Db.Context.FromSql(sql).ToDataTable();
            return ret;
        }

        /// <summary>
        /// 查询部长
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public DataTable GetMinisterUser(string code, string rcode, string dcode, string type)
        {
            string sql = @"select a.ProjectId,a.ProjectName,b.CompanyCode,b.OrgType,c.DepartmentCode,c.DepartmentId,
c.DepartmentName,d.RoleId,d.RoleName,e.UserCode,f.UserName,f.MobilePhone 
from TbProjectInfo a inner join TbProjectCompany b on a.ProjectId=b.ProjectId 
inner join TbDepartment c on a.ProjectId=c.DepartmentProjectId 
inner join TbRole d on c.DepartmentId=d.DepartmentId 
inner join TbUserRole e on d.RoleId=e.RoleCode and e.DeptId=c.DepartmentId 
and e.OrgId=b.CompanyCode and e.ProjectId=a.ProjectId 
inner join TbUser f on e.UserCode=f.UserId
where b.CompanyCode='" + code + "' and a.ProjectId='6245721945602523136' and b.OrgType='" + type + @"' 
and c.DepartmentType='" + type + "' and c.DepartmentCode='" + dcode + "' and e.OrgType='" + type + "' and e.RoleCode='" + rcode + "'";
            var ret = Db.Context.FromSql(sql).ToDataTable();
            return ret;
        }

        /// <summary>
        /// 新增进度
        /// </summary>
        /// <param name="tbsms"></param>
        /// <returns></returns>
        public int Add(TbSMSAlert tbsms) 
        {
            var count = Db.Context.Insert<TbSMSAlert>(tbsms);
            if (count > 0)
            {
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// 原材料需求计划
        /// </summary>
        /// <returns></returns>
        public DataTable GetExaminestatus()
        {
            var sql = @"select a.FormDataCode,a.FlowCode,a.FlowPerformID,a.UserCode,a.FormCode,e.MenuName,c.FlowNodeCode,
c.PreNodeCompleteDate,c.PerformDate,d.UserName,d.MobilePhone,f.CompanyFullName,b.SiteCode 
from TbFlowPerform a 
inner join TbRawMaterialMonthDemandPlan b on a.FormDataCode=b.ID 
left join TbFlowPerformOpinions c on a.FlowPerformID=c.FlowPerformID 
left join TbUser d on a.UserCode=d.UserCode 
left join TbSysMenu e on a.FormCode=e.MenuCode 
left join TbCompany f on b.SiteCode=f.CompanyCode 
where b.Examinestatus='审批中' and a.FormCode='RawMonthDemandPlan' 
union all 
select a.FormDataCode,a.FlowCode,a.FlowPerformID,a.UserCode,a.FormCode,e.MenuName,c.FlowNodeCode,
c.PreNodeCompleteDate,c.PerformDate,d.UserName,d.MobilePhone,f.CompanyFullName,b.SiteCode 
from TbFlowPerform a 
inner join TbRawMaterialMonthDemandSupplyPlan b on a.FormDataCode=b.ID 
left join TbFlowPerformOpinions c on a.FlowPerformID=c.FlowPerformID 
left join TbUser d on a.UserCode=d.UserCode 
left join TbSysMenu e on a.FormCode=e.MenuCode 
left join TbCompany f on b.SiteCode=f.CompanyCode 
where b.Examinestatus='审批中' and a.FormCode='RawMonthDemandSupplyPlan' 
union all 
select a.FormDataCode,a.FlowCode,a.FlowPerformID,a.UserCode,a.FormCode,e.MenuName,c.FlowNodeCode,
c.PreNodeCompleteDate,c.PerformDate,d.UserName,d.MobilePhone,f.CompanyFullName,b.SiteCode 
from TbFlowPerform a 
inner join TbFactoryBatchNeedPlan b on a.FormDataCode=b.ID 
left join TbFlowPerformOpinions c on a.FlowPerformID=c.FlowPerformID 
left join TbUser d on a.UserCode=d.UserCode 
left join TbSysMenu e on a.FormCode=e.MenuCode 
left join TbCompany f on b.SiteCode=f.CompanyCode 
where b.Examinestatus='审批中' and a.FormCode='FactoryBatchNeedPlan'
union all 
select a.FormDataCode,a.FlowCode,a.FlowPerformID,a.UserCode,a.FormCode,e.MenuName,c.FlowNodeCode,
c.PreNodeCompleteDate,c.PerformDate,d.UserName,d.MobilePhone,f.CompanyFullName,b.SiteCode 
from TbFlowPerform a 
inner join TbRMProductionMaterial b on a.FormDataCode=b.ID 
left join TbFlowPerformOpinions c on a.FlowPerformID=c.FlowPerformID 
left join TbUser d on a.UserCode=d.UserCode 
left join TbSysMenu e on a.FormCode=e.MenuCode 
left join TbCompany f on b.SiteCode=f.CompanyCode 
where b.Examinestatus='审批中' and a.FormCode='RMProductionMaterial'
union all 
select a.FormDataCode,a.FlowCode,a.FlowPerformID,a.UserCode,a.FormCode,e.MenuName,c.FlowNodeCode,
c.PreNodeCompleteDate,c.PerformDate,d.UserName,d.MobilePhone,f.CompanyFullName,b.SiteCode 
from TbFlowPerform a 
inner join TbInOrder b on a.FormDataCode=b.ID 
left join TbFlowPerformOpinions c on a.FlowPerformID=c.FlowPerformID 
left join TbUser d on a.UserCode=d.UserCode 
left join TbSysMenu e on a.FormCode=e.MenuCode 
left join TbCompany f on b.SiteCode=f.CompanyCode 
where b.Examinestatus='审批中' and a.FormCode='InOrder'
union all 
select a.FormDataCode,a.FlowCode,a.FlowPerformID,a.UserCode,a.FormCode,e.MenuName,c.FlowNodeCode,
c.PreNodeCompleteDate,c.PerformDate,d.UserName,d.MobilePhone,f.CompanyFullName,b.SiteCode 
from TbFlowPerform a 
inner join TbOrderProgress b on a.FormDataCode=b.ID 
left join TbFlowPerformOpinions c on a.FlowPerformID=c.FlowPerformID 
left join TbUser d on a.UserCode=d.UserCode 
left join TbSysMenu e on a.FormCode=e.MenuCode 
left join TbCompany f on b.SiteCode=f.CompanyCode 
where b.Examinestatus='审批中' and a.FormCode='WorkOrder'
union all 
select a.FormDataCode,a.FlowCode,a.FlowPerformID,a.UserCode,a.FormCode,e.MenuName,c.FlowNodeCode,
c.PreNodeCompleteDate,c.PerformDate,d.UserName,d.MobilePhone,f.CompanyFullName,b.SiteCode 
from TbFlowPerform a 
inner join TbPersonnelWorkDayConsume b on a.FormDataCode=b.ID 
left join TbFlowPerformOpinions c on a.FlowPerformID=c.FlowPerformID 
left join TbUser d on a.UserCode=d.UserCode 
left join TbSysMenu e on a.FormCode=e.MenuCode 
left join TbCompany f on b.SiteCode=f.CompanyCode 
where b.Examinestatus='审批中' and a.FormCode='PersonnelWorkDayConsume'
union all 
select a.FormDataCode,a.FlowCode,a.FlowPerformID,a.UserCode,a.FormCode,e.MenuName,c.FlowNodeCode,
c.PreNodeCompleteDate,c.PerformDate,d.UserName,d.MobilePhone,f.CompanyFullName,b.SiteCode 
from TbFlowPerform a 
inner join TbProblemOrder b on a.FormDataCode=b.ID 
left join TbFlowPerformOpinions c on a.FlowPerformID=c.FlowPerformID 
left join TbUser d on a.UserCode=d.UserCode 
left join TbSysMenu e on a.FormCode=e.MenuCode 
left join TbCompany f on b.SiteCode=f.CompanyCode 
where b.Examinestatus='审批中' and a.FormCode='ProblemOrder'
union all 
select a.FormDataCode,a.FlowCode,a.FlowPerformID,a.UserCode,a.FormCode,e.MenuName,c.FlowNodeCode,
c.PreNodeCompleteDate,c.PerformDate,d.UserName,d.MobilePhone,f.CompanyFullName,b.SiteCode 
from TbFlowPerform a 
inner join TbSiteDischargeCargo b on a.FormDataCode=b.ID 
left join TbFlowPerformOpinions c on a.FlowPerformID=c.FlowPerformID 
left join TbUser d on a.UserCode=d.UserCode 
left join TbSysMenu e on a.FormCode=e.MenuCode 
left join TbCompany f on b.SiteCode=f.CompanyCode 
where b.Examinestatus='审批中' and a.FormCode='SiteDischargeCargo' 
union all 
select a.FormDataCode,a.FlowCode,a.FlowPerformID,a.UserCode,a.FormCode,e.MenuName,c.FlowNodeCode,
c.PreNodeCompleteDate,c.PerformDate,d.UserName,d.MobilePhone,f.CompanyFullName,b.SiteCode 
from TbFlowPerform a 
inner join TbMachineCost b on a.FormDataCode=b.ID 
left join TbFlowPerformOpinions c on a.FlowPerformID=c.FlowPerformID 
left join TbUser d on a.UserCode=d.UserCode 
left join TbSysMenu e on a.FormCode=e.MenuCode 
left join TbCompany f on b.SiteCode=f.CompanyCode 
where b.Examinestatus='审批中' and a.FormCode='MachineCost' 
union all 
select a.FormDataCode,a.FlowCode,a.FlowPerformID,a.UserCode,a.FormCode,e.MenuName,c.FlowNodeCode,
c.PreNodeCompleteDate,c.PerformDate,d.UserName,d.MobilePhone,f.CompanyFullName,b.SiteCode 
from TbFlowPerform a 
inner join TbUserCost b on a.FormDataCode=b.ID 
left join TbFlowPerformOpinions c on a.FlowPerformID=c.FlowPerformID 
left join TbUser d on a.UserCode=d.UserCode 
left join TbSysMenu e on a.FormCode=e.MenuCode 
left join TbCompany f on b.SiteCode=f.CompanyCode 
where b.Examinestatus='审批中' and a.FormCode='UserCost' 
union all 
select a.FormDataCode,a.FlowCode,a.FlowPerformID,a.UserCode,a.FormCode,e.MenuName,c.FlowNodeCode,
c.PreNodeCompleteDate,c.PerformDate,d.UserName,d.MobilePhone,f.CompanyFullName,b.SiteCode 
from TbFlowPerform a 
inner join TbValuationDeclare b on a.FormDataCode=b.ID 
left join TbFlowPerformOpinions c on a.FlowPerformID=c.FlowPerformID 
left join TbUser d on a.UserCode=d.UserCode 
left join TbSysMenu e on a.FormCode=e.MenuCode 
left join TbCompany f on b.SiteCode=f.CompanyCode 
where b.Examinestatus='审批中' and a.FormCode='ValuationDeclare' 
union all 
select a.FormDataCode,a.FlowCode,a.FlowPerformID,a.UserCode,a.FormCode,e.MenuName,c.FlowNodeCode,
c.PreNodeCompleteDate,c.PerformDate,d.UserName,d.MobilePhone,f.CompanyFullName,b.SiteCode 
from TbFlowPerform a 
inner join TbSignforDuiZhang b on a.FormDataCode=b.ID 
left join TbFlowPerformOpinions c on a.FlowPerformID=c.FlowPerformID 
left join TbUser d on a.UserCode=d.UserCode 
left join TbSysMenu e on a.FormCode=e.MenuCode 
left join TbCompany f on b.SiteCode=f.CompanyCode 
where b.Examinestatus='审批中' and a.FormCode='SignforDuiZhang' 
union all 
select a.FormDataCode,a.FlowCode,a.FlowPerformID,a.UserCode,a.FormCode,e.MenuName,c.FlowNodeCode,
c.PreNodeCompleteDate,c.PerformDate,d.UserName,d.MobilePhone,f.CompanyFullName,b.SiteCode 
from TbFlowPerform a 
inner join TbSettlementOrder b on a.FormDataCode=b.ID 
left join TbFlowPerformOpinions c on a.FlowPerformID=c.FlowPerformID 
left join TbUser d on a.UserCode=d.UserCode 
left join TbSysMenu e on a.FormCode=e.MenuCode 
left join TbCompany f on b.SiteCode=f.CompanyCode 
where b.Examinestatus='审批中' and a.FormCode='SettlementOrder' ";
            var newDataTable = Db.Context.FromSql(sql).ToDataTable();
            return newDataTable;
        }

        public DataTable GetInStock() 
        {
            var sql = @"select 
a.*,c.DictionaryText,d.BeginTime,b.ID as InID 
from (select * from TbFactoryBatchNeedPlan where Examinestatus='审核完成') a 
left join (select * from TbInOrder where Examinestatus='审核完成') b on a.BatchPlanNum=b.BatchPlanCode 
left join TbSysDictionaryData c on a.SteelsTypeCode=c.DictionaryCode 
left join (select * from TbFlowPerform where FormCode='FactoryBatchNeedPlan' and FlowState<>3) d on a.ID=d.FormDataCode";
            return Db.Context.FromSql(sql).ToDataTable();
        }

        /// <summary>
        /// 根据加工厂Code查询加工厂物机部人员
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public string GetJGCWJUser(string code) 
        {
            var sql = @"select 
a.ProjectId,a.ProjectName,
b.CompanyCode,b.OrgType,
c.DepartmentId,c.DepartmentCode,c.DepartmentType,c.DepartmentName,
d.RoleCode,d.RoleId,d.RoleName,e.DeptId,e.UserCode,f.UserName,f.MobilePhone 
from TbProjectInfo a 
inner join TbProjectCompany b on a.ProjectId=b.ProjectId 
inner join TbDepartment c on a.ProjectId=c.DepartmentProjectId 
inner join TbRole d on c.DepartmentId=d.DepartmentId 
inner join TbUserRole e on e.RoleCode = d.RoleCode 
inner join TbUser f on f.UserId=e.UserCode 
where 
a.ProjectId='6245721945602523136' and b.OrgType='1' 
and b.CompanyCode='"+code+"' and c.DepartmentCode='B70002' and c.DepartmentType='1' and e.OrgType='1' and (d.RoleCode='B700021' or d.RoleCode='B700022')";
            string phone = "";
            var user = Db.Context.FromSql(sql).ToDataTable();
            for (var i = 0; i < user.Rows.Count; i++) 
            {
                if (i == 0)
                {
                    phone += user.Rows[i]["MobilePhone"].ToString();
                }
                else 
                {
                    phone += "," + user.Rows[i]["MobilePhone"].ToString();
                }
            }
            return phone;
        }

        /// <summary>
        /// 运输车辆运输到工点后，超过0.5小时，进行提醒—加工厂工程部
        /// </summary>
        /// <returns></returns>
        public DataTable GetKaiShiXieHuo() 
        {
            string sql = @"select 
a.DistributionCode,a.FlowType,a.InsertTime,b.ProcessFactoryCode,
b.SiteCode,c.CompanyFullName,b.TypeCode 
from TbTransportFlow a 
left join TbDistributionEnt b on a.DistributionCode=b.Examinestatus 
left join TbCompany c on b.SiteCode=c.CompanyCode 
where a.FlowType=2";
            var tranEnt = Db.Context.FromSql(sql).ToDataTable();
            tranEnt.Columns.Add(new DataColumn("Phone"));
            if (tranEnt.Rows.Count != 0) 
            {
                for (var i = 0; i < tranEnt.Rows.Count; i++) 
                {
                    var ccode = tranEnt.Rows[i]["ProcessFactoryCode"].ToString();
                    var ussql = @"select 
a.ProjectId,a.ProjectName,
b.CompanyCode,b.OrgType,
c.DepartmentId,c.DepartmentCode,c.DepartmentType,c.DepartmentName,
d.RoleCode,d.RoleId,d.RoleName,e.DeptId,e.UserCode,f.UserName,f.MobilePhone 
from TbProjectInfo a 
inner join TbProjectCompany b on a.ProjectId=b.ProjectId 
inner join TbDepartment c on a.ProjectId=c.DepartmentProjectId 
inner join TbRole d on c.DepartmentId=d.DepartmentId 
inner join TbUserRole e on e.RoleCode = d.RoleCode 
inner join TbUser f on f.UserId=e.UserCode 
where 
a.ProjectId='6245721945602523136' and b.OrgType='1' 
and b.CompanyCode='" + ccode + "' and c.DepartmentCode='B70001' and c.DepartmentType='1' and e.OrgType='1' and (d.RoleCode='B700011' or d.RoleCode='B700012')";
                    var usen = Db.Context.FromSql(ussql).ToDataTable();
                    if (usen.Rows.Count != 0)
                    {
                        var phone = "";
                        for (var j = 0; j < usen.Rows.Count; j++)
                        {
                            if (j == 0)
                            {
                                phone += usen.Rows[j]["MobilePhone"].ToString();//usen[j].MobilePhone;
                            }
                            else 
                            {
                                phone += "," + usen.Rows[j]["MobilePhone"].ToString();
                            }
                        }
                        tranEnt.Rows[i]["Phone"] = phone;
                    }
                }
            }
            return tranEnt;
        }


        public DataTable UnloadingWaiting(string code)
        {
            string sql = @"select 
a.DistributionCode,a.FlowType,a.InsertTime,b.ProcessFactoryCode,
b.SiteCode,c.CompanyFullName,b.TypeCode 
from TbTransportFlow a 
left join TbDistributionEnt b on a.DistributionCode=b.Examinestatus 
left join TbCompany c on b.SiteCode=c.CompanyCode 
where a.DistributionCode='" + code + "'";
            var tranEnt = Db.Context.FromSql(sql).ToDataTable();
            tranEnt.Columns.Add(new DataColumn("Phone"));
            if (tranEnt.Rows.Count != 0)
            {
                for (var i = 0; i < tranEnt.Rows.Count; i++)
                {
                    //tranEnt.Rows[i]["SiteCode"].ToString()
                    var ussql = @"select a.ProjectId,a.ProjectName,b.CompanyCode,b.OrgType,c.DepartmentCode,c.DepartmentId,
c.DepartmentName,d.RoleId,d.RoleName,e.UserCode,f.UserName,f.MobilePhone 
from TbProjectInfo a inner join TbProjectCompany b on a.ProjectId=b.ProjectId 
inner join TbDepartment c on a.ProjectId=c.DepartmentProjectId 
inner join TbRole d on c.DepartmentId=d.DepartmentId 
inner join TbUserRole e on d.RoleId=e.RoleCode and e.DeptId=c.DepartmentId 
and e.OrgId=b.CompanyCode and e.ProjectId=a.ProjectId 
inner join TbUser f on e.UserCode=f.UserId
where b.CompanyCode=(select cy.ParentCompanyCode from TbCompany cy where cy.CompanyCode='"+tranEnt.Rows[i]["SiteCode"].ToString()+@"') and a.ProjectId='6245721945602523136' and b.OrgType='4' 
and c.DepartmentType='4' and c.DepartmentCode='B30020' and e.OrgType='4' and e.RoleCode='6337324141988941824'";
                    var usen = Db.Context.FromSql(ussql).ToDataTable();
                    if (usen.Rows.Count != 0)
                    {
                        var phone = "";
                        for (var j = 0; j < usen.Rows.Count; j++)
                        {
                            if (j == 0)
                            {
                                phone += usen.Rows[j]["MobilePhone"].ToString();
                            }
                            else
                            {
                                phone += "," + usen.Rows[j]["MobilePhone"].ToString();
                            }
                        }
                        tranEnt.Rows[i]["Phone"] = phone;
                    }
                }
            }
            return tranEnt;
        }

        /// <summary>
        /// 查询是否发过短信
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public DataTable GetDXData(string bcode,string code) 
        {
            var sql = "select * from TbSMSAlert where BusinessCode = '" + bcode + "' and DataCode='" + code + "' and DXType='10'";
            return Db.Context.FromSql(sql).ToDataTable();
        }
    }
}
