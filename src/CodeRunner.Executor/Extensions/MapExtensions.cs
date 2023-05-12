using CodeRunner.Common.Kafka.Messages;
using CodeRunner.Executor.Modules.Execute.Models;

namespace CodeRunner.Executor.Extensions;

public static class MapExtensions
{
    public static ScriptMessage ToMessage(this SubmittedScript script)
    {
        return new ScriptMessage
        {
            Id = script.Id,
            Code = script.Code,
            ProcessCount = script.ProcessCount,
            Language = script.Language
        };
    }
    public static ScriptExecutionResult ToModel(this ScriptExecutionResultMessage scriptExecutionResult)
    {
        return new ScriptExecutionResult
        {
            Id = scriptExecutionResult.Id,
            Status = scriptExecutionResult.Status,
            CompilationErrors = scriptExecutionResult.CompilationErrors?.ToList(),
            ProcessResults = scriptExecutionResult.ProcessResults?.Select(x => new ProcessResult
            {
                Id = x.Id,
                Error = x.Error,
                Output = x.Output
            }).ToList()
        };
    }
}