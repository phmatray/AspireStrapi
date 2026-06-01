namespace AspireStrapi.Infrastructure.Adapters;

/// <summary>
/// Turns the relative media URLs returned by Strapi (for example
/// <c>/uploads/cover_abc.png</c>) into absolute URLs the browser can load.
/// Strapi serves uploaded files from the root of the same host that exposes
/// the GraphQL endpoint.
/// </summary>
public sealed class StrapiMediaUrlResolver
{
    private readonly Uri _mediaBaseUri;

    public StrapiMediaUrlResolver(Uri graphQlEndpoint)
    {
        ArgumentNullException.ThrowIfNull(graphQlEndpoint);

        // Strip the GraphQL path: media lives at the scheme/host/port root.
        _mediaBaseUri = new Uri(graphQlEndpoint.GetLeftPart(UriPartial.Authority) + "/");
    }

    /// <summary>
    /// Returns an absolute URL for the supplied media path, or <c>null</c> when
    /// the path is empty. Already-absolute URLs are returned unchanged.
    /// </summary>
    public string? Resolve(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        if (Uri.TryCreate(path, UriKind.Absolute, out Uri? absolute))
        {
            return absolute.ToString();
        }

        string relative = path.StartsWith('/') ? path[1..] : path;
        return new Uri(_mediaBaseUri, relative).ToString();
    }
}
