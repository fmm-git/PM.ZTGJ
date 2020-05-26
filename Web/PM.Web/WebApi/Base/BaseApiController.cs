using PM.Business;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace PM.Web.WebApi.Base
{
    public class BaseApiController : ApiController
    {
        private string HostAddress = Configs.GetValue("HostAddress");
        public string UserCode { get; set; }

        private readonly TbAttachmentLogic _attachmentImp = new TbAttachmentLogic();
        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name="File">文件数据(Base64)</param>
        /// <param name="FileName">文件名称</param>
        /// <returns>Item1:文件地址 Item2:附件Id</returns>
        [HttpPost]
        public Tuple<string, string> UploadFileData(List<FileData> files, bool inDatabase = false)
        {
            try
            {
                string UrlPaths = "";
                string fileId = "";
                if (files.Count > 0)
                {
                    fileId = Guid.NewGuid().ToString();
                }
                string saveTempPath = "/UploadFile";
                string resTempPath = "http://" + HostAddress + saveTempPath;
                string dirTempPath = System.Web.Hosting.HostingEnvironment.MapPath(saveTempPath);
                foreach (var item in files)
                {
                    string fname = item.fileName;
                    string path = dirTempPath;
                    byte[] bt = Convert.FromBase64String(item.file);
                    System.IO.MemoryStream stream = new System.IO.MemoryStream(bt);
                    System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(stream);
                    if (!System.IO.Directory.Exists(path))
                        System.IO.Directory.CreateDirectory(path);
                    item.fileName = Guid.NewGuid().ToString() + item.fileName;
                    resTempPath += "/" + item.fileName;
                    path += "/" + item.fileName;
                    bitmap.Save(path);
                    if (inDatabase)
                    {
                        var info = GetFileInfoByPath(path, fname);
                        if (info == null)
                            return null;

                        #region 文件保存成功后，写入附件的数据库记录
                        TbAttachment attachmentInfo = new TbAttachment();
                        attachmentInfo.FileID = fileId;
                        attachmentInfo.FunModule = "test";
                        attachmentInfo.FileType = "application/octet-stream";
                        attachmentInfo.FileSize = info.Item1;
                        attachmentInfo.FileName = info.Item2;
                        attachmentInfo.FileModified = DateTime.Now.ToString();
                        attachmentInfo.StartTime = DateTime.Now;
                        attachmentInfo.LastTime = DateTime.Now;
                        attachmentInfo.FileStoragePath = saveTempPath + "/" + item.fileName;
                        attachmentInfo.UserCode = UserCode;
                        attachmentInfo.StorageOver = 0;
                        attachmentInfo.ServerCode = "";
                        attachmentInfo.FileToKen = "";
                        var data = _attachmentImp.Insert(attachmentInfo);
                        //fileId += attachmentInfo.FileID + ",";
                        #endregion
                    }
                    UrlPaths += resTempPath + ",";
                }
                UrlPaths = UrlPaths.TrimEnd(',');
                //fileId = fileId.TrimEnd(',');

                return new Tuple<string, string>(UrlPaths, fileId);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// 附件数据
        /// </summary>
        public class FileData
        {
            public string file { get; set; }
            public string fileName { get; set; }
        }

        /// <summary>
        /// 文件base64解码
        /// </summary>
        /// <param name="base64Str">文件base64编码</param>
        /// <param name="outPath">生成文件路径</param>
        public void Base64ToOriFile(string base64Str, string outPath)
        {
            var contents = Convert.FromBase64String(base64Str);
            using (var fs = new FileStream(outPath, FileMode.Create, FileAccess.Write))
            {
                fs.Write(contents, 0, contents.Length);
                fs.Flush();
            }
        }
        /// <summary>
        /// 得到路径下文件的信息
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private Tuple<long, string> GetFileInfoByPath(string path,string filName)
        {
            FileInfo fileInfo = new FileInfo(path);
            if (fileInfo != null && fileInfo.Exists)
            {
                //long Size = fileInfo.Length / 1024 / 1024;//MB
                //if (Size == 0)
                //    Size = fileInfo.Length / 1024;//KB
                if (string.IsNullOrEmpty(filName))
                    filName = fileInfo.Name;
                return new Tuple<long, string>(fileInfo.Length, filName);
            }
            return null;
        }
       
        /// <summary>
        /// 获取附件地址
        /// </summary>
        /// <param name="FileId"></param>
        /// <returns></returns>
        public DataTable GetEnclosureUrl(string FileId)
        {
            string sql = @"select FileStoragePath,FileName from TbAttachment where FileID=@FileID";
            DataTable dt = Db.Context.FromSql(sql).AddInParameter("@FileId", DbType.String, FileId)
                .ToDataTable();
            if (dt.Rows.Count>0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["FileStoragePath"] = "http://" + HostAddress + dt.Rows[i]["FileStoragePath"];
                }
            }
            return dt;
        }
    }
}