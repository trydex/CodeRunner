using MongoDB.Bson.Serialization.Attributes;

namespace CodeRunner.Worker.Models;

public class ScriptExecutionResult
{
    [BsonId]
    public Guid Id { get; set; } = Guid.NewGuid();
    public ExecutionStatus Status { get; set; }
    public string Output { get; set; }
    public string Error { get; set; }
}