using Dos.ORM;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.DataAccess.Flow
{
    public class FlowNodeJudgeCriteriaDA : Repository<TbFlowNodeJudgeCriteria>
    {
        /// <summary>
        /// 通过流程编码找到对应的业务单据数据存储表
        /// </summary>
        /// <param name="FlowCode">流程编码</param>
        /// <returns></returns>
        public string GetTableNameByFlowCode(string FlowCode)
        {
            string sql = "select TableName from TbSysMenuTable where MenuCode in (select FormCode from TbFlowDefine where FlowCode=@FlowCode) and IsMainTabel=0";
            DataTable dt = Db.Context.FromSql(sql).AddInParameter("@FlowCode", DbType.String, FlowCode).ToDataTable();
            if (dt != null && dt.Rows.Count > 0) return Convert.ToString(dt.Rows[0]["TableName"]);
            else return "";
        }
        /// <summary>
        /// 通过流程编码及节点编码获取流程条件
        /// </summary>
        /// <param name="FlowCode"></param>
        /// <param name="FlowNodeCode"></param>
        /// <returns></returns>
        public DataTable GetJudgeCriteriaByFlowNode(string FlowCode, string FlowNodeCode)
        {
            string sql = @"select d.id, d.FieldCode,d.FieldName,d.JudgeSymbol,
                case d.JudgeSymbol when '=' then '等于' 
                when '>' then '大于'
                when '=>' then '大于等于'
                when '<' then '小于'
                when '<=' then '小于等于'
                when 'like' then '包含'
                when 'notlike' then '不包含'
                end as JudgeSymbolText ,
                d.JudgeValue,
                d.JudgeRelation,
                case d.JudgeRelation when '' then '' when 'or' then '或者' when 'and' then '并且' end as JudgeRelationText
                 from TbFlowNodeJudgeCriteria d
                 where d.FlowCode=@FlowCode and d.FlowNodeCode=@FlowNodeCode";
            return Db.Context.FromSql(sql).AddInParameter("@FlowCode", DbType.String, FlowCode).AddInParameter("@FlowNodeCode", DbType.String, FlowNodeCode).ToDataTable();
        }
        /// <summary>
        /// 判断当前节点能否设置条件
        /// </summary>
        /// <param name="FlowCode"></param>
        /// <param name="FlowNodeCode"></param>
        /// <returns></returns>
        public bool MathNodeJudgeCriter(string FlowCode, string FlowNodeCode)
        {
            string sql = @"select COUNT(1) as ParentChildNum from TbFlowNodeRelation where FlowCode=@FlowCode and ParentNodeCode in 
                            ( select ParentNodeCode from TbFlowNodeRelation  where   FlowCode=@FlowCode and ChildNodeCode=@FlowNodeCode)
                            ";
            DataTable dt = Db.Context.FromSql(sql).AddInParameter("@FlowCode", DbType.String, FlowCode).AddInParameter("@FlowNodeCode", DbType.String, FlowNodeCode).ToDataTable();
            if (dt != null && dt.Rows.Count > 0)
            {
                int num = Convert.ToInt32(dt.Rows[0]["ParentChildNum"]);
                if (num > 1) { return true; }
                else {return false;}
            }
            else
            {
                return false;
            }
            
        }
        
    }
}
