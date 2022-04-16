namespace BoardGamesOnline.Server.Games
{
    public class TickTackToePlayer
    {
        private string me;
        private string opponent;
        private TickTackToe game;
        private bool right;

        public TickTackToePlayer(string me, string opponent, TickTackToe game, bool right)
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
        public TickTackToe Game { get { return game; } }
        public bool Right { get { return right; } }
    }
}
