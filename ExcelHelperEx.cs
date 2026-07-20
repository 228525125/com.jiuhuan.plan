using Microsoft.Office.Interop.Excel;
using QRCoder;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace com.jiuhuan.plan.tools {

    public class ExcelHelperEx : ExcelHelper {

        public const int QRCodePictuteWidth = 35;
        public const int QRCodePictuteHeight = 35;

        public static void Print(string file, Dictionary<string,object> record, int times, string printer)
        {
            Application excelApp = new Application();
            excelApp.DisplayAlerts = false;
            excelApp.Visible = true;
            Workbook book = excelApp.Workbooks.Open(file);
            Worksheet sheet = book.Worksheets["Sheet1"];

            for (int i = 1; i < 20; i++)
            {
                for (int j = 1; j < 20; j++)
                {
                    object cellVal = ((Range)sheet.Cells[i, j]).Value;
                    if (null != cellVal && !cellVal.ToString().Equals(""))
                    {
                        object v = record.ContainsKey(cellVal.ToString()) ? record[cellVal.ToString()] : null;
                        if (v != null) sheet.Cells[i, j] = v;
                    }
                }
            }

            //插入二维码
            string tempDirectory = Utils.GetTemplateDirectory();
            string qrcode = record.ContainsKey("FCode") ? record["FCode"].ToString() : null;

            if(null != qrcode)
            {
                var range = FindAddress(sheet, "FQRCode");
                if(null != range)
                {
                    string qrcodeFilePath = QRCodeHelper.GenerateQRCode(qrcode, tempDirectory, QRCodeHelper.FILE_NAME);
                    InsertPicture(sheet, "FQRCode", qrcodeFilePath, QRCodePictuteWidth, QRCodePictuteHeight);  //插入二维码图片
                    range.Value = "";
                }
            }

            for (int i = 0; i < times; i++)
            {
                if ("Default".Equals(printer))
                    sheet.PrintOutEx();
                else
                    sheet.PrintOutEx(ActivePrinter: $"{printer}");
            }

            ReleaseProcess(sheet, book, excelApp);
        }

        public static void Print<T>(string file, T record, int times, string printer)
        {
            Application excelApp = new Application();
            excelApp.DisplayAlerts = false;
            excelApp.Visible = true;
            Workbook book = excelApp.Workbooks.Open(file);
            Worksheet sheet = book.Worksheets["Sheet1"];

            for (int i = 1; i < 20; i++)
            {
                for (int j = 1; j < 20; j++)
                {
                    object cellVal = ((Range)sheet.Cells[i, j]).Value;
                    if (null != cellVal && !cellVal.ToString().Equals(""))
                    {
                        object v = GetValue(cellVal.ToString(), record);
                        if (v != null) sheet.Cells[i, j] = v;
                    }
                }
            }

            for (int i = 0; i < times; i++)
            {
                if ("Default".Equals(printer))
                    sheet.PrintOutEx();
                else
                    sheet.PrintOutEx(ActivePrinter: $"{printer}");
            }

            ReleaseProcess(sheet, book, excelApp);
        }

        public static void Print<T>(string file, List<T> records, int times, string printer)
        {
            foreach (T record in records)
            {
                Print(file, record, times, printer);
            }
        }

        public static void PrintPreview<T>(string file, T record) {
            Application excelApp = new Application();
            excelApp.DisplayAlerts = false;
            excelApp.Visible = true;
            Workbook book = excelApp.Workbooks.Open(file);
            Worksheet sheet = book.Worksheets["Sheet1"];

            for (int i = 1; i < 20; i++) {
                for (int j = 1; j < 20; j++) {
                    object cellVal = ((Range)sheet.Cells[i, j]).Value;
                    if (null != cellVal && !cellVal.ToString().Equals("")) {
                        object v = GetValue(cellVal.ToString(), record);
                        if (v != null) sheet.Cells[i, j] = v;
                    }
                }
            }

            sheet.PrintPreview();

            ReleaseProcess(sheet, book, excelApp);
        }

        private static object GetValue<T>(string propertyName, T record) {
            foreach (PropertyInfo info in record.GetType().GetProperties()) {
                if (propertyName.Equals(info.Name))
                    return info.GetValue(record);
            }

            return null;
        }

        /// <summary>
        /// 将图片插入到指定的单元格位置，并设置图片的宽度和高度。
        /// 注意：图片必须是绝对物理路径
        /// </summary>
        /// <param name="RangeName">单元格名称，例如：B4</param>
        /// <param name="PicturePath">要插入图片的绝对路径。</param>
        /// <param name="PictuteWidth">插入后，图片在Excel中显示的宽度。</param>
        /// <param name="PictureHeight">插入后，图片在Excel中显示的高度。</param>
        private static void InsertPicture(Worksheet sheet, string RangeName, string PicturePath, float PictuteWidth, float PictureHeight)
        {
            var range = FindAddress(sheet, RangeName);
            range.Select();
            float PicLeft, PicTop;
            PicLeft = Convert.ToSingle(range.Left);
            PicTop = Convert.ToSingle(range.Top);
            //参数含义：
            //图片路径
            //是否链接到文件
            //图片插入时是否随文档一起保存
            //图片在文档中的坐标位置（单位：points）
            //图片显示的宽度和高度（单位：points）
            //参数详细信息参见：http://msdn2.microsoft.com/zh-cn/library/aa221765(office.11).aspx
            sheet.Shapes.AddPicture(PicturePath, Microsoft.Office.Core.MsoTriState.msoFalse,
              Microsoft.Office.Core.MsoTriState.msoTrue, PicLeft, PicTop, PictuteWidth, PictureHeight);
        }
    }
}
