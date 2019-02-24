namespace TronWebApp.Hubs
{
    public class PlayerPositionDto
    {
        public int Row { get; set; }

        public int Col { get; set; }

        public PlayerDirection Direction { get; set; }
    }
}