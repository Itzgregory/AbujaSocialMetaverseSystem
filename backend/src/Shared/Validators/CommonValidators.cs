using System.Text.RegularExpressions;
using AbujaSocialMetaverse.Shared.Constants;

namespace AbujaSocialMetaverse.Shared.Validators;

/// <summary>
/// Reusable validation logic shared across all module validators.
/// FluentValidation validators in each module extend these.
/// </summary>
public static class CommonValidators
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex PhoneRegex = new(
        @"^\+?[1-9]\d{7,14}$",
        RegexOptions.Compiled);

    private static readonly Regex PasswordRegex = new(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        RegexOptions.Compiled);

    /// <summary>Returns true if the email format is valid.</summary>
    public static bool IsValidEmail(string? email)
        => !string.IsNullOrWhiteSpace(email) && EmailRegex.IsMatch(email);

    /// <summary>Returns true if the phone number format is valid.</summary>
    public static bool IsValidPhone(string? phone)
        => !string.IsNullOrWhiteSpace(phone) && PhoneRegex.IsMatch(phone);

    /// <summary>
    /// Returns true if the password meets complexity requirements:
    /// min 8 chars, uppercase, lowercase, digit, special character.
    /// </summary>
    public static bool IsValidPassword(string? password)
        => !string.IsNullOrWhiteSpace(password) && PasswordRegex.IsMatch(password);

    /// <summary>Returns true if the Guid is not empty.</summary>
    public static bool IsValidGuid(Guid id) => id != Guid.Empty;

    /// <summary>Returns true if coordinates are within Abuja boundaries.</summary>
    public static bool IsWithinAbuja(double latitude, double longitude)
        => latitude >= AppConstants.Business.AbujaMinLatitude &&
           latitude <= AppConstants.Business.AbujaMaxLatitude &&
           longitude >= AppConstants.Business.AbujaMinLongitude &&
           longitude <= AppConstants.Business.AbujaMaxLongitude;

    /// <summary>Returns true if pagination parameters are valid.</summary>
    public static bool IsValidPagination(int page, int pageSize)
        => page >= 1 &&
           pageSize >= 1 &&
           pageSize <= AppConstants.Pagination.MaxPageSize;

    /// <summary>
    /// Returns a list of password strength issues.
    /// Empty list means password is valid.
    /// </summary>
    public static IReadOnlyList<string> GetPasswordIssues(string? password)
    {
        var issues = new List<string>();

        if (string.IsNullOrWhiteSpace(password))
        {
            issues.Add("Password is required.");
            return issues;
        }

        if (password.Length < AppConstants.Security.MinPasswordLength)
            issues.Add($"Password must be at least {AppConstants.Security.MinPasswordLength} characters.");

        if (password.Length > AppConstants.Security.MaxPasswordLength)
            issues.Add($"Password cannot exceed {AppConstants.Security.MaxPasswordLength} characters.");

        if (!password.Any(char.IsUpper))
            issues.Add("Password must contain at least one uppercase letter.");

        if (!password.Any(char.IsLower))
            issues.Add("Password must contain at least one lowercase letter.");

        if (!password.Any(char.IsDigit))
            issues.Add("Password must contain at least one digit.");

        if (!password.Any(c => "@$!%*?&".Contains(c)))
            issues.Add("Password must contain at least one special character (@$!%*?&).");

        return issues;
    }
}