using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CodeRunner.Worker.Models;

public class ScriptExecutionResult
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }
    public Script ScriptMetadata { get; set; }
    public ExecutionStatus Status { get; set; }
    public IReadOnlyList<ProcessResult> ProcessResults { get; set; }
    public IReadOnlyList<string> CompilationErrors { get; set; }
}