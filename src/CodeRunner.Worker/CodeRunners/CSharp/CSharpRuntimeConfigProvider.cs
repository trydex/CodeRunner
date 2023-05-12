namespace CodeRunner.Worker.CodeRunners.CSharp;

public interface ICSharpRuntimeConfigProvider
{
    byte[] GetRuntimeConfig();
}

public class CSharpRuntimeConfigProvider : ICSharpRuntimeConfigProvider
{
    private const string Filename = "assembly.runtimeconfig.json";
    public byte[] GetRuntimeConfig()
    {
        var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Filename);
        return File.ReadAllBytes(configPath);
    }
}