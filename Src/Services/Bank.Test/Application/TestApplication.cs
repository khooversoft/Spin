using Directory.sdk.Client;
using Directory.sdk.Model;
using Directory.sdk.Tools;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Spin.Common.Model;
using Spin.Common.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Application;
using Toolbox.Azure.Queue;
using Toolbox.Extensions;

namespace Bank.Test.Application;

internal static class TestApplication
{
    private static ApiHost?[] _hosts = new ApiHost?[2];
    private static object _lock = new object();

    public static ApiHost GetHost(BankName bank)
    {
        lock (_lock)
        {
            string hostName = bank switch
            {
                BankName.First => "Bank-First",
                BankName.Second => "Bank-Second",

                _ => throw new ArgumentException($"Unknown bank={bank}")
            };

            return _hosts[(int)bank] ??= new ApiHost(hostName);
        }
    }

    public static async Task ResetQueues()
    {
        if (_hosts.Any(x => x != null)) return;

        var queueOptions = await DirectoryTools.GetDirectoryOption(@"d:\SpinDisk", RunEnvironment.Dev)
            .Run<IReadOnlyList<QueueOption>>(async client =>
            {
                IReadOnlyList<BankServiceRecord> bankServiceRecords = await client.GetBankServiceRecords(RunEnvironment.Dev);

                var list = new List<QueueOption>();
                foreach (var bank in bankServiceRecords)
                {
                    QueueOption queueOption = await client.GetQueueOption(RunEnvironment.Dev, bank.QueueId);
                    list.Add(queueOption);
                }

                return list;
            });

        foreach (var queue in queueOptions)
        {
            QueueAdmin admin = new QueueAdmin(queue, new NullLogger<QueueAdmin>());
            await admin.DeleteIfExist(queue.QueueName);

            var definition = new QueueDefinition
            {
                QueueName = queue.QueueName,
            };

            await admin.CreateIfNotExist(definition);

            var getDefinition = await admin.GetDefinition(queue.QueueName);
            getDefinition.Should().NotBeNull();
            getDefinition.QueueName.Should().Be(queue.QueueName);
        }
    }
}
