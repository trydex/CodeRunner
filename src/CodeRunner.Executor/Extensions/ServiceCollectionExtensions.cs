using CodeRunner.Executor.Modules.Execute.Models;
using CodeRunner.Executor.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CodeRunner.Executor.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureSettings(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.Configure<BusSettings>(configuration.GetSection("Bus"));
        serviceCollection.Configure<ScriptsDatabaseSettings>(configuration.GetSection("ScriptsDatabase"));

        return serviceCollection;
    }

    public static IServiceCollection AddDataBase(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IMongoDatabase>(provider =>
        {
            var scriptsDatabaseSettings = provider.GetService<IOptions<ScriptsDatabaseSettings>>();

            var mongoClient = new MongoClient(
                scriptsDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                scriptsDatabaseSettings.Value.DatabaseName);

            return mongoDatabase;
        });

        serviceCollection.AddScoped<IMongoCollection<SubmittedScript>>(provider =>
        {
            var scriptsDatabaseSettings = provider.GetService<IOptions<ScriptsDatabaseSettings>>();
            var mongoDatabase = provider.GetService<IMongoDatabase>();

            var scriptsCollection = mongoDatabase
                .GetCollection<SubmittedScript>(scriptsDatabaseSettings.Value.ScriptsCollectionName);

            return scriptsCollection;
        });

        return serviceCollection;
    }
}