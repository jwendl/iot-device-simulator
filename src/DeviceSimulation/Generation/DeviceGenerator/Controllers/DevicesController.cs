using DeviceGenerator.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace DeviceGenerator.Controllers
{
    [Route("devices")]
    public class DevicesController
        : Controller
    {
        private readonly IOptions<SimulationIoTHubOptions> simulationIoTHubOptions;
        private readonly IStorageService storageService;
        private readonly ISimulationManagerService simulationManagerService;
        private readonly ILogger<SimulationsController> logger;

        public DevicesController(IOptions<SimulationIoTHubOptions> simulationIoTHubOptions, IStorageService storageService, ISimulationManagerService simulationManagerService, ILogger<SimulationsController> logger)
        {
            this.simulationIoTHubOptions = simulationIoTHubOptions;
            this.storageService = storageService;
            this.simulationManagerService = simulationManagerService;
            this.logger = logger;
        }

        [HttpDelete()]
        public IActionResult DeleteDevices()
        {
#pragma warning disable CS4014 // Fire off the running of the simulation on a background thread and don't await that, instead we manually observe any possible exceptions using ContinueWith
            Task.Run(() =>
            {
                simulationManagerService.DeleteAllDevicesAsync(simulationIoTHubOptions.Value);
            }).ContinueWith(
                t =>
                {
                    logger.LogError(t.Exception, $"Exception thrown when running delete devices");
                },
                TaskContinuationOptions.OnlyOnFaulted);
#pragma warning restore CS4014

            return Accepted();
        }
    }
}
