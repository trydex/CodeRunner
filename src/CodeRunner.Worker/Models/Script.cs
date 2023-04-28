namespace CodeRunner.Worker.Models;

public record Script
{
    public Guid Id { get; set; }
    public int WorkerCount { get; set; }
    public string Code { get; set; }
}