using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Toolbox.Tools
{
    public class MovingAverage
    {
        private readonly int _mask;
        private readonly double?[] _values;
        private int _nextIndex = -1;

        public MovingAverage(int windowSize)
        {
            _mask = windowSize - 1;
            if (windowSize == 0 || (windowSize & _mask) != 0)
            {
                throw new ArgumentException("Must be power of two", nameof(windowSize));
            }

            _values = new double?[windowSize];
        }

        public void Add(double newValue)
        {
            var index = Interlocked.Increment(ref _nextIndex) & _mask;
            _values[index] = newValue;
        }

        public double ComputeAverage()
        {
            return _values.TakeWhile(x => x.HasValue)
                .Select(x => x ?? 0)
                .DefaultIfEmpty(0)
                .Average();
        }
    }
}
