using CodeRunner.Common.Kafka.Messages;
using CodeRunner.Worker.CodeRunners.CSharp;
using CodeRunner.Worker.CodeRunners.Golang;
using Microsoft.Extensions.DependencyInjection;

namespace CodeRunner.Worker.CodeRunners;

public interface IScriptRunnerFactory
{
    public IScriptRunner Create(Language language);
}

public class ScriptRunnerFactory : IScriptRunnerFactory
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ScriptRunnerFactory(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public IScriptRunner Create(Language language)
    {
        using var scope = _scopeFactory.CreateScope();
        switch (language)
        {
            case Language.CSharp:
                return scope.ServiceProvider.GetService<CSharpScriptRunner>();
            case Language.Golang:
                return scope.ServiceProvider.GetService<GolangScriptRunner>();
            default:
                throw new ArgumentOutOfRangeException(nameof(language), language, null);
        }
    }
}