using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace Updater
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            while (Process.GetProcessesByName("WildberriesParser").Length > 0)
            {
                Process[] myProcesses2 = Process.GetProcessesByName("WildberriesParser");
                for (int i = 1; i < myProcesses2.Length; i++) { myProcesses2[i].Kill(); }

                Thread.Sleep(300);
            }

            using (FileStream fs = new FileStream("update.zip", FileMode.Open))
            {
                ZipArchive zipArchive = new ZipArchive(fs);
                zipArchive.ExtractToDirectory(".", true);
            }

            Process.Start("WildberriesParser.exe");
        }
    }
}