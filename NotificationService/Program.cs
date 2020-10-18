using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using NotificationService.Settings;
using NotificationShared;
using System;

namespace NotificationService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch(Exception x)
            {
                Console.WriteLine($"Unhandled exception: {x}");
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging => 
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddDebug();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<ConnectionSettings>(hostContext.Configuration.GetSection(nameof(ConnectionSettings)));

                    services
                        .AddSingleton<IConnectionSettings>(sp =>
                            sp.GetRequiredService<IOptions<ConnectionSettings>>().Value)
                     //   .AddLogging()
                        .AddSingleton<INotificationRepository, NotificationRepository>()
                        .AddSingleton<INotificationFactory, NotificationFactory>()
                        .AddSingleton<INotificationProcessor, NotificationProcessor>()
                        .AddHostedService<NotificationService>();
                    
                });
    }
}
