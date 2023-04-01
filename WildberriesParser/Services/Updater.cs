using Renci.SshNet;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace WildberriesParser.Services
{
    public static class Updater
    {
        private static readonly SftpClient _sftpClient;

        public static bool CheckNewVersion()
        {
            return GetNewVersion() > GetCurrentVersion();
        }

        public static Version GetNewVersion()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                _sftpClient.Connect();
                _sftpClient.DownloadFile("WBUpdates/version.txt", ms);
                _sftpClient.Disconnect();
                return Version.Parse(Encoding.ASCII.GetString(ms.ToArray()));
            }
        }

        static Updater()
        {
            string username = "root";
            string host = "176.113.82.114";
            string password = "px50c20UHs";
            int port = 1234;

            _sftpClient = new SftpClient(host, port, username, password);
        }

        public static Version GetCurrentVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }
    }
}