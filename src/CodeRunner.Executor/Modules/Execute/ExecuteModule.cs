using CodeRunner.Executor.Modules.Common;
using CodeRunner.Executor.Modules.Execute.Endpoints;
using CodeRunner.Executor.Repositories;

namespace CodeRunner.Executor.Modules.Execute;

public class ExecuteModule : IModule
{
    public IServiceCollection RegisterModule(IServiceCollection services)
    {
        services.AddScoped<IScriptsRepository, ScriptsRepository>();
        services.AddScoped<IExecutionResultsRepository, ExecutionResultsRepository>();

        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/execute", ExecuteScriptEndpoint.Execute);
        endpoints.MapGet("/execute/{scriptId}", ExecuteScriptEndpoint.GetResult);

        return endpoints;
    }
}
