using MessageNet.sdk.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Toolbox.Application;
using Toolbox.Azure.Queue;

namespace MessageNet.Application
{
    public class OptionBuilder : OptionBuilder<Option>
    {
        public OptionBuilder()
        {
            SetFinalize(FinalizeOption);
            SetConfigStream(GetResourceStream);
            SetArgs("Environment=dev");
        }

        private Option FinalizeOption(Option option, RunEnvironment runEnvironment)
        {
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

            option.Verify();

            return option;
        }

        protected virtual Stream GetResourceStream(RunEnvironment runEnvironment) => runEnvironment.GetResourceStream(typeof(OptionBuilder), "MessageNet.Configs");
    }
}
