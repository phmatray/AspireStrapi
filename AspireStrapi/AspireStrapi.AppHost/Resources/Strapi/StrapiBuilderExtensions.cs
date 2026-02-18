using System.Net.Sockets;
using System.Text.Json;

namespace AspireStrapi.AppHost.Resources.Strapi;

/// <summary>
/// Provides extension methods for adding Strapi resources to an <see cref="IDistributedApplicationBuilder"/>.
/// </summary>
public static class StrapiBuilderExtensions
{
    // The environment in which the application is running.
    private const string NodeEnvEnvVarName = "NODE_ENV";
    // The database client to use.
    private const string DatabaseClientEnvVarName = "DATABASE_CLIENT";
    // The database host.
    private const string DatabaseHostEnvVarName = "DATABASE_HOST";
    // The database port.
    private const string DatabasePortEnvVarName = "DATABASE_PORT";
    // The database name.
    private const string DatabaseNameEnvVarName = "DATABASE_NAME";
    // The database username.
    private const string DatabaseUsernameEnvVarName = "DATABASE_USERNAME";
    // The database password.
    private const string DatabasePasswordEnvVarName = "DATABASE_PASSWORD";
    // The secret used to sign the JWT for the Users-Permissions plugin.
    private const string JwtSecretEnvVarName = "JWT_SECRET";
    // The secret used to sign the JWT for the Admin panel.
    private const string AdminJwtSecretEnvVarName = "ADMIN_JWT_SECRET";
    // The secret keys used to sign the session cookies.
    private const string AppKeysEnvVarName = "APP_KEYS";
    
    
    /// <summary>
    /// Adds a Strapi container to the application model. The default image is "ghcr.io/linuxserver/plex" and the tag is "latest".
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    /// <param name="name">The name of the resource. This name will be used as the connection string name when referenced in a dependency.</param>
    /// <param name="port">The host port for Strapi.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{StrapiContainerResource}"/>.</returns>
    public static IResourceBuilder<StrapiContainerResource> AddStrapiContainer(
        this IDistributedApplicationBuilder builder,
        string name,
        int? port = null)
    {
        var plexContainer = new StrapiContainerResource(name);

        return builder
            .AddResource(plexContainer)
            // TODO: Use a custom image.
            .WithAnnotation(new ContainerImageAnnotation { Image = "ghcr.io/linuxserver/plex", Tag = "latest" })
            .WithHttpEndpoint(port: port, targetPort: 32400, name: "web")
            .PublishAsContainer()
            .WithEnvironment(NodeEnvEnvVarName, "production")
            .WithEnvironment(DatabaseClientEnvVarName, "mysql")
            .WithEnvironment(DatabaseHostEnvVarName, "mysql")
            .WithEnvironment(DatabasePortEnvVarName, "3306")
            .WithEnvironment(DatabaseNameEnvVarName, "strapi")
            .WithEnvironment(DatabaseUsernameEnvVarName, "strapi")
            .WithEnvironment(DatabasePasswordEnvVarName, "strapi");
    }
}