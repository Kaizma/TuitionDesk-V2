using System.Text.RegularExpressions;

namespace TuitionDesk.Domain.Common;

/// <summary>Small validation helpers shared by the domain entities.</summary>
internal static partial class Guard
{
    private static readonly Regex PhonePattern = new(@"^[0-9+\-() ]+$", RegexOptions.Compiled);
    private static readonly Regex EmailPattern = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public static string RequiredText(string? value, string code, string field, int min, int max)
    {
        var cleaned = value?.Trim();
        if (string.IsNullOrWhiteSpace(cleaned) || cleaned.Length < min || cleaned.Length > max)
        {
            throw new DomainValidationException(
                code,
                $"{field} must be between {min} and {max} characters.");
        }

        return cleaned;
    }

    public static string RequiredName(string? value, string code, string field, int min, int max)
    {
        var cleaned = RequiredText(value, code, field, min, max);
        if (!cleaned.Any(char.IsLetter))
        {
            throw new DomainValidationException(code, $"{field} must contain at least one letter.");
        }

        return cleaned;
    }

    public static string? OptionalText(string? value, string code, string field, int max)
    {
        var cleaned = value?.Trim();
        if (string.IsNullOrEmpty(cleaned))
        {
            return null;
        }

        if (cleaned.Length > max)
        {
            throw new DomainValidationException(code, $"{field} must be {max} characters or fewer.");
        }

        return cleaned;
    }

    public static string RequiredPhone(string? value, string code, string field)
    {
        var cleaned = RequiredText(value, code, field, 7, 20);
        if (!PhonePattern.IsMatch(cleaned))
        {
            throw new DomainValidationException(
                code,
                $"{field} may only contain digits, spaces, and + - ( ) characters.");
        }

        return cleaned;
    }

    public static string? OptionalPhone(string? value, string code, string field)
    {
        var cleaned = OptionalText(value, code, field, 20);
        if (cleaned is null)
        {
            return null;
        }

        if (cleaned.Length < 7 || !PhonePattern.IsMatch(cleaned))
        {
            throw new DomainValidationException(
                code,
                $"{field} must be 7–20 characters using digits, spaces, and + - ( ) only.");
        }

        return cleaned;
    }

    public static string? OptionalEmail(string? value, string code, string field)
    {
        var cleaned = OptionalText(value, code, field, 200);
        if (cleaned is null)
        {
            return null;
        }

        if (!EmailPattern.IsMatch(cleaned))
        {
            throw new DomainValidationException(code, $"{field} must be a valid email address.");
        }

        return cleaned;
    }

    public static string RequiredCurrency(string? value, string code)
    {
        var cleaned = RequiredText(value, code, "Currency", 3, 3).ToUpperInvariant();
        if (!cleaned.All(char.IsLetter))
        {
            throw new DomainValidationException(code, "Currency must be a three-letter ISO 4217 code.");
        }

        return cleaned;
    }

    public static DateOnly NotInFuture(DateOnly value, DateOnly today, string code, string field)
    {
        if (value > today)
        {
            throw new DomainValidationException(code, $"{field} cannot be in the future.");
        }

        return value;
    }

    public static DateOnly? NotInFuture(DateOnly? value, DateOnly today, string code, string field)
    {
        return value is null ? null : NotInFuture(value.Value, today, code, field);
    }

    public static long NonNegative(long value, string code, string field)
    {
        if (value < 0)
        {
            throw new DomainValidationException(code, $"{field} cannot be negative.");
        }

        return value;
    }

    public static int? PositiveCapacity(int? value, string code)
    {
        if (value is <= 0)
        {
            throw new DomainValidationException(code, "Capacity must be at least 1 when it is set.");
        }

        return value;
    }
}
