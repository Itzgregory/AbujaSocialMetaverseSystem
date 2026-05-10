using AbujaSocialMetaverse.Shared.Models;

namespace AbujaSocialMetaverse.Modules.Core.Public.Interfaces;

public interface IPasswordService
{
    Result<string> HashPassword(string password);
    Result<bool> VerifyPassword(string password, string hash);
    Result ValidateStrength(string password);
    Result<bool> NeedsRehash(string hash, int currentWorkFactor);
}