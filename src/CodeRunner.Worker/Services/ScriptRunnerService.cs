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
                result.CompilationErrors = compileResult.Errors.ToList();

                _logger.LogInformation($"Execute script with Id = {script?.Id} aborted by compile errors");

                return result;
            }

            result.ProcessResults = await _runner.ExecuteInProcess(
                compiledAssembly: compileResult.Assembly,
                args: Array.Empty<string>(),
                workerCount: script.ProcessCount
                );

            if (result.ProcessResults.All(x => string.IsNullOrEmpty(x.Error)))
            {
                result.Status = ExecutionStatus.Success;
            }
            else
            {
                result.Status = ExecutionStatus.Failed;
            }
        }
        catch(Exception ex)
        {
            result.Status = ExecutionStatus.Failed;
            _logger.LogError(ex, $"Execute script with Id = {script?.Id} failed");
        }

        _logger.LogInformation($"Execute script with Id = {script?.Id} finished");

        return result;
    }
}