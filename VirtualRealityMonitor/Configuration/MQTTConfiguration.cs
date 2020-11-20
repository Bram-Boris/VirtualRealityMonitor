namespace VirtualRealityMonitor.Configuration
{
    public class MQTTConfiguration
    {
        public static string configurationSectionName = "MQTT";

        public string Host { get; set; }
        public int Port { get; set; }
        public string ClientId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string BaseTopic { get; set; }
    }
}
