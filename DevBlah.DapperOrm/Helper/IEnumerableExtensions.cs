using System.Collections.Generic;
using System.Linq;

namespace DevBlah.DapperOrm.Helper
{
    // ReSharper disable once InconsistentNaming
    internal static class IEnumerableExtensions
    {
        /// <summary>
        /// Break a list of items into chunks of a specific size
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="chunksize"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunksize)
        {
            while (source.Any())
            {
                yield return source.Take(chunksize);
                source = source.Skip(chunksize);
            }
        }
    }
}
