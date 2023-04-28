using CodeRunner.Common.Kafka.Consumer;
using CodeRunner.Executor.Modules.Execute.Models;
using CodeRunner.Executor.Repositories;
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

    public PullExecutionResultsJob(ILogger<PullExecutionResultsJob> logger,
        IExecutionResultsRepository executionResultsRepository,
        IMessageConsumer executeResultsConsumer)
    {
        _logger = logger;
        _executionResultsRepository = executionResultsRepository;
        _executeResultsConsumer = executeResultsConsumer;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation($"Job with key {context.JobDetail.Key.Name} started");

        try
        {
            var executionResult = _executeResultsConsumer.Consume<ExecutionResult>();
            await _executionResultsRepository.UpdateAsync(executionResult.Id, executionResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Job with key {context.JobDetail.Key.Name} failed");
            throw;
        }

        _logger.LogInformation($"Job with key {context.JobDetail.Key.Name} finished");
    }
}