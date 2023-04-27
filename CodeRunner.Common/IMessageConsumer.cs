using System.Net;
using Confluent.Kafka;
using Newtonsoft.Json;

namespace CodeRunner.Common;

public interface IMessageConsumer
{
    TMessage Consume<TMessage>(string server, string topic, string consumerGroup);
}

public class MessageConsumer : IMessageConsumer
{
    public TMessage? Consume<TMessage>(string server, string topic, string consumerGroup)
    {
        var config = new ConsumerConfig
        {
            GroupId = consumerGroup,
            BootstrapServers = server,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<Null, string>(config).Build();

        consumer.Subscribe(topic);
        var result = consumer.Consume();

        if(result != null)
            return JsonConvert.DeserializeObject<TMessage>(result.Message.Value);

        return default;
    }
}