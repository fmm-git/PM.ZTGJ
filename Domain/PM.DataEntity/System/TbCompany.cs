using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dos.ORM;

namespace PM.DataEntity
{
    /// <summary>
    /// 实体类TbCompany。(属性说明自动提取数据库字段的描述信息)
    /// 公司表
    /// </summary>
    [Table("TbCompany")]
    [Serializable]
    public class TbCompany : Entity
    {
        #region Model
        private int _id;
        private int _IsEnable;
        private string _CompanyCode;
        private string _ParentCompanyCode;
        private string _ParentCompanyCode_F;
        private string _FullCode;
        private string _CompanyFullName;
        private string _CompanyShortName;
        private string _EnglishName;
        private string _LocalCurrency;
        private string _Address;
        private string _Telephone;
        private string _Fax;
        private string _Website;
        private string _Email;
        private string _PostalCode;
        private string _LegalPerson;
        private string _contacter;
        private int? _OrgType;
        public string ParentCompanyName { get; set; }
        /// <summary>
        /// 项目id
        /// </summary>
        public string ProjectId { get; set; }

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
        /// 公司状态
        /// </summary>
        [Field("IsEnable")]
        public int IsEnable
        {
            get { return _IsEnable; }
            set
            {
                this.OnPropertyValueChange("IsEnable");
                this._IsEnable = value;
            }
        }

