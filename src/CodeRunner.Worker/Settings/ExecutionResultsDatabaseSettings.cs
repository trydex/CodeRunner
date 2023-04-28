namespace CodeRunner.Worker.Settings;

public class ExecutionResultsDatabaseSettings
{
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
    public string ExecutionResultsCollectionName { get; set; }
}