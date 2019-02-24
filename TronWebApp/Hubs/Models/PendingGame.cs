namespace TronWebApp.Hubs
{
    public class PendingGame
    {
        public TronPlayer Player { get; set; }

        public GameBoard PlayerBoard { get; set; }
    }
}