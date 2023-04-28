namespace CodeRunner.Executor.Settings;

public class ScriptsDatabaseSettings
{
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
    public string ScriptsCollectionName { get; set; }
    public string ExecutionResultsCollectionName { get; set; }
}