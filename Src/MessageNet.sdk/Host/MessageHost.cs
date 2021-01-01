using MessageNet.sdk.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace MessageNet.sdk.Host
{
    public class MessageHost
    {
        private readonly MessageOption _messageOption;
        private readonly ILoggerFactory _loggerFactory;
        private readonly MessageAwaiterService _awaiterCollection = new MessageAwaiterService();

        public MessageHost(MessageOption messageOption, ILoggerFactory loggerFactory)
        {
            messageOption.VerifyNotNull(nameof(messageOption));
            loggerFactory.VerifyNotNull(nameof(loggerFactory));

            _messageOption = messageOption;
            _loggerFactory = loggerFactory;
        }


    }
}
