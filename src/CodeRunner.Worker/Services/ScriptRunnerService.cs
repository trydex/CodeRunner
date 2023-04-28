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
        //all the logic for dynamic compilation and code execution is taken from this article
        //https://laurentkempe.com/2019/02/18/dynamically-compile-and-run-code-using-dotNET-Core-3.0/

        var assembly = _compiler.Compile(script.Code);
        _runner.Execute(assembly, args: Array.Empty<string>());

        return default;
    }
}