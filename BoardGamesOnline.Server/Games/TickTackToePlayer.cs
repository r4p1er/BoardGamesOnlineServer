namespace BoardGamesOnline.Server.Games
{
    public class TickTackToePlayer : IDisposable
    {
        private string me;
        private string? opponent;
        private TickTackToe? game;
        private bool? turn;
        private Action abort;
        private CancellationTokenSource cancellationTokenSource;

        public TickTackToePlayer(string me, Action abort)
        {
            this.me = me;
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

        public void CancelAbortion()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = new CancellationTokenSource();
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
        public TickTackToe? Game
        {
            get
            {
                return game;
            }
            set
            {
                if (game == null) game = value;
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
    }
}
