using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace DevBlah.DapperOrm.Helper
{
    /// <summary>
    /// extension methods to call dapper with a enumerable of param
    /// </summary>
    public static class DbConnectionExtensions
    {
        /// <summary>
        /// Executes a query, returning the data typed as per T
        /// </summary>
        /// <remarks>the dynamic param may seem a bit odd, but this works around a major usability issue in vs, if it 
        /// is Object vs completion gets annoying. Eg type new [space] get new object</remarks>
        /// <returns>A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the 
        /// data from the first column in assumed, otherwise an instance is
        /// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>">
        public static IEnumerable<T> Query<T>(this IDbConnection cnn, string sql,
            IEnumerable<IDbDataParameter> param, IDbTransaction transaction = null, bool buffered = true,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            return cnn.Query<T>(sql, _GetDbArgs(param), transaction, buffered, commandTimeout,
                commandType);
        }

        /// <summary>
        /// Maps a query to objects
        /// </summary>
        /// <typeparam name="TFirst">The first type in the record set</typeparam>
        /// <typeparam name="TSecond">The second type in the record set</typeparam>
        /// <typeparam name="TReturn">The return type</typeparam>
        /// <param name="cnn"></param>
        /// <param name="sql"></param>
        /// <param name="map"></param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <param name="buffered"></param>
        /// <param name="splitOn">The Field we should split and read the second object from (default: id)</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns></returns>
        public static IEnumerable<TReturn> Query<TFirst, TSecond, TReturn>(this IDbConnection cnn,
            string sql, Func<TFirst, TSecond, TReturn> map, IEnumerable<IDbDataParameter> param,
            IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id",
            int? commandTimeout = null, CommandType? commandType = null)
        {
            return cnn.Query(sql, map, _GetDbArgs(param), transaction, buffered, splitOn,
                commandTimeout, commandType);
        }

        /// <summary>
        /// Maps a query to objects
        /// </summary>
        /// <typeparam name="TFirst"></typeparam>
        /// <typeparam name="TSecond"></typeparam>
        /// <typeparam name="TThird"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="cnn"></param>
        /// <param name="sql"></param>
        /// <param name="map"></param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <param name="buffered"></param>
        /// <param name="splitOn">The Field we should split and read the second object from (default: id)</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TReturn>(this IDbConnection cnn,
            string sql, Func<TFirst, TSecond, TThird, TReturn> map, IEnumerable<IDbDataParameter> param,
            IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id",
            int? commandTimeout = null, CommandType? commandType = null)
        {
            return cnn.Query(sql, map, _GetDbArgs(param), transaction, buffered, splitOn,
                commandTimeout, commandType);
        }

        /// <summary>
        /// Perform a multi mapping query with 4 input parameters
        /// </summary>
        /// <typeparam name="TFirst"></typeparam>
        /// <typeparam name="TSecond"></typeparam>
        /// <typeparam name="TThird"></typeparam>
        /// <typeparam name="TFourth"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="cnn"></param>
        /// <param name="sql"></param>
        /// <param name="map"></param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <param name="buffered"></param>
        /// <param name="splitOn"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TReturn>(
            this IDbConnection cnn, string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map,
            IEnumerable<IDbDataParameter> param, IDbTransaction transaction = null, bool buffered = true,
            string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            return cnn.Query(sql, map, _GetDbArgs(param), transaction, buffered, splitOn,
                commandTimeout, commandType);
        }

        /// <summary>
        /// Perform a multi mapping query with 5 input parameters
        /// </summary>
        /// <typeparam name="TFirst"></typeparam>
        /// <typeparam name="TSecond"></typeparam>
        /// <typeparam name="TThird"></typeparam>
        /// <typeparam name="TFourth"></typeparam>
        /// <typeparam name="TFifth"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="cnn"></param>
        /// <param name="sql"></param>
        /// <param name="map"></param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <param name="buffered"></param>
        /// <param name="splitOn"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(
            this IDbConnection cnn, string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map,
            IEnumerable<IDbDataParameter> param, IDbTransaction transaction = null, bool buffered = true,
            string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            return cnn.Query(sql, map, _GetDbArgs(param), transaction, buffered, splitOn,
                commandTimeout, commandType);
        }

        /// <summary>
        /// Perform a multi mapping query with 6 input parameters
        /// </summary>
        /// <typeparam name="TFirst"></typeparam>
        /// <typeparam name="TSecond"></typeparam>
        /// <typeparam name="TThird"></typeparam>
        /// <typeparam name="TFourth"></typeparam>
        /// <typeparam name="TFifth"></typeparam>
        /// <typeparam name="TSixth"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="cnn"></param>
        /// <param name="sql"></param>
        /// <param name="map"></param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <param name="buffered"></param>
        /// <param name="splitOn"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(
            this IDbConnection cnn, string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map,
            IEnumerable<IDbDataParameter> param, IDbTransaction transaction = null, bool buffered = true,
            string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            return cnn.Query(sql, map, _GetDbArgs(param), transaction, buffered, splitOn,
                commandTimeout, commandType);
        }

        /// <summary>
        /// Perform a multi mapping query with 7 input parameters
        /// </summary>
        /// <typeparam name="TFirst"></typeparam>
        /// <typeparam name="TSecond"></typeparam>
        /// <typeparam name="TThird"></typeparam>
        /// <typeparam name="TFourth"></typeparam>
        /// <typeparam name="TFifth"></typeparam>
        /// <typeparam name="TSixth"></typeparam>
        /// <typeparam name="TSeventh"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="cnn"></param>
        /// <param name="sql"></param>
        /// <param name="map"></param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <param name="buffered"></param>
        /// <param name="splitOn"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(
            this IDbConnection cnn, string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map,
            IEnumerable<IDbDataParameter> param, IDbTransaction transaction = null, bool buffered = true,
            string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            return cnn.Query(sql, map, _GetDbArgs(param), transaction, buffered, splitOn,
                commandTimeout, commandType);
        }

        /// <summary>
        /// Execute a query asynchronously using .NET 4.5 Task.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cnn"></param>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<T>> QueryAsync<T>(this IDbConnection cnn, string sql,
            IEnumerable<IDbDataParameter> param, IDbTransaction transaction = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return await cnn.QueryAsync<T>(sql, _GetDbArgs(param), transaction, commandTimeout, commandType);
        }

        /// <summary>
        /// Maps a query to objects
        /// </summary>
        /// <typeparam name="TFirst">The first type in the recordset</typeparam>
        /// <typeparam name="TSecond">The second type in the recordset</typeparam>
        /// <typeparam name="TReturn">The return type</typeparam>
        /// <param name="cnn"></param>
        /// <param name="sql"></param>
        /// <param name="map"></param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <param name="buffered"></param>
        /// <param name="splitOn">The field we should split and read the second object from (default: id)</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns></returns>
        public static async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(this IDbConnection cnn,
            string sql, Func<TFirst, TSecond, TReturn> map, IEnumerable<IDbDataParameter> param,
            IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return await cnn.QueryAsync(sql, map, _GetDbArgs(param), transaction, buffered, splitOn,
                commandTimeout, commandType);
        }

        /// <summary>
        /// Maps a query to objects
        /// </summary>
        /// <typeparam name="TFirst"></typeparam>
        /// <typeparam name="TSecond"></typeparam>
        /// <typeparam name="TThird"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="cnn"></param>
        /// <param name="sql"></param>
        /// <param name="map"></param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <param name="buffered"></param>
        /// <param name="splitOn">The Field we should split and read the second object from (default: id)</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TReturn>(this IDbConnection cnn,
            string sql, Func<TFirst, TSecond, TThird, TReturn> map, IEnumerable<IDbDataParameter> param,
            IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return await cnn.QueryAsync(sql, map, _GetDbArgs(param), transaction, buffered, splitOn,
                commandTimeout, commandType);
        }

        /// <summary>
        /// Perform a multi mapping query with 4 input parameters
        /// </summary>
        /// <typeparam name="TFirst"></typeparam>
        /// <typeparam name="TSecond"></typeparam>
        /// <typeparam name="TThird"></typeparam>
        /// <typeparam name="TFourth"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="cnn"></param>
        /// <param name="sql"></param>
        /// <param name="map"></param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <param name="buffered"></param>
        /// <param name="splitOn"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(
            this IDbConnection cnn, string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map,
            IEnumerable<IDbDataParameter> param, IDbTransaction transaction = null, bool buffered = true,
            string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            return await cnn.QueryAsync(sql, map, _GetDbArgs(param), transaction, buffered, splitOn,
                commandTimeout, commandType);
        }

        /// <summary>
        /// Perform a multi mapping query with 5 input parameters
        /// </summary>
        public static async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(
            this IDbConnection cnn, string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map,
            IEnumerable<IDbDataParameter> param, IDbTransaction transaction = null, bool buffered = true,
            string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            return await cnn.QueryAsync(sql, map, _GetDbArgs(param), transaction, buffered, splitOn,
                commandTimeout, commandType);
        }

        /// <summary>
        /// Perform a multi mapping query with 6 input parameters
        /// </summary>
        public static async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(
            this IDbConnection cnn, string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map,
            IEnumerable<IDbDataParameter> param, IDbTransaction transaction = null, bool buffered = true,
            string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            return await cnn.QueryAsync(sql, map, _GetDbArgs(param), transaction, buffered, splitOn,
                commandTimeout, commandType);
        }

        /// <summary>
        /// Perform a multi mapping query with 7 input parameters
        /// </summary>
        public static async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(
            this IDbConnection cnn, string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map,
            IEnumerable<IDbDataParameter> param, IDbTransaction transaction = null, bool buffered = true,
            string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            return await cnn.QueryAsync(sql, map, _GetDbArgs(param), transaction, buffered, splitOn,
                commandTimeout, commandType);
        }

        /// <summary>
        /// queries a single line
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cnn"></param>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static T QuerySingle<T>(this IDbConnection cnn, string sql, IEnumerable<IDbDataParameter> param)
        {
            IEnumerable<T> bufferedList = cnn.Query<T>(sql, _GetDbArgs(param), buffered: false);
            return bufferedList.FirstOrDefault();
        }

        private static DynamicParameters _GetDbArgs(IEnumerable<IDbDataParameter> param)
        {
            var dbArgs = new DynamicParameters();
            foreach (var dbDataParameter in param)
            {
                dbArgs.Add(dbDataParameter.ParameterName, dbDataParameter.Value);
            }
            return dbArgs;
        }
    }
}