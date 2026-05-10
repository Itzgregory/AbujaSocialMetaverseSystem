namespace AbujaSocialMetaverse.Shared.Constants;

/// <summary>
/// Application-wide constants.
/// No magic numbers or strings anywhere else in the codebase.
/// </summary>
public static class AppConstants
{
    public static class Pagination
    {
        public const int DefaultPage = 1;
        public const int DefaultPageSize = 20;
        public const int MaxPageSize = 100;
    }

    public static class Avatar
    {
        public const int MaxPolygonCount = 10000;
        public const int PositionDecimalPlaces = 4;
        public const float MinMovementThresholdMeters = 0.5f;
    }

    public static class Chat
    {
        public const int MaxMessageLengthChars = 500;
        public const int MaxSessionsPerUser = 10;
    }

    public static class Business
    {
        public const int MaxNameLengthChars = 100;
        public const int MaxDescriptionLengthChars = 1000;
        public const int MaxImagesPerListing = 10;
        public const double AbujaMinLatitude = 8.4;
        public const double AbujaMaxLatitude = 9.4;
        public const double AbujaMinLongitude = 6.7;
        public const double AbujaMaxLongitude = 7.9;
    }

    public static class Security
    {
        public const int MinPasswordLength = 8;
        public const int MaxPasswordLength = 128;
        public const int MaxFailedLoginAttempts = 5;
        public const int LockoutDurationMinutes = 15;
        public const int BcryptWorkFactor = 12;
    }

    public static class Recommendation
    {
        public const int MaxResultsHardLimit = 100;
        public const int MinRelevanceScore = 0;
        public const int MaxRelevanceScore = 100;
    }

    public static class Cache
    {
        public const int DistributedLockTimeoutSeconds = 30;
        public const int StampedeRetryDelayMs = 50;
    }

    public static class Abuja
    {
        // Geographic center of Abuja — used as default spawn point
        public const double CenterLatitude = 9.0579;
        public const double CenterLongitude = 7.4951;
        public const double BoundaryRadiusKm = 50.0;
    }
}