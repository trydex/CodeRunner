using CodeRunner.Worker.Models;
using CodeRunner.Worker.Repositories;

namespace CodeRunner.Worker.Services;

public class ScriptRunnerService
{
    private readonly IRunner _runner;
    private readonly ICompiler _compiler;
    private readonly IScriptResultsRepository _scriptResultsRepository;

    public ScriptRunnerService(
        IRunner runner,
        ICompiler compiler,
        IScriptResultsRepository scriptResultsRepository)
    {
        _runner = runner;
        _compiler = compiler;
        _scriptResultsRepository = scriptResultsRepository;
    }

    public ScriptExecutionResult Run(Script script)
    {
        var result = new ScriptExecutionResult();

        try
        {
            //part of the logic for dynamic compilation and code execution is taken from this article
            //https://laurentkempe.com/2019/02/18/dynamically-compile-and-run-code-using-dotNET-Core-3.0/

            var assembly = _compiler.Compile(script.Code);
            var (output, error) = _runner.ExecuteInProcess(assembly, args: Array.Empty<string>());

            result.Output = output;
            result.Error = error;

            if (string.IsNullOrEmpty(error))
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