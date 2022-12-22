using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using IMAP_server.Enums;

namespace IMAP_server.Commands
{
    public abstract class Command : ICommand
    {
        public Client Client { get; set; }
        public abstract Task Execute(Client client, string commandNum, string request);

        protected async Task SendServiceResponseAsync(Client client, string response)
        {
            try
            {
                byte[] resp;
                await using (var stream = new NetworkStream(client.TcpClient.Client))
                {
                    resp = Encoding.UTF8.GetBytes($"* {response}\r\n");
                    await stream.WriteAsync(resp);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}
