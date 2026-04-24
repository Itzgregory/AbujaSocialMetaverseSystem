using AbujaSocialMetaverse.Modules.Core.Public.Models;
using AbujaSocialMetaverse.Shared.Models;

namespace AbujaSocialMetaverse.Modules.Core.Public.Interfaces;

public interface IUserCreationService
{
    Task<Result<UserDto>> CreateUserAsync(
        string email,
        string passwordHash,
        string displayName,
        CancellationToken cancellationToken = default);
}