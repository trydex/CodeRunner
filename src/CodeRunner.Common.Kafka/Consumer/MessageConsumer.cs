using System.Text.Json;
using Confluent.Kafka;

namespace CodeRunner.Common.Kafka.Consumer;

public class MessageConsumer : IMessageConsumer, IDisposable
{
    private readonly IConsumer<Null,string> _consumer;

    public MessageConsumer(MessageConsumerOptions messageConsumerOptions)
    {
        var config = new ConsumerConfig
        {
            GroupId = messageConsumerOptions.Group,
            BootstrapServers = messageConsumerOptions.Server,
            AutoOffsetReset = messageConsumerOptions.AutoOffsetReset
        };

        _consumer = new ConsumerBuilder<Null, string>(config).Build();

        _consumer.Subscribe(messageConsumerOptions.Topic);
    }
    
    public TMessage? Consume<TMessage>()
    {
        var result = _consumer.Consume();

        if(result != null)
            return JsonSerializer.Deserialize<TMessage>(result.Message.Value);

        return default;
    }

    public void Dispose()
    {
        _consumer.Dispose();
    }
}