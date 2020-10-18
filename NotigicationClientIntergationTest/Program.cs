using NotificationClient;
using NotificationShared;
using System;

namespace NotigicationClientIntergationTest
{
    class Program
    {
        class ConnectionSettings : IConnectionSettings
        {
            public string Address { get => "46.229.214.22"; set => throw new NotImplementedException(); }
            public int Port { get => 2550; set => throw new NotImplementedException(); }
            public Protocol Protocol { get => Protocol.UDP; set => throw new NotImplementedException(); }
            public int MaxConnections { get => 10; set => throw new NotImplementedException(); }
        }

        static void Main(string[] args)
        {
            var client = new NotificationServiceClient( new ConnectionSettings());

            client.OpenConnection(payload =>
            {
                Console.WriteLine($"Recieve Result: {payload}");
            });

            do
            {
                var message = Console.ReadLine();

                client.SendMessage(message);
            }
            while (Console.KeyAvailable);

            client.CloseConnection();

        }
    }
}
