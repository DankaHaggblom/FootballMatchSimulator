using MatchCalculator;
using MatchCalculator.DatabaseItemTypes;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var dbHelper = new DatabaseHelper();

app.MapGet("/", () => Environment.GetEnvironmentVariable("DATABASE_HOST"));
app.MapGet("/CreatePlayers", async () => {
    for (int i = 0; i < 10; i++)
    {
        var player = new Player
        {
            Name = i.ToString()
        };
        await dbHelper.CreatePlayerAsync(player);
    }
});
app.MapGet("/ListPlayers", async () => {

        return await dbHelper.GetAllPlayersAsync();
});

app.Run();
