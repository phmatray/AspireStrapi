using AspireStrapi.Application.Dtos;
using AspireStrapi.Application.Ports;
using AspireStrapi.Domain.Entities;

namespace AspireStrapi.Application.UseCases;

/// <summary>
/// Default implementation of <see cref="IContentService"/>. Orchestrates the
/// driven repository ports and maps domain entities to presentation DTOs.
/// </summary>
public sealed class ContentService : IContentService
{
    private readonly IArticleRepository _articleRepository;
    private readonly IAboutPageRepository _aboutPageRepository;

    public ContentService(
        IArticleRepository articleRepository,
        IAboutPageRepository aboutPageRepository)
    {
        _articleRepository = articleRepository;
        _aboutPageRepository = aboutPageRepository;
    }

    public async Task<IReadOnlyList<ArticleDto>> GetArticlesAsync(
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Article> articles =
            await _articleRepository.GetArticlesAsync(cancellationToken);

        return articles.Select(ToDto).ToList();
    }

    public async Task<AboutPageDto?> GetAboutAsync(
        CancellationToken cancellationToken = default)
    {
        AboutPage? about = await _aboutPageRepository.GetAboutAsync(cancellationToken);

        return about is null ? null : new AboutPageDto(about.Title, about.CreatedAt);
    }

    private static ArticleDto ToDto(Article article) => new(
        article.Id,
        article.Title,
        article.Description,
        article.Author?.Name,
        article.Category?.Name,
        article.Tags.Select(tag => tag.Name).ToList(),
        article.PublishedAt);
}
