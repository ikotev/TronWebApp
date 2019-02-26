using System.Collections.Generic;
using System.Linq;

namespace TronWebApp.Hubs
{
    public static class ModelExtensions
    {
        public static GameBoard ToModel(this GameBoardDto dto)
        {
            return new GameBoard
            {
                Cols = dto.Cols,
                Rows = dto.Rows
            };
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
    }
}