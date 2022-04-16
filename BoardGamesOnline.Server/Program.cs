using BoardGamesOnline.Server.Games;
using BoardGamesOnline.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
builder.Services.AddSingleton<TickTackToeQueue>();
builder.Services.AddSingleton<TickTackToeInfo>();
var app = builder.Build();

app.MapHub<TickTackToeHub>("/ticktacktoe");

app.Run();
