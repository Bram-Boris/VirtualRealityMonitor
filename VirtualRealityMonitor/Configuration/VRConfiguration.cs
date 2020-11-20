using System.Collections.Generic;

namespace VirtualRealityMonitor.Configuration
{
    public class VRConfiguration
    {
        public static string configurationSectionName = "VR";
        public List<string> ControllerTypes { get; set; }
        public Dictionary<string,string> PropertiesToTrack { get; set; }
        public int Interval { get; set; }
    }
}
