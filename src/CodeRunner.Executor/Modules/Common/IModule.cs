namespace CodeRunner.Executor.Modules.Common;

public interface IModule
{
    IServiceCollection RegisterModule(IServiceCollection services);
    IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints);
}