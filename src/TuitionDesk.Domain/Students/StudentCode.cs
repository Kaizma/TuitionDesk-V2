using TuitionDesk.Domain.Common;

namespace TuitionDesk.Domain.Students;

public static class StudentCodes
{
    public const string Prefix = "STU-";

    public static string Format(int number) => $"{Prefix}{number:00000}";

    public static string Normalize(string? value)
    {
        var cleaned = value?.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(cleaned) ||
            !cleaned.StartsWith(Prefix, StringComparison.Ordinal) ||
            cleaned.Length < Prefix.Length + 5 ||
            !cleaned[Prefix.Length..].All(char.IsAsciiDigit))
        {
            throw new DomainValidationException(
                "student.invalid_code",
                "Student code must look like STU-00001.");
        }

        return cleaned;
    }
}
