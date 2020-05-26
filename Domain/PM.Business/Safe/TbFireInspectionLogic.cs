using Dos.ORM;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using PM.DataEntity.Safe.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Script.Serialization;
namespace PM.Business.Safe
{
    //消防检查
    public class TbFireInspectionLogic
    {

        private string HostAddress = Configs.GetValue("HostAddress");

        #region 获取列表
        /// <summary>
        /// 获取数据列表(分页)
        /// </summary>
        public PageModel GetDataListForPage(FireInspectionRequest request)
        {

            #region 模糊搜索条件

            var where = new Where<TbFireInspection>();
            if (!string.IsNullOrWhiteSpace(request.CheckCode))
            {
                where.And(d => d.CheckCode == request.CheckCode);
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where.And(d => d.ProcessFactoryCode == request.ProcessFactoryCode);
            }
            //if (!string.IsNullOrWhiteSpace(request.ProjectId))
            //{
            //     where.And(d => d.ProjectId == request.ProjectId);
            //}
            if (request.CheckTimeS.HasValue)
            {
                where.And(d => d.CheckDate >= request.CheckTimeS);
            }
            if (request.CheckTimeE.HasValue)
            {
                where.And(d => d.CheckDate <= request.CheckTimeE);
            }
            #endregion

            try
            {
                var data = Db.Context.From<TbFireInspection>()
                    .Select(
                      TbFireInspection._.All
                    , TbCompany._.CompanyFullName.As("ProcessFactoryName")
                    , TbUser._.UserName)
                  .LeftJoin<TbCompany>((a, c) => a.ProcessFactoryCode == c.CompanyCode)
                  .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                  .Where(where)
                  .OrderByDescending(TbFireInspection._.InsertTime)
                  .ToPageList(request.rows, request.page);
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 获取数据(编辑、查看)
        /// </summary>
        /// <param name="keyValue">数据Id</param>
        /// <returns></returns>
        public TbFireInspection FindEntity(int keyValue)
        {
            var ret = Db.Context.From<TbFireInspection>()
                .Select(
                       TbFireInspection._.All
                    , TbCompany._.CompanyFullName.As("ProcessFactoryName")
                    , TbUser._.UserName)
                  .LeftJoin<TbCompany>((a, c) => a.ProcessFactoryCode == c.CompanyCode)
                  .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                  .Where(p => p.ID == keyValue).First();
            return ret;
        }

        public DataTable GetCheckUserList()
        {
            string sql = @"select UserCode,UserName from TbUser where UserClosed='在职'";
            DataTable dt = Db.Context.FromSql(sql)
                .ToDataTable();
            return dt;
        }
        public DataTable GetCompanyList()
        {
            string sql = @"select CompanyCode,CompanyFullName from TbCompany where OrgType=1";
            DataTable dt = Db.Context.FromSql(sql)
                .ToDataTable();
            return dt;
        }
        public DataTable GetEnclosureUrl(string strId)
        {
            string sql = @"select FileStoragePath,FileName from TbAttachment where FileID in(" + strId + ")";
            DataTable dt = Db.Context.FromSql(sql)
                .ToDataTable();
            return dt;
        }
        #endregion

        #region 新增数据

        /// <summary>
        /// 新增数据
        /// </summary>
        public AjaxResult Insert(TbFireInspection model, bool isApi = false)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            try
            {

                //string str = UploadFile(model.Enclosure, model.InsertUserCode);
                //model.Enclosure = str.TrimEnd(',');
                //添加信息
                Repository<TbFireInspection>.Insert(model, isApi);
                return AjaxResult.Success();
            }
            catch (Exception ex)
            {
                return AjaxResult.Error();
            }
        }

        #endregion

        #region 修改数据

        /// <summary>
        /// 修改数据
        /// </summary>
        public AjaxResult Update(TbFireInspection model, bool isApi = false)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");
            var anyRet = AnyInfo(model.ID);
            if (anyRet.state.ToString() != ResultType.success.ToString())
                return anyRet;
            try
            {
                Repository<TbFireInspection>.Update(model, p => p.ID == model.ID, isApi);
                return AjaxResult.Success();
            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.ToString());
            }
        }

        #endregion

        #region 删除数据

