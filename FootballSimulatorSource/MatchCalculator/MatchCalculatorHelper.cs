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
        static Vector2[] goalCenterPositions = new Vector2[]
        {
            new Vector2(-30,0),
            new Vector2(30, 0)
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

        private static async Task ResetField(DatabaseHelper dbHelper, Match match)
        {
            var teams = new[]
            {
                await dbHelper.GetTeamAsync(match.AwayTeamId!),
                await dbHelper.GetTeamAsync(match.HomeTeamId!)
            };

            foreach (var team in teams)
            {
                var players = await dbHelper.GetAllPlayersFromTeamAsync(team);


                for (int i = 0; i < 7; i++)
                {
                    var player = players[i];
                    player.PosX = startingPositions[i].X * (team.Id == match.HomeTeamId ? -1 : 1);
                    player.PosY = startingPositions[i].Y;
                }

                await dbHelper.SavePlayersAsync(players);
            }

            match.BallPosX = 0;
            match.BallPosY = 0;
            match.BallPossessionPlayerId = null;
        }

        public static async Task<SimulateResults> ResetMatch(DatabaseHelper dbHelper, string matchId)
        {
            var match = await dbHelper.GetMatchAsync(matchId);
            var team1 = await dbHelper.GetTeamAsync(match.HomeTeamId!);
            var team2 = await dbHelper.GetTeamAsync(match.AwayTeamId!);
            var players1 = await dbHelper.GetAllPlayersFromTeamAsync(team1);
            var players2 = await dbHelper.GetAllPlayersFromTeamAsync(team2);
            await ResetField(dbHelper, match);
            await dbHelper.SaveMatchAsync(match);

            return new SimulateResults
            {
                Events = new(),
                Match = match,
                AwayTeam = team2,
                HomeTeam = team1,                
                Players = players1.Concat(players2).ToList()
            };
        }

        public static async Task<SimulateResults> GetMatchGameState(DatabaseHelper dbHelper, string matchId)
        {
            var match = await dbHelper.GetMatchAsync(matchId);
            var team1 = await dbHelper.GetTeamAsync(match.HomeTeamId!);
            var team2 = await dbHelper.GetTeamAsync(match.AwayTeamId!);
            var players1 = await dbHelper.GetAllPlayersFromTeamAsync(team1);
            var players2 = await dbHelper.GetAllPlayersFromTeamAsync(team2);

            return new SimulateResults
            {
                Events = new(),
                Match = match,
                AwayTeam = team2,
                HomeTeam = team1,                
                Players = players1.Concat(players2).ToList()
            };
        }

        public static async Task<SimulateResults> SimulateMatchIncrement(DatabaseHelper dbHelper, float clickX, float clickY, string coachId, string matchId)
        {
            var clickPos = new Vector2(clickX, clickY);
            var match = await dbHelper.GetMatchAsync(matchId);
            var coach = await dbHelper.GetCoachAsync(coachId);
            var coachTeam = await dbHelper.GetTeamAsync(coach.TeamId);
            var aiTeam = await dbHelper.GetTeamAsync(match.HomeTeamId == coach.TeamId ? match.AwayTeamId! : match.HomeTeamId!);
            var coachPlayers = await dbHelper.GetAllPlayersFromTeamAsync(coachTeam);
            var aiPlayers = await dbHelper.GetAllPlayersFromTeamAsync(aiTeam);
            var aiClickPos = new Vector2();
            var events = new List<string>();

            // Simulate the movement of the coach's players from the human click
            await SimulateTeamIncrement(dbHelper, clickPos, match, coachPlayers, aiPlayers, events, goalCenterPositions[1]);
            // Decide a click position for the AI.
            if (aiPlayers.Any(x => x.Id == match.BallPossessionPlayerId))
            {
                aiClickPos = goalCenterPositions[0];
            }
            else
            {
                aiClickPos = new Vector2(match.BallPosX, match.BallPosY);
            }
            // Simulate the movement of the AI's players from the AI click
            await SimulateTeamIncrement(dbHelper, aiClickPos, match, aiPlayers, coachPlayers, events, goalCenterPositions[0]);

            await dbHelper.SaveMatchAsync(match);
            await dbHelper.SavePlayersAsync(aiPlayers.Concat(coachPlayers).ToList());

            return new SimulateResults
            {
                Events = events,
                Match = match,
                AwayTeam = aiTeam,
                HomeTeam = coachTeam,
                Coach = coach,
                Players = aiPlayers.Concat(coachPlayers).ToList()
            };
        }

        private static async Task SimulateTeamIncrement(DatabaseHelper dbHelper, Vector2 clickPos, Match match, List<Player> teamPlayers, List<Player> otherPlayers, List<string> events, Vector2 goalPosition)
        {
            // Depending on whether the coachTeam holds the ball or not, do different things.
            if (teamPlayers.Any(x => x.Id == match.BallPossessionPlayerId))
            {
                foreach (var player in teamPlayers)
                {
                    var oldPos = new Vector2(player.PosX, player.PosY);
                    var clickDirection = clickPos - oldPos;
                    var goalDirection = goalPosition - oldPos;
                    // The closest coachTeam mate to pass is the player in the coachTeam that is closest to the click and that is not the current player.
                    var closestTeamMateToPass = (from mate in teamPlayers
                                                 where mate.Id != match.BallPossessionPlayerId
                                                 let matePos = new Vector2(mate.PosX, mate.PosY)
                                                 let distanceToClick = (clickPos - matePos).Length()
                                                 where distanceToClick < clickDirection.Length()
                                                 let mateDistanceToUs = (oldPos - matePos).Length()
                                                 orderby mateDistanceToUs
                                                 select mate).FirstOrDefault();
                    Vector2? closestTeamMateToPassDirection = (closestTeamMateToPass is not null)
                        ? new Vector2(closestTeamMateToPass.PosX, closestTeamMateToPass.PosY) - oldPos
                        : null;

                    // If the goal is close enough to the ball holder to shoot, then shoot.
                    // Otherwise, if the closest coachTeam mate is within range, pass.
                    // Otherwise, run for the click direction.
                    if (goalDirection.Length() < 15)
                    {
                        var goalChance = 5;
                        var goalRoll = RandomNumberGenerator.GetInt32(10);

                        if (goalChance > goalRoll)
                        {
                            events.Add($"{player.Name} scores!");

                            if (player.TeamId == match.HomeTeamId)
                            {
                                match.ScoreHome = match.ScoreHome++;
                            }
                            else
                            {
                                match.ScoreAway = match.ScoreAway++;
                            }

                            await ResetField(dbHelper, match);
                        }
                    }
                    else if (closestTeamMateToPassDirection?.Length() < 40 && closestTeamMateToPass is not null)
                    {
                        // Roll for a pass. If it succeeds, it will end up in the possession of the mate.
                        var passChance = 7;
                        var passRoll = RandomNumberGenerator.GetInt32(10);

                        if (passChance > passRoll)
                        {
                            events.Add($"{player.Name} accurately passes to {closestTeamMateToPass.Name}!");
                            match.BallPossessionPlayerId = closestTeamMateToPass.Id;
                            match.BallPosX = closestTeamMateToPass.PosX;
                            match.BallPosY = closestTeamMateToPass.PosY;
                        }
                        // If it fails, it will end up at some point between the passer and the mate, and not held by anyone.
                        else
                        {
                            events.Add($"{player.Name} misses when passing to {closestTeamMateToPass.Name}!");
                            var dirToMate = closestTeamMateToPassDirection ?? default;
                            var lengthToMate = closestTeamMateToPassDirection?.Length() ?? 0;
                            var endDistance = RandomNumberGenerator.GetInt32((int)lengthToMate);
                            var endPosition = (dirToMate == Vector2.Zero)
                                ? oldPos
                                : oldPos + Vector2.Normalize(dirToMate) * endDistance;
                            match.BallPossessionPlayerId = null;
                            match.BallPosX = endPosition.X;
                            match.BallPosY = endPosition.Y;
                        }
                    }
                    else
                    {
                        // The player moves towards the click direction at their speed
                        var newPos = (clickDirection == Vector2.Zero)
                            ? oldPos
                            : oldPos + Vector2.Normalize(clickDirection) * Math.Min(playerSpeed, clickDirection.Length());
                        player.PosX = newPos.X;
                        player.PosY = newPos.Y;
                        match.BallPosX = player.PosX;
                        match.BallPosY = player.PosY;
                    }
                }
            }
            // When the team doesn't hold the ball...
            else
            {
                foreach (var player in teamPlayers)
                {
                    var oldPos = new Vector2(player.PosX, player.PosY);
                    var clickDirection = clickPos - oldPos;
                    var ballDirection = new Vector2(match.BallPosX, match.BallPosY) - oldPos;
                    // The player moves towards the click or towards the ball, whatever is closest
                    var targetDirection = clickDirection.Length() < ballDirection.Length() ? clickDirection : ballDirection;
                    // The player moves towards its target at their speed.
                    var newPos = (targetDirection == Vector2.Zero)
                        ? oldPos
                        : oldPos + Vector2.Normalize(targetDirection) * Math.Min(playerSpeed, targetDirection.Length());

                    player.PosX = newPos.X;
                    player.PosY = newPos.Y;

                    // Calculate distance from the new position to the ball
                    ballDirection = new Vector2(match.BallPosX, match.BallPosY) - newPos;
                    // If close enough, 
                    if (ballDirection.Length() < 2)
                    {
                        // If the ball was free, the player gets the ball
                        if (match.BallPossessionPlayerId is null)
                        {
                            match.BallPossessionPlayerId = player.Id;
                            match.BallPosX = player.PosX;
                            match.BallPosY = player.PosY;
                            events.Add($"{player.Name} takes the ball.");
                        }
                        // If it was held by the other coachTeam, attempt to steal it.
                        else if (otherPlayers.Any(x => x.Id == match.BallPossessionPlayerId))
                        {
                            var stealingChance = 4;
                            var stealingRoll = RandomNumberGenerator.GetInt32(10);

                            if (stealingChance > stealingRoll)
                            {
                                events.Add($"{player.Name} steals the ball from {otherPlayers.FirstOrDefault(x => x.Id == match.BallPossessionPlayerId)?.Name}.");
                                match.BallPossessionPlayerId = player.Id;
                                match.BallPosX = player.PosX;
                                match.BallPosY = player.PosY;
                            }
                        }
                    }
                }
            }
        }
    }
}
