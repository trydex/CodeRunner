using CodeRunner.Common.Kafka.Consumer;
using CodeRunner.Common.Kafka.Producer;
using CodeRunner.Common.Quartz;
using CodeRunner.Executor.Jobs;
using CodeRunner.Executor.Modules.Execute.Models;
using CodeRunner.Executor.Services;
using CodeRunner.Executor.Settings;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Quartz;
using StackExchange.Redis;

namespace CodeRunner.Executor.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<BusSettings>(configuration.GetSection("Bus"));
        services.Configure<ScriptsDatabaseSettings>(configuration.GetSection("ScriptsDatabase"));
        services.Configure<CacheSettings>(configuration.GetSection("Cache"));
        services.Configure<CacheSettings>(configuration.GetSection("Jobs"));

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

        services.AddSingleton<IMessageConsumer, MessageConsumer>(provider =>
        {
            var busSettings = provider.GetService<IOptions<BusSettings>>().Value;

            return new MessageConsumer(new MessageConsumerOptions
            {
                Server = busSettings.Server,
                Topic = busSettings.ResultsTopicName,
                Group = busSettings.ResultsConsumerGroup,
                AutoOffsetReset = AutoOffsetReset.Latest
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
    public static IServiceCollection AddQuartzJobs(this IServiceCollection services, JobSettings jobSettings)
    {
        services.AddScoped<PullExecutionResultsJob>();

        services.AddQuartz(q =>
        {
            q.UseJobFactory<JobFactory>();

            AddJob<PullExecutionResultsJob>(q);
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        return services;

        void AddJob<TJob>(IServiceCollectionQuartzConfigurator quartzConfigurator) where TJob : IJob
        {
            var jobKey = new JobKey(typeof(TJob).Name);
            quartzConfigurator.AddJob<TJob>(opts => opts.WithIdentity(jobKey));

            quartzConfigurator.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity($"{jobKey.Name}-trigger")
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(jobSettings.RepeatIntervalInSeconds)
                    .RepeatForever())
            );
        }
    }
}