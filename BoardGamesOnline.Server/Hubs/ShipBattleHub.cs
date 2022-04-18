using BoardGamesOnline.Server.Games;
using Microsoft.AspNetCore.SignalR;

namespace BoardGamesOnline.Server.Hubs
{
    public class ShipBattleHub : Hub
    {
        private readonly ShipBattleQueue queue;
        private readonly ShipBattleInfo info;

        public ShipBattleHub(ShipBattleQueue queue, ShipBattleInfo info)
        {
            this.queue = queue;
            this.info = info;
        }

        public override async Task OnConnectedAsync()
        {
            if(queue.Count != 0)
            {
                string opponentId = queue.Item;
                var opponentGame = new ShipBattle();
                var playerGame = new ShipBattle();
                bool right = new Random().Next(1, 3) == 1;
                var player = new ShipBattlePlayer(Context.ConnectionId, opponentId, playerGame, right);
                var opponent = new ShipBattlePlayer(opponentId, Context.ConnectionId, opponentGame, !right);
                info.Add(player);
                info.Add(opponent);
                await Clients.Client(player.Me).SendAsync("Notify", $"game started;{player.Right}");
                await Clients.Client(opponent.Me).SendAsync("Notify", $"game started;{opponent.Right}");
            }
            else
            {
                queue.Item = Context.ConnectionId;
            }
            await base.OnConnectedAsync();
        }

        public async Task SetShips(int[,] ships)
        {
            var player = info.Get(Context.ConnectionId);
            if(player == null)
            {
                await Clients.Caller.SendAsync("Notify", "error;1");
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
            await Clients.Caller.SendAsync("Notify", "success");
        }

        public async Task Shoot(int x, int y)
        {
            var player = info.Get(Context.ConnectionId);

            if(player == null)
            {
                await Clients.Caller.SendAsync("Notify", "error;1");
                return;
            }

            var opponent = info.Get(player.Opponent);

            if (!player.Game.IsReady || !opponent!.Game.IsReady)
            {
                await Clients.Clients(player.Me, opponent!.Me).SendAsync("Notify", "error;4");
                return;
            }

            if (!player.Right)
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

            if(shoot == 3)
            {
                await Clients.Caller.SendAsync("Notify", "killed");
                await Clients.Client(opponent.Me).SendAsync("Notify", $"killed {x} {y}");
            }
            if(shoot == 1)
            {
                await Clients.Caller.SendAsync("Notify", "injured");
                await Clients.Client(opponent.Me).SendAsync("Notify", $"injured {x} {y}");
            }
            if(shoot == -1)
            {
                await Clients.Caller.SendAsync("Notify", "missed");
                await Clients.Client(opponent.Me).SendAsync("Notify", $"missed {x} {y}");
            }

            if(player.Game.IsDead || opponent.Game.IsDead)
            {
                await Clients.Caller.SendAsync("Notify", opponent.Game.IsDead ? "win" : "lose");
                await Clients.Client(opponent.Me).SendAsync("Notify", player.Game.IsDead ? "win" : "lose");
                info.Remove(player.Me);
                info.Remove(opponent.Me);
            }

            player.SwitchRight();
            opponent.SwitchRight();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var player = info.Get(Context.ConnectionId);
            if(player != null)
            {
                await Clients.Client(player.Opponent).SendAsync("Notify", "win");
                info.Remove(player.Opponent);
                info.Remove(Context.ConnectionId);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
