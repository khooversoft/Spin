using ObjectStore.sdk.Application;
using Toolbox.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Logging
    .AddConsole()
    .AddDebug();

ObjectStoreOption option = builder.Configuration
    .Bind<ObjectStoreOption>()
    .Verify();

builder.Services.AddObjectStore(option);

builder.Logging.AddApplicationInsights(
    configureTelemetryConfiguration: (config) => config.ConnectionString = option.AppicationInsightsConnectionString,
    configureApplicationInsightsLoggerOptions: (options) => { }
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapObjectStore();
app.Run();
