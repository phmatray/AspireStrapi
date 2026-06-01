using AspireStrapi.Application.Ports;
using AspireStrapi.Domain.Entities;
using AspireStrapi.Domain.ValueObjects;
using AspireStrapi.Infrastructure.ApiClient;
using StrawberryShake;

namespace AspireStrapi.Infrastructure.Adapters;

/// <summary>
/// Outbound adapter that fetches authors and the articles attributed to them
/// from the Strapi GraphQL API, mapping them into domain entities.
/// </summary>
public sealed class StrapiAuthorRepository : IAuthorRepository
{
    private readonly IBlogClient _client;
    private readonly StrapiMediaUrlResolver _media;

    public StrapiAuthorRepository(IBlogClient client, StrapiMediaUrlResolver media)
    {
        _client = client;
        _media = media;
    }

    public async Task<IReadOnlyList<(Author Author, IReadOnlyList<Article> Articles)>>
        GetAuthorsWithArticlesAsync(CancellationToken cancellationToken = default)
    {
        IOperationResult<IGetAuthorsResult> result =
            await _client.GetAuthors.ExecuteAsync(cancellationToken);

        result.EnsureNoErrors();

        IReadOnlyList<IGetAuthors_Authors?>? authors = result.Data?.Authors;
        if (authors is null)
        {
            return [];
        }

        return authors
            .Where(item => item?.Name is not null)
            .Select(MapAuthorWithArticles)
            .ToList();
    }

    private (Author, IReadOnlyList<Article>) MapAuthorWithArticles(IGetAuthors_Authors? item)
    {
        EmailAddress? email = string.IsNullOrWhiteSpace(item!.Email)
            ? null
            : new EmailAddress(item.Email);

        var author = new Author(
            name: item.Name!,
            email: email,
            avatarUrl: _media.Resolve(item.Avatar?.Url),
            id: item.DocumentId);

        IReadOnlyList<Article> articles = item.Articles
            .Where(article => article?.Title is not null)
            .Select(article => new Article(
                id: article!.DocumentId,
                title: article.Title!,
                description: article.Description,
                slug: string.IsNullOrWhiteSpace(article.Slug) ? null : new Slug(article.Slug),
                author: author,
                category: article.Category?.Name is null
                    ? null
                    : new Category(
                        article.Category.Name,
                        string.IsNullOrWhiteSpace(article.Category.Slug)
                            ? null
                            : new Slug(article.Category.Slug)),
                tags: null,
                publishedAt: article.PublishedAt,
                coverImageUrl: _media.Resolve(article.Cover?.Url)))
            .ToList();

        return (author, articles);
    }
}
