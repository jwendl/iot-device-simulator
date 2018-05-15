using CodeEngine.CSharp;
using CodeEngine.CSharp.Interfaces;
using CodeEngine.FSharp;
using CodeEngine.FSharp.Interfaces;
using CodeEngine.JavaScript;
using CodeEngine.JavaScript.Interfaces;
using CodeEngine.Python;
using CodeEngine.Python.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using System;
using System.Threading;

namespace DeviceSimulator
{
    internal static class Program
    {
        private static void Main()
        {
            var rootServiceProvider = ConfigureServices();

            try
            {
                ActorRuntime.RegisterActorAsync<DeviceSimulatorActor>(
                   (context, actorType) => new ActorService(
                       context,
                       actorType,
                       (actorService, actorId) =>
                       {
                           var actorFactory = rootServiceProvider.GetRequiredService<Func<ActorService, ActorId, DeviceSimulatorActor>>();

                           var actor = actorFactory(actorService, actorId);

                           return actor;
                       })).GetAwaiter().GetResult();

                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                DeviceSimulatorActorEventSource.Current.ActorHostInitializationFailed(e.ToString());
                throw;
            }
        }

        private static IServiceProvider ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection
                .AddLogging()
                .AddTransient<ICSharpService<string>, CSharpService<string>>()
                .AddTransient<IFSharpService<string>, FSharpService<string>>()
                .AddTransient<IPythonService<string>, PythonService<string>>()
                .AddTransient<IJavaScriptService<string>, JavaScriptService<string>>()
                .AddSingleton<Func<ICSharpService<string>>>(sp => () => sp.GetRequiredService<ICSharpService<string>>())
                .AddSingleton<IDeviceTypeScriptServiceCache<ICSharpService<string>>, DeviceTypeScriptEngineCache<ICSharpService<string>>>()
                .AddSingleton<Func<ActorService, ActorId, DeviceSimulatorActor>>(sp => (actorService, actorId) => new DeviceSimulatorActor(actorService, actorId, sp.GetRequiredService<IDeviceTypeScriptServiceCache<ICSharpService<string>>>(), sp.GetRequiredService<Func<ICSharpService<string>>>()));

            return serviceCollection.BuildServiceProvider();
        }
    }
}
