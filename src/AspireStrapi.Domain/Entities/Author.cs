using AspireStrapi.Domain.ValueObjects;

namespace AspireStrapi.Domain.Entities;

/// <summary>
/// The author of one or more <see cref="Article"/> items.
/// </summary>
public sealed class Author
{
    public Author(string name, EmailAddress? email = null, string? avatarUrl = null, string? id = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Author name is required.", nameof(name));
        }

        Id = id;
        Name = name.Trim();
        Email = email;
        AvatarUrl = avatarUrl;
    }

    /// <summary>The Strapi documentId, when the author was loaded as a top-level record.</summary>
    public string? Id { get; }

    public string Name { get; }

    public EmailAddress? Email { get; }

    public string? AvatarUrl { get; }
}
