var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => Environment.GetEnvironmentVariable("DATABASE_HOST"));

app.Run();
