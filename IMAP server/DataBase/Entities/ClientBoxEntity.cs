using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace IMAP_server.DataBase.Entities
{
    public class ClientBoxEntity
    {
        public string Name { get; set; }
        public List<ClientBoxEntity> Boxes { get; set; }
        public uint NextMailUid { get; set; }
        public ulong UidValidity { get; set; }

        public List<MailEntity> Mails { get; set; }

        public ClientBoxEntity(string boxName)
        {
            Name = boxName;
            NextMailUid = 1;
            UidValidity = 1;
            Boxes = new List<ClientBoxEntity>();
            Mails = new List<MailEntity>();
        }

        public IEnumerator<ClientBoxEntity> GetEnumerator()
        {
            yield return this;
            for (int i = 0; i < Boxes.Count; i++)
            {
                foreach (var box in Boxes[i])
                {
                    yield return box;
                }
            }
        }

        public async Task AddMail(string clientName, DBContext dbContext, MailEntity mailEntity)
        {
            await dbContext.AddMail(clientName, Name, mailEntity);
        }
    }
}
