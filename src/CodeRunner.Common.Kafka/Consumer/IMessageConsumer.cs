namespace CodeRunner.Common.Kafka.Consumer;

public interface IMessageConsumer
{
    TMessage Consume<TMessage>();
}