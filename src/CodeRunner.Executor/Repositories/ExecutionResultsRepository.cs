using CodeRunner.Executor.Modules.Execute.Models;
using MongoDB.Driver;

namespace CodeRunner.Executor.Repositories;

public interface IExecutionResultsRepository
{
    public Task<ScriptExecutionResult?> GetAsync(Guid id);
    public Task CreateAsync(ScriptExecutionResult script);
    Task UpdateAsync(Guid id, ScriptExecutionResult updatedScript);
}

public class ExecutionResultsRepository : IExecutionResultsRepository
{
    private readonly IMongoCollection<ScriptExecutionResult> _collection;

    public ExecutionResultsRepository(IMongoCollection<ScriptExecutionResult> collection)
    {
        _collection = collection;
    }

    public async Task<List<ScriptExecutionResult>> GetAsync() =>
        await _collection.Find(_ => true).ToListAsync();

    public async Task<ScriptExecutionResult?> GetAsync(Guid id) =>
        await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(ScriptExecutionResult script) =>
        await _collection.InsertOneAsync(script);

    public async Task UpdateAsync(Guid id, ScriptExecutionResult updatedScript) =>
        await _collection.ReplaceOneAsync(x => x.Id == id, updatedScript);
}