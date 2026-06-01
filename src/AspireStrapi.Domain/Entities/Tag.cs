using AspireStrapi.Domain.ValueObjects;

namespace AspireStrapi.Domain.Entities;

/// <summary>
/// A free-form label that can be attached to articles.
/// </summary>
public sealed class Tag
{
    public Tag(string name, Slug? slug = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Tag name is required.", nameof(name));
        }

        Name = name.Trim();
        Slug = slug;
    }

    public string Name { get; }

    public Slug? Slug { get; }
}
