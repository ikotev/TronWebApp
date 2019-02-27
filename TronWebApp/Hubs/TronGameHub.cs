using Microsoft.AspNetCore.SignalR;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace TronWebApp.Hubs
{
    public class TronGameHub : Hub<ITronGameClient>
    {
        private readonly PlayerMatchmakingService _playersMatchmakingService;
        private readonly PlayerSpawnService _playerSpawnService;
        private readonly GameService _gameService;
        
        public TronGameHub(PlayerMatchmakingService playersMatchmakingService,
            PlayerSpawnService playerSpawnService, GameService gameService)
        {
            _playersMatchmakingService = playersMatchmakingService;
            _playerSpawnService = playerSpawnService;
            _gameService = gameService;
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
                Player = new TronPlayer(dto.PlayerName, Context.ConnectionId)                
            };

            var gameLobby = _playersMatchmakingService.GetOrCreateLobby(request);

            if (gameLobby.IsReady)
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
            var game = _gameService.GetGame(connectionId);            
            
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
            var game = _gameService.RemoveGame(connectionId);            

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
            var game = _gameService.CreateNewGame(gameLobby);

            foreach (var player in game.Players)
            {
                await Groups.AddToGroupAsync(player.ConnectionId, game.GroupName);
            }

            return game;
        }
    }
}
