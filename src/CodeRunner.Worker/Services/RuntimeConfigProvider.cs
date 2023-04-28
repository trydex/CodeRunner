namespace CodeRunner.Worker.Services;

public interface IRuntimeConfigProvider
{
    string GetRuntimeConfig();
}

public class RuntimeConfigProvider : IRuntimeConfigProvider
{
    private const string Filename = "assembly.runtimeconfig.json";
    private string _content;
    public string GetRuntimeConfig()
    {
        var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Filename);
        _content ??= File.ReadAllText(configPath);

        return _content;
    }
}