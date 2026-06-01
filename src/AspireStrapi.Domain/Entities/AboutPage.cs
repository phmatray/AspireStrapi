namespace AspireStrapi.Domain.Entities;

/// <summary>
/// The single "About" content page.
/// </summary>
public sealed class AboutPage
{
    public AboutPage(string title, DateTimeOffset? createdAt = null)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("About page title is required.", nameof(title));
        }

        Title = title.Trim();
        CreatedAt = createdAt;
    }

    public string Title { get; }

    public DateTimeOffset? CreatedAt { get; }
}
