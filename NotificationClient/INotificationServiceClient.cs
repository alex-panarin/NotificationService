using NotificationShared;
using System;
using System.Threading.Tasks;

namespace NotificationClient
{    
    public interface INotificationServiceClient 
    {
        void CloseConnection();
        void OpenConnection(Action<NotificationPayload> recieveAction);
        void SendMessage(string message);
    }

    public interface INotificationServiceClientAsync 
    {
        Task CloseConnectionAsync();
        Task OpenConnectionAsync(Action<NotificationPayload> recieveAction);
        Task SendMessageAsync(string message);
    }
}