using CodeRunner.Common.Kafka.Consumer;
using CodeRunner.Common.Kafka.Producer;
using CodeRunner.Common.Quartz;
using CodeRunner.Executor.Jobs;
using CodeRunner.Executor.Modules.Execute.Models;
using CodeRunner.Executor.Services;
using CodeRunner.Executor.Settings;
using Confluent.Kafka;
using MongoDB.Driver;
using Quartz;

namespace CodeRunner.Executor.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBusServices(this IServiceCollection services, BusSettings busSettings)
    {
        services.AddSingleton<IMessageProducer, MessageProducer>(_ =>
            new MessageProducer(new MessageProducerOptions
            {
                Server = busSettings.Server,
                Topic = busSettings.ScriptsTopicName
            }));

        services.AddSingleton<IMessageConsumer, MessageConsumer>(_ =>
            new MessageConsumer(new MessageConsumerOptions
            {
                Server = busSettings.Server,
                Topic = busSettings.ResultsTopicName,
                Group = busSettings.ResultsConsumerGroup,
                AutoOffsetReset = AutoOffsetReset.Latest
            }));

        return services;
    }

    public static IServiceCollection AddDataBase(this IServiceCollection services, ScriptsDatabaseSettings dbSettings)
    {
        services.AddScoped<IMongoDatabase>(_ =>
        {
            var mongoClient = new MongoClient(dbSettings.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(dbSettings.DatabaseName);

            return mongoDatabase;
        });

        services.AddScoped<IMongoCollection<SubmittedScript>>(provider =>
        {
            var mongoDatabase = provider.GetService<IMongoDatabase>();
            var collection = mongoDatabase.GetCollection<SubmittedScript>(dbSettings.ScriptsCollectionName);

            return collection;
        });

        services.AddScoped<IMongoCollection<ExecutionResult>>(provider =>
        {
            var mongoDatabase = provider.GetService<IMongoDatabase>();
            var collection = mongoDatabase.GetCollection<ExecutionResult>(dbSettings.ExecutionResultsCollectionName);

            return collection;
        });

        return services;
    }

    public static IServiceCollection AddCache(this IServiceCollection services, CacheSettings cacheSettings)
    {
        services.AddStackExchangeRedisCache(options => { options.Configuration = cacheSettings.ConnectionString; });

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