using AspireStrapi.Domain.ValueObjects;

namespace AspireStrapi.Domain.Entities;

/// <summary>
/// A grouping that classifies articles by topic.
/// </summary>
public sealed class Category
{
    public Category(
        string name,
        Slug? slug = null,
        string? description = null,
        string? id = null,
        int articleCount = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Category name is required.", nameof(name));
        }

        Id = id;
        Name = name.Trim();
        Slug = slug;
        Description = description;
        ArticleCount = articleCount;
    }

    /// <summary>The Strapi documentId, when the category was loaded as a top-level record.</summary>
    public string? Id { get; }

    public string Name { get; }

    public Slug? Slug { get; }

    public string? Description { get; }

    /// <summary>The number of published articles in this category.</summary>
    public int ArticleCount { get; }
}
