using Microsoft.EntityFrameworkCore;
using Restaurante.Infraestructura.DBContext;

namespace Restaurante.Api.DI
{
    public static class Dependencies
    {
        public static IServiceCollection AddInfraestructura(this IServiceCollection services, IConfiguration configuration)
        {
            var conn = configuration.GetConnectionString("DefaultConnection_SQLEXPRESS")
                       ?? throw new InvalidOperationException("No connection string provided.");

            // Interceptor sencillo para auditoría (ya implementado en el DbContext) - puedes registrar otros interceptores aquí
            //services.AddSingleton<Restaurante.Infraestructura.Persistence.AuditableEntitySaveChangesInterceptor>();

            services.AddDbContextPool<RestauranteDbContext>(options =>
            {
                options.UseSqlServer(conn, sql =>
                {
                    sql.MigrationsAssembly(typeof(RestauranteDbContext).Assembly.FullName);
                    sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                    sql.CommandTimeout(configuration.GetValue<int?>("EfCore:CommandTimeoutSeconds") ?? 180);
                });

                // Comportamiento adicional
                options.EnableDetailedErrors(configuration.GetValue<bool>("EfCore:EnableDetailedErrors"));
                options.EnableSensitiveDataLogging(configuration.GetValue<bool>("EfCore:EnableSensitiveDataLogging"));
                options.ConfigureWarnings(w => w.Default(WarningBehavior.Log));
            }, poolSize: 128); // pool size configurable

            // Registrar repositorios concretos
            // services.AddScoped<IPedidoRepositorio, PedidoRepository>();
            // services.AddScoped<IMesaRepositorio, MesaRepository>();

            return services;
        }
    }
}
