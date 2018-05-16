using System;
using System.Diagnostics;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Diagnostics.EventFlow.ServiceFabric;
using Microsoft.ServiceFabric.Services.Runtime;

namespace MonitoringService
{
    internal static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {
            try
            {
                using (ManualResetEvent terminationEvent = new ManualResetEvent(initialState: false))
                using (var diagnosticsPipeline = ServiceFabricDiagnosticPipelineFactory.CreatePipeline("IoTDeviceSimulation-Cluster-DiagnosticsPipeline"))
                {
                    // Ctrl-C signal is the standard way the Service Fabric runtime uses to notify service processes that they need to perform cleanup and exit. 
                    // By default the process has 30 seconds to react.
                    // Using termination event lets event flow clean up ETW system-level events
                    Console.CancelKeyPress += (sender, eventArgs) => Shutdown(diagnosticsPipeline, terminationEvent);

                    AppDomain.CurrentDomain.UnhandledException += (sender, unhandledExceptionArgs) =>
                    {
                        ServiceEventSource.Current.UnhandledException(unhandledExceptionArgs.ExceptionObject?.ToString() ?? "(no exception information)");
                        Shutdown(diagnosticsPipeline, terminationEvent);
                    };

                    ServiceRuntime.RegisterServiceAsync("MonitoringServiceType",
                        context => new MonitoringService(context)).GetAwaiter().GetResult();

                    ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(MonitoringService).Name);

                    terminationEvent.WaitOne();
                }
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                throw;
            }
        }

        private static void Shutdown(IDisposable disposable, ManualResetEvent terminationEvent)
        {
            try
            {
                disposable.Dispose();
            }
            finally
            {
                terminationEvent.Set();
            }
        }
    }
}
