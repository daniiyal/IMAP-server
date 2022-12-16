using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMAP_server.Enums
{
    [Flags]
    public enum ClientState
    {
        NOT_AUTHENTICATED = 1,
        AUTHENTICATED = 2,
        SELECTED = 4,
        LOGOUT = 8
    }
}
