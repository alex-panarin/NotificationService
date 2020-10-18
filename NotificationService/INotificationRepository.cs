using System.Collections.Generic;
using System.Net;

namespace NotificationService
{
    public interface INotificationRepository
    {
        NotificationSession GetOrAddSession(string endPoint);
        IEnumerable<NotificationSession> GetSessionWithoutProducer(string producerEndPoint);
        void Remove(string endPoint);
    }
}