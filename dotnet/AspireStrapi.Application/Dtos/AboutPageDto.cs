namespace AspireStrapi.Application.Dtos;

/// <summary>
/// A read model describing the about page for presentation.
/// </summary>
public sealed record AboutPageDto(
    string Title,
    DateTimeOffset? CreatedAt);
