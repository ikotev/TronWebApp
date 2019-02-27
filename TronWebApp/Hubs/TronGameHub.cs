using Microsoft.AspNetCore.SignalR;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace TronWebApp.Hubs
{
    public class TronGameHub : Hub<ITronGameClient>, ITronGameServer
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

            var request = new JoinLobbyRequest
            {
                PlayerBoard = dto.PlayerBoard.ToModel(),
                Player = new TronPlayer(dto.PlayerName, Context.ConnectionId)
            };

            var gameLobby = _playersMatchmakingService.JoinLobby(request);

            if (gameLobby.IsReady)
            {
                var game = await CreateNewGame(gameLobby);

                await StartGame(game);
            }
        }

        public async Task ChangePlayerDirection(ChangePlayerDirectionDto dto)
        {
            if (dto == null || !Enum.IsDefined(typeof(PlayerDirection), dto.Direction))
            {
                throw new ArgumentException(nameof(FindGameDto));
            }

            var connectionId = Context.ConnectionId;
            var game = _gameService.GetPlayerGame(connectionId);

            if (game != null)
            {
                var playerName = game.Players.First(p => p.Key == connectionId).Name;

                await Clients.GroupExcept(game.GroupName, connectionId)
                    .ReceivePlayerDirectionChanged(new PlayerDirectionChangedDto
                    {
                        Direction = dto.Direction,
                        PlayerName = playerName
                    });
            }
        }

        public async Task FinishGame(FinishGameDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentException(nameof(FindGameDto));
            }

            var connectionId = Context.ConnectionId;
            await FinishGame(connectionId, dto.ToGameFinishedDto());
        }        

        public async Task ForfeitGame(GameForfeitedDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.PlayerName))
            {
                throw new ArgumentException(nameof(FindGameDto));
            }
            
            await DisconnectPlayer(Context.ConnectionId);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await DisconnectPlayer(Context.ConnectionId);
        }

        private async Task DisconnectPlayer(string connectionId)
        {            
            if (!_playersMatchmakingService.TryToLeaveLobby(connectionId))
            {
                await FinishGame(connectionId, new GameFinishedDto());
            }            
        }

        private async Task FinishGame(string connectionId, GameFinishedDto dto)
        {
            var game = _gameService.DisbandPlayerGame(connectionId);

            if (game != null)
            {
                await Clients.GroupExcept(game.GroupName, connectionId)
                    .ReceiveGameFinished(dto);

                foreach (var player in game.Players)
                {
                    await Groups.RemoveFromGroupAsync(player.Key, game.GroupName);
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

            await Clients.Group(game.GroupName).ReceiveGameStarted(dto);
        }

        private async Task<TronGame> CreateNewGame(TronLobby tronLobby)
        {
            var game = _gameService.CreateNewGame(tronLobby);

            foreach (var player in game.Players)
            {
                await Groups.AddToGroupAsync(player.Key, game.GroupName);
            }

            return game;
        }
    }
}
