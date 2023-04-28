using CodeRunner.Common;
using CodeRunner.Worker.Models;
using CodeRunner.Worker.Repositories;
using CodeRunner.Worker.Services;
using Quartz;

namespace CodeRunner.Worker.Jobs;

public class CodeExecutionJob : IJob
{
    private readonly IMessageConsumer _scriptConsumer;
    private readonly IMessageWriter _resultProducer;
    private readonly IScriptResultsRepository _scriptResultsRepository;

    private readonly IScriptRunnerService _scriptRunnerService;

    public CodeExecutionJob(IMessageConsumer scriptConsumer,
        IMessageWriter resultProducer,
        IScriptRunnerService scriptRunnerService,
        IScriptResultsRepository scriptResultsRepository)
    {
        _scriptConsumer = scriptConsumer;
        _scriptRunnerService = scriptRunnerService;
        _scriptResultsRepository = scriptResultsRepository;
        _resultProducer = resultProducer;
    }
    public async Task Execute(IJobExecutionContext context)
    {
        var script = _scriptConsumer.Consume<Script>();
        var result = await _scriptRunnerService.Run(script);

        await _scriptResultsRepository.CreateAsync(result);
        await _resultProducer.Write(result);
    }
}