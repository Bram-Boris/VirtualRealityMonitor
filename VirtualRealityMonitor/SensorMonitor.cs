using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using Valve.VR;
using VirtualRealityMonitor.Configuration;

namespace VirtualRealityMonitor
{
    class SensorMonitor
    {
        private VR _vr;
        private MQTT _mqtt;
        private SensorConfiguration _configuration;

        public SensorMonitor(SensorConfiguration configuration, VR vr, MQTT mqtt)
        {
            _configuration = configuration;
            _vr = vr;
            _mqtt = mqtt;
        }

        public void StartMonitoring()
        {
            while (true)
            {
                while (!_vr.isConnected)
                {
                    try
                    {
                        _vr.Connect();
                        _mqtt.Connect();
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine("Could not connect to OpenVR: " + e.Message);
                    }
                    Thread.Sleep(5000);
                }

                Timer timer = new Timer(UpdateSensors, null, 0, _configuration.Interval * 1000);
                while (_vr.isConnected)
                {
                    _vr.PollNextEvent();
                }
                timer.Dispose();
            }
        }

        private void UpdateSensors(object sender)
        {
            foreach (var controller in _vr.ControllerIdentifiers)
            {
                Dictionary<string, string> sensorData = new Dictionary<string, string>();
                foreach(var property in _vr.PropertiesToTrack)
                {
                    object value = null;
                    ETrackedDeviceProperty enumProperty = (ETrackedDeviceProperty)int.Parse(property.Key);
                    string sensorName = enumProperty.ToString();
                    if (property.Value.Equals("float"))
                    {
                        // Remove Prop_ and _Float from sensor name.
                        sensorName = sensorName.Substring(5, sensorName.Length - (5 + 6));
                        value = _vr.GetTrackedDevicePropertyFloat(controller.Key, enumProperty);
                    }
                    sensorData.Add(sensorName, value.ToString());
                }

                _mqtt.PublishSensorData(controller.Value, JsonSerializer.Serialize(sensorData));
            }
        }
    }
}
