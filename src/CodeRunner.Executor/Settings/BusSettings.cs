namespace CodeRunner.Executor.Settings;

public class BusSettings
{
    public string Server { get; set; }
    public string ScriptsTopicName { get; set; }
    public string ResultsTopicName { get; set; }
    public string ResultsConsumerGroup { get; set; }
}
