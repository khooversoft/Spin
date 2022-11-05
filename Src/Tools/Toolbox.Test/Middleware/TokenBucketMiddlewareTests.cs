using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Middleware;
using Xunit;

namespace Toolbox.Test.Middleware;

public class TokenBucketMiddlewareTests
{
    [Fact]
    public async Task GivenNoConfiguration_ShouldFail()
    {
        var option = new TokenBucketOption().Verify();

        using var host = await new HostBuilder()
            .ConfigureWebHost(builder =>
            {
                builder
                    .UseTestServer()
                    .ConfigureServices(service =>
                    {
                        service.AddMemoryCache();
                        service.AddSingleton(option);
                    })
                    .Configure(app =>
                    {
                        app.UseMiddleware<TokenBucketMiddleware>();
                    });
            })
            .StartAsync();

        HttpClient client = host.GetTestClient();

        HttpResponseMessage response = await client.GetAsync("/");
        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task GivenWildcardConfiguration_ShouldFail()
    {
        var option = new TokenBucketOption
        {
            ProtectPaths = new[]
            {
                new TokenBucketPathOption
                {
                    PolicyName = "default",
                    Path = "*",
                    BucketSize = 10,
                    WindowSpan = TimeSpan.FromSeconds(10),
                }
            }.ToArray()
        }.Verify();

        using var host = await new HostBuilder()
            .ConfigureWebHost(builder =>
            {
                builder
                    .UseTestServer()
                    .ConfigureServices(service =>
                    {
                        service.AddMemoryCache();
                        service.AddSingleton(option);
                    })
                    .Configure(app =>
                    {
                        app.UseMiddleware<TokenBucketMiddleware>();
                    });
            })
            .StartAsync();

        HttpClient client = host.GetTestClient();

        HttpResponseMessage response = await client.GetAsync("/");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GivenWildcardConfiguration_WhenFullSpeed_ShouldPass()
    {
        var option = new TokenBucketOption
        {
            ProtectPaths = new[]
            {
                new TokenBucketPathOption
                {
                    PolicyName = "default",
                    Path = "*",
                    BucketSize = 10,
                    WindowSpan = TimeSpan.FromSeconds(10),
                }
            }.ToArray()
        }.Verify();

        using var host = await new HostBuilder()
            .ConfigureWebHost(builder =>
            {
                builder
                    .UseTestServer()
                    .ConfigureServices(service =>
                    {
                        service.AddMemoryCache();
                        service.AddSingleton(option);
                    })
                    .Configure(app =>
                    {
                        app.UseMiddleware<TokenBucketMiddleware>();
                    });
            })
            .StartAsync();

        HttpClient client = host.GetTestClient();

        const int count = 100;
        var tasks = Enumerable.Range(0, count)
            .Select(_ => client.GetAsync("/"))
            .ToList();

        IReadOnlyList<HttpResponseMessage> results = await Task.WhenAll(tasks);

        int passCount = results.Count(x => x.StatusCode == HttpStatusCode.NotFound);
        passCount.Should().BeGreaterThanOrEqualTo(10);

        int failedCount = results.Count(x => x.StatusCode == HttpStatusCode.TooManyRequests);
        failedCount.Should().Be(count - passCount);
    }

    [Fact]
    public async Task GivenWildcardConfiguration_WhenPaused_ShouldPass()
    {
        var option = new TokenBucketOption
        {
            ProtectPaths = new[]
            {
                new TokenBucketPathOption
                {
                    PolicyName = "default",
                    Path = "*",
                    BucketSize = 10,
                    WindowSpan = TimeSpan.FromSeconds(10),
                }
            }.ToArray()
        }.Verify();

        using var host = await new HostBuilder()
            .ConfigureWebHost(builder =>
            {
                builder
                    .UseTestServer()
                    .ConfigureServices(service =>
                    {
                        service.AddMemoryCache();
                        service.AddSingleton(option);
                    })
                    .Configure(app =>
                    {
                        app.UseMiddleware<TokenBucketMiddleware>();
                    });
            })
            .StartAsync();

        HttpClient client = host.GetTestClient();

        const int count = 100;
        var tasks = Enumerable.Range(0, count)
            .Select(_ => Task.Delay(TimeSpan.FromMilliseconds(100)).ContinueWith(_ => client.GetAsync("/")).Unwrap())
            .ToList();

        IReadOnlyList<HttpResponseMessage> results = await Task.WhenAll(tasks);

        int passCount = results.Count(x => x.StatusCode == HttpStatusCode.NotFound);
        passCount.Should().BeGreaterThanOrEqualTo(1);

        int failedCount = results.Count(x => x.StatusCode == HttpStatusCode.TooManyRequests);
        failedCount.Should().Be(count - passCount);
    }

    [Fact]
    public async Task GivenPathConfiguration_WhenPaused_ShouldPass()
    {
        var option = new TokenBucketOption
        {
            ProtectPaths = new[]
            {
                new TokenBucketPathOption
                {
                    PolicyName = "default",
                    Path = "*",
                    BucketSize = 10,
                    WindowSpan = TimeSpan.FromSeconds(10),
                }
            }.ToArray()
        }.Verify();

        int method1Count = 0;

        using var host = await new HostBuilder()
            .ConfigureWebHost(builder =>
            {
                builder
                    .UseTestServer()
                    .ConfigureServices(service =>
                    {
                        service.AddMemoryCache();
                        service.AddSingleton(option);
                        service.AddRouting();
                    })
                    .Configure(app =>
                    {
                        app.UseMiddleware<TokenBucketMiddleware>();
                        app.UseRouting();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapGet("/method1", async context =>
                            {
                                Interlocked.Increment(ref method1Count);
                                await context.Response.WriteAsync("method1");
                            });
                        });
                    });
            })
            .StartAsync();

        HttpClient client = host.GetTestClient();

        const int count = 100;
        var tasks = Enumerable.Range(0, count)
            .Select(_ => Task.Delay(TimeSpan.FromMilliseconds(100)).ContinueWith(_ => client.GetAsync("/method1")).Unwrap())
            .ToList();

        IReadOnlyList<HttpResponseMessage> results = await Task.WhenAll(tasks);

        int passCount = results.Count(x => x.StatusCode == HttpStatusCode.OK);
        passCount.Should().BeGreaterThanOrEqualTo(1);

        method1Count.Should().Be(passCount);

        int failedCount = results.Count(x => x.StatusCode == HttpStatusCode.TooManyRequests);
        failedCount.Should().Be(count - passCount);
    }

