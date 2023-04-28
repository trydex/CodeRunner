using CodeRunner.Common;
using CodeRunner.Common.Kafka.Producer;
using CodeRunner.Executor.Modules.Execute.Models;
using CodeRunner.Executor.Repositories;
using CodeRunner.Executor.Services;

namespace CodeRunner.Executor.Modules.Execute.Endpoints;

public static class ExecuteScriptEndpoint
{
    public static async Task<ExecutionResult> Run(
        IMessageProducer messageProducer,
        IScriptsRepository scriptsRepository,
        IExecutionResultsRepository executionResultsRepository,
        ICacheService cacheService,
        SubmittedScript script,
        ILogger<ExecuteModule> logger)
    {
        var result = new ExecutionResult
        {
            Id = script.Id,
            Status = ExecutionState.Pending
        };

        try
        {
            var scriptHashCode = script.GetHashCode();
            logger.LogInformation($"Trying to get the result of executing the script with Id = {script.Id} from the cache by hash = {scriptHashCode}");

            var (existsInCache, cachedResult) = await cacheService.TryGetValue<int, ExecutionResult>(scriptHashCode);
            if (!existsInCache)
            {
                await cacheService.SetValue(script.Id, scriptHashCode);

                logger.LogInformation($"Save the script with Id = {script.Id} to db");
                await scriptsRepository.CreateAsync(script);

                logger.LogInformation($"Enqueue the script with Id = {script.Id} to the message broker");
                await messageProducer.Write(script);

                await executionResultsRepository.CreateAsync(result);
            }
            else
            {
                result = cachedResult;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex,$"Execute script with Id = {script.Id} failed");
            throw;
        }

        logger.LogInformation("Try to get script execution result from cache by hash");

        return result!;
    }

    public static async Task<ExecutionResult> GetResult(
        Guid scriptId,
        IExecutionResultsRepository executionResultsRepository,
        ICacheService cacheService)
    {
        var (hashCodeExistsInCache, hashCode) = await cacheService.TryGetValue<Guid, int>(scriptId);
        if (hashCodeExistsInCache)
        {
            var (resultExistsInCache, cachedExecutionResult) = await cacheService.TryGetValue<int, ExecutionResult>(hashCode);
            if (resultExistsInCache)
            {
                return cachedExecutionResult!;
            }
        }

        var executionResult = await executionResultsRepository.GetAsync(scriptId);
        return executionResult!;
    }
}