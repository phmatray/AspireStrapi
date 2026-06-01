namespace AspireStrapi.Application.Dtos;

/// <summary>
/// A read model describing an article for list/summary presentation.
/// </summary>
public sealed record ArticleDto(
    string Id,
    string Title,
    string? Description,
    string? Slug,
    string? AuthorName,
    string? AuthorAvatarUrl,
    string? CategoryName,
    string? CategorySlug,
    string? CoverImageUrl,
    IReadOnlyList<string> Tags,
    DateTimeOffset? PublishedAt);
