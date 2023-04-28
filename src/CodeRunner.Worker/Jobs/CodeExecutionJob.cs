using CodeRunner.Common;
using CodeRunner.Worker.Models;
using CodeRunner.Worker.Repositories;
using CodeRunner.Worker.Services;
using Microsoft.Extensions.Logging;
using Quartz;

namespace CodeRunner.Worker.Jobs;

public class CodeExecutionJob : IJob
{
    private readonly IMessageConsumer _scriptConsumer;
    private readonly IMessageWriter _resultProducer;
    private readonly IScriptResultsRepository _scriptResultsRepository;
    private readonly ILogger<CodeExecutionJob> _logger;

    private readonly IScriptRunnerService _scriptRunnerService;

    public CodeExecutionJob(IMessageConsumer scriptConsumer,
        IMessageWriter resultProducer,
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
        _logger.LogInformation($"Job with key {context.JobDetail.Key.Name} started");

        try
        {
            var script = _scriptConsumer.Consume<Script>();
            var result = await _scriptRunnerService.Run(script);

            await _scriptResultsRepository.CreateAsync(result);
            await _resultProducer.Write(result);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Job with key {context.JobDetail.Key.Name} failed", ex);
            throw;
        }

        _logger.LogInformation($"Job with key {context.JobDetail.Key.Name} finished");
    }
}