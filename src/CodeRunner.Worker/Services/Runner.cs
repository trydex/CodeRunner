using System.Diagnostics;

namespace CodeRunner.Worker.Services;

public interface IRunner
{
    (string ouput, string error) ExecuteInProcess(byte[] compiledAssembly, string[] args);
}

public class Runner : IRunner
{
    private readonly string _runtimeConfigContent;

    public Runner(IRuntimeConfigProvider runtimeConfigProvider)
    {
        _runtimeConfigContent = runtimeConfigProvider.GetRuntimeConfig();
    }

    public (string ouput, string error) ExecuteInProcess(byte[] compiledAssembly, string[] args)
    {
        var tempFileName = Path.GetTempFileName();
        var dllPath = tempFileName + ".dll";
        var runtimeConfigPath = tempFileName + ".runtimeconfig.json";

        File.WriteAllBytes(dllPath, compiledAssembly);
        File.WriteAllText(runtimeConfigPath, _runtimeConfigContent);

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"{dllPath} {string.Join(" ", args)}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        var curDir = Path.GetDirectoryName(typeof(object).Assembly.Location);
        processStartInfo.EnvironmentVariables["PATH"] += ";" + curDir;

        var process = new Process { StartInfo = processStartInfo };
        process.Start();
        process.WaitForExit();

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();

        File.Delete(dllPath);
        File.Delete(runtimeConfigPath);

        return (output, error);
    }
}