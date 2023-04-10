using FluentAssertions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Azure.Queue;
using Toolbox.Tools;
using ToolBox.Azure.Test.Application;
using Xunit;

namespace ToolBox.Azure.Test.Queue;

public class QueueAdminTests
{
    [Fact]
    public async Task FullLifeCycleForQueue_ShouldPass()
    {
        const string queueName = "test-full-cycle";
        QueueOption queueOption = TestHost.Default.GetQueueOption();

        QueueAdmin admin = new QueueAdmin(queueOption, TestHost.Default.GetLoggerFactory().CreateLogger<QueueAdmin>());

        bool exist = await admin.Exist(queueName);
        if (exist) await admin.Delete(queueName);

        var definition = new QueueDefinition
        {
            QueueName = queueName,
        };

        var createdDefinition = await admin.Create(definition);
        createdDefinition.Should().NotBeNull();

        var getDefinition = await admin.GetDefinition(queueName);
        getDefinition.Should().NotBeNull();
        getDefinition.QueueName.Should().Be(queueName);

        exist = await admin.Exist(queueName);
        exist.Should().BeTrue();
        await admin.Delete(queueName);
    }
}
