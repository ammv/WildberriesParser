using Renci.SshNet;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace UpdaterCLI
{
    internal class Program
    {
        private static string _username = "root";
        private static string _host = "176.113.82.114";
        private static string _password = "px50c20UHs";
        private static int _port = 1234;
        private static SftpClient _sftpClient;

        private static void Main(string[] args)
        {
            string pathToDirectory;
            Version version;

            Console.Clear();

            Console.WriteLine("Вас привествует интерфейс для загрузки обновлений!\n");

            while (true)
            {
                Console.WriteLine("Введите путь к папке: ");
                pathToDirectory = Console.ReadLine();

                if (!Directory.Exists(pathToDirectory))
                {
                    Console.WriteLine("Указан неверный путь!");
                }
                break;
            }
            while (true)
            {
                Console.WriteLine("Введите версию обновления в формате x.x.x.x: ");
                if (!Version.TryParse(Console.ReadLine(), out version))
                {
                    Console.WriteLine("Указана неверная версия!");
                }
                break;
            }

            try
            {
                _sftpClient = new SftpClient(_host, _port, _username, _password);
                _sftpClient.Connect();

                using (ZipArchive archive = ZipFile.Open($"{version}.zip", ZipArchiveMode.Create))
                {
                    string[] files = Directory.GetFiles(pathToDirectory);
                    foreach (string file in files)
                    {
                        if (!file.Contains("Update.exe"))
                        {
                            archive.CreateEntryFromFile(file, Path.GetFileName(file));
                        }
                    }
                }

                using (FileStream fs = File.Open($"{version}.zip", FileMode.Open))
                {
                    _sftpClient.UploadFile(fs, "WBParser");
                    using (var versionFile = _sftpClient.OpenWrite(@"WBParser/version.txt"))
                    {
                        byte[] versionAsBytes = Encoding.ASCII.GetBytes(version.ToString());
                        versionFile.Write(versionAsBytes, 0, versionAsBytes.Length);
                    }
                }

                _sftpClient.Disconnect();
                _sftpClient.Dispose();

                Console.WriteLine($"Обновление {version} залито!");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("====Произошла ошибка!====");
                Console.WriteLine(ex.Message);
                Console.WriteLine();
                Console.WriteLine(ex.StackTrace);
                Console.ReadLine();
            }
        }
    }
}