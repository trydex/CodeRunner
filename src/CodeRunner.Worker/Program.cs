using CodeRunner.Common;
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
using Microsoft.Extensions.Options;
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
                configurationBuilder.AddJsonFile(AppSettingsConstants.FileName);
                Configuration = configurationBuilder.Build();
            })
            .ConfigureServices(ConfigureServices);
    }

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        ConfigureSettings(services);
        AddApplicationDependencies(services);
        AddQuartz(services);
    }

    private static void ConfigureSettings(IServiceCollection services)
    {
        services.Configure<BusSettings>(Configuration.GetSection(AppSettingsConstants.BusSettingsSectionName));
        services.Configure<ExecutionResultsDatabaseSettings>(Configuration.GetSection(AppSettingsConstants.ExecutionResultsDatabaseSectionName));
        services.Configure<JobSettings>(Configuration.GetSection(AppSettingsConstants.JobSettingsSectionName));
    }

    private static void AddQuartz(IServiceCollection services)
    {
        services.AddQuartz(q =>
        {
            q.UseJobFactory<JobFactory>();

            var jobKey = new JobKey(nameof(CodeExecutionJob));
            q.AddJob<CodeExecutionJob>(opts => opts.WithIdentity(jobKey));

            var repeatInterval = Options.Create(
                    Configuration.GetSection(AppSettingsConstants.JobSettingsSectionName).Get<JobSettings>()
                    ).Value
                .RepeatIntervalInSeconds;

            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity($"{jobKey.Name}-trigger")
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(repeatInterval)
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

        services.AddScoped<IMongoDatabase>(provider =>
        {
            var dbSettings = provider.GetService<IOptions<ExecutionResultsDatabaseSettings>>();

            var mongoClient = new MongoClient(dbSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);

            return mongoDatabase;
        });

        services.AddScoped<IMongoCollection<ScriptExecutionResult>>(provider =>
        {
            var dbSettings = provider.GetService<IOptions<ExecutionResultsDatabaseSettings>>();
            var mongoDatabase = provider.GetService<IMongoDatabase>();

            var collection = mongoDatabase
                .GetCollection<ScriptExecutionResult>(dbSettings.Value.ExecutionResultsCollectionName);

            return collection;
        });

        services.AddSingleton<IMessageProducer, MessageProducer>(provider =>
        {
            var busSettings = provider.GetService<IOptions<BusSettings>>().Value;

            return new MessageProducer(new MessageProducerOptions
            {
                Server = busSettings.Server,
                Topic = busSettings.ExecutionResultsTopicName
            });
        });

        services.AddSingleton<IMessageConsumer, MessageConsumer>(provider =>
        {
            var busSettings = provider.GetService<IOptions<BusSettings>>().Value;

            return new MessageConsumer(new MessageConsumerOptions
            {
                Server = busSettings.Server,
                Topic = busSettings.ScriptsTopicName,
                Group = busSettings.ScriptsConsumerGroup,
                AutoOffsetReset = AutoOffsetReset.Latest
            });
        });
    }
}