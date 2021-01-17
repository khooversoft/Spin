using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Toolbox.Tools;
using Xunit;

namespace Toolbox.Test.Tools
{
    [Trait("Category", "Actor")]
    public class DelayActionTest
    {
        [Fact]
        public void GivenDelayAction_WhenSingleAction_ShouldReceive()
        {
            bool testFlag = false;
            var delayAction = new DelayAction(TimeSpan.FromSeconds(1));

            delayAction.Post(() => testFlag = true);

            Thread.Sleep(TimeSpan.FromMilliseconds(200));
            testFlag.Should().BeFalse();

            Thread.Sleep(TimeSpan.FromMilliseconds(200));
            testFlag.Should().BeFalse();

            Thread.Sleep(TimeSpan.FromMilliseconds(200));
            testFlag.Should().BeFalse();

            Thread.Sleep(TimeSpan.FromMilliseconds(200));
            testFlag.Should().BeFalse();

            Thread.Sleep(TimeSpan.FromMilliseconds(200));
            testFlag.Should().BeTrue();

            testFlag = false;
            Thread.Sleep(TimeSpan.FromSeconds(2));
            testFlag.Should().BeFalse();
        }

        [Fact]
        public void GivenDelayAction_WhenMultipleAction_ShouldReceive()
        {
            int testValue = 0;
            var delayAction = new DelayAction(TimeSpan.FromSeconds(1));

            delayAction.Post(() => testValue = 1);

            Thread.Sleep(TimeSpan.FromMilliseconds(400));
            testValue.Should().Be(0);

            delayAction.Post(() => testValue = 2);

            Thread.Sleep(TimeSpan.FromMilliseconds(400));
            testValue.Should().Be(0);

            delayAction.Post(() => testValue = 3);

            Thread.Sleep(TimeSpan.FromMilliseconds(1500));
            testValue.Should().Be(3);
        }
    }
}
