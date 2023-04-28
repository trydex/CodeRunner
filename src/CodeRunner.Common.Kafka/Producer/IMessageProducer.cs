namespace CodeRunner.Common.Kafka.Producer;

public interface IMessageProducer
{
    Task Write<TMessage>(TMessage message);
}