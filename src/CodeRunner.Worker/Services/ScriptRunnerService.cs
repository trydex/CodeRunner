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
        var assembly = _compiler.Compile(script.Code);
        _runner.Execute(assembly, args: Array.Empty<string>());

        return default;
    }
}