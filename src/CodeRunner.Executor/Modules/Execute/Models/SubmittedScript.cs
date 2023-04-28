using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CodeRunner.Executor.Modules.Execute.Models;

public record SubmittedScript
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [BsonIgnoreIfDefault] public DateTime SubmitDate { get; set; } = DateTime.UtcNow;

    public int WorkerCount { get; set; }
    public string Code { get; set; }

    public override int GetHashCode()
    {
        return Code.GetHashCode() + WorkerCount.GetHashCode();
    }
}