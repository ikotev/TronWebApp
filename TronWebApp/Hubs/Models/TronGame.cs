using System;
using System.Collections.Generic;

namespace TronWebApp.Hubs
{
    public class TronGame
    {
        public GameState State { get; set; }

        public string GroupName { get; set; }

        public DateTime TimeCreated { get; set; }

        public List<TronPlayer> Players { get; set; }

        public GameBoard Board { get; set; }
    }
}