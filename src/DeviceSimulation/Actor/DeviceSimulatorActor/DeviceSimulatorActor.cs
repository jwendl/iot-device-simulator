﻿using CodeEngine.CSharp.Interfaces;
using DeviceSimulation.Common.Models;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Newtonsoft.Json.Linq;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IotHubConnectionStringBuilder = Microsoft.Azure.Devices.IotHubConnectionStringBuilder;
using Message = Microsoft.Azure.Devices.Client.Message;

namespace DeviceSimulator
{
    [StatePersistence(StatePersistence.Volatile)]
    internal class DeviceSimulatorActor
        : Actor, IDeviceSimulator
    {
        private static readonly RetryPolicy DeviceServiceAddRetryPolicy = Policy
            .Handle<Exception>()
            .RetryAsync(5, (exception, retryCount, context) =>
            {
                DeviceSimulatorActorEventSource.Current.DeviceFailedAdd((DeviceSimulatorActor)context[nameof(DeviceSimulatorActor)], (DeviceServiceSettings)context[nameof(DeviceServiceSettings)], retryCount);
            });

        private static readonly RetryPolicy DeviceServiceTwinRetryPolicy = Policy
            .Handle<Exception>()
            .RetryAsync(5, (exception, retryCount, context) =>
            {
                DeviceSimulatorActorEventSource.Current.DeviceFailedTwin((DeviceSimulatorActor)context[nameof(DeviceSimulatorActor)], (DeviceServiceSettings)context[nameof(DeviceServiceSettings)], retryCount);
            });

        private static readonly RetryPolicy DeviceServiceConnectRetryPolicy = Policy
            .Handle<Exception>()
            .RetryAsync(5, (exception, retryCount, context) =>
            {
                DeviceSimulatorActorEventSource.Current.DeviceFailedConnection((DeviceSimulatorActor)context[nameof(DeviceSimulatorActor)], (DeviceSettings)context[nameof(DeviceSettings)], retryCount);
            });

        private static readonly ITransportSettings[] DeviceClientTransportSettings = new[]
        {
            new AmqpTransportSettings(
                Microsoft.Azure.Devices.Client.TransportType.Amqp_Tcp_Only,
                50, // NOTE: 50 is the default value according to source; completely unclear what this does (filed issues on GH)
                new AmqpConnectionPoolSettings
                {
                    Pooling = true, // Turn on connection pooling
                    MaxPoolSize = 100, // Each connection in the pool can handle 995 unique devices (according to source code), so this gets us 99,500 *possible* connections from a single node in the cluster
                })
        };

        private readonly IDeviceTypeScriptServiceCache<ICSharpService<string>> scriptServiceCache;
        private readonly Func<ICSharpService<string>> cSharpServiceFactory;
        private RegistryManager cachedRegistryManager;
        private DeviceClient cachedDeviceClient;

        public DeviceSimulatorActor(ActorService actorService, ActorId actorId, IDeviceTypeScriptServiceCache<ICSharpService<string>> scriptServiceCache, Func<ICSharpService<string>> cSharpServiceFactory)
            : base(actorService, actorId)
        {
            this.scriptServiceCache = scriptServiceCache;
            this.cSharpServiceFactory = cSharpServiceFactory;
        }

        protected override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();

            DeviceSimulatorActorEventSource.Current.ActorMessage(this, "Actor activated.");
        }

        public async Task AddDeviceAsync(DeviceServiceSettings deviceServiceSettings, CancellationToken cancellationToken)
        {
            var registryManager = BuildRegistryManager(deviceServiceSettings);

            var retryContext = new Context()
                {
                    { nameof(DeviceSimulatorActor), this },
                    { nameof(DeviceServiceSettings), deviceServiceSettings }
                };

            await DeviceServiceAddRetryPolicy.ExecuteAsync(
                async context =>
                {
                    var device = await registryManager.GetDeviceAsync(deviceServiceSettings.DeviceName);
                    if (device == null)
                    {
                        device = await registryManager.AddDeviceAsync(new Device(deviceServiceSettings.DeviceName));
                    }
                },
                retryContext);
        }

