using NotificationClient;
using NotificationShared;
using System;
using System.Diagnostics;
using Xunit;

namespace NotificationClientTest
{
    public class NotificationClientTests
    {
        private NotificationServiceClient _client;

        public NotificationClientTests()
        {
            var factory = new NotificationFactory();
            _client = new NotificationServiceClient(factory);
        }

        
        [Fact]
        public void ShouldOpenConnection()
        {
            _client.OpenConnection(new Uri("http://127.0.0.1:2550"), (payload) =>
            {
                Assert.NotNull(payload);
                Assert.Equal(Notifications.Connect.ToString(), payload.Notification.ToString());

                Debug.WriteLine($"Reiecve Result => {payload.Notification} : {payload.Payload}");
            });
            
        }

    }
}
