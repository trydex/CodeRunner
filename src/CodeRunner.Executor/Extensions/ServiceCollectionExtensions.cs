using CodeRunner.Common.Kafka.Producer;
using CodeRunner.Executor.Modules.Execute.Models;
using CodeRunner.Executor.Services;
using CodeRunner.Executor.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using StackExchange.Redis;

namespace CodeRunner.Executor.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<BusSettings>(configuration.GetSection("Bus"));
        services.Configure<ScriptsDatabaseSettings>(configuration.GetSection("ScriptsDatabase"));
        services.Configure<CacheSettings>(configuration.GetSection("Cache"));

        return services;
    }

    public static IServiceCollection AddBusServices(this IServiceCollection services)
    {
        services.AddSingleton<IMessageProducer, MessageProducer>(provider =>
        {
            var busSettings = provider.GetService<IOptions<BusSettings>>().Value;

            return new MessageProducer(new MessageProducerOptions
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
            var dbSettings = provider.GetService<IOptions<ScriptsDatabaseSettings>>();

            var mongoClient = new MongoClient(
                dbSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                dbSettings.Value.DatabaseName);

            return mongoDatabase;
        });

        services.AddScoped<IMongoCollection<SubmittedScript>>(provider =>
        {
            var dbSettings = provider.GetService<IOptions<ScriptsDatabaseSettings>>();
            var mongoDatabase = provider.GetService<IMongoDatabase>();

            var collection = mongoDatabase
                .GetCollection<SubmittedScript>(dbSettings.Value.ScriptsCollectionName);

            return collection;
        });

        services.AddScoped<IMongoCollection<ExecutionResult>>(provider =>
        {
            var dbSettings = provider.GetService<IOptions<ScriptsDatabaseSettings>>();
            var mongoDatabase = provider.GetService<IMongoDatabase>();

            var collection = mongoDatabase
                .GetCollection<ExecutionResult>(dbSettings.Value.ExecutionResultsCollectionName);

            return collection;
        });

        return services;
    }

    public static IServiceCollection AddCache(this IServiceCollection services)
    {
        services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            var cacheSettings = provider.GetService<IOptions<CacheSettings>>().Value;
            var multiplexer = ConnectionMultiplexer.Connect(cacheSettings.ConnectionString);

            return multiplexer;
        });

        services.AddSingleton<ICacheService, CacheService>();

        return services;
    }
}