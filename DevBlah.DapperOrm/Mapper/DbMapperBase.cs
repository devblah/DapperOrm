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
    /// <summary>
    /// base class for mappers of the DapperOrm
    /// </summary>
    public abstract class DbMapperBase
    {
        /// <summary>
        /// creates the class mapper for the dapper extensions
        /// </summary>
        static DbMapperBase()
        {
            DapperExtensions.DapperExtensions.DefaultMapper = typeof(DapperExtensionClassMapper<>);
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="connectionString">connection string to your database</param>
        protected DbMapperBase(string connectionString)
        {
            ConnectionString = connectionString;
        }

        /// <summary>
        /// connection string to your database
        /// </summary>
        protected string ConnectionString { get; private set; }
    }

    /// <summary>
    /// templated base class to use any kind of specific db implementations
    /// </summary>
    /// <typeparam name="TDbConnection">type of db connection, must inherit IDbConnection</typeparam>
    /// <typeparam name="TSqlBuilder">type of sql expression builder, must inherit ISqlExpressionBuilder</typeparam>
    /// <typeparam name="TQuery">type of the query</typeparam>
    /// <typeparam name="TEntity">type of the entity</typeparam>
    public abstract class DbMapperBase<TDbConnection, TSqlBuilder, TQuery, TEntity> : DbMapperBase
        where TDbConnection : IDbConnection
        where TSqlBuilder : ISqlExpressionBuilder, new()
        where TQuery : QueryBase
        where TEntity : class
    {
        /// <summary>
        /// Default value for single chunk size for chunked query
        /// </summary>
        private int _maxChunkSize = 2000;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="connectionString"></param>
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

        /// <summary>
        /// Representation of the current db table
        /// </summary>
        public Table Table { get; set; }

        /// <summary>
        /// string to the property in the query, which should be split into chunks
        /// </summary>
        protected string ChunkOnPath { get; private set; }

        /// <summary>
        /// deletes an entity
        /// </summary>
        /// <param name="entity">Entity which should be deleted from the database</param>
        /// <returns>boolean, which specifies if the deleting process was successful</returns>
        public virtual bool Delete(TEntity entity)
        {
            using (var connection = CreateConnection(ConnectionString))
            {
                return connection.Delete(entity);
            }
        }

        /// <summary>
        /// overridable method, where all sql expression builder operations needed for the current entity take place
        /// </summary>
        /// <param name="builder">current instance of the sql expression builder</param>
        /// <param name="table">current represenation of the database table</param>
        /// <param name="query">current query object</param>
        public virtual void ExtendSqlBuilder(ISqlExpressionBuilder builder, Table table, TQuery query)
        {
            builder.Select<TEntity>(table);
        }

        /// <summary>
        /// returns the resultset for the given query
        /// </summary>
        /// <param name="query"></param>
        /// <returns>resultset</returns>
        public virtual IEnumerable<TEntity> GetResults(TQuery query)
        {
            return GetResults(query, Query);
        }

        /// <summary>
        /// returns the resultset for the given query asynchronously
        /// </summary>
        /// <param name="query"></param>
        /// <returns>resultset</returns>
        public virtual Task<IEnumerable<TEntity>> GetResultsAsync(TQuery query)
        {
            return GetResultsAsync(query, QueryAsync);
        }

        /// <summary>
        /// returns a single resultset for the given query
        /// </summary>
        /// <param name="query"></param>
        /// <returns>resultset</returns>
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

        /// <summary>
        /// returns a single resultset for the given query asynchronously
        /// </summary>
        /// <param name="query"></param>
        /// <returns>resultset</returns>
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

        /// <summary>
        /// inserts the given entity into the table
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual dynamic Insert(TEntity entity)
        {
            using (var connection = CreateConnection(ConnectionString))
            {
                return connection.Insert(entity);
            }
        }

        /// <summary>
        /// inserts a set of entities into the table
        /// </summary>
        /// <param name="entities"></param>
        public virtual void Insert(IEnumerable<TEntity> entities)
        {
            using (var connection = CreateConnection(ConnectionString))
            {
                connection.Insert(entities);
            }
        }

        /// <summary>
        /// the core query process
        /// </summary>
        /// <param name="connection">current instance of the db connection</param>
        /// <param name="builder">the current instance of the sql expression builder</param>
        /// <returns>resultset</returns>
        public virtual IEnumerable<TEntity> Query(TDbConnection connection, TSqlBuilder builder)
        {
            return connection.Query<TEntity>(builder.ToString(), builder.Parameters);
        }

        /// <summary>
        /// the asynchronous core query process
        /// </summary>
        /// <param name="connection">current instance of the db connection</param>
        /// <param name="builder">the current instance of the sql expression builder</param>
        /// <returns>resultset</returns>
        public virtual async Task<IEnumerable<TEntity>> QueryAsync(TDbConnection connection, TSqlBuilder builder)
        {
            return await connection.QueryAsync<TEntity>(builder.ToString(), builder.Parameters);
        }

        /// <summary>
        /// sets the object path to the query member, which should be chunked on exceeding the maximum chunk size
        /// </summary>
        /// <param name="expression"></param>
        public void SetChunkOn(Expression<Func<TQuery,
            IEnumerable<int>>> expression)
        {
            ChunkOnPath = expression.GetFullPropertyName();
        }

        /// <summary>
        /// updates a single entity in the database
        /// </summary>
        /// <param name="entity"></param>
        public virtual void Update(TEntity entity)
        {
            using (var connection = CreateConnection(ConnectionString))
            {
                connection.Update(entity);
            }
        }

        /// <summary>
        ///  updates a set of entities in the database
        /// </summary>
        /// <param name="entities"></param>
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

        /// <summary>
        /// abstract method which creates the specific db connection from the given connection string
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        protected abstract TDbConnection CreateConnection(string connectionString);
        /// <summary>
        /// does all initialisations and queries the db with the given function
        /// </summary>
        /// <typeparam name="TOtherEntity">type of the result entity (normally the same like TEntity)</typeparam>
        /// <param name="query">query object for the current request</param>
        /// <param name="queryDelegate">function, where the actual db querying takes place</param>
        /// <returns></returns>
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
        /// <typeparam name="TOtherEntity">type of the result entity (normally the same like TEntity)</typeparam>
        /// <param name="query">query object for the current request</param>
        /// <param name="queryDelegate">function, where the actual db querying takes place</param>
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