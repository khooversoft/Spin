using ObjectStore.sdk;
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
    .Bind<ObjectStoreOption>();

builder.Services.AddObjectStore(option);

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
