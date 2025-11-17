using Consul;
using Ocelot.Logging;
using Ocelot.Provider.Consul;
using Ocelot.Provider.Consul.Interfaces;

namespace api_gateway.Configuration;

public class ConsulServiceBuilder : DefaultConsulServiceBuilder
{
    public ConsulServiceBuilder(IHttpContextAccessor contextAccessor, IConsulClientFactory clientFactory, IOcelotLoggerFactory loggerFactory)
        : base(contextAccessor, clientFactory, loggerFactory) { }

    protected override string GetDownstreamHost(ServiceEntry entry, Node node)
    {
        return entry.Service.Address;
    }
}