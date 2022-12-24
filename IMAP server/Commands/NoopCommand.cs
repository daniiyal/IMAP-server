using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMAP_server.Enums;

namespace IMAP_server.Commands
{
    public class NoopCommand : Command
    {
        public override async Task Execute(Client client, string commandNum, string request)
        {
            Client = client;
            try
            {
                await Client.SendMessageAsync(commandNum, Status.OK, "NOOP completed");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
