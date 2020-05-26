using Dos.Common;
using Dos.ORM;
using PM.Common;
using PM.Common.Helper;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Business
{
    public class TbAttachmentLogic
    {
        #region 附件旧版

        #region 新增数据

        /// <summary>
        /// 新增数据(单条)
        /// </summary>
        public AjaxResult Insert(TbAttachment request)
        {
            if (request == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                var model = MapperHelper.Map<TbAttachment, TbAttachment>(request);
                var count = Repository<TbAttachment>.Insert(model, true);
                if (count > 0)
                {
                    return AjaxResult.Success("上传成功");
                }
                return AjaxResult.Error();
            }
            catch (Exception)
            {
                return AjaxResult.Error();
            }

        }

        #endregion

        #region 删除数据

        /// <summary>
        /// 删除数据
        /// </summary>
        public AjaxResult Delete(string FileID)
        {
            try
            {
                var count = Repository<TbAttachment>.Delete(t => t.FileID == FileID);
                if (count > 0)
                {
                    return AjaxResult.Success();
                }
                return AjaxResult.Error();
            }
            catch (Exception)
            {
                return AjaxResult.Error();
            }

        }

        #endregion

        #region 获取列表数据
        /// <summary>
        /// 获取数据列表(分页)
        /// </summary>
        public List<TbAttachment> GetAttachmentJson(string FileID)
        {
            string FileIDNew = "";
            if (!string.IsNullOrEmpty(FileID))
            {
                string str = FileID;
                string[] strs = str.Split(',');
                for (int i = 0; i < strs.Length; i++)
                {
                    if (FileIDNew == "")
                    { FileIDNew += "'" + strs[i] + "'"; }
                    else
                    {
                        FileIDNew += ",'" + strs[i] + "'";
                    }
                }
            }
            try
            {
                string sql;
                if (!string.IsNullOrEmpty(FileIDNew))
                {
                    sql = @"select TbAttachment.*,TbUser.UserName from TbAttachment 
left join TbUser on TbAttachment.UserCode=TbUser.UserCode
where FileID in (" + FileIDNew + ")";
                }
                else
                {
                    sql = @"select TbAttachment.*,TbUser.UserName from TbAttachment 
left join TbUser on TbAttachment.UserCode=TbUser.UserCode
where FileID in ('')";
                }
                var model = Db.Context.FromSql(sql).ToDataTable();
                var list = ModelConvertHelper<TbAttachment>.ToList(model);
                return list;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #endregion

        #region 附件新版

        /// <summary>
        /// 删除附件
        /// </summary>
        public AjaxResult Del(string menuTable, int id)
        {
            try
            {
                var fileModel = Repository<TbAttachment>.First(p => p.id == id);
                if (fileModel == null)
                    return AjaxResult.Warning("参数错误");
                //查找表单对应表名信息
                var menuTable1 = Db.Context.From<TbSysMenuTable>()
          .Select(TbSysMenuTable._.All)
               .Where(p => p.MenuCode == menuTable).First();
                if (menuTable1 != null)
                {
                    string sql = @"select ID from " + menuTable1.TableName + " where Enclosure like '%" + fileModel.FileID + "%'";
                    DataTable dt = Db.Context.FromSql(sql).ToDataTable();
                    var fileModelall = Repository<TbAttachment>.Query(p => p.FileID == fileModel.FileID && p.id != id).ToList();
                    if (fileModelall.Count() > 0)//改数据下还存在附件
                    {
                        Repository<TbAttachment>.Delete(t => t.id == id);
                    }
                    else
                    {
                        if (dt.Rows.Count > 0)
                        {
                            //修改业务表附件字段
                            string upatesql = @"update " + menuTable1.TableName + " set Enclosure='' where ID=" + Convert.ToInt32(dt.Rows[0]["ID"]) + "";
                            Db.Context.FromSql(upatesql).ExecuteNonQuery();
                        }
                        //删除附件
                        Repository<TbAttachment>.Delete(t => t.id == id);
                    }
                }
                return AjaxResult.Success();
            }
            catch (Exception)
            {
                return AjaxResult.Error();
            }

        }
        /// <summary>
        /// 获取所有附件ID
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<TbAttachment> GetUploadList(string ids)
        {
            string FileIDNew = "";
            if (!string.IsNullOrEmpty(ids))
            {
                string str = ids;
                string[] strs = str.Split(',');
                for (int i = 0; i < strs.Length; i++)
                {
                    if (FileIDNew == "")
                    {
                        FileIDNew += "'" + strs[i] + "'";
                    }
                    else
                    {
                        FileIDNew += ",'" + strs[i] + "'";
                    }
                }
            }
            try
            {
                string sql;
                if (!string.IsNullOrEmpty(FileIDNew))
                {
                    sql = @"select TbAttachment.*,TbUser.UserName from TbAttachment 
left join TbUser on TbAttachment.UserCode=TbUser.UserCode
where FileID in (" + FileIDNew + ")";
                }
                else
                {
                    sql = @"select TbAttachment.*,TbUser.UserName from TbAttachment 
left join TbUser on TbAttachment.UserCode=TbUser.UserCode
where FileID='" + ids + "'";
                }
                var model = Db.Context.FromSql(sql).ToDataTable();
                var list = ModelConvertHelper<TbAttachment>.ToList(model);
                return list;
            }
            catch (Exception)
            {
                throw;
            }
            //var ret = Db.Context.From<TbAttachment>()
            //.Select(TbAttachment._.All).Where(p => p.FileID == ids).ToList();
            //return ret;
        }
        /// <summary>
        /// 获取附件的地址
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TbAttachment GetUploadModel(int id)
        {
            var ret = Db.Context.From<TbAttachment>()
            .Select(TbAttachment._.All).Where(p => p.id == id).First();
            return ret;
        }

        public DataTable AnyInfo(string menuTable, int ID)
        {
            try
            {
                DataTable dt = new DataTable();
                var menuTable1 = Db.Context.From<TbSysMenuTable>()
               .Select(TbSysMenuTable._.All)
                    .Where(p => p.MenuCode == menuTable).First();
                if (menuTable1 != null)
                {
                    string sql = @"select ID from " + menuTable1.TableName + " where ID=" + ID + " and (DATALENGTH(Enclosure)=0  or Enclosure is null)";
                    dt = Db.Context.FromSql(sql).ToDataTable();
                }
                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool SaveDataFile(string keyID, string menuTable, int id)
        {
            bool flag = false;
            try
            {
                //查找表单对应表名信息
                var menuTable1 = Db.Context.From<TbSysMenuTable>()
          .Select(TbSysMenuTable._.All)
               .Where(p => p.MenuCode == menuTable).First();
                if (menuTable1 != null)
                {

                    //修改业务表附件字段
                    string upatesql = @"update " + menuTable1.TableName + " set Enclosure='" + keyID + "' where ID=" + id + "";
                    int reslut = Db.Context.FromSql(upatesql).ExecuteNonQuery();
                    flag = true;
                }
            }
            catch (Exception)
            {
                flag = false;
            }
            return flag;
        }
        #endregion
    }
}
