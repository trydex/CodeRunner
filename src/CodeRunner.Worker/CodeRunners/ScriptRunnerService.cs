using CodeRunner.Common.Kafka.Messages;
using CodeRunner.Worker.Models;
using Microsoft.Extensions.Logging;

namespace CodeRunner.Worker.CodeRunners;

public interface IScriptRunnerService
{
    Task<ScriptExecutionResult>  Run(Script script);
}

public class ScriptRunnerService : IScriptRunnerService
{
    private readonly IScriptRunnerFactory _scriptRunnerFactory;
    private readonly ILogger<ScriptRunnerService> _logger;

    public ScriptRunnerService(
        IScriptRunnerFactory scriptRunnerFactory,
        ILogger<ScriptRunnerService> logger)
    {
        _scriptRunnerFactory = scriptRunnerFactory;
        _logger = logger;
    }

    public async Task<ScriptExecutionResult> Run(Script script)
    {
        _logger.LogInformation($"Execute script with Id = {0}", script.Id);

        var result = new ScriptExecutionResult
        {
            Id = script.Id,
            ScriptMetadata = script
        };

        try
        {
            var runner = _scriptRunnerFactory.Create(script.Language);
            result.ProcessResults = await runner.Run(script.Code, script.ProcessCount);

            result.Status = result.ProcessResults.All(x => string.IsNullOrEmpty(x.Error))
                ? ExecutionStatus.Success
                : ExecutionStatus.Failed;
        }
        catch(Exception ex)
        {
            result.Status = ExecutionStatus.Failed;
            _logger.LogError(ex, $"Execute script with Id = {0} failed", script?.Id);
        }

        _logger.LogInformation($"Execute script with Id = {0} finished", script?.Id);

        return result;
    }
}