using BoardGamesOnline.Server.Games;
using BoardGamesOnline.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
builder.Services.AddSingleton<TickTackToeQueue>();
builder.Services.AddSingleton<TickTackToeInfo>();
builder.Services.AddSingleton<ShipBattleQueue>();
builder.Services.AddSingleton<ShipBattleInfo>();
var app = builder.Build();

app.MapGet("/", () => "Runs");

app.MapHub<TickTackToeHub>("/ticktacktoe");
app.MapHub<ShipBattleHub>("/shipbattle");

app.Run();
