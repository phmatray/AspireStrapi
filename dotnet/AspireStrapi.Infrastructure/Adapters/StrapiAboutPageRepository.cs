using AspireStrapi.Application.Ports;
using AspireStrapi.Domain.Entities;
using AspireStrapi.Infrastructure.ApiClient;
using StrawberryShake;

namespace AspireStrapi.Infrastructure.Adapters;

/// <summary>
/// Outbound adapter that fetches the about page from the Strapi GraphQL API and
/// maps the StrawberryShake response into the domain <see cref="AboutPage"/>.
/// </summary>
public sealed class StrapiAboutPageRepository : IAboutPageRepository
{
    private readonly IBlogClient _client;

    public StrapiAboutPageRepository(IBlogClient client)
    {
        _client = client;
    }

    public async Task<AboutPage?> GetAboutAsync(
        CancellationToken cancellationToken = default)
    {
        IOperationResult<IGetPageAboutResult> result =
            await _client.GetPageAbout.ExecuteAsync(cancellationToken);

        result.EnsureNoErrors();

        // Strapi 5 flattened the GraphQL schema: `about` is now the About
        // object directly (no v4 `data`/`attributes` wrapping).
        IGetPageAbout_About? about = result.Data?.About;

        if (about?.Title is null)
        {
            return null;
        }

        return new AboutPage(about.Title, about.CreatedAt);
    }
}
