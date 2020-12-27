using System.Collections.Generic;

namespace Assets.Scripts.Utilities
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<K> TryPullFromDictionary<T, K>(this IEnumerable<T> self, IDictionary<T, K> dictionary)
        {
            foreach (var item in self)
            {
                if (dictionary.TryGetValue(item, out var value))
                {
                    yield return value;
                }
            }
        }
    }
}
