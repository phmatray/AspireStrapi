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

        IGetPageAbout_About_Data_Attributes? attributes =
            result.Data?.About?.Data?.Attributes;

        if (attributes?.Title is null)
        {
            return null;
        }

        return new AboutPage(attributes.Title, attributes.CreatedAt);
    }
}