        /// <summary>
        /// 删除数据
        /// </summary>
        public AjaxResult Delete(int keyValue, bool isApi = false)
        {
            try
            {
                //判断信息是否存在
                var anyRet = AnyInfo(keyValue);
                if (anyRet.state.ToString() != ResultType.success.ToString())
                    return anyRet;
                //删除信息
                var count = Repository<TbFireInspection>.Delete(p => p.ID == keyValue, isApi);
                return AjaxResult.Success();
            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.ToString());
            }
        }

        #endregion

        #region 判断

        /// <summary>
        /// 判断信息是否可操作
        /// </summary>
        /// <returns></returns>
        public AjaxResult AnyInfo(int keyValue)
        {
            var article = Repository<TbFireInspection>.First(p => p.ID == keyValue);
            if (article == null)
                return AjaxResult.Warning("信息不存在");
            return AjaxResult.Success(article);
        }

        #endregion

        #region 保存附件
        public AjaxResult AttachmentInsert(List<TbAttachment> attachmentInfo, bool isApi = false)
        {
            if (attachmentInfo == null)
                return AjaxResult.Warning("参数错误");
            try
            {
                //添加信息
                Repository<TbAttachment>.Insert(attachmentInfo, isApi);
                return AjaxResult.Success();
            }
            catch (Exception ex)
            {
                return AjaxResult.Error();
            }
        }

        #endregion

        #region App上传图片

        /// <summary>
        /// 上传图片(多张)
        /// </summary>
        /// <param name="quePara">list集合</param>
        /// <returns></returns>
        public string UploadFile(string Enclosure, string UserCode)
        {
            string str = "";
            try
            {
                List<TbAttachment> list = new List<TbAttachment>();
                JavaScriptSerializer Serializers = new JavaScriptSerializer();
                //json字符串转为数组对象, 反序列化
                List<File> quePara = Serializers.Deserialize<List<File>>(Enclosure);
                foreach (var item in quePara)
                {
                    int FileSize = 0;
                    string file = item.file == null ? "" : item.file.ToString();
                    string fileName = item.fileName == null ? "" : item.fileName.ToString();
                    byte[] bt = Convert.FromBase64String(file);
                    FileSize = bt.Length;
                    MemoryStream stream = new MemoryStream(bt);
                    Bitmap bitmap = new Bitmap(stream);
                    string saveTempPath = "/UploadFile";
                    string resTempPath = "http://" + HostAddress + "/UploadFile";
                    string dirTempPath = HostingEnvironment.MapPath(saveTempPath);
                    if (!Directory.Exists(dirTempPath))
                    {
                        Directory.CreateDirectory(dirTempPath);
                    }

                    fileName = Guid.NewGuid().ToString() + fileName;
                    resTempPath += "/" + fileName;
                    dirTempPath += "/" + fileName;
                    bitmap.Save(dirTempPath);

                    #region 文件保存成功后，写入附件的数据库记录

                    TbAttachment attachmentInfo = new TbAttachment();
                    attachmentInfo.FileID = Guid.NewGuid().ToString();
                    attachmentInfo.FunModule = "test";
                    attachmentInfo.FileType = "application/octet-stream";
                    attachmentInfo.FileSize = FileSize;
                    attachmentInfo.FileName = fileName;
                    attachmentInfo.FileModified = DateTime.Now.ToString();
                    attachmentInfo.StartTime = DateTime.Now;
                    attachmentInfo.LastTime = DateTime.Now;
                    attachmentInfo.FileStoragePath = dirTempPath;
                    attachmentInfo.UserCode = UserCode;
                    attachmentInfo.StorageOver = 0;
                    attachmentInfo.StorageSize = 0;
                    attachmentInfo.ServerCode = "";
                    attachmentInfo.FileToKen = "";
                    list.Add(attachmentInfo);

                    #endregion

                    str += attachmentInfo.FileID + ",";
                }

                AttachmentInsert(list, true);
                return str;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public class File
        {
            public string file { get; set; }
            public string fileName { get; set; }
        }

        #endregion

        #region 导出
        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public DataTable GetExportList(FireInspectionRequest request)
        {

            #region 模糊搜索条件

            var where = new Where<TbFireInspection>();
            if (!string.IsNullOrWhiteSpace(request.CheckCode))
            {
                where.And(d => d.CheckCode == request.CheckCode);
            }
            if (!string.IsNullOrWhiteSpace(request.ProcessFactoryCode))
            {
                where.And(d => d.ProcessFactoryCode == request.ProcessFactoryCode);
            }
            if (request.CheckTimeS.HasValue)
            {
                where.And(d => d.CheckDate >= request.CheckTimeS);
            }
            if (request.CheckTimeE.HasValue)
            {
                where.And(d => d.CheckDate <= request.CheckTimeE);
            }
            #endregion

            try
            {
                var data = Db.Context.From<TbFireInspection>()
                    .Select(
                      TbFireInspection._.All
                    , TbCompany._.CompanyFullName.As("ProcessFactoryName")
                    , TbUser._.UserName)
                  .LeftJoin<TbCompany>((a, c) => a.ProcessFactoryCode == c.CompanyCode)
                  .LeftJoin<TbUser>((a, c) => a.InsertUserCode == c.UserCode)
                  .Where(where)
                  .OrderByDescending(TbFireInspection._.InsertTime).ToDataTable();
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion
    }
}
