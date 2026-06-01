namespace AspireStrapi.Application.Dtos;

/// <summary>
/// A read model describing a category and how many articles it groups.
/// </summary>
public sealed record CategoryDto(
    string Id,
    string Name,
    string? Slug,
    string? Description,
    int ArticleCount);
