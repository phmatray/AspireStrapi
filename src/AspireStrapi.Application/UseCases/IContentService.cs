using AspireStrapi.Application.Dtos;

namespace AspireStrapi.Application.UseCases;

/// <summary>
/// Driving port: the use-cases the presentation layer invokes.
/// </summary>
public interface IContentService
{
    Task<IReadOnlyList<ArticleDto>> GetArticlesAsync(CancellationToken cancellationToken = default);

    Task<AboutPageDto?> GetAboutAsync(CancellationToken cancellationToken = default);
}
