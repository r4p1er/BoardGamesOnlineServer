namespace BoardGamesOnline.Server.Games
{
    public class ShipBattle
    {
        private int[,] field;
        private int battleshipCount;
        private int cruiserCount;
        private int destroyerCount;
        private int boatCount;
        private bool isReady;

        public ShipBattle()
        {
            field = new int[10, 10];
            battleshipCount = 0;
            cruiserCount = 0;
            destroyerCount = 0;
            boatCount = 0;
            isReady = false;
        }

        public int Shoot(int x, int y)
        {
            if (x < 0 || x > 10 || y < 0 || y > 10 || field[x, y] == 1 || field[x, y] == -1) return -2;

            field[x, y] -= 1;

            if(field[x, y] == 1)
            {
                bool up, down, right, left;

                try
                {
                    if (field[x - 1, y] == 1 || field[x - 1, y] == 2) up = true;
                    else up = false;
                }
                catch (IndexOutOfRangeException)
                {
                    up = false;
                }

                try
                {
                    if(field[x + 1, y] == 1 || field[x + 1, y] == 2) down = true;
                    else down = false;
                }
                catch (IndexOutOfRangeException)
                {
                    down = false;
                }

                try
                {
                    if(field[x, y + 1] == 1 || field[x, y + 1] == 2) right = true;
                    else right = false;
                }
                catch (IndexOutOfRangeException)
                {
                    right = false;
                }

                try
                {
                    if(field[x, y - 1] == 1 || field[x, y - 1] == 2) left = true;
                    else left = false;
                }
                catch (IndexOutOfRangeException)
                {
                    left = false;
                }

                bool isDeadUp = true, isDeadDown = true, isDeadLeft = true, isDeadRight = true;

                if (left)
                {
                    for (int i = y - 1; i >= 0 && field[x, i] != 0 && field[x, i] != -1; --i)
                    {
                        if (field[x, i] != 1) isDeadLeft = false;
                    }
                }

                if (right)
                {
                    for (int i = y + 1; i <= 10 && field[x, i] != 0 && field[x, i] != -1; ++i)
                    {
                        if (field[x, i] != 1) isDeadRight = false;
                    }
                }

                if (up)
                {
                    for(int i = x - 1; i >= 0 && field[i, y] != 0 && field[i, y] != -1; --i)
                    {
                        if (field[i, y] != 1) isDeadUp = false;
                    }
                }

                if (down)
                {
                    for (int i = x + 1; i <= 10 && field[i, y] != 0 && field[i, y] != -1; ++i)
                    {
                        if (field[i, y] != 1) isDeadDown = false;
                    }
                }

                if (((right || left) && isDeadRight && isDeadLeft) || ((up || down) && isDeadUp && isDeadDown) || (!right && !left && !up && !down)) return 3;
            }

            return field[x, y];
        }

        public bool SetShips(int[,] positions)
        {
            field = new int[10, 10];
            if(positions == null || positions.GetUpperBound(0) != 9 || positions.GetUpperBound(1) != 3) return false;
            for(int i = 0; i < 10; ++i)
            {
                int x1 = Math.Min(positions[i, 0], positions[i, 2]);
                int x2 = Math.Max(positions[i, 0], positions[i, 2]);
                int y1 = Math.Min(positions[i, 1], positions[i, 3]);
                int y2 = Math.Max(positions[i, 1], positions[i, 3]);

                if(x1 < 0 || x1 > 10 || y1 < 0 || y1 > 10 || x2 < 0 || x2 > 10 || y2 < 0 || y2 > 10) return false;

                int rx1 = FixR(x1 - 1), ry1 = FixR(y1 - 1), rx2 = FixR(x2 + 1), ry2 = FixR(y2 + 1);
                for(int n = rx1; n <= rx2; ++n)
                {
                    for(int m = ry1; m <= ry2; ++m)
                    {
                        if(field[n, m] != default(int)) return false;
                    }
                }

                if (x1 == x2 && y1 == y2)
                {
                    field[x1, y1] = 2;
                    ++boatCount;
                }
                else if (x1 == x2)
                {
                    int l = y2 - y1 + 1;
                    if (l > 4) return false;
                    if (l == 2) ++destroyerCount;
                    else if (l == 3) ++cruiserCount;
                    else ++battleshipCount;
                    for (int k = y1; k <= y2; ++k) field[x1, k] = 2;
                }
                else if (y1 == y2)
                {
                    int l = x2 - x1 + 1;
                    if (l > 4) return false;
                    if (l == 2) ++destroyerCount;
                    else if (l == 3) ++cruiserCount;
                    else ++battleshipCount;
                    for (int k = x1; k <= x2; ++k) field[k, y1] = 2;
                }
                else return false;
            }
            if (battleshipCount != 1 || cruiserCount != 2 || destroyerCount != 3 || boatCount != 4) return false;
            isReady = true;
            return true;
        }

        private int FixR(int r)
        {
            if (r > 10) return 10;
            if (r < 0) return 0;
            return r;
        }

        public bool IsDead
        {
            get
            {
                for(int i = 0; i < 10; ++i)
                {
                    for(int j = 0; j < 10; ++j)
                    {
                        if (field[i, j] == 2) return false;
                    }
                }
                return true;
            }
        }

        public bool IsReady { get { return isReady; } }
    }
}
