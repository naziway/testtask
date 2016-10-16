using System;
using System.Collections.Generic;

namespace Heathmill.FixAT.Utilities
{
    public static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key,
            TValue defaultValue)
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : defaultValue;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key,
            Func<TValue> defaultValueProvider)
        {
            TValue value;
            return dictionary.TryGetValue(key, out value)
                       ? value
                       : defaultValueProvider();
        }

        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
                                                       TKey key)
            where TValue : new()
        {
            return GetOrCreate(dictionary, key, () => new TValue());
        }

        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
                                                       TKey key,
                                                       Func<TValue> createNewValue)
        {
            TValue ret;
            if (!dictionary.TryGetValue(key, out ret))
            {
                ret = createNewValue();
                dictionary[key] = ret;
            }
            return ret;
        }
    }
}
