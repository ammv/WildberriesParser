using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Collector
{
    public class Server
    {
        private int _port;
        private IPEndPoint _ipEndPoint;
        private Socket _listener;
        public Worker CurrentWorker { get; set; }

        public Server(string address, int port = 8888)
        {
            _port = port;
            _ipEndPoint = new IPEndPoint(IPAddress.Parse(address), _port);

            _listener = new Socket(
                _ipEndPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            _listener.Bind(_ipEndPoint);
            _listener.Listen(1);
        }

        public Dictionary<string, string> HandleParameters(string data)
        {
            string[] parameters = data.Split(';');

            Dictionary<string, string> parametersDict = new Dictionary<string, string>();

            foreach (var p in parameters)
            {
                int index = p.IndexOf("=");
                string paramName = p.Substring(0, index);
                string paramValue = p.Substring(index + 1, p.Length);

                parametersDict.Add(paramName, paramValue);
            }

            return parametersDict;
        }

        public string HandleCommand(string data)
        {
            int index = data.IndexOf("]");
            return data.Substring(1, index);
        }

        public async Task StartListen()
        {
            var handler = await _listener.AcceptAsync();
            while (true)
            {
                // Receive message.
                var buffer = new byte[2048];
                var received = await handler.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                var response = Encoding.UTF8.GetString(buffer, 0, received);

                var command = HandleCommand(response);
                var parameters = HandleParameters(response);

                switch (command)
                {
                    case "START_TASK":
                        await SendAnswer(handler, "AcceptTask");
                        break;

                    default:
                        await SendAnswer(handler, "ERROR");
                        break;
                }
            }
        }

        private static async Task SendAnswer(Socket handler, string response)
        {
            var echoBytes = Encoding.UTF8.GetBytes(response);
            await handler.SendAsync(new ArraySegment<byte>(echoBytes), 0);
        }
    }
}