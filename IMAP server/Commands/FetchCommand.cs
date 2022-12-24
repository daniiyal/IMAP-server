using IMAP_server.DataBase.Entities;
using IMAP_server.Enums;

namespace IMAP_server.Commands
{
    public class FetchCommand : Command
    {
        public override async Task Execute(Client client, string commandNum, string request)
        {
            try
            {
                Client = client;

                var arg = request.Split(' ')[2];

                var mails = await Client.DbContext.GetMails(Client.ClientEntity.Name, Client.SelectedBox.Name);


                if (arg.Contains(':'))
                {
                    var mailBounds = arg.Split(':');

                    var partialMails = GetPartialMails(mailBounds, mails);

                    foreach (var mailEntity in partialMails)
                    {
                        await SendServiceResponseAsync(Client, $"{mailEntity.Uid} FETCH (FLAGS (\\{mailEntity.MailFlag}) " +
                                                               $"RFC822.SIZE {mailEntity.Body.Length} UID {mailEntity.Uid}");
                    }
                }

                if (isNumber(arg))
                {
                    var exactMail = mails.FirstOrDefault(mailEntity => mailEntity.Uid == Convert.ToInt32(arg));
                    if (exactMail != null)
                    {
                        if (request.Split(' ').Length > 3)
                        {
                            var part = request.Split(' ')[3];

                            switch (part)
                            {
                                case "BODY[]":
                                    await SendServiceResponseAsync(Client, exactMail.ToString());
                                    break;
                                case "ALL":
                                    await SendServiceResponseAsync(Client, $"{exactMail.Uid} FETCH (FLAGS (\\{exactMail.MailFlag}) " +
                                                                           $"RFC822.SIZE {exactMail.Body.Length} UID {exactMail.Uid}");
                                    break;
                            }
                        }
                    }
                    else
                    {
                        await Client.SendMessageAsync(commandNum, Status.NO, "FETCH is not done");
                    }

                }

                await Client.SendMessageAsync(commandNum, Status.OK, "FETCH done");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        private List<MailEntity> GetPartialMails(string[] bounds, List<MailEntity> mails)
        {
            var partialMails = new List<MailEntity>();
            if (mails.Last().Uid < Convert.ToInt32(bounds[0]))
            {
                for (int i = Convert.ToInt32(bounds[0]); i < Convert.ToInt32(bounds[1]); i++)
                {
                    partialMails.AddRange(mails.Where(mailEntity => mailEntity.Uid == i));
                }
            }

            return partialMails;
        }

        private bool isNumber(string arg)
        {
            return int.TryParse(arg, out _);
        }
    }
}
