using api_gateway.Configuration;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;

var builder = WebApplication.CreateBuilder(args);

// Configurar Kestrel (porta 9000 e remover header do servidor)
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.AddServerHeader = false;
    serverOptions.ListenAnyIP(9000);
});

// Carregar arquivos de configuração
var env = builder.Environment;
builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
     .AddJsonFile("ocelot.json")
     .AddJsonFile($"ocelot.{env.EnvironmentName}.json", true, true)
     .AddEnvironmentVariables()
     .Build();

// Registrar Ocelot e agregador
builder.Services
    .AddOcelot(builder.Configuration)
    .AddConsul<ConsulServiceBuilder>();
    //.AddTransientDefinedAggregator<BookDetailsAggregator>();

var app = builder.Build();

if (env.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
// Configurar Ocelot
app.UseOcelot().Wait();

app.Run();
