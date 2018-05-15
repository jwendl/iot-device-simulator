using System;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Utility;
using Xunit;

namespace DeviceSimulatorActor.Tests
{
    public class ActorLoggingTests
    {
        [Fact]
        public void Validate_Etw_Manifest()
        {
            EventSourceAnalyzer.InspectAll(DeviceSimulator.DeviceSimulatorActorEventSource.Current);
        }
    }
}
