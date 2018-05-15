using DeviceSimulation.Common.Models;
using DeviceSimulator;

namespace DeviceGenerator.Models
{
    public class DeviceBlockInformation
    {
        public IDeviceSimulator DeviceSimulatorActor { get; set; }

        public DeviceSettings DeviceSettings { get; set; }
    }
}
