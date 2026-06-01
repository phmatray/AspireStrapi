namespace AspireStrapi.Infrastructure.Adapters;

/// <summary>
/// Turns the relative media URLs returned by Strapi (for example
/// <c>/uploads/cover_abc.png</c>) into absolute URLs the browser can load.
/// Strapi serves uploaded files from the root of its host.
/// </summary>
/// <remarks>
/// The media base must be a <em>browser-reachable</em> host. In a containerised
/// deployment the GraphQL endpoint the server calls (e.g. <c>http://strapi:1337</c>)
/// is only resolvable inside the Docker network, so a separate public base URL
/// (e.g. <c>http://127.0.0.1:1337</c>) is supplied for media.
/// </remarks>
public sealed class StrapiMediaUrlResolver
{
    private readonly Uri _mediaBaseUri;

    public StrapiMediaUrlResolver(Uri mediaBaseUri)
    {
        ArgumentNullException.ThrowIfNull(mediaBaseUri);

        // Media lives at the scheme/host/port root, regardless of any path.
        _mediaBaseUri = new Uri(mediaBaseUri.GetLeftPart(UriPartial.Authority) + "/");
    }

    /// <summary>
    /// Returns an absolute URL for the supplied media path, or <c>null</c> when
    /// the path is empty. Already-absolute HTTP(S) URLs are returned unchanged.
    /// </summary>
    public string? Resolve(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        // Only an http/https URL counts as "already absolute". A rooted path
        // such as "/uploads/x.jpg" parses as an absolute file:// URI on Unix,
        // so we must NOT short-circuit on UriKind.Absolute alone.
        if (Uri.TryCreate(path, UriKind.Absolute, out Uri? absolute)
            && (absolute.Scheme == Uri.UriSchemeHttp || absolute.Scheme == Uri.UriSchemeHttps))
        {
            return absolute.ToString();
        }

        string relative = path.StartsWith('/') ? path[1..] : path;
        return new Uri(_mediaBaseUri, relative).ToString();
    }
}
