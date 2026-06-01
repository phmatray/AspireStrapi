using AspireStrapi.Domain.Entities;

namespace AspireStrapi.Application.Ports;

/// <summary>
/// Driven port: a source of <see cref="Author"/> data together with each author's
/// articles. Implemented by an outbound adapter in the Infrastructure layer.
/// </summary>
public interface IAuthorRepository
{
    /// <summary>
    /// Returns every author paired with the articles attributed to them.
    /// </summary>
    Task<IReadOnlyList<(Author Author, IReadOnlyList<Article> Articles)>> GetAuthorsWithArticlesAsync(
        CancellationToken cancellationToken = default);
}
