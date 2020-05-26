#region << 版 本 注 释 >>
/****************************************************
* 文 件 名：Sys_BaseDataLogic
* Copyright(c) 青之软件
* CLR 版本: 4.0.30319.17929
* 创 建 人：周浩
* 电子邮箱：admin@itdos.com
* 创建日期：2014/10/1 11:00:49
* 文件描述：
******************************************************
* 修 改 人：
* 修改日期：
* 备注描述：
*******************************************************/
#endregion
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Dos.Common;
using Dos.ORM;
using Dos.ORM.Common;
using System.Data.Common;
using PM.Common;

namespace PM.DataAccess.DbContext
{
    public abstract partial class Repository<T> where T : Entity,new()
    {
        static Repository()
        {
            Db.Context.RegisterSqlLogger(delegate(string sql)
            {
                //记录sql日志
                //LogHelper.Debug(sql, "SQL日志", "F:\\MyWork");
            });
        }
        #region 查询
        /// <summary>
        /// 获取整表数据
        /// </summary>
        /// <returns></returns>
        public static List<T> GetAll()
        {
            return Db.Context.From<T>().ToList();
        }
        /// <summary>
        /// 通用查询
        /// </summary>
        public static List<T> Query(Expression<Func<T, bool>> where, Expression<Func<T, object>> orderBy = null, string ascOrDesc = "asc", int? top = null, int? pageSize = null, int? pageIndex = null)
        {
            var fs = Db.Context.From<T>().Where(where);
            if (top != null)
            {
                fs.Top(top.Value);
            }
            else if (pageIndex != null && pageSize != null)
            {
                fs.Page(pageSize.Value, pageIndex.Value);
            }
            if (orderBy != null)
            {
                if (ascOrDesc.ToLower() == "asc")
                {
                    fs.OrderBy(orderBy);
                }
                else
                {
                    fs.OrderByDescending(orderBy);
                }
            }
            return fs.ToList();
        }
        /// <summary>
        /// 通用查询
        /// </summary>
        public static List<T> Query(Where<T> where, Expression<Func<T, object>> orderBy = null, string ascOrDesc = "asc", int? top = null, int? pageSize = null, int? pageIndex = null)
        {
            var fs = Db.Context.From<T>().Where(where);
            if (top != null)
            {
                fs.Top(top.Value);
            }
            else if (pageIndex != null && pageSize != null)
            {
                fs.Page(pageSize.Value, pageIndex.Value);
            }
            if (orderBy != null)
            {
                if (ascOrDesc.ToLower() == "asc")
                {
                    fs.OrderBy(orderBy);
                }
                else
                {
                    fs.OrderByDescending(orderBy);
                }
            }
            return fs.ToList();
        }
        /// <summary>
        /// 通用查询
        /// </summary>
        public static List<T> Query(Where<T> where, OrderByClip orderBy = null, string ascOrDesc = "asc", int? pageSize = null, int? pageIndex = null)
        {
            var fs = Db.Context.From<T>().Where(where);
            
            if (pageIndex != null && pageSize != null)
            {
                fs.Page(pageSize.Value, pageIndex.Value);
            }
            if (orderBy != null)
            {
                fs.OrderBy(orderBy);
            }
            return fs.ToList();
        }

        /// <summary>
        /// 通用查询（返回分页数据）
        /// </summary>
        public static PageModel QueryPage(Where<T> where, Expression<Func<T, object>> orderBy = null, string ascOrDesc = "asc", int? pageSize = null, int? pageIndex = null)
        {
            int index = 1, size=10;
            var fs = Db.Context.From<T>().Where(where);
            if (pageIndex != null && pageSize != null)
            {
                fs.Page(pageSize.Value, pageIndex.Value);
                index = pageIndex.Value;
                size = pageSize.Value;
            }
            if (orderBy != null)
            {
                if (ascOrDesc.ToLower() == "asc")
                {
                    fs.OrderBy(orderBy);
                }
                else
                {
                    fs.OrderByDescending(orderBy);
                }
            }
            var dataList = fs.ToList();
            var count = Db.Context.From<T>().Where(where).Count();
            var model = new PageModel(index, size, count, dataList);
            return model;
        }


