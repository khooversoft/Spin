using System.Reflection;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.FluentUI.AspNetCore.Components;
using TicketShare.sdk;
using TicketShareWeb.Application;
using TicketShareWeb.Components;
using Toolbox.Orleans;
using Toolbox.Tools;

Console.WriteLine($"Ticket Share Web Server - Version {Assembly.GetExecutingAssembly().GetName().Version}");
Console.WriteLine();

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.AddApplicationConfiguration();

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddFluentUIComponents();

builder.AddTicketShareAuthentication();

builder.Host.UseOrleans((context, silo) =>
{
    silo.UseLocalhostClustering();
    silo.AddTickShareCluster(context);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    //var x = new Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddleware()
    //app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
