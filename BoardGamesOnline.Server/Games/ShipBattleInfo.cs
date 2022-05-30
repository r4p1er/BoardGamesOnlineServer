namespace BoardGamesOnline.Server.Games
{
    public class ShipBattleInfo
    {
        private Dictionary<string, ShipBattlePlayer> players;

        public ShipBattleInfo()
        {
            players = new Dictionary<string, ShipBattlePlayer>();
        }

        public ShipBattlePlayer? Get(string id)
        {
            return (players.ContainsKey(id) ? players[id] : null);
        }

        public void Add(ShipBattlePlayer player)
        {
            players.Add(player.Me, player);
        }

        public void Remove(string id)
        {
            players.Remove(id);
        }
    }
}
