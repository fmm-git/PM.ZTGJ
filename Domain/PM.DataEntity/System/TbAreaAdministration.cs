﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.36440
//     Website: http://ITdos.com/Dos/ORM/Index.html
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using Dos.ORM;

namespace PM.DataEntity
{
    /// <summary>
    /// 实体类TbAreaAdministration。(属性说明自动提取数据库字段的描述信息)
    /// </summary>
    [Table("TbAreaAdministration")]
    [Serializable]
    public partial class TbAreaAdministration : Entity
    {
        #region Model
		private int _id;
		private string _AreaCode;
		private string _AreaName;
		private string _FK_AreaCode_F;
		private string _FK_AreaCode;
		private int? _Sort;

		/// <summary>
		/// 
		/// </summary>
		[Field("id")]
		public int id
		{
			get{ return _id; }
			set
			{
				this.OnPropertyValueChange("id");
				this._id = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		[Field("AreaCode")]
		public string AreaCode
		{
			get{ return _AreaCode; }
			set
			{
				this.OnPropertyValueChange("AreaCode");
				this._AreaCode = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		[Field("AreaName")]
		public string AreaName
		{
			get{ return _AreaName; }
			set
			{
				this.OnPropertyValueChange("AreaName");
				this._AreaName = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		[Field("FK_AreaCode_F")]
		public string FK_AreaCode_F
		{
			get{ return _FK_AreaCode_F; }
			set
			{
				this.OnPropertyValueChange("FK_AreaCode_F");
				this._FK_AreaCode_F = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		[Field("FK_AreaCode")]
		public string FK_AreaCode
		{
			get{ return _FK_AreaCode; }
			set
			{
				this.OnPropertyValueChange("FK_AreaCode");
				this._FK_AreaCode = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		[Field("Sort")]
		public int? Sort
		{
			get{ return _Sort; }
			set
			{
				this.OnPropertyValueChange("Sort");
				this._Sort = value;
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
			};
        }
        /// <summary>
        /// 获取列信息
        /// </summary>
        public override Field[] GetFields()
        {
            return new Field[] {
				_.id,
				_.AreaCode,
				_.AreaName,
				_.FK_AreaCode_F,
				_.FK_AreaCode,
				_.Sort,
			};
        }
        /// <summary>
        /// 获取值信息
        /// </summary>
        public override object[] GetValues()
        {
            return new object[] {
				this._id,
				this._AreaCode,
				this._AreaName,
				this._FK_AreaCode_F,
				this._FK_AreaCode,
				this._Sort,
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
			public readonly static Field All = new Field("*", "TbAreaAdministration");
            /// <summary>
			/// 
			/// </summary>
			public readonly static Field id = new Field("id", "TbAreaAdministration", "");
            /// <summary>
			/// 
			/// </summary>
			public readonly static Field AreaCode = new Field("AreaCode", "TbAreaAdministration", "");
            /// <summary>
			/// 
			/// </summary>
			public readonly static Field AreaName = new Field("AreaName", "TbAreaAdministration", "");
            /// <summary>
			/// 
			/// </summary>
			public readonly static Field FK_AreaCode_F = new Field("FK_AreaCode_F", "TbAreaAdministration", "");
            /// <summary>
			/// 
			/// </summary>
			public readonly static Field FK_AreaCode = new Field("FK_AreaCode", "TbAreaAdministration", "");
            /// <summary>
			/// 
			/// </summary>
			public readonly static Field Sort = new Field("Sort", "TbAreaAdministration", "");
        }
        #endregion
	}
}