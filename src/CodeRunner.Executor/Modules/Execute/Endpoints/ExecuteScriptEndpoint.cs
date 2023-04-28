using CodeRunner.Common;
using CodeRunner.Executor.Modules.Execute.Models;
using CodeRunner.Executor.Repositories;

namespace CodeRunner.Executor.Modules.Execute.Endpoints;

public static class ExecuteScriptEndpoint
{
    public static async Task<Execution> Run(
        IMessageWriter messageWriter,
        IScriptsRepository scriptsRepository,
        SubmittedScript script,
        ILogger<ExecuteModule> logger)
    {
        try
        {
            var scriptHash = script.GetHashCode();
            logger.LogInformation($"Trying to get the result of executing the script with Id = {script.Id} from the cache by hash = {scriptHash}");

            //Todo: Add retrieve from cache
            var existsInCache = false;
            if (!existsInCache)
            {
                logger.LogInformation($"Save the script with Id = {script.Id} to db");
                await scriptsRepository.CreateAsync(script);

                logger.LogInformation($"Enqueue the script with Id = {script.Id} to the message broker");
                await messageWriter.Write(script);
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Execute script failed", ex, script);
            throw;
        }

        logger.LogInformation("Try to get script execution result from cache by hash");


        return default;
    }

    public static SubmittedScript Get(int id)
    {
        return default;
    }
}