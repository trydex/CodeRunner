namespace CodeRunner.Common.Kafka.Messages;

public record ScriptMessage
{
    public Guid Id { get; set; }
    public int ProcessCount { get; set; }
    public Language Language { get; set; }
    public string Code { get; set; }
}

public enum Language
{
    Unknown = 0,
    CSharp = 1,
    Golang= 2
}