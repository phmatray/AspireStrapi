using AspireStrapi.Domain.ValueObjects;

namespace AspireStrapi.Domain.Entities;

/// <summary>
/// A blog article aggregate root.
/// </summary>
public sealed class Article
{
    private readonly List<Tag> _tags;

    public Article(
        string id,
        string title,
        string? description = null,
        Slug? slug = null,
        Author? author = null,
        Category? category = null,
        IEnumerable<Tag>? tags = null,
        DateTimeOffset? publishedAt = null,
        string? coverImageUrl = null,
        string? body = null)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Article id is required.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Article title is required.", nameof(title));
        }

        Id = id;
        Title = title.Trim();
        Description = description;
        Slug = slug;
        Author = author;
        Category = category;
        _tags = tags?.ToList() ?? [];
        PublishedAt = publishedAt;
        CoverImageUrl = coverImageUrl;
        Body = body;
    }

    public string Id { get; }

    public string Title { get; }

    public string? Description { get; }

    public Slug? Slug { get; }

    public Author? Author { get; }

    public Category? Category { get; }

    public IReadOnlyList<Tag> Tags => _tags;

    public DateTimeOffset? PublishedAt { get; }

    /// <summary>Absolute URL of the cover image, if one was authored.</summary>
    public string? CoverImageUrl { get; }

    /// <summary>The article body, rendered as HTML from the Strapi rich-text blocks.</summary>
    public string? Body { get; }

    public bool IsPublished => PublishedAt is not null;
}
