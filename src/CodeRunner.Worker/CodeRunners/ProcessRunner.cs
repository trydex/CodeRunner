using System.Diagnostics;
using CodeRunner.Common.Kafka.Messages;

namespace CodeRunner.Worker.CodeRunners;

public interface IProcessRunner
{
    Task<IReadOnlyList<ProcessOutput>> Run(ProcessStartInfo processStartInfo, int processCount);
}

public class ProcessRunner : IProcessRunner
{
    public async Task<IReadOnlyList<ProcessOutput>> Run(ProcessStartInfo processStartInfo, int processCount)
    {
        var workerTasks = new List<Task<ProcessOutput>>();

        for (int i = 1; i <= processCount; i++)
        {
            var processId = i;

            var task = Task.Run(() =>
            {
                var process = new Process { StartInfo = processStartInfo };
                process.Start();
                if (!process.WaitForExit(TimeSpan.FromSeconds(30)))
                {
                    process.Kill();

                    return new ProcessOutput
                    {
                        Id = processId,
                        Error = "Timeout limit exceeded"
                    };
                }

                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();

                return new ProcessOutput
                {
                    Id = processId,
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

        return workerResults;
    }
}