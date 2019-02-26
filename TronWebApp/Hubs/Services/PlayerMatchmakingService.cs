using System.Collections.Generic;
using System.Linq;

namespace TronWebApp.Hubs
{
    public class PlayerMatchmakingService
    {
        private static readonly List<GameLobby> GameLobbies = new List<GameLobby>();
        private static readonly object GameLobbiesLock = new object();

        public GameLobby TryFind(GameLobbyRequest request)
        {
            GameLobby lobby = null;

            lock (GameLobbiesLock)
            {
                var index = GameLobbies
                    .FindIndex(pg => pg.Board.Cols == request.PlayerBoard.Cols &&
                                     pg.Board.Rows == request.PlayerBoard.Rows &&
                                     pg.Players.All(p => p.ConnectionId != request.Player.ConnectionId));

                if (index > -1)
                {
                    lobby = GameLobbies[index];
                    GameLobbies.RemoveAt(index);
                }
                else
                {
                    GameLobbies.Add(new GameLobby
                    {
                        Players = new List<TronPlayer>
                        {
                            request.Player
                        },
                        Board =  request.PlayerBoard
                    });
                }
            }

            lobby?.Players.Add(request.Player);

            return lobby;
        }
    }
}