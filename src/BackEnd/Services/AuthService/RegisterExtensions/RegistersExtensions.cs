using AuthService.Settings;
using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;

namespace AuthService.RegistersExtensions;

public static class ServiceRegistryExtensions
{
    public static IServiceCollection AddConsulSettings(this IServiceCollection services, ServiceSettings serviceSettings)
    {
        services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
        {
            consulConfig.Address = new Uri(serviceSettings.ServiceDiscoveryAddress);
        }));

        return services;
    }

    public static IApplicationBuilder UseConsul(this IApplicationBuilder app, ServiceSettings serviceSettings)
    {
        var consulClient = app.ApplicationServices.GetRequiredService<IConsulClient>();
        var logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger("AppExtensions");
        var lifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();

        var machineIdentifier = GetMachineIdentifier();
        var hashedMachineIdentifier = ComputeHash(machineIdentifier);
        var serviceInstance = $"{serviceSettings.ServiceName}-{hashedMachineIdentifier}";
        
        var uri = new Uri($"{serviceSettings.ServiceDiscoveryAddress}");


        var registration = new AgentServiceRegistration()
        {
            ID = serviceInstance,
            Name = serviceSettings.ServiceName,
            Address = serviceSettings.ServiceHost,            
            Port = serviceSettings.ServicePort, 
            Tags = new[] { $"{serviceSettings.ServiceName}" }
        };

        logger.LogInformation("Registring with Consul");
        consulClient.Agent.ServiceDeregister(registration.ID).ConfigureAwait(true);
        consulClient.Agent.ServiceRegister(registration).ConfigureAwait(true);

        lifetime.ApplicationStopping.Register(async () =>
        {
            logger.LogInformation("Unregistering from Consul");
        });

        return app;
    }
        
    private static string GetMachineIdentifier()
    {
        var macAddress = GetMacAddress();
        if (!string.IsNullOrEmpty(macAddress))
            return macAddress; 
   
        return GetMachineGuid();
    }
        
    private static string GetMacAddress()
    {
        var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
        var macAddress = networkInterfaces
            .Where(nic => nic.OperationalStatus == OperationalStatus.Up) 
            .Select(nic => nic.GetPhysicalAddress().ToString())
            .FirstOrDefault();

        return macAddress;
    }

    private static string GetMachineGuid()
    {
        var machineName = Environment.MachineName;
        var guid = Guid.TryParse(machineName, out var parsedGuid)
            ? parsedGuid
            : Guid.NewGuid();

        return guid.ToString();
    }

    private static string ComputeHash(string input)
    {
        using (var sha256 = SHA256.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = sha256.ComputeHash(bytes);

            return string.Concat(hashBytes.Select(b => b.ToString("x2")));
        }
    }
}
