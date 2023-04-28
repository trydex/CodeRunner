using CodeRunner.Worker.Models;
using CodeRunner.Worker.Repositories;

namespace CodeRunner.Worker.Services;

public interface IScriptRunnerService
{
    Task<ScriptExecutionResult>  Run(Script script);
}

public class ScriptRunnerService : IScriptRunnerService
{
    private readonly IRunner _runner;
    private readonly ICompiler _compiler;

    public ScriptRunnerService(
        IRunner runner,
        ICompiler compiler)
    {
        _runner = runner;
        _compiler = compiler;
    }

    public async Task<ScriptExecutionResult> Run(Script script)
    {
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

                return result;
            }

            result.Results = await _runner.ExecuteInProcess(
                compiledAssembly: compileResult.Assembly,
                args: Array.Empty<string>(), 
                workerCount: script.WorkerCount
                );

            if (result.Results.All(x => !string.IsNullOrEmpty(x.Error)))
            {
                result.Status = ExecutionStatus.Success;
            }
        }
        catch(Exception e)
        {
            result.Status = ExecutionStatus.Failed;
        }

        return result;
    }
}