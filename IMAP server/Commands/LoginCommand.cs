using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMAP_server.DataBase.Entities;
using IMAP_server.Enums;

namespace IMAP_server.Commands
{
    public class LoginCommand : Command
    {

        public override async Task Execute(Client client, string commandNum, string request)
        {
            Client = client;
            if (Client.ClientState == ClientState.AUTHENTICATED)
            {
                await Client.SendMessageAsync(commandNum, Status.BAD, "Authenticated Already");
                return;
            }

            try
            {
                var username = request.Split(' ')[2];
                var password = request.Split(' ')[3];

                var clientEntity = new ClientEntity(username, password);

                if (!await Client.DbContext.Authenticate(clientEntity))
                {
                    await Client.SendMessageAsync(commandNum, Status.NO, "Login failed");
                    return;
                }

                await Client.SendMessageAsync(commandNum, Status.OK, "User logged in");
                Client.ClientEntity = clientEntity;
                //await Client.AddMail(Client.ClientEntity.ClientBox[0]);
                Client.ClientState = ClientState.AUTHENTICATED;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await Client.SendMessageAsync(commandNum, Status.BAD, "Something went wrong!");
            }
        }
    }
}
