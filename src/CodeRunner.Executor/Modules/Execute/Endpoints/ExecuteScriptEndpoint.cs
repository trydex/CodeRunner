using CodeRunner.Common.Kafka.Messages;
using CodeRunner.Common.Kafka.Producer;
using CodeRunner.Executor.Extensions;
using CodeRunner.Executor.Modules.Execute.Models;
using CodeRunner.Executor.Repositories;
using CodeRunner.Executor.Services;

namespace CodeRunner.Executor.Modules.Execute.Endpoints;

public static class ExecuteScriptEndpoint
{
    public static async Task<ScriptExecutionResult> Execute(
        IMessageProducer messageProducer,
        IScriptsRepository scriptsRepository,
        IExecutionResultsRepository executionResultsRepository,
        ICacheService cacheService,
        SubmittedScript script,
        ILogger<ExecuteModule> logger)
    {
        var result = new ScriptExecutionResult
        {
            Id = script.Id,
            Status = ExecutionStatus.Pending
        };

        try
        {
            var scriptHashCode = script.GetHashCode();
            logger.LogInformation($"Trying to get the result of executing the script with Id = {0} from the cache by hash = {1}", script.Id, scriptHashCode);

            var (existsInCache, cachedResult) = await cacheService.TryGetValue<int, ScriptExecutionResult>(scriptHashCode);
            if (!existsInCache)
            {
                await cacheService.SetValue(script.Id, scriptHashCode);

                logger.LogInformation($"Save the script with Id = {0} to db", script.Id);
                await scriptsRepository.CreateAsync(script);

                logger.LogInformation($"Enqueue the script with Id = {0} to the message broker", script.Id);
                await messageProducer.Write(script.ToMessage());

                await executionResultsRepository.CreateAsync(result);
            }
            else
            {
                result = cachedResult;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex,$"Execute script with Id = {0} failed", script.Id);
            throw;
        }

        return result!;
    }

    public static async Task<ScriptExecutionResult> GetResult(
        Guid scriptId,
        IExecutionResultsRepository executionResultsRepository,
        ICacheService cacheService)
    {
        var (hashCodeExistsInCache, hashCode) = await cacheService.TryGetValue<Guid, int>(scriptId);
        if (hashCodeExistsInCache)
        {
            var (resultExistsInCache, cachedExecutionResult) = await cacheService.TryGetValue<int, ScriptExecutionResult>(hashCode);
            if (resultExistsInCache)
            {
                return cachedExecutionResult!;
            }
        }

        var executionResult = await executionResultsRepository.GetAsync(scriptId);
        return executionResult!;
    }
}