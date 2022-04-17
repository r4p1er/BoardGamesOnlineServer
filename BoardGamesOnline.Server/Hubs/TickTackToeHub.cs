using BoardGamesOnline.Server.Games;
using Microsoft.AspNetCore.SignalR;

namespace BoardGamesOnline.Server.Hubs
{
    public class TickTackToeHub : Hub
    {
        private readonly TickTackToeQueue queue;
        private readonly TickTackToeInfo info;

        public TickTackToeHub(TickTackToeQueue queue, TickTackToeInfo info)
        {
            this.queue = queue;
            this.info = info;
        }

        public override async Task OnConnectedAsync()
        {
            if(queue.Count != 0)
            {
                string opponentId = queue.Item;
                var game = new TickTackToe();
                bool right = (new Random().Next(1, 3) == 1 ? true : false);
                var player = new TickTackToePlayer(Context.ConnectionId, opponentId, game, right);
                var opponent = new TickTackToePlayer(opponentId, Context.ConnectionId, game, !right);
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

        public async Task Move(int x, int y)
        {
            var player = info.Get(Context.ConnectionId);
            if (player == null)
            {
                await Clients.Caller.SendAsync("Notify", "There is not such a player");
                return;
            }

            if(player.Right == false)
            {
                await Clients.Caller.SendAsync("Notify", "It is not your turn");
                return;
            }

            var isSuccessful = player.Game.Move(player.Me, x, y);
            if(isSuccessful == false)
            {
                await Clients.Caller.SendAsync("Notify", "Bad move");
                return;
            }

            await Clients.Caller.SendAsync("Notify", "Ok");
            player.SwitchRight();
            var opponent = info.Get(player.Opponent);
            opponent!.SwitchRight();
            await Clients.Client(opponent.Me).SendAsync("Notify", $"Opponent {x} {y}");

            if (player.Game.Winner != "progress")
            {
                if(player.Game.Winner == "tie")
                {
                    await Clients.Clients(player.Me, player.Opponent).SendAsync("Notify", "Tie");
                }
                else if(player.Game.Winner == player.Me)
                {
                    await Clients.Caller.SendAsync("Notify", "Win");
                    await Clients.Client(player.Opponent).SendAsync("Notify", "Lose");
                }
                else
                {
                    await Clients.Caller.SendAsync("Notify", "Lose");
                    await Clients.Client(player.Opponent).SendAsync("Notify", "Win");
                }

                info.Remove(player.Opponent);
                info.Remove(Context.ConnectionId);
                return;
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var player = info.Get(Context.ConnectionId);
            if(player != null)
            {
                await Clients.Client(player.Opponent).SendAsync("Notify", "Win");
                info.Remove(player.Opponent);
                info.Remove(Context.ConnectionId);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
