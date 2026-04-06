namespace AbujaSocialMetaverse.Shared.Configuration;

/// <summary>
/// Root base for all option classes in the system.
/// Every option class inherits from this directly or via a mid-level base.
/// </summary>
public abstract class BaseOptions
{
    /// <summary>
    /// The configuration section name this options class binds to.
    /// Must match the key in appsettings.json and the IConfiguration override in Program.cs.
    /// </summary>
    public abstract string SectionName { get; }

    /// <summary>
    /// Validates the options values at startup.
    /// Override in each subclass to enforce required fields and constraints.
    /// Throw InvalidOperationException with a clear message on failure.
    /// </summary>
    public virtual void Validate() { }
}