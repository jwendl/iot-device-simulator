using DeviceSimulation.Common.Models;
using Microsoft.ServiceFabric.Actors.Runtime;
using System;
using System.Diagnostics.Tracing;

namespace DeviceSimulator
{
    [EventSource(Name = "Microsoft-IoTDeviceSimulator-DeviceSimulator-Actor")]
    public sealed class DeviceSimulatorActorEventSource
        : EventSource
    {
        public static readonly DeviceSimulatorActorEventSource Current = new DeviceSimulatorActorEventSource();

        private DeviceSimulatorActorEventSource() : base() { }

        #region Keywords
        // Event keywords can be used to categorize events. 
        // Each keyword is a bit flag. A single event can be associated with multiple keywords (via EventAttribute.Keywords property).
        // Keywords must be defined as a public class named 'Keywords' inside EventSource that uses them.
        public static class Keywords
        {
            public const EventKeywords HostInitialization = (EventKeywords)0x1L;
        }
        #endregion

        [NonEvent]
        public void DeviceConnected(Actor actor, DeviceSettings deviceInfo)
        {
            DeviceConnected(actor.ActorService.Context.ServiceName.ToString(),
                actor.ActorService.Context.ServiceTypeName,
                actor.ActorService.Context.ReplicaId,
                actor.ActorService.Context.PartitionId,
                actor.ActorService.Context.CodePackageActivationContext.ApplicationName,
                actor.ActorService.Context.CodePackageActivationContext.ApplicationTypeName,
                actor.ActorService.Context.NodeContext.NodeName,
                deviceInfo.SimulationSettings.SimulationId,
                deviceInfo.SimulationSettings.SimulationName,
                deviceInfo.DeviceServiceSettings.DeviceName,
                deviceInfo.DeviceServiceSettings.DeviceType);
        }

        private const int DeviceConnectedEventId = 100;
        [Event(DeviceConnectedEventId, Level = EventLevel.Informational, Message = "Device connected with id: {7}")]
        private void DeviceConnected(string serviceName, string serviceTypeName, long replicaId, Guid partitionId,
            string applicationName, string applicationTypeName, string nodeName, string simulationId,
            string simulationName, string deviceName, string deviceType)
        {
            if (this.IsEnabled())
            {
                WriteEvent(DeviceConnectedEventId,
                    serviceName,
                    serviceTypeName,
                    replicaId,
                    partitionId,
                    applicationName,
                    applicationTypeName,
                    nodeName,
                    simulationId,
                    simulationName,
                    deviceName,
                    deviceType);
            }
        }

        [NonEvent]
        public void DeviceSendStart(Actor actor, DeviceSettings deviceInfo)
        {
            DeviceSendStart(actor.ActorService.Context.ServiceName.ToString(),
                actor.ActorService.Context.ServiceTypeName,
                actor.ActorService.Context.ReplicaId,
                actor.ActorService.Context.PartitionId,
                actor.ActorService.Context.CodePackageActivationContext.ApplicationName,
                actor.ActorService.Context.CodePackageActivationContext.ApplicationTypeName,
                actor.ActorService.Context.NodeContext.NodeName,
                deviceInfo.SimulationSettings.SimulationId,
                deviceInfo.SimulationSettings.SimulationName,
                deviceInfo.DeviceServiceSettings.DeviceName,
                deviceInfo.DeviceServiceSettings.DeviceType);
        }

        private const int DeviceSendStartId = 101;
        [Event(DeviceSendStartId, Level = EventLevel.Informational, Message = "Device send with id: {7}")]
        private void DeviceSendStart(string serviceName, string serviceTypeName, long replicaId, Guid partitionId, string applicationName, string applicationTypeName, string nodeName, string simulationId, string simulationName, string deviceName, string deviceType)
        {
            if (this.IsEnabled())
            {
                WriteEvent(DeviceSendStartId,
                    serviceName,
                    serviceTypeName,
                    replicaId,
                    partitionId,
                    applicationName,
                    applicationTypeName,
                    nodeName,
                    simulationId,
                    simulationName,
                    deviceName,
                    deviceType);
            }
        }

        [NonEvent]
        public void DeviceSendStop(Actor actor, DeviceSettings deviceInfo)
        {
            DeviceSendStop(actor.ActorService.Context.ServiceName.ToString(),
                actor.ActorService.Context.ServiceTypeName,
                actor.ActorService.Context.ReplicaId,
                actor.ActorService.Context.PartitionId,
                actor.ActorService.Context.CodePackageActivationContext.ApplicationName,
                actor.ActorService.Context.CodePackageActivationContext.ApplicationTypeName,
                actor.ActorService.Context.NodeContext.NodeName,
                deviceInfo.SimulationSettings.SimulationId,
                deviceInfo.SimulationSettings.SimulationName,
                deviceInfo.DeviceServiceSettings.DeviceName,
                deviceInfo.DeviceServiceSettings.DeviceType);
        }

        private const int DeviceSendStopId = 102;
        [Event(DeviceSendStopId, Level = EventLevel.Informational, Message = "Device send end with id: {7}")]
        public void DeviceSendStop(string serviceName, string serviceTypeName, long replicaId, Guid partitionId, string applicationName, string applicationTypeName, string nodeName, string simulationId, string simulationName, string deviceName, string deviceType)
        {
            if (this.IsEnabled())
            {
                WriteEvent(DeviceSendStopId,
                    serviceName,
                    serviceTypeName,
                    replicaId,
                    partitionId,
                    applicationName,
                    applicationTypeName,
                    nodeName,
                    simulationId,
                    simulationName,
                    deviceName,
                    deviceType);
            }
        }

        [NonEvent]
        public void DeviceCreated(Actor actor, DeviceSettings deviceInfo)
        {
            DeviceCreated(actor.ActorService.Context.ServiceName.ToString(),
                actor.ActorService.Context.ServiceTypeName,
                actor.ActorService.Context.ReplicaId,
                actor.ActorService.Context.PartitionId,
                actor.ActorService.Context.CodePackageActivationContext.ApplicationName,
                actor.ActorService.Context.CodePackageActivationContext.ApplicationTypeName,
                actor.ActorService.Context.NodeContext.NodeName,
                deviceInfo.SimulationSettings.SimulationId,
                deviceInfo.SimulationSettings.SimulationName,
                deviceInfo.DeviceServiceSettings.DeviceName,
                deviceInfo.DeviceServiceSettings.DeviceType);
        }

        private const int DeviceCreatedEventId = 103;
        [Event(DeviceCreatedEventId, Level = EventLevel.Informational, Message = "Device created with id: {7}")]
        private void DeviceCreated(string serviceName, string serviceTypeName, long replicaId, Guid partitionId, string applicationName, string applicationTypeName, string nodeName, string simulationId, string simulationName, string deviceName, string deviceType)
        {
            if (this.IsEnabled())
            {
                WriteEvent(DeviceCreatedEventId,
                    serviceName,
                    serviceTypeName,
                    replicaId,
                    partitionId,
                    applicationName,
                    applicationTypeName,
                    nodeName,
                    simulationId,
                    simulationName,
                    deviceName,
                    deviceType);
            }
        }

        [NonEvent]
        public void DeviceFailedAdd(Actor actor, DeviceSettings deviceInfo, int retryCount)
        {
            DeviceFailedAdd(actor.ActorService.Context.ServiceName.ToString(),
                actor.ActorService.Context.ServiceTypeName,
                actor.ActorService.Context.ReplicaId,
                actor.ActorService.Context.PartitionId,
                actor.ActorService.Context.CodePackageActivationContext.ApplicationName,
                actor.ActorService.Context.CodePackageActivationContext.ApplicationTypeName,
                actor.ActorService.Context.NodeContext.NodeName,
                deviceInfo.SimulationSettings.SimulationId,
                deviceInfo.SimulationSettings.SimulationName,
                deviceInfo.DeviceServiceSettings.DeviceName,
                deviceInfo.DeviceServiceSettings.DeviceType,
                retryCount);
        }

        private const int DeviceFailedAddEventId = 104;
        [Event(DeviceFailedAddEventId, Level = EventLevel.Informational, Message = "Device connected with id: {7}")]
        private void DeviceFailedAdd(string serviceName, string serviceTypeName, long replicaId, Guid partitionId,
            string applicationName, string applicationTypeName, string nodeName, string simulationId,
            string simulationName, string deviceName, string deviceType, int retryCount)
        {
            if (this.IsEnabled())
            {
                WriteEvent(DeviceFailedAddEventId,
                    serviceName,
                    serviceTypeName,
                    replicaId,
                    partitionId,
                    applicationName,
                    applicationTypeName,
                    nodeName,
                    simulationId,
                    simulationName,
                    deviceName,
                    deviceType,
                    retryCount);
            }
        }

        [NonEvent]
        public void DeviceFailedTwin(Actor actor, DeviceSettings deviceInfo, int retryCount)
        {
            DeviceFailedTwin(actor.ActorService.Context.ServiceName.ToString(),
                actor.ActorService.Context.ServiceTypeName,
                actor.ActorService.Context.ReplicaId,
                actor.ActorService.Context.PartitionId,
                actor.ActorService.Context.CodePackageActivationContext.ApplicationName,
                actor.ActorService.Context.CodePackageActivationContext.ApplicationTypeName,
                actor.ActorService.Context.NodeContext.NodeName,
                deviceInfo.SimulationSettings.SimulationId,
                deviceInfo.SimulationSettings.SimulationName,
                deviceInfo.DeviceServiceSettings.DeviceName,
                deviceInfo.DeviceServiceSettings.DeviceType,
                retryCount);
        }

        private const int DeviceFailedTwinEventId = 105;
        [Event(DeviceFailedTwinEventId, Level = EventLevel.Informational, Message = "Device connected with id: {7}")]
        private void DeviceFailedTwin(string serviceName, string serviceTypeName, long replicaId, Guid partitionId,
            string applicationName, string applicationTypeName, string nodeName, string simulationId,
            string simulationName, string deviceName, string deviceType, int retryCount)
        {
            if (this.IsEnabled())
            {
                WriteEvent(DeviceFailedTwinEventId,
                    serviceName,
                    serviceTypeName,
                    replicaId,
                    partitionId,
                    applicationName,
                    applicationTypeName,
                    nodeName,
                    simulationId,
                    simulationName,
                    deviceName,
                    deviceType,
                    retryCount);
            }
        }

        [NonEvent]
        public void DeviceFailedConnection(Actor actor, DeviceSettings deviceInfo, int retryCount)
        {
            DeviceFailedConnection(actor.ActorService.Context.ServiceName.ToString(),
                actor.ActorService.Context.ServiceTypeName,
                actor.ActorService.Context.ReplicaId,
                actor.ActorService.Context.PartitionId,
                actor.ActorService.Context.CodePackageActivationContext.ApplicationName,
                actor.ActorService.Context.CodePackageActivationContext.ApplicationTypeName,
                actor.ActorService.Context.NodeContext.NodeName,
                deviceInfo.SimulationSettings.SimulationId,
                deviceInfo.SimulationSettings.SimulationName,
                deviceInfo.DeviceServiceSettings.DeviceName,
                deviceInfo.DeviceServiceSettings.DeviceType,
                retryCount);
        }

        private const int DeviceFailedConnectionEventId = 106;
        [Event(DeviceFailedConnectionEventId, Level = EventLevel.Informational, Message = "Device connected with id: {7}")]
        private void DeviceFailedConnection(string serviceName, string serviceTypeName, long replicaId, Guid partitionId,
            string applicationName, string applicationTypeName, string nodeName, string simulationId,
            string simulationName, string deviceName, string deviceType, int retryCount)
        {
            if (this.IsEnabled())
            {
                WriteEvent(DeviceFailedConnectionEventId,
                    serviceName,
                    serviceTypeName,
                    replicaId,
                    partitionId,
                    applicationName,
                    applicationTypeName,
                    nodeName,
                    simulationId,
                    simulationName,
                    deviceName,
                    deviceType,
                    retryCount);
            }
        }

        #region Events
        // Define an instance method for each event you want to record and apply an [Event] attribute to it.
        // The method name is the name of the event.
        // Pass any parameters you want to record with the event (only primitive integer types, DateTime, Guid & string are allowed).
        // Each event method implementation should check whether the event source is enabled, and if it is, call WriteEvent() method to raise the event.
        // The number and types of arguments passed to every event method must exactly match what is passed to WriteEvent().
        // Put [NonEvent] attribute on all methods that do not define an event.
        // For more information see https://msdn.microsoft.com/en-us/library/system.diagnostics.tracing.eventsource.aspx

        [NonEvent]
        public void Message(string message, params object[] args)
        {
            if (this.IsEnabled())
            {
                string finalMessage = string.Format(message, args);
                Message(finalMessage);
            }
        }

        private const int MessageEventId = 1;
        [Event(MessageEventId, Level = EventLevel.Informational, Message = "{0}")]
        public void Message(string message)
        {
            if (this.IsEnabled())
            {
                WriteEvent(MessageEventId, message);
            }
        }

        [NonEvent]
        public void ActorMessage(Actor actor, string message, params object[] args)
        {
            if (this.IsEnabled()
                && actor.Id != null
                && actor.ActorService != null
                && actor.ActorService.Context != null
                && actor.ActorService.Context.CodePackageActivationContext != null)
            {
                string finalMessage = string.Format(message, args);
                ActorMessage(
                    actor.GetType().ToString(),
                    actor.Id.ToString(),
                    actor.ActorService.Context.CodePackageActivationContext.ApplicationTypeName,
                    actor.ActorService.Context.CodePackageActivationContext.ApplicationName,
                    actor.ActorService.Context.ServiceTypeName,
                    actor.ActorService.Context.ServiceName.ToString(),
                    actor.ActorService.Context.PartitionId,
                    actor.ActorService.Context.ReplicaId,
                    actor.ActorService.Context.NodeContext.NodeName,
                    finalMessage);
            }
        }

        // For very high-frequency events it might be advantageous to raise events using WriteEventCore API.
        // This results in more efficient parameter handling, but requires explicit allocation of EventData structure and unsafe code.
        // To enable this code path, define UNSAFE conditional compilation symbol and turn on unsafe code support in project properties.
        private const int ActorMessageEventId = 2;
        [Event(ActorMessageEventId, Level = EventLevel.Informational, Message = "{9}")]
        private
#if UNSAFE
            unsafe
#endif
            void ActorMessage(
            string actorType,
            string actorId,
            string applicationTypeName,
            string applicationName,
            string serviceTypeName,
            string serviceName,
            Guid partitionId,
            long replicaOrInstanceId,
            string nodeName,
            string message)
        {
#if !UNSAFE
            WriteEvent(
                    ActorMessageEventId,
                    actorType,
                    actorId,
                    applicationTypeName,
                    applicationName,
                    serviceTypeName,
                    serviceName,
                    partitionId,
                    replicaOrInstanceId,
                    nodeName,
                    message);
#else
                const int numArgs = 10;
                fixed (char* pActorType = actorType, pActorId = actorId, pApplicationTypeName = applicationTypeName, pApplicationName = applicationName, pServiceTypeName = serviceTypeName, pServiceName = serviceName, pNodeName = nodeName, pMessage = message)
                {
                    EventData* eventData = stackalloc EventData[numArgs];
                    eventData[0] = new EventData { DataPointer = (IntPtr) pActorType, Size = SizeInBytes(actorType) };
                    eventData[1] = new EventData { DataPointer = (IntPtr) pActorId, Size = SizeInBytes(actorId) };
                    eventData[2] = new EventData { DataPointer = (IntPtr) pApplicationTypeName, Size = SizeInBytes(applicationTypeName) };
                    eventData[3] = new EventData { DataPointer = (IntPtr) pApplicationName, Size = SizeInBytes(applicationName) };
                    eventData[4] = new EventData { DataPointer = (IntPtr) pServiceTypeName, Size = SizeInBytes(serviceTypeName) };
                    eventData[5] = new EventData { DataPointer = (IntPtr) pServiceName, Size = SizeInBytes(serviceName) };
                    eventData[6] = new EventData { DataPointer = (IntPtr) (&partitionId), Size = sizeof(Guid) };
                    eventData[7] = new EventData { DataPointer = (IntPtr) (&replicaOrInstanceId), Size = sizeof(long) };
                    eventData[8] = new EventData { DataPointer = (IntPtr) pNodeName, Size = SizeInBytes(nodeName) };
                    eventData[9] = new EventData { DataPointer = (IntPtr) pMessage, Size = SizeInBytes(message) };

                    WriteEventCore(ActorMessageEventId, numArgs, eventData);
                }
#endif
        }

        private const int ActorHostInitializationFailedEventId = 3;
        [Event(ActorHostInitializationFailedEventId, Level = EventLevel.Error, Message = "Actor host initialization failed", Keywords = Keywords.HostInitialization)]
        public void ActorHostInitializationFailed(string exception)
        {
            WriteEvent(ActorHostInitializationFailedEventId, exception);
        }
        #endregion

        private const int ExceptionId = 4;
        [Event(ExceptionId, Level = EventLevel.Error, Message = "unkown excpetion")]
        public void ExceptionOccured(string exception)
        {
            this.WriteEvent(ExceptionId, exception);
        }


        #region Private Methods
#if UNSAFE
            private int SizeInBytes(string s)
            {
                if (s == null)
                {
                    return 0;
                }
                else
                {
                    return (s.Length + 1) * sizeof(char);
                }
            }
#endif
        #endregion
    }
}
