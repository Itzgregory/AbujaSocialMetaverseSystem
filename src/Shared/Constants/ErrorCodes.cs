namespace AbujaSocialMetaverse.Shared.Constants;

/// <summary>
/// Centralised error codes for the entire system.
/// Machine-readable, domain-organized.
/// Never use magic strings — always reference from here.
/// </summary>
public static class ErrorCodes
{
    public static class Auth
    {
        public const string InvalidCredentials = "AUTH_INVALID_CREDENTIALS";
        public const string TokenExpired = "AUTH_TOKEN_EXPIRED";
        public const string TokenInvalid = "AUTH_TOKEN_INVALID";
        public const string TokenRevoked = "AUTH_TOKEN_REVOKED";
        public const string RefreshTokenExpired = "AUTH_REFRESH_TOKEN_EXPIRED";
        public const string RefreshTokenInvalid = "AUTH_REFRESH_TOKEN_INVALID";
        public const string Unauthorized = "AUTH_UNAUTHORIZED";
        public const string InsufficientPermissions = "AUTH_INSUFFICIENT_PERMISSIONS";
        public const string AccountLocked = "AUTH_ACCOUNT_LOCKED";
        public const string AccountNotVerified = "AUTH_ACCOUNT_NOT_VERIFIED";
    }

    public static class User
    {
        public const string NotFound = "USER_NOT_FOUND";
        public const string AlreadyExists = "USER_ALREADY_EXISTS";
        public const string EmailAlreadyExists = "USER_EMAIL_ALREADY_EXISTS";
        public const string InvalidEmail = "USER_INVALID_EMAIL";
        public const string InvalidPassword = "USER_INVALID_PASSWORD";
        public const string PasswordTooWeak = "USER_PASSWORD_TOO_WEAK";
        public const string ProfileIncomplete = "USER_PROFILE_INCOMPLETE";
        public const string InvalidMode = "USER_INVALID_MODE";
        public const string CannotInteractWithSelf = "USER_CANNOT_INTERACT_WITH_SELF";
    }

    public static class Business
    {
        public const string NotFound = "BUSINESS_NOT_FOUND";
        public const string AlreadyExists = "BUSINESS_ALREADY_EXISTS";
        public const string NotApproved = "BUSINESS_NOT_APPROVED";
        public const string InvalidCategory = "BUSINESS_INVALID_CATEGORY";
        public const string InvalidCoordinates = "BUSINESS_INVALID_COORDINATES";
        public const string InactiveSubscription = "BUSINESS_INACTIVE_SUBSCRIPTION";
        public const string AnalyticsNotFound = "BUSINESS_ANALYTICS_NOT_FOUND";
    }

    public static class Social
    {
        public const string AvatarNotFound = "SOCIAL_AVATAR_NOT_FOUND";
        public const string SessionNotFound = "SOCIAL_SESSION_NOT_FOUND";
        public const string SessionAlreadyActive = "SOCIAL_SESSION_ALREADY_ACTIVE";
        public const string IncompatibleUsers = "SOCIAL_INCOMPATIBLE_USERS";
        public const string ProximityNotMet = "SOCIAL_PROXIMITY_NOT_MET";
        public const string UserNotOnline = "SOCIAL_USER_NOT_ONLINE";
        public const string ChatMessageTooLong = "SOCIAL_CHAT_MESSAGE_TOO_LONG";
        public const string InteractionRejected = "SOCIAL_INTERACTION_REJECTED";
    }

    public static class Map
    {
        public const string TileFetchFailed = "MAP_TILE_FETCH_FAILED";
        public const string GeocodingFailed = "MAP_GEOCODING_FAILED";
        public const string InvalidCoordinates = "MAP_INVALID_COORDINATES";
        public const string RegionNotFound = "MAP_REGION_NOT_FOUND";
        public const string OutOfBounds = "MAP_OUT_OF_BOUNDS";
    }

    public static class Payment
    {
        public const string NotFound = "PAYMENT_NOT_FOUND";
        public const string ProcessingFailed = "PAYMENT_PROCESSING_FAILED";
        public const string SubscriptionNotFound = "PAYMENT_SUBSCRIPTION_NOT_FOUND";
        public const string SubscriptionAlreadyActive = "PAYMENT_SUBSCRIPTION_ALREADY_ACTIVE";
        public const string SubscriptionExpired = "PAYMENT_SUBSCRIPTION_EXPIRED";
        public const string InvalidWebhookSignature = "PAYMENT_INVALID_WEBHOOK_SIGNATURE";
        public const string InvoiceNotFound = "PAYMENT_INVOICE_NOT_FOUND";
        public const string RefundFailed = "PAYMENT_REFUND_FAILED";
    }

    public static class Privacy
    {
        public const string ConsentRequired = "PRIVACY_CONSENT_REQUIRED";
        public const string ConsentAlreadyGranted = "PRIVACY_CONSENT_ALREADY_GRANTED";
        public const string ConsentAlreadyWithdrawn = "PRIVACY_CONSENT_ALREADY_WITHDRAWN";
        public const string ErasureRequestNotFound = "PRIVACY_ERASURE_REQUEST_NOT_FOUND";
        public const string ErasureAlreadyRequested = "PRIVACY_ERASURE_ALREADY_REQUESTED";
        public const string AuditLogWriteFailed = "PRIVACY_AUDIT_LOG_WRITE_FAILED";
    }

    public static class Admin
    {
        public const string NotFound = "ADMIN_NOT_FOUND";
        public const string InsufficientPermissions = "ADMIN_INSUFFICIENT_PERMISSIONS";
        public const string ModerationActionFailed = "ADMIN_MODERATION_ACTION_FAILED";
        public const string ReportNotFound = "ADMIN_REPORT_NOT_FOUND";
        public const string BusinessApprovalFailed = "ADMIN_BUSINESS_APPROVAL_FAILED";
    }

    public static class Validation
    {
        public const string InvalidInput = "VALIDATION_INVALID_INPUT";
        public const string RequiredFieldMissing = "VALIDATION_REQUIRED_FIELD_MISSING";
        public const string InvalidFormat = "VALIDATION_INVALID_FORMAT";
        public const string OutOfRange = "VALIDATION_OUT_OF_RANGE";
        public const string InvalidGuid = "VALIDATION_INVALID_GUID";
        public const string InvalidPagination = "VALIDATION_INVALID_PAGINATION";
    }

    public static class Cache
    {
        public const string Unavailable = "CACHE_UNAVAILABLE";
        public const string SerializationFailed = "CACHE_SERIALIZATION_FAILED";
        public const string StampedePrevented = "CACHE_STAMPEDE_PREVENTED";
    }

    public static class RealTime
    {
        public const string ConnectionFailed = "REALTIME_CONNECTION_FAILED";
        public const string UserNotConnected = "REALTIME_USER_NOT_CONNECTED";
        public const string RegionFull = "REALTIME_REGION_FULL";
        public const string MessageDeliveryFailed = "REALTIME_MESSAGE_DELIVERY_FAILED";
    }
}