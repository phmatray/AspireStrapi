namespace AspireStrapi.Application.Dtos;

/// <summary>
/// A read model describing an author together with the articles they have written.
/// </summary>
public sealed record AuthorDto(
    string Id,
    string Name,
    string? Email,
    string? AvatarUrl,
    IReadOnlyList<ArticleDto> Articles);
