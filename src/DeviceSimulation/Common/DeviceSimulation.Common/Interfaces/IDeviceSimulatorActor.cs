using DeviceSimulation.Common.Models;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting;
using System.Threading;
using System.Threading.Tasks;

[assembly: FabricTransportActorRemotingProvider(RemotingListener = RemotingListener.V2Listener, RemotingClient = RemotingClient.V2Client)]
namespace DeviceSimulator
{
    public interface IDeviceSimulator
        : IActor
    {
        Task CleanDevicesAsync(DeviceServiceSettings deviceServiceSettings, CancellationToken cancellationToken);

        Task AddDeviceAsync(DeviceServiceSettings deviceServiceSettings, CancellationToken cancellationToken);

        Task CreateDeviceTwinAsync(DeviceServiceSettings deviceServiceSettings, CancellationToken cancellationToken);

        Task ConnectToHubAsync(DeviceSettings deviceSettings, CancellationToken cancellationToken);

        Task SendEventAsync();
    }
}
