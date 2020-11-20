using System.Text;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;

namespace VirtualRealityMonitor
{
    public class MQTT
    {
        private IMqttClient _mqttClient;
        private IMqttClientOptions _mqttClientOptions;
        private string _baseTopic;

        public MQTT(string baseTopic, IMqttClientOptions options)
        {
            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();
            _mqttClientOptions = options;
            _baseTopic = baseTopic;
        }

        public void Connect()
        {
            _mqttClient.ConnectAsync(_mqttClientOptions).Wait();
        }

        public void PublishSensorData(string identifier, string message)
        {
            var mqttMessage = new MqttApplicationMessage();
            mqttMessage.Topic = _baseTopic + identifier;
            mqttMessage.Payload = Encoding.ASCII.GetBytes(message);
            mqttMessage.Retain = true;
            if (!_mqttClient.IsConnected)
            {
                Connect();
            }
            _mqttClient.PublishAsync(mqttMessage).Wait();

        }
    }
}
