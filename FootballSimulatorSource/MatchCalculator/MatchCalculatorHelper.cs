using MatchCalculator.DatabaseItemTypes;
using System.Numerics;

namespace MatchCalculator
{
    public class MatchCalculatorHelper
    {
        public class InitializeResults
        {
            public Match? Match { get; set; }
            public Team? HomeTeam { get; set; }
            public Team? AwayTeam { get; set; }
            public Coach? Coach { get; set; }
            public List<Player>? Players { get; set; }
        }

        static Vector2[] startingPositions = new Vector2[]
        {
            new Vector2(28, 0),
            new Vector2(20, 0),
            new Vector2(20, 10),
            new Vector2(20, -10),
            new Vector2(10, 0),
            new Vector2(10, 10),
            new Vector2(10, -10),
        };
        const float fieldLength = 60;
        const float fieldWidth = 40;

        public static async Task<InitializeResults> InitializeDatabase(DatabaseHelper dbHelper)
        {
            await dbHelper.ClearDatabase();
            var match = new Match();
            var homeTeam = new Team { Name = "Mongos" };
            var awayTeam = new Team { Name = "Tontos" };
            var players = new List<Player>();
            var coach = new Coach
            {
                Name = "Dunckus",
                Username = "user",
                Password = "password",
                TeamId = homeTeam.Id
            };
            match.HomeTeamId = homeTeam.Id;
            match.AwayTeamId = awayTeam.Id;

            foreach (var team in new[] { homeTeam, awayTeam })
                for (int i = 0; i < 7; i++)
                {
                    var player = new Player
                    {
                        Name = team.Name + i.ToString(),
                        TeamId = team.Id,
                        MatchId = match.Id,
                        PosX = startingPositions[i].X * (team == homeTeam ? -1 : 1),
                        PosY = startingPositions[i].Y
                    };

                    players.Add(player);
                    await dbHelper.CreatePlayerAsync(player);
                }

            await dbHelper.CreateMatchAsync(match);
            await dbHelper.CreateTeamAsync(homeTeam);
            await dbHelper.CreateTeamAsync(awayTeam);
            await dbHelper.CreateCoachAsync(coach);

            return new InitializeResults
            {
                Match = match,
                HomeTeam = homeTeam,
                AwayTeam = awayTeam,
                Players = players,
                Coach = coach
            };
        }
    }
}
