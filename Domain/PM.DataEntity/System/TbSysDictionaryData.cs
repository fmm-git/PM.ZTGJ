using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dos.ORM;

namespace PM.DataEntity
{
    /// <summary>
    /// 数据字典类
    /// </summary>
    /// <summary>
    /// 实体类TbSysDictionaryData。(属性说明自动提取数据库字段的描述信息)
    /// </summary>
    [Table("TbSysDictionaryData")]
    [Serializable]
    public partial class TbSysDictionaryData : Entity
    {
        #region Model
        private int _id;
        private string _FDictionaryCode;
        private string _DictionaryCode;
        private string _DictionaryText;
        private int _DictionaryOrder;
        private string _DictionaryValue;

        /// <summary>
        /// 标识ID
        /// </summary>
        [Field("id")]
        public int id
        {
            get { return _id; }
            set
            {
                this.OnPropertyValueChange("id");
                this._id = value;
            }
        }
        /// <summary>
        /// 字典类别Code
        /// </summary>
        [Field("FDictionaryCode")]
        public string FDictionaryCode
        {
            get { return _FDictionaryCode; }
            set
            {
                this.OnPropertyValueChange("FDictionaryCode");
                this._FDictionaryCode = value;
            }
        }
        /// <summary>
        /// 字典Code
        /// </summary>
        [Field("DictionaryCode")]
        public string DictionaryCode
        {
            get { return _DictionaryCode; }
            set
            {
                this.OnPropertyValueChange("DictionaryCode");
                this._DictionaryCode = value;
            }
        }
        /// <summary>
        /// 字典名称/字典内容
        /// </summary>
        [Field("DictionaryText")]
        public string DictionaryText
        {
            get { return _DictionaryText; }
            set
            {
                this.OnPropertyValueChange("DictionaryText");
                this._DictionaryText = value;
            }
        }
        /// <summary>
        /// 排序顺序
        /// </summary>
        [Field("DictionaryOrder")]
        public int DictionaryOrder
        {
            get { return _DictionaryOrder; }
            set
            {
                this.OnPropertyValueChange("DictionaryOrder");
                this._DictionaryOrder = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        [Field("DictionaryValue")]
        public string DictionaryValue
        {
            get { return _DictionaryValue; }
            set
            {
                this.OnPropertyValueChange("DictionaryValue");
                this._DictionaryValue = value;
            }
        }
        #endregion

        #region Method
        /// <summary>
        /// 获取实体中的主键列
        /// </summary>
        public override Field[] GetPrimaryKeyFields()
        {
            return new Field[] {
				_.id
			};
        }
        /// <summary>
        /// 获取实体中的标识列
        /// </summary>
        public override Field GetIdentityField()
        {
            return _.id;
        }
        /// <summary>
        /// 获取列信息
        /// </summary>
        public override Field[] GetFields()
        {
            return new Field[] {
				_.id,
                _.FDictionaryCode,
				_.DictionaryCode,
				_.DictionaryText,
				_.DictionaryOrder,
				_.DictionaryValue,
			};
        }
        /// <summary>
        /// 获取值信息
        /// </summary>
        public override object[] GetValues()
        {
            return new object[] {
				this._id,
                this._FDictionaryCode,
				this._DictionaryCode,
				this._DictionaryText,
				this._DictionaryOrder,
				this._DictionaryValue,
			};
        }
        /// <summary>
        /// 是否是v1.10.5.6及以上版本实体。
        /// </summary>
        /// <returns></returns>
        public override bool V1_10_5_6_Plus()
        {
            return true;
        }
        #endregion

        #region _Field
        /// <summary>
        /// 字段信息
        /// </summary>
        public class _
        {
            /// <summary>
            /// * 
            /// </summary>
            public readonly static Field All = new Field("*", "TbSysDictionaryData");
            /// <summary>
            /// 标识ID
            /// </summary>
            public readonly static Field id = new Field("id", "TbSysDictionaryData", "标识ID");
            /// <summary>
            /// 字典类别Code
            /// </summary>
            public readonly static Field FDictionaryCode = new Field("FDictionaryCode", "TbSysDictionaryData", "字典类别Code");
            /// <summary>
            /// 字典Code
            /// </summary>
            public readonly static Field DictionaryCode = new Field("DictionaryCode", "TbSysDictionaryData", "字典Code");
            /// <summary>
            /// 字典名称/字典内容
            /// </summary>
            public readonly static Field DictionaryText = new Field("DictionaryText", "TbSysDictionaryData", "字典名称/字典内容");
            /// <summary>
            /// 排序顺序
            /// </summary>
            public readonly static Field DictionaryOrder = new Field("DictionaryOrder", "TbSysDictionaryData", "排序顺序");
            /// <summary>
            /// 
            /// </summary>
            public readonly static Field DictionaryValue = new Field("DictionaryValue", "TbSysDictionaryData", "");
        }
        #endregion

    }
}
