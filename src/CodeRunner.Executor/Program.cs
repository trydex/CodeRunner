using CodeRunner.Executor.Extensions;
using CodeRunner.Executor.Settings;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddCors();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.RegisterModules();
builder.Services.AddDataBase(configuration.GetSection("ScriptsDatabase").Get<ScriptsDatabaseSettings>());
builder.Services.AddBusServices(configuration.GetSection("Bus").Get<BusSettings>());
builder.Services.AddCache(configuration.GetSection("Cache").Get<CacheSettings>());
builder.Services.AddQuartzJobs(configuration.GetSection("Jobs").Get<JobSettings>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapEndpoints();

app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader()
);

app.Run();