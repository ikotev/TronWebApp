using System.Threading.Tasks;

namespace TronWebApp.Hubs
{
    public interface ITronGameClient
    {
        Task PlayerDirectionChanged(PlayerDirectionChangedDto dto);

        Task GameFinished(GameFinishedDto dto);

        Task GameStarted(GameStartedDto dto);
    }
}