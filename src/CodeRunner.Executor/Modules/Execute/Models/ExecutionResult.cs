namespace CodeRunner.Executor.Modules.Execute.Models;

public class ExecutionResult
{
    public Guid Id { get; set; }
    public ExecutionState Status { get; set; }
    public IReadOnlyList<ProcessResult> ProcessResults { get; set; }
    public IReadOnlyList<string> CompilationErrors { get; set; }
}