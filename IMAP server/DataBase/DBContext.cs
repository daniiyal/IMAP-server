using IMAP_server.DataBase.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace IMAP_server.DataBase
{
    public class DBContext
    {
        public static string connectionString = "mongodb://127.0.0.1:27017";
        public static string database = "IMAP";

        private MongoClient _client;

        public DBContext()
        {
            _client = new MongoClient(connectionString);
        }

        public IMongoCollection<BsonDocument> GetCollection(string database, string collection)
        {
            var db = _client.GetDatabase(database);
            return db.GetCollection<BsonDocument>(collection);
        }

        public async Task<ClientEntity?> GetClientAsync(string name)
        {
            ClientEntity? client = null;
            var filter = new BsonDocument { { "Name", name } };
            var dbClient = await GetCollection(database, "Clients").Find(filter).FirstOrDefaultAsync();

            if (dbClient == null)
                return null;

            client = BsonSerializer.Deserialize<ClientEntity>(dbClient);

            return client;
        }



        public async Task<List<ClientEntity>> GetClientsAsync()
        {
            var clientList = new List<ClientEntity>();
            var clients = await GetCollection(database, "Clients").Find(new BsonDocument()).ToListAsync();

            foreach (var client in clients)
            {
                clientList.Add(BsonSerializer.Deserialize<ClientEntity>(client));
            }

            return clientList;
        }

        public async Task SaveClient(ClientEntity client)
        {
            var usersCollection = GetCollection(database, "Clients");

            var clientBson = client.ToBsonDocument();

            await usersCollection.InsertOneAsync(clientBson);
        }

        public async Task<bool> Authenticate(ClientEntity client)
        {
            var dbClient = await GetClientAsync(client.Name);

            if (dbClient != null)
                return dbClient.Password == client.Password;

            await SaveClient(client);

            return true;
        }


        public async Task DeleteUser(string name)
        {
            var usersCollection = GetCollection(database, "Clients");

            BsonDocument user = new BsonDocument
            {
                {"Name", name},
            };

            await usersCollection.DeleteOneAsync(user);
        }

        public async Task<List<ClientBoxEntity>?> GetBoxes(string name)
        {
            var user = await GetClientAsync(name);

            return user?.ClientBox;
        }

        public async Task<ClientBoxEntity?> GetBox(string name, string boxName)
        {
            var user = await GetClientAsync(name);

            ClientBoxEntity userBox = null;

            foreach (var box in user)
            {
                if (box.Name == boxName)
                {
                    userBox = box;
                    break;
                }
            }

            return userBox;
        }

        public async Task AddMail(string name, string boxName, MailEntity mailEntity)
        {
            var builder = Builders<BsonDocument>.Filter;

            var filter = builder.And(
                builder.Eq("Name", name),
                builder.Eq("ClientBox.Name", boxName));


            var users = GetCollection(database, "Clients");

            var mail = mailEntity.ToBsonDocument();

            var updateSettings = Builders<BsonDocument>.Update.Set("ClientBox.$.NextMailUid", mailEntity.Uid+1).Set("ClientBox.$.UidValidity", mailEntity.Uid + 1).Push("ClientBox.$.Mails", mail);

            await users.UpdateOneAsync(filter, updateSettings);
        }

        public async Task DeleteMail(string name, string boxName, MailEntity mailEntity)
        {
            var builder = Builders<BsonDocument>.Filter;

            var filter = builder.And(builder.Eq("Name", name),
                                                builder.Eq("ClientBox.Name", boxName));

            var users = GetCollection(database, "Clients");

            var mail = mailEntity.ToBsonDocument();

            var updateSettings = Builders<BsonDocument>.Update.Pull("ClientBox.$.Mails", mail);

            await users.UpdateOneAsync(filter, updateSettings);
        }

        public async Task<List<MailEntity>> GetMails(String user, String boxName)
        {
            var box = await GetBox(user, boxName);

            return box.Mails;
        }
    }

}
