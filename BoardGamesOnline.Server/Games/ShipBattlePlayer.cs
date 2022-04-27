namespace BoardGamesOnline.Server.Games
{
    public class ShipBattlePlayer : IDisposable
    {
        private string me;
        private string? opponent;
        private ShipBattle game;
        private bool? turn;
        private CancellationTokenSource cancellationTokenSource;
        private Action abort;

        public ShipBattlePlayer(string me, Action abort)
        {
            this.me = me;
            this.game = new ShipBattle();
            this.abort = abort;
            this.cancellationTokenSource = new CancellationTokenSource();
        }

        public void Abort()
        {
            abort();
        }

        public void AbortAfterSeconds(int seconds)
        {
            var token = cancellationTokenSource.Token;
            Task.Run(async () =>
            {
                bool cancelled = false;
                token.Register(() => cancelled = true);
                await Task.Delay(1000 * seconds);
                if (cancelled) return;
                Abort();
            }, token);
        }

        public void SwitchTurn()
        {
            turn = !turn;
        }

        public void Dispose()
        {
            cancellationTokenSource.Dispose();
        }

        public string Me { get { return me; } }
        public string? Opponent
        {
            get
            {
                return opponent;
            }
            set
            {
                if (opponent == null) opponent = value;
            }
        }
        public ShipBattle Game
        {
            get
            {
                return game;
            }
        }
        public bool? Turn
        {
            get
            {
                return turn;
            }
            set
            {
                if (turn == null) turn = value;
            }
        }

        public CancellationTokenSource AbortToken { get { return cancellationTokenSource; } }
    }
}
