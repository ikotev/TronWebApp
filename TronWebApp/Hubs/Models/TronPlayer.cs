namespace TronWebApp.Hubs
{
    public class TronPlayer
    {
        public TronPlayer(string name, string key)
        {
            Name = name;
            Key = key;
        }

        public string Name { get; set; }

        public string Key { get; }
    }
}