using Microsoft.AspNetCore.SignalR;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TronWebApp.Hubs
{
    public class TronGameHub : Hub<ITronGameClient>
    {
        private readonly PlayerMatchmakingService _playersMatchmakingService;
        private readonly PlayerSpawnService _playerSpawnService;

        private static readonly Dictionary<string, TronGame> PlayerGameMap =
            new Dictionary<string, TronGame>();
        private static readonly object PlayerGameMapLock = new object();

        public TronGameHub(PlayerMatchmakingService playersMatchmakingService,
            PlayerSpawnService playerSpawnService)
        {
            _playersMatchmakingService = playersMatchmakingService;
            _playerSpawnService = playerSpawnService;
        }

        public async Task FindGame(FindGameDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.PlayerName) || dto.PlayerBoard == null ||
                dto.PlayerBoard.Rows <= 0 || dto.PlayerBoard.Cols <= 0)
            {
                throw new ArgumentException(nameof(FindGameDto));
            }            
            
            var request = new GameLobbyRequest
            {
                PlayerBoard = dto.PlayerBoard.ToModel(),
                Player = new TronPlayer
                {
                    Name = dto.PlayerName,
                    ConnectionId = Context.ConnectionId
                }
            };
            var gameLobby = _playersMatchmakingService.TryFind(request);

            if (gameLobby != null)
            {               
                var game = await CreateNewGame(gameLobby);

                await StartGame(game);
            }
        }

        public async Task PlayerDirectionChanged(DirectionChangedDto dto)
        {
            if (dto == null || !Enum.IsDefined(typeof(PlayerDirection), dto.Direction))
            {
                throw new ArgumentException(nameof(FindGameDto));
            }

            var connectionId = Context.ConnectionId;
            TronGame game;

            lock (PlayerGameMapLock)
            {
                PlayerGameMap.TryGetValue(connectionId, out game);
            }

            if (game != null)
            {
                var playerName = game.Players.First(p => p.ConnectionId == connectionId).Name;

                await Clients.GroupExcept(game.GroupName, connectionId)
                    .ReceivePlayerDirectionChanged(new PlayerDirectionChangedDto
                    {
                        Direction = dto.Direction,
                        PlayerName = playerName
                    });
            }
        }

        public async Task FinishGame(GameFinishedDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.WinnerName))
            {
                throw new ArgumentException(nameof(FindGameDto));
            }

            var connectionId = Context.ConnectionId;
            TronGame game;

            lock (PlayerGameMapLock)
            {
                if (PlayerGameMap.TryGetValue(connectionId, out game))
                {
                    foreach (var player in game.Players)
                    {
                        PlayerGameMap.Remove(player.ConnectionId, out _);
                    }
                }
            }

            if (game != null)
            {
                await Clients.GroupExcept(game.GroupName, connectionId)
                    .ReceiveGameFinished(dto);

                foreach (var player in game.Players)
                {
                    await Groups.RemoveFromGroupAsync(player.ConnectionId, game.GroupName);
                }
            }
        }

        private async Task StartGame(TronGame game)
        {
            var players = game.Players;
            var positions = _playerSpawnService.GetPosition(players.Count, game.Board);
            var positionDtos = positions.ToDtos();

            var playerDtos = players.Zip(positionDtos, (player, position) => new PlayerDto
            {
                Name = player.Name,
                Position = position
            });

            var dto = new GameStartedDto
            {
                Players = playerDtos
            };

            await Clients.Group(game.GroupName).ReceiveStartGame(dto);
        }

        private async Task<TronGame> CreateNewGame(GameLobby gameLobby)
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

            lock (PlayerGameMapLock)
            {
                foreach (var player in game.Players)
                {
                    PlayerGameMap.Add(player.ConnectionId, game);
                }
            }

            foreach (var player in gameLobby.Players)
            {
                await Groups.AddToGroupAsync(player.ConnectionId, groupName);
            }

            return game;
        }
    }
}
