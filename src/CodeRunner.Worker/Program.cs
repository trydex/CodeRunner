using CodeRunner.Common;
using CodeRunner.Worker.Jobs;
using CodeRunner.Worker.Services;
using CodeRunner.Worker.Settings;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Quartz;
using Quartz.Spi;

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
        AddQuartz(services);
        AddApplicationDependencies(context, services);
    }

    private static void ConfigureSettings(HostBuilderContext context, IServiceCollection services)
    {
        services.Configure<BusSettings>(context.Configuration.GetSection("Bus"));
        services.Configure<ScriptsDatabaseSettings>(context.Configuration.GetSection("ScriptsDatabase"));
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

        services.AddSingleton<ScriptRunnerService>();
        services.AddSingleton<IMessageWriter, MessageWriter>(provider =>
        {
            var busSettings = provider.GetService<IOptions<BusSettings>>().Value;

            return new MessageWriter(new WriterOptions
            {
                Server = busSettings.Server,
                Topic = busSettings.ScriptsTopicName
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