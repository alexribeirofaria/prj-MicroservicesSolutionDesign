using Infrastructure.DatabaseContexts;
using Repository.Mapping.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Migrations.MySqlServer.CommonInjectDependence
{
    public static class MigrationsMySqlServerInjectDependence
    {
        // Método único que trata tanto da configuração com IConfiguration quanto com string de conexão.
        public static IServiceCollection ConfigureMySqlServerMigrationsContext(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            string connectionString = GetConnectionString(configuration);

            return ConfigureMySqlServerMigrationsContext(services, connectionString);
        }

        // Método de configuração utilizando diretamente a string de conexão.
        public static IServiceCollection ConfigureMySqlServerMigrationsContext(
            this IServiceCollection services,
            string connectionString)
        {
            ConfigureDbContext(services, connectionString);
            RegisterDatabaseProvider(services);

            return services;
        }

        // Método auxiliar para obter a string de conexão de forma clara.
        private static string GetConnectionString(IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("MySqlConnectionString")
                ?? configuration.GetConnectionString("SqlConnectionString");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("Connection string 'SqlConnectionString' não encontrada no appsettings.json.");
            }

            return connectionString;
        }

        // Método para configurar o DbContext, isolando a lógica de configuração.
        private static void ConfigureDbContext(IServiceCollection services, string connectionString)
        {
            services.AddDbContext<RegisterContext>((sp, options) =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                options.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString),
                    b => b.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name)
                );
                options.UseLoggerFactory(loggerFactory);
                options.UseLazyLoadingProxies();
            });
        }

        // Método para registrar o DatabaseProvider como Singleton.
        private static void RegisterDatabaseProvider(IServiceCollection services)
        {
            var provider = DatabaseProvider.MySql;
            services.AddSingleton(typeof(DatabaseProvider), provider);

        }
    }
}
