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
            if (args.Length == 0)
            {
                Console.WriteLine("Вы указали путь к exe файлу для запуска после распаковки");
            }
            else
            {
                Extract(args);
            }
        }

        private static void Extract(string[] args)
        {
            string programName = args[0].Replace(".exe", "");
            int attempts = 0;
            while (attempts != 5)
            {
                try
                {
                    while (Process.GetProcessesByName(programName).Length > 0)
                    {
                        Process[] myProcesses = Process.GetProcessesByName(programName);
                        for (int i = 1; i < myProcesses.Length; i++) { myProcesses[i].Kill(); }

                        Thread.Sleep(300);
                    }
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Попытка: {attempts}. Не удалось завершить процессы\n" + ex.Message);
                    Console.ReadLine();
                }

                attempts++;
            }
            try
            {
                using (FileStream fs = new FileStream("update.zip", FileMode.Open))
                {
                    ZipArchive zip = new ZipArchive(fs);
                    zip.ExtractToDirectory(Directory.GetCurrentDirectory(), true);
                }

                Process.Start(programName + ".exe");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Не удалось установить обновление\n" + ex.Message);
                Console.ReadLine();
            }
        }
    }
}