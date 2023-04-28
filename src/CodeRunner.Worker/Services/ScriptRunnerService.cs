using CodeRunner.Worker.Models;
using CodeRunner.Worker.Repositories;
using Microsoft.Extensions.Logging;

namespace CodeRunner.Worker.Services;

public interface IScriptRunnerService
{
    Task<ScriptExecutionResult>  Run(Script script);
}

public class ScriptRunnerService : IScriptRunnerService
{
    private readonly IRunner _runner;
    private readonly ICompiler _compiler;
    private readonly ILogger<ScriptRunnerService> _logger;

    public ScriptRunnerService(
        IRunner runner,
        ICompiler compiler,
        ILogger<ScriptRunnerService> logger)
    {
        _runner = runner;
        _compiler = compiler;
        _logger = logger;
    }

    public async Task<ScriptExecutionResult> Run(Script script)
    {
        _logger.LogInformation($"Execute script with Id = {script?.Id}");

        var result = new ScriptExecutionResult
        {
            Id = script.Id,
            ScriptMetadata = script
        };

        try
        {
            //part of the logic for dynamic compilation and code execution is taken from this article
            //https://laurentkempe.com/2019/02/18/dynamically-compile-and-run-code-using-dotNET-Core-3.0/

            var compileResult = _compiler.Compile(script.Code);
            if (!compileResult.Success)
            {
                result.Results = new List<WorkerResult>
                {
                    new ()
                    {
                        Id = -1,
                        Error = string.Join(Environment.NewLine, compileResult.Errors)
                    }
                };

                _logger.LogInformation($"Execute script with Id = {script?.Id} aborted by compile errors");

                return result;
            }

            result.Results = await _runner.ExecuteInProcess(
                compiledAssembly: compileResult.Assembly,
                args: Array.Empty<string>(),
                workerCount: script.WorkerCount
                );

            if (result.Results.All(x => string.IsNullOrEmpty(x.Error)))
            {
                result.Status = ExecutionStatus.Success;
            }
        }
        catch(Exception ex)
        {
            result.Status = ExecutionStatus.Failed;
            _logger.LogError($"Execute script with Id = {script?.Id} failed", ex);
        }

        _logger.LogInformation($"Execute script with Id = {script?.Id} finished");

        return result;
    }
}