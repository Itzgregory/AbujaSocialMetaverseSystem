using AbujaSocialMetaverse.Shared.Constants;

namespace AbujaSocialMetaverse.Shared.Helpers;

/// <summary>
/// Centralised guard clauses for defensive method entry validation.
/// Every service method entry point uses Guard before any logic runs.
/// Throws ArgumentException variants — caught by GlobalExceptionMiddleware.
/// </summary>
public static class Guard
{
    public static class Against
    {
        /// <summary>Throws if value is null.</summary>
        public static T Null<T>(T? value, string paramName) where T : class
        {
            if (value is null)
                throw new ArgumentNullException(paramName,
                    $"'{paramName}' cannot be null.");
            return value;
        }

        /// <summary>Throws if string is null or whitespace.</summary>
        public static string NullOrWhiteSpace(string? value, string paramName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(
                    $"'{paramName}' cannot be null or empty.", paramName);
            return value;
        }

        /// <summary>Throws if Guid is empty.</summary>
        public static Guid EmptyGuid(Guid value, string paramName)
        {
            if (value == Guid.Empty)
                throw new ArgumentException(
                    $"'{paramName}' cannot be an empty Guid.", paramName);
            return value;
        }

        /// <summary>Throws if value is outside the specified range.</summary>
        public static T OutOfRange<T>(T value, string paramName, T min, T max)
            where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
                throw new ArgumentOutOfRangeException(paramName,
                    $"'{paramName}' must be between {min} and {max}. Actual: {value}.");
            return value;
        }

        /// <summary>Throws if value is less than or equal to zero.</summary>
        public static T NegativeOrZero<T>(T value, string paramName)
            where T : IComparable<T>
        {
            if (value.CompareTo(default(T)) <= 0)
                throw new ArgumentOutOfRangeException(paramName,
                    $"'{paramName}' must be greater than zero. Actual: {value}.");
            return value;
        }

        /// <summary>Throws if collection is null or empty.</summary>
        public static IEnumerable<T> NullOrEmpty<T>(
            IEnumerable<T>? collection, string paramName)
        {
            if (collection is null || !collection.Any())
                throw new ArgumentException(
                    $"'{paramName}' cannot be null or empty.", paramName);
            return collection;
        }

        /// <summary>Throws if coordinates are outside Abuja boundaries.</summary>
        public static void OutsideAbuja(double latitude, double longitude)
        {
            if (latitude < AppConstants.Business.AbujaMinLatitude ||
                latitude > AppConstants.Business.AbujaMaxLatitude ||
                longitude < AppConstants.Business.AbujaMinLongitude ||
                longitude > AppConstants.Business.AbujaMaxLongitude)
            {
                throw new ArgumentException(
                    $"Coordinates ({latitude}, {longitude}) are outside Abuja boundaries. " +
                    $"Latitude must be between {AppConstants.Business.AbujaMinLatitude} " +
                    $"and {AppConstants.Business.AbujaMaxLatitude}. " +
                    $"Longitude must be between {AppConstants.Business.AbujaMinLongitude} " +
                    $"and {AppConstants.Business.AbujaMaxLongitude}.");
            }
        }

        /// <summary>Throws if pagination parameters are invalid.</summary>
        public static void InvalidPagination(int page, int pageSize)
        {
            if (page < 1)
                throw new ArgumentOutOfRangeException(nameof(page),
                    $"Page must be at least 1. Actual: {page}.");

            if (pageSize < 1 || pageSize > AppConstants.Pagination.MaxPageSize)
                throw new ArgumentOutOfRangeException(nameof(pageSize),
                    $"PageSize must be between 1 and {AppConstants.Pagination.MaxPageSize}. " +
                    $"Actual: {pageSize}.");
        }

        /// <summary>
        /// Throws if string exceeds maximum length.
        /// </summary>
        public static string ExceedsMaxLength(string value, string paramName, int maxLength)
        {
            NullOrWhiteSpace(value, paramName);
            if (value.Length > maxLength)
                throw new ArgumentException(
                    $"'{paramName}' cannot exceed {maxLength} characters. " +
                    $"Actual length: {value.Length}.", paramName);
            return value;
        }

        /// <summary>
        /// Throws if the condition is true.
        /// Use for business rule violations.
        /// </summary>
        public static void InvalidState(bool condition, string message)
        {
            if (condition)
                throw new InvalidOperationException(message);
        }
    }
}