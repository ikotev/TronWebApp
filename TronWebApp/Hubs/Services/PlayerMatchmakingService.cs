using System;
using System.Collections.Generic;
using System.Linq;

namespace TronWebApp.Hubs
{
    public class PlayerMatchmakingService
    {
        private static readonly List<MatchmakingLobby> GameLobbies = new List<MatchmakingLobby>();
        private static readonly object GameLobbiesLock = new object();

        public TronLobby JoinLobby(JoinLobbyRequest request)
        {
            TronLobby tronLobby;

            lock (GameLobbiesLock)
            {
                var index = GameLobbies.FindIndex(lobby => lobby.Board.Cols == request.PlayerBoard.Cols &&
                                                           lobby.Board.Rows == request.PlayerBoard.Rows &&
                                                           lobby.Players.All(p => p.Key != request.Player.Key));

                if (index > -1)
                {
                    var lobby = GameLobbies[index];
                    lobby.Players.Add(request.Player);
                    lobby.IsReady = lobby.Players.Count == 2;

                    if (lobby.IsReady)
                    {
                        GameLobbies.RemoveAt(index);
                    }

                    tronLobby = lobby.ToTronLobby();
                }
                else
                {
                    var lobby = new MatchmakingLobby
                    {
                        Players = new List<TronPlayer>
                        {
                            request.Player
                        },
                        Board = request.PlayerBoard
                    };

                    GameLobbies.Add(lobby);

                    tronLobby = lobby.ToTronLobby();
                }
            }

            return tronLobby;
        }
     
        public bool TryToLeaveLobby(string playerKey)
        {
            lock (GameLobbiesLock)
            {
                var index = GameLobbies.FindIndex(lobby => lobby.Players.Any(p => p.Key == playerKey));

                if (index > -1)
                {
                    var lobby = GameLobbies[index];
                    lobby.Players.RemoveAll(p => p.Key == playerKey);
                    lobby.IsReady = false;

                    if (lobby.Players.Count == 0)
                    {
                        GameLobbies.RemoveAt(index);
                    }                    

                    return true;
                }
            }
            
            return false;
        }
    }
}