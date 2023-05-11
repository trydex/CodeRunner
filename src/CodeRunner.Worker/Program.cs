using CodeRunner.Common.Kafka.Consumer;
using CodeRunner.Common.Kafka.Producer;
using CodeRunner.Common.Quartz;
using CodeRunner.Worker;
using CodeRunner.Worker.Jobs;
using CodeRunner.Worker.Models;
using CodeRunner.Worker.Repositories;
using CodeRunner.Worker.Services;
using CodeRunner.Worker.Settings;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Quartz;

public class Program
{
    private static void Main(string[] args)
    {
        CreateDefaultBuilder().Build().Run();
    }

    private static IConfiguration Configuration { get; set; }

    private static IHostBuilder CreateDefaultBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(configurationBuilder =>
            {
                configurationBuilder.AddJsonFile(AppSettingsConstants.FileName, optional: true, reloadOnChange: true);

#if DEBUG
                configurationBuilder.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
#endif
                Configuration = configurationBuilder.Build();
            })
            .ConfigureServices(ConfigureServices);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        AddApplicationDependencies(services);
        AddQuartz(services);
    }

    private static void AddQuartz(IServiceCollection services)
    {
        var jobSettings = Configuration.GetSection(AppSettingsConstants.JobSettingsSectionName).Get<JobSettings>();

        services.AddQuartz(q =>
        {
            q.UseJobFactory<JobFactory>();

            var jobKey = new JobKey(nameof(CodeExecutionJob));
            q.AddJob<CodeExecutionJob>(opts => opts.WithIdentity(jobKey));

            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity($"{jobKey.Name}-trigger")
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(jobSettings.RepeatIntervalInSeconds)
                    .RepeatForever())
            );
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
    }

    private static void AddApplicationDependencies(IServiceCollection services)
    {
        services.AddScoped<CodeExecutionJob>();
        services.AddScoped<ScriptRunnerService>();
        services.AddScoped<IRuntimeConfigProvider, RuntimeConfigProvider>();
        services.AddScoped<IRunner, Runner>();
        services.AddScoped<ICompiler, Compiler>();
        services.AddScoped<IScriptRunnerService, ScriptRunnerService>();
        services.AddScoped<IScriptResultsRepository, ScriptResultsRepository>();

        var dbSettings = Configuration
            .GetSection(AppSettingsConstants.ExecutionResultsDatabaseSectionName)
            .Get<ExecutionResultsDatabaseSettings>();

        services.AddScoped<IMongoDatabase>(_ =>
        {
            var mongoClient = new MongoClient(dbSettings.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(dbSettings.DatabaseName);

            return mongoDatabase;
        });

        services.AddScoped<IMongoCollection<ScriptExecutionResult>>(provider =>
        {
            var mongoDatabase = provider.GetService<IMongoDatabase>();

            var collection = mongoDatabase
                .GetCollection<ScriptExecutionResult>(dbSettings.ExecutionResultsCollectionName);

            return collection;
        });

        var busSettings = Configuration.GetSection(AppSettingsConstants.BusSettingsSectionName).Get<BusSettings>();

        services.AddSingleton<IMessageProducer, MessageProducer>(_ =>
            new MessageProducer(new MessageProducerOptions
            {
                Server = busSettings.Server,
                Topic = busSettings.ExecutionResultsTopicName
            }));

        services.AddSingleton<IMessageConsumer, MessageConsumer>(_ =>
            new MessageConsumer(new MessageConsumerOptions
            {
                Server = busSettings.Server,
                Topic = busSettings.ScriptsTopicName,
                Group = busSettings.ScriptsConsumerGroup,
                AutoOffsetReset = AutoOffsetReset.Latest
            }));
    }
}