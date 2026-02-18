namespace AspireStrapi.AppHost.Resources.Strapi;

/// <summary>
/// A resource that represents a Strapi container.
/// </summary>
/// <param name="name">The name of the resource.</param>
public class StrapiContainerResource(string name)
    : ContainerResource(name), IStrapiResource
{
    /// <summary>
    /// Gets the connection string expression for the Strapi server.
    /// </summary>
    public ReferenceExpression ConnectionStringExpression => 
        ReferenceExpression.Create($"http://{{{Name}.bindings.http.host}}:{{{Name}.bindings.http.port}}");

    /// <summary>
    /// Gets the connection string for the Strapi server.
    /// </summary>
    /// <returns>A connection string for the Strapi server in the form "http://host:port".</returns>
    public string? GetConnectionString()
    {
        // In Aspire 9, we return null here as the actual connection string 
        // is resolved at runtime through the ConnectionStringExpression
        return null;
    }
}