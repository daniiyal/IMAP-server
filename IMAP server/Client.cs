using IMAP_server.DataBase;
using IMAP_server.DataBase.Entities;
using System.Net.Sockets;
using System.Text;

namespace IMAP_server
{
    public class Client
    {
        public string Name { get; set; }
        //public string MailAddress { get; set; }

        private TcpClient client { get; }
        private DBContext dbContext { get; }

        private string oldMessagePart = "";
        public Client(TcpClient client)
        {
            this.client = client;
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
                        switch (request.Split(' ')[1])
                        {
                            case "LOGIN":
                                await Authenticate(request);
                                break;
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


        public async Task Authenticate(string command)
        {
            var commandNum = command.Split(' ')[0];
            var username = command.Split(' ')[2];
            var password = command.Split(' ')[3];
            try
            {
                var clientEntity = new ClientEntity(username, password);

                if (!await dbContext.Authenticate(clientEntity))
                {
                    await SendMessageAsync(commandNum, Status.NO, "Login failed");
                    return;
                }

                await SendMessageAsync(commandNum, Status.OK, "User logged in");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await SendMessageAsync(commandNum, Status.BAD, "Something went wrong!");
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
                    result[0] = oldMessagePart + response[0];
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
                throw new Exception(ex.Message);
            }

            return result;
        }
    }
}
