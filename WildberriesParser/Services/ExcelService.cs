using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using OfficeOpenXml;

namespace WildberriesParser.Services
{
    internal class ExcelService
    {
        public void Export(List<string> headers, )
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "All files (*.*) | *.* | Excel file (*.xlsx)|*.xlsx |*.xlsm | *.csv | *.csv | *.xls";
            dialog.Title = "Выбор файла для экспорта";
            if (dialog.ShowDialog() == true)
            {
                ExcelPackage excelPackage = new ExcelPackage(dialog.FileName);
                var worksheet = excelPackage.Workbook.Worksheets[0];

                f
            }
            else
            {
            }
        }
    }
}