        public async Task CreateDeviceTwinAsync(DeviceServiceSettings deviceServiceSettings, CancellationToken cancellationToken)
        {
            var registryManager = BuildRegistryManager(deviceServiceSettings);
            var device = await registryManager.GetDeviceAsync(deviceServiceSettings.DeviceName);
            var twin = new Twin()
            {
                Tags = { ["IsSimulated"] = "Y" }
            };

            var retryContext = new Context()
                {
                    { nameof(DeviceSimulatorActor), this },
                    { nameof(DeviceServiceSettings), deviceServiceSettings }
                };

            await DeviceServiceTwinRetryPolicy.ExecuteAsync(
                async context =>
                {
                    await registryManager.UpdateTwinAsync(device.Id, twin, "*");
                },
                retryContext);
        }

        public async Task ConnectToHubAsync(DeviceSettings deviceSettings, CancellationToken cancellationToken)
        {
            if (deviceSettings == null)
            {
                throw new ArgumentNullException(nameof(deviceSettings));
            }

            await StateManager.AddStateAsync("DeviceStateJson", deviceSettings.InitialStateJson, cancellationToken);
            await StateManager.AddStateAsync(nameof(DeviceSettings), deviceSettings, cancellationToken);

            var deviceClient = await BuildDeviceClientAsync(deviceSettings.DeviceServiceSettings);
            string deviceType = deviceSettings.DeviceServiceSettings.DeviceType;

            if (!scriptServiceCache.TryGetScriptService(deviceType, out _))
            {
                var scriptService = cSharpServiceFactory();
                scriptService.Compile(deviceSettings.Script);

                scriptServiceCache.RegisterDeviceTypeScript(deviceType, scriptService);
            }

            var retryContext = new Context()
                {
                    { nameof(DeviceSimulatorActor), this },
                    { nameof(DeviceSettings), deviceSettings }
                };

            await DeviceServiceConnectRetryPolicy.ExecuteAsync(
                async context =>
                {
                    await deviceClient.OpenAsync();
                    DeviceSimulatorActorEventSource.Current.DeviceConnected(this, deviceSettings);
                },
                retryContext);

            DeviceSimulatorActorEventSource.Current.DeviceCreated(this, deviceSettings);
        }

        public async Task SendEventAsync()
        {
            var deviceSettings = await StateManager.GetStateAsync<DeviceSettings>(nameof(DeviceSettings));
            var deviceServiceSettings = deviceSettings.DeviceServiceSettings;

            // Simulate the next state for the device and record that new state for the next turn
            var nextStateJson = await BuildNextStateAsync();
            var deviceClient = await BuildDeviceClientAsync(deviceServiceSettings);

            try
            {
                DeviceSimulatorActorEventSource.Current.DeviceSendStart(this, deviceSettings);

                var jsonBytes = Encoding.UTF8.GetBytes(nextStateJson);
                var message = new Message(jsonBytes);

                IDictionary<string, string> messageProperties = message.Properties;
                messageProperties.Add("messageType", deviceSettings.MessageType);
                messageProperties.Add("correlationId", Guid.NewGuid().ToString());
                messageProperties.Add("parentCorrelationId", Guid.NewGuid().ToString());
                messageProperties.Add("createdDateTime", DateTime.UtcNow.ToString("u", DateTimeFormatInfo.InvariantInfo));
                messageProperties.Add("deviceId", deviceServiceSettings.DeviceName);

                var properties = deviceSettings.Properties;
                if (properties != null)
                {
                    foreach (var property in properties)
                    {
                        messageProperties.Add(property.Key, property.Value);
                    }
                }

                await deviceClient.SendEventAsync(message);
            }
            catch (Exception ex)
            {
                //TODO How to handle throttling exceptions
                DeviceSimulatorActorEventSource.Current.ExceptionOccured(ex.ToString());
                throw;
            }
            finally
            {
                DeviceSimulatorActorEventSource.Current.DeviceSendStop(this, deviceSettings);
            }
        }

