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

        // Garante que o ID do serviço seja único
        var machineIdentifier = GetMachineIdentifier(); // Método para obter o identificador da máquina
        var hashedMachineIdentifier = ComputeHash(machineIdentifier);
        var serviceInstance = $"{serviceSettings.ServiceName}-{hashedMachineIdentifier}";

        var registration = new AgentServiceRegistration()
        {
            ID = serviceInstance,
            Name = serviceSettings.ServiceName,
            Address = serviceSettings.ServiceHost,
            Port = serviceSettings.ServicePort
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

    private static string GetMachineGuid()
    {
        // Usando o nome da máquina como identificador fixo
        var machineName = Environment.MachineName;

        // Gerando um GUID a partir do nome da máquina (garante que o GUID será sempre o mesmo para esta máquina)
        var guid = Guid.TryParse(machineName, out var parsedGuid)
            ? parsedGuid
            : Guid.NewGuid();

        return guid.ToString();
    }

    private static string GetMachineIdentifier()
    {
        // Usa o primeiro endereço MAC encontrado para identificar a máquina de forma única
        var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
        var macAddress = networkInterfaces
            .Where(nic => nic.OperationalStatus == OperationalStatus.Up)
            .Select(nic => nic.GetPhysicalAddress())
            .FirstOrDefault();

        // Se o endereço MAC não for encontrado, você pode usar um GUID como fallback
        return macAddress?.ToString() ?? GetMachineGuid();
    }

    private static string ComputeHash(string input)
    {
        using (var sha256 = SHA256.Create())
        {
            // Converte a entrada em um array de bytes e calcula o hash
            var bytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = sha256.ComputeHash(bytes);

            // Converte o hash para uma string hexadecimal
            return string.Concat(hashBytes.Select(b => b.ToString("x2")));
        }
    }
}
