using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace DevBlah.DapperOrm.Helper
{
    public static class DbConnectionExtensions
    {
        public static IEnumerable<T> Query<T>(this IDbConnection connection, string sql,
            IEnumerable<IDbDataParameter> parameters)
        {
            return connection.Query<T>(sql, _GetDbArgs(parameters));
        }

        public static IEnumerable<TResult> Query<T1, T2, TResult>(this IDbConnection connection,
            string sql, Func<T1, T2, TResult> action, IEnumerable<IDbDataParameter> parameters, string splitOn = "Nr")
        {
            return connection.Query(sql, action, _GetDbArgs(parameters), splitOn: splitOn);
        }

        public static IEnumerable<TResult> Query<T1, T2, T3, TResult>(this IDbConnection connection,
            string sql, Func<T1, T2, T3, TResult> action, IEnumerable<IDbDataParameter> parameters, string splitOn = "Nr")
        {
            return connection.Query(sql, action, _GetDbArgs(parameters), splitOn: splitOn);
        }

        public static IEnumerable<TResult> Query<T1, T2, T3, T4, TResult>(
            this IDbConnection connection, string sql, Func<T1, T2, T3, T4, TResult> action,
            IEnumerable<IDbDataParameter> parameters, string splitOn = "Nr")
        {
            return connection.Query(sql, action, _GetDbArgs(parameters), splitOn: splitOn);
        }

        public static IEnumerable<TResult> Query<T1, T2, T3, T4, T5, TResult>(
            this IDbConnection connection, string sql, Func<T1, T2, T3, T4, T5, TResult> action,
            IEnumerable<IDbDataParameter> parameters, string splitOn = "Nr")
        {
            return connection.Query(sql, action, _GetDbArgs(parameters), splitOn: splitOn);
        }

        public static IEnumerable<TResult> Query<T1, T2, T3, T4, T5, T6, TResult>(
            this IDbConnection connection, string sql, Func<T1, T2, T3, T4, T5, T6, TResult> action,
            IEnumerable<IDbDataParameter> parameters, string splitOn = "Nr")
        {
            return connection.Query(sql, action, _GetDbArgs(parameters), splitOn: splitOn);
        }

        public static IEnumerable<TResult> Query<T1, T2, T3, T4, T5, T6, T7, TResult>(
            this IDbConnection connection, string sql, Func<T1, T2, T3, T4, T5, T6, T7, TResult> action,
            IEnumerable<IDbDataParameter> parameters, string splitOn = "Nr")
        {
            return connection.Query(sql, action, _GetDbArgs(parameters), splitOn: splitOn);
        }

        public static async Task<IEnumerable<T>> QueryAsync<T>(this IDbConnection connection, string sql,
            IEnumerable<IDbDataParameter> parameters)
        {
            return await connection.QueryAsync<T>(sql, _GetDbArgs(parameters));
        }

        public static async Task<IEnumerable<TResult>> QueryAsync<T1, T2, TResult>(this IDbConnection connection,
            string sql, Func<T1, T2, TResult> action, IEnumerable<IDbDataParameter> parameters, string splitOn = "Nr")
        {
            return await connection.QueryAsync(sql, action, _GetDbArgs(parameters), splitOn: splitOn);
        }

        public static async Task<IEnumerable<TResult>> QueryAsync<T1, T2, T3, TResult>(this IDbConnection connection,
            string sql, Func<T1, T2, T3, TResult> action, IEnumerable<IDbDataParameter> parameters, string splitOn = "Nr")
        {
            return await connection.QueryAsync(sql, action, _GetDbArgs(parameters), splitOn: splitOn);
        }

        public static async Task<IEnumerable<TResult>> QueryAsync<T1, T2, T3, T4, TResult>(
            this IDbConnection connection, string sql, Func<T1, T2, T3, T4, TResult> action,
            IEnumerable<IDbDataParameter> parameters, string splitOn = "Nr")
        {
            return await connection.QueryAsync(sql, action, _GetDbArgs(parameters), splitOn: splitOn);
        }

        public static async Task<IEnumerable<TResult>> QueryAsync<T1, T2, T3, T4, T5, TResult>(
            this IDbConnection connection, string sql, Func<T1, T2, T3, T4, T5, TResult> action,
            IEnumerable<IDbDataParameter> parameters, string splitOn = "Nr")
        {
            return await connection.QueryAsync(sql, action, _GetDbArgs(parameters), splitOn: splitOn);
        }

        public static async Task<IEnumerable<TResult>> QueryAsync<T1, T2, T3, T4, T5, T6, TResult>(
            this IDbConnection connection, string sql, Func<T1, T2, T3, T4, T5, T6, TResult> action,
            IEnumerable<IDbDataParameter> parameters, string splitOn = "Nr")
        {
            return await connection.QueryAsync(sql, action, _GetDbArgs(parameters), splitOn: splitOn);
        }

        public static async Task<IEnumerable<TResult>> QueryAsync<T1, T2, T3, T4, T5, T6, T7, TResult>(
            this IDbConnection connection, string sql, Func<T1, T2, T3, T4, T5, T6, T7, TResult> action,
            IEnumerable<IDbDataParameter> parameters, string splitOn = "Nr")
        {
            return await connection.QueryAsync(sql, action, _GetDbArgs(parameters), splitOn: splitOn);
        }

        public static T QuerySingle<T>(this IDbConnection connection, string sql, IEnumerable<IDbDataParameter> parameters)
        {
            IEnumerable<T> bufferedList = connection.Query<T>(sql, _GetDbArgs(parameters));
            return bufferedList.FirstOrDefault();
        }

        private static DynamicParameters _GetDbArgs(IEnumerable<IDbDataParameter> parameters)
        {
            var dbArgs = new DynamicParameters();
            foreach (var dbDataParameter in parameters)
            {
                dbArgs.Add(dbDataParameter.ParameterName, dbDataParameter.Value);
            }
            return dbArgs;
        }
    }
}