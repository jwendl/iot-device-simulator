using DeviceGenerator.Interfaces;
using DeviceSimulation.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeviceGenerator.Controllers
{
    [Route("simulations")]
    public class SimulationsController
        : Controller
    {
        private readonly IOptions<SimulationIoTHubOptions> simulationIoTHubOptions;
        private readonly IStorageService storageService;
        private readonly ISimulationManagerService simulationManagerService;
        private readonly ILogger<SimulationsController> logger;

        public SimulationsController(IOptions<SimulationIoTHubOptions> simulationIoTHubOptions, IStorageService storageService, ISimulationManagerService simulationManagerService, ILogger<SimulationsController> logger)
        {
            this.simulationIoTHubOptions = simulationIoTHubOptions;
            this.storageService = storageService;
            this.simulationManagerService = simulationManagerService;
            this.logger = logger;
        }

        [HttpPost("{simulationName}")]
        public async Task<IActionResult> StartSimulation(string simulationName)
        {
            // Load the simulation
            var simulations = new List<SimulationItem>();
            try
            {
                var json = await storageService.FetchFileAsync("run", simulationName);
                simulations = JsonConvert.DeserializeObject<List<SimulationItem>>(json);
            }
            catch (StorageException storageException)
            {
                ServiceEventSource.Current.Message(storageException.ToString());
                return NotFound($"Simulation file '{simulationName}' doesn't exist.");
            }

            var loadedDeviceTypeDetails = new Dictionary<string, (string simulationScript, string initialStateJson)>();
            foreach (var simulation in simulations)
            {
                // If we haven't loaded the details for this device type, do it now
                if (!loadedDeviceTypeDetails.TryGetValue(simulation.DeviceType, out (string simulationScript, string initialStateJson) deviceTypeDetails))
                {
                    deviceTypeDetails.simulationScript = await storageService.FetchFileAsync("scripts", $"{simulation.DeviceType}.cscript");
                    deviceTypeDetails.initialStateJson = await storageService.FetchFileAsync("state", $"{simulation.DeviceType}.json");

                    loadedDeviceTypeDetails.Add(simulation.DeviceType, deviceTypeDetails);
                }

                simulation.Script = deviceTypeDetails.simulationScript;
                simulation.InitialState = deviceTypeDetails.initialStateJson;
            }

            var simulationId = Guid.NewGuid().ToString();
            var simulationUniqueName = $"Simulation_{simulationId}";

#pragma warning disable CS4014 // Fire off the running of the simulation on a background thread and don't await that, instead we manually observe any possible exceptions using ContinueWith
            Task.Run(() =>
            {
                simulationManagerService.RunSimulationAsync(simulationId, simulationUniqueName, simulations, simulationIoTHubOptions.Value);
            }).ContinueWith(
                t =>
                {
                    logger.LogError(t.Exception, $"Exception thrown when running simulation {simulationUniqueName}");
                },
                TaskContinuationOptions.OnlyOnFaulted);
#pragma warning restore CS4014

            return Accepted(simulationUniqueName);
        }

        [HttpDelete("{simulationName}")]
        public IActionResult StopSimulation(string simulationName)
        {
            Task.Run(() =>
            {
                simulationManagerService.StopSimulation(simulationName);
            }).ContinueWith(
                t =>
                {
                    logger.LogError(t.Exception, $"Exception thrown when stopping simulation {simulationName}");
                },
                TaskContinuationOptions.OnlyOnFaulted);

            return Accepted();
        }
    }
}
