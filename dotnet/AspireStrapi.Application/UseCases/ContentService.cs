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
    private readonly ICategoryRepository _categoryRepository;
    private readonly IAuthorRepository _authorRepository;
    private readonly IAboutPageRepository _aboutPageRepository;

    public ContentService(
        IArticleRepository articleRepository,
        ICategoryRepository categoryRepository,
        IAuthorRepository authorRepository,
        IAboutPageRepository aboutPageRepository)
    {
        _articleRepository = articleRepository;
        _categoryRepository = categoryRepository;
        _authorRepository = authorRepository;
        _aboutPageRepository = aboutPageRepository;
    }

    public async Task<IReadOnlyList<ArticleDto>> GetArticlesAsync(
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Article> articles =
            await _articleRepository.GetArticlesAsync(cancellationToken);

        return articles.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<ArticleDto>> GetArticlesByCategoryAsync(
        string categorySlug,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Article> articles =
            await _articleRepository.GetArticlesByCategorySlugAsync(categorySlug, cancellationToken);

        return articles.Select(ToDto).ToList();
    }

    public async Task<ArticleDetailDto?> GetArticleAsync(
        string documentId,
        CancellationToken cancellationToken = default)
    {
        Article? article =
            await _articleRepository.GetArticleByIdAsync(documentId, cancellationToken);

        return article is null ? null : ToDetailDto(article);
    }

    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Category> categories =
            await _categoryRepository.GetCategoriesAsync(cancellationToken);

        return categories.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<AuthorDto>> GetAuthorsAsync(
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<(Author Author, IReadOnlyList<Article> Articles)> authors =
            await _authorRepository.GetAuthorsWithArticlesAsync(cancellationToken);

        return authors
            .Select(pair => new AuthorDto(
                pair.Author.Id ?? pair.Author.Name,
                pair.Author.Name,
                pair.Author.Email?.Value,
                pair.Author.AvatarUrl,
                pair.Articles.Select(ToDto).ToList()))
            .ToList();
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
        article.Slug?.Value,
        article.Author?.Name,
        article.Author?.AvatarUrl,
        article.Category?.Name,
        article.Category?.Slug?.Value,
        article.CoverImageUrl,
        article.Tags.Select(tag => tag.Name).ToList(),
        article.PublishedAt);

    private static ArticleDetailDto ToDetailDto(Article article) => new(
        article.Id,
        article.Title,
        article.Description,
        article.Slug?.Value,
        article.Body,
        article.CoverImageUrl,
        article.Author?.Name,
        article.Author?.AvatarUrl,
        article.Category?.Name,
        article.Category?.Slug?.Value,
        article.Tags.Select(tag => tag.Name).ToList(),
        article.PublishedAt);

    private static CategoryDto ToDto(Category category) => new(
        category.Id ?? category.Slug?.Value ?? category.Name,
        category.Name,
        category.Slug?.Value,
        category.Description,
        category.ArticleCount);
}
