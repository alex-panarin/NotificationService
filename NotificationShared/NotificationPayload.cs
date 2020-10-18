using System;
using System.Text;
using System.Text.Json;

namespace NotificationShared
{
    public enum Notifications
    {
        Error = 1,
        Connect,
        Disconnect,
        Notify
    }

    public class NotificationPayload 
    {
        public Notifications Notification { get; set; }
        public string Payload { get; set; }
        public string Message { get; set; }
        public override string ToString()
        {
            return $"{Notification}:{Message}@{Payload}";
        }
        public byte[] ToByteArray()
        {
            return Encoding.UTF8.GetBytes(ToString());
        }
        public string ToJsonString()
        {
            return JsonSerializer.Serialize<NotificationPayload>(this);//JsonConvert.SerializeObject(this);
        }
        public byte[] ToJsonByteArray()
        {
            return Encoding.UTF8.GetBytes(ToJsonString());
        }
    }

}
