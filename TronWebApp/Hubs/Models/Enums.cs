namespace TronWebApp.Hubs
{
    public enum GameState
    {
        None,        
        Pending,
        Playing,
        Finished        
    }

    public enum PlayerDirection
    {
        None,
        Left,
        Up,
        Right,
        Down
    }
}