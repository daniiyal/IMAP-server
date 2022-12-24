using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMAP_server.Enums;

namespace IMAP_server.Commands
{
    public class IdleCommand : Command
    {
        public override async Task Execute(Client client, string commandNum, string request)
        {
            try
            {
                Client = client;

                var done = await Client.ReadMessageAsync();
                foreach (var d in done)
                {
                    if (d == "DONE")
                    {
                        await Client.SendMessageAsync(commandNum, Status.OK, "IDLE terminated");
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        

        }
    }
}
