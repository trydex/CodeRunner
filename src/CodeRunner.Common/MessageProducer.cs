using System.Net;
using Confluent.Kafka;
using Newtonsoft.Json;

namespace CodeRunner.Common;

public interface IMessageProducer
{
    Task Write<TMessage>(TMessage message);
}

public class MessageProducerOptions
{
    public string Server { get; set; }
    public string Topic { get; set; }
}

public class MessageProducer : IMessageProducer, IDisposable
{
    private readonly string _topic;
    private readonly IProducer<Null,string> _producer;

    public MessageProducer(MessageProducerOptions options)
    {
        _topic = options.Topic;

        var config = new ProducerConfig
        {
            BootstrapServers = options.Server,
            ClientId = Dns.GetHostName(),
        };

        _producer = new ProducerBuilder<Null, string>(config).Build();
    }
    public async Task Write<TMessage>(TMessage message)
    {
        var kafkaMessage = new Message<Null, string> { Value = JsonConvert.SerializeObject(message)};
        await _producer.ProduceAsync(_topic, kafkaMessage);
    }

    public void Dispose()
    {
        _producer.Dispose();
    }
}