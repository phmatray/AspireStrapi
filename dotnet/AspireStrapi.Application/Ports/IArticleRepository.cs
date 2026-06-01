using AspireStrapi.Domain.Entities;

namespace AspireStrapi.Application.Ports;

/// <summary>
/// Driven port: a source of <see cref="Article"/> data the application depends on.
/// Implemented by an outbound adapter in the Infrastructure layer.
/// </summary>
public interface IArticleRepository
{
    Task<IReadOnlyList<Article>> GetArticlesAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns the articles that belong to the category with the given slug.</summary>
    Task<IReadOnlyList<Article>> GetArticlesByCategorySlugAsync(
        string categorySlug,
        CancellationToken cancellationToken = default);

    /// <summary>Returns a single article by its Strapi documentId, or <c>null</c> if not found.</summary>
    Task<Article?> GetArticleByIdAsync(
        string documentId,
        CancellationToken cancellationToken = default);
}
