using Hangfire;
using System.Linq.Expressions;

namespace AbujaSocialMetaverse.Infrastructure.BackgroundJobs;

public class HangfireBackgroundJobService : IBackgroundJobService
{
    private readonly IBackgroundJobClient _jobClient;
    private readonly IRecurringJobManager _recurringJobManager;

    public HangfireBackgroundJobService(
        IBackgroundJobClient jobClient,
        IRecurringJobManager recurringJobManager)
    {
        _jobClient = jobClient;
        _recurringJobManager = recurringJobManager;
    }

    public string Enqueue(Expression<Action> job)
        => _jobClient.Enqueue(job);

    public string Enqueue(Expression<Func<Task>> job)
        => _jobClient.Enqueue(job);

    public string Schedule(Expression<Action> job, TimeSpan delay)
        => _jobClient.Schedule(job, delay);

    public string Schedule(Expression<Func<Task>> job, TimeSpan delay)
        => _jobClient.Schedule(job, delay);

    public void AddOrUpdateRecurring(
        string jobId,
        Expression<Action> job,
        string cronExpression)
        => _recurringJobManager.AddOrUpdate(jobId, job, cronExpression);

    public void AddOrUpdateRecurring(
        string jobId,
        Expression<Func<Task>> job,
        string cronExpression)
        => _recurringJobManager.AddOrUpdate(jobId, job, cronExpression);

    public void RemoveRecurring(string jobId)
        => _recurringJobManager.RemoveIfExists(jobId);
}