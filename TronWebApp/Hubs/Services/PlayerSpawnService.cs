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

                positions.Add(new PlayerPosition
                {
                    Col = xCenter,
                    Row = board.Rows - 1,
                    Direction = PlayerDirection.Up

                });

                positions.Add(new PlayerPosition
                {
                    Col = xCenter,
                    Row = 0,
                    Direction = PlayerDirection.Down

                });
            }
            else
            {
                throw new NotSupportedException(nameof(playersCount));
            }            

            return positions;
        }
    }
}