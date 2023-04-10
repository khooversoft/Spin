using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Work;
using Xunit;

namespace Toolbox.Test.Work;

public class WorkflowTest
{
    [Fact]
    public async Task GivenSimpleWorkflow_WhenRun_ShouldPass()
    {
        IServiceProvider serviceProvider = new ServiceCollection()
            .AddSingleton<TestMessageActivity>()
            .AddSingleton<TestStringActivity>()
            .AddSingleton<WorkflowBuilder>()
            .AddLogging(config =>
            {
                config.AddDebug();
                config.AddFilter(x => true);
            })
            .BuildServiceProvider();

        Workflow workflow = serviceProvider.GetRequiredService<WorkflowBuilder>()
            .Add<TestMessageActivity>()
            .Add<TestStringActivity>()
            .Build();

        string testStringResponse = await workflow.Send<TestStringActivity>("hello");
        testStringResponse.Should().Be("hello***");

        Message sendMessage = new Message { Name = "sendMessage" };
        Message messageResponse = await workflow.Send<TestMessageActivity, Message, Message>(sendMessage);
        messageResponse.Name.Should().Be("sendMessage_response");
    }

    [Fact]
    public async Task GivenSimpleWorkflowWithName_WhenRun_ShouldPass()
    {
        IServiceProvider serviceProvider = new ServiceCollection()
            .AddSingleton<TestMessageActivity>()
            .AddSingleton<TestStringActivity>()
            .AddSingleton<WorkflowBuilder>()
            .AddLogging(config =>
            {
                config.AddDebug();
                config.AddFilter(x => true);
            })
            .BuildServiceProvider();

        Workflow workflow = serviceProvider.GetRequiredService<WorkflowBuilder>()
            .Add<TestMessageActivity>("testMessage")
            .Add<TestStringActivity>("testString")
            .Build();

        string testStringResponse = await workflow.Send("testString", "hello");
        testStringResponse.Should().Be("hello***");

        Message sendMessage = new Message { Name = "sendMessage" };
        Message messageResponse = await workflow.Send<Message, Message>("testMessage", sendMessage);
        messageResponse.Name.Should().Be("sendMessage_response");
    }

    [Fact]
    public async Task GivenWorkflow_WithMultipleActivity_WhenMessageSend_ShouldPass()
    {
        IServiceProvider serviceProvider = new ServiceCollection()
            .AddSingleton<TestMessageActivity>()
            .AddSingleton<TestStringActivity>()
            .AddSingleton<TestBounceActivity>()
            .AddSingleton<WorkflowBuilder>()
            .AddLogging(config =>
            {
                config.AddDebug();
                config.AddFilter(x => true);
            })
            .BuildServiceProvider();

        Workflow workflow = serviceProvider.GetRequiredService<WorkflowBuilder>()
            .Add<TestMessageActivity>()
            .Add<TestStringActivity>()
            .Add<TestBounceActivity>()
            .Build();

        BounceMessage sendMessage = new BounceMessage { Name = "bounceSendMessage" };
        Message messageResponse = await workflow.Send<TestBounceActivity, BounceMessage, Message>(sendMessage);
        messageResponse.Name.Should().Be("bounceSendMessage_response[bounce]");
    }

    private record Message
    {
        public string Name { get; init; } = null!;
        public Context Context { get; init; } = new Context();
    }

    private record BounceMessage
    {
        public string Name { get; init; } = null!;
    }


    private class TestMessageActivity : WorkflowActivity<Message, Message>
    {
        protected override Task<Message> Send(Message request, Workflow workflow) => Task.FromResult(request with { Name = request.Name + "_response" });
    }

    private class TestStringActivity : WorkflowActivity<string, string>
    {
        protected override Task<string> Send(string request, Workflow workflow) => Task.FromResult(request + "***");
    }

    private class TestBounceActivity : WorkflowActivity<BounceMessage, Message>
    {
        protected override async Task<Message> Send(BounceMessage request, Workflow workflow)
        {
            var requestMessage = new Message { Name = request.Name };
            Message responseMessage = await workflow.Send<TestMessageActivity, Message, Message>(requestMessage);
            return responseMessage with { Name = responseMessage.Name + "[bounce]" };
        }
    }
}
