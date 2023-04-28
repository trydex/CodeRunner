using System.Text.Json;
using CodeRunner.Common;
using CodeRunner.Worker.Models;
using CodeRunner.Worker.Services;
using CodeRunner.Worker.Settings;
using Microsoft.Extensions.Options;
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
        Console.WriteLine(JsonSerializer.Serialize(script));
        var result = _scriptRunnerService.Run(script);

        return Task.FromResult(true);
    }
}