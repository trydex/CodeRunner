using System.Net;
using Confluent.Kafka;
using Newtonsoft.Json;

namespace CodeRunner.Common;

public interface IMessageWriter
{
    Task Write<TMessage>(string server, string topic, TMessage message);
}

public class MessageWriter : IMessageWriter
{
    public async Task Write<TMessage>(string server, string topic, TMessage message)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = server,
            ClientId = Dns.GetHostName(),
        };

        using var producer = new ProducerBuilder<Null, string>(config).Build();

        var kafkaMessage = new Message<Null, string> { Value = JsonConvert.SerializeObject(message)};
        await producer.ProduceAsync(topic, kafkaMessage);
    }
}