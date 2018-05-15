namespace DeviceSimulation.Common.Models
{
    public class DeviceServiceSettings
    {
        public string IoTHubConnectionString { get; set; }
        public string IoTHubName { get; set; }
        public string DeviceName { get; set; }
        public string DeviceType { get; set; }
        public int DeviceInterval { get; set; }
    }
}
