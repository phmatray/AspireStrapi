using AspireStrapi.Domain.Entities;

namespace AspireStrapi.Application.Ports;

/// <summary>
/// Driven port: a source of <see cref="Category"/> data, including article counts.
/// Implemented by an outbound adapter in the Infrastructure layer.
/// </summary>
public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default);
}
