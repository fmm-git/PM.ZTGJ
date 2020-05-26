using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PM.Common;
using System.Web.Script.Serialization;
using PM.Business;
using PM.DataEntity;
using PM.DataAccess.DbContext;
using System.Text;
using Aspose.Words;
using System.Drawing;
using System.Drawing.Imaging;
using Aspose.Words.Drawing;

namespace PM.Web.Controllers
{
    public class AttachmentController : BaseController
    {
        private readonly TbAttachmentLogic _attachmentImp = new TbAttachmentLogic();

        #region 上传旧版

        #region 附件上传页面

        public ActionResult Index(string UserCode)
        {
            ViewBag.UserCode = OperatorProvider.Provider.CurrentUser.UserCode;
            return View();
        }

        #endregion

        #region 附件列表

        /// <summary>
        /// 获取附件列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetAttachmentJson(string FileID)
        {
            var data = _attachmentImp.GetAttachmentJson(FileID);
            return Content(data.ToJson());
        }

        #endregion

        #region 附件上传

        /// <summary>
        /// 上传
        /// </summary>
        /// <param name="Filedata"></param>
        /// <returns></returns>
        public ActionResult UploadifyFun(HttpPostedFileBase Filedata, string UserCode)
        {
            if (Filedata == null || String.IsNullOrEmpty(Filedata.FileName) || Filedata.ContentLength == 0)
            {
                return this.HttpNotFound();
            }
            string savePath = Server.MapPath("/") + "UploadFile\\";//保存文件地址
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            #region 生成4位随机数

            string vc = "";
            Random rNum = new Random();//随机生成类
            int num1 = rNum.Next(0, 9);//返回指定范围内的随机数
            int num2 = rNum.Next(0, 9);
            int num3 = rNum.Next(0, 9);
            int num4 = rNum.Next(0, 9);

            int[] nums = new int[4] { num1, num2, num3, num4 };
            for (int i = 0; i < nums.Length; i++)//循环添加四个随机生成数
            {
                vc += nums[i].ToString();
            }

            #endregion

            string filename = System.IO.Path.GetFileName(vc + Filedata.FileName);
            string virtualPath = String.Format(savePath + filename);
            Filedata.SaveAs(virtualPath);
            bool uploaded = System.IO.File.Exists(virtualPath);
            if (uploaded)
            {
                #region 文件保存成功后，写入附件的数据库记录
                TbAttachment attachmentInfo = new TbAttachment();
                attachmentInfo.FileID = Guid.NewGuid().ToString();
                attachmentInfo.FunModule = "test";
                attachmentInfo.FileType = Filedata.ContentType;
                attachmentInfo.FileSize = long.Parse(FormatFileSize(Convert.ToInt64(Filedata.ContentLength)));
                attachmentInfo.FileName = filename;
                attachmentInfo.FileModified = DateTime.Now.ToString();
                attachmentInfo.StartTime = DateTime.Now;
                attachmentInfo.LastTime = DateTime.Now;
                attachmentInfo.FileStoragePath = virtualPath;
                attachmentInfo.UserCode = UserCode;
                attachmentInfo.StorageOver = 0;
                attachmentInfo.ServerCode = "";
                attachmentInfo.FileToKen = "";
                var data = _attachmentImp.Insert(attachmentInfo);
                return Content(attachmentInfo.FileID);

                #endregion
            }
            else
            {
                return Content("上传失败");
            }
        }
        /// <summary>
        /// 附件上传
        /// </summary>
        /// <param name="Filedata"></param>
        /// <param name="UserCode"></param>
        /// <returns></returns>
        public string UploadFile(HttpPostedFileBase Filedata, string UserCode)
        {
            var returnstr = new { success = "false", data = "", message = "" };
            //第一步创建路径
            if (Filedata != null)
            {
                try
                {

                    string savePath = Server.MapPath("/") + "UploadFile\\";//保存文件地址
                    if (!Directory.Exists(savePath))
                    {
                        Directory.CreateDirectory(savePath);
                    }

                    #region 生成4位随机数

                    string vc = "";
                    Random rNum = new Random();//随机生成类
                    int num1 = rNum.Next(0, 9);//返回指定范围内的随机数
                    int num2 = rNum.Next(0, 9);
                    int num3 = rNum.Next(0, 9);
                    int num4 = rNum.Next(0, 9);

                    int[] nums = new int[4] { num1, num2, num3, num4 };
                    for (int i = 0; i < nums.Length; i++)//循环添加四个随机生成数
                    {
                        vc += nums[i].ToString();
                    }

                    #endregion
                    string filename = System.IO.Path.GetFileName(vc + Filedata.FileName);
                    string virtualPath = String.Format(savePath + filename);
                    //将文件“拷贝”到指定路径
                    try
                    {
                        Filedata.SaveAs(virtualPath);
                        try
                        {
                            bool uploaded = System.IO.File.Exists(virtualPath);
                            if (uploaded)
                            {
                                #region 文件保存成功后，写入附件的数据库记录
                                TbAttachment attachmentInfo = new TbAttachment();
                                attachmentInfo.FileID = Guid.NewGuid().ToString();
                                attachmentInfo.FunModule = "test";
                                attachmentInfo.FileType = Filedata.ContentType;
                                //attachmentInfo.FileSize = long.Parse(FormatFileSize(Convert.ToInt64(Filedata.ContentLength)));
                                attachmentInfo.FileSize = Filedata.ContentLength;
                                attachmentInfo.FileName = filename;
                                attachmentInfo.FileModified = DateTime.Now.ToString();
                                attachmentInfo.StartTime = DateTime.Now;
                                attachmentInfo.LastTime = DateTime.Now;
                                attachmentInfo.FileStoragePath = virtualPath;
                                attachmentInfo.UserCode = UserCode;
                                attachmentInfo.StorageOver = 0;
                                attachmentInfo.ServerCode = "";
                                attachmentInfo.FileToKen = "";
                                var data = _attachmentImp.Insert(attachmentInfo);
                                returnstr = new { success = "true", data = attachmentInfo.FileID, message = "文档上传成功！" };
                                return new JavaScriptSerializer().Serialize(returnstr);
                                #endregion
                            }
                            else
                            {
                                returnstr = new { success = "false", data = "", message = "上传文件过程中发生异常" };
                                return new JavaScriptSerializer().Serialize(returnstr);
                            }
                        }
                        catch (Exception e)
                        {
                            //删掉已经上传的文件
                            System.IO.File.Delete(virtualPath);
                            returnstr = new { success = "false", data = "", message = "连接文档服务器OK！调用SaveAs方法过程中发生异常，异常信息如下：" + e.Message };
                            return new JavaScriptSerializer().Serialize(returnstr);
                        }
                    }
                    catch (System.Exception e)
                    {
                        //删掉已经上传的文件
                        System.IO.File.Delete(virtualPath);
                        returnstr = new { success = "false", data = "", message = "连接文档服务器OK！调用SaveAs方法过程中发生异常，异常信息如下：" + e.Message };
                        return new JavaScriptSerializer().Serialize(returnstr);
                    }


                }
                catch (System.Exception ex)
                {
                    returnstr = new { success = "false", data = "", message = "上传文件过程中发生异常，异常详情如下：" + ex.Message };
                    return new JavaScriptSerializer().Serialize(returnstr);
                }
            }
            else
            {
                return new JavaScriptSerializer().Serialize(returnstr);
            }

        }

