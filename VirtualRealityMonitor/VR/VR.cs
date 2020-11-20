using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Valve.VR;
using VirtualRealityMonitor.Configuration;

namespace VirtualRealityMonitor
{
    public class VR
    {
        private CVRSystem _vr_system;
        private List<string> _requiredControllers;
        private Nullable<uint> _hmdIndex = null;
        private Dictionary<uint, string> _trackedControllerIndices;
        private Dictionary<uint, string> _trackedControllerIdentifiers;
        private Dictionary<string, string> _propertiesToTrack;
        private EVRApplicationType _applicationType;

        public bool isConnected { get; private set; } = false;
        public Dictionary<uint, string> Controllers {
            get {
                return _trackedControllerIndices;
            } 
        }

        public Dictionary<uint, string> ControllerIdentifiers
        {
            get
            {
                return _trackedControllerIdentifiers;
            }
        }

        public Dictionary<string, string> PropertiesToTrack
        {
            get
            {
                return _propertiesToTrack;
            }
        }

        public VR(EVRApplicationType applicationType, VRConfiguration configuration)
        {        
            _trackedControllerIndices = new Dictionary<uint, string>();
            _trackedControllerIdentifiers = new Dictionary<uint, string>();
            _requiredControllers = configuration.ControllerTypes;
            _applicationType = applicationType;
            _propertiesToTrack = configuration.PropertiesToTrack;
        }

        public void Connect()
        {
            var error = new EVRInitError();

            _vr_system = OpenVR.Init(ref error, _applicationType);

            if (error != 0)
            {
                throw new Exception(error.ToString());
            }

            isConnected = true;
            UpdateControllers();
        }

        public bool PollNextEvent()
        {
            var vrEvent = new VREvent_t();
            bool success = _vr_system.PollNextEvent(ref vrEvent, (uint)Marshal.SizeOf(typeof(VREvent_t)));

            switch ((EVREventType) vrEvent.eventType)
            {
                case EVREventType.VREvent_TrackedDeviceActivated:
                    UpdateControllers();
                    break;
                case EVREventType.VREvent_TrackedDeviceDeactivated:
                    UpdateControllers();
                    break;
                case EVREventType.VREvent_Quit:
                    Shutdown();
                    break;
            }

            return success;
        }
   
        public string GetTrackedDevicePropertyString(uint index, ETrackedDeviceProperty property)
        {
            var sb = new StringBuilder();
            var error = new ETrackedPropertyError();
            uint succeeded = _vr_system.GetStringTrackedDeviceProperty(index, property, sb, (uint)sb.Capacity, ref error);

            if (error == ETrackedPropertyError.TrackedProp_Success)
            {
                return sb.ToString();
            }
            return null;
        }

        public float GetTrackedDevicePropertyFloat(uint index, ETrackedDeviceProperty property)
        {
            var error = new ETrackedPropertyError();
            float value = _vr_system.GetFloatTrackedDeviceProperty(index, property, ref error);

            if (error == ETrackedPropertyError.TrackedProp_Success)
            {
                return value;
            }
            return -1;
        }

        private void UpdateControllers()
        {
            _trackedControllerIndices.Clear();
            _trackedControllerIdentifiers.Clear();

            for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
            {
                if (_vr_system.GetTrackedDeviceClass(i) == ETrackedDeviceClass.Controller)
                {
                    string type = GetTrackedDevicePropertyString(i, ETrackedDeviceProperty.Prop_ControllerType_String);
                    string identifier = GetTrackedDevicePropertyString(i, ETrackedDeviceProperty.Prop_SerialNumber_String);

                    if (type.Equals("hmd_controller"))
                    {
                        _hmdIndex = i;
                    }
                    else if (_requiredControllers.Contains(type))
                    {
                        _trackedControllerIndices.Add(i, type);
                        _trackedControllerIdentifiers.Add(i, identifier);
                    }
                }
            }
        }

        private void Shutdown()
        {
            Console.WriteLine("SteamVR connection lost.");
            _vr_system.AcknowledgeQuit_Exiting();
            OpenVR.Shutdown();
            isConnected = false;
        }
    }
}
