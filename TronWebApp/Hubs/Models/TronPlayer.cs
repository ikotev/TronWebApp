namespace TronWebApp.Hubs
{
    public class TronPlayer
    {
        public TronPlayer(string name, string connectionId)
        {
            Name = name;
            ConnectionId = connectionId;
        }

        public string Name { get; set; }

        public string ConnectionId { get; }
    }
}