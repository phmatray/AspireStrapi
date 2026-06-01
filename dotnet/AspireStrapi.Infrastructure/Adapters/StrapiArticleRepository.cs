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
    private readonly StrapiMediaUrlResolver _media;
    private readonly StrapiArticleBodyClient _bodyClient;

    public StrapiArticleRepository(
        IBlogClient client,
        StrapiMediaUrlResolver media,
        StrapiArticleBodyClient bodyClient)
    {
        _client = client;
        _media = media;
        _bodyClient = bodyClient;
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

    public async Task<IReadOnlyList<Article>> GetArticlesByCategorySlugAsync(
        string categorySlug,
        CancellationToken cancellationToken = default)
    {
        IOperationResult<IGetArticlesByCategoryResult> result =
            await _client.GetArticlesByCategory.ExecuteAsync(categorySlug, cancellationToken);

        result.EnsureNoErrors();

        IReadOnlyList<IGetArticlesByCategory_Articles?>? articles = result.Data?.Articles;
        if (articles is null)
        {
            return [];
        }

        return articles
            .Where(item => item?.Title is not null)
            .Select(item => MapToArticle(item!))
            .ToList();
    }

    public async Task<Article?> GetArticleByIdAsync(
        string documentId,
        CancellationToken cancellationToken = default)
    {
        IOperationResult<IGetArticleByIdResult> result =
            await _client.GetArticleById.ExecuteAsync(documentId, cancellationToken);

        result.EnsureNoErrors();

        IGetArticleById_Articles? article = result.Data?.Articles
            ?.FirstOrDefault(item => item?.Title is not null);
        if (article is null)
        {
            return null;
        }

        // The rich-text body lives in a dynamic-zone union that the typed
        // StrawberryShake client cannot model; fetch it via the body client.
        string? body = await _bodyClient.GetBodyHtmlAsync(article.DocumentId, cancellationToken);

        return new Article(
            id: article.DocumentId,
            title: article.Title!,
            description: article.Description,
            slug: MapSlug(article.Slug),
            author: MapAuthor(article.Author),
            category: MapCategory(article.Category),
            tags: null,
            publishedAt: article.PublishedAt,
            coverImageUrl: _media.Resolve(article.Cover?.Url),
            body: body);
    }

    private Article MapToArticle(IGetArticles_Articles item) => new(
        id: item.DocumentId,
        title: item.Title!,
        description: item.Description,
        slug: MapSlug(item.Slug),
        author: MapAuthor(item.Author),
        category: MapCategory(item.Category),
        tags: null,
        publishedAt: item.PublishedAt,
        coverImageUrl: _media.Resolve(item.Cover?.Url));

    private Article MapToArticle(IGetArticlesByCategory_Articles item) => new(
        id: item.DocumentId,
        title: item.Title!,
        description: item.Description,
        slug: MapSlug(item.Slug),
        author: MapAuthor(item.Author),
        category: MapCategory(item.Category),
        tags: null,
        publishedAt: item.PublishedAt,
        coverImageUrl: _media.Resolve(item.Cover?.Url));

    private static Slug? MapSlug(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : new Slug(value);

    private Author? MapAuthor(IGetArticles_Articles_Author? author)
        => MapAuthor(author?.Name, author?.Email, author?.Avatar?.Url);

    private Author? MapAuthor(IGetArticlesByCategory_Articles_Author? author)
        => MapAuthor(author?.Name, author?.Email, author?.Avatar?.Url);

    private Author? MapAuthor(IGetArticleById_Articles_Author? author)
        => MapAuthor(author?.Name, author?.Email, author?.Avatar?.Url);

    private Author? MapAuthor(string? name, string? email, string? avatarUrl)
    {
        if (name is null)
        {
            return null;
        }

        EmailAddress? parsedEmail = string.IsNullOrWhiteSpace(email)
            ? null
            : new EmailAddress(email);

        return new Author(name, parsedEmail, _media.Resolve(avatarUrl));
    }

    private static Category? MapCategory(IGetArticles_Articles_Category? category)
        => MapCategory(category?.Name, category?.Slug);

    private static Category? MapCategory(IGetArticlesByCategory_Articles_Category? category)
        => MapCategory(category?.Name, category?.Slug);

    private static Category? MapCategory(IGetArticleById_Articles_Category? category)
        => MapCategory(category?.Name, category?.Slug);

    private static Category? MapCategory(string? name, string? slug)
        => name is null ? null : new Category(name, MapSlug(slug));
}
