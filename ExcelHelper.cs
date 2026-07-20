using Microsoft.Office.Interop.Excel;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using com.jiuhuan.plan.domain;
using System.Windows.Forms;
using Application = Microsoft.Office.Interop.Excel.Application;

namespace com.jiuhuan.plan.tools {

    public class ExcelHelper {

        ///  <summary>
        /// 导入Excel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>        
        public async static Task<List<T>> Import<T>() where T : Entity
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel 文件 (*.xlsx)|*.xlsx";
            openFileDialog.Title = "选择 Excel 文件";
            DialogResult result = openFileDialog.ShowDialog();

            if (result != DialogResult.OK)
            {
                return new List<T>();
            }

            string filePath = openFileDialog.FileName;
            Application excelApp = new Application();
            Workbook book = excelApp.Workbooks.Open(filePath);
            Worksheet sheet = book.ActiveSheet;

            // 获取列名（第二行）
            Range usedRange = sheet.UsedRange;
            int columnCount = usedRange.Columns.Count;

            List<T> entities = new List<T>();

            // 获取属性信息
            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties();

            // 显示进度窗口
            ProgressWindow progressWindow = new ProgressWindow();
            progressWindow.Show();

            await Task.Run(() =>
            {
                int rowCount = usedRange.Rows.Count;

                // 从第三行开始读取数据
                for (int row = 3; row <= rowCount; row++)
                {
                    T entity = Activator.CreateInstance<T>();

                    for (int col = 1; col <= columnCount; col++)
                    {
                        // 从第二行获取列名（属性名）
                        string columnName = sheet.Cells[2, col].Value?.ToString();
                        object cellValue = sheet.Cells[row, col].Value;

                        PropertyInfo property = Array.Find(properties, p => p.Name == columnName);

                        if (property != null && cellValue != null)
                        {
                            property.SetValue(entity, Convert.ChangeType(cellValue, property.PropertyType), null);
                        }
                    }

                    entities.Add(entity);

                    // 更新进度
                    int percentComplete = (int)((row - 2) / (float)(rowCount - 2) * 100); // 从第三行开始，所以总数是rowCount-2
                    progressWindow.UpdateProgress(percentComplete, $"正在导入数据... {row - 2}/{rowCount - 2}");
                }
            });

            // 关闭进度窗口
            progressWindow.Close();

            ReleaseProcess(sheet, book, excelApp);

            MessageBox.Show("导入完毕！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);

            return entities;
        }

