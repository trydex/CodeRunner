using System.Diagnostics;
using System.Text;
using CodeRunner.Common.Kafka.Messages;

namespace CodeRunner.Worker.CodeRunners;

public class WorkDirFileDescription
{
    public string FileName { get; set; }
    public byte[] Content { get; set; }
}

public abstract class BaseRunner
{
    private readonly IProcessRunner _processRunner;
    public BaseRunner(IProcessRunner processRunner)
    {
        _processRunner = processRunner;
    }

    public async Task<IReadOnlyList<ProcessOutput>> Run(IEnumerable<WorkDirFileDescription> workDirFiles,
        string processName,
        string[] args,
        int processCount)
    {
        var workDir = "";

        try
        {
            workDir = GetWorkDir();
            await DumpFilesToWorkDir(workDir, workDirFiles);

            var processStartInfo = CreateProcessStartInfo(processName, args, workDir);

            var processOutputs = await _processRunner.Run(processStartInfo, processCount);
            return processOutputs;
        }
        finally
        {
            TryDeleteDirectory(workDir);
        }
    }

    private string GetWorkDir()
    {
        var workDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(workDir);

        return workDir;
    }

    private ProcessStartInfo CreateProcessStartInfo(string processName, string[] args, string workDir)
    {
        return new ProcessStartInfo
        {
            FileName = processName,
            Arguments = string.Join(" ", args),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            WorkingDirectory = workDir,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };
    }

    private async Task DumpFilesToWorkDir(string workDir, IEnumerable<WorkDirFileDescription> workDirFilesDescriptions)
    {
        foreach (var workDirFile in workDirFilesDescriptions)
        {
            var filePath = Path.Combine(workDir, workDirFile.FileName);
            await File.WriteAllBytesAsync(filePath, workDirFile.Content);
        }
    }

    private static void TryDeleteDirectory(string directoryPath, int attemptCount = 5)
    {
        while (attemptCount > 0)
        {
            try
            {
                if (!string.IsNullOrEmpty(directoryPath) && Directory.Exists(directoryPath))
                {
                    Directory.Delete(directoryPath, true);
                }

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