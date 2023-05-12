using CodeRunner.Common.Kafka.Messages;
using CodeRunner.Worker.Models;

namespace CodeRunner.Worker.Extensions;

public static class MapExtensions
{
    public static Script ToModel(this ScriptMessage sm)
    {
        return new Script
        {
            Id = sm.Id,
            Code = sm.Code,
            ProcessCount = sm.ProcessCount,
            Language = sm.Language
        };
    }

    public static ScriptExecutionResultMessage ToMessage(this ScriptExecutionResult scriptExecutionResult)
    {
        return new ScriptExecutionResultMessage
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