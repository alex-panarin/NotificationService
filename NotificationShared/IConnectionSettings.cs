namespace NotificationShared
{
    public enum Protocol
    {
        UDP, TCP, WS
    }
    public interface IConnectionSettings
    {
        string Address { get; set; }
        int Port { get; set; }
        Protocol Protocol { get; set; }
        int MaxConnections { get; set; }
    }
}