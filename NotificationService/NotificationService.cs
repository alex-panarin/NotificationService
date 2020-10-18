using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificationService.Settings;
using NotificationShared;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NotificationService
{
    public class NotificationService : BackgroundService
    {
        private readonly ILogger<NotificationService> _logger;
        private readonly INotificationProcessor _processor;
        private readonly IConnectionSettings _settings;

        public NotificationService(
            ILogger<NotificationService> logger,
            INotificationProcessor processor,
            IConnectionSettings settings)
        {
            _logger = logger;
            _processor = processor;
            _settings = settings;

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _processor.Start(_settings.Address, _settings.Port, stoppingToken);
            }
            catch (Exception x)
            {
                _logger.LogError(x, "START");
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _processor.ReceiveAsync();
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
                catch(SocketException s)
                {
                    _logger.LogWarning(s, "EXECUTE");
                }
                catch(Exception x)
                {
                    _logger.LogError(x, "EXECUTE");
                }
            }
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _processor.Stop();

            return base.StopAsync(cancellationToken);
        }
    }
}
