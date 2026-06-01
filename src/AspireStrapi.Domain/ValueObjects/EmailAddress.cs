namespace AspireStrapi.Domain.ValueObjects;

/// <summary>
/// A validated e-mail address.
/// </summary>
public readonly record struct EmailAddress
{
    public string Value { get; }

    public EmailAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !value.Contains('@'))
        {
            throw new ArgumentException("A valid e-mail address is required.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString() => Value;

    public static implicit operator string(EmailAddress email) => email.Value;
}
