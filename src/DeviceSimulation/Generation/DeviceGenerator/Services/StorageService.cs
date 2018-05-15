using DeviceGenerator.Interfaces;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Threading.Tasks;

namespace DeviceGenerator.Services
{
    public class StorageService
        : IStorageService
    {
        private readonly CloudBlobClient cloudBlobClient;

        public StorageService(string storageConnectionString)
        {
            var cloudStorageAccount = CloudStorageAccount.Parse(storageConnectionString);
            cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
        }

        public async Task<string> FetchFileAsync(string containerName, string fileName)
        {
            var containerReference = cloudBlobClient.GetContainerReference(containerName);
            var blockBlobReference = containerReference.GetBlockBlobReference(fileName);
            return await blockBlobReference.DownloadTextAsync();
        }
    }
}
