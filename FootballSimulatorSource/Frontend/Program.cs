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

app.Run();
