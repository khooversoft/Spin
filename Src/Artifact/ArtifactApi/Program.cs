using Artifact.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spin.Common;
using System.Runtime.CompilerServices;
using Toolbox.Configuration;
using Toolbox.Extensions;

[assembly: InternalsVisibleTo("Directory.Test")]

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Logging
    .AddConsole()
    .AddFilter(x => true);

ApplicationOption option = builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"{{ConfigStore}}/Environments/{{runEnvironment}}-Spin.environment.json", JsonFileOption.Enhance)
    .AddCommandLine(args)
    .AddPropertyResolver()
    .Build()
    .Bind<ApplicationOption>()
    .VerifyPartial();


//await builder.Services.ConfigureArtifactService(option);
builder.Services.ConfigurePingService(builder.Logging);

//  ///////////////////////////////////////////////////////////////////////////////////////////////

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

//app.ConfigureArtifactService();

if (app.Environment.IsEnvironment("Test"))
    app.Run();
else
    app.Run(option.HostUrl ?? "http://localhost:5011");

