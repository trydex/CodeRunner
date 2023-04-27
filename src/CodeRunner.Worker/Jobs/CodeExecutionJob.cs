using Quartz;

namespace CodeRunner.Worker.Jobs;

public class CodeExecutionJob : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        Console.WriteLine("Do some job at " + DateTime.Now.ToString("T"));

        return Task.FromResult(true);
    }
}