using System.Collections.Generic;
using System.Drawing;

namespace TronWebApp.Hubs
{
    public class GameStartedDto
    {
        public List<EnemyPlayerDto> Enemies { get; set; }

        public PlayerPositionDto Position { get; set; }        
    }
}