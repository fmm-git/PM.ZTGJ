using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dos.ORM;

namespace PM.DataEntity
{
    /// <summary>
    /// 数据字典类别
    /// </summary>
    /// <summary>
    /// 实体类TbSysDictionaryType。(属性说明自动提取数据库字段的描述信息)
    /// </summary>
    [Table("TbSysDictionaryType")]
    [Serializable]
    public partial class TbSysDictionaryType : Entity
    {
        #region Model
        private int _id;
        private string _DictionaryCode;
        private string _DictionaryType;
        private string _PDictionaryCode;
        private string _FullCode;

        /// <summary>
        /// 
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
        /// 字典分类代码
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
        /// 字典类别
        /// </summary>
        [Field("DictionaryType")]
        public string DictionaryType
        {
            get { return _DictionaryType; }
            set
            {
                this.OnPropertyValueChange("DictionaryType");
                this._DictionaryType = value;
            }
        }
        /// <summary>
        /// 数据字典类别Code
        /// </summary>
        [Field("PDictionaryCode")]
        public string PDictionaryCode
        {
            get { return _PDictionaryCode; }
            set
            {
                this.OnPropertyValueChange("PDictionaryCode");
                this._PDictionaryCode = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        [Field("FullCode")]
        public string FullCode
        {
            get { return _FullCode; }
            set
            {
                this.OnPropertyValueChange("FullCode");
                this._FullCode = value;
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
				_.DictionaryCode,
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
				_.DictionaryCode,
				_.DictionaryType,
				_.PDictionaryCode,
				_.FullCode,
			};
        }
        /// <summary>
        /// 获取值信息
        /// </summary>
        public override object[] GetValues()
        {
            return new object[] {
				this._id,
				this._DictionaryCode,
				this._DictionaryType,
				this._PDictionaryCode,
				this._FullCode,
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
            public readonly static Field All = new Field("*", "TbSysDictionaryType");
            /// <summary>
            /// 
            /// </summary>
            public readonly static Field id = new Field("id", "TbSysDictionaryType", "");
            /// <summary>
            /// 字典分类代码
            /// </summary>
            public readonly static Field DictionaryCode = new Field("DictionaryCode", "TbSysDictionaryType", "字典分类代码");
            /// <summary>
            /// 字典类别
            /// </summary>
            public readonly static Field DictionaryType = new Field("DictionaryType", "TbSysDictionaryType", "字典类别");
            /// <summary>
            /// 数据字典类别Code
            /// </summary>
            public readonly static Field PDictionaryCode = new Field("PDictionaryCode", "TbSysDictionaryType", "数据字典类别Code");
            /// <summary>
            /// 
            /// </summary>
            public readonly static Field FullCode = new Field("FullCode", "TbSysDictionaryType", "");
        }
        #endregion

    }
}
