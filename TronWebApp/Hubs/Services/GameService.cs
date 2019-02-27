using System;
using System.Collections.Generic;

namespace TronWebApp.Hubs
{
    public class GameService
    {
        private static readonly Dictionary<string, TronGame> Map =
            new Dictionary<string, TronGame>();
        private static readonly object MapLock = new object();

        public TronGame CreateNewGame(GameLobby gameLobby)
        {
            var groupName = Guid.NewGuid().ToString();

            var game = new TronGame
            {
                GroupName = groupName,
                State = GameState.Playing,
                TimeCreated = DateTime.UtcNow,
                Players = gameLobby.Players,
                Board = gameLobby.Board
            };

            lock (MapLock)
            {
                foreach (var player in game.Players)
                {
                    Map.Add(player.ConnectionId, game);
                }
            }           

            return game;
        }

        public TronGame RemoveGame(string connectionId)
        {
            TronGame game;

            lock (MapLock)
            {
                if (Map.TryGetValue(connectionId, out game))
                {
                    game.State = GameState.Finished;
                    game.TimeEnded = DateTime.UtcNow;

                    foreach (var player in game.Players)
                    {
                        Map.Remove(player.ConnectionId, out _);
                    }
                }
            }

            return game;
        }

        public TronGame GetGame(string connectionId)
        {
            TronGame game;

            lock (MapLock)
            {
                if (Map.TryGetValue(connectionId, out game))
                {
                    
                }
            }

            return game;
        }
    }
}