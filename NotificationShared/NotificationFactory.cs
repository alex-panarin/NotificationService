using System;
using System.Text;
using System.Text.Json;

namespace NotificationShared
{

    public class NotificationFactory : INotificationFactory
    {
        public NotificationFactory()
        {

        }
        public NotificationPayload Create(Notifications notifications, string payload)
        {
            return new NotificationPayload
            {
                Notification = notifications,
                Payload = payload,
            };
        }
        public NotificationPayload Create(byte[] byteArray)
        {
            string payload = Encoding.UTF8.GetString(byteArray);
            
            if (!payload.Contains(":")) 
                throw new FormatException($"Payload format exception - {payload}");

            if (payload.Contains("\"")) // JSON string expected
            {
                return JsonSerializer.Deserialize<NotificationPayload>(payload); //JsonConvert.DeserializeObject<NotificationPayload>(payload);
            }

            return Create(Enum.Parse<Notifications>(payload.Split(':')[0]), payload.Split(':')[1]);
        }
    }
}
