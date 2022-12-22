using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace IMAP_server.DataBase.Entities
{
    public class ClientEntity
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public List<ClientBoxEntity>? ClientBox { get; set; }

        public ClientEntity(string name, string password)
        {
            Name = name;
            Password = password;
            ClientBox = new List<ClientBoxEntity>()
            {
                new("INBOX"),
                new("Drafts"),
                new("Junk"),
                new("Sent"),
                new("Trash")
            };
        }

        public IEnumerator<ClientBoxEntity> GetEnumerator()
        {
            for (int i = 0; i < ClientBox.Count; i++)
            {
                yield return ClientBox[i];
                foreach (var box in ClientBox[i].Boxes)
                {
                    yield return box;
                }

            }
        }
    }
}
