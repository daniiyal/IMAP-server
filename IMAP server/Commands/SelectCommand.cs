using IMAP_server.DataBase.Entities;
using IMAP_server.Enums;

namespace IMAP_server.Commands
{
    public class SelectCommand : Command
    {
        public override async Task Execute(Client client, string commandNum, string request)
        {
            Client = client;
            if (Client.ClientState == ClientState.NOT_AUTHENTICATED)
            {
                await Client.SendMessageAsync(commandNum, Status.BAD, "Must Authenticate");
                return;
            }

            try
            {
                var boxName = request.Split(' ')[2];

                Client.SelectedBox = await Client.DbContext.GetBox(Client.ClientEntity.Name, boxName);

                var mails = Client.SelectedBox.Mails;

                var recentMail = mails.FindAll(m => m.MailFlag == MailFlag.RECENT).Count;

                await SendServiceResponseAsync(Client, $"{mails.Count} EXISTS");
                await SendServiceResponseAsync(Client, $"{recentMail} RECENT");
                await SendServiceResponseAsync(Client, $"[UIDVALIDITY {Client.SelectedBox.UidValidity}] UIDs valid");
                await SendServiceResponseAsync(Client, $"[UIDNEXT {Client.SelectedBox.NextMailUid}] Predicted next UID");
                await Client.SendMessageAsync(commandNum, Status.OK, "Select completed");

                Client.ClientState |= ClientState.SELECTED;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Client.SendMessageAsync(commandNum, Status.NO, "Select not completed");
            }

        }
    }
}
