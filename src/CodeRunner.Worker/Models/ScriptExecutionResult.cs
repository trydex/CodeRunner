namespace CodeRunner.Worker.Models;

public class ScriptExecutionResult
{
    public ExecutionStatus ExecutionStatus { get; set; }
    public string Output { get; set; }
    public string Error { get; set; }
}