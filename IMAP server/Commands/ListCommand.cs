using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using IMAP_server.DataBase.Entities;
using IMAP_server.Enums;

namespace IMAP_server.Commands
{
    public class ListCommand : Command
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
                Client.ClientEntity.ClientBox = await Client.DbContext.GetBoxes(Client.ClientEntity.Name);

                if (Client.ClientEntity.ClientBox == null)
                {
                    await Client.SendMessageAsync(commandNum, Status.NO, "Couldn't find boxes");
                    return;
                }

                await SendBoxListAsync(request.Split(' ')[2]);


                await Client.SendMessageAsync(commandNum, Status.OK, "Completed");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await Client.SendMessageAsync(commandNum, Status.BAD, "Invalid Command Argument");
            }

        }
        private async Task SendBoxListAsync(string box)
        {
            try
            {
                if (box.Trim('\"') == "")
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

            foreach (var boxEntity in Client.ClientEntity)
            {
                if (boxEntity.Name == box)
                {
                    concreteBox = boxEntity;
                    break;
                }
            }

            byte[] response;

            await using (var stream = new NetworkStream(Client.TcpClient.Client))
            {
                string parent = "";
                foreach (var b in concreteBox)
                {
                    if (b.Boxes.Count > 0)
                    {
                        response = Encoding.UTF8.GetBytes($"* LIST (\\HasChildren) \".\" \"{b.Name}\"\r\n");
                        parent = b.Name + ".";
                        await stream.WriteAsync(response);
                    }
                    else
                    {
                        response = Encoding.UTF8.GetBytes($"* LIST (\\HasNoChildren) \".\" \"{parent}{b.Name}\"\r\n");
                        await stream.WriteAsync(response);
                    }
                }
            }
        }

        private async Task SendBoxes()
        {
            byte[] response;

            await using (var stream = new NetworkStream(Client.TcpClient.Client))
            {
                string parent = "";
                foreach (var clientBox in Client.ClientEntity)
                {
                    if (clientBox.Boxes.Count > 0)
                    {
                        response = Encoding.UTF8.GetBytes($"* LIST (\\HasChildren) \".\" {clientBox.Name}\r\n");
                        parent = clientBox.Name + ".";
                        await stream.WriteAsync(response);
                    }
                    else
                    {
                        response = Encoding.UTF8.GetBytes($"* LIST (\\HasNoChildren) \".\" \"{parent}{clientBox.Name}\"\r\n");
                        await stream.WriteAsync(response);
                    }
                }
            }
        }

    }
}
