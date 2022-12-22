using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMAP_server.Enums;

namespace IMAP_server.Commands
{
    public class LogoutCommand : Command
    {

        public override async Task Execute(Client client, string commandNum, string request)
        {
            Client = client;
            try
            {
                Console.WriteLine($"Клиент {Client.TcpClient.Client.RemoteEndPoint} отключился");
                await SendServiceResponseAsync(Client, "* BYE SUETA... IMAP server logging out");
                await Client.SendMessageAsync(commandNum, Status.OK, "Logout Completed");
                Client.ClientState = ClientState.LOGOUT;
                Client.TcpClient.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    }
}
