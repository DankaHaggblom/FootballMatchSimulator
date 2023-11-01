using MatchCalculator.DatabaseItemTypes;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading.Channels;

namespace MatchCalculator
{
    public class MatchCalculatorHelper
    {
        public class SimulateResults
        {
            public Match? Match { get; set; }
            public Team? HomeTeam { get; set; }
            public Team? AwayTeam { get; set; }
            public Coach? Coach { get; set; }
            public List<Player>? Players { get; set; }
            public List<string> Events { get; set; } = new List<string>();
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
        const float playerSpeed = 5;

        public static async Task<SimulateResults> InitializeDatabase(DatabaseHelper dbHelper)
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

            return new SimulateResults
            {
                Match = match,
                HomeTeam = homeTeam,
                AwayTeam = awayTeam,
                Players = players,
                Coach = coach
            };
        }

        public static async Task<SimulateResults> SimulateMatchIncrement(DatabaseHelper dbHelper, float clickX, float clickY, string coachId, string matchId)
        {
            var clickPos = new Vector2(clickX, clickY);
            var match = await dbHelper.GetMatchAsync(matchId);
            var coach = await dbHelper.GetCoachAsync(coachId);
            var team = await dbHelper.GetTeamAsync(coach.TeamId);
            var otherTeam = await dbHelper.GetTeamAsync(match.HomeTeamId == coach.TeamId ? match.AwayTeamId!:match.HomeTeamId!);
            var teamPlayers = await dbHelper.GetAllPlayersFromTeamAsync(team);
            var otherPlayers = await dbHelper.GetAllPlayersFromTeamAsync(otherTeam);
            
            var events = new List<string>();

            if (teamPlayers.Any(x => x.Id == match.BallPossessionPlayerId))
            {

            }
            else
            {
                foreach(var player in teamPlayers) 
                {
                    var oldPos = new Vector2(player.PosX, player.PosY);
                    var clickDirection = new Vector2(clickX, clickY) - oldPos;
                    var ballDirection = new Vector2(match.BallPosX, match.BallPosY) - oldPos;
                    var targetDirection = clickDirection.Length() < ballDirection.Length() ? clickDirection : ballDirection;
                    var newPos = oldPos + Vector2.Normalize(targetDirection) * Math.Min(playerSpeed, targetDirection.Length());

                    player.PosX = newPos.X;
                    player.PosY = newPos.Y;

                    ballDirection = new Vector2(match.BallPosX, match.BallPosY) - newPos;
                    if (ballDirection.Length() < 2)
                    {
                        if (match.BallPossessionPlayerId is null)
                        {
                            match.BallPossessionPlayerId = player.Id;
                            match.BallPosX = player.PosX;
                            match.BallPosY = player.PosY;
                            events.Add($"{player.Name} takes the ball.");
                        }
                        else if (!teamPlayers.Any(x => x.Id == match.BallPossessionPlayerId))
                        {
                            var stealingChance = 4;
                            var stealingRoll = RandomNumberGenerator.GetInt32(10);

                            if (stealingChance > stealingRoll)
                            {
                                events.Add($"{player.Name} steals the ball from {otherPlayers.FirstOrDefault(x => x.Id == match.BallPossessionPlayerId)}.");
                                match.BallPossessionPlayerId = player.Id; 
                                match.BallPosX = player.PosX;
                                match.BallPosY = player.PosY;
                            }
                        }
                    }
                }
            }

            //TODO: Save the match and players that moved to the database.

            return new SimulateResults
            {
                Events = events,
                Match = match,
                AwayTeam = otherTeam,
                HomeTeam = team,
                Coach = coach,
                Players = otherPlayers.Concat(teamPlayers).ToList()
            };
        }
    }
}
