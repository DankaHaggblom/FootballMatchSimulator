using MatchCalculator;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var dbHelper = new DatabaseHelper();

app.MapGet("/", () => Environment.GetEnvironmentVariable("DATABASE_HOST"));
app.MapGet("/CreateDataSet", async () => await MatchCalculatorHelper.InitializeDatabase(dbHelper));
app.MapGet("/SimulateMatch", async (float clickX, float clickY, string coachId, string matchId) => await MatchCalculatorHelper.SimulateMatchIncrement(dbHelper, clickX, clickY, coachId, matchId));
app.MapGet("/ResetMatch", async (string matchId) => await MatchCalculatorHelper.ResetMatch(dbHelper, matchId));
app.MapGet("/GetMatchGameState", async (string matchId) => await MatchCalculatorHelper.GetMatchGameState(dbHelper, matchId));
app.Run();