        /// <summary>
        /// 公司编码
        /// </summary>
        [Field("CompanyCode")]
        public string CompanyCode
        {
            get { return _CompanyCode; }
            set
            {
                this.OnPropertyValueChange("CompanyCode");
                this._CompanyCode = value;
            }
        }
        /// <summary>
        /// 上级公司
        /// </summary>
        [Field("ParentCompanyCode")]
        public string ParentCompanyCode
        {
            get { return _ParentCompanyCode; }
            set
            {
                this.OnPropertyValueChange("ParentCompanyCode");
                this._ParentCompanyCode = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        [Field("ParentCompanyCode_F")]
        public string ParentCompanyCode_F
        {
            get { return _ParentCompanyCode_F; }
            set
            {
                this.OnPropertyValueChange("ParentCompanyCode_F");
                this._ParentCompanyCode_F = value;
            }
        }
        /// <summary>
        /// 编码路径
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
        /// <summary>
        /// 公司全称
        /// </summary>
        [Field("CompanyFullName")]
        public string CompanyFullName
        {
            get { return _CompanyFullName; }
            set
            {
                this.OnPropertyValueChange("CompanyFullName");
                this._CompanyFullName = value;
            }
        }
        /// <summary>
        /// 公司简称
        /// </summary>
        [Field("CompanyShortName")]
        public string CompanyShortName
        {
            get { return _CompanyShortName; }
            set
            {
                this.OnPropertyValueChange("CompanyShortName");
                this._CompanyShortName = value;
            }
        }
        /// <summary>
        /// 英文名称
        /// </summary>
        [Field("EnglishName")]
        public string EnglishName
        {
            get { return _EnglishName; }
            set
            {
                this.OnPropertyValueChange("EnglishName");
                this._EnglishName = value;
            }
        }
        /// <summary>
        /// 本位币
        /// </summary>
        [Field("LocalCurrency")]
        public string LocalCurrency
        {
            get { return _LocalCurrency; }
            set
            {
                this.OnPropertyValueChange("LocalCurrency");
                this._LocalCurrency = value;
            }
        }
        /// <summary>
        /// 公司地址
        /// </summary>
        [Field("Address")]
        public string Address
        {
            get { return _Address; }
            set
            {
                this.OnPropertyValueChange("Address");
                this._Address = value;
            }
        }
        /// <summary>
        /// 联系电话
        /// </summary>
        [Field("Telephone")]
        public string Telephone
        {
            get { return _Telephone; }
            set
            {
                this.OnPropertyValueChange("Telephone");
                this._Telephone = value;
            }
        }
        /// <summary>
        /// 传真电话
        /// </summary>
        [Field("Fax")]
        public string Fax
        {
            get { return _Fax; }
            set
            {
                this.OnPropertyValueChange("Fax");
                this._Fax = value;
            }
        }
        /// <summary>
        /// 网络地址
        /// </summary>
        [Field("Website")]
        public string Website
        {
            get { return _Website; }
            set
            {
                this.OnPropertyValueChange("Website");
                this._Website = value;
            }
        }
        /// <summary>
        /// 邮件地址
        /// </summary>
        [Field("Email")]
        public string Email
        {
            get { return _Email; }
            set
            {
                this.OnPropertyValueChange("Email");
                this._Email = value;
            }
        }
        /// <summary>
        /// 邮政编码
        /// </summary>
        [Field("PostalCode")]
        public string PostalCode
        {
            get { return _PostalCode; }
            set
            {
                this.OnPropertyValueChange("PostalCode");
                this._PostalCode = value;
            }
        }
        /// <summary>
        /// 法人代表
        /// </summary>
        [Field("LegalPerson")]
        public string LegalPerson
        {
            get { return _LegalPerson; }
            set
            {
                this.OnPropertyValueChange("LegalPerson");
                this._LegalPerson = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        [Field("contacter")]
        public string contacter
        {
            get { return _contacter; }
            set
            {
                this.OnPropertyValueChange("contacter");
                this._contacter = value;
            }
        }
        /// <summary>
        /// 组织机构类型
        /// </summary>
        [Field("OrgType")]
        public int? OrgType
        {
            get { return _OrgType; }
            set
            {
                this.OnPropertyValueChange("OrgType");
                this._OrgType = value;
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
				_.CompanyCode,
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
                _.IsEnable,
				_.CompanyCode,
				_.ParentCompanyCode,
				_.ParentCompanyCode_F,
				_.FullCode,
				_.CompanyFullName,
				_.CompanyShortName,
				_.EnglishName,
				_.LocalCurrency,
				_.Address,
				_.Telephone,
				_.Fax,
				_.Website,
				_.Email,
				_.PostalCode,
				_.LegalPerson,
				_.contacter,
				_.OrgType,
			};
        }
        /// <summary>
        /// 获取值信息
        /// </summary>
        public override object[] GetValues()
        {
            return new object[] {
				this._id,
                this._IsEnable,
				this._CompanyCode,
				this._ParentCompanyCode,
				this._ParentCompanyCode_F,
				this._FullCode,
				this._CompanyFullName,
				this._CompanyShortName,
				this._EnglishName,
				this._LocalCurrency,
				this._Address,
				this._Telephone,
				this._Fax,
				this._Website,
				this._Email,
				this._PostalCode,
				this._LegalPerson,
				this._contacter,
				this._OrgType,
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
            public readonly static Field All = new Field("*", "TbCompany");
            /// <summary>
            /// 
            /// </summary>
            public readonly static Field id = new Field("id", "TbCompany", "");
            /// <summary>
            /// 公司状态
            /// </summary>
            public readonly static Field IsEnable = new Field("IsEnable", "TbCompany","公司状态");
            /// <summary>
            /// 公司编码
            /// </summary>
            public readonly static Field CompanyCode = new Field("CompanyCode", "TbCompany", "公司编码");
            /// <summary>
            /// 上级公司
            /// </summary>
            public readonly static Field ParentCompanyCode = new Field("ParentCompanyCode", "TbCompany", "上级公司");
            /// <summary>
            /// 
            /// </summary>
            public readonly static Field ParentCompanyCode_F = new Field("ParentCompanyCode_F", "TbCompany", "");
            /// <summary>
            /// 编码路径
            /// </summary>
            public readonly static Field FullCode = new Field("FullCode", "TbCompany", "编码路径");
            /// <summary>
            /// 公司全称
            /// </summary>
            public readonly static Field CompanyFullName = new Field("CompanyFullName", "TbCompany", "公司全称");
            /// <summary>
            /// 公司简称
            /// </summary>
            public readonly static Field CompanyShortName = new Field("CompanyShortName", "TbCompany", "公司简称");
            /// <summary>
            /// 英文名称
            /// </summary>
            public readonly static Field EnglishName = new Field("EnglishName", "TbCompany", "英文名称");
            /// <summary>
            /// 本位币
            /// </summary>
            public readonly static Field LocalCurrency = new Field("LocalCurrency", "TbCompany", "本位币");
            /// <summary>
            /// 公司地址
            /// </summary>
            public readonly static Field Address = new Field("Address", "TbCompany", "公司地址");
            /// <summary>
            /// 联系电话
            /// </summary>
            public readonly static Field Telephone = new Field("Telephone", "TbCompany", "联系电话");
            /// <summary>
            /// 传真电话
            /// </summary>
            public readonly static Field Fax = new Field("Fax", "TbCompany", "传真电话");
            /// <summary>
            /// 网络地址
            /// </summary>
            public readonly static Field Website = new Field("Website", "TbCompany", "网络地址");
            /// <summary>
            /// 邮件地址
            /// </summary>
            public readonly static Field Email = new Field("Email", "TbCompany", "邮件地址");
            /// <summary>
            /// 邮政编码
            /// </summary>
            public readonly static Field PostalCode = new Field("PostalCode", "TbCompany", "邮政编码");
            /// <summary>
            /// 法人代表
            /// </summary>
            public readonly static Field LegalPerson = new Field("LegalPerson", "TbCompany", "法人代表");
            /// <summary>
            /// 
            /// </summary>
            public readonly static Field contacter = new Field("contacter", "TbCompany", "");
            /// <summary>
            /// 组织机构类型
            /// </summary>
            public readonly static Field OrgType = new Field("OrgType", "TbCompany", "");
        }
        #endregion

    }
}
