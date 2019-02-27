using System.Threading.Tasks;

namespace TronWebApp.Hubs
{
    public interface ITronGameServer
    {
        Task FindGame(FindGameDto dto);

        Task ChangePlayerDirection(ChangePlayerDirectionDto dto);

        Task FinishGame(FinishGameDto dto);

        Task ForfeitGame(GameForfeitedDto dto);
    }
}