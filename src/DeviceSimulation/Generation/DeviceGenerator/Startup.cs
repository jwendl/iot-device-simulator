using DeviceGenerator.Interfaces;
using DeviceGenerator.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Fabric;

namespace DeviceGenerator
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddLogging(lb => lb.AddDebug());

            services.AddSingleton<IConfigureOptions<SimulationIoTHubOptions>, SimulationIoTHubOptionsConfigurer>();

            services.AddScoped<IStorageService, StorageService>(sp =>
            {
                var serviceContext = sp.GetRequiredService<StatelessServiceContext>();
                var configurationPackageObject = serviceContext.CodePackageActivationContext.GetConfigurationPackageObject("Config");

                var storageAccouuntConnectionStringParameter = configurationPackageObject.Settings.Sections["ConnectionStrings"].Parameters["StorageAccountConnectionString"];

                return new StorageService(storageAccouuntConnectionStringParameter.Value);
            });

            services.AddScoped<ISimulationManagerService, SimulationManagerService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseMvc();
        }

        private sealed class SimulationIoTHubOptionsConfigurer : IConfigureOptions<SimulationIoTHubOptions>
        {
            private readonly StatelessServiceContext serviceContext;

            public SimulationIoTHubOptionsConfigurer(StatelessServiceContext serviceContext)
            {
                this.serviceContext = serviceContext;
            }

            public void Configure(SimulationIoTHubOptions options)
            {
                var configurationPackageObject = serviceContext.CodePackageActivationContext.GetConfigurationPackageObject("Config");

                options.IotHubConnectionString = configurationPackageObject.Settings.Sections["ConnectionStrings"].Parameters["IoTHubConnectionString"].Value;
                options.IoTHubName = configurationPackageObject.Settings.Sections["IoTHub"].Parameters["HubName"].Value;
            }
        }
    }
}
