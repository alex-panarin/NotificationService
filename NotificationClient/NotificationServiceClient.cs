using Microsoft.Extensions.Logging;
using NotificationShared;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NotificationClient
{
    public class NotificationServiceClient 
        : INotificationServiceClient, 
          INotificationServiceClientAsync
    {   
        private readonly CancellationTokenSource _cancellation;
        private readonly INotificationFactory _factory;
        private readonly IConnectionSettings _settings;
        
        private Action<NotificationPayload> _recieveAction;
        private IPEndPoint _remoteEndPoint;

        private UdpClient Client { get; set; }
        public NotificationServiceClient(IConnectionSettings settings):
            this(new NotificationFactory(), settings)
        {
           
        }
        public NotificationServiceClient(
            INotificationFactory factory, 
            IConnectionSettings settings)
        {
            _cancellation = new CancellationTokenSource();
            _factory = factory;
            _settings = settings;
        }
                
        public void OpenConnection(Action<NotificationPayload> recieveAction)
        {
            Task.Run(() => OpenConnectionAsync(recieveAction)).Wait();
        }
        public async Task OpenConnectionAsync(Action<NotificationPayload> recieveAction)
        {
            try
            {
                _recieveAction = recieveAction;
                
                _remoteEndPoint = new IPEndPoint(IPAddress.Parse(_settings.Address), _settings.Port);

                CheckValidClient(_remoteEndPoint);

                await SendMessageAsync(Notifications.Connect); // SEND CONNECTED

                RecieveNotifications(); // Blocked operation
            }
            catch(Exception x)
            {
                if (!(x is SocketException))
                {
                    await SendMessageAsync(Notifications.Error, x.ToString()); // Errors occurred
                }

                recieveAction?.Invoke(_factory.Create(Notifications.Error, x.ToString()));

            }
            finally
            {
                await CloseConnectionAsync();
            }

        }
        public void CloseConnection()
        {
            Task.Run(() => CloseConnectionAsync()).Wait();
        }
        public async Task CloseConnectionAsync()
        {
            if (_cancellation.IsCancellationRequested) return;
                
            await SendMessageAsync(Notifications.Disconnect); // SEND DISCONNECTED

            _cancellation.Cancel();
            _cancellation.Dispose();

            if (Client == null) return;
            
            Client.Close();
            Client.Dispose();
        
        }
        public void SendMessage(string message)
        {
            Task.Run(() => SendMessageAsync(message));
        }
        public async Task SendMessageAsync(string message)
        {
            await SendMessageAsync(Notifications.Notify, message);
        }

        private void CheckValidClient(IPEndPoint remote)
        {
            int local = 0;
            
            var localEndPoint = new IPEndPoint(IPAddress.Any, remote.Port);

            while (true)
            {
                try
                {
                    if (local++ == _settings.MaxConnections) // Try connect numberConnections times
                        throw new SocketException((int)SocketError.TimedOut);

                    Client = new UdpClient(localEndPoint) { DontFragment = true };

                    return;
                }
                catch (SocketException x)
                {
                    if (x.SocketErrorCode == SocketError.AddressAlreadyInUse)
                    {
                        localEndPoint.Port += local;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
        private async Task SendMessageAsync(Notifications notifications)
        {
            await SendMessageAsync(notifications, string.Empty);
        }
        private async Task SendMessageAsync(Notifications notifications, string message)
        {
            NotificationPayload payload = _factory.Create(notifications, message);
            await SendMessageAsync(payload, true);
        }
        private async Task SendMessageAsync(NotificationPayload payload, bool sendToServer = true)
        {
            if (payload.Notification != Notifications.Notify || !sendToServer)
            {
                _recieveAction?.Invoke(payload);
            }

            if (payload.Notification != Notifications.Error && sendToServer)
            {
                if (Client == null) return;

                byte[] toSend = payload.ToByteArray();
                
                await Client.SendAsync(toSend, toSend.Length, _remoteEndPoint);
            }
        }
        private void RecieveNotifications()
        {
            ThreadPool.QueueUserWorkItem(async _ => 
            {
                try
                {
                    while (!_cancellation.IsCancellationRequested)
                    {

                        UdpReceiveResult result = await Client?.ReceiveAsync(); // Blocked operation
                        if (result == null)
                        {
                            _cancellation.Cancel(true);
                        }

                        _remoteEndPoint = result.RemoteEndPoint;

                        NotificationPayload payload = _factory.Create(result.Buffer);

                        switch (payload.Notification)
                        {
                            case Notifications.Error:
                            case Notifications.Connect:
                                break;
                            case Notifications.Disconnect:
                                await SendMessageAsync(Notifications.Error, "SERVER DISCONNECTED");
                                _cancellation.Cancel();
                                break;
                            case Notifications.Notify:
                                await SendMessageAsync(payload, false);
                                break;
                            default:
                                break;
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            });
        }
        
    }
}
