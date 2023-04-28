namespace CodeRunner.Worker.Settings;

public class ScriptsDatabaseSettings
{
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
    public string ScriptResultsCollectionName { get; set; }
}