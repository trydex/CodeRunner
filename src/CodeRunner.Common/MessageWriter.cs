using System.Net;
using Confluent.Kafka;
using Newtonsoft.Json;

namespace CodeRunner.Common;

public interface IMessageWriter
{
    Task Write<TMessage>(TMessage message);
}

public class WriterOptions
{
    public string Server { get; set; }
    public string Topic { get; set; }
}

public class MessageWriter : IMessageWriter, IDisposable
{
    private readonly string _topic;
    private readonly IProducer<Null,string> _producer;

    public MessageWriter(WriterOptions options)
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