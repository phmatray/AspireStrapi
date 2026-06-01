using AspireStrapi.Application.Dtos;

namespace AspireStrapi.Application.UseCases;

/// <summary>
/// Driving port: the use-cases the presentation layer invokes.
/// </summary>
public interface IContentService
{
    Task<IReadOnlyList<ArticleDto>> GetArticlesAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns the published articles in the category identified by <paramref name="categorySlug"/>.</summary>
    Task<IReadOnlyList<ArticleDto>> GetArticlesByCategoryAsync(
        string categorySlug,
        CancellationToken cancellationToken = default);

    /// <summary>Returns a single article (with its rich-text body) or <c>null</c> if not found.</summary>
    Task<ArticleDetailDto?> GetArticleAsync(
        string documentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuthorDto>> GetAuthorsAsync(CancellationToken cancellationToken = default);

    Task<AboutPageDto?> GetAboutAsync(CancellationToken cancellationToken = default);
}
