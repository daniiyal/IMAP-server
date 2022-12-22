using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using IMAP_server.DataBase;

namespace IMAP_server
{
    public class Server
    {
        public DBContext Db { get; }
        private TcpListener Listener { get; }
        private TcpClient TcpClient { get; set; }

        public Server(string ip, int port)
        {
            Db = new DBContext();
            Listener = new TcpListener(IPAddress.Parse(ip), port);
        }

        public void StartServer()
        {
            try
            {
                Listener.Start();
                Console.WriteLine("Сервер запустился. Ожидание подключений...");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Не удалось запустить сервер");
                Console.WriteLine(ex.Message);
            }
        }

        public void StopServer()
        {
            Listener.Stop();
            Console.WriteLine("Сервер остановлен.");
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            if (host.AddressList[3].AddressFamily == AddressFamily.InterNetwork)
            {
                return host.AddressList[3].ToString();

            }

            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        public async Task ConnectClient()
        
        {
            try
            {
                while (true)
                {
                    TcpClient = await Listener.AcceptTcpClientAsync();
                    Console.WriteLine($"Подключился клиент - {TcpClient.Client.RemoteEndPoint}");

                    Client client = new Client(TcpClient);

                    Task.Run(client.HandleRequestsAsync);

                }
            }
            catch (Exception e)
            { 
                Console.WriteLine(e.Message);
            }
            
        }


    }
}
