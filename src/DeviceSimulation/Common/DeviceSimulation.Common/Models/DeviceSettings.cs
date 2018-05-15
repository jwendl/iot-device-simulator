using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceSimulation.Common.Models
{
    public class DeviceSettings
    {
        public string InitialStateJson { get; set; }
        public string Script { get; set; }
        public string MessageType { get; set; }
        public DeviceServiceSettings DeviceServiceSettings { get; set; }
        public SimulationSettings SimulationSettings { get; set; }
        public Dictionary<string, string> Properties { get; set; }
    }
}
