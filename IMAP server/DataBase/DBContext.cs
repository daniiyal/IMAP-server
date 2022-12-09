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

            BsonDocument user = new BsonDocument
            {
                {"Name", client.Name},
                {"Password", client.Password}
            };

            await usersCollection.InsertOneAsync(user);

        }

        public async Task<bool> Authenticate(ClientEntity client)
        {
            var dbClient = await GetClientAsync(client.Name);

            if (dbClient != null) 
                return dbClient.Password == client.Password;
            
            await SaveClient(client);
            
            return true;
        }


    }
}
