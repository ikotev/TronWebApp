namespace TronWebApp.Hubs
{
    public class GameLobbyRequest
    {
        public TronPlayer Player { get; set; }

        public GameBoard PlayerBoard { get; set; }
    }
}