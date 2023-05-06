using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using DataLayer;

namespace WildberriesParser.Services
{
    public class CollectorService
    {
        private static CollectorClient _client;

        public static void InitClient()
        {
            _client = new CollectorClient("127.0.0.1");
        }

        public static string SerializeObject<T>(T toSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());

            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, toSerialize);
                return textWriter.ToString();
            }
        }

        public async Task<bool> StartTask(CollectorTask task)
        {
            string answer = await _client.SendAndGetMessage("[START_TASK]" + SerializeObject(task));
            return answer == "AcceptTask";
        }
    }

    public class CollectorClient
    {
        private int _port;
        private IPEndPoint _ipEndPoint;
        private Socket _client;

        public CollectorClient(string address, int port = 8888)
        {
            _port = port;
            _ipEndPoint = new IPEndPoint(IPAddress.Parse(address), _port);

            _client = new Socket(
                _ipEndPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            ///_client.Connect(_ipEndPoint);
        }

        public async Task<string> SendAndGetMessage(string message)
        {
            // Send message
            var messageBytes = Encoding.UTF8.GetBytes(message);
            await _client.SendAsync(new ArraySegment<byte>(messageBytes), SocketFlags.None);

            // Get message
            var buffer = new byte[2048];
            var received = await _client.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
            var response = Encoding.UTF8.GetString(buffer, 0, received);
            return response;
        }

        public async Task SendMessage(string message)
        {
            // Send message
            await _client.ConnectAsync(_ipEndPoint);
            var messageBytes = Encoding.UTF8.GetBytes(message);
        }
    }
}