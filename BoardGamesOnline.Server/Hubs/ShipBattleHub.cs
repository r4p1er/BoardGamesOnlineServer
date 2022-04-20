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
                    var opponentGame = new ShipBattle();
                    var playerGame = new ShipBattle();
                    bool right = new Random().Next(1, 3) == 1;

                    player.Opponent = opponent.Me;
                    player.Right = right;
                    player.Game = playerGame;

                    opponent.Opponent = player.Me;
                    opponent.Right = !right;
                    opponent.Game = opponentGame;

                    await Clients.Client(player.Me).SendAsync("Notify", $"started;{player.Right}");
                    await Clients.Client(opponent.Me).SendAsync("Notify", $"started;{opponent.Right}");

                    player.StartTimeout(92);
                    opponent.StartTimeout(92);
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

        public async Task SetShips(int[,] ships)
        {
            if (environment.IsDevelopment())
            {
                Console.WriteLine($"{Context.ConnectionId} is at SetShips method");
            }

            var player = info.Get(Context.ConnectionId);

            if (player!.Game == null)
            {
                await Clients.Caller.SendAsync("Notify", "error;7");
                return;
            }

            if (player.Game.IsReady)
            {
                await Clients.Caller.SendAsync("Notify", "error;2");
                return;
            }

            bool isSuccessful = player.Game.SetShips(ships);
            if (!isSuccessful)
            {
                await Clients.Caller.SendAsync("Notify", "error;3");
                return;
            }

            player.TimeoutToken.Cancel();
            await Clients.Caller.SendAsync("Notify", "success");

            var opponent = info.Get(player.Opponent);
            if (opponent == null) return;

            if (opponent.Game!.IsReady)
            {
                if ((bool)player.Right!) player.StartTimeout(32);
                else opponent.StartTimeout(32);
            }
        }

        public async Task Shoot(int x, int y)
        {
            if (environment.IsDevelopment())
            {
                Console.WriteLine($"{Context.ConnectionId} is at Shoot method");
            }

            var player = info.Get(Context.ConnectionId);
            var opponent = info.Get(player!.Opponent);

            if (opponent == null)
            {
                await Clients.Caller.SendAsync("Notify", "error;7");
                return;
            }

            if (!player.Game!.IsReady || !opponent.Game!.IsReady)
            {
                await Clients.Clients(player.Me, opponent.Me).SendAsync("Notify", "error;4");
                return;
            }

            if (!(bool)player.Right!)
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

            player.TimeoutToken.Cancel();

            if(shoot == 3)
            {
                await Clients.Caller.SendAsync("Notify", "killed");
                await Clients.Client(opponent.Me).SendAsync("Notify", $"killed;{x};{y}");
            }
            if(shoot == 1)
            {
                await Clients.Caller.SendAsync("Notify", "injured");
                await Clients.Client(opponent.Me).SendAsync("Notify", $"injured;{x};{y}");
            }
            if(shoot == -1)
            {
                await Clients.Caller.SendAsync("Notify", "missed");
                await Clients.Client(opponent.Me).SendAsync("Notify", $"missed;{x};{y}");
            }

            if(player.Game.IsDead || opponent.Game.IsDead)
            {
                if (player.Game.IsDead)
                {
                    await Clients.Caller.SendAsync("lose");
                    player.StartTimeout(0);
                }
                else
                {
                    await Clients.Client(opponent.Me).SendAsync("lose");
                    opponent.StartTimeout(0);
                }
            }

            player.SwitchRight();
            opponent.SwitchRight();

            opponent.StartTimeout(32);
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
            if (player.Opponent != string.Empty)
            {
                await Clients.Client(player.Opponent).SendAsync("Notify", "win");
                var opponent = info.Get(player.Opponent);
                opponent!.Opponent = string.Empty;
                opponent.StartTimeout(0);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
