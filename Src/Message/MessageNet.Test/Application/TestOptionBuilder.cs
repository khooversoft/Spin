using MessageNet.sdk.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Application;
using Toolbox.Azure.Queue;
using Toolbox.Tools;

namespace MessageNet.Test.Application
{
    public class TestOptionBuilder : OptionBuilder<TestOption>
    {
        public TestOptionBuilder() =>
            SetFinalize(FinalizeOption)
            .SetConfigStream(GetResourceStream)
            .SetArgs("Environment=dev");

        private TestOption FinalizeOption(TestOption option, RunEnvironment runEnvironment)
        {
            option.Verify();

            option = option with
            {
                RunEnvironment = runEnvironment,

                Nodes = option.Nodes
                    .Select(x => new MessageNodeOption
                    {
                        EndpointId = x.EndpointId,
                        BusQueue = new QueueOption
                        {
                            Namespace = option.BusNamespace.BusNamespace,
                            QueueName = x.BusQueue.QueueName,
                            AccessKey = option.BusNamespace.AccessKey,
                            KeyName = option.BusNamespace.KeyName,
                        }
                    }).ToArray(),
            };

            return option;
        }

        private Stream GetResourceStream(RunEnvironment runEnvironment) =>
            runEnvironment.GetResourceStream(typeof(TestOptionBuilder), "MessageNet.Test.Application.Configs");
    }
}
