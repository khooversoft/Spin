using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Types
{
    public struct MaskDecimal4
    {
        private const int mask = 10000;

        public MaskDecimal4(long value) => Value = value;

        public MaskDecimal4(float value) => Value = (long)(value * mask);

        public MaskDecimal4(double value) => Value = (long)(value * mask);

        public long Value { get; }

        public double ToDouble() => (double)Value / mask;

        public static implicit operator double(MaskDecimal4 subject) => subject.ToDouble();

        public static implicit operator long(MaskDecimal4 subject) => subject.Value;

        public static implicit operator MaskDecimal4(long value) => new MaskDecimal4(value);

        public static implicit operator MaskDecimal4(float value) => new MaskDecimal4(value);

        public static explicit operator MaskDecimal4(double value) => new MaskDecimal4(value);
    }
}
