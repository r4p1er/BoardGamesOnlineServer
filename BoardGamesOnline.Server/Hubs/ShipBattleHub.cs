using BoardGamesOnline.Server.Games;
using Microsoft.AspNetCore.SignalR;

namespace BoardGamesOnline.Server.Hubs
{
    public class ShipBattleHub : Hub
    {
        private readonly ShipBattleQueue queue;
        private readonly ShipBattleInfo info;
        private readonly IWebHostEnvironment environment;

        public ShipBattleHub(ShipBattleQueue queue, ShipBattleInfo info, IWebHostEnvironment environment)
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

            var player = new ShipBattlePlayer(Context.ConnectionId, Context.Abort);
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

                    player.AbortAfterSeconds(91);
                    opponent.AbortAfterSeconds(94);
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
        
        public async Task SetShips(int[][] jaggedShips)
        {
            if (environment.IsDevelopment())
            {
                Console.WriteLine($"{Context.ConnectionId} is at SetShips method");
            }

            var player = info.Get(Context.ConnectionId);

            var opponent = info.Get(player!.Opponent ?? "");
            if (opponent == null)
            {
                await Clients.Caller.SendAsync("Notify", "error;7");
            }

            if (player.Game.IsReady)
            {
                await Clients.Caller.SendAsync("Notify", "error;2");
                return;
            }

            if (jaggedShips.Length != 10)
            {
                await Clients.Caller.SendAsync("Notify", "error;3");
                return;
            }

            var ships = new int[10, 4];

            for(int i = 0; i < jaggedShips.Length; ++i)
            {
                if (jaggedShips[i].Length != 4)
                {
                    await Clients.Caller.SendAsync("Notify", "error;3");
                    return;
                }
                for(int j = 0; j < jaggedShips[i].Length; ++j)
                {
                    ships[i, j] = jaggedShips[i][j];
                }
            }

            bool isSuccessful = player.Game.SetShips(ships);
            if (!isSuccessful)
            {
                await Clients.Caller.SendAsync("Notify", "error;3");
                return;
            }

            player.CancelAbortion();
            await Clients.Caller.SendAsync("Notify", "placed");

            if (opponent!.Game.IsReady)
            {
                await Clients.Clients(player.Me, opponent.Me).SendAsync("Notify", "fight");
                if ((bool)player.Turn!) player.AbortAfterSeconds(32);
                else opponent.AbortAfterSeconds(32);
            }
        }

        public async Task Shoot(int x, int y)
        {
            if (environment.IsDevelopment())
            {
                Console.WriteLine($"{Context.ConnectionId} is at Shoot method");
            }

            var player = info.Get(Context.ConnectionId);

            var opponent = info.Get(player!.Opponent ?? "");
            if (opponent == null)
            {
                await Clients.Caller.SendAsync("Notify", "error;7");
                return;
            }

            if (player.Game.IsDead || opponent.Game.IsDead)
            {
                await Clients.Caller.SendAsync("Notify", "error;8");
                return;
            }

            if (!player.Game.IsReady || !opponent.Game.IsReady)
            {
                await Clients.Clients(player.Me, opponent.Me).SendAsync("Notify", "error;4");
                return;
            }

            if (!(bool)player.Turn!)
            {
                await Clients.Caller.SendAsync("Notify", "error;5");
                return;
            }

            int shoot = opponent.Game.Shoot(x, y);

            if(shoot == -2)
            {
                await Clients.Caller.SendAsync("Notify", "error;6");
                return;
            }

            player.CancelAbortion();

            if(shoot == 3)
            {
                await Clients.Caller.SendAsync("Notify", "destroyed");
                await Clients.Client(opponent.Me).SendAsync("Notify", $"destroyed;{x};{y}");
            }

            if(shoot == 1)
            {
                await Clients.Caller.SendAsync("Notify", "damaged");
                await Clients.Client(opponent.Me).SendAsync("Notify", $"damaged;{x};{y}");
            }

            if(player.Game.IsDead || opponent.Game.IsDead)
            {
                if (player.Game.IsDead)
                {
                    await Clients.Caller.SendAsync("Notify", "lose");
                    await Clients.Client(opponent.Me).SendAsync("Notify", "win");
                }
                else
                {
                    await Clients.Caller.SendAsync("Notify", "win");
                    await Clients.Client(opponent.Me).SendAsync("Notify", "lose");
                }
                player.Abort();
                return;
            }

            if (shoot == -1)
            {
                await Clients.Caller.SendAsync("Notify", "missed");
                await Clients.Client(opponent.Me).SendAsync("Notify", $"missed;{x};{y}");
                player.SwitchTurn();
                opponent.SwitchTurn();
                opponent.AbortAfterSeconds(32);
                return;
            }

            player.AbortAfterSeconds(32);
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
                if (!player.Game.IsReady || (!player.Game.IsDead && !opponent.Game.IsDead))
                {
                    await Clients.Client(opponent.Me).SendAsync("Notify", "win");
                }
                opponent.AbortAfterSeconds(3);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
