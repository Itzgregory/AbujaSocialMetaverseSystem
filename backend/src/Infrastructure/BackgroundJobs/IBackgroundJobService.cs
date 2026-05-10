using System.Linq.Expressions;
namespace AbujaSocialMetaverse.Infrastructure.BackgroundJobs;

/// <summary>
/// Abstraction over the background job scheduler.
/// Current implementation: Hangfire.
/// Swappable without touching any module.
/// </summary>
public interface IBackgroundJobService
{
    /// <summary>Fire and forget — runs once immediately in the background.</summary>
    string Enqueue(Expression<Action> job);
    string Enqueue(Expression<Func<Task>> job);

    /// <summary>Delayed — runs once after a specified delay.</summary>
    string Schedule(Expression<Action> job, TimeSpan delay);
    string Schedule(Expression<Func<Task>> job, TimeSpan delay);

    /// <summary>Recurring — runs on a cron schedule.</summary>
    void AddOrUpdateRecurring(string jobId, Expression<Action> job, string cronExpression);
    void AddOrUpdateRecurring(string jobId, Expression<Func<Task>> job, string cronExpression);

    /// <summary>Remove a recurring job by its ID.</summary>
    void RemoveRecurring(string jobId);
}