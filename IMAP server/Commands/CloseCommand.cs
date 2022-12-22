using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMAP_server.DataBase.Entities;
using IMAP_server.Enums;

namespace IMAP_server.Commands
{
    public class CloseCommand : Command
    {
        public override async Task Execute(Client client, string commandNum, string request)
        {
            Client = client;
            if (Client.ClientState == ClientState.NOT_AUTHENTICATED)
            {
                await Client.SendMessageAsync(commandNum, Status.BAD, "Must Choose Box");
                return;
            }
            try
            {
                foreach (var mail in Client.SelectedBox.Mails)
                {
                    if (mail.MailFlag == MailFlag.DELETED)
                    {
                        await Client.DbContext.DeleteMail(Client.ClientEntity.Name, Client.SelectedBox.Name, mail);
                    }
                }

                Client.ClientState = ClientState.AUTHENTICATED;
                await Client.SendMessageAsync(commandNum, Status.OK, "Close completed");
            }
            catch (Exception ex)
            {
                await Client.SendMessageAsync(commandNum, Status.NO, "Close not completed");
                Console.WriteLine(ex.Message);

            }
        }
    }
}
