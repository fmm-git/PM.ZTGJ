﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.18408
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
    /// 我的收藏
    /// </summary>
    [Table("TbArticleCollect")]
    [Serializable]
    public partial class TbArticleCollect : Entity
    {
        #region Model
		private int _ID;
		private int _ArticleID;
		private string _InsertUserCode;
		private DateTime _InsertTime;

		/// <summary>
		/// ID
		/// </summary>
		[Field("ID")]
		public int ID
		{
			get{ return _ID; }
			set
			{
				this.OnPropertyValueChange("ID");
				this._ID = value;
			}
		}
		/// <summary>
		/// 文章ID
		/// </summary>
		[Field("ArticleID")]
		public int ArticleID
		{
			get{ return _ArticleID; }
			set
			{
				this.OnPropertyValueChange("ArticleID");
				this._ArticleID = value;
			}
		}
		/// <summary>
		/// 录入人
		/// </summary>
		[Field("InsertUserCode")]
		public string InsertUserCode
		{
			get{ return _InsertUserCode; }
			set
			{
				this.OnPropertyValueChange("InsertUserCode");
				this._InsertUserCode = value;
			}
		}
		/// <summary>
		/// 录入时间
		/// </summary>
		[Field("InsertTime")]
		public DateTime InsertTime
		{
			get{ return _InsertTime; }
			set
			{
				this.OnPropertyValueChange("InsertTime");
				this._InsertTime = value;
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
				_.ID,
			};
        }
		/// <summary>
        /// 获取实体中的标识列
        /// </summary>
        public override Field GetIdentityField()
        {
            return _.ID;
        }
        /// <summary>
        /// 获取列信息
        /// </summary>
        public override Field[] GetFields()
        {
            return new Field[] {
				_.ID,
				_.ArticleID,
				_.InsertUserCode,
				_.InsertTime,
			};
        }
        /// <summary>
        /// 获取值信息
        /// </summary>
        public override object[] GetValues()
        {
            return new object[] {
				this._ID,
				this._ArticleID,
				this._InsertUserCode,
				this._InsertTime,
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
			public readonly static Field All = new Field("*", "TbArticleCollect");
            /// <summary>
			/// ID
			/// </summary>
			public readonly static Field ID = new Field("ID", "TbArticleCollect", "ID");
            /// <summary>
			/// 文章ID
			/// </summary>
			public readonly static Field ArticleID = new Field("ArticleID", "TbArticleCollect", "文章ID");
            /// <summary>
			/// 录入人
			/// </summary>
			public readonly static Field InsertUserCode = new Field("InsertUserCode", "TbArticleCollect", "录入人");
            /// <summary>
			/// 录入时间
			/// </summary>
			public readonly static Field InsertTime = new Field("InsertTime", "TbArticleCollect", "录入时间");
        }
        #endregion
	}
}