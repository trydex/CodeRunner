using System.Text;
using CodeRunner.Common.Kafka.Messages;

namespace CodeRunner.Worker.CodeRunners.Golang;

public class GolangScriptRunner : BaseRunner, IScriptRunner
{
    private const string ProcessName = "go";
    private readonly string _entryPointFileName;
    private readonly string _entryPointFileNameWithExtension;

    public GolangScriptRunner(IProcessRunner processRunner) : base(processRunner)
    {
        _entryPointFileName = Path.GetRandomFileName();
        _entryPointFileNameWithExtension = _entryPointFileName + ".go";
    }

    public Task<IReadOnlyList<ProcessOutput>> Run(string code, int processCount)
    {
        var files = GetWorkDirFiles(code);
        return base.Run(files, ProcessName, new []{"run", _entryPointFileNameWithExtension}, processCount);
    }
    private IEnumerable<WorkDirFileDescription> GetWorkDirFiles(string code)
    {
        return new List<WorkDirFileDescription>
        {
            new()
            {
                FileName = _entryPointFileNameWithExtension,
                Content = Encoding.UTF8.GetBytes(code)
            }
        };
    }
}