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

        // Resolves Strapi's relative media URLs against the GraphQL host root.
        services.AddSingleton(new StrapiMediaUrlResolver(graphQlEndpoint));

        // Raw GraphQL client used to fetch the article body (dynamic-zone union)
        // that the typed StrawberryShake client cannot model.
        services.AddHttpClient<StrapiArticleBodyClient>(client =>
            client.BaseAddress = graphQlEndpoint);

        services.AddScoped<IArticleRepository, StrapiArticleRepository>();
        services.AddScoped<ICategoryRepository, StrapiCategoryRepository>();
        services.AddScoped<IAuthorRepository, StrapiAuthorRepository>();
        services.AddScoped<IAboutPageRepository, StrapiAboutPageRepository>();

        return services;
    }
}
