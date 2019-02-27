using System;
using System.Collections.Generic;

namespace TronWebApp.Hubs
{
    public class PlayerSpawnService
    {
        public List<PlayerPosition> GetPosition(int playersCount, GameBoard board)
        {
            var positions = new List<PlayerPosition>();

            if (playersCount == 2)
            {
                var xCenter = board.Cols / 2;

                positions.Add(new PlayerPosition(board.Rows - 1, xCenter, PlayerDirection.Up));
                positions.Add(new PlayerPosition(0, xCenter, PlayerDirection.Down));                
            }
            else
            {
                throw new NotSupportedException(nameof(playersCount));
            }            

            return positions;
        }
    }
}