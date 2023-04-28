using CodeRunner.Common;
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

    private static IHostBuilder CreateDefaultBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(app =>
            {
                app.AddJsonFile("appsettings.json");
            })
            .ConfigureServices(ConfigureServices);
    }

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        ConfigureSettings(context, services);
        AddApplicationDependencies(context, services);
        AddQuartz(services);
    }

    private static void ConfigureSettings(HostBuilderContext context, IServiceCollection services)
    {
        services.Configure<BusSettings>(context.Configuration.GetSection("Bus"));
        services.Configure<ExecutionResultsDatabaseSettings>(context.Configuration.GetSection("ExecutionResultsDatabase"));
    }

    private static void AddQuartz(IServiceCollection services)
    {
        services.AddQuartz(q =>
        {
            q.UseJobFactory<JobFactory>();

            var jobKey = new JobKey(nameof(CodeExecutionJob));
            q.AddJob<CodeExecutionJob>(opts => opts.WithIdentity(jobKey));

            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity($"{nameof(CodeExecutionJob)}-trigger")
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(1)
                    .RepeatForever())
            );
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
    }

    private static void AddApplicationDependencies(HostBuilderContext context, IServiceCollection services)
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

            var mongoClient = new MongoClient(
                dbSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                dbSettings.Value.DatabaseName);

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

        services.AddSingleton<IMessageWriter, MessageWriter>(provider =>
        {
            var busSettings = provider.GetService<IOptions<BusSettings>>().Value;

            return new MessageWriter(new WriterOptions
            {
                Server = busSettings.Server,
                Topic = busSettings.ExecutionResultsTopicName
            });
        });

        services.AddSingleton<IMessageConsumer, MessageConsumer>(provider =>
        {
            var busSettings = provider.GetService<IOptions<BusSettings>>().Value;

            return new MessageConsumer(new ConsumerOptions
            {
                Server = busSettings.Server,
                Topic = busSettings.ScriptsTopicName,
                Group = busSettings.ScriptsConsumerGroup,
                AutoOffsetReset = AutoOffsetReset.Latest
            });
        });
    }
}