using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace Updater
{
    public class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                while (Process.GetProcessesByName("WildberriesParser").Length > 0)
                {
                    Process[] myProcesses2 = Process.GetProcessesByName("WildberriesParser");
                    for (int i = 1; i < myProcesses2.Length; i++) { myProcesses2[i].Kill(); }

                    Thread.Sleep(300);
                }

                using (FileStream fs = new FileStream("update.zip", FileMode.Open))
                {
                    ZipArchive zip = new ZipArchive(fs);
                    zip.ExtractToDirectory(Directory.GetCurrentDirectory(), true);
                }

                Process.Start("WildberriesParser.exe");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Не удалось установить обновление\n" + ex.Message);
                Console.ReadLine();
            }
        }
    }
}