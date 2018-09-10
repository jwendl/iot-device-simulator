using DeviceGenerator.Interfaces;
using DeviceGenerator.Models;
using DeviceSimulation.Common.Models;
using DeviceSimulator;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Query;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DeviceGenerator.Services
{
    internal sealed class SimulationManagerService
        : ISimulationManagerService
    {
        private readonly Uri deviceActorApplicationUri = new Uri("fabric:/DeviceSimulationActor.App/DeviceSimulatorActorService");

        public async Task RunSimulationAsync(string simulationId, string simulationName, IEnumerable<SimulationItem> simulationItems, SimulationIoTHubOptions simulationIoTHubOptions)
        {
            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
            var addDeviceBlock = new TransformBlock<DeviceBlockInformation, DeviceBlockInformation>(async (deviceBlockInformation) =>
            {
                await deviceBlockInformation.DeviceSimulatorActor.AddDeviceAsync(deviceBlockInformation.DeviceSettings.DeviceServiceSettings, CancellationToken.None);
                return deviceBlockInformation;
            }, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 4 });

            var createTwinBlock = new TransformBlock<DeviceBlockInformation, DeviceBlockInformation>(async (deviceBlockInformation) =>
            {
                await deviceBlockInformation.DeviceSimulatorActor.CreateDeviceTwinAsync(deviceBlockInformation.DeviceSettings.DeviceServiceSettings, CancellationToken.None);
                return deviceBlockInformation;
            });

            var connectToHubBlock = new TransformBlock<DeviceBlockInformation, DeviceBlockInformation>(async (deviceBlockInformation) =>
            {
                await deviceBlockInformation.DeviceSimulatorActor.ConnectToHubAsync(deviceBlockInformation.DeviceSettings, CancellationToken.None);
                return deviceBlockInformation;
            });

            ActionBlock<DeviceBlockInformation> sendEventBlock = null;
            sendEventBlock = new ActionBlock<DeviceBlockInformation>(async (deviceBlockInformation) =>
            {
                await deviceBlockInformation.DeviceSimulatorActor.SendEventAsync();
                await Task.Delay(deviceBlockInformation.DeviceSettings.DeviceServiceSettings.DeviceInterval);
            });

            addDeviceBlock.LinkTo(createTwinBlock, linkOptions);
            createTwinBlock.LinkTo(connectToHubBlock, linkOptions);
            connectToHubBlock.LinkTo(sendEventBlock, linkOptions);

            // Enumerate the simulations and begin to stand up device instances for each one
            foreach (var simulationItem in simulationItems)
            {
                // Begin producing 
                var deviceItems = Enumerable.Range(0, simulationItem.NumberOfDevices);
                foreach (var deviceIndex in deviceItems)
                {
                    // Generate a unique id for this device
                    var deviceNumber = deviceIndex.ToString("000000");
                    var deviceId = $"{simulationName}_{simulationItem.DeviceType}_{deviceNumber}";

                    var deviceSimulatorActor = ActorProxy.Create<IDeviceSimulator>(new ActorId(deviceId), deviceActorApplicationUri);
                    var deviceSettings = new DeviceSettings()
                    {
                        InitialStateJson = simulationItem.InitialState,
                        Script = simulationItem.Script,
                        MessageType = simulationItem.MessageType,
                        SimulationSettings = new SimulationSettings()
                        {
                            SimulationId = simulationId,
                            SimulationName = simulationName
                        },
                        Properties = simulationItem.Properties,
                        DeviceServiceSettings = new DeviceServiceSettings()
                        {
                            DeviceType = simulationItem.DeviceType,
                            DeviceName = deviceId,
                            IoTHubConnectionString = simulationIoTHubOptions.IotHubConnectionString,
                            IoTHubName = simulationIoTHubOptions.IoTHubName,
                            DeviceInterval = simulationItem.Interval,
                        }
                    };

                    await addDeviceBlock.SendAsync(new DeviceBlockInformation()
                    {
                        DeviceSimulatorActor = deviceSimulatorActor,
                        DeviceSettings = deviceSettings,
                    });
                }
            }

            // Signal that we've completed adding all the devices
            addDeviceBlock.Complete();

            // Wait for all the devices to be running their simulations
            await sendEventBlock.Completion;
        }

        public async Task DeleteAllDevicesAsync(SimulationIoTHubOptions simulationIoTHubOptions)
        {
            var deleteDevicesActor = ActorProxy.Create<IDeviceSimulator>(new ActorId(Guid.NewGuid().ToString()), deviceActorApplicationUri);
            var deviceServiceSettings = new DeviceServiceSettings()
            {
                IoTHubConnectionString = simulationIoTHubOptions.IotHubConnectionString,
                IoTHubName = simulationIoTHubOptions.IoTHubName,
            };

            await deleteDevicesActor.CleanDevicesAsync(deviceServiceSettings, CancellationToken.None);
        }

        public async Task StopSimulation(string simulationName)
        {
            var fabricClient = new FabricClient();
            var deleteTasks = new List<Task>();

            // Find all partitions for the device simulator actor application
            foreach (var partition in await fabricClient.QueryManager.GetPartitionListAsync(deviceActorApplicationUri))
            {
                var partitionInformation = ((Int64RangePartitionInformation)partition.PartitionInformation);

                // Get the rangs of possible partition key values in the partition
                var partitionKeyRange = CreateLongRange(partitionInformation.LowKey, partitionInformation.HighKey);

                foreach (var partitionKey in partitionKeyRange)
                {
                    // Get the actor service for this particular partition key
                    var actorService = ActorServiceProxy.Create(deviceActorApplicationUri, partitionKey);

                    var pagedResult = default(PagedResult<ActorInformation>);

                    // Page through all the actors in this partition and delete them concurrently
                    do
                    {
                        pagedResult = await actorService.GetActorsAsync(pagedResult?.ContinuationToken, CancellationToken.None);

                        foreach (var actorInformation in pagedResult.Items)
                        {
                            deleteTasks.Add(actorService.DeleteActorAsync(actorInformation.ActorId, CancellationToken.None));
                        }

                        // Wait for the actors in this page to be deleted
                        await Task.WhenAll(deleteTasks);
                    }
                    while (pagedResult.ContinuationToken != null);
                }

                deleteTasks.Clear();
            }

            IEnumerable<long> CreateLongRange(long start, long end)
            {
                for (var i = start; i <= end; i++)
                {
                    yield return i;
                }
            }
        }
    }
}
