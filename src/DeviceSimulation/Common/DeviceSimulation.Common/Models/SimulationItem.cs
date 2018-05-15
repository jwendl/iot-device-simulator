using DeviceSimulation.Common.Enums;
using System;
using System.Collections.Generic;

namespace DeviceSimulation.Common.Models
{
    public class SimulationItem
    {
        /// <summary>
        /// A unique identifier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The base name prefix of the device.
        /// </summary>
        public string DevicePrefix { get; set; }

        /// <summary>
        /// The device type like Truck.
        /// </summary>
        public string DeviceType { get; set; }

        /// <summary>
        /// Specifies the type of message being sent
        /// </summary>
        public string MessageType { get; set; }

        /// <summary>
        /// The contents of the json initial state file.
        /// </summary>
        public string InitialState { get; set; }

        /// <summary>
        /// The contents of the script file.
        /// </summary>
        public string Script { get; set; }

        /// <summary>
        /// The language of the script file.
        /// </summary>
        public ScriptLanguage ScriptLanguage { get; set; }

        /// <summary>
        /// This is the time in seconds.
        /// </summary>
        public int Interval { get; set; }

        /// <summary>
        /// The total number of devices to simulate
        /// </summary>
        public int NumberOfDevices { get; set; }

        /// <summary>
        /// Communication protocol to use from Microsoft.Azure.Devices.Client.TransportType;
        /// </summary>
        public string DeviceTransportType { get; set; }

        /// <summary>
        /// application properties of device-to-cloud messages
        /// </summary>
        public Dictionary<string, string> Properties { get; set; }
    }
}
