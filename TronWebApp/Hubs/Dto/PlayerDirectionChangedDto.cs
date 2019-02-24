namespace TronWebApp.Hubs
{
    public class PlayerDirectionChangedDto
    {
        public string PlayerName { get; set; }

        public PlayerDirection Direction { get; set; }
    }
}