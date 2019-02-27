using System.Collections.Generic;
using System.Linq;

namespace TronWebApp.Hubs
{
    public static class ModelExtensions
    {
        public static GameBoard ToModel(this GameBoardDto dto)
        {
            return new GameBoard(dto.Rows, dto.Cols);
        }

        public static PlayerPositionDto ToDto(this PlayerPosition model)
        {
            return new PlayerPositionDto
            {
                Col = model.Col,
                Row = model.Row,
                Direction = model.Direction
            };
        }

        public static IEnumerable<PlayerPositionDto> ToDtos(this IEnumerable<PlayerPosition> models)
        {
            return models
                .Select(m => m.ToDto());
        }

        public static TronLobby ToTronLobby(this MatchmakingLobby matchmakingLobby)
        {
            return new TronLobby
            {
                Board = matchmakingLobby.Board,
                IsReady = matchmakingLobby.IsReady,
                Players = new List<TronPlayer>(matchmakingLobby.Players)
            };
        }

        public static GameFinishedDto ToGameFinishedDto(this FinishGameDto dto)
        {
            return new GameFinishedDto
            {
                WinnerName = dto.WinnerName
            };
        }
    }
}