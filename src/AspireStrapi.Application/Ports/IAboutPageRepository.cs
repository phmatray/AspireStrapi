using AspireStrapi.Domain.Entities;

namespace AspireStrapi.Application.Ports;

/// <summary>
/// Driven port: a source of the <see cref="AboutPage"/> content.
/// Implemented by an outbound adapter in the Infrastructure layer.
/// </summary>
public interface IAboutPageRepository
{
    Task<AboutPage?> GetAboutAsync(CancellationToken cancellationToken = default);
}
