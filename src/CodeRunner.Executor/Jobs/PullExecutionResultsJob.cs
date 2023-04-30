using CodeRunner.Common.Kafka.Consumer;
using CodeRunner.Executor.Modules.Execute.Models;
using CodeRunner.Executor.Repositories;
using CodeRunner.Executor.Services;
using Quartz;

namespace CodeRunner.Executor.Jobs;

public class SendEmailJob : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        // Code that sends a periodic email to the user (for example)
        // Note: This method must always return a value
        // This is especially important for trigger listers watching job execution
        return Task.FromResult(true);
    }
}

public class PullExecutionResultsJob : IJob
{
    private readonly ILogger<PullExecutionResultsJob> _logger;
    private readonly IExecutionResultsRepository _executionResultsRepository;
    private readonly IMessageConsumer _executeResultsConsumer;
    private readonly ICacheService _cacheService;

    public PullExecutionResultsJob(ILogger<PullExecutionResultsJob> logger,
        IExecutionResultsRepository executionResultsRepository,
        IMessageConsumer executeResultsConsumer,
        ICacheService cacheService)
    {
        _logger = logger;
        _executionResultsRepository = executionResultsRepository;
        _executeResultsConsumer = executeResultsConsumer;
        _cacheService = cacheService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation($"Job with key {0} started", context.JobDetail.Key.Name);

        try
        {
            var executionResult = _executeResultsConsumer.Consume<ExecutionResult>();
            executionResult.Status = ExecutionState.Done;

            var (hashCodeExistsInCache, hashCode) = await _cacheService.TryGetValue<Guid, int>(executionResult.Id);
            if (hashCodeExistsInCache)
            {
                await _cacheService.SetValue(hashCode, executionResult);
            }

            await _executionResultsRepository.UpdateAsync(executionResult.Id, executionResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Job with key {0} failed", context.JobDetail.Key.Name);
            throw;
        }

        _logger.LogInformation($"Job with key {0} finished", context.JobDetail.Key.Name);
    }
}