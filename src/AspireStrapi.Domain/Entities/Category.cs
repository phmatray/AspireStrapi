using AspireStrapi.Domain.ValueObjects;

namespace AspireStrapi.Domain.Entities;

/// <summary>
/// A grouping that classifies articles by topic.
/// </summary>
public sealed class Category
{
    public Category(string name, Slug? slug = null, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Category name is required.", nameof(name));
        }

        Name = name.Trim();
        Slug = slug;
        Description = description;
    }

    public string Name { get; }

    public Slug? Slug { get; }

    public string? Description { get; }
}
