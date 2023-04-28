using CodeRunner.Worker.Models;
using MongoDB.Driver;

namespace CodeRunner.Worker.Repositories;

public interface IScriptResultsRepository
{
    public Task<ScriptExecutionResult?> GetAsync(Guid id);
    public Task CreateAsync(ScriptExecutionResult script);
}

public class ScriptResultsRepository : IScriptResultsRepository
{
    private readonly IMongoCollection<ScriptExecutionResult> _collection;

    public ScriptResultsRepository(IMongoCollection<ScriptExecutionResult> collection)
    {
        _collection = collection;
    }

    public async Task<List<ScriptExecutionResult>> GetAsync() =>
        await _collection.Find(_ => true).ToListAsync();

    public async Task<ScriptExecutionResult?> GetAsync(Guid id) =>
        await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(ScriptExecutionResult script) =>
        await _collection.InsertOneAsync(script);
}