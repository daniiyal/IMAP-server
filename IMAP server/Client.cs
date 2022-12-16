using IMAP_server.DataBase;
using IMAP_server.DataBase.Entities;
using IMAP_server.Enums;
using System.Net.Sockets;
using System.Text;
using System.Xml.Linq;

namespace IMAP_server
{
    public class Client
    {
        private TcpClient client { get; }
        private DBContext dbContext { get; }
        private ClientEntity ClientEntity { get; set; }
        private ClientBoxEntity SelectedBox { get; set; }
        private ClientState ClientState { get; set; }

        private string oldMessagePart = "";

        public Client(TcpClient client)
        {
            this.client = client;
            ClientState = ClientState.NOT_AUTHENTICATED;
            dbContext = new DBContext();
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

                            switch (request.Split(' ')[1])
                            {
                                case "LOGIN":
                                    await Authenticate(commandNum, request);
                                    break;
                                case "LIST":
                                    await GetBoxes(commandNum, request);
                                    break;
                                case "SELECT":
                                    await SelectBox(commandNum, request);
                                    break;
                                case "CLOSE":
                                    await CloseBox(commandNum, request);
                                    break;
                                case "LOGOUT":
                                    await Logout(commandNum);
                                    return;
                                default:
                                    await SendMessageAsync("*", Status.BAD, "Unknown Command");
                                    break;
                            }

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

        private async Task Logout(string commandNum)
        {
            try
            {
                Console.WriteLine($"Клиент - {client.Client.RemoteEndPoint} отключился");
                await SendServiceResponseAsync("BYE SUETA... IMAP server logging out");
                await SendMessageAsync(commandNum, Status.OK, "Logout Completed");
                client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
          
        }

        private async Task CloseBox(string commandNum, string request)
        {
            foreach (var mail in SelectedBox.Mails)
            {
                if (mail.MailFlag == MailFlag.DELETED)
                {

                }
            }
        }

        private async Task SelectBox(string commandNum, string request)
        {
            if (ClientState == ClientState.NOT_AUTHENTICATED)
            {
                await SendMessageAsync(commandNum, Status.BAD, "Must Authenticate");
                return;
            }

            try
            {
                var boxName = request.Split(' ')[2];

                foreach (var box in ClientEntity)
                {
                    if (box.Name == boxName)
                    {
                        SelectedBox = box;
                        break;
                    }
                }
                
                var mails = SelectedBox.Mails;
                var recentMail = mails.FindAll(m => m.MailFlag == MailFlag.RECENT).Count;

                await SendServiceResponseAsync($"{mails.Count} EXISTS");
                await SendServiceResponseAsync($"{recentMail} RECENT");
                await SendServiceResponseAsync($"{mails.Count} EXISTS");
                await SendServiceResponseAsync($"[UIDVALIDITY {ClientEntity.UidValidity}] UIDs valid");
                await SendServiceResponseAsync($"[UIDNEXT {SelectedBox.nextMailUid}] Predicted next UID");
                await SendMessageAsync(commandNum, Status.OK, "Select completed");

                ClientState |= ClientState.SELECTED;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await SendMessageAsync(commandNum, Status.NO, "Select not completed");
            }
        }


        private async Task GetBoxes(string commandNum, string request)
        {
            if (ClientState == ClientState.NOT_AUTHENTICATED)
            {
                await SendMessageAsync(commandNum, Status.BAD, "Must Authenticate");
                return;
            }

            try
            {
                ClientEntity.ClientBox = await dbContext.GetBoxes(ClientEntity.Name);

                if (ClientEntity.ClientBox == null)
                {
                    await SendMessageAsync(commandNum, Status.NO, "Couldn't find boxes");
                    return;
                }
                
                await SendBoxListAsync(request.Split(' ')[2]);
              

                await SendMessageAsync(commandNum, Status.OK, "Completed");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await SendMessageAsync(commandNum, Status.BAD, "Invalid Command Argument");
            }

        }


        public async Task Authenticate(string commandNum, string request)
        {

            if (ClientState == ClientState.AUTHENTICATED)
            {
                await SendMessageAsync(commandNum, Status.BAD, "Authenticated Already");
                return;
            }

            try
            {
                var username = request.Split(' ')[2];
                var password = request.Split(' ')[3];

                var clientEntity = new ClientEntity(username, password);

                if (!await dbContext.Authenticate(clientEntity))
                {
                    await SendMessageAsync(commandNum, Status.NO, "Login failed");
                    return;
                }

                await SendMessageAsync(commandNum, Status.OK, "User logged in");
                ClientEntity = clientEntity;
                ClientState = ClientState.AUTHENTICATED;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await SendMessageAsync(commandNum, Status.BAD, "Something went wrong!");
            }

        }

       

        private async Task SendBoxListAsync(string box)
        {
            try
            {
                if (box.Trim('\"') == "/")
                {
                    await SendBoxes();
                    return;
                }

                await SendBoxes(box.Trim('\"'));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async Task SendBoxes(string box)
        {
            ClientBoxEntity concreteBox = null;
            foreach (var boxEntity in ClientEntity)
            {
                if (boxEntity.Name == box)
                {
                    concreteBox = boxEntity;
                    break;
                }
            }

            byte[] response;

            await using (var stream = new NetworkStream(client.Client))
            {
                foreach (var b in concreteBox)
                {
                    if (b.Boxes.Count > 0)
                    {
                        response = Encoding.UTF8.GetBytes($"* (\\HasChildren) \"/\" \"{b.Name}\"\r\n");
                        await stream.WriteAsync(response);
                    }
                    else
                    {
                        response = Encoding.UTF8.GetBytes($"* (\\HasNoChildren) \"/\" \"{b.Name}\"\r\n");
                        await stream.WriteAsync(response);
                    }
                }
            }
        }

        private async Task SendBoxes()
        {
            byte[] response;

            await using (var stream = new NetworkStream(client.Client))
            {
                foreach (var clientBox in ClientEntity)
                {
                    if (clientBox.Boxes.Count > 0)
                    {
                        response = Encoding.UTF8.GetBytes($"* (\\HasChildren) \"/\" \"{clientBox.Name}\"\r\n");
                        await stream.WriteAsync(response);
                    }
                    else
                    {
                        response = Encoding.UTF8.GetBytes($"* (\\HasNoChildren) \"/\" \"{clientBox.Name}\"\r\n");
                        await stream.WriteAsync(response);
                    }
                }
            }
        }

        private async Task SendServiceResponseAsync(string response)
        {
            try
            {
                byte[] resp;
                await using (var stream = new NetworkStream(client.Client))
                {
                    resp = Encoding.UTF8.GetBytes($"* {response}\r\n");
                    await stream.WriteAsync(resp);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }


        }

        public async Task SendMessageAsync(string command, Status status, string message)
        {
            try
            {
                var request = Encoding.UTF8.GetBytes($"{command} {status.Value} {message}\r\n");

                await using (var stream = new NetworkStream(client.Client))
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
                await using var stream = new NetworkStream(client.Client);
                do
                {
                    bytes = await stream.ReadAsync(data);
                    response += Encoding.UTF8.GetString(data, 0, bytes);
                } while (data[^1] != 0);


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
