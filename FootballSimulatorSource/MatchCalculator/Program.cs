using MatchCalculator;
using MatchCalculator.DatabaseItemTypes;
using System.Numerics;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var dbHelper = new DatabaseHelper();

app.MapGet("/", () => Environment.GetEnvironmentVariable("DATABASE_HOST"));
app.MapGet("/CreateDataSet", async () => await MatchCalculatorHelper.InitializeDatabase(dbHelper));
app.Run();
