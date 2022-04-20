namespace BoardGamesOnline.Server.Games
{
    public class ShipBattlePlayer : IDisposable
    {
        private string me;
        private string opponent;
        private ShipBattle? game;
        private bool? right;
        private CancellationTokenSource cancellationTokenSource;
        private Action abort;

        public ShipBattlePlayer(string me, Action abort)
        {
            this.me = me;
            this.opponent = string.Empty;
            this.game = null;
            this.right = null;
            this.abort = abort;
            this.cancellationTokenSource = new CancellationTokenSource();
        }

        public void StartTimeout(int seconds)
        {
            var token = cancellationTokenSource.Token;
            Task.Run(async () =>
                {
                    bool cancelled = false;
                    token.Register(() => cancelled = true);
                    await Task.Delay(seconds * 1000);
                    if (cancelled) return;
                    abort();
                }, token);
        }

        public void SwitchRight()
        {
            right = !right;
        }

        public void Dispose()
        {
            cancellationTokenSource.Dispose();
        }

        public string Me { get { return me; } }
        public string Opponent
        {
            get
            {
                return opponent;
            }
            set
            {
                opponent = value;
            }
        }
        public ShipBattle? Game
        {
            get
            {
                return game;
            }
            set
            {
                if(game == null) game = value;
            }
        }
        public bool? Right
        {
            get
            {
                return right;
            }
            set
            {
                if(right == null) right = value;
            }
        }

        public CancellationTokenSource TimeoutToken { get { return cancellationTokenSource; } }
    }
}
