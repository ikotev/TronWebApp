using System;
using System.Collections.Generic;

namespace TronWebApp.Hubs
{
    public class GameService
    {
        private static readonly Dictionary<string, TronGame> Map =
            new Dictionary<string, TronGame>();
        private static readonly object MapLock = new object();

        public TronGame CreateNewGame(TronLobby tronLobby)
        {
            var groupName = Guid.NewGuid().ToString();

            var game = new TronGame
            {
                GroupName = groupName,
                State = GameState.Playing,
                TimeCreated = DateTime.UtcNow,
                Players = tronLobby.Players,
                Board = tronLobby.Board
            };

            lock (MapLock)
            {
                foreach (var player in game.Players)
                {
                    Map.Add(player.Key, game);
                }
            }           

            return game;
        }

        public TronGame DisbandPlayerGame(string playerKey)
        {
            TronGame game;

            lock (MapLock)
            {
                if (Map.TryGetValue(playerKey, out game))
                {
                    game.State = GameState.Finished;
                    game.TimeEnded = DateTime.UtcNow;

                    foreach (var player in game.Players)
                    {
                        Map.Remove(player.Key, out _);
                    }
                }
            }

            return game;
        }

        public TronGame GetPlayerGame(string playerKey)
        {
            TronGame game;

            lock (MapLock)
            {
                if (Map.TryGetValue(playerKey, out game))
                {
                    
                }
            }

            return game;
        }
    }
}