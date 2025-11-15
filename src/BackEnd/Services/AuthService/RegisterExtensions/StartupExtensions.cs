using AuthService.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AuthService.RegistersExtensions;

public static class StartupExtensions
{
    public static ServiceSettings StartupBootstrap(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ServiceSettings>(configuration.GetSection(nameof(ServiceSettings)));
        var serviceProvider = services.BuildServiceProvider();
        var iop = serviceProvider.GetService<IOptions<ServiceSettings>>();
        return iop.Value;
    }
}