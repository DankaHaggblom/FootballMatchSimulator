using MatchCalculator;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var dbHelper = new DatabaseHelper();

app.MapGet("/", () => Environment.GetEnvironmentVariable("DATABASE_HOST"));
app.MapGet("/CreateDataSet", async () => await MatchCalculatorHelper.InitializeDatabase(dbHelper));
app.MapGet("/SimulateMatch", async (float clickX, float clickY, string coachId, string matchId) => await MatchCalculatorHelper.SimulateMatchIncrement(dbHelper, clickX, clickY, coachId, matchId));
app.Run();
