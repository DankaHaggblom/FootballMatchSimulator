using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.MapGet("/GetGameState", async (string matchId) =>
{
    var matchCalculatorUrl = @$"http://{Environment.GetEnvironmentVariable("MATCHCALCULATOR_HOST")}/GetMatchGameState?matchId={matchId}";
    // Use HttpClient to send an http web request to the match calculator url to obtain the JSON data.
    var client = new HttpClient();
    var response = await client.GetAsync(matchCalculatorUrl);
    var content = await response.Content.ReadAsStringAsync();
    return Results.Ok(content);
});

app.MapGet("/SimulateMatchClick", async (string coachId, string matchId, float clickX, float clickY) =>
{
    var matchCalculatorUrl = @$"http://{Environment.GetEnvironmentVariable("MATCHCALCULATOR_HOST")}/SimulateMatch?clickX={clickX}&clickY={clickY}&coachId={coachId}&matchId={matchId}";
    // Use HttpClient to send an http web request to the match calculator url to obtain the JSON data.
    var client = new HttpClient();
    var response = await client.GetAsync(matchCalculatorUrl);
    var content = await response.Content.ReadAsStringAsync();
    return Results.Ok(content);

});

app.MapGet("/ResetMatch", async (string matchId) =>
{
    var matchCalculatorUrl = @$"http://{Environment.GetEnvironmentVariable("MATCHCALCULATOR_HOST")}/ResetMatch?matchId={matchId}";
    // Use HttpClient to send an http web request to the match calculator url to obtain the JSON data.
    var client = new HttpClient();
    var response = await client.GetAsync(matchCalculatorUrl);
    var content = await response.Content.ReadAsStringAsync();
    return Results.Ok(content);
});

app.MapPost("/Login", async (Credentials credentials) =>
{
    var matchCalculatorUrl = @$"http://{Environment.GetEnvironmentVariable("MATCHCALCULATOR_HOST")}/LoginOrRegister?username={credentials.username}&password={credentials.password}";
    // Use HttpClient to send an http web request to the match calculator url to obtain the JSON data.
    var client = new HttpClient();
    var response = await client.GetAsync(matchCalculatorUrl);
    var content = await response.Content.ReadAsStringAsync();
    return Results.Ok(content);
});

app.Run();

record Credentials (string username, string password);