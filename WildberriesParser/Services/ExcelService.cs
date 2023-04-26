using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using OfficeOpenXml;

namespace WildberriesParser.Services
{
    public enum ExcelCellFormatType
    {
        Text,
        Hyperlink
    }

    public class ExcelColumn
    {
        public ExcelColumn(string name, List<object> values, ExcelCellFormatType formatType = ExcelCellFormatType.Text)
        {
            Name = name;
            Values = values;
            CellFormatType = formatType;
        }

        public static List<ExcelColumn> FromDictionary(Dictionary<string, List<object>> data)
        {
            List<ExcelColumn> columns = new List<ExcelColumn>();

            foreach (var key in data.Keys)
            {
                columns.Add(new ExcelColumn(key, data[key]));
            }

            return columns;
        }

        public string Name { get; set; }
        public List<object> Values { get; set; }
        public ExcelCellFormatType CellFormatType { get; set; }
    }

    public class ExcelService
    {
        public string ShowChoiceFilePathDialog()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Excel files (*.xls*, *.csv)|*.xls*;*.csv|All files(*.*)|*.*";
            dialog.Title = "Выбор файла для экспорта";
            if (dialog.ShowDialog() == true)
            {
                return dialog.FileName;
            }
            return null;
        }

        public string ShowSaveAsFileDialog()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Excel files (*.xls*, *.csv)|*.xls*;*.csv|All files(*.*)|*.*";
            dialog.Title = "Выберите место для сохранения файла";
            dialog.OverwritePrompt = true;
            dialog.DefaultExt = "xlsx";
            if (dialog.ShowDialog() == true)
            {
                return dialog.FileName;
            }
            return null;
        }

        public void WriteTextColumn(ExcelWorksheet worksheet, ExcelColumn column, int i, int j)
        {
            foreach (var value in column.Values)
            {
                worksheet.Cells[j + 1, i].Value = value;
                j++;
            }
        }

        public void WriteHyperlinkColumn(ExcelWorksheet worksheet, ExcelColumn column, int i, int j)
        {
            foreach (var value in column.Values)
            {
                ExcelRange cell = worksheet.Cells[j + 1, i];
                cell.Value = value;
                cell.Style.Font.UnderLine = true;
                cell.Style.Font.Color.SetColor(Color.Blue);
                try
                {
                    cell.Hyperlink = new Uri((string)value);
                }
                catch { }
                j++;
            }
        }

        public void Export(List<ExcelColumn> columns, string path, string sheetName = "Лист 1")
        {
            {
                using (FileStream fs = File.Create(path))
                {
                    ExcelPackage excelPackage = new ExcelPackage(fs);
                    excelPackage.Workbook.Worksheets.Add(sheetName);
                    var worksheet = excelPackage.Workbook.Worksheets[0];

                    int i = 1, j = 1;

                    foreach (var column in columns)
                    {
                        worksheet.Cells[1, i].Value = column.Name;
                        worksheet.Cells[1, i].Style.Font.Bold = true;

                        switch (column.CellFormatType)
                        {
                            case ExcelCellFormatType.Text:
                                WriteTextColumn(worksheet, column, i, j);
                                break;

                            case ExcelCellFormatType.Hyperlink:
                                WriteHyperlinkColumn(worksheet, column, i, j);
                                break;
                        }

                        j = 1;
                        i++;
                    }

                    worksheet.Columns.AutoFit();
                    worksheet.Cells[1, 1, 1, columns.Count].AutoFilter = true;

                    excelPackage.Save();
                }
            }
        }
    }
}