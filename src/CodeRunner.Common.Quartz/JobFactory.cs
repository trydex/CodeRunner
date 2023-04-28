using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;

namespace CodeRunner.Common.Quartz;

public class JobFactory : IJobFactory
{
    private readonly IServiceScopeFactory _scopeFactory;

    public JobFactory(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
    }

    public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
    {
        var jobDetail = bundle.JobDetail;
        var jobType = jobDetail.JobType;
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var job = scope.ServiceProvider.GetRequiredService(jobType) as IJob;

            return job;
        }
        catch (Exception ex)
        {
            throw new SchedulerException($"Failed to instantiate job {jobDetail.Key}", ex);
        }
    }

    public void ReturnJob(IJob job)
    {
        var disposable = job as IDisposable;
        disposable?.Dispose();
    }
}