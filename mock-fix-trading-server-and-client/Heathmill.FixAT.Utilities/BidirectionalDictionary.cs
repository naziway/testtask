using System;
using System.Collections.Generic;

namespace Heathmill.FixAT.Utilities
{
    /// <summary>
    /// Implements a bidirectional dictionary that supports lookup in both directions
    /// </summary>
    /// <remarks>
    /// http://stackoverflow.com/questions/255341/getting-key-of-value-of-a-generic-dictionary#255630
    /// </remarks>
    public class BidirectionalDictionary<TFirst, TSecond>
    {
        readonly IDictionary<TFirst, TSecond> _firstToSecond = new Dictionary<TFirst, TSecond>();
        readonly IDictionary<TSecond, TFirst> _secondToFirst = new Dictionary<TSecond, TFirst>();

        public void Add(TFirst first, TSecond second)
        {
            if (_firstToSecond.ContainsKey(first) ||
                _secondToFirst.ContainsKey(second))
            {
                throw new ArgumentException("Duplicate first or second");
            }
            _firstToSecond.Add(first, second);
            _secondToFirst.Add(second, first);
        }

        public void RemoveByFirst(TFirst first)
        {
            TSecond second;
            if (TryGetByFirst(first, out second))
            {
                _firstToSecond.Remove(first);
                _secondToFirst.Remove(second);   
            }
        }

        public bool TryGetByFirst(TFirst first, out TSecond second)
        {
            return _firstToSecond.TryGetValue(first, out second);
        }

        public TSecond GetByFirst(TFirst first)
        {
            return _firstToSecond[first];
        }

        public bool TryGetBySecond(TSecond second, out TFirst first)
        {
            return _secondToFirst.TryGetValue(second, out first);
        }

        public TFirst GetBySecond(TSecond second)
        {
            return _secondToFirst[second];
        }
    }
}
