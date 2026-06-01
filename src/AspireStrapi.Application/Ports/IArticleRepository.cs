using AspireStrapi.Domain.Entities;

namespace AspireStrapi.Application.Ports;

/// <summary>
/// Driven port: a source of <see cref="Article"/> data the application depends on.
/// Implemented by an outbound adapter in the Infrastructure layer.
/// </summary>
public interface IArticleRepository
{
    Task<IReadOnlyList<Article>> GetArticlesAsync(CancellationToken cancellationToken = default);
}
