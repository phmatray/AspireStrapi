using AspireStrapi.Application.Ports;
using AspireStrapi.Domain.Entities;
using AspireStrapi.Infrastructure.ApiClient;
using StrawberryShake;

namespace AspireStrapi.Infrastructure.Adapters;

/// <summary>
/// Outbound adapter that fetches articles from the Strapi GraphQL API and
/// maps the StrawberryShake response into domain <see cref="Article"/> entities.
/// This is the anti-corruption layer between the GraphQL schema and the domain.
/// </summary>
public sealed class StrapiArticleRepository : IArticleRepository
{
    private readonly IBlogClient _client;

    public StrapiArticleRepository(IBlogClient client)
    {
        _client = client;
    }

    public async Task<IReadOnlyList<Article>> GetArticlesAsync(
        CancellationToken cancellationToken = default)
    {
        IOperationResult<IGetArticlesResult> result =
            await _client.GetArticles.ExecuteAsync(cancellationToken);

        result.EnsureNoErrors();

        IReadOnlyList<IGetArticles_Articles_Data>? data = result.Data?.Articles?.Data;
        if (data is null)
        {
            return [];
        }

        return data
            .Where(item => item.Attributes?.Title is not null)
            .Select(MapToArticle)
            .ToList();
    }

    private static Article MapToArticle(IGetArticles_Articles_Data item)
    {
        IGetArticles_Articles_Data_Attributes attributes = item.Attributes!;

        return new Article(
            id: item.Id ?? Guid.NewGuid().ToString(),
            title: attributes.Title!,
            description: attributes.Description,
            slug: null,
            author: null,
            category: null,
            tags: null,
            publishedAt: null);
    }
}
