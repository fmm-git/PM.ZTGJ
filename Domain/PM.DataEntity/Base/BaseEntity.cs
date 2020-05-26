using Dos.ORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.DataEntity.Base
{
    public class BaseEntity: Entity
    {
        #region Model

        ///<summary>
        /// 项目Id
        /// </summary>
        public string _ProjectId;

        /// <summary>
        /// 项目Id
        /// </summary>
        [Field("ProjectId")]
        public string ProjectId
        {
            get { return _ProjectId; }
            set
            {
                this.OnPropertyValueChange("ProjectId");
                this._ProjectId = value;
            }
        }

        #endregion
    }
}
