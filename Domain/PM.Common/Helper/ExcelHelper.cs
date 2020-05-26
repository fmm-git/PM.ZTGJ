using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using NPOI.HPSF;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.HSSF.Util;
using NPOI.XSSF.UserModel;
using System.Data;

namespace PM.Common.Helper
{
    /// <summary>
    /// Excel操作类
    /// </summary>
    public class ExcelHelper
    {
        #region Excel导入

        /// <summary>
        /// 从Excel取数据并记录到List集合里
        /// </summary>
        /// <param name="cellHeard">单元头的值和名称：{ { "UserName", "姓名" }, { "Age", "年龄" } };</param>
        /// <param name="filePath">保存文件绝对路径</param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns>转换后的List对象集合</returns>
        public static List<T> ExcelToEntityList<T>(Dictionary<string, string> cellHeard, string filePath,
            out StringBuilder errorMsg) where T : new()
        {
            List<T> enlist = new List<T>();
            errorMsg = new StringBuilder();
            try
            {
                if (Regex.IsMatch(filePath, ".xls$")) // 2003
                {
                    enlist = ExcelToEntityList<T>(cellHeard, filePath, null, out errorMsg);
                }
                else if (Regex.IsMatch(filePath, ".xlsx$")) // 2007
                {
                    //return FailureResultMsg("请选择Excel文件"); // 未设计
                }
                return enlist;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 传入数据流
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cellHeard"></param>
        /// <param name="stream"></param>
        /// <param name="exName"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public static List<T> ExcelToEntityList<T>(Dictionary<string, string> cellHeard, Stream stream, string exName,
            out StringBuilder errorMsg) where T : new()
        {
            var enlist = new List<T>();
            errorMsg = new StringBuilder();
            try
            {
                var ex = exName.ToLower();
                if (stream != null && (ex == ".xls" || ex == ".xlsx"))
                {
                    enlist = ExcelToEntityList<T>(cellHeard, exName, stream, out errorMsg);
                }

                return enlist;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion Excel导入

        #region Excel导出

        /// <summary>
        /// 实体类集合导出到EXCLE2003
        /// </summary>
        /// <param name="cellHeard">单元头的Key和Value：{ { "UserName", "姓名" }, { "Age", "年龄" } };</param>
        /// <param name="enList">数据源</param>
        /// <param name="sheetName">工作表名称</param>
        /// <returns>文件的下载地址</returns>
        public static string EntityListToExcel(Dictionary<string, string> cellHeard, IList enList, string sheetName, string[] items = null)
        {
            try
            {
                string fileName = sheetName + "-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xls"; // 文件名称
                string urlPath = "UpFiles/ExcelFiles/" + fileName; // 文件下载的URL地址，供给前台下载
                string filePath = HttpContext.Current.Server.MapPath("\\" + urlPath); // 文件路径

                // 1.检测是否存在文件夹，若不存在就建立个文件夹
                string directoryName = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }

                // 2.解析单元格头部，设置单元头的中文名称
                HSSFWorkbook workbook = new HSSFWorkbook(); // 工作簿
                ISheet sheet = workbook.CreateSheet(sheetName); // 工作表
                IRow row = sheet.CreateRow(0);
                List<string> keys = cellHeard.Keys.ToList();
                for (int i = 0; i < keys.Count; i++)
                {
                    row.CreateCell(i).SetCellValue(cellHeard[keys[i]]); // 列名为Key的值
                }

                //有下拉选项，增加下拉表头
                if (items != null)
                {
                    var regions = new CellRangeAddressList(1, 65535, 0, 0);
                    var constraint = DVConstraint.CreateExplicitListConstraint(items); //这是下拉选项值
                    var dataValidate = new HSSFDataValidation(regions, constraint);

                    sheet.AddValidationData(dataValidate);
                }

                // 3.List对象的值赋值到Excel的单元格里
                int rowIndex = 1; // 从第二行开始赋值(第一行已设置为单元头)
                foreach (var en in enList)
                {
                    IRow rowTmp = sheet.CreateRow(rowIndex);
                    for (int i = 0; i < keys.Count; i++) // 根据指定的属性名称，获取对象指定属性的值
                    {
                        string cellValue = ""; // 单元格的值
                        object properotyValue = null; // 属性的值
                        PropertyInfo properotyInfo = null; // 属性的信息

                        // 3.1 若属性头的名称包含'.',就表示是子类里的属性，那么就要遍历子类，eg：UserEn.UserName
                        if (keys[i].IndexOf(".") >= 0)
                        {
                            // 3.1.1 解析子类属性(这里只解析1层子类，多层子类未处理)
                            string[] properotyArray = keys[i].Split(new string[] { "." },
                                StringSplitOptions.RemoveEmptyEntries);
                            string subClassName = properotyArray[0]; // '.'前面的为子类的名称
                            string subClassProperotyName = properotyArray[1]; // '.'后面的为子类的属性名称
                            PropertyInfo subClassInfo = en.GetType().GetProperty(subClassName);
                            // 获取子类的类型
                            if (subClassInfo != null)
                            {
                                // 3.1.2 获取子类的实例
                                var subClassEn = en.GetType().GetProperty(subClassName).GetValue(en, null);
                                // 3.1.3 根据属性名称获取子类里的属性类型
                                properotyInfo = subClassInfo.PropertyType.GetProperty(subClassProperotyName);
                                if (properotyInfo != null)
                                {
                                    properotyValue = properotyInfo.GetValue(subClassEn, null); // 获取子类属性的值
                                }
                            }
                        }
                        else
                        {
                            // 3.2 若不是子类的属性，直接根据属性名称获取对象对应的属性
                            properotyInfo = en.GetType().GetProperty(keys[i]);
                            if (properotyInfo != null)
                            {
                                properotyValue = properotyInfo.GetValue(en, null);
                            }
                        }

                        // 3.3 属性值经过转换赋值给单元格值
                        if (properotyValue != null)
                        {
                            cellValue = properotyValue.ToString();
                            // 3.3.1 对时间初始值赋值为空
                            if (cellValue.Trim() == "0001/1/1 0:00:00" || cellValue.Trim() == "0001/1/1 23:59:59")
                            {
                                cellValue = "";
                            }
                        }

                        // 3.4 填充到Excel的单元格里
                        rowTmp.CreateCell(i).SetCellValue(cellValue);
                    }
                    rowIndex++;
                }
                //sheet.CreateFreezePane(4, rowIndex, 1, 0);
                // 4.生成文件
                FileStream file = new FileStream(filePath, FileMode.Create);
                workbook.Write(file);
                file.Close();

                // 5.返回下载路径
                return urlPath;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 实体类集合导出到EXCLE(内存流)
        /// </summary>
        /// <param name="cellHeard">单元头的Key和Value：{ { "UserName", "姓名" }, { "Age", "年龄" } };</param>
        /// <param name="enList">数据源</param>
        /// <param name="sheetName">工作表名称</param>
        /// <returns>数据流</returns>
        public static MemoryStream EntityListToExcelStream(Dictionary<string, string> cellHeard, IList enList, string sheetName, string[] items = null)
        {
            try
            {

                // 1.解析单元格头部，设置单元头的中文名称
                HSSFWorkbook workbook = new HSSFWorkbook(); // 工作簿
                ISheet sheet = workbook.CreateSheet(sheetName); // 工作表
                IRow row = sheet.CreateRow(0);
                List<string> keys = cellHeard.Keys.ToList();

                #region 表头单元格样式

                //单元格样式
                ICellStyle style = workbook.CreateCellStyle();//创建样式对象
                IFont font = workbook.CreateFont(); //创建一个字体样式对象
                font.FontName = "微软雅黑"; //字体
                font.Color = HSSFColor.White.Index;//字体颜色
                font.IsItalic = false; //斜体
                font.FontHeightInPoints = 12;//字体大小
                font.Boldweight = short.MaxValue;//字体加粗
                style.SetFont(font); //将字体样式赋给样式对象
                style.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;//表格线
                style.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                style.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
                style.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
                style.Alignment = HorizontalAlignment.Center;//水平居中
                style.VerticalAlignment = VerticalAlignment.Center;//垂直居中
                style.FillPattern = FillPattern.SolidForeground;//背景颜色
                style.FillForegroundColor = 57;
                row.Height = 30 * 20;//表头行高

                #endregion

                for (int i = 0; i < keys.Count; i++)
                {
                    row.CreateCell(i).SetCellValue(cellHeard[keys[i]]); // 列名为Key的值
                    //设置Head的样式
                    row.GetCell(i).CellStyle = style;
                    sheet.SetColumnWidth(i, 13 * 256);//列宽
                }

                //有下拉选项，增加下拉表头
                if (items != null)
                {
                    var regions = new CellRangeAddressList(1, 65535, 0, 0);
                    var constraint = DVConstraint.CreateExplicitListConstraint(items); //这是下拉选项值
                    var dataValidate = new HSSFDataValidation(regions, constraint);

                    sheet.AddValidationData(dataValidate);
                }

                // 2.List对象的值赋值到Excel的单元格里
                int rowIndex = 1; // 从第二行开始赋值(第一行已设置为单元头)
                foreach (var en in enList)
                {
                    IRow rowTmp = sheet.CreateRow(rowIndex);
                    for (int i = 0; i < keys.Count; i++) // 根据指定的属性名称，获取对象指定属性的值
                    {
                        string cellValue = ""; // 单元格的值
                        object properotyValue = null; // 属性的值
                        PropertyInfo properotyInfo = null; // 属性的信息

                        // 2.1 若属性头的名称包含'.',就表示是子类里的属性，那么就要遍历子类，eg：UserEn.UserName
                        if (keys[i].IndexOf(".") >= 0)
                        {
                            // 2.1.1 解析子类属性(这里只解析1层子类，多层子类未处理)
                            string[] properotyArray = keys[i].Split(new string[] { "." },
                                StringSplitOptions.RemoveEmptyEntries);
                            string subClassName = properotyArray[0]; // '.'前面的为子类的名称
                            string subClassProperotyName = properotyArray[1]; // '.'后面的为子类的属性名称
                            PropertyInfo subClassInfo = en.GetType().GetProperty(subClassName);
                            // 获取子类的类型
                            if (subClassInfo != null)
                            {
                                // 2.1.2 获取子类的实例
                                var subClassEn = en.GetType().GetProperty(subClassName).GetValue(en, null);
                                // 2.1.3 根据属性名称获取子类里的属性类型
                                properotyInfo = subClassInfo.PropertyType.GetProperty(subClassProperotyName);
                                if (properotyInfo != null)
                                {
                                    properotyValue = properotyInfo.GetValue(subClassEn, null); // 获取子类属性的值
                                }
                            }
                        }
                        else
                        {
                            // 2.2 若不是子类的属性，直接根据属性名称获取对象对应的属性
                            properotyInfo = en.GetType().GetProperty(keys[i]);
                            if (properotyInfo != null)
                            {
                                properotyValue = properotyInfo.GetValue(en, null);
                            }
                        }

                        // 2.3 属性值经过转换赋值给单元格值
                        if (properotyValue != null)
                        {
                            cellValue = properotyValue.ToString();
                            // 2.3.1 对时间初始值赋值为空
                            if (cellValue.Trim() == "0001/1/1 0:00:00" || cellValue.Trim() == "0001/1/1 23:59:59")
                            {
                                cellValue = "";
                            }
                        }
                        // 2.4 填充到Excel的单元格里
                        //判断类型是否是数字
                        var typeStr = properotyInfo.PropertyType.Name.ToLower();
                        if (typeStr == "decimal")
                        {
                            double money;
                            var ret = double.TryParse(cellValue, out money);
                            if (ret)
                            {
                                rowTmp.CreateCell(i).SetCellValue(money);
                            }
                            else
                            {
                                rowTmp.CreateCell(i).SetCellValue(cellValue);
                            }
                        }
                        else
                        {
                            rowTmp.CreateCell(i).SetCellValue(cellValue);
                        }
                    }
                    rowIndex++;
                }

                // 3.写入到客户端 
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                workbook.Write(ms);
                ms.Seek(0, SeekOrigin.Begin);
                return ms;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 实体类集合导出到EXCLE(内存流-合并单元格)
        /// </summary>
        /// <param name="enList">数据源</param>
        /// <param name="sheetName">工作表名称</param>
        /// <returns>数据流</returns>
        public static MemoryStream EntityListToExcelStream<T>(IList enList, string sheetName, string sumRow = "")
        {
            try
            {
                // 1.解析单元格头部，设置单元头的中文名称
                HSSFWorkbook workbook = new HSSFWorkbook(); // 工作簿
                ISheet sheet = workbook.CreateSheet(sheetName); // 工作表
                IRow row = sheet.CreateRow(0);

                #region 表头单元格样式

                //单元格样式
                ICellStyle style = workbook.CreateCellStyle();//创建样式对象
                IFont font = workbook.CreateFont(); //创建一个字体样式对象
                font.FontName = "微软雅黑"; //字体
                font.Color = HSSFColor.White.Index;//字体颜色
                font.IsItalic = false; //斜体
                font.FontHeightInPoints = 12;//字体大小
                font.Boldweight = short.MaxValue;//字体加粗
                style.SetFont(font); //将字体样式赋给样式对象
                style.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;//表格线
                style.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                style.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
                style.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
                style.Alignment = HorizontalAlignment.Center;//水平居中
                style.VerticalAlignment = VerticalAlignment.Center;//垂直居中
                style.FillPattern = FillPattern.SolidForeground;//背景颜色
                style.FillForegroundColor = 57;
                row.Height = 30 * 20;//表头行高

                #endregion

                List<ExcelHead> cellHeard = GetAttribute<T>.GetAttributeSignName();
                //合并行数
                int rangeRowCount = 0;
                for (int i = 0; i < cellHeard.Count; i++)
                {
                    row.CreateCell(i).SetCellValue(cellHeard[i].Value); // 列名为Key的值
                    //设置Head的样式
                    row.GetCell(i).CellStyle = style;
                    sheet.SetColumnWidth(i, cellHeard[i].ColumnWidthExcel);//列宽
                    if (cellHeard[i].Key.IndexOf("_") >= 0)
                    {
                        string[] properotyArray = cellHeard[i].Key.Split(new string[] { "_" },
                            StringSplitOptions.RemoveEmptyEntries);
                        rangeRowCount = properotyArray.Length - 1;
                    }
                }
                // 2.List对象的值赋值到Excel的单元格里  
                List<string> keys = cellHeard.Select(p => p.Key).ToList();
                int rowIndex = 1; // 从第二行开始赋值(第一行已设置为单元头)
                //单元格样式
                ICellStyle styleCell = workbook.CreateCellStyle();//创建样式对象
                styleCell.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;//表格线
                styleCell.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                styleCell.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
                styleCell.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
                styleCell.VerticalAlignment = VerticalAlignment.Center;//垂直居中
                foreach (var en in enList)
                {
                    IRow rowTmp = sheet.CreateRow(rowIndex);
                    rowTmp.Height = 300;
                    IRow rowTmp2 = null;
                    if (rangeRowCount > 0)
                    {
                        rowTmp2 = sheet.CreateRow(rowIndex + rangeRowCount);
                        rowTmp2.Height = 300;
                    }
                    for (int i = 0; i < keys.Count; i++) // 根据指定的属性名称，获取对象指定属性的值
                    {
                        object properotyValue = null; // 属性的值
                        PropertyInfo properotyInfo = null; // 属性的信息

                        // 2.1 若属性头的名称包含'.',就表示是子类里的属性，那么就要遍历子类，eg：UserEn.UserName
                        if (keys[i].IndexOf("_") >= 0)
                        {
                            // 2.1.1 解析子类属性(这里只解析1层子类，多层子类未处理)
                            string[] properotyArray = keys[i].Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
                            string subClassName = properotyArray[0]; // '.'前面的为子类的名称
                            string subClassProperotyName = properotyArray[1]; // '.'后面的为子类的属性名称  
                            properotyInfo = en.GetType().GetProperty(subClassName);
                            if (properotyInfo != null)
                            {
                                properotyValue = properotyInfo.GetValue(en, null);
                            }
                            //单元格赋值
                            GetPropertyToExcelCell(properotyValue, properotyInfo, rowTmp, i);
                            rowTmp.GetCell(i).CellStyle = styleCell;
                            // 获取子类的类型
                            object subProperotyValue = null; // 属性的值
                            PropertyInfo subClassInfo = en.GetType().GetProperty(subClassProperotyName);
                            if (subClassInfo != null)
                            {
                                subProperotyValue = subClassInfo.GetValue(en, null);
                                //单元格赋值
                                GetPropertyToExcelCell(subProperotyValue, subClassInfo, rowTmp2, i);
                                rowTmp2.GetCell(i).CellStyle = styleCell;
                            }
                        }
                        else
                        {
                            // 2.2 若不是子类的属性，直接根据属性名称获取对象对应的属性
                            properotyInfo = en.GetType().GetProperty(keys[i]);
                            if (properotyInfo != null)
                            {
                                properotyValue = properotyInfo.GetValue(en, null);
                            }
                            if (rangeRowCount > 0)
                                RangeBuild(sheet, rowIndex, rowIndex + rangeRowCount, i, i);  //合并单元格
                            //单元格赋值
                            GetPropertyToExcelCell(properotyValue, properotyInfo, rowTmp, i);
                            rowTmp.GetCell(i).CellStyle = styleCell;
                            if (rowTmp2 != null)
                            {
                                GetPropertyToExcelCell(null, properotyInfo, rowTmp2, i);
                                rowTmp2.GetCell(i).CellStyle = styleCell;
                            }

                        }
                    }
                    if (rangeRowCount > 0)
                        rowIndex += rangeRowCount;
                    rowIndex++;
                }
                //判断是否有汇总行
                if (!string.IsNullOrEmpty(sumRow))
                    AddSumRow(workbook, sheet, rowIndex, keys.Count, sumRow);

                // 3.写入到客户端 
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                workbook.Write(ms);
                ms.Seek(0, SeekOrigin.Begin);
                return ms;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// DataTable导出到Excel的MemoryStream
        /// </summary>
        /// <param name="dtSource">源DataTable</param>
        /// <param name="sheetName">导出的文件名</param>
        /// <param name="hzzfc">汇总字符串</param>
        public static MemoryStream ExportToMemoryStream(List<ExcelHead> cellHeard, DataTable dtSource, string sheetName = "", string hzzfc = "")
        {
            try
            {
                // 1.解析单元格头部，设置单元头的中文名称
                HSSFWorkbook workbook = new HSSFWorkbook(); // 工作簿
                ISheet sheet = workbook.CreateSheet(sheetName); // 工作表
                IRow cellRow = sheet.CreateRow(0);

                #region 表头单元格样式

                //单元格样式
                ICellStyle style = workbook.CreateCellStyle();//创建样式对象
                IFont font = workbook.CreateFont(); //创建一个字体样式对象
                font.FontName = "微软雅黑"; //字体
                font.Color = HSSFColor.White.Index;//字体颜色
                font.IsItalic = false; //斜体
                font.FontHeightInPoints = 12;//字体大小
                font.Boldweight = short.MaxValue;//字体加粗
                style.SetFont(font); //将字体样式赋给样式对象
                style.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;//表格线
                style.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                style.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
                style.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
                style.Alignment = HorizontalAlignment.Center;//水平居中
                style.VerticalAlignment = VerticalAlignment.Center;//垂直居中
                style.FillPattern = FillPattern.SolidForeground;//背景颜色
                style.FillForegroundColor = 57;
                cellRow.Height = 30 * 20;//表头行高

                #endregion

                //合并行数
                List<string> keys = cellHeard.Select(p => p.Key).ToList();
                int rowIndex = 1; // 从第二行开始赋值(第一行已设置为单元头)
                for (int i = 0; i < cellHeard.Count; i++)
                {
                    cellRow.CreateCell(i).SetCellValue(cellHeard[i].Value); // 列名为Key的值
                    //设置Head的样式
                    cellRow.GetCell(i).CellStyle = style;
                    sheet.SetColumnWidth(i, cellHeard[i].ColumnWidthExcel);//列宽
                }

                ICellStyle cellStyle1 = workbook.CreateCellStyle();
                cellStyle1.BorderLeft = BorderStyle.Thin;
                cellStyle1.BorderRight = BorderStyle.Thin;
                cellStyle1.BorderTop = BorderStyle.Thin;
                cellStyle1.BorderBottom = BorderStyle.Thin;
                cellStyle1.WrapText = true;//自动换行
                //日期格式的单元格格式
                ICellStyle csDateFormat = workbook.CreateCellStyle();
                csDateFormat.BorderLeft = BorderStyle.Thin;
                csDateFormat.BorderRight = BorderStyle.Thin;
                csDateFormat.BorderTop = BorderStyle.Thin;
                csDateFormat.BorderBottom = BorderStyle.Thin;

                foreach (DataRow row in dtSource.Rows)
                {
                    #region 填充内容

                    IRow dataRow = sheet.CreateRow(rowIndex);
                    for (int i = 0; i < keys.Count; i++)
                    {
                        ICell newCell = dataRow.CreateCell(i);
                        newCell.CellStyle = cellStyle1;
                        string drValue = row[row.Table.Columns["" + keys[i] + ""].ToString()].ToString(); //因列都是字符串型的，所以循环Parse来确认列的对应数据格式。 
                        DateTime dateV;
                        double doubV = 0;
                        //大于8字节的才被认为符合日期格式（2000-1-1），防止-1也被认为是日期 
                        if (drValue.Length >= 8 && DateTime.TryParse(drValue, out dateV))
                        {
                            newCell.CellStyle = csDateFormat;
                            if (dateV == Convert.ToDateTime(null))
                            {
                                newCell.SetCellValue("");
                            }
                            else
                            {
                                var d = dateV.ToString("yyyy-MM-dd");
                                var islangt = cellHeard.First(p => p.Key == keys[i]).Islangt;
                                if (islangt)
                                    d = dateV.ToString("yyyy-MM-dd HH:mm:ss");
                                newCell.SetCellValue(d);
                            }
                        }
                        else
                        {
                            newCell.SetCellValue(drValue);
                        }
                    }

                    #endregion
                    rowIndex++;
                }

                //判断是否有汇总行
                if (!string.IsNullOrEmpty(hzzfc))
                    AddSumRow(workbook, sheet, rowIndex, keys.Count, hzzfc);
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                workbook.Write(ms);
                ms.Seek(0, SeekOrigin.Begin);
                return ms;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        #endregion Excel导出

        #region 下载带下拉选项的模板

        /// <summary>
        /// 下载带下拉选项的模板
        /// </summary>
        /// <param name="cellHeard"></param>
        /// <param name="items">下拉选项</param>
        /// <param name="sheetName">sheet名称</param>
        /// <param name="excelName">excel名称</param>
        /// <param name="description">描述</param>
        /// <returns></returns>
        public static string CreateDropdownExcel(Dictionary<string, string> cellHeard, string[] items, string sheetName, string excelName, string description)
        {
            HSSFWorkbook wk = new HSSFWorkbook();
            DocumentSummaryInformation dsi = PropertySetFactory.CreateDocumentSummaryInformation();
            dsi.Company = "";
            wk.DocumentSummaryInformation = dsi;

            //创建一个名称为mySheet的表
            ISheet tb = wk.CreateSheet(sheetName);
            ICellStyle cellStyle = wk.CreateCellStyle();
            //导入说明sheet
            try
            {
                IDataFormat textFormat = wk.CreateDataFormat();
                cellStyle.DataFormat = textFormat.GetFormat("text");

                IRow row = tb.CreateRow(0);
                List<string> keys = cellHeard.Keys.ToList();
                for (int i = 0; i < keys.Count; i++)
                {
                    row.CreateCell(i).SetCellValue(cellHeard[keys[i]]); // 列名为Key的值

                    //第一列选择框除外，其余都设为文本 todo 根据header的类型来设置
                    if (i > 0)
                        tb.SetDefaultColumnStyle(i, cellStyle);
                }

                CellRangeAddressList regions = new CellRangeAddressList(1, 65535, 0, 0);
                DVConstraint constraint = DVConstraint.CreateExplicitListConstraint(items);//这是下拉选项值
                HSSFDataValidation dataValidate = new HSSFDataValidation(regions, constraint);
                tb.AddValidationData(dataValidate);

                //设置
            }
            catch (Exception e)
            {
                throw e;
            }

            string fileName = excelName + "-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xls"; // 文件名称
            string urlPath = "UpFiles/TemplateFiles/" + fileName; // 文件下载的URL地址，供给前台下载
            string filePath = HttpContext.Current.Server.MapPath("\\" + urlPath); // 文件路径

            // 1.检测是否存在文件夹，若不存在就建立个文件夹
            string directoryName = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            FileStream file = new FileStream(filePath, FileMode.Create);
            wk.Write(file);
            file.Close();

            return filePath;
        }

        #endregion

        #region 私有

        /// <summary>
        /// 从Excel2003取数据并记录到List集合里
        /// </summary>
        /// <param name="cellHeard">单元头的Key和Value：{ { "UserName", "姓名" }, { "Age", "年龄" } };</param>
        /// <param name="fileExt">文件扩展名</param>
        /// <param name="stream"></param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns>转换好的List对象集合</returns>
        private static List<T> ExcelToEntityList<T>(Dictionary<string, string> cellHeard, string fileExt, Stream stream,
            out StringBuilder errorMsg) where T : new()
        {
            errorMsg = new StringBuilder(); // 错误信息,Excel转换到实体对象时，会有格式的错误信息
            List<T> enlist = new List<T>(); // 转换后的集合
            List<string> keys = cellHeard.Keys.ToList(); // 要赋值的实体对象属性名称
            try
            {
                if (stream != null)
                {
                    ReadFromStream(cellHeard, errorMsg, fileExt, stream, keys, enlist);
                }
                return enlist;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void ReadFromStream<T>(Dictionary<string, string> cellHeard, StringBuilder errorMsg, string fileExt, Stream fs, List<string> keys,
            List<T> enlist) where T : new()
        {
            IWorkbook workbook;
            //XSSFWorkbook 适用XLSX格式，HSSFWorkbook 适用XLS格式
            if (fileExt == ".xlsx") { workbook = new XSSFWorkbook(fs); } else { workbook = new HSSFWorkbook(fs); }
            ISheet sheet = workbook.GetSheetAt(0);
            for (int i = 1; i <= sheet.LastRowNum; i++) // 从1开始，第0行为单元头
            {
                // 1.判断当前行是否空行，若空行就不在进行读取下一行操作，结束Excel读取操作
                if (sheet.GetRow(i) == null)
                {
                    break;
                }

                T en = new T();
                string errStr = ""; // 当前行转换时，是否有错误信息，格式为：第1行数据转换异常：XXX列；
                for (int j = 0; j < keys.Count; j++)
                {
                    // 2.若属性头的名称包含'.',就表示是子类里的属性，那么就要遍历子类，eg：UserEn.TrueName
                    if (keys[j].IndexOf(".") >= 0)
                    {
                        // 2.1解析子类属性
                        string[] properotyArray = keys[j].Split(new string[] { "." },
                            StringSplitOptions.RemoveEmptyEntries);
                        string subClassName = properotyArray[0]; // '.'前面的为子类的名称
                        string subClassProperotyName = properotyArray[1]; // '.'后面的为子类的属性名称
                        PropertyInfo subClassInfo = en.GetType().GetProperty(subClassName);
                        // 获取子类的类型
                        if (subClassInfo != null)
                        {
                            // 2.1.1 获取子类的实例
                            var subClassEn = en.GetType().GetProperty(subClassName).GetValue(en, null);
                            // 2.1.2 根据属性名称获取子类里的属性信息
                            PropertyInfo properotyInfo =
                                subClassInfo.PropertyType.GetProperty(subClassProperotyName);
                            if (properotyInfo != null)
                            {
                                try
                                {
                                    // Excel单元格的值转换为对象属性的值，若类型不对，记录出错信息
                                    properotyInfo.SetValue(subClassEn,
                                        GetExcelCellToProperty(properotyInfo.PropertyType,
                                            sheet.GetRow(i).GetCell(j)), null);
                                }
                                catch (Exception e)
                                {
                                    if (errStr.Length == 0)
                                    {
                                        errStr = "第" + i + "行数据转换异常：";
                                    }
                                    errStr += cellHeard[keys[j]] + "列；";
                                }
                            }
                        }
                    }
                    else
                    {
                        // 3.给指定的属性赋值
                        PropertyInfo properotyInfo = en.GetType().GetProperty(keys[j]);
                        if (properotyInfo != null)
                        {
                            try
                            {
                                // Excel单元格的值转换为对象属性的值，若类型不对，记录出错信息
                                properotyInfo.SetValue(en,
                                    GetExcelCellToProperty(properotyInfo.PropertyType,
                                        sheet.GetRow(i).GetCell(j)), null);
                            }
                            catch (Exception e)
                            {
                                if (errStr.Length == 0)
                                {
                                    errStr = "第" + i + "行数据转换异常：";
                                }
                                errStr += cellHeard[keys[j]] + "列；";
                            }
                        }
                    }
                }
                // 若有错误信息，就添加到错误信息里
                if (errStr.Length > 0)
                {
                    errorMsg.AppendLine(errStr);
                }
                enlist.Add(en);
            }
        }

        /// <summary>
        /// 从Excel获取值传递到对象的属性里
        /// </summary>
        /// <param name="distanceType">目标对象类型</param>
        /// <param name="sourceCell">对象属性的值</param>
        private static Object GetExcelCellToProperty(Type distanceType, ICell sourceCell)
        {
            object rs = distanceType.IsValueType ? Activator.CreateInstance(distanceType) : null;

            // 1.判断传递的单元格是否为空
            if (sourceCell == null || string.IsNullOrEmpty(sourceCell.ToString()))
            {
                return rs;
            }

            // 2.Excel文本和数字单元格转换，在Excel里文本和数字是不能进行转换，所以这里预先存值
            object sourceValue = null;
            switch (sourceCell.CellType)
            {
                case CellType.Blank:
                    break;

                case CellType.Boolean:
                    break;

                case CellType.Error:
                    break;

                case CellType.Formula:
                    break;

                case CellType.Numeric:
                    sourceValue = sourceCell.NumericCellValue;
                    break;

                case CellType.String:
                    sourceValue = sourceCell.StringCellValue;
                    break;

                case CellType.Unknown:
                    break;

                default:
                    break;
            }

            string valueDataType = distanceType.Name;

            // 在这里进行特定类型的处理
            switch (valueDataType.ToLower()) // 以防出错，全部小写
            {
                case "string":
                    rs = sourceValue.ToString();
                    break;
                case "int":
                case "int16":
                case "int32":
                    rs = (int)Convert.ChangeType(sourceCell.NumericCellValue.ToString(), distanceType);
                    break;
                case "float":
                case "single":
                    rs = (float)Convert.ChangeType(sourceCell.NumericCellValue.ToString(), distanceType);
                    break;
                case "decimal":
                    rs = (decimal)Convert.ChangeType(sourceCell.NumericCellValue.ToString(), distanceType);
                    break;
                case "datetime":
                    rs = sourceCell.DateCellValue;
                    break;
                case "guid":
                    rs = (Guid)Convert.ChangeType(sourceCell.NumericCellValue.ToString(), distanceType);
                    return rs;
            }
            return rs;
        }
        private static void GetPropertyToExcelCell(Object properotyValue, PropertyInfo properotyInfo, IRow rowTmp, int i)
        {
            string cellValue = ""; // 单元格的值
            //  属性值经过转换赋值给单元格值
            if (properotyValue != null)
            {
                cellValue = properotyValue.ToString();
                //  对时间初始值赋值为空
                if (cellValue.Trim() == "0001/1/1 0:00:00" || cellValue.Trim() == "0001/1/1 23:59:59")
                {
                    cellValue = "";
                }
            }
            if (string.IsNullOrEmpty(cellValue))
            {
                rowTmp.CreateCell(i).SetCellValue(cellValue);
            }
            else
            {
                //  填充到Excel的单元格里
                //判断类型是否是数字
                var typeStr = properotyInfo.PropertyType.Name.ToLower();
                //if (typeStr == "decimal")
                //{
                //    double money;
                //    var ret = double.TryParse(cellValue, out money);
                //    if (ret)
                //    {
                //        rowTmp.CreateCell(i).SetCellValue(money);
                //    }
                //    else
                //    {
                //        rowTmp.CreateCell(i).SetCellValue(cellValue);
                //    }
                //}
                //else 
                if (typeStr == "datetime")
                {
                    cellValue = (DateTime.Parse(cellValue)).ToString("yyyy-MM-dd");
                    rowTmp.CreateCell(i).SetCellValue(cellValue);
                }
                else if (typeStr.Contains("nullable"))
                {
                    DateTime time;
                    var istime = DateTime.TryParse(cellValue, out time);
                    if (istime)
                    {
                        cellValue = time.ToString("yyyy-MM-dd");
                        rowTmp.CreateCell(i).SetCellValue(cellValue);
                    }
                }
                else
                {
                    rowTmp.CreateCell(i).SetCellValue(cellValue);
                }
            }
        }

        /// <summary>
        /// 保存Excel文件
        /// <para>Excel的导入导出都会在服务器生成一个文件</para>
        /// <para>路径：UpFiles/ExcelFiles</para>
        /// </summary>
        /// <param name="file">传入的文件对象</param>
        /// <returns>如果保存成功则返回文件的位置;如果保存失败则返回空</returns>
        private static string SaveExcelFile(HttpPostedFile file)
        {
            try
            {
                var fileName = file.FileName.Insert(file.FileName.LastIndexOf('.'),
                    "-" + DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                var filePath = Path.Combine(HttpContext.Current.Server.MapPath("~/UpFiles/ExcelFiles"), fileName);
                string directoryName = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }
                file.SaveAs(filePath);
                return filePath;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static void RangeBuild(ISheet sheet, int firstRow, int lastRow, int firstCol, int lastCol)
        {
            CellRangeAddress region = new CellRangeAddress(firstRow, lastRow, firstCol, lastCol);
            sheet.AddMergedRegion(region);
        }
        private static void AddSumRow(IWorkbook workbook, ISheet sheet, int rowIndex, int cumIndex, string sumRow)
        {
            //添加一行(最后一行汇总)
            IRow footRow = sheet.CreateRow(rowIndex);
            footRow.HeightInPoints = 25;
            ICellStyle footStyle = workbook.CreateCellStyle();
            footStyle.VerticalAlignment = VerticalAlignment.Center;
            footStyle.Alignment = HorizontalAlignment.Right;
            IFont font1 = workbook.CreateFont();
            font1.FontHeightInPoints = 12;
            font1.Boldweight = 300;
            font1.Color = HSSFColor.Red.Index;
            footStyle.SetFont(font1);
            footStyle.BorderLeft = BorderStyle.Thin;
            footStyle.BorderRight = BorderStyle.Thin;
            footStyle.BorderTop = BorderStyle.Thin;
            footStyle.BorderBottom = BorderStyle.Thin;
            for (int i = 0; i < cumIndex; i++)
            {
                ICell newCell = footRow.CreateCell(i);
                newCell.CellStyle = footStyle;
            }
            RangeBuild(sheet, rowIndex, rowIndex, 0, cumIndex - 1);  //合并单元格
            footRow.CreateCell(0).SetCellValue(sumRow);
            //设置合并后style
            var cell = sheet.GetRow(rowIndex).GetCell(0);
            cell.CellStyle = footStyle;
        }
        #endregion

        #region NPOI导出

        /// <summary>
        /// DataTable导出到Excel的MemoryStream
        /// </summary>
        /// <param name="dtSource">源DataTable</param>
        /// <param name="strHeaderText">表头文本</param>
        /// <param name="templateFile">模板文件</param>
        /// <param name="fileName">导出的文件名</param>
        /// <param name="hzzfc">汇总字符串</param>
        public static MemoryStream ExportToMemoryStream(Dictionary<string, string> cellHeard, DataTable dtSource, string strHeaderText, string templateFile = "", string fileName = "", string hzzfc = "")
        {
            try
            {
                IWorkbook workbook = null;
                ISheet sheet = null;
                bool isTemplateFile = false;//是否采用模板文件导出
                int rowIndex = 0;
                bool isExcel2007 = false;
                if (!templateFile.IsNullOrEmpty() && File.Exists(templateFile))
                {
                    string templateFilePath = templateFile;
                    isExcel2007 = templateFilePath.EndsWith("xlsx", StringComparison.CurrentCultureIgnoreCase);
                    using (FileStream file = new FileStream(templateFilePath, FileMode.Open, FileAccess.Read))
                    {
                        if (isExcel2007)
                        {
                            workbook = new XSSFWorkbook(file);
                        }
                        else
                        {
                            workbook = new HSSFWorkbook(file);
                        }
                    }
                    if (workbook != null)
                    {
                        sheet = workbook.GetSheetAt(0);
                        if (sheet != null)
                        {
                            isTemplateFile = true;
                            rowIndex = sheet.LastRowNum + 1;
                        }
                    }
                }
                else
                {
                    isExcel2007 = fileName.EndsWith("xlsx", StringComparison.CurrentCultureIgnoreCase);
                }
                if (workbook == null)
                {
                    if (isExcel2007)
                    {
                        workbook = new XSSFWorkbook();
                    }
                    else
                    {
                        workbook = new HSSFWorkbook();
                    }
                }
                if (sheet == null)
                {
                    sheet = workbook.CreateSheet("Sheet1");
                }
                int[] arrColWidth = new int[] { };
                string[] arrColDataType = new string[] { };
                List<string> keys = cellHeard.Keys.ToList();
                if (!isTemplateFile)
                {
                    //取得列宽
                    arrColWidth = new int[keys.Count];
                    arrColDataType = new string[keys.Count];
                    int Num = 0;
                    foreach (DataColumn item in dtSource.Columns)
                    {
                        if (keys.Contains(item.ToString()))
                        {
                            if (item.Caption.IsInt())
                            {
                                arrColWidth[Num] = Convert.ToInt32(item.Caption);
                            }
                            else
                            {
                                arrColWidth[Num] = Encoding.GetEncoding(936).GetBytes(item.ColumnName.ToString()).Length;
                            }
                            arrColDataType[Num] = item.DataType.ToString();
                            Num++;
                        }

                    }
                    if (dtSource.Rows.Count > 0)
                    {
                        for (int j = 0; j < dtSource.Columns.Count; j++)
                        {
                            if (keys.Contains(dtSource.Rows[0][j].ToString()))
                            {
                                if (!dtSource.Columns[j].Caption.IsInt())
                                {
                                    int intTemp = Encoding.GetEncoding(936).GetBytes(dtSource.Rows[0][j].ToString()).Length; if (intTemp > arrColWidth[j])
                                    {
                                        arrColWidth[j] = intTemp;
                                    }
                                }
                            }

                        }
                    }
                }
                bool hasHeader = !strHeaderText.IsNullOrEmpty();//是否有表头

                if (!isTemplateFile)
                {
                    #region 新建表，填充表头，填充列头，样式

                    if (rowIndex == 65535 || rowIndex == 0)
                    {
                        #region 表头及样式

                        if (hasHeader)
                        {

                            IRow headerRow = sheet.CreateRow(0);
                            headerRow.HeightInPoints = 25;
                            headerRow.CreateCell(0).SetCellValue(strHeaderText);
                            ICellStyle headStyle = workbook.CreateCellStyle();
                            headStyle.Alignment = HorizontalAlignment.Center;
                            IFont font = workbook.CreateFont();
                            font.FontHeightInPoints = 20;
                            font.Boldweight = 700;
                            headStyle.SetFont(font);
                            headStyle.BorderLeft = BorderStyle.Thin;
                            headStyle.BorderRight = BorderStyle.Thin;
                            headStyle.BorderTop = BorderStyle.Thin;
                            headStyle.BorderBottom = BorderStyle.Thin;
                            headerRow.GetCell(0).CellStyle = headStyle;
                            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, dtSource.Columns.Count - 1));
                        }
                        #endregion

                        #region 列头及样式
                        {

                            IRow headerRow = sheet.CreateRow(hasHeader ? 1 : 0);
                            ICellStyle headStyle = workbook.CreateCellStyle();
                            headStyle.Alignment = HorizontalAlignment.Center;
                            IFont font = workbook.CreateFont();
                            font.FontHeightInPoints = 12;
                            font.Boldweight = 500;
                            headStyle.BorderLeft = BorderStyle.Thin;
                            headStyle.BorderRight = BorderStyle.Thin;
                            headStyle.BorderTop = BorderStyle.Thin;
                            headStyle.BorderBottom = BorderStyle.Thin;
                            headStyle.Alignment = HorizontalAlignment.Center;//水平居中
                            headStyle.VerticalAlignment = VerticalAlignment.Center;//垂直居中
                            headStyle.IsLocked = true;
                            headStyle.SetFont(font);
                            headStyle.FillPattern = FillPattern.SolidForeground;//背景颜色
                            headStyle.FillForegroundColor = 57;
                            headerRow.Height = 30 * 20;
                            ICellStyle cellStyle = workbook.CreateCellStyle();

                            for (int i = 0; i < keys.Count; i++)
                            {
                                headerRow.CreateCell(i).SetCellValue(cellHeard[keys[i]]); // 列名为Key的值
                                headerRow.GetCell(i).CellStyle = headStyle;
                                //设置列宽
                                if (arrColWidth.Length > 0)
                                {
                                    //sheet.SetColumnWidth(i, Math.Min(arrColWidth[i] + 1, 200) * 256);
                                    sheet.AutoSizeColumn(i, true);//自适应列宽度
                                }
                            }
                        }
                        #endregion

                        rowIndex = hasHeader ? 2 : 1;
                    }
                    #endregion
                }
                ICellStyle cellStyle1 = workbook.CreateCellStyle();
                cellStyle1.BorderLeft = BorderStyle.Thin;
                cellStyle1.BorderRight = BorderStyle.Thin;
                cellStyle1.BorderTop = BorderStyle.Thin;
                cellStyle1.BorderBottom = BorderStyle.Thin;
                cellStyle1.WrapText = true;//自动换行
                //日期格式的单元格格式
                ICellStyle csDateFormat = workbook.CreateCellStyle();
                csDateFormat.BorderLeft = BorderStyle.Thin;
                csDateFormat.BorderRight = BorderStyle.Thin;
                csDateFormat.BorderTop = BorderStyle.Thin;
                csDateFormat.BorderBottom = BorderStyle.Thin;
                IDataFormat dfDate = workbook.CreateDataFormat();
                csDateFormat.DataFormat = dfDate.GetFormat("yyyy-mm-dd");


                foreach (DataRow row in dtSource.Rows)
                {
                    #region 填充内容

                    IRow dataRow = sheet.CreateRow(rowIndex);
                    if (fileName == "加工工艺")
                    {
                        if (Convert.ToInt32(row["childCount"]) > 1)
                        {
                            if (row["PProcessingTechnologyName"].ToString() == "格栅")
                            {
                                CellRangeAddress region = new CellRangeAddress(3, 2 + Convert.ToInt32(row["childCount"]), 0, 0);
                                sheet.AddMergedRegion(region);
                            }
                            else if (row["PProcessingTechnologyName"].ToString() == "H型钢")
                            {
                                CellRangeAddress region = new CellRangeAddress(8, 7 + Convert.ToInt32(row["childCount"]), 0, 0);
                                sheet.AddMergedRegion(region);
                            }
                        }
                    }
                    for (int i = 0; i < keys.Count; i++)
                    {
                        ICellStyle csNew = workbook.CreateCellStyle();
                        csNew.BorderLeft = BorderStyle.Thin;
                        csNew.BorderRight = BorderStyle.Thin;
                        csNew.BorderTop = BorderStyle.Thin;
                        csNew.BorderBottom = BorderStyle.Thin;
                        csNew.WrapText = true;//自动换行
                        ICell newCell = dataRow.CreateCell(i);
                        if (fileName == "材料库存盘点")
                        {
                            IFont fontNew = workbook.CreateFont();
                            if (row[row.Table.Columns["EarnOrLos"]].ToString() == "-1" && keys[i] == "TotalEarnOrLos")
                            {

                                fontNew.Color = HSSFColor.Red.Index;
                                csNew.SetFont(fontNew);
                            }
                            else if (row[row.Table.Columns["EarnOrLos"]].ToString() != "-1" && keys[i] == "TotalEarnOrLos")
                            {
                                fontNew.Color = HSSFColor.Green.Index;
                                csNew.SetFont(fontNew);
                            }
                            newCell.CellStyle = csNew;
                        }
                        else
                        {
                            newCell.CellStyle = cellStyle1;
                        }
                        string drValue = row[row.Table.Columns["" + keys[i] + ""].ToString()].ToString(); //因列都是字符串型的，所以循环Parse来确认列的对应数据格式。 
                        DateTime dateV;
                        double doubV = 0;
                        //大于8字节的才被认为符合日期格式（2000-1-1），防止-1也被认为是日期 
                        if (drValue.Length >= 8 && DateTime.TryParse(drValue, out dateV))
                        {
                            newCell.CellStyle = csDateFormat;

                            if (dateV == Convert.ToDateTime(null))
                                newCell.SetCellValue("");
                            else
                                newCell.SetCellValue(dateV);
                        }
                        //else if (double.TryParse(drValue, out doubV))
                        //{
                        //    //如果标题含有号、码字样，即使是全数字也显示文本
                        //    if ((dtSource.Columns[i] + "").IndexOf("号") >= 0 || (dtSource.Columns[i] + "").IndexOf("码") >= 0)
                        //    {
                        //        newCell.SetCellValue(drValue);
                        //    }
                        //    else
                        //    {
                        //        newCell.SetCellValue(doubV);
                        //    }

                        //}
                        else
                        {
                            newCell.SetCellValue(drValue);
                        }
                    }

                    #endregion
                    rowIndex++;
                }

                //判断是否有汇总行
                if (!string.IsNullOrEmpty(hzzfc))
                    AddSumRow(workbook, sheet, rowIndex, keys.Count, hzzfc);
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                workbook.Write(ms);
                ms.Seek(0, SeekOrigin.Begin);
                return ms;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }


        ///// <summary>
        ///// Excel复制行
        ///// </summary>
        ///// <param name="wb"></param>
        ///// <param name="sheet"></param>
        ///// <param name="starRow"></param>
        ///// <param name="rows"></param>
        //private void insertRow(HSSFWorkbook wb, HSSFSheet sheet, int starRow, int rows)
        //{
        //    /*
        //     * ShiftRows(int startRow, int endRow, int n, bool copyRowHeight, bool resetOriginalRowHeight);
        //     *
        //     * startRow 开始行
        //     * endRow 结束行
        //     * n 移动行数
        //     * copyRowHeight 复制的行是否高度在移
        //     * resetOriginalRowHeight 是否设置为默认的原始行的高度
        //     *
        //     */

        //    sheet.ShiftRows(starRow + 1, sheet.LastRowNum, rows, true, true);

        //    starRow = starRow - 1;

        //    for (int i = 0; i < rows; i++)
        //    {

        //        HSSFRow sourceRow = null;
        //        HSSFRow targetRow = null;
        //        HSSFCell sourceCell = null;
        //        HSSFCell targetCell = null;

        //        short m;

        //        starRow = starRow + 1;
        //        sourceRow = (HSSFRow)sheet.GetRow(starRow);
        //        targetRow = (HSSFRow)sheet.CreateRow(starRow + 1);
        //        targetRow.HeightInPoints = sourceRow.HeightInPoints;

        //        for (m = (short)sourceRow.FirstCellNum; m < sourceRow.LastCellNum; m++)
        //        {

        //            sourceCell = (HSSFCell)sourceRow.GetCell(m);
        //            targetCell = (HSSFCell)targetRow.CreateCell(m);

        //            targetCell.Encoding = sourceCell.Encoding;
        //            targetCell.CellStyle = sourceCell.CellStyle;
        //            targetCell.SetCellType(sourceCell.CellType);

        //        }
        //    }

        //}



        #endregion

    }
}
