using System.Collections.Concurrent;

namespace DeviceSimulator
{
    internal sealed class DeviceTypeScriptEngineCache<TScriptService> : IDeviceTypeScriptServiceCache<TScriptService>
    {
        private ConcurrentDictionary<string, TScriptService> _cachedScriptServices = new ConcurrentDictionary<string, TScriptService>();

        public void RegisterDeviceTypeScript(string deviceType, TScriptService scriptService) => _cachedScriptServices.AddOrUpdate(deviceType, scriptService, (dt, ess) => scriptService);

        public bool TryGetScriptService(string deviceType, out TScriptService scriptService) => _cachedScriptServices.TryGetValue(deviceType, out scriptService);
    }

    internal interface IDeviceTypeScriptServiceCache<TScriptService>
    {
        void RegisterDeviceTypeScript(string deviceType, TScriptService scriptService);

        bool TryGetScriptService(string deviceType, out TScriptService scriptService);
    }
}