        public static string FormatFileSize(Int64 fileSize)
        {
            if (fileSize < 0)
            {
                throw new ArgumentOutOfRangeException("fileSize");
            }
            else if (fileSize >= 1024 * 1024 * 1024)
            {
                return string.Format("{0:########0.00} GB", ((Double)fileSize) / (1024 * 1024 * 1024));
            }
            else if (fileSize >= 1024 * 1024)
            {
                return string.Format("{0:####0.00} MB", ((Double)fileSize) / (1024 * 1024));
            }
            else if (fileSize >= 1024)
            {
                return string.Format("{0:####0.00} KB", ((Double)fileSize) / 1024);
            }
            else
            {
                return string.Format("{0} bytes", fileSize);
            }
        }


        #endregion

        #region 下载

        ///// <summary>
        ///// 下载
        ///// </summary>
        ///// <returns></returns>
        //public ActionResult FileDownload(string FileUrl, string FileName)
        //{
        //    if (!string.IsNullOrEmpty(FileUrl) && !string.IsNullOrEmpty(FileName))
        //    {
        //        FileStream fs = new FileStream(FileUrl, FileMode.Open);
        //        byte[] bytes = new byte[(int)fs.Length];
        //        fs.Read(bytes, 0, bytes.Length);
        //        fs.Close();
        //        Response.Charset = "UTF-8";
        //        Response.ContentEncoding = System.Text.Encoding.GetEncoding("UTF-8");
        //        Response.ContentType = "application/octet-stream";
        //        Response.AddHeader("Content-Disposition", "attachment; filename=" + Server.UrlEncode(FileName));
        //        Response.BinaryWrite(bytes);
        //        Response.Flush();
        //        Response.End();
        //        return Content("下载成功");
        //    }
        //    else
        //    {
        //        return Content("文件不存在");
        //    }

