using Microsoft.Extensions.Logging;
using NotificationShared;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NotificationService
{
    public class NotificationProcessor : INotificationProcessor
    {
        private readonly BlockingCollection<KeyValuePair<string, NotificationPayload>> _messageQueue = 
            new BlockingCollection<KeyValuePair<string, NotificationPayload>>();

        private CancellationTokenSource _tokenSource;
        private readonly INotificationRepository _repository;
        private readonly INotificationFactory _factory;
        private readonly ILogger<NotificationProcessor> _logger;

        public NotificationProcessor(
            INotificationRepository repository,
            INotificationFactory factory,
            ILogger<NotificationProcessor> logger)
        {
            _repository = repository;
            _factory = factory;
            _logger = logger;
        }
        public void Start(string address, int port, CancellationToken stoppingToken)
        {
            _tokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);

            Client = new UdpClient(new IPEndPoint(IPAddress.Parse(address), port)) { DontFragment = true };
            
            _logger.LogInformation($"SERVER STARTED AT {address} : {port}");

            ProcessSendMessage();
        }
        public async Task ReceiveAsync()
        {
            var result = await Client.ReceiveAsync();

            var endPoint = result.RemoteEndPoint.ToString();
            
            try
            {
                NotificationPayload payload = _factory.Create(result.Buffer);
                _logger.LogInformation($"Recieve Result: {endPoint} => {payload}");

                switch (payload.Notification)
                {
                    case Notifications.Error:
                        _logger.LogWarning($"Error from: {endPoint} => {payload}");
                        break;
                    case Notifications.Connect:
                        //Register new Session in storage
                        _repository.GetOrAddSession(endPoint);
                        break;
                    case Notifications.Disconnect:
                        //Remove session from storage
                        _repository.Remove(endPoint);
                        break;
                    case Notifications.Notify:
                        // Add message to the  queue and Start processing message => ProcessWork()
                        _messageQueue.TryAdd(new KeyValuePair<string, NotificationPayload>(endPoint, payload));
                        break;
                    default:
                        break;
                }
            }
            catch (JsonException x)
            {
                _logger.LogError(x, $"{nameof(JsonException)}");
                throw x;
            }
            catch (FormatException x)
            {
                _logger.LogError(x, $"{nameof(FormatException)}");
                throw x;
            }
            catch (InvalidOperationException x)
            {
                throw new InvalidOperationException($"Blocking operations exception - {nameof(ReceiveAsync)}", x);
            }
        }
        public void Stop()
        {
            Task.Run(async () =>
            {
                await SendMessageAsync(string.Empty, _factory.Create(Notifications.Disconnect, "CLOSE"));
                
            }).Wait();

            _tokenSource.Cancel();
            _tokenSource.Dispose();

            Client.Close();
            Client.Dispose();
        }

        private UdpClient Client { get; set; }

        private void ProcessSendMessage()
        {
            ThreadPool.QueueUserWorkItem(async _ =>
            {
                while (!_tokenSource.IsCancellationRequested)
                {
                    try
                    {
                        foreach (var messageItem in _messageQueue.GetConsumingEnumerable(_tokenSource.Token))
                        {
                            try
                            {
                                await SendMessageAsync(messageItem.Key, messageItem.Value);
                            }
                            catch (SocketException)
                            {
                                _repository.Remove(messageItem.Key);
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogWarning($"Operation was canceled");
                    }
                }
            });
        }

        private async Task SendMessageAsync(string endPoint, NotificationPayload message)
        {
            byte[] toSend = message.ToByteArray();

            foreach (var session in _repository.GetSessionWithoutProducer(endPoint)) //DONT send message by yourself
            {
                await Client.SendAsync(toSend, toSend.Length, session.EndPoint);
            }
        }
    }

}
