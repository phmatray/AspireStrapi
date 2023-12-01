namespace AspireStrapi.AppHost.Resources.Strapi;

/// <summary>
/// A resource that represents a Strapi container.
/// </summary>
/// <param name="name">The name of the resource.</param>
public class StrapiContainerResource(string name)
    : ContainerResource(name), IStrapiResource
{
    /// <summary>
    /// Gets the connection string for the Strapi server.
    /// </summary>
    /// <returns>A connection string for the Strapi server in the form "http://host:port".</returns>
    public string? GetConnectionString()
    {
        if (!this.TryGetAllocatedEndPoints(out var allocatedEndpoints))
        {
            throw new DistributedApplicationException($"Strapi resource \"{Name}\" does not have endpoint annotation.");
        }
        
        var endpoint = allocatedEndpoints.Single();
        return $"http://{endpoint.EndPointString}";
    }
}