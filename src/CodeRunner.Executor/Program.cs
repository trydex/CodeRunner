using CodeRunner.Executor.Extensions;
using CodeRunner.Executor.Settings;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddCors();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.RegisterModules();
builder.Services.ConfigureSettings(builder.Configuration);
builder.Services.AddDataBase();
builder.Services.AddBusServices();
builder.Services.AddCache(builder.Configuration.GetSection("Cache").Get<CacheSettings>());
builder.Services.AddQuartzJobs(builder.Configuration.GetSection("Jobs").Get<JobSettings>());

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