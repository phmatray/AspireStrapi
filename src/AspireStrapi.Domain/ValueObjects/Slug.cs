namespace AspireStrapi.Domain.ValueObjects;

/// <summary>
/// A URL-friendly identifier for a piece of content.
/// </summary>
public readonly record struct Slug
{
    public string Value { get; }

    public Slug(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Slug cannot be null or empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString() => Value;

    public static implicit operator string(Slug slug) => slug.Value;
}
