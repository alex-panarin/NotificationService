using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NotificationService
{
    public interface INotificationProcessor
    {
        void Start(string address, int port, CancellationToken stoppingToken);
        void Stop();
        Task ReceiveAsync();

    }
}