using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Business
{
    public class FlowNodeRelationLogic : Repository<TbFlowNodeRelation>
    {
        public AjaxResult AddFlowNodeRelation(TbFlowNodeRelation model)
        {
            try
            {
                if (model != null) Insert(model);
                return AjaxResult.Success();
            }
            catch (Exception)
            {
                return AjaxResult.Error();
            }
        }
    }
}
