using System.Threading.Tasks;

namespace DeviceGenerator.Interfaces
{
    public interface IStorageService
    {
        Task<string> FetchFileAsync(string containerName, string fileName);
    }
}
