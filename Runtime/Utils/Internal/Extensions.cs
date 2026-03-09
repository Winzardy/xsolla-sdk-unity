using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Xsolla.SDK.Utils
{
    internal static class ArrayExtensions
    {
        public static T1[] Map<T, T1>(this T[] collection, Func<T, T1> mapper)
        {
            var result = new T1[collection.Length];
            
            for (var i = 0; i < collection.Length; i++) 
                result[i] = mapper(collection[i]);
            
            return result;
        }
        
        public static bool Find<T>(this T[] collection, Func<T, bool> finder)
        {
            for (var i = 0; i < collection.Length; i++)
            {
                if (finder(collection[i]))
                    return true;
            }
            
            return false;
        }
        
        public static bool FindFirst<T>(this T[] collection, Func<T, bool> finder, out T valueOut)
        {
            for (var i = 0; i < collection.Length; i++)
            {
                var item = collection[i];
                if (finder(item))
                {
                    valueOut = item;
                    return true;
                }
            }
            
            valueOut = default;
            return false;
        }
        
        public static T1[] MapWithFilter<T, T1>(this T[] collection, Func<T, bool> filter, Func<T, T1> mapper)
        {
            var result = new List<T1>();

            foreach (var item in collection)
            {
                if (filter(item))
                    result.Add(mapper(item));
            }

            return result.ToArray();
        }
    }

    internal static class EnumerableExtensions
    {
        public static List<T> ToList<T>(this IEnumerable<T> collection) => new List<T>(collection);
    }

    internal static class CollectionExtensions
    {
        public static T1[] MapToArray<T, T1>(this ReadOnlyCollection<T> collection, Func<T, T1> mapper)
        {
            var result = new T1[collection.Count];
            
            for (var i = 0; i < collection.Count; i++) 
                result[i] = mapper(collection[i]);

            return result;
        }
    }

    internal static class TaskExtensions
    {
        public static async Task<T1> Map<T, T1>(this Task<T> collection, Func<T, T1> mapper)
        {
            var result = await collection;
            return mapper(result);
        }
        
        public static async Task<T> MapE<T>(this Task collection, Func<T> mapper)
        {
            await collection;
            return mapper();
        }
    }
}