        //}
        /// <summary>
        /// 下载
        /// </summary>
        /// <returns></returns>
        public ActionResult FileDownload(string FileID)
        {
            if (!string.IsNullOrEmpty(FileID))
            {
                //通过附件ID查询附件
                string sql = @"select * from TbAttachment where FileID='" + FileID + "'";
                var dt = Db.Context.FromSql(sql).ToDataTable();
                if (dt != null && dt.Rows.Count > 0)
                {
                    FileStream fs = new FileStream(dt.Rows[0]["FileStoragePath"].ToString(), FileMode.Open);
                    byte[] bytes = new byte[(int)fs.Length];
                    fs.Read(bytes, 0, bytes.Length);
                    fs.Close();
                    Response.Charset = "UTF-8";
                    Response.ContentEncoding = System.Text.Encoding.GetEncoding("UTF-8");
                    Response.ContentType = "application/octet-stream";
                    Response.AddHeader("Content-Disposition", "attachment; filename=" + dt.Rows[0]["FileName"].ToString().Substring(4, dt.Rows[0]["FileName"].ToString().Length - 4));
                    Response.BinaryWrite(bytes);
                    Response.Flush();
                    Response.End();
                }
                return Content("下载成功");
            }
            else
            {
                return Content("文件不存在");
            }

        }

        #endregion

        #region 删除

        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        public ActionResult FileDelete(string FileID, string FileStoragePath)
        {
            try
            {
                if (!string.IsNullOrEmpty(FileID) && !string.IsNullOrEmpty(FileStoragePath))
                {
                    var a = FileID.Split(',');
                    var b = FileStoragePath.Split(',');
                    for (int i = 0; i < a.Length; i++)
                    {
                        //文件地址
                        if (System.IO.File.Exists(b[i]))
                        {
                            System.IO.File.Delete(b[i]);
                        }
                        var data = _attachmentImp.Delete(a[i]);

                    }
                    return Success("删除文件成功");
                }
                else
                {
                    return Error("删除文件失败");
                }

            }
            catch (Exception)
            {
                return Error("删除文件失败");
            }

        }

        #endregion

