namespace AbujaSocialMetaverse.Infrastructure.Caching;

public static class CacheKeys
{
    private const string Prefix = "asm:";

    /// <summary>
    /// User sessions, profiles, tokens, consent.
    /// TTL: Session = JWT expiry. Profile = 30 min. Token blacklist = JWT expiry.
    /// </summary>
    public static class Users
    {
        public static string Session(Guid userId) =>
            $"{Prefix}session:{userId}";

        public static string Profile(Guid userId) =>
            $"{Prefix}user:profile:{userId}";

        public static string TokenBlacklist(string jti) =>
            $"{Prefix}token:blacklist:{jti}";

        public static string RefreshToken(Guid userId) =>
            $"{Prefix}token:refresh:{userId}";

        public static string Consent(Guid userId) =>
            $"{Prefix}consent:{userId}";
    }

    /// <summary>
    /// Avatar positions and region membership.
    /// TTL: Location = 30 seconds (refreshed on each position update).
    /// Region membership = session lifetime.
    /// </summary>
    public static class Avatars
    {
        public static string Location(Guid userId) =>
            $"{Prefix}avatar:location:{userId}";

        public static string RegionMembers(string regionId) =>
            $"{Prefix}region:members:{regionId}";

        public static string ActiveInRegion(string regionId) =>
            $"{Prefix}region:active:{regionId}";
    }

    /// <summary>
    /// Business recommendations per user per mode.
    /// TTL: 10 minutes per optimization docs.
    /// </summary>
    public static class Recommendations
    {
        public static string ForUser(Guid userId, string mode) =>
            $"{Prefix}recommendations:{userId}:{mode}";

        public static string ByRegion(string regionId, string mode) =>
            $"{Prefix}businesses:region:{regionId}:{mode}";
    }

    /// <summary>
    /// Business detail cache.
    /// TTL: 15 minutes — refreshed on business data update.
    /// </summary>
    public static class Businesses
    {
        public static string Detail(Guid businessId) =>
            $"{Prefix}business:{businessId}";
    }

    /// <summary>
    /// Compatibility scores between user pairs.
    /// TTL: 5 minutes — recalculated if mode changes.
    /// </summary>
    public static class Compatibility
    {
        public static string Score(Guid userAId, Guid userBId)
        {
            // Consistent ordering — A+B and B+A produce the same key
            var (first, second) = userAId.CompareTo(userBId) < 0
                ? (userAId, userBId)
                : (userBId, userAId);
            return $"{Prefix}compatibility:{first}:{second}";
        }
    }

    /// <summary>
    /// Rate limiting counters.
    /// TTL: Matches rate limit window from config.
    /// </summary>
    public static class RateLimiting
    {
        public static string Counter(string clientId) =>
            $"{Prefix}ratelimit:{clientId}";
    }
}