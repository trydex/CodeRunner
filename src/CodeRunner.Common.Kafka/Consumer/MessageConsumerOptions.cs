using Confluent.Kafka;

namespace CodeRunner.Common.Kafka.Consumer;

public class MessageConsumerOptions
{
    public string Server { get; set; }
    public string Topic { get; set; }
    public string Group { get; set; }
    public AutoOffsetReset AutoOffsetReset { get; set; } = AutoOffsetReset.Earliest;
}