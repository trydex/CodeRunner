using CodeRunner.Common;
using CodeRunner.Executor.Modules.Execute.Models;
using CodeRunner.Executor.Repositories;
using CodeRunner.Executor.Settings;
using Microsoft.Extensions.Options;

namespace CodeRunner.Executor.Modules.Execute.Endpoints;

public static class ExecuteScriptEndpoint
{
    public static async Task<Execution> Run(
        IOptions<BusSettings> configuration,
        IMessageWriter messageWriter,
        IScriptsRepository scriptsRepository,
        SubmittedScript script
        )
    {
        try
        {
            var busSettings = configuration.Value;
            //try to get script execution result from cache by hash
            //if doesn't exists enqueue to the execution pool
            await scriptsRepository.CreateAsync(script);
            await messageWriter.Write(busSettings.Server, busSettings.ScriptsTopicName, script);

            return default;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occured: {ex.Message}");
            throw;
        }
    }

    public static SubmittedScript Get(int id, IOptions<BusSettings> configuration, IMessageConsumer messageConsumer)
    {
        var busSettings = configuration.Value;
        return messageConsumer.Consume<SubmittedScript>(busSettings.Server, busSettings.ScriptsTopicName, $"{busSettings.ScriptsTopicName}-consumers");
    }
}