        #region 附件上传(不写入数据库)
        /// <summary>
        /// 附件上传
        /// </summary>
        /// <param name="Filedata"></param>
        /// <param name="UserCode"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UploadFileNoBas()
        {
            var returnstr = new { errno = "1", data = "" };

            HttpPostedFileBase Filedata = Request.Files["wangEditorH5File"];
            //第一步创建路径
            if (Filedata != null)
            {
                try
                {
                    string savePath = Server.MapPath("/") + "UploadFile\\";//保存文件地址
                    if (!Directory.Exists(savePath))
                    {
                        Directory.CreateDirectory(savePath);
                    }

                    #region 生成4位随机数

                    string vc = "";
                    Random rNum = new Random();//随机生成类
                    int num1 = rNum.Next(0, 9);//返回指定范围内的随机数
                    int num2 = rNum.Next(0, 9);
                    int num3 = rNum.Next(0, 9);
                    int num4 = rNum.Next(0, 9);

                    int[] nums = new int[4] { num1, num2, num3, num4 };
                    for (int i = 0; i < nums.Length; i++)//循环添加四个随机生成数
                    {
                        vc += nums[i].ToString();
                    }

                    #endregion
                    string filename = System.IO.Path.GetFileName(vc + Filedata.FileName);
                    string virtualPath = String.Format(savePath + filename);
                    //将文件“拷贝”到指定路径
                    try
                    {
                        Filedata.SaveAs(virtualPath);
                        try
                        {
                            bool uploaded = System.IO.File.Exists(virtualPath);
                            if (uploaded)
                            {
                                #region 文件保存成功后
                                var url = GetRequesterIP() + "/UploadFile/" + filename;
                                return Content(url);
                                #endregion
                            }
                            else
                            {
                                return Content(returnstr.ToJson());
                            }
                        }
                        catch (Exception e)
                        {
                            //删掉已经上传的文件
                            System.IO.File.Delete(virtualPath);
                            return Content(returnstr.ToJson());
                        }
                    }
                    catch (System.Exception e)
                    {
                        //删掉已经上传的文件
                        System.IO.File.Delete(virtualPath);
                        return Content(returnstr.ToJson());
                    }
                }
                catch (System.Exception ex)
                {
                    return Content(returnstr.ToJson());
                }
            }
            else
            {
                return Content(returnstr.ToJson());
            }

        }

        /// <summary>
        /// 获取服务器IP+端口号
        /// </summary>
        /// <returns></returns>
        public string GetRequesterIP()
        {
            string Result = "http://";
            Result += Request.ServerVariables["Http_Host"];
            if (null == Result || Result == String.Empty)
            {
                Result = Request.UserHostAddress;
            }
            if (null == Result || Result == String.Empty)
            {
                return "0.0.0.0";
            }
            return Result;
        }

        #endregion

        #endregion

        #region 附件新版

        #region 文件查看

        public ActionResult UploadFileList(string ids, string type, string menuTable, string IsItem, string index1)
        {
            ViewBag.ids = ids;
            ViewBag.type = type;
            ViewBag.menuTable = menuTable;
            ViewBag.IsItem = IsItem;
            ViewBag.index1 = index1;
            return View();
        }
        /// <summary>
        /// 获取该业务ID下所有的附件信息
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public ActionResult GetUploadList(string ids = "")
        {
            var list = _attachmentImp.GetUploadList(ids);
            return Content(list.ToJson());
        }

        /// <summary>
        /// 在线预览文件
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult ShowDocument(int id = -1)
        {

            if (id <= 0)
                return Content(@"<div style='width:100%;text-align:center;padding-top:10px;'>当前参数传递错误!</div>");
            //获取改附件信息
            var fileEntity = _attachmentImp.GetUploadModel(id);
            if (fileEntity == null)
                return Content(@"<div style='width:100%;text-align:center;padding-top:10px;'>不存在此文件!</div>");
            //文件物理路径
            var mapPath = HttpContext.Server.MapPath(fileEntity.FileStoragePath);        //F/1.doc
            string name = fileEntity.id + fileEntity.FileType;                  //文件名称
            if (!System.IO.File.Exists(mapPath))//判断文件是否存在 
                return Content(@"<div style='width:100%;text-align:center;padding-top:10px;'>服务器存放文件路径错误!</div>");


            //将 xls  word 等文件转为为pdf文件进行在线预览
            FileConversion(ref fileEntity, mapPath, name);
            return View(fileEntity);
        }

