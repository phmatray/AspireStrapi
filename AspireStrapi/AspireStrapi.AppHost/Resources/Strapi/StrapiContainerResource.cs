namespace AspireStrapi.AppHost.Resources.Strapi;

/// <summary>
/// A resource that represents a Strapi container.
/// </summary>
/// <param name="name">The name of the resource.</param>
public class StrapiContainerResource(string name)
    : ContainerResource(name), IStrapiResource
{
    internal const string PrimaryEndpointName = "http";

    private EndpointReference? _primaryEndpoint;

    /// <summary>
    /// Gets the primary endpoint for the Strapi server.
    /// </summary>
    public EndpointReference PrimaryEndpoint =>
        _primaryEndpoint ??= new(this, PrimaryEndpointName);

    /// <summary>
    /// Gets the connection string expression for the Strapi server
    /// in the form "http://host:port".
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"http://{PrimaryEndpoint.Property(EndpointProperty.Host)}:{PrimaryEndpoint.Property(EndpointProperty.Port)}");
}
