// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the MIT License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Toolbox.Tools
{
    /// <summary>
    /// Fast deferred execution using lambda
    /// </summary>
    public class Deferred<T>
    {
        private T _value = default!;
        private Func<T> _getValue;

        /// <summary>
        /// Construct with lambda to return value
        /// </summary>
        /// <param name="getValue"></param>
        public Deferred(Func<T> getValue)
        {
            _getValue = () =>
            {
                Interlocked.Exchange(ref _getValue, () => _value);
                return _value = getValue();
            };
        }

        /// <summary>
        /// Return value (lazy)
        /// </summary>
        public T Value => _getValue();
    }
}
