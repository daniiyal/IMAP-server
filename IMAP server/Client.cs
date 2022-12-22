using IMAP_server.DataBase;
using IMAP_server.DataBase.Entities;
using IMAP_server.Enums;
using System.Net.Sockets;
using System.Text;
using IMAP_server.Commands;

namespace IMAP_server
{
    public class Client
    {
        public TcpClient TcpClient { get; }
        public DBContext DbContext { get; }
        public ClientEntity ClientEntity { get; set; }
        public ClientBoxEntity SelectedBox { get; set; }
        public ClientState ClientState { get; set; }

        private string oldMessagePart = "";

        private Dictionary<string, ICommand> commands;

        public Client(TcpClient tcpClient)
        {
            this.TcpClient = tcpClient;
            ClientState = ClientState.NOT_AUTHENTICATED;
            DbContext = new DBContext();

            commands = new Dictionary<string, ICommand>
            {
                {"LOGIN", new LoginCommand()},
                {"LIST", new ListCommand()},
                {"SELECT", new SelectCommand()},
                {"CLOSE", new CloseCommand()},
                {"LOGOUT", new LogoutCommand()},
                {"NAMESPACE", new NamespaceCommand()},
                {"CAPABILITY", new CapabilityCommand()},
            };
        }


        public async Task AddMail(ClientBoxEntity box)
        {
            var mails = new List<MailEntity>
            {
                new (ClientEntity.ClientBox[0].NextMailUid, "es@we.es", new List<string> {"us@us.com"},
                    new List<string>(), "Hola", DateTime.Now),
                new (ClientEntity.ClientBox[0].NextMailUid, "en@en.ru", new List<string> {"us@us.com"},
                    new List<string>(), "Hello ", DateTime.Now)

            };


            foreach (var mailEntity in mails)
            {
                await box.AddMail(ClientEntity.Name, DbContext, mailEntity);
            }

        }

        public async Task HandleRequestsAsync()
        {
            try
            {
                await SendMessageAsync("*", Status.OK, "IMAP server ready");

                while (true)
                {

                    var requests = await ReadMessageAsync();

                    foreach (var request in requests)
                    {
                        if (request.Split(' ').Length > 1)
                        {
                            var commandNum = request.Split(' ')[0];

                            if (!commands.ContainsKey(request.Split()[1]))
                            {
                                await SendMessageAsync(commandNum, Status.BAD, "Unknown Command");
                            }

                            await commands[request.Split()[1]].Execute(this, commandNum, request);
                        }
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public async Task SendMessageAsync(string command, Status status, string message)
        {
            try
            {
                var request = Encoding.UTF8.GetBytes($"{command} {status.Value} {message}\r\n");

                await using (var stream = new NetworkStream(TcpClient.Client))
                {
                    await stream.WriteAsync(request);
                    await stream.FlushAsync();
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }




        public async Task<List<string>> ReadMessageAsync()
        {
            List<string> result = new List<string>();

            try
            {
                var data = new byte[1024];
                int bytes;
                string response = null;
                await using var stream = new NetworkStream(TcpClient.Client);
                do
                {
                    bytes = await stream.ReadAsync(data);
                    response += Encoding.UTF8.GetString(data, 0, bytes);
                } while (data[^1] != 0 && response.Length > 0);


                result = response.Split("\r\n").ToList();


                if (oldMessagePart.Length > 0)
                {
                    result[0] = oldMessagePart + response.Split("\r\n")[0];
                    oldMessagePart = "";
                }

                if (result.Last().SkipLast(Math.Max(0, result.Last().Length - 2)) != "\r\n")
                {
                    oldMessagePart += result.Last();
                    result.Remove(result.Last());
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await SendMessageAsync("*", Status.BAD, "Oops...Couldn't handle request");
            }

            return result;
        }
    }


}
