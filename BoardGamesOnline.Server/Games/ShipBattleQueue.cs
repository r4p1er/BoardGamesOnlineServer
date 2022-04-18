namespace BoardGamesOnline.Server.Games
{
    public class ShipBattleQueue
    {
        private Queue<string> queue;

        public ShipBattleQueue()
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

        public int Count { get { return queue.Count; } }
    }
}
