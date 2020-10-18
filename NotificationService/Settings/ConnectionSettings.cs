using NotificationShared;

namespace NotificationService.Settings
{

    public class ConnectionSettings : IConnectionSettings
    {
        public string Address { get; set; }
        public int Port { get; set; }
        public Protocol Protocol { get; set; }
        public int MaxConnections { get ; set ; }
    }
}
