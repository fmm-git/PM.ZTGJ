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
    public class FlowNodePersonnelLogic : Repository<TbFlowNodePersonnel>
    {
        public AjaxResult AddPersonnel(TbFlowNodePersonnel t)
        {
            try
            {
                Insert(t);
                return AjaxResult.Success();
            }
            catch (Exception)
            {
                return AjaxResult.Error();
            }
        }
    }
}
