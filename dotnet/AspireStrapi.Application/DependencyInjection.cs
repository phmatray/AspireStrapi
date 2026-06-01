using AspireStrapi.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace AspireStrapi.Application;

/// <summary>
/// Registers the application use-cases. Driven ports must be registered
/// separately by an Infrastructure adapter.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IContentService, ContentService>();
        return services;
    }
}
