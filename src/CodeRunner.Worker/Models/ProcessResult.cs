namespace CodeRunner.Worker.Models;

public class ProcessResult
{
    public int Id { get; set; }
    public string Output { get; set; }
    public string Error { get; set; }
}