        private void FileConversion(ref TbAttachment fileEntity, string mapPath, string name)
        {
            //转换pdf路径
            var fileConfig = System.Configuration.ConfigurationManager.AppSettings["uploadFileChange"] ?? "";
            var fielConfigCreate = Server.MapPath("/" + fileConfig);
            if (!Directory.Exists(fielConfigCreate))
                Directory.CreateDirectory(fielConfigCreate);           //创建文件夹


            string fileUrl = string.Format("/{0}/{1}.pdf", fileConfig, name);
            string path = Server.MapPath(fileUrl);
            if (fileEntity.FileStoragePath.ToUpper().Contains(".DOC") || fileEntity.FileStoragePath.ToUpper().Contains(".DOCX"))
            {
                if (!System.IO.File.Exists(path))//判断文件是否存在
                {
                    //读取doc文档
                    Aspose.Words.Document doc = new Aspose.Words.Document(mapPath);
                    //保存为PDF文件，此处的SaveFormat支持很多种格式，如图片，epub,rtf 等等
                    doc.Save(path, Aspose.Words.SaveFormat.Pdf);
                }
                fileEntity.FileStoragePath = fileUrl;
            }
            else if (fileEntity.FileStoragePath.ToUpper().Contains(".XLS") || fileEntity.FileStoragePath.ToUpper().Contains(".XLSX"))
            {
                if (!System.IO.File.Exists(path))
                {
                    Aspose.Cells.Workbook wb = new Aspose.Cells.Workbook(mapPath);
                    wb.Save(path, Aspose.Cells.SaveFormat.Pdf);
                }
                fileEntity.FileStoragePath = fileUrl;
            }
            else if (fileEntity.FileStoragePath.ToUpper().Contains(".TXT"))
            {
                string[] lines = System.IO.File.ReadAllLines(urlconvertorlocal(fileEntity.FileStoragePath), Encoding.Default);
                ViewData["c_txt"] = lines;
            }
            else if (fileEntity.FileStoragePath.ToUpper().Contains(".JPG")||fileEntity.FileStoragePath.ToUpper().Contains(".PNG")||fileEntity.FileStoragePath.ToUpper().Contains(".GIF")||fileEntity.FileStoragePath.ToUpper().Contains(".JPEG"))
            {
                if (!System.IO.File.Exists(path))//判断文件是否存在
                {
                    ConvertImageToPdf(mapPath, path);
                }
                fileEntity.FileStoragePath = fileUrl;
            }
        }
        /// <summary>
        /// 将图片转换为PDF
        /// </summary>
        /// <param name="inputFileName"></param>
        /// <param name="outputFileName"></param>
        public void ConvertImageToPdf(string inputFileName,string outputFileName) 
        {
            // For complete examples and data files, please go to https://github.com/aspose-words/Aspose.Words-for-.NET
            // Create Document and DocumentBuilder. 
            // The builder makes it simple to add content to the document.
            Document doc = new Document();
            DocumentBuilder builder = new DocumentBuilder(doc);

            // Read the image from file, ensure it is disposed.
            using (Image image = Image.FromFile(inputFileName))
            {
                // Find which dimension the frames in this image represent. For example 
                // The frames of a BMP or TIFF are "page dimension" whereas frames of a GIF image are "time dimension". 
                FrameDimension dimension = new FrameDimension(image.FrameDimensionsList[0]);

                // Get the number of frames in the image.
                int framesCount = image.GetFrameCount(dimension);

                // Loop through all frames.
                for (int frameIdx = 0; frameIdx < framesCount; frameIdx++)
                {
                    // Insert a section break before each new page, in case of a multi-frame TIFF.
                    if (frameIdx != 0)
                        builder.InsertBreak(BreakType.SectionBreakNewPage);

                    // Select active frame.
                    image.SelectActiveFrame(dimension, frameIdx);

                    // We want the size of the page to be the same as the size of the image.
                    // Convert pixels to points to size the page to the actual image size.
                    PageSetup ps = builder.PageSetup;
                    ps.PageWidth = ConvertUtil.PixelToPoint(image.Width, image.HorizontalResolution);
                    ps.PageHeight = ConvertUtil.PixelToPoint(image.Height, image.VerticalResolution);

                    // Insert the image into the document and position it at the top left corner of the page.
                    builder.InsertImage(
                        image,
                        RelativeHorizontalPosition.Page,
                        0,
                        RelativeVerticalPosition.Page,
                        0,
                        ps.PageWidth,
                        ps.PageHeight,
                        WrapType.None);
                }
            }

            // Save the document to PDF.
            doc.Save(outputFileName);
        }

