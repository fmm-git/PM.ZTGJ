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
    public class FlowDefineLogic : Repository<TbFlowDefine>
    {
        /// <summary>
        /// 获取菜单列表
        /// </summary>
        /// <returns>菜单列表</returns>
        public List<TbSysMenu> GetMenu()
        {
            string sql = @"with cte as
                            (
                            select * from TbSysMenu where OperationExamination='1'
                            union all
                            select c.* from cte P inner join TbSysMenu c on p.MenuPCode=c.MenuCode 
                            )
                            select distinct * from cte order by Sort ";
            return Db.Context.FromSql(sql).ToList<TbSysMenu>();
        }
        /// <summary>
        /// 获取菜单列表
        /// </summary>
        /// <returns>菜单列表</returns>
        public List<TbSysMenu> GetMenuNew()
        {
            string sql = @"with cte as
                            (
                            select * from TbSysMenu where IsNoticeOrEarly='1'
                            union all
                            select c.* from cte P inner join TbSysMenu c on p.MenuPCode=c.MenuCode 
                            )
                            select distinct * from cte order by Sort ";
            return Db.Context.FromSql(sql).ToList<TbSysMenu>();
        }
        /// <summary>
        /// 获取流程编码
        /// </summary>
        /// <returns></returns>
        public string GetFlowNumber()
        { 
            string number="FlowID";
            string sql = "select CodeValue from TbFlowCode where CodeType='FlowCode'";
            DataTable dt = Db.Context.FromSql(sql).ToDataTable();
            int value = Convert.ToInt32(dt.Rows[0]["CodeValue"]);
            number += value.ToString().PadLeft(6, '0');
            return number;
        }
        public AjaxResult Update(TbFlowDefine request)
        {
            if (request == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                //判断流程编号是否重复
                var isAnyNumber = Repository<TbFlowDefine>.Any(p => p.FlowCode == request.FlowCode&&p.FlowCode!=request.FlowCode);
                if (isAnyNumber)
                    return AjaxResult.Warning("流程编号已存在");
                var name = Repository<TbFlowDefine>.Any(p => p.FlowName == request.FlowName && p.FormCode == request.FormCode&&p.FlowCode!=request.FlowCode);
                if (name)
                    return AjaxResult.Warning("该表单已存在该流程名称");
                var model = Repository<TbFlowDefine>.First(p => p.FlowCode == request.FlowCode);
                if (model == null)
                    return AjaxResult.Error("信息不存在");
                model.FlowDetail = request.FlowDetail;
                model.FlowIsFree = request.FlowIsFree;
                model.FlowIsPublic = request.FlowIsPublic;
                model.RollbackAttribute = request.RollbackAttribute;
                model.FlowName = request.FlowName;
                model.FlowSpOrCsType = request.FlowSpOrCsType;
                var count = Repository<TbFlowDefine>.Update(model);
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
        public AjaxResult Add(TbFlowDefine request)
        {
            if (request == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                DbTrans trans = null;
                using (trans = Db.Context.BeginTransaction())
                {
                    //判断流程编号是否重复
                    var isAnyNumber = Repository<TbFlowDefine>.Any(p => p.FlowCode == request.FlowCode);
                    if (isAnyNumber)
                        return AjaxResult.Warning("流程编号已存在");
                    var name = Repository<TbFlowDefine>.Any(p => p.FlowName == request.FlowName && p.FormCode == request.FormCode);
                    if (name)
                        return AjaxResult.Warning("该表单已存在该流程名称");
                    request.FlowType = "New";
                    var count = Repository<TbFlowDefine>.Insert(request);
                    if (count > 0)
                    {
                        string sql = "update TbFlowCode set CodeValue=CodeValue+1 where CodeType='FlowCode'";
                        Db.Context.FromSql(sql).ExecuteNonQuery();
                        new FlowNodeLogic().LoadingNode(request.FlowCode);
                        return AjaxResult.Success();
                    }
                    return AjaxResult.Error("操作失败");
                }
                
            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.ToString());
            }
        }
        public TbFlowDefineResponse GetFlowDefine(string keyvalue)
        {
            var dt = Db.Context.From<TbFlowDefine>()
               .Select(
                       TbFlowDefine._.All,
                       TbSysMenu._.MenuName)
               .LeftJoin<TbSysMenu>((a, c) => a.FormCode == c.MenuCode).Where(p => p.FlowCode == keyvalue).ToDataTable();
            var list = ModelConvertHelper<TbFlowDefineResponse>.ToList(dt);
            return list[0];
        }


        public List<TbFlowDefine> GetChangeFlow(string FormCode, string ProjectId)
        {
            string sql = @"select TbFlowDefine.id,FlowCode,FlowName,FormCode,TbSysMenu.MenuName as FormName,FlowIsPublic,FlowIsFree,FlowDetail from TbFlowDefine
left join TbSysMenu on TbFlowDefine.FormCode=TbSysMenu.MenuCode where TbFlowDefine.FormCode=''+@FormCode+'' and (ISNULL(@ProjectId,'')='' or TbFlowDefine.ProjectId=@ProjectId)";
            var list = Db.Context.FromSql(sql)
                .AddInParameter("@FormCode", DbType.String, FormCode)
                .AddInParameter("@ProjectId", DbType.String, ProjectId).ToList<TbFlowDefine>();
            return list;
        }
    }
}
