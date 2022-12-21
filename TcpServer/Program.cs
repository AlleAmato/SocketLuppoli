using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NLog;
using System.Xml.Serialization;
using System.IO;

namespace TcpServer
{
    internal class CustomData//oggetto da passare
    {
        public string text { get; set; }
        public int number { get; set; }
    }
    internal class NetworkPacket //classe di supporto per ricreare il pacchetto,
                                 //va bene per qualsiasi oggetto da trasferire traformato in array o lista di byte
    {
        public List<byte> header;
        public List<byte> data;
    }
    internal class Program
    {
        static NetworkPacket Encode (CustomData data)
        {
            var serializer = new XmlSerializer(typeof(CustomData));
            var stringBuilder = new StringBuilder();
            var stringWriter = new StringWriter(stringBuilder);
            serializer.Serialize(stringWriter, data);
            
            var bodyBytes= Encoding.ASCII.GetBytes(stringBuilder.ToString());
            var headerBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder( bodyBytes.Length));
            return new NetworkPacket
            {
                header = new List<byte>(headerBytes),
                data = new List<byte>(bodyBytes)
            };
        }
        static async Task Main(string[] args)
        {
            //Creazione della socket
            //Binding della socket(ip address e porta)
            var serverAddress = new IPEndPoint(IPAddress.Loopback, 8888);
            TcpListener socket = new TcpListener(serverAddress);

            socket.Start();

            //Accettazione della connessione(solo per TCP)
            while (true)
            {
                using (TcpClient clientSocket = await socket.AcceptTcpClientAsync())
                {
                    //Generazione dello stream di comunicazione(solo per TCP)
                    using (NetworkStream networkStream = clientSocket.GetStream())
                    {
                        while (true)
                        {
                            try
                            {
                                //Invio e ricezione dei dati
                                byte[] inMsgByte = new byte[100];
                                await networkStream.ReadAsync(inMsgByte, 0, inMsgByte.Length);
                                string inMsg = Encoding.ASCII.GetString(inMsgByte);

                                string outMsg = $"Echo di: {inMsg}";
                                byte[] outMsgByte = Encoding.ASCII.GetBytes(outMsg);
                                await networkStream.WriteAsync(outMsgByte, 0, outMsgByte.Length);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Connessione client chiusa");
                                Logger logger = LogManager.GetCurrentClassLogger();
                                //logger.Debug(), molto utile perchè posso passargli del codice che viene eseguito solo ce c'è un eccezione ecc
                                logger.Info("Connessione client chiusa");

                                break;
                            }
                        }
                    }
                }
            }

            //Chiusura socket
        }
    }
}
