using System.Collections.Generic;

namespace TronWebApp.Hubs
{
    public class MatchmakingLobby
    {
        public List<TronPlayer> Players { get; set; }

        public GameBoard Board { get; set; }

        public bool IsReady { get; set; }        
    }
}