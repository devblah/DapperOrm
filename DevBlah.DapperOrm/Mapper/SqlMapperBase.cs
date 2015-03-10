using System.Data.SqlClient;
using DevBlah.DapperOrm.Query;
using DevBlah.SqlExpressionBuilder;

namespace DevBlah.DapperOrm.Mapper
{
    /// <summary>
    /// Base class for Mappers using the MSSQL connection
    /// </summary>
    /// <typeparam name="TQuery">type of the query</typeparam>
    /// <typeparam name="TEntity">type of the entity</typeparam>
    public abstract class SqlMapperBase<TQuery, TEntity>
        : DbMapperBase<SqlConnection, SqlExpressionBuilderSelect, TQuery, TEntity>
        where TQuery : QueryBase
        where TEntity : class
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="connectionString">given query string</param>
        protected SqlMapperBase(string connectionString)
            : base(connectionString)
        { }

        /// <summary>
        /// Creates a new MSSQL Connection with the given query string
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        protected override SqlConnection CreateConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }
    }
}