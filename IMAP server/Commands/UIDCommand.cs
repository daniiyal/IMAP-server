using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMAP_server.Enums;

namespace IMAP_server.Commands
{
    public class UIDCommand : Command
    {
        public override async Task Execute(Client client, string commandNum, string request)
        {
            try
            {
                Client = client;

                var secondArg = request.Split(' ')[2];
                var thirdArg = request.Split(' ')[3];

                var mails = await Client.DbContext.GetMails(Client.ClientEntity.Name, Client.SelectedBox.Name);
                if (secondArg == "SEARCH")
                {
                    switch (thirdArg)
                    {
                        case "ALL":
                            foreach (var mailEntity in mails)
                            {
                                await SendServiceResponseAsync(Client, $"{mailEntity.Uid} SEARCH");
                            }
                            break;
                        case "UNSEEN":
                            foreach (var mailEntity in mails.Where(m => m.MailFlag == MailFlag.UNSEEN))
                            {
                                await SendServiceResponseAsync(Client, $"{mailEntity.Uid} SEARCH");
                            }

                            break;
                        case "FLAGGED":
                            foreach (var mailEntity in mails.Where(m => m.MailFlag == MailFlag.FLAGGED))
                            {
                                await SendServiceResponseAsync(Client, $"{mailEntity.Uid} SEARCH");
                            }
                            break;
                    }
                    await Client.SendMessageAsync(commandNum, Status.OK, "SEARCH Completed");
                }

                if (secondArg == "FETCH")
                {
                    FetchCommand fetchCommand = new FetchCommand();
                    var newRequest = $"{commandNum} {request.Split(' ')[2]} {request.Split(' ')[3]} {request.Split(' ')[4]}";
                    await fetchCommand.Execute(Client, commandNum, newRequest);
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
