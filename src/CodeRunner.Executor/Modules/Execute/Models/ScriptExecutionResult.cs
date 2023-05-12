using CodeRunner.Common.Kafka.Messages;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CodeRunner.Executor.Modules.Execute.Models;

public class ScriptExecutionResult
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }
    public ExecutionStatus Status { get; set; }
    public IReadOnlyList<ProcessOutput> ProcessResults { get; set; }
    public IReadOnlyList<string> CompilationErrors { get; set; }
}