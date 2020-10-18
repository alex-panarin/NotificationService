using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace NotificationService
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly ConcurrentDictionary<string, NotificationSession> _sessionStorage = new ConcurrentDictionary<string, NotificationSession>();
        public NotificationRepository()
        {

        }
        public IEnumerable<NotificationSession> GetSessionWithoutProducer(string producerKey = "")
        {
            foreach (var session in _sessionStorage.ToArray().Where(e => e.Key != producerKey))
            {
                yield return session.Value;
            }
        }

        public NotificationSession GetOrAddSession(string sessionKey)
        {
            return _sessionStorage.GetOrAdd(
                sessionKey,
                k => new NotificationSession(IPEndPoint.Parse(sessionKey)));
        }

        public void Remove(string key)
        {
            _sessionStorage.TryRemove(key, out NotificationSession s);
        }
    }
}
