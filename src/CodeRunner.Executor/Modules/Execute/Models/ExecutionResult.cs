using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CodeRunner.Executor.Modules.Execute.Models;

public class ExecutionResult
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }
    public ExecutionState Status { get; set; }
    public IReadOnlyList<ProcessResult> ProcessResults { get; set; }
    public IReadOnlyList<string> CompilationErrors { get; set; }
}