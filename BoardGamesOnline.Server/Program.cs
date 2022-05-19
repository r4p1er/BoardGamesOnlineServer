using BoardGamesOnline.Server.Games;
using BoardGamesOnline.Server.Hubs;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddSingleton<TickTackToeQueue>();
builder.Services.AddSingleton<TickTackToeInfo>();
builder.Services.AddSingleton<ShipBattleQueue>();
builder.Services.AddSingleton<ShipBattleInfo>();

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.MapGet("/", () => "Runs");

app.MapHub<TickTackToeHub>("/ticktacktoe");
app.MapHub<ShipBattleHub>("/shipbattle");

app.Run();
