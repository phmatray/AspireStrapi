using AspireStrapi.Domain.ValueObjects;

namespace AspireStrapi.Domain.Entities;

/// <summary>
/// The author of one or more <see cref="Article"/> items.
/// </summary>
public sealed class Author
{
    public Author(string name, EmailAddress? email = null, string? avatarUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Author name is required.", nameof(name));
        }

        Name = name.Trim();
        Email = email;
        AvatarUrl = avatarUrl;
    }

    public string Name { get; }

    public EmailAddress? Email { get; }

    public string? AvatarUrl { get; }
}
