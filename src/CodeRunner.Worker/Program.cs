using CodeRunner.Worker.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;

class Program
{
    static void Main(string[] args)
    {
        CreateDefaultBuilder().Build().Run();
    }

    static IHostBuilder CreateDefaultBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(app =>
            {
                app.AddJsonFile("appsettings.json");
            })
            .ConfigureServices(ConfigureQuartz);
    }

    static void ConfigureQuartz(IServiceCollection services)
    {
        services.AddQuartz(q =>
        {
            var jobKey = new JobKey(nameof(CodeExecutionJob));
            q.AddJob<CodeExecutionJob>(opts => opts.WithIdentity(jobKey));

            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity($"{nameof(CodeExecutionJob)}-trigger")
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(10)
                    .RepeatForever())
            );
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
    }
}