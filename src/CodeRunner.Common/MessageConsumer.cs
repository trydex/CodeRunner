using Confluent.Kafka;
using Newtonsoft.Json;

namespace CodeRunner.Common;

public interface IMessageConsumer
{
    TMessage Consume<TMessage>();
}

public class ConsumerOptions
{
    public string Server { get; set; }
    public string Topic { get; set; }
    public string Group { get; set; }
    public AutoOffsetReset AutoOffsetReset { get; set; } = AutoOffsetReset.Earliest;
}

public class MessageConsumer : IMessageConsumer, IDisposable
{
    private readonly IConsumer<Null,string> _consumer;

    public MessageConsumer(ConsumerOptions consumerOptions)
    {
        var config = new ConsumerConfig
        {
            GroupId = consumerOptions.Group,
            BootstrapServers = consumerOptions.Server,
            AutoOffsetReset = consumerOptions.AutoOffsetReset
        };

        _consumer = new ConsumerBuilder<Null, string>(config).Build();

        _consumer.Subscribe(consumerOptions.Topic);
    }
    
    public TMessage? Consume<TMessage>()
    {
        var result = _consumer.Consume();

        if(result != null)
            return JsonConvert.DeserializeObject<TMessage>(result.Message.Value);

        return default;
    }

    public TMessage Consume<TMessage>(string server, string topic, string consumerGroup)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        _consumer.Dispose();
    }
}