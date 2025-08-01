using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using StoreBoost.Application.Interfaces;
using StoreBoost.Infrastructure.Repositories;
using System.Reflection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddSingleton<ISlotRepository, InMemorySlotRepository>();

        return services;
    }
}
