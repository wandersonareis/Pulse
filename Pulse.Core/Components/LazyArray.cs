﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Pulse.Core
{
    public sealed class LazyArray<T> : IEnumerable<KeyValuePair<int, T>>
    {
        private readonly ConcurrentDictionary<int, T> _dic = new();
        private readonly Func<int, T> _factory;

        public int Count => _dic.Count;

        public LazyArray(Func<int, T> factory)
        {
            _factory = Exceptions.CheckArgumentNull(factory, "factory");
        }

        public T this[int index] => _dic.GetOrAdd(index, _factory);

        public IEnumerator<KeyValuePair<int, T>> GetEnumerator()
        {
            return _dic.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}