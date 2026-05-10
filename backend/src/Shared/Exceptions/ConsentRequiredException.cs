using AbujaSocialMetaverse.Shared.Constants;
using AbujaSocialMetaverse.Shared.Models;

namespace AbujaSocialMetaverse.Shared.Exceptions;

/// <summary>
/// Thrown when a module attempts to process personal data
/// without the user's explicit consent.
/// Mapped to 403 Forbidden by GlobalExceptionMiddleware.
/// </summary>
public class ConsentRequiredException : DomainException
{
    public Guid UserId { get; }
    public DataCategory Category { get; }

    public ConsentRequiredException(Guid userId, DataCategory category)
        : base(
            ErrorCodes.Privacy.ConsentRequired,
            $"User {userId} has not granted consent for data category '{category}'. " +
            $"Consent must be obtained before processing this data.",
            ErrorType.Forbidden)
    {
        UserId = userId;
        Category = category;
    }
}