        //相对路径转换成服务器本地物理路径
        private string urlconvertorlocal(string imagesurl1)
        {
            string tmpRootDir = Server.MapPath(System.Web.HttpContext.Current.Request.ApplicationPath.ToString());//获取程序根目录
            string imagesurl2 = tmpRootDir + imagesurl1.Replace(@"/", @"/"); //转换成绝对路径
            return imagesurl2;
        }

        #endregion

        #region 文件上传

        public ActionResult Upload()
        {
            return View();
        }

        #region 上传文件保存

        /// <summary>
        /// 文件上传
        /// </summary>
        /// <returns></returns>
        public ActionResult UplaodFile(string keyID, string menuTable, string DataId)
        {
            if (!string.IsNullOrWhiteSpace(keyID))
            {
                var fileContext = SaveFiles();
                if (fileContext.Count > 0)
                {
                    foreach (var item in fileContext)
                    {
                        var entity = new TbAttachment
                        {

                            FileID = keyID,
                            FunModule = "test",
                            FileType = item.Format,
                            FileSize = Convert.ToInt32(item.Size),
                            FileName = item.Name,
                            FileModified = DateTime.Now.ToString(),
                            StartTime = DateTime.Now,
                            LastTime = DateTime.Now,
                            FileStoragePath = item.Url,
                            UserCode = OperatorProvider.Provider.CurrentUser.UserCode,
                            StorageOver = 0,
                            StorageSize = 0,
                            ServerCode = "",
                            FileToKen = "",

                        };
                        if (!string.IsNullOrWhiteSpace(DataId))
                        {
                            var anyRet = _attachmentImp.AnyInfo(menuTable, Convert.ToInt32(DataId));
                            if (anyRet != null && anyRet.Rows.Count > 0)
                            {

                                //保存业务表中附件字段
                                bool flag = _attachmentImp.SaveDataFile(keyID, menuTable, Convert.ToInt32(DataId));
                                if (flag)
                                {
                                    //将上传的相关信息存入数据中
                                    Repository<TbAttachment>.Insert(entity);
                                }
                            }
                            else
                            {
                                //将上传的相关信息存入数据中
                                Repository<TbAttachment>.Insert(entity);
                            }
                        }
                        else
                        {
                            //将上传的相关信息存入数据中
                            Repository<TbAttachment>.Insert(entity);
                        }
                        //获取上传附件的ID
                        var model = Repository<TbAttachment>.GetAll().Where(d => d.FileID == keyID).OrderByDescending(d => d.id).FirstOrDefault();
                        item.ServerFileID = Convert.ToString(model.id);
                        item.keyID = keyID;
                    }
                }
                return Json(fileContext);
            }
            else
            {
                return Json("");
            }

        }


