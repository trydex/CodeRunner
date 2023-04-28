using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace CodeRunner.Worker.Services;

public record CompileResult(bool Success, byte[] Assembly, IEnumerable<string> Errors);

public interface ICompiler
{
    CompileResult Compile(string source);
}

public class Compiler : ICompiler
{
    public CompileResult Compile(string source)
    {
        using var peStream = new MemoryStream();
        var result = GenerateCode(source).Emit(peStream);

        if (!result.Success)
        {
            var failures = result.Diagnostics
                .Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);

            var errors = new List<string>();

            foreach (var diagnostic in failures)
            {
                errors.Add($"{diagnostic.Id}: {diagnostic.GetMessage()}");
            }
            return new CompileResult(false, Array.Empty<byte>(), errors);
        }

        peStream.Seek(0, SeekOrigin.Begin);

        return new CompileResult(true, peStream.ToArray(), Enumerable.Empty<string>());
    }

    private static CSharpCompilation GenerateCode(string sourceCode)
    {
        var codeString = SourceText.From(sourceCode);
        var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp11);

        var parsedSyntaxTree = SyntaxFactory.ParseSyntaxTree(codeString, options);

        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Diagnostics.Process).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.ComponentModel.Component).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
        };

        Assembly.GetEntryAssembly()?.GetReferencedAssemblies().ToList()
            .ForEach(a => references.Add(MetadataReference.CreateFromFile(Assembly.Load(a).Location)));

        return CSharpCompilation.Create("Hello.dll",
            new[] { parsedSyntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.ConsoleApplication,
                optimizationLevel: OptimizationLevel.Release,
                assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default));
    }
}