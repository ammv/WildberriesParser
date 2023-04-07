using Renci.SshNet;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace WildberriesParser.Services
{
    public class Updater
    {
        private readonly SftpClient _sftpClient;
        private readonly string _username = "root";
        private readonly string _host = "176.113.82.114";
        private readonly string _password = "px50c20UHs";
        private readonly int _port = 1234;
        private Version _newVersion;

        /// <summary>
        /// Проверяет наличие новой версии
        /// </summary>
        /// <returns></returns>
        public bool CheckNewVersion()
        {
            return GetNewVersion() > GetCurrentVersion();
        }

        /// <summary>
        /// Получает новую версию
        /// </summary>
        /// <returns></returns>
        public Version GetNewVersion()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                _sftpClient.Connect();
                _sftpClient.DownloadFile("WBParser/version.txt", ms);
                _sftpClient.Disconnect();
                _newVersion = Version.Parse(Encoding.ASCII.GetString(ms.ToArray()));
                return _newVersion;
            }
        }

        public bool CheckConnection()
        {
            try
            {
                _sftpClient.Connect();
                _sftpClient.Disconnect();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Проверяет наличие файла содержащего обновление, например MyProject/1.0.5.0.zip
        /// </summary>
        /// <returns></returns>
        public bool HasUpdateZip()
        {
            _sftpClient.Connect();
            bool result = _sftpClient.Exists($"WBParser/{_newVersion}.zip");
            _sftpClient.Disconnect();
            return result;
        }

        /// <summary>
        /// Скачивает обновление и запускет Updater.exe выполняющий распакову обновления и запуск приложения
        /// </summary>
        public void Update()
        {
            using (var fileStream = new FileStream("update.zip", FileMode.Create))
            {
                _sftpClient.Connect();
                _sftpClient.DownloadFile($"WBParser/{_newVersion}.zip", fileStream);
                _sftpClient.Disconnect();
            }

            Process.Start("Updater.exe", "WildberriesParser.exe");
        }

        public Updater()
        {
            _sftpClient = new SftpClient(_host, _port, _username, _password);
        }

        /// <summary>
        /// Возвращает текущую версию
        /// </summary>
        /// <returns></returns>
        public Version GetCurrentVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }
    }
}