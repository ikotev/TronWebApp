using System.Collections.Generic;

namespace TronWebApp.Hubs
{
    public class GameLobby
    {
        public List<TronPlayer> Players { get; set; }

        public GameBoard Board { get; set; }
    }
}