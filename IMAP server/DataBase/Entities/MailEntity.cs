using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using IMAP_server.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace IMAP_server.DataBase.Entities
{
    public class MailEntity
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public uint Uid { get; private set; }
        public MailFlag MailFlag { get; set; }
        public string From { get; set; }
        public List<string> To { get; set; }
        public List<string> Cc { get; set; }
        public string Body { get; set; }
        public DateTime Created { get; set; }

        public MailEntity(uint uid, string from, List<string> to, List<string> cc, string body, DateTime created)
        {
            Uid = uid;
            MailFlag = MailFlag.RECENT;
            From = from;
            To = to;
            Cc = cc;
            Body = body;
            Created = created;
        }
    }
}
