using CodeRunner.Executor.Modules.Execute.Models;
using MongoDB.Driver;

namespace CodeRunner.Executor.Repositories;

public interface IExecutionResultsRepository
{
    public Task<ExecutionResult?> GetAsync(Guid id);
    public Task CreateAsync(ExecutionResult script);
    Task UpdateAsync(Guid id, ExecutionResult updatedScript);
}

public class ExecutionResultsRepository : IExecutionResultsRepository
{
    private readonly IMongoCollection<ExecutionResult> _collection;

    public ExecutionResultsRepository(IMongoCollection<ExecutionResult> collection)
    {
        _collection = collection;
    }

    public async Task<List<ExecutionResult>> GetAsync() =>
        await _collection.Find(_ => true).ToListAsync();

    public async Task<ExecutionResult?> GetAsync(Guid id) =>
        await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(ExecutionResult script) =>
        await _collection.InsertOneAsync(script);

    public async Task UpdateAsync(Guid id, ExecutionResult updatedScript) =>
        await _collection.ReplaceOneAsync(x => x.Id == id, updatedScript);
}