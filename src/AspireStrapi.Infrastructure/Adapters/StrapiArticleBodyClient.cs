using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AspireStrapi.Infrastructure.Adapters;

/// <summary>
/// Fetches an article's dynamic-zone <c>blocks</c> with a hand-written GraphQL
/// request. The typed StrawberryShake client cannot model the Strapi dynamic
/// zone union reliably (its generated mapper mishandles the union), so this
/// small adapter issues the query directly and renders the result to HTML.
/// Living in the Infrastructure layer, it stays behind the article repository
/// port and never leaks to the application.
/// </summary>
public sealed class StrapiArticleBodyClient
{
    private const string Query = """
        query ArticleBlocks($documentId: ID!) {
          articles(filters: { documentId: { eq: $documentId } }) {
            blocks {
              __typename
              ... on ComponentSharedRichText { body }
              ... on ComponentSharedQuote { title body }
              ... on ComponentSharedMedia { file { url alternativeText } }
            }
          }
        }
        """;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        // camelCase on serialize so the request body is {"query":...,"variables":...}
        // (GraphQL requires lowercase keys); case-insensitive on deserialize.
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _http;
    private readonly StrapiMediaUrlResolver _media;

    public StrapiArticleBodyClient(HttpClient http, StrapiMediaUrlResolver media)
    {
        _http = http;
        _media = media;
    }

    public async Task<string?> GetBodyHtmlAsync(
        string documentId,
        CancellationToken cancellationToken = default)
    {
        var request = new GraphQlRequest(Query, new Variables(documentId));

        using HttpResponseMessage response =
            await _http.PostAsJsonAsync(string.Empty, request, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();

        GraphQlResponse? payload = await response.Content
            .ReadFromJsonAsync<GraphQlResponse>(JsonOptions, cancellationToken);

        IReadOnlyList<Block>? blocks = payload?.Data?.Articles?.FirstOrDefault()?.Blocks;
        return blocks is null ? null : ArticleBodyRenderer.Render(blocks, _media);
    }

    private sealed record GraphQlRequest(string Query, Variables Variables);

    private sealed record Variables([property: JsonPropertyName("documentId")] string DocumentId);

    private sealed class GraphQlResponse
    {
        public ResponseData? Data { get; init; }
    }

    private sealed class ResponseData
    {
        public IReadOnlyList<ArticleNode>? Articles { get; init; }
    }

    private sealed class ArticleNode
    {
        public IReadOnlyList<Block>? Blocks { get; init; }
    }

    internal sealed class Block
    {
        [JsonPropertyName("__typename")]
        public string? TypeName { get; init; }

        public string? Title { get; init; }

        public string? Body { get; init; }

        public MediaFile? File { get; init; }
    }

    internal sealed class MediaFile
    {
        public string? Url { get; init; }

        public string? AlternativeText { get; init; }
    }
}
