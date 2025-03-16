using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace TelexistenceAPI.Repositories
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetConnectionString("MongoDB"));
            _database = client.GetDatabase(
                configuration["MongoDB:DatabaseName"] ?? "TelexistenceDB"
            );
        }

        public IMongoCollection<T> GetCollection<T>(string name)
        {
            return _database.GetCollection<T>(name);
        }
    }
}
