using System.Collections.Generic;

namespace TronWebApp.Hubs
{
    public class PlayerMatchmakingService
    {
        private static readonly List<PendingGame> PendingGames = new List<PendingGame>();
        private static readonly object PendingGamesLock = new object();

        public PendingGame TryFind(PendingGame game)
        {
            PendingGame pendingGame = null;

            lock (PendingGamesLock)
            {                
                var index = PendingGames.FindIndex(p => p.Board.Cols == game.Board.Cols && p.Board.Rows == game.Board.Rows);

                if (index > -1)
                {
                    pendingGame = PendingGames[index];
                    PendingGames.RemoveAt(index);
                }
                else
                {
                    PendingGames.Add(game);
                }
            }

            return pendingGame;
        }
    }
}