        /// <summary>
        /// 通用查询
        /// </summary>
        public static T First(Expression<Func<T, bool>> where, Expression<Func<T, object>> orderBy = null, string ascOrDesc = "asc", int? top = null, int? pageSize = null, int? pageIndex = null)
        {
            var fs = Db.Context.From<T>().Where(where);
            if (top != null)
            {
                fs.Top(top.Value);
            }
            else if (pageIndex != null && pageSize != null)
            {
                fs.Page(pageSize.Value, pageIndex.Value);
            }
            if (orderBy != null)
            {
                if (ascOrDesc.ToLower() == "asc")
                {
                    return fs.OrderBy(orderBy).First();
                }
                return fs.OrderByDescending(orderBy).First();
            }
            var model = fs.First();
            return model;
        }
        /// <summary>
        /// 通用查询
        /// </summary>
        public static T First(Where<T> where, Expression<Func<T, object>> orderBy = null, string ascOrDesc = "asc", int? top = null, int? pageSize = null, int? pageIndex = null)
        {
            var fs = Db.Context.From<T>().Where(where);
            if (top != null)
            {
                fs.Top(top.Value);
            }
            else if (pageIndex != null && pageSize != null)
            {
                fs.Page(pageSize.Value, pageIndex.Value);
            }
            if (orderBy != null)
            {
                if (ascOrDesc.ToLower() == "asc")
                {
                    return fs.OrderBy(orderBy).First();
                }
                return fs.OrderByDescending(orderBy).First();
            }
            return fs.First();
        }
        /// <summary>
        /// 根据条件判断是否存在数据
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public static bool Any(Expression<Func<T, bool>> where)
        {
            return Db.Context.Exists<T>(where);
        }
        /// <summary>
        /// 取总数
        /// </summary>
        public static int Count(Expression<Func<T, bool>> where)
        {
            return Db.Context.From<T>().Where(where).Count();
        }
        /// <summary>
        /// 取总数
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public static int Count(Where<T> where)
        {
            return Db.Context.From<T>().Where(where).Count();
        }
        #endregion
        #region 插入
        /// <summary>
        /// 插入单个实体
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static int Insert(T entity, bool isApi = false)
        {
            if (!isApi)
            SysLog.inputLog(entity.GetTableName(), "新增");
            return Db.Context.Insert<T>(entity);
        }
        /// <summary>
        /// 插入单个实体
        /// </summary>
        /// <param name="context"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static int Insert(DbTrans context, T entity, bool isApi = false)
        {
            if (!isApi)
            SysLog.inputLog(entity.GetTableName(), "新增");
            return Db.Context.Insert<T>(context, entity);
            //context.Set<T>().Add(entity);
        }
        /// <summary>
        /// 插入多个实体
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static int Insert(IEnumerable<T> entities, bool isApi = false)
        {
            if (!isApi)
            SysLog.inputLog(entities.First().GetTableName(), "新增");
            return Db.Context.Insert<T>(entities);
        }
        public static void Insert(DbTrans context, IEnumerable<T> entities, bool isApi = false)
        {
            if (!isApi)
            SysLog.inputLog(entities.First().GetTableName(), "新增");
            Db.Context.Insert<T>(context, entities.ToArray());
        }
        #endregion
        #region 更新
        /// <summary>
        /// 更新单个实体
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static int Update(T entity, bool isApi = false)
        {
            if (!isApi)
            SysLog.inputLog(entity.GetTableName(), "编辑");
            return Db.Context.Update(entity);
        }
        /// <summary>
        /// 更新单个实体
        /// </summary>
        public static int Update(T entity, Where where, bool isApi = false)
        {
            if (!isApi)
            SysLog.inputLog(entity.GetTableName(), "编辑");
            return Db.Context.Update(entity, where);
        }
        /// <summary>
        /// 更新单个实体
        /// </summary>
        public static int Update(T entity, Expression<Func<T, bool>> lambdaWhere, bool isApi = false)
        {
            if (!isApi)
            SysLog.inputLog(entity.GetTableName(), "编辑");
            return Db.Context.Update(entity, lambdaWhere);
        }
        public static void Update(DbTrans context, T entity, bool isApi = false)
        {
            if (!isApi)
            SysLog.inputLog(entity.GetTableName(), "编辑");
            Db.Context.Update(context, entity);
        }
        public static void Update(DbTrans context, T entity, Expression<Func<T, bool>> lambdaWhere, bool isApi = false)
        {
            if (!isApi)
            SysLog.inputLog(entity.GetTableName(), "编辑");
            Db.Context.Update(context, entity, lambdaWhere);
        }

        /// <summary>
        /// 更新多个实体
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static int Update(IEnumerable<T> entities, bool isApi = false)
        {
            if (!isApi)
            SysLog.inputLog(entities.First().GetTableName(), "编辑");
            var enumerable = entities as T[] ?? entities.ToArray();
            Db.Context.Update(enumerable.ToArray());
            return 1;
        }
        public static void Update(DbTrans context, IEnumerable<T> entities, bool isApi = false)
        {
            if (!isApi)
            SysLog.inputLog(entities.First().GetTableName(), "编辑");
            Db.Context.Update(context, entities.ToArray());
        }
        #endregion
        #region 删除
        /// <summary>
        /// 删除单个实体
        /// </summary>
        public static int Delete(T entitie, bool isApi = false)
        {
            //SysLog.inputLog(entitie.GetTableName(), "删除");
            return Db.Context.Delete<T>(entitie);
        }
        /// <summary>
        /// 删除多个实体
        /// </summary>
        public static int Delete(IEnumerable<T> entities, bool isApi = false)
        {
            if (!isApi)
            SysLog.inputLog(entities.First().GetTableName(), "删除");
            return Db.Context.Delete<T>(entities);
        }
        public static void Delete(DbTrans context, IEnumerable<T> entities, bool isApi = false)
        {
            if (!isApi)
                SysLog.inputLog(entities.First().GetTableName(), "新增");
            Db.Context.Delete<T>(context, entities.ToArray());
        }
        /// <summary>
        /// 删除单个实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static int Delete(Guid? id)
        {
            if (id == null)
            {
                return 0;
            }
            
            SysLog.inputLog(new T().GetTableName(),"删除");
            return Db.Context.Delete<T>(id.Value);
        }

        /// <summary>
        /// 删除单个实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static int Delete(int id)
        {
            if (id == null)
            {
                return 0;
            }
            SysLog.inputLog(new T().GetTableName(), "删除");
            return Db.Context.Delete<T>(id);
        }

        /// <summary>
        /// 删除单个实体
        /// </summary>
        public static int Delete(Expression<Func<T, bool>> where, bool isApi = false)
        {
            if (!isApi)
            SysLog.inputLog(new T().GetTableName(), "删除");
            return Db.Context.Delete<T>(where);
        }

        /// <summary>
        /// 删除单个实体
        /// </summary>
        public static int Delete(Where<T> where, bool isApi = false)
        {
            if (!isApi)
            SysLog.inputLog(new T().GetTableName(), "删除");
            return Db.Context.Delete<T>(where.ToWhereClip());
        }

        /// <summary>
        /// 删除单个实体(事务)
        /// </summary>
        public static void Delete(DbTrans context, T entity, bool isApi = false)
        {
            if (!isApi)
            SysLog.inputLog(entity.GetTableName(), "删除");
            Db.Context.Delete(context, entity);
        }

        /// <summary>
        /// 删除实体(事务)
        /// </summary>
        public static int Delete(DbTrans context, Expression<Func<T, bool>> where, bool isApi = false)
        {
            if (!isApi)
            SysLog.inputLog(new T().GetTableName(), "删除");
            return Db.Context.Delete<T>(context, where);
        }

        /// <summary>
        /// 删除多个实体(事务)
        /// </summary>
        public static void Delete(DbTrans context, IEnumerable<T> entities)
        {
            SysLog.inputLog(entities.First().GetTableName(), "删除");
            Db.Context.Delete(context, entities.ToArray());
        }
        #endregion
    }
}
