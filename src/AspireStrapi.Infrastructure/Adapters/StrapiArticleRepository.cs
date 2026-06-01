using AspireStrapi.Application.Ports;
using AspireStrapi.Domain.Entities;
using AspireStrapi.Domain.ValueObjects;
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

        // Strapi 5 flattened the GraphQL schema: `articles` is now a plain
        // list of Article objects (no v4 `data`/`attributes` wrapping).
        IReadOnlyList<IGetArticles_Articles?>? articles = result.Data?.Articles;
        if (articles is null)
        {
            return [];
        }

        return articles
            .Where(item => item?.Title is not null)
            .Select(item => MapToArticle(item!))
            .ToList();
    }

    private static Article MapToArticle(IGetArticles_Articles item)
    {
        return new Article(
            id: item.DocumentId,
            title: item.Title!,
            description: item.Description,
            slug: MapSlug(item.Slug),
            author: MapAuthor(item.Author),
            category: MapCategory(item.Category),
            tags: null,
            publishedAt: item.PublishedAt);
    }

    private static Slug? MapSlug(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : new Slug(value);

    private static Author? MapAuthor(IGetArticles_Articles_Author? author)
    {
        if (author?.Name is null)
        {
            return null;
        }

        EmailAddress? email = string.IsNullOrWhiteSpace(author.Email)
            ? null
            : new EmailAddress(author.Email);

        return new Author(author.Name, email, author.Avatar?.Url);
    }

    private static Category? MapCategory(IGetArticles_Articles_Category? category)
    {
        if (category?.Name is null)
        {
            return null;
        }

        return new Category(category.Name, MapSlug(category.Slug));
    }
}
