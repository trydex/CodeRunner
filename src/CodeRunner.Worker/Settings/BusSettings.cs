namespace CodeRunner.Worker.Settings;

public class BusSettings
{
    public string Server { get; set; }
    public string ScriptsTopicName { get; set; }
    public string ExecutionResultsTopicName { get; set; }
    public string ScriptsConsumerGroup { get; set; }
}
