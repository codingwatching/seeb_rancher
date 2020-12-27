using System;
using System.Collections.Generic;
using System.Linq;


namespace Assets.Scripts.Utilities
{
    public static class LinqExtensions
    {
        public static IEnumerable<T[]> Buffer<T>(this IEnumerable<T> source, int bufferSize)
        {
            if(bufferSize <= 0)
            {
                while (true)
                {
                    yield return new T[0];
                }
            }

            var iterator = source.GetEnumerator();

            int position;
            while(true)
            {
                var workingList = new T[bufferSize];
                for (position = 0; position < bufferSize && iterator.MoveNext(); position++)
                {
                    var value = iterator.Current;
                    workingList[position] = value;
                }
                if(position < bufferSize)
                {
                    yield break;
                }
                yield return workingList;
            }
        }

        public static IEnumerable<IList<T>> RollingWindow<T>(this IEnumerable<T> source, int window)
        {
            var iterator = source.GetEnumerator();

            var position = 0;
            var workingList = new List<T>(window);
            while (position < window && iterator.MoveNext())
            {
                var value = iterator.Current;
                workingList.Add(value);
                position++;
            }
            if (position < window)
            {
                yield break;
            }

            yield return workingList.ToList();
            while (iterator.MoveNext())
            {
                var value = iterator.Current;
                workingList.RemoveAt(0);
                workingList.Add(value);
                yield return workingList.ToList();
            }
        }
        public static IDictionary<T, float> SumTogether<T>(this IEnumerable<IDictionary<T, float>> source)
        {
            var result = new Dictionary<T, float>();
            var iterator = source.GetEnumerator();

            while (iterator.MoveNext())
            {
                var value = iterator.Current;
                foreach (var key in value.Keys)
                {
                    if (!result.ContainsKey(key))
                    {
                        result[key] = 0f;
                    }
                    result[key] += value[key];
                }
            }

            return result;
        }

        public static IDictionary<TKey, TOut> SelectDictionary<TKey, TIn, TOut>(this IDictionary<TKey, TIn> source, Func<TIn, TOut> valueSelector)
        {
            return source.ToDictionary(x => x.Key, x => valueSelector(x.Value));
        }

        public static IDictionary<TKey, float> Normalize<TKey>(this IDictionary<TKey, float> source)
        {
            var sum = source.Values.Sum();
            return source.SelectDictionary(f => f / sum);
        }
    }

}