        public async Task CleanDevicesAsync(DeviceServiceSettings deviceServiceSettings, CancellationToken cancellationToken)
        {
            var registryManager = BuildRegistryManager(deviceServiceSettings);
            var deviceQuery = registryManager.CreateQuery("select * from devices", 100);
            do
            {
                var deviceTwins = await deviceQuery.GetNextAsTwinAsync();
                var devices = new List<Device>();
                foreach (var deviceTwin in deviceTwins)
                {
                    devices.Add(new Device(deviceTwin.DeviceId)
                    {
                        ETag = deviceTwin.ETag,
                    });
                }

                var bulkRegistryOperationResult = await registryManager.RemoveDevices2Async(devices, true, cancellationToken);
                if (!bulkRegistryOperationResult.IsSuccessful)
                {
                    foreach (var error in bulkRegistryOperationResult.Errors)
                    {
                        DeviceSimulatorActorEventSource.Current.ExceptionOccured($"Error deleting device {error.DeviceId} because {error.ErrorCode} : {error.ErrorStatus}");
                    }
                }
            } while (deviceQuery.HasMoreResults);
        }

        private RegistryManager BuildRegistryManager(DeviceServiceSettings deviceServiceSettings)
        {
            if (cachedRegistryManager != null)
            {
                return cachedRegistryManager;
            }

            cachedRegistryManager = RegistryManager.CreateFromConnectionString(deviceServiceSettings.IoTHubConnectionString);
            return cachedRegistryManager;
        }

        private ValueTask<DeviceClient> BuildDeviceClientAsync(DeviceServiceSettings deviceServiceSettings)
        {
            if (cachedDeviceClient != null)
            {
                return new ValueTask<DeviceClient>(cachedDeviceClient);
            }

            return new ValueTask<DeviceClient>(CreateAndCacheDeviceClient());

            async Task<DeviceClient> CreateAndCacheDeviceClient()
            {
                var registryManager = RegistryManager.CreateFromConnectionString(deviceServiceSettings.IoTHubConnectionString);
                var device = await registryManager.GetDeviceAsync(deviceServiceSettings.DeviceName);
                if (device == null)
                {
                    var message = $"Device {deviceServiceSettings.DeviceName} is not registered!";
                    DeviceSimulatorActorEventSource.Current.ExceptionOccured(message);
                    throw new InvalidOperationException(message);
                }

                var deviceKeyInfo = new DeviceAuthenticationWithRegistrySymmetricKey(deviceServiceSettings.DeviceName, device.Authentication.SymmetricKey.PrimaryKey);
                var iotHubConnectionStringBuilder = IotHubConnectionStringBuilder.Create(deviceServiceSettings.IoTHubConnectionString);

                cachedDeviceClient = DeviceClient.Create(
                    iotHubConnectionStringBuilder.HostName,
                    deviceKeyInfo,
                    DeviceClientTransportSettings);

                return cachedDeviceClient;
            }
        }

        private async Task<string> BuildNextStateAsync()
        {
            var deviceSettings = await StateManager.GetStateAsync<DeviceSettings>(nameof(DeviceSettings));
            var deviceServiceSettings = deviceSettings.DeviceServiceSettings;

            var deviceStateJson = await StateManager.GetStateAsync<string>("DeviceStateJson");
            if (!scriptServiceCache.TryGetScriptService(deviceServiceSettings.DeviceType, out var scriptService))
            {
                throw new Exception($"Script service not registered for device type: {deviceServiceSettings.DeviceType}");
            }

            var newDeviceStateJson = await scriptService.ExecuteAsync(deviceStateJson);
            var newDeviceState = JObject.Parse(newDeviceStateJson);

            var deviceState = JObject.Parse(deviceStateJson);
            var previousState = deviceState.Property("previousState");

            if (previousState == null)
            {
                deviceState.Add("previousState", newDeviceState);
            }
            else
            {
                previousState.Value = newDeviceState;
            }

            deviceStateJson = deviceState.ToString();

            await StateManager.SetStateAsync("DeviceStateJson", deviceStateJson);

            return newDeviceStateJson;
        }
    }
}