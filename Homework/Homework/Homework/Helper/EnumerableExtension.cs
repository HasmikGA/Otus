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
        public static IEnumerable<T> GetBatchByNumbe<T>(this IEnumerable<T> collection, int batchSize, int batchNumber)
        {
            return collection.Skip(batchSize * batchNumber).Take(batchSize);
            //var indexeNumbers = numbers.Select((value, index) => new { value, index });

            //var quary = (from item in indexeNumbers
            //             group item by item.index / batchSize into groupBySize
            //             where groupBySize.Key == batchNumber
            //             select groupBySize).FirstOrDefault();

            //var batchList = quary.Select(g => g.value);

            //return batchList;
        }
    }
}