    [Fact]
    public async Task GivenMultiplePathConfiguration_WhenDelayed_ShouldPass()
    {
        var option = new TokenBucketOption
        {
            ProtectPaths = new[]
            {
                new TokenBucketPathOption
                {
                    PolicyName = "method1",
                    Path = "/method1",
                    BucketSize = 5,
                    WindowSpan = TimeSpan.FromSeconds(10),
                },
                new TokenBucketPathOption
                {
                    PolicyName = "method2",
                    Path = "/method2",
                    BucketSize = 50,
                    WindowSpan = TimeSpan.FromSeconds(10),
                },
            }.ToArray()
        }.Verify();

        int method1Count = 0;
        int method2Count = 0;

        using var host = await new HostBuilder()
            .ConfigureWebHost(builder =>
            {
                builder
                    .UseTestServer()
                    .ConfigureServices(service =>
                    {
                        service.AddMemoryCache();
                        service.AddSingleton(option);
                        service.AddRouting();
                    })
                    .Configure(app =>
                    {
                        app.UseMiddleware<TokenBucketMiddleware>();
                        app.UseRouting();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapGet("/method1", async context =>
                            {
                                Interlocked.Increment(ref method1Count);
                                await context.Response.WriteAsync("method1");
                            });
                            endpoints.MapGet("/method2", async context =>
                            {
                                Interlocked.Increment(ref method2Count);
                                await context.Response.WriteAsync("method2");
                            });
                        });
                    });
            })
            .StartAsync();

        HttpClient client = host.GetTestClient();

        // Call methods
        const int count = 1000;

        var method1Calls = Enumerable.Range(0, count)
            .Select(_ => Task.Delay(TimeSpan.FromMilliseconds(100)).ContinueWith(_ => client.GetAsync("/method1")).Unwrap());

        var method2Calls = Enumerable.Range(0, count)
            .Select(_ => Task.Delay(TimeSpan.FromMilliseconds(100)).ContinueWith(_ => client.GetAsync("/method2")).Unwrap());

        var tasks = method1Calls
            .Concat(method2Calls)
            .ToList();

        IReadOnlyList<HttpResponseMessage> results = await Task.WhenAll(tasks);

        // Verify result data
        var summary = results
            .Select(x => (Path: x.RequestMessage?.RequestUri?.LocalPath ?? "**", x.StatusCode))
            .GroupBy(x => x.Path + "_" + x.StatusCode.ToString())
            .Select(x => (
                x.First().Path,
                x.First().StatusCode,
                Count: x.Count()
                ))
            .ToList();

        var distinctCount = summary
            .Select(x => x.Count)
            .Distinct()
            .Count();
        distinctCount.Should().Be(4);

        int passCountMethod1 = summary.Where(x => x.Path == "/method1" && x.StatusCode == HttpStatusCode.OK).Sum(x => x.Count);
        int failCountMethod1 = summary.Where(x => x.Path == "/method1" && x.StatusCode == HttpStatusCode.TooManyRequests).Sum(x => x.Count);

        int passCountMethod2 = summary.Where(x => x.Path == "/method2" && x.StatusCode == HttpStatusCode.OK).Sum(x => x.Count);
        int failCountMethod2 = summary.Where(x => x.Path == "/method2" && x.StatusCode == HttpStatusCode.TooManyRequests).Sum(x => x.Count);

        passCountMethod1.Should().Be(method1Count);
        passCountMethod1.Should().BeGreaterThan(1);

        passCountMethod2.Should().Be(method2Count);
        passCountMethod2.Should().BeGreaterThan(1);

        failCountMethod1.Should().Be(count - passCountMethod1);
        failCountMethod2.Should().Be(count - passCountMethod2);
    }

    [Fact]
    public async Task GivenMultiplePathConfiguration_WithWilcard_ShouldPass()
    {
        // Because of the random shuffle, method1 on average should be less then method2 + method3
        var option = new TokenBucketOption
        {
            ProtectPaths = new[]
            {
                new TokenBucketPathOption
                {
                    PolicyName = "method1",
                    Path = "/method1",
                    BucketSize = 5,
                    WindowSpan = TimeSpan.FromSeconds(10),
                },
                new TokenBucketPathOption
                {
                    PolicyName = "default",
                    Path = "*",
                    BucketSize = 10,
                    WindowSpan = TimeSpan.FromSeconds(10),
                },
            }.ToArray()
        }.Verify();

        int method1Count = 0;
        int method2Count = 0;
        int method3Count = 0;

        using var host = await new HostBuilder()
            .ConfigureWebHost(builder =>
            {
                builder
                    .UseTestServer()
                    .ConfigureServices(service =>
                    {
                        service.AddMemoryCache();
                        service.AddRouting();
                    })
                    .Configure(app =>
                    {
                        app.UseMiddleware<TokenBucketMiddleware>(option);
                        app.UseRouting();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapGet("/method1", async context =>
                            {
                                Interlocked.Increment(ref method1Count);
                                await context.Response.WriteAsync("method1");
                            });
                            endpoints.MapGet("/method2", async context =>
                            {
                                Interlocked.Increment(ref method2Count);
                                await context.Response.WriteAsync("method2");
                            });
                            endpoints.MapGet("/method3", async context =>
                            {
                                Interlocked.Increment(ref method3Count);
                                await context.Response.WriteAsync("method2");
                            });
                        });
                    });
            })
            .StartAsync();

        HttpClient client = host.GetTestClient();

        // Call methods
        const int count = 1000;
        var rnd = new Random();

        var methodList = Enumerable.Range(0, count)
            .SelectMany(_ => new[] { "/method1", "/method2", "/method3" })
            .OrderBy(_ => rnd.Next())
            .Select(x => Task.Delay(TimeSpan.FromMilliseconds(200)).ContinueWith(_ => client.GetAsync(x)).Unwrap())
            .ToList();

        IReadOnlyList<HttpResponseMessage> results = await Task.WhenAll(methodList);

        // Verify result data
        var summary = results
            .Select(x => (Path: x.RequestMessage?.RequestUri?.LocalPath ?? "**", x.StatusCode))
            .GroupBy(x => x.Path + "_" + x.StatusCode.ToString())
            .Select(x => (
                x.First().Path,
                x.First().StatusCode,
                Count: x.Count()
                ))
            .ToList();
    }
}
