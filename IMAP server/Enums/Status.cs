using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMAP_server.Enums
{
    public class Status
    {
        private Status(string value)
        {
            Value = value;
        }

        public string Value { get; }


        public static Status OK => new("OK");
        public static Status NO => new("NO");

        public static Status BAD => new("BAD");

        public override string ToString()
        {
            return Value;
        }
    }
}
