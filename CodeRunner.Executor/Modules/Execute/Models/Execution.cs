namespace CodeRunner.Executor.Modules.Execute.Models;

public record Execution(int Id, ExecutionState State, string[] Output);