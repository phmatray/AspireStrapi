using AspireStrapi.Application.Ports;
using AspireStrapi.Infrastructure.Adapters;
using Microsoft.Extensions.DependencyInjection;

namespace AspireStrapi.Infrastructure;

/// <summary>
/// Registers the Strapi GraphQL adapter (StrawberryShake client) and the
/// driven port implementations.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddStrapiInfrastructure(
        this IServiceCollection services,
        Uri graphQlEndpoint)
    {
        services
            .AddBlogClient()
            .ConfigureHttpClient(client => client.BaseAddress = graphQlEndpoint);

        services.AddScoped<IArticleRepository, StrapiArticleRepository>();
        services.AddScoped<IAboutPageRepository, StrapiAboutPageRepository>();

        return services;
    }
}
