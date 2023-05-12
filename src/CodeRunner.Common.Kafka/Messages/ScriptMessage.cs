namespace CodeRunner.Common.Kafka.Messages;

public record ScriptMessage
{
    public Guid Id { get; set; }
    public int ProcessCount { get; set; }
    public string Code { get; set; }
}