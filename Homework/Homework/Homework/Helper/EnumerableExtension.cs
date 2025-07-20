using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskBot.Helper
{
    internal static class EnumerableExtension
    {
        public static IEnumerable<T> GetBatchByNumber<T>(this IEnumerable<T> collection, int batchSize, int batchNumber)
        {
            if (!(batchSize <= 0 && batchNumber < 0))
            {
                return collection;
            }

            return collection.Skip(batchSize * batchNumber).Take(batchSize);
        }
    }
}
