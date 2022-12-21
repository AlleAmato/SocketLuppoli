using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;

namespace TcpClient
{

    internal class CustomData
    {
        public string text { get; set; }
        public int number { get; set; }

    }

    internal class NetworkPacket
    {
        public List<byte> header;
        public List<byte> body;
    }


    internal class Program
    {
        static NetworkPacket Encode(CustomData data)
        {
            var serializer = new XmlSerializer(typeof(CustomData));
            var stringBuilder = new StringBuilder();
            var stringWriter = new StringWriter(stringBuilder);
            serializer.Serialize(stringWriter, data);

            var bodyBytes = Encoding.ASCII.GetBytes(stringBuilder.ToString());
            var headerBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(bodyBytes.Length));

            return new NetworkPacket
            { header = new List<byte>(headerBytes), body = new List<byte>(bodyBytes) };

        }

        static CustomData Decode(byte[] body)
        {
            var deserializer = new XmlSerializer(typeof(CustomData));
            var stringReader = new StringReader(Encoding.ASCII.GetString(body));
            return (CustomData) deserializer.Deserialize(stringReader);
        }

        static async Task Main(string[] args)
        {
            //Creazione della socket
            System.Net.Sockets.TcpClient socket = new System.Net.Sockets.TcpClient();

            //Collegamento al server(ip address e porta)
            await socket.ConnectAsync(IPAddress.Loopback, 8888);

            //Generazione dello stream di comunicazione(solo per TCP)
            NetworkStream networkStream = socket.GetStream();

            //Invio e ricezione dei dati
            Console.WriteLine("Inserire il messaggio:");
            string msg = Console.ReadLine();

            CustomData d = new CustomData
            {
                number = 6969,
                text = msg
            };

            //byte[] msgByte = Encoding.ASCII.GetBytes(msg);
            NetworkPacket p = Encode(d);
            byte[] msgByte = p.header.Concat(p.body).ToArray();

            await networkStream.WriteAsync(msgByte, 0, msgByte.Length);

            byte[] headerBytes = new byte[4];
            await networkStream.ReadAsync(headerBytes, 0, headerBytes.Length);
            int header = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(headerBytes, 0));
            byte[] bodyBytes = new byte[header];

            Console.WriteLine($"Il server ha risposto con: {bodyBytes}");

            //Chiusura socket
            networkStream.Close();
            socket.Dispose();

            Console.ReadLine();
        }
    }
}
