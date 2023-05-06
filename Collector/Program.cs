using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Collector
{
    internal class Program
    {
        private static void SetAutostartup()
        {
            RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (!IsStartupItem())
                // Add the value in the registry so that the application runs at startup
                rkApp.SetValue(System.AppDomain.CurrentDomain.FriendlyName, System.Reflection.Assembly.GetEntryAssembly().Location);
        }

        private static bool IsStartupItem()
        {
            // The path to the key where Windows looks for startup applications
            RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (rkApp.GetValue(System.AppDomain.CurrentDomain.FriendlyName) == null)
                // The value doesn't exist, the application is not set to run at startup
                return false;
            else
                // The value exists, the application is set to run at startup
                return true;
        }

        private static async Task Main(string[] args)
        {
            if (!IsStartupItem())
            {
                SetAutostartup();
            }

            Server server;
            Worker worker = null;

            Thread serverThread = new Thread(async () =>
           {
               worker = new Worker();
               server = new Server("127.0.0.1");
               server.CurrentWorker = worker; ;

               await server.StartListen();
           });
            Thread workerThread = new Thread(() =>
           {
               worker.Start();
           });

            serverThread.Start();

            while (true)
            {
            }
        }
    }
}