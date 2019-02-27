namespace TronWebApp.Hubs
{
    public class JoinLobbyRequest
    {
        public TronPlayer Player { get; set; }

        public GameBoard PlayerBoard { get; set; }
    }
}