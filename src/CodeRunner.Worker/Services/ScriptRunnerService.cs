using CodeRunner.Worker.Models;
using DynamicRun.Builder;

namespace CodeRunner.Worker.Services;

public class ScriptRunnerService
{
    private readonly Runner _runner;
    private readonly Compiler _compiler;

    public ScriptRunnerService()
    {
        _compiler = new Compiler();
        _runner = new Runner();
    }
    public ScriptExecutionResult Run(Script script)
    {
        var result = new ScriptExecutionResult();

        try
        {
            //part of the logic for dynamic compilation and code execution is taken from this article
            //https://laurentkempe.com/2019/02/18/dynamically-compile-and-run-code-using-dotNET-Core-3.0/

            var assembly = _compiler.Compile(script.Code);
            var (output, error) = _runner.ExecuteInSeparateProcess(assembly, args: Array.Empty<string>());

            result.Output = output;
            result.Error = error;

            if (string.IsNullOrEmpty(error))
            {
                result.ExecutionStatus = ExecutionStatus.Success;
            }
        }
        catch(Exception e)
        {
            result.ExecutionStatus = ExecutionStatus.Failed;
        }

        return result;
    }
}