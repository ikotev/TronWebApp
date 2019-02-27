using System.Collections.Generic;

namespace TronWebApp.Hubs
{
    public class GameLobby
    {
        public List<TronPlayer> Players { get; set; }

        public GameBoard Board { get; set; }

        public bool IsReady { get; set; }

        public GameLobby GetSafeClone()
        {
            var clone = (GameLobby)MemberwiseClone();
            clone.Players = new List<TronPlayer>(Players);

            return clone;
        }
    }
}