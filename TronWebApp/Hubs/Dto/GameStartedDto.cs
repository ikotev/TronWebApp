using System.Collections.Generic;

namespace TronWebApp.Hubs
{
    public class GameStartedDto
    {
        public List<PlayerDto> Players { get; set; }               
    }
}