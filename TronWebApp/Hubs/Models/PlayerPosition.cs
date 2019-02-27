namespace TronWebApp.Hubs
{
    public class PlayerPosition
    {
        public PlayerPosition(int row, int col, PlayerDirection direction)
        {
            Row = row;
            Col = col;
            Direction = direction;
        }

        public int Row { get; }

        public int Col { get; }

        public PlayerDirection Direction { get; }
    }
}