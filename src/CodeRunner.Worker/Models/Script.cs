using CodeRunner.Common.Kafka.Messages;

namespace CodeRunner.Worker.Models;

public record Script
{
    public Guid Id { get; set; }
    public int ProcessCount { get; set; }
    public string Code { get; set; }
    public Language Language { get; set; }
}