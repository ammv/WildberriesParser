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
            string serverDirectory;

            Version version;

            Console.Clear();

            Console.WriteLine("Вас привествует интерфейс для загрузки обновлений!\n");

            while (true)
            {
                Console.WriteLine("Введите путь к папку с новой версией приложения: ");
                pathToDirectory = Console.ReadLine();

                if (!Directory.Exists(pathToDirectory))
                {
                    Console.WriteLine("Указан неверный или несуществующий путь!");
                }
                else
                {
                    break;
                }
            }
            while (true)
            {
                Console.WriteLine("Введите версию обновления в формате x.x.x.x: ");
                if (!Version.TryParse(Console.ReadLine(), out version))
                {
                    Console.WriteLine("Указана неверная версия!");
                }
                else
                {
                    break;
                }
            }
            string zipFileName = version.ToString() + ".zip";

            Console.WriteLine($"Укажите путь к папке на сервере, где будет лежать {zipFileName}");
            serverDirectory = Console.ReadLine();

            try
            {
                _sftpClient = new SftpClient(_host, _port, _username, _password);
                _sftpClient.Connect();

                Console.WriteLine("SFTP клиент подключен");

                try
                {
                    File.Delete($"{version}.zip");
                }
                catch { }

                ZipFile.CreateFromDirectory(pathToDirectory, zipFileName);

                Console.WriteLine($"Архив {zipFileName} создан");

                ZipArchive zipArchive = new ZipArchive(File.Open(zipFileName, FileMode.Open), ZipArchiveMode.Update);

                zipArchive.GetEntry("Updater.exe").Delete();

                zipArchive.Dispose();

                Console.WriteLine("Из архива удален файл Updater.exe");

                using (FileStream fs = new FileStream($"{version}.zip", FileMode.Open))
                {
                    //_sftpClient.ChangeDirectory(serverDirectory);
                    _sftpClient.UploadFile(fs, $"{serverDirectory}/{Path.GetFileName(zipFileName)}");
                    Console.WriteLine($"{zipFileName} загружен на сервер в папку {serverDirectory}!");
                    using (var versionFile = _sftpClient.OpenWrite($"{serverDirectory}/version.txt"))
                    {
                        byte[] versionAsBytes = Encoding.ASCII.GetBytes(version.ToString());
                        versionFile.Write(versionAsBytes, 0, versionAsBytes.Length);
                    }
                    Console.WriteLine($"Версия в файле version.txt обновлена на {version}");
                }

                File.Delete($"{version}.zip");

                Console.WriteLine($"Удаление {zipFileName} на этом компьютере!");

                _sftpClient.Disconnect();
                _sftpClient.Dispose();

                Console.WriteLine($"Отключение SFTP клиента");

                var temp = Console.ForegroundColor;

                Console.ForegroundColor = ConsoleColor.Green;

                Console.WriteLine($"Обновление {version} залито!");

                Console.ForegroundColor = temp;
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