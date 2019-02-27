using System.Threading.Tasks;

namespace TronWebApp.Hubs
{
    public interface ITronGameClient
    {
        Task ReceivePlayerDirectionChanged(PlayerDirectionChangedDto dto);

        Task ReceiveGameFinished(GameFinishedDto dto);

        Task ReceiveGameStarted(GameStartedDto dto);
    }
}