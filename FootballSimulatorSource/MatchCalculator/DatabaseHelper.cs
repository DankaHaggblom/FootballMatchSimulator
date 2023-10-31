using MongoDB.Driver;

namespace MatchCalculator
{
    public class DatabaseHelper
    {
        public DatabaseHelper() 
        {
            var connectionString = $"mongodb://{Environment.GetEnvironmentVariable("DATABASE_HOST")}:27017";
            MongoClient dbClient = new MongoClient(connectionString);
            var database = dbClient.GetDatabase("FootballSimulatorDB");

        }
    }
}
