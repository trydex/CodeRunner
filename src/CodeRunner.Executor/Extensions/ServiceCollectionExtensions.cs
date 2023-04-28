using CodeRunner.Common;
using CodeRunner.Executor.Modules.Execute.Models;
using CodeRunner.Executor.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CodeRunner.Executor.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<BusSettings>(configuration.GetSection("Bus"));
        services.Configure<ScriptsDatabaseSettings>(configuration.GetSection("ScriptsDatabase"));

        return services;
    }

    public static IServiceCollection AddBusServices(this IServiceCollection services)
    {
        services.AddSingleton<IMessageWriter, MessageWriter>(provider =>
        {
            var busSettings = provider.GetService<IOptions<BusSettings>>().Value;

            return new MessageWriter(new WriterOptions
            {
                Server = busSettings.Server,
                Topic = busSettings.ScriptsTopicName
            });
        });

        return services;
    }

    public static IServiceCollection AddDataBase(this IServiceCollection services)
    {
        services.AddScoped<IMongoDatabase>(provider =>
        {
            var scriptsDatabaseSettings = provider.GetService<IOptions<ScriptsDatabaseSettings>>();

            var mongoClient = new MongoClient(
                scriptsDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                scriptsDatabaseSettings.Value.DatabaseName);

            return mongoDatabase;
        });

        services.AddScoped<IMongoCollection<SubmittedScript>>(provider =>
        {
            var scriptsDatabaseSettings = provider.GetService<IOptions<ScriptsDatabaseSettings>>();
            var mongoDatabase = provider.GetService<IMongoDatabase>();

            var scriptsCollection = mongoDatabase
                .GetCollection<SubmittedScript>(scriptsDatabaseSettings.Value.ScriptsCollectionName);

            return scriptsCollection;
        });

        return services;
    }
}