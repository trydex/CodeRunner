using CodeRunner.Common.Kafka.Consumer;
using CodeRunner.Common.Kafka.Messages;
using CodeRunner.Common.Kafka.Producer;
using CodeRunner.Worker.Extensions;
using CodeRunner.Worker.Repositories;
using CodeRunner.Worker.Services;
using Microsoft.Extensions.Logging;
using Quartz;

namespace CodeRunner.Worker.Jobs;

public class CodeExecutionJob : IJob
{
    private readonly IMessageConsumer _scriptConsumer;
    private readonly IMessageProducer _resultProducer;
    private readonly IScriptResultsRepository _scriptResultsRepository;
    private readonly IScriptRunnerService _scriptRunnerService;
    private readonly ILogger<CodeExecutionJob> _logger;

    public CodeExecutionJob(IMessageConsumer scriptConsumer,
        IMessageProducer resultProducer,
        IScriptRunnerService scriptRunnerService,
        IScriptResultsRepository scriptResultsRepository,
        ILogger<CodeExecutionJob> logger)
    {
        _scriptConsumer = scriptConsumer;
        _scriptRunnerService = scriptRunnerService;
        _scriptResultsRepository = scriptResultsRepository;
        _logger = logger;
        _resultProducer = resultProducer;
    }
    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation($"Job with key {0} started", context.JobDetail.Key.Name);

        try
        {
            var scriptMessage = _scriptConsumer.Consume<ScriptMessage>();
            var executionResult = await _scriptRunnerService.Run(scriptMessage.ToModel());

            await _scriptResultsRepository.CreateAsync(executionResult);
            await _resultProducer.Write(executionResult.ToMessage());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Job with key {0} failed", context.JobDetail.Key.Name);
            throw;
        }

        _logger.LogInformation($"Job with key {0} finished", context.JobDetail.Key.Name);
    }
}