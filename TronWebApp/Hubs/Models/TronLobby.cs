using System.Collections.Generic;

namespace TronWebApp.Hubs
{
    public class TronLobby
    {
        public IReadOnlyList<TronPlayer> Players { get; set; }

        public GameBoard Board { get; set; }

        public bool IsReady { get; set; }        
    }
}