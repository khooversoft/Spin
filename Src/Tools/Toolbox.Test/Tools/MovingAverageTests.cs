using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;
using Xunit;

namespace Toolbox.Test.Tools
{
    public class MovingAverageTests
    {
        [Fact]
        public void Should_compute_average()
        {
            var sut = new MovingAverage(4);

            sut.ComputeAverage().Should().Be(0);
            sut.Add(2);
            sut.ComputeAverage().Should().Be(2);
            sut.Add(4);
            sut.ComputeAverage().Should().Be(3);
            sut.Add(0);
            sut.ComputeAverage().Should().Be(2);
            sut.Add(6);
            sut.ComputeAverage().Should().Be(3);
            sut.Add(6);
            sut.ComputeAverage().Should().Be(4);
            sut.Add(0);
            sut.Add(0);
            sut.Add(0);
            sut.Add(0);
            sut.ComputeAverage().Should().Be(0);
            sut.Add(10);
            sut.Add(10);
            sut.Add(10);
            sut.Add(10);
            sut.ComputeAverage().Should().Be(10);
        }
    }
}
