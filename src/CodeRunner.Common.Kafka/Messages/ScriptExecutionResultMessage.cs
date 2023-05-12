namespace CodeRunner.Common.Kafka.Messages;

public class ScriptExecutionResultMessage
{
    public Guid Id { get; set; }
    public ExecutionStatus Status { get; set; }
    public IReadOnlyList<ProcessResult> ProcessResults { get; set; }
    public IReadOnlyList<string> CompilationErrors { get; set; }
}

public enum ExecutionStatus
{
    Pending = 0,
    Failed = 1,
    Success = 2
}

public class ProcessResult
{
    public int Id { get; set; }
    public string Output { get; set; }
    public string Error { get; set; }
}