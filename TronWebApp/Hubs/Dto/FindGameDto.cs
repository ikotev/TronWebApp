namespace TronWebApp.Hubs
{
    public class FindGameDto
    {
        public string PlayerName { get; set; }

        public GameBoardDto PlayerBoard { get; set; }
    }
}