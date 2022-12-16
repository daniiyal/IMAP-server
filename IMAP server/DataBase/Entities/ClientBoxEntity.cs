using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace IMAP_server.DataBase.Entities
{
    public class ClientBoxEntity
    {
        [BsonId]
        public ObjectId Id { get; set; }
        
        public string Name { get; set; }
        public List<ClientBoxEntity> Boxes { get; set; }
        public uint nextMailUid { get; set; }

        public List<MailEntity> Mails { get; set; }

        public ClientBoxEntity(string boxName)
        {
            Name = boxName;
            nextMailUid = 1;
            Boxes = new List<ClientBoxEntity>();
            Mails = new List<MailEntity>();
        }

        public void InitBoxes()
        {
            nextMailUid = 1;
            var boxes = new List<ClientBoxEntity>()
            {
                new("Drafts"),
                new("Junk"),
                new("Sent"),
                new("Trash")
            };

            Boxes.AddRange(boxes);
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

    }
}
