using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMAP_server.Enums;

namespace IMAP_server.Commands
{
    public class NamespaceCommand : Command
    {
        public override async Task Execute(Client client, string commandNum, string request)
        {
            Client = client;
            try
            {
                await SendServiceResponseAsync(Client, "* NAMESPACE ((\"\" \"/\")) NIL NIL");
                await Client.SendMessageAsync(commandNum, Status.OK, "Logout Completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
