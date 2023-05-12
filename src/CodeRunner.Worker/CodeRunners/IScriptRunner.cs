using CodeRunner.Common.Kafka.Messages;

namespace CodeRunner.Worker.CodeRunners;

public interface IScriptRunner
{
    Task<IReadOnlyList<ProcessOutput>> Run(string code, int processCount);
}