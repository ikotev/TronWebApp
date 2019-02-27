namespace TronWebApp.Hubs
{
    public class GameBoard
    {
        public GameBoard(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
        }

        public int Rows { get; }

        public int Cols { get; }
    }
}