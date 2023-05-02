using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DAL.Infrastructure;
using DAL.Repositories;
using DAL.Repositories.Interfaces;

namespace DAL.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDalRepositories(
        this IServiceCollection services)
    {
        services.AddScoped<IScheduleRepository, ScheduleRepository>();
        
        return services;
    }
    
    public static IServiceCollection AddDalInfrastructure(
        this IServiceCollection services)
    {
        //configure postrges types
        Postgres.MapCompositeTypes();
        
        //add migrations
        Postgres.AddMigrations(services);
        
        return services;

    }
}