        #region 保存上传文件
        /// <summary>
        /// 保存上传文件
        /// </summary>
        /// <returns></returns>
        List<FileContext> SaveFiles()
        {
            try
            {
                //保存后的文件
                var fileSaveList = new List<FileContext>();

                var fileContext = Request.Files;
                for (int i = 0; i < fileContext.Count; i++)
                {
                    var fileItem = fileContext[i];
                    var fileFormat = Path.GetExtension(fileItem.FileName).ToLower();
                    var fileConfig = System.Configuration.ConfigurationManager.AppSettings["uploadFile"];
                    var fileFolder = string.Format("/{0}/{1}/", fileConfig, DateTime.Now.ToString("yyyy-MM-dd"));      //文件路径
                    var path = Server.MapPath(fileFolder);                                                            //文件物理路径
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(path));                             //创建文件夹
                    }
                    string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssff") + new Random().Next(1000, 9999);
                    //文件相对路径
                    var relativePath = String.Format("{0}{1}{2}", fileFolder, newFileName, fileFormat);
                    //文件绝对路径
                    string fullPath = String.Format("{0}{1}{2}", path, newFileName, fileFormat);
                    fileItem.SaveAs(fullPath);


                    //将保存后的文件存入集合中
                    fileSaveList.Add(new FileContext { Name = Path.GetFileNameWithoutExtension(fileItem.FileName), NewName = newFileName, Url = relativePath, Format = fileFormat, Size = fileItem.ContentLength.ToString() });
                }
                return fileSaveList;
            }
            catch (Exception)
            {
            }
            return new List<FileContext>();
        }

        #endregion

        #endregion

        #endregion

        #region 文件删除
        public ActionResult Del(string menuTable, int id = -1)
        {
            var data = _attachmentImp.Del(menuTable, id);
            return Content(data.ToJson());
        }

        #endregion

        #region 文件下载

        /// <summary>
        /// 下载
        /// </summary>
        /// <returns></returns>
        public ActionResult FileDownloadNew(string FileID)
        {
            if (!string.IsNullOrEmpty(FileID))
            {
                //通过附件ID查询附件
                string sql = @"select * from TbAttachment where ID=" + FileID + "";
                var dt = Db.Context.FromSql(sql).ToDataTable();
                if (dt != null && dt.Rows.Count > 0)
                {
                    string type = dt.Rows[0]["FileType"].ToString();
                    if (dt.Rows[0]["FileType"].ToString()=="application/octet-stream")
                    {
                        type = ".jpg";
                    }
                    FileStream fs = new FileStream(Server.MapPath(dt.Rows[0]["FileStoragePath"].ToString()), FileMode.Open);
                    byte[] bytes = new byte[(int)fs.Length];
                    fs.Read(bytes, 0, bytes.Length);
                    fs.Close();
                    Response.Charset = "UTF-8";
                    Response.ContentEncoding = System.Text.Encoding.GetEncoding("UTF-8");
                    Response.ContentType = "application/octet-stream";
                    Response.AddHeader("Content-Disposition", "attachment; filename=" + dt.Rows[0]["FileName"].ToString() + type);
                    Response.BinaryWrite(bytes);
                    Response.Flush();
                    Response.End();
                    return Content("下载成功");
                }
                else
                {
                    return Content("文件不存在");
                }
            }
            else
            {
                return Content("文件不存在");
            }

        }

        #endregion

        /// <summary>
        /// 获取附件地址
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult GetDownFileModel(int id = -1)
        {
            var data = _attachmentImp.GetUploadModel(id);
            var path = data.FileStoragePath;
            return Content(path);
        }

        #endregion

    }
    class FileContext
    {
        /// <summary>
        /// 存入数据中的文件主键ID
        /// </summary>
        public string ServerFileID { get; set; }

        /// <summary>
        /// 文件原名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 当前文件名称
        /// </summary>
        public string NewName { get; set; }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 文件格式
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        ///文件大小
        /// </summary>
        public string Size { get; set; }

        public string keyID { get; set; }
    }
}
