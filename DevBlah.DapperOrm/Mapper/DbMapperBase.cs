using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DapperExtensions;
using DevBlah.DapperOrm.Helper;
using DevBlah.DapperOrm.Helper.Attributes;
using DevBlah.DapperOrm.Query;
using DevBlah.SqlExpressionBuilder;

namespace DevBlah.DapperOrm.Mapper
{
    public abstract class DbMapperBase
    {
        static DbMapperBase()
        {
            DapperExtensions.DapperExtensions.DefaultMapper = typeof(DapperExtensionClassMapper<>);
        }

        protected DbMapperBase(string connectionString)
        {
            ConnectionString = connectionString;
        }

        protected string ConnectionString { get; private set; }
    }

    public abstract class DbMapperBase<TDbConnection, TSqlBuilder, TQuery, TEntity> : DbMapperBase
        where TDbConnection : IDbConnection
        where TSqlBuilder : ISqlExpressionBuilder, new()
        where TQuery : QueryBase
        where TEntity : class
    {
        private int _maxChunkSize = 2000;

        protected DbMapperBase(string connectionString)
            : base(connectionString)
        {
            _DetermineTable();
        }

        /// <summary>
        /// specifies the threshold, where the query has to be chunked into multiple requests
        /// </summary>
        public int MaxChunkSize
        {
            get { return _maxChunkSize; }
            set { _maxChunkSize = value; }
        }

        public Table Table { get; set; }

        protected string ChunkOnPath { get; private set; }

        public virtual bool Delete(TEntity entity)
        {
            using (var connection = CreateConnection(ConnectionString))
            {
                return connection.Delete(entity);
            }
        }

        protected virtual void ExtendSqlBuilder(ISqlExpressionBuilder builder, Table table, TQuery query)
        {
            builder.Select<TEntity>(table);
        }

        public virtual IEnumerable<TEntity> GetResults(TQuery query)
        {
            return GetResults(query, Query);
        }

        public virtual Task<IEnumerable<TEntity>> GetResultsAsync(TQuery query)
        {
            return GetResultsAsync(query, QueryAsync);
        }

        public virtual TEntity GetSingleResult(TQuery query)
        {
            TSqlBuilder builder = BuildSelect(query);

            IEnumerable<TEntity> results;
            using (var connection = CreateConnection(ConnectionString))
            {
                results = Query(connection, builder);
            }

            return results.FirstOrDefault();
        }

        public virtual async Task<TEntity> GetSingleResultAsync(TQuery query)
        {
            TSqlBuilder builder = BuildSelect(query);

            IEnumerable<TEntity> results;
            using (var connection = CreateConnection(ConnectionString))
            {
                results = await QueryAsync(connection, builder);
            }

            return results.FirstOrDefault();
        }

        public virtual dynamic Insert(TEntity entity)
        {
            using (var connection = CreateConnection(ConnectionString))
            {
                return connection.Insert(entity);
            }
        }

        public virtual void Insert(IEnumerable<TEntity> entities)
        {
            using (var connection = CreateConnection(ConnectionString))
            {
                connection.Insert(entities);
            }
        }

        public virtual IEnumerable<TEntity> Query(TDbConnection connection, TSqlBuilder builder)
        {
            return connection.Query<TEntity>(builder.ToString(), builder.Parameters);
        }

        public virtual async Task<IEnumerable<TEntity>> QueryAsync(TDbConnection connection, TSqlBuilder builder)
        {
            return await connection.QueryAsync<TEntity>(builder.ToString(), builder.Parameters);
        }

        public void SetChunkOn(Expression<Func<TQuery,
            IEnumerable<int>>> expression)
        {
            ChunkOnPath = expression.GetFullPropertyName();
        }

        public virtual void Update(TEntity entity)
        {
            using (var connection = CreateConnection(ConnectionString))
            {
                connection.Update(entity);
            }
        }

        public virtual void Update(IEnumerable<TEntity> entities)
        {
            using (var connection = CreateConnection(ConnectionString))
            {
                connection.Update(entities);
            }
        }

        /// <summary>
        /// Creates the query out of the SqlBuilder object
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        protected virtual TSqlBuilder BuildSelect(TQuery query)
        {
            var builder = new TSqlBuilder();
            builder.From(Table);
            ExtendSqlBuilder(builder, Table, query);
            return builder;
        }

        protected abstract TDbConnection CreateConnection(string connectionString);

        protected IEnumerable<TOtherEntity> GetResults<TOtherEntity>(TQuery query,
                    Func<TDbConnection, TSqlBuilder, IEnumerable<TOtherEntity>> queryDelegate)
        {
            if (ChunkOnPath != null)
            {
                Type queryType = typeof(TQuery);
                // TODO generic IEnumerable
                IEnumerable<int> chunkableList;
                if (queryType.TryGetValueFromPropertyPath(ChunkOnPath, out chunkableList, query)
                    && chunkableList.Count() > MaxChunkSize)
                {
                    IEnumerable<IEnumerable<int>> chunks = chunkableList.Chunk(MaxChunkSize);

                    var resultCollection = new ConcurrentBag<IEnumerable<TOtherEntity>>();
                    Parallel.ForEach(chunks, chunk =>
                        resultCollection.Add(_ProcessChunk(query, chunk, queryDelegate)));

                    return resultCollection.Aggregate((t1, t2) => t1.Union(t2));
                }
            }

            return GetResultsUnchunked(query, queryDelegate);
        }

        /// <summary>
        /// does all initialisations and queries the db with the given function
        /// </summary>
        /// <typeparam name="TOtherEntity"></typeparam>
        /// <param name="query"></param>
        /// <param name="queryDelegate"></param>
        /// <returns></returns>
        protected async Task<IEnumerable<TOtherEntity>> GetResultsAsync<TOtherEntity>(TQuery query,
            Func<TDbConnection, TSqlBuilder, Task<IEnumerable<TOtherEntity>>> queryDelegate)
        {
            if (ChunkOnPath != null)
            {
                Type queryType = typeof(TQuery);
                // TODO generic IEnumerable
                IEnumerable<int> chunkableList;
                if (queryType.TryGetValueFromPropertyPath(ChunkOnPath, out chunkableList, query)
                    && chunkableList.Count() > MaxChunkSize)
                {
                    IEnumerable<IEnumerable<int>> chunks = chunkableList.Chunk(MaxChunkSize);
                    IEnumerable<Task<IEnumerable<TOtherEntity>>> tasks =
                        chunks.Select(x => _ProcessChunkAsync(query, x, queryDelegate)).ToArray();
                    return (await Task.WhenAll(tasks)).Aggregate((t1, t2) => t1.Union(t2));
                }
            }

            return await GetResultsUnchunkedAsync(query, queryDelegate);
        }

        /// <summary>
        /// Creates and initiates the query for a single request
        /// </summary>
        /// <typeparam name="TOtherEntity"></typeparam>
        /// <param name="query"></param>
        /// <param name="queryDelegate"></param>
        /// <returns></returns>
        protected virtual IEnumerable<TOtherEntity> GetResultsUnchunked<TOtherEntity>(TQuery query,
            Func<TDbConnection, TSqlBuilder, IEnumerable<TOtherEntity>> queryDelegate)
        {
            TSqlBuilder builder = BuildSelect(query);

            query.RaiseAfterSqlBuilderInitialized(builder);

            IEnumerable<TOtherEntity> results;
            using (var connection = CreateConnection(ConnectionString))
            {
                results = queryDelegate(connection, builder);
            }

            return results;
        }

        /// <summary>
        /// Creates and initiates the query for a single request
        /// </summary>
        /// <param name="query"></param>
        /// <param name="queryDelegate"></param>
        /// <returns></returns>
        protected virtual async Task<IEnumerable<TOtherEntity>> GetResultsUnchunkedAsync<TOtherEntity>(TQuery query,
            Func<TDbConnection, TSqlBuilder, Task<IEnumerable<TOtherEntity>>> queryDelegate)
        {
            TSqlBuilder builder = BuildSelect(query);

            query.RaiseAfterSqlBuilderInitialized(builder);

            IEnumerable<TOtherEntity> results;
            using (var connection = CreateConnection(ConnectionString))
            {
                results = await queryDelegate(connection, builder);
            }

            return results;
        }

        /// <summary>
        /// creates the table object for the sql builder from the table attribute marking the entity
        /// </summary>
        private void _DetermineTable()
        {
            var resultType = typeof(TEntity);

            var tableAttribute = Attribute.GetCustomAttribute(resultType, typeof(TableAttribute)) as TableAttribute;
            if (tableAttribute == null)
            {
                throw new ArgumentException(string.Format(
                    "The result type '{0}' does not implement the TableAttribute", resultType.FullName));
            }

            Table = new Table(tableAttribute.Name, tableAttribute.Alias);
        }

        /// <summary>
        /// processes a single chunk
        /// </summary>
        /// <typeparam name="TOtherEntity"></typeparam>
        /// <param name="query"></param>
        /// <param name="chunk"></param>
        /// <param name="queryDelegate"></param>
        /// <returns></returns>
        private IEnumerable<TOtherEntity> _ProcessChunk<TOtherEntity>(TQuery query, IEnumerable<int> chunk,
            Func<TDbConnection, TSqlBuilder, IEnumerable<TOtherEntity>> queryDelegate)
        {
            var newQuery = query.Clone(true, null);
            newQuery.GetType().TrySetValueFromPropertyPath(ChunkOnPath, newQuery, chunk.ToList());
            return GetResultsUnchunked(newQuery, queryDelegate);
        }

        /// <summary>
        /// processes a single chunk
        /// </summary>
        /// <typeparam name="TOtherEntity"></typeparam>
        /// <param name="query"></param>
        /// <param name="chunk"></param>
        /// <param name="queryDelegate"></param>
        /// <returns></returns>
        private async Task<IEnumerable<TOtherEntity>> _ProcessChunkAsync<TOtherEntity>(TQuery query, IEnumerable<int> chunk,
            Func<TDbConnection, TSqlBuilder, Task<IEnumerable<TOtherEntity>>> queryDelegate)
        {
            var newQuery = query.Clone(true, null);
            newQuery.GetType().TrySetValueFromPropertyPath(ChunkOnPath, newQuery, chunk.ToList());
            return await GetResultsUnchunkedAsync(newQuery, queryDelegate);
        }
    }
}