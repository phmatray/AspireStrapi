namespace AspireStrapi.Application.Dtos;

/// <summary>
/// A read model describing an article for presentation.
/// </summary>
public sealed record ArticleDto(
    string Id,
    string Title,
    string? Description,
    string? AuthorName,
    string? CategoryName,
    IReadOnlyList<string> Tags,
    DateTimeOffset? PublishedAt);
