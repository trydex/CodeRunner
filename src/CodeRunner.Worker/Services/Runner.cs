using System.Diagnostics;
using CodeRunner.Worker.Models;

namespace CodeRunner.Worker.Services;

public interface IRunner
{
    Task<IReadOnlyList<ProcessResult>>  ExecuteInProcess(byte[] compiledAssembly, string[] args, int workerCount);
}

public class Runner : IRunner
{
    private readonly string _runtimeConfigContent;

    public Runner(IRuntimeConfigProvider runtimeConfigProvider)
    {
        _runtimeConfigContent = runtimeConfigProvider.GetRuntimeConfig();
    }

    public async Task<IReadOnlyList<ProcessResult>> ExecuteInProcess(byte[] compiledAssembly, string[] args, int workerCount)
    {
        var tempFileName = Path.GetTempFileName();
        var dllPath = tempFileName + ".dll";
        var runtimeConfigPath = tempFileName + ".runtimeconfig.json";

        await File.WriteAllBytesAsync(dllPath, compiledAssembly);
        await File.WriteAllTextAsync(runtimeConfigPath, _runtimeConfigContent);

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

        var workerTasks = new List<Task<ProcessResult>>();

        for (int i = 1; i <= workerCount; i++)
        {
            var workerId = i;

            var task = Task.Run(() =>
            {
                var process = new Process { StartInfo = processStartInfo };
                process.Start();
                if (!process.WaitForExit(TimeSpan.FromSeconds(30)))
                {
                    process.Kill();

                    return new ProcessResult
                    {
                        Id = workerId,
                        Error = "Timeout limit exceeded"
                    };
                }

                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();

                return new ProcessResult
                {
                    Id = workerId,
                    Output = output,
                    Error = error
                };
            });

            workerTasks.Add(task);
        }

        await Task.WhenAll(workerTasks);

        var workerResults = workerTasks
            .Select(x => x.Result)
            .ToList();

        TryDeleteFile(dllPath);
        TryDeleteFile(runtimeConfigPath);

        return workerResults;
    }

    private static void TryDeleteFile(string path, int attemptCount = 5)
    {
        while (attemptCount > 0)
        {
            try
            {
                File.Delete(path);
                break;
            }
            catch(Exception)
            {
                attemptCount--;

                if (attemptCount == 0)
                {
                    throw;
                }

                Thread.Sleep(300);
            }
        }
    }
}