        /// <summary>
        /// 导出Excel，将字典数据的 key 作为列名写入第一行
        /// </summary>
        /// <param name="data">字典数据列表</param>
        public async static void Export(List<Dictionary<string, object>> data)
        {
            if (data == null || data.Count == 0)
            {
                throw new ArgumentException("导出数据不能为空");
            }

            Application excelApp = new Application();
            Workbook book = excelApp.Workbooks.Add(Missing.Value);
            Worksheet sheet = (Worksheet)book.ActiveSheet;

            // 弹窗选择保存路径
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Excel 文件 (*.xlsx)|*.xlsx";
            saveFileDialog.Title = "保存 Excel 文件";
            DialogResult result = saveFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                // 获取所有列名（以第一条数据的 key 为准）
                List<string> columnNames = new List<string>(data[0].Keys);
                int columnCount = columnNames.Count;

                // 写入列名到第一行
                for (int col = 0; col < columnCount; col++)
                {
                    sheet.Cells[1, col + 1] = columnNames[col];
                }

                // 显示进度窗口
                ProgressWindow progressWindow = new ProgressWindow();
                progressWindow.Show();

                await Task.Run(() =>
                {
                    var total = data.Count;
                    var count = 0;

                    // 写入数据，从第二行开始
                    for (int row = 0; row < data.Count; row++)
                    {
                        count++;
                        Dictionary<string, object> record = data[row];
                        for (int col = 0; col < columnCount; col++)
                        {
                            string key = columnNames[col];
                            object value = record.ContainsKey(key) ? record[key] : null;
                            sheet.Cells[row + 2, col + 1] = value?.ToString();
                        }

                        // 更新进度
                        int percentComplete = (int)(count / (float)total * 100);
                        progressWindow.UpdateProgress(percentComplete, $"正在导出数据... {count}/{total}");
                    }
                });

                // 关闭进度窗口
                progressWindow.Close();

                // 设置自动筛选
                Range headerRange = sheet.Range[sheet.Cells[1, 1], sheet.Cells[1, columnCount]];
                headerRange.AutoFilter(1);

                string filePath = saveFileDialog.FileName;
                SaveFile(book, filePath);

                ReleaseProcess(sheet, book, excelApp);

                MessageBox.Show("导出完毕！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        ///  <summary>
        /// 导出Excel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        public async static void Export<T>(List<T> entities) where T : Entity
        {
            if (entities == null || entities.Count == 0)
            {
                throw new ArgumentException("导出数据不能为空");
            }

            Application excelApp = new Application();
            Workbook book = excelApp.Workbooks.Add(Missing.Value);
            Worksheet sheet = (Worksheet)book.ActiveSheet;
            Type type = typeof(T);

            // 弹窗选择保存路径
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Excel 文件 (*.xlsx)|*.xlsx";
            saveFileDialog.Title = "保存 Excel 文件";
            DialogResult result = saveFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                // 获取属性信息
                PropertyInfo[] properties = type.GetProperties();

                var n = 0;

                // 写入列标题（ColumnAttribute.title）
                for (int i = 0; i < properties.Length; i++)
                {
                    var prop = properties[i];

                    //if (Attribute.IsDefined(prop, typeof(IgnoreAttribute)))
                    //{
                    //    n++;
                    //    continue; // 跳过带 Ignore 标记的属性
                    //}

                    // 获取ColumnAttribute的Title作为列标题
                    var columnAttr = Attribute.GetCustomAttribute(prop, typeof(ColumnAttribute)) as ColumnAttribute;
                    string columnTitle = columnAttr != null ? (string.IsNullOrEmpty(columnAttr.Title) ? Utility.GetAttributeValueByField<T>("Field", "Name", prop.Name) as string : columnAttr.Title) : prop.Name;

                    sheet.Cells[1, i + 1 - n] = columnTitle;
                }

                n = 0;

                // 写入列名
                for (int i = 0; i < properties.Length; i++)
                {
                    var prop = properties[i];

                    //if (Attribute.IsDefined(prop, typeof(IgnoreAttribute)))
                    //{
                    //    n++;
                    //    continue; // 跳过带 Ignore 标记的属性
                    //}

                    sheet.Cells[2, i + 1 - n] = prop.Name;
                }

                // 显示进度窗口
                ProgressWindow progressWindow = new ProgressWindow();
                progressWindow.Show();

                await Task.Run(() => {

                    var total = entities.Count;
                    var count = 0;

                    // 写入数据
                    for (int row = 0; row < entities.Count; row++)
                    {
                        count++;
                        n = 0;
                        T entity = entities[row];
                        for (int col = 0; col < properties.Length; col++)
                        {
                            var prop = properties[col];

                            //if (Attribute.IsDefined(prop, typeof(IgnoreAttribute)))
                            //{
                            //    n++;
                            //    continue; // 跳过带 Ignore 标记的属性
                            //}

                            object value = prop.GetValue(entity, null);
                            sheet.Cells[row + 3, col + 1 - n] = value?.ToString();
                        }

                        // 更新进度
                        int percentComplete = (int)(count / (float)total * 100);
                        progressWindow.UpdateProgress(percentComplete, $"正在导出数据... {count}/{total}");
                    }
                });

                // 关闭进度窗口
                progressWindow.Close();

                // 设置自动筛选
                Range headerRange = sheet.Range[sheet.Cells[1, 1], sheet.Cells[1, properties.Length]];
                headerRange.AutoFilter(1);

                string filePath = saveFileDialog.FileName;
                SaveFile(book, filePath);

                ReleaseProcess(sheet, book, excelApp);

                MessageBox.Show("导出完毕！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public static void SaveFile(Workbook book, string OutputFilePath)
        {
            book.SaveAs(OutputFilePath, Missing.Value, Missing.Value,
              Missing.Value, Missing.Value, Missing.Value, XlSaveAsAccessMode.xlNoChange,
              Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value);
        }

        /// <summary>
        /// 根据文本查询位置
        /// </summary>
        /// <param name="file"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static Range FindAddress(string file, string text)
        {
            Application excelApp = new Application();
            excelApp.DisplayAlerts = false;
            excelApp.Visible = true;
            Workbook book = excelApp.Workbooks.Open(file);
            Worksheet sheet = book.Worksheets["Sheet1"];

            return FindAddress(sheet, text);
        }

        public static Range FindAddress(Worksheet sheet, string text)
        {
            var searchRange = sheet.UsedRange;
            return searchRange.Find(text, Type.Missing, XlFindLookIn.xlValues, XlLookAt.xlWhole, XlSearchOrder.xlByRows, XlSearchDirection.xlNext, false, false, false);
        }

        protected static void ReleaseProcess(Worksheet sheet, Workbook book, Application excelApp)
        {
            book.Close();

            System.Runtime.InteropServices.Marshal.ReleaseComObject((object)sheet);

            System.Runtime.InteropServices.Marshal.ReleaseComObject((object)book);

            excelApp.Quit();

            System.Runtime.InteropServices.Marshal.ReleaseComObject((object)excelApp);
            System.GC.Collect();
        }
    }
}
