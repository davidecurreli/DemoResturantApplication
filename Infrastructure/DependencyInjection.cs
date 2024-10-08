using Infrastructure.Helpers;
using Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Inject all Services in Services folder
        InstantiateCoreServices(services);

        // Inject Db Context
        services.AddDbContext<DbContextClass>();

        return services;
    }

    public static IServiceCollection InstantiateCoreServices(IServiceCollection services)
    {
        // Inject all services from Infrastructure.Services with Scrutor  
        services.Scan(scan => scan
            .FromAssemblyOf<IAssemblyMarker>()
            .AddClasses(classes =>
                classes
                    .InNamespaces("Infrastructure.Services")
                    .Where(type => type.Name.EndsWith("Service") && !type.Name.Contains("Generic"))
            )
            .AsImplementedInterfaces()
            .WithScopedLifetime()
        );

        // Istanciate and inject necessary entity Repositories
        var entityTypeList = GetTypeHelper.GetTypesFromNamespace("Domain.Entities");
        foreach (var entityType in entityTypeList)
        {
            if (entityType is null) continue; // should throw some kind of exception

            var repositoryInterface = typeof(IGenericRepository<>).MakeGenericType(entityType);
            var repositoryIstance = typeof(GenericRepository<>).MakeGenericType(entityType);
            services.AddScoped(repositoryInterface, repositoryIstance);
        }

        return services;
    }
}
