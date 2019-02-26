using System.Collections.Generic;

namespace TronWebApp.Hubs
{
    public class GameStartedDto
    {
        public IEnumerable<PlayerDto> Players { get; set; }               
    }
}