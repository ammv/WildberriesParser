using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using OfficeOpenXml;

namespace WildberriesParser.Services
{
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

        public void Export(Dictionary<string, List<object>> data, string path, string sheetName = "Лист 1")
        {
            {
                using (FileStream fs = File.Create(path))
                {
                    ExcelPackage excelPackage = new ExcelPackage(fs);
                    excelPackage.Workbook.Worksheets.Add(sheetName);
                    var worksheet = excelPackage.Workbook.Worksheets[0];

                    int i = 1, j = 1;

                    foreach (var key in data.Keys)
                    {
                        worksheet.Cells[1, i].Value = key;
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                        foreach (var value in data[key])
                        {
                            worksheet.Cells[j + 1, i].Value = value;
                            j++;
                        }
                        j = 1;
                        i++;
                    }

                    worksheet.Columns.AutoFit();
                    worksheet.Cells[1, 1, 1, data.Count].AutoFilter = true;

                    excelPackage.Save();
                }
            }
        }
    }
}