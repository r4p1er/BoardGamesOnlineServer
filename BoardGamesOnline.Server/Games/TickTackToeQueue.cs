namespace BoardGamesOnline.Server.Games
{
    public class TickTackToeQueue
    {
        private Queue<string> queue;

        public TickTackToeQueue()
        {
            queue = new Queue<string>();
        }

        public string Item
        {
            get
            {
                return queue.Dequeue();
            }
            set
            {
                queue.Enqueue(value);
            }
        }

        public int Count
        {
            get
            {
                return queue.Count;
            }
        }
    }
}
