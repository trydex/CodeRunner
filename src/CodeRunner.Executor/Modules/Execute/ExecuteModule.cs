using CodeRunner.Common;
using CodeRunner.Executor.Modules.Common;
using CodeRunner.Executor.Modules.Execute.Endpoints;
using CodeRunner.Executor.Repositories;
using CodeRunner.Executor.Settings;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace CodeRunner.Executor.Modules.Execute;

public class ExecuteModule : IModule
{
    public IServiceCollection RegisterModule(IServiceCollection builder)
    {
        builder.AddScoped<IScriptsRepository, ScriptsRepository>();

        return builder;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/execute", ExecuteScriptEndpoint.Run);
        endpoints.MapGet("/execute/{id}", ExecuteScriptEndpoint.Get);

        return endpoints;
    }
}
