using DeviceSimulation.Common.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeviceGenerator.Interfaces
{
    public interface ISimulationManagerService
    {
        Task RunSimulationAsync(string simulationId, string simulationName, IEnumerable<SimulationItem> simulations, SimulationIoTHubOptions options);

        Task DeleteAllDevicesAsync(SimulationIoTHubOptions simulationIoTHubOptions);

        Task StopSimulation(string simulationName);
    }
}
