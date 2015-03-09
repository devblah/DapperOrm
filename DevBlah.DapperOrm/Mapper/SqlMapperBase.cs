using System.Data.SqlClient;
using DevBlah.DapperOrm.Query;
using DevBlah.SqlExpressionBuilder;

namespace DevBlah.DapperOrm.Mapper
{
    public abstract class SqlMapperBase<TQuery, TEntity>
        : DbMapperBase<SqlConnection, SqlExpressionBuilderSelect, TQuery, TEntity>
        where TQuery : QueryBase
        where TEntity : class
    {
        protected SqlMapperBase(string connectionString)
            : base(connectionString)
        { }

        protected override SqlConnection CreateConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }
    }
}