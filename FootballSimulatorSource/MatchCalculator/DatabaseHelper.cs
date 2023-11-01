using MatchCalculator.DatabaseItemTypes;
using MongoDB.Driver;

namespace MatchCalculator
{
    public class DatabaseHelper
    {
        private readonly IMongoCollection<Match> _matches;
        private readonly IMongoCollection<Player> _players;
        private readonly IMongoCollection<Team> _teams;
        private readonly IMongoCollection<Coach> _coaches;

        public DatabaseHelper() 
        {
            var connectionString = $"mongodb://{Environment.GetEnvironmentVariable("DATABASE_HOST")}:27017";
            MongoClient dbClient = new MongoClient(connectionString);
            var database = dbClient.GetDatabase("FootballSimulatorDB");

            _matches = database.GetCollection<Match>("Matches");
            _players = database.GetCollection<Player>("Players");
            _teams = database.GetCollection<Team>("Teams");
            _coaches = database.GetCollection<Coach>("Coaches");
        }

        public async Task<Player> CreatePlayerAsync(Player player)
        {
            await _players.InsertOneAsync(player);
            return player;
        }

        public async Task<Coach> CreateCoachAsync(Coach coach)
        {
            await _coaches.InsertOneAsync(coach);
            return coach;
        }

        public async Task<Team> CreateTeamAsync(Team team)
        {
            await _teams.InsertOneAsync(team);
            return team;
        }

        public async Task<Match> CreateMatchAsync(Match match)
        {
            await _matches.InsertOneAsync(match);
            return match;
        }

        public async Task<Coach> GetCoach(string username, string password)
        {
            return await _coaches.Find(x => x.Username == username && x.Password == password).FirstOrDefaultAsync();
        }

        public async Task<List<Player>> GetAllPlayersAsync()
        {
            return await _players.Find(_ => true).ToListAsync();
        }

        public async Task<List<Player>> GetAllPlayersFromTeamAsync(Team team)
        {
            return await _players.Find(x => x.TeamId == team.Id).ToListAsync();
        }

        public async Task ClearDatabase()
        {
            await _matches.DeleteManyAsync(x => true);
            await _players.DeleteManyAsync(x => true);
            await _teams.DeleteManyAsync(x => true);
            await _coaches.DeleteManyAsync(x => true);
        }
    }
}
