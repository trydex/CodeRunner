using CodeRunner.Common;
using CodeRunner.Worker.Models;
using CodeRunner.Worker.Services;
using Quartz;

namespace CodeRunner.Worker.Jobs;

public class CodeExecutionJob : IJob
{
    private readonly IMessageConsumer _scriptConsumer;
    private readonly ScriptRunnerService _scriptRunnerService;

    public CodeExecutionJob(IMessageConsumer scriptConsumer,
        ScriptRunnerService scriptRunnerService)
    {
        _scriptConsumer = scriptConsumer;
        _scriptRunnerService = scriptRunnerService;
    }
    public Task Execute(IJobExecutionContext context)
    {
        Console.WriteLine("Do some job at " + DateTime.Now.ToString("T"));

        var script =  _scriptConsumer.Consume<Script>();
        _scriptRunnerService.Run(script);

        return Task.FromResult(true);
    }
}