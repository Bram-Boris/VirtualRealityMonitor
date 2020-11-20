using Microsoft.Extensions.Configuration;
using MQTTnet.Client.Options;
using System;
using System.Collections.Generic;
using Valve.VR;
using VirtualRealityMonitor.Configuration;

namespace VirtualRealityMonitor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var mqttConfiguration = configuration.GetSection(MQTTConfiguration.configurationSectionName).Get<MQTTConfiguration>();
            var vrConfiguration = configuration.GetSection(VRConfiguration.configurationSectionName).Get<VRConfiguration>();
            var sensorConfiguration = configuration.GetSection(SensorConfiguration.configurationSectionName).Get<SensorConfiguration>();

            VR vr = new VR(EVRApplicationType.VRApplication_Background, vrConfiguration);

            var options = new MqttClientOptionsBuilder()
               .WithClientId(mqttConfiguration.ClientId)
               .WithTcpServer(mqttConfiguration.Host, mqttConfiguration.Port)
               .WithCredentials(mqttConfiguration.Username, mqttConfiguration.Password)
               .WithKeepAlivePeriod(TimeSpan.FromSeconds(60))
               .WithCleanSession()
               .Build();

            MQTT mqtt = new MQTT(mqttConfiguration.BaseTopic, options);

            SensorMonitor monitor = new SensorMonitor(sensorConfiguration, vr, mqtt);
            monitor.StartMonitoring();
        }
    }
}
