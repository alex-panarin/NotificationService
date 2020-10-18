using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NotificationService
{
    public class NotificationSession
    {
        public NotificationSession(IPEndPoint endPoint)
        {
            EndPoint = endPoint;
            Key = endPoint.ToString();
        }

        public string Key { get; }
        public IPEndPoint EndPoint { get; }

    }
}
