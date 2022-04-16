namespace BoardGamesOnline.Server.Games
{
    public class TickTackToeInfo
    {
        private Dictionary<string, TickTackToePlayer> players;

        public TickTackToeInfo()
        {
            players = new Dictionary<string, TickTackToePlayer>();
        }

        public TickTackToePlayer? Get(string id)
        {
            return (players.ContainsKey(id) ? players[id] : null);
        }

        public void Add(TickTackToePlayer player)
        {
            players.Add(player.Me, player);
        }

        public void Remove(string id)
        {
            players.Remove(id);
        }
    }
}
