using System;
using System.Collections.Generic;
using System.Linq;

namespace TronWebApp.Hubs
{
    public class PlayerMatchmakingService
    {
        private static readonly List<GameLobby> GameLobbies = new List<GameLobby>();
        private static readonly object GameLobbiesLock = new object();

        public GameLobby GetOrCreateLobby(GameLobbyRequest request)
        {
            GameLobby lobby;

            lock (GameLobbiesLock)
            {
                var index = GameLobbies.FindIndex(FindLobbyPredicate(request));

                if (index > -1)
                {
                    lobby = GameLobbies[index];
                    lobby.Players.Add(request.Player);
                    lobby.IsReady = lobby.Players.Count == 2;

                    if (lobby.IsReady)
                    {
                        GameLobbies.RemoveAt(index);
                    }
                    else
                    {
                        lobby = lobby.GetSafeClone();
                    }

                }
                else
                {
                    lobby = new GameLobby
                    {
                        Players = new List<TronPlayer>
                        {
                            request.Player
                        },
                        Board = request.PlayerBoard
                    };

                    GameLobbies.Add(lobby);

                    lobby = lobby.GetSafeClone();
                }
            }

            return lobby;
        }

        private static Predicate<GameLobby> FindLobbyPredicate(GameLobbyRequest request)
        {
            return lobby => lobby.Board.Cols == request.PlayerBoard.Cols &&
                            lobby.Board.Rows == request.PlayerBoard.Rows &&
                            lobby.Players.All(p => p.ConnectionId != request.Player.ConnectionId);
        }

        public bool TryLeaveLobby(GameLobbyRequest request, out GameLobby lobby)
        {
            lock (GameLobbiesLock)
            {
                var index = GameLobbies.FindIndex(FindLobbyPredicate(request));

                if (index > -1)
                {
                    lobby = GameLobbies[index];
                    lobby.Players.RemoveAll(p => p.ConnectionId == request.Player.ConnectionId);
                    lobby.IsReady = false;

                    if (lobby.Players.Count == 0)
                    {
                        GameLobbies.RemoveAt(index);
                    }

                    lobby = lobby.GetSafeClone();

                    return true;
                }
            }

            lobby = null;
            return false;
        }
    }
}