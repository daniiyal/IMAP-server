using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMAP_server.Commands
{
    public interface ICommand
    {
        Client Client { get; set; }
        public Task Execute(Client client, string commandNum, string request);
    }
}
