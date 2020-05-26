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

namespace PM.DataAccess.Flow
{
    public class FlowNodeUIDA : Repository<TbFlowNodeUI>
    {
        public int UpdataFlowNodeUI(string FlowCode, string FlowNodeCode, string NodeLeft, string NodeTop, string processData)
        {
            int result = Db.Context.FromSql("Update TbFlowNodeUI Set processData=@processData, NodeLeft=@NodeLeft,NodeTop=@NodeTop Where FlowCode=@FlowCode and FlowNodeCode=@FlowNodeCode ")
                .AddInParameter("@NodeLeft", DbType.String, NodeLeft)
                .AddInParameter("@NodeTop", DbType.String, NodeTop)
                .AddInParameter("@FlowCode", DbType.String, FlowCode)
                .AddInParameter("@processData", DbType.String, processData)
                .AddInParameter("@FlowNodeCode", DbType.String, FlowNodeCode).ExecuteNonQuery();
            return result;
        }
        public int AddNodeUI(List<TbFlowNodeUI> list)
        {
            DbTrans trans = null;
            try
            {
                using (trans = Db.Context.BeginTransaction())
                {
                    FlowNodeUIDA.Insert(list);
                    trans.Commit();
                    return 1;
                }
            }
            catch (Exception)
            {
                if (trans != null) trans.Rollback();
                return 0;
            }
        }
        public int LineAdd(string FlowCode, string strNode, string endNode)
        {
            int result = Db.Context.FromProc("FlowLineAdd")
                .AddInParameter("@FlowCode", DbType.String, FlowCode)
                .AddInParameter("@StartCode", DbType.String, strNode)
                .AddInParameter("@EndCode", DbType.String, endNode)
                .ExecuteNonQuery();
            return result;
        }
        public List<TbFlowNodeUI> GetList(string flowCode)
        {
            var where = new Where<TbFlowNodeUI>();
            where.And(p => p.FlowCode == flowCode);
            return Db.Context.From<TbFlowNodeUI>().Select(TbFlowNodeUI._.All).Where(where).ToList<TbFlowNodeUI>() ;
        }
    }
}
