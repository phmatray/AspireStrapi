namespace AspireStrapi.Application.Dtos;

/// <summary>
/// A read model describing a single article in full, including its rich-text body.
/// </summary>
public sealed record ArticleDetailDto(
    string Id,
    string Title,
    string? Description,
    string? Slug,
    string? BodyHtml,
    string? CoverImageUrl,
    string? AuthorName,
    string? AuthorAvatarUrl,
    string? CategoryName,
    string? CategorySlug,
    IReadOnlyList<string> Tags,
    DateTimeOffset? PublishedAt);
