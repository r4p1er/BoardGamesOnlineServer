namespace BoardGamesOnline.Server.Games
{
    public class TickTackToe
    {
        private string[,] field;
        private int movesCount;

        public TickTackToe()
        {
            field = new string[3, 3];
            movesCount = 0;
        }

        public bool Move(string name, int x, int y)
        {
            if(x < 0 || x > 2 || y < 0 || y > 2 || movesCount > 8) return false;
            field[x, y] = name;
            ++movesCount;
            return true;
        }

        public string Winner
        {
            get
            {
                if (movesCount < 9) return "progress";
                else if (field[0, 0] == field[0, 1] && field[0, 1] == field[0, 2]) return field[0, 0];
                else if (field[1, 0] == field[1, 1] && field[1, 1] == field[1, 2]) return field[1, 0];
                else if (field[2, 0] == field[2, 1] && field[2, 1] == field[2, 2]) return field[2, 0];
                else if (field[0, 0] == field[1, 0] && field[1, 0] == field[2, 0]) return field[0, 0];
                else if (field[0, 1] == field[1, 1] && field[1, 1] == field[2, 1]) return field[0, 1];
                else if (field[0, 2] == field[1, 2] && field[1, 2] == field[2, 2]) return field[0, 2];
                else if (field[0, 0] == field[1, 1] && field[1, 1] == field[2, 2]) return field[0, 0];
                else if (field[0, 2] == field[1, 1] && field[1, 1] == field[2, 0]) return field[0, 2];
                else return "tie";
            }
        }
    }
}
