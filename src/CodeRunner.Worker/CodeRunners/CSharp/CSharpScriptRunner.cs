using CodeRunner.Common.Kafka.Messages;

namespace CodeRunner.Worker.CodeRunners.CSharp;

public class CSharpScriptRunner : BaseRunner, IScriptRunner
{
    private const string ProcessName = "dotnet";
    private readonly string _assemblyName;
    private readonly string _assemblyNameWithExtension;
    private readonly ICSharpCompiler _csharpCompiler;
    private readonly ICSharpRuntimeConfigProvider _csharpRuntimeConfigProvider;

    public CSharpScriptRunner(ICSharpCompiler csharpCompiler,
        IProcessRunner processRunner,
        ICSharpRuntimeConfigProvider csharpRuntimeConfigProvider) : base(processRunner)
    {
        _csharpCompiler = csharpCompiler;
        _csharpRuntimeConfigProvider = csharpRuntimeConfigProvider;

        _assemblyName = Path.GetRandomFileName();
        _assemblyNameWithExtension = $"{_assemblyName}.dll";
    }

    public async Task<IReadOnlyList<ProcessOutput>> Run(string code, int processCount)
    {
        var compileResult = _csharpCompiler.Compile(code);
        if (!compileResult.Success)
        {
            return new List<ProcessOutput>
            {
                new()
                {
                    Error = string.Join(Environment.NewLine, compileResult.Errors)
                }
            };
        }

        var runtimeConfigContent = _csharpRuntimeConfigProvider.GetRuntimeConfig();
        var workDirFiles = GetWorkDirFiles(compileResult.Assembly, runtimeConfigContent);
        return await base.Run(workDirFiles, ProcessName, new []{_assemblyNameWithExtension}, processCount);
    }

    private IEnumerable<WorkDirFileDescription> GetWorkDirFiles(byte[] compiledAssembly, byte[] runtimeConfigContent)
    {
        return new List<WorkDirFileDescription>
        {
            new()
            {
                FileName = _assemblyNameWithExtension,
                Content = compiledAssembly
            },
            new()
            {
                FileName = $"{_assemblyName}.runtimeconfig.json",
                Content = runtimeConfigContent
            },
        };
    }
}