using BoardGamesOnline.Server.Games;
using Microsoft.AspNetCore.SignalR;

namespace BoardGamesOnline.Server.Hubs
{
    public class TickTackToeHub : Hub
    {
        private readonly TickTackToeQueue queue;
        private readonly TickTackToeInfo info;
        private readonly IWebHostEnvironment environment;

        public TickTackToeHub(TickTackToeQueue queue, TickTackToeInfo info, IWebHostEnvironment environment)
        {
            this.queue = queue;
            this.info = info;
            this.environment = environment;
        }

        public override async Task OnConnectedAsync()
        {
            if (environment.IsDevelopment())
            {
                Console.WriteLine($"{Context.ConnectionId} has connected");
            }

            var player = new TickTackToePlayer(Context.ConnectionId, Context.Abort);
            info.Add(player);

            if (queue.Count != 0)
            {
                string opponentId = queue.Item;
                var opponent = info.Get(opponentId);

                if(opponent != null)
                {
                    bool turn = new Random().Next(1, 3) == 1;

                    player.Opponent = opponent.Me;
                    player.Turn = turn;

                    opponent.Opponent = player.Me;
                    opponent.Turn = !turn;

                    await Clients.Client(player.Me).SendAsync("Notify", $"selected;{player.Turn}");
                    await Clients.Client(opponent.Me).SendAsync("Notify", $"selected;{opponent.Turn}");

                    if (turn) player.AbortAfterSeconds(32);
                    else opponent.AbortAfterSeconds(32);
                }
                else
                {
                    queue.Item = Context.ConnectionId;
                }
            }
            else
            {
                queue.Item = Context.ConnectionId;
            }
            await base.OnConnectedAsync();
        }

        public async Task Move(int x, int y)
        {
            if (environment.IsDevelopment())
            {
                Console.WriteLine($"{Context.ConnectionId} is at Move method");
            }

            var player = info.Get(Context.ConnectionId);

            var opponent = info.Get(player!.Opponent ?? "");
            if (opponent == null)
            {
                await Clients.Caller.SendAsync("Notify", "error;3");
            }

            if(!(bool)player.Turn!)
            {
                await Clients.Caller.SendAsync("Notify", "error;2");
                return;
            }

            var isSuccessful = player.Game!.Move(player.Me, x, y);
            if(!isSuccessful)
            {
                await Clients.Caller.SendAsync("Notify", "error;1");
                return;
            }

            player.CancelAbortion();
            await Clients.Caller.SendAsync("Notify", "ok");
            await Clients.Client(opponent!.Me).SendAsync("Notify", $"opponent;{x};{y}");

            player.SwitchTurn();
            opponent.SwitchTurn();

            if (player.Game.Winner != "progress")
            {
                if(player.Game.Winner == "tie")
                {
                    await Clients.Clients(player.Me, opponent.Me).SendAsync("Notify", "tie");
                }
                else if(player.Game.Winner == player.Me)
                {
                    await Clients.Caller.SendAsync("Notify", "win");
                    await Clients.Client(opponent.Me).SendAsync("Notify", "lose");
                }
                else
                {
                    await Clients.Caller.SendAsync("Notify", "lose");
                    await Clients.Client(opponent.Me).SendAsync("Notify", "win");
                }
                player.Abort();
                return;
            }

            opponent.AbortAfterSeconds(32);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (environment.IsDevelopment())
            {
                Console.WriteLine($"{Context.ConnectionId} has disconnected");
            }

            var player = info.Get(Context.ConnectionId);
            player!.Dispose();
            info.Remove(Context.ConnectionId);

            var opponent = info.Get(player.Opponent ?? "");

            if (opponent != null)
            {
                if (player.Game!.Winner == "progress")
                {
                    await Clients.Client(opponent.Me).SendAsync("Notify", "win");
                }
                opponent.AbortAfterSeconds(3);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
