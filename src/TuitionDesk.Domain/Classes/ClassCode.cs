using TuitionDesk.Domain.Common;

namespace TuitionDesk.Domain.Classes;

public static class ClassCodes
{
    public const string Prefix = "CLS-";

    public static string Format(int number) => $"{Prefix}{number:0000}";

    public static string Normalize(string? value)
    {
        var cleaned = value?.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(cleaned) ||
            !cleaned.StartsWith(Prefix, StringComparison.Ordinal) ||
            cleaned.Length < Prefix.Length + 4 ||
            !cleaned[Prefix.Length..].All(char.IsAsciiDigit))
        {
            throw new DomainValidationException(
                "class.invalid_code",
                "Class code must look like CLS-0001.");
        }

        return cleaned;
    }
}
