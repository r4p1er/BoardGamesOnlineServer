namespace BoardGamesOnline.Server.Games
{
    public class ShipBattlePlayer
    {
        private string me;
        private string opponent;
        private ShipBattle game;
        private bool right;

        public ShipBattlePlayer(string me, string opponent, ShipBattle game, bool right)
        {
            this.me = me;
            this.opponent = opponent;
            this.game = game;
            this.right = right;
        }

        public void SwitchRight()
        {
            right = !right;
        }

        public string Me { get { return me; } }
        public string Opponent { get { return opponent; } }
        public ShipBattle Game { get { return game; } }
        public bool Right { get { return right; } }
    }
}
