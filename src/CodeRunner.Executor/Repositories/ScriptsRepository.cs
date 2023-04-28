using CodeRunner.Executor.Modules.Execute.Models;
using CodeRunner.Executor.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CodeRunner.Executor.Repositories;

public interface IScriptsRepository
{
    public Task<SubmittedScript?> GetAsync(Guid id);
    public Task CreateAsync(SubmittedScript script);
}

public class ScriptsRepository : IScriptsRepository
{
    private readonly IMongoCollection<SubmittedScript> _collection;

    public ScriptsRepository(IMongoCollection<SubmittedScript> collection)
    {
        _collection = collection;
    }

    public async Task<List<SubmittedScript>> GetAsync() =>
        await _collection.Find(_ => true).ToListAsync();

    public async Task<SubmittedScript?> GetAsync(Guid id) =>
        await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(SubmittedScript script) =>
        await _collection.InsertOneAsync(script);

    public async Task UpdateAsync(Guid id, SubmittedScript updatedScript) =>
        await _collection.ReplaceOneAsync(x => x.Id == id, updatedScript);

    public async Task RemoveAsync(Guid id) =>
        await _collection.DeleteOneAsync(x => x.Id == id);
}