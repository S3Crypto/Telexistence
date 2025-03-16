using MongoDB.Driver;
using TelexistenceAPI.Core.Entities;
using TelexistenceAPI.Core.Interfaces;

namespace TelexistenceAPI.Repositories
{
    public class RobotRepository : IRobotRepository
    {
        private readonly IMongoCollection<Robot> _robots;

        public RobotRepository(MongoDbContext context)
        {
            _robots = context.GetCollection<Robot>("Robots");
        }

        public async Task<Robot?> GetAsync(string id)
        {
            return await _robots.Find(r => r.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Robot>> GetAllAsync()
        {
            return await _robots.Find(_ => true).ToListAsync();
        }

        public async Task<Robot> UpdateAsync(Robot robot)
        {
            await _robots.ReplaceOneAsync(r => r.Id == robot.Id, robot);
            return robot;
        }
    }
}
