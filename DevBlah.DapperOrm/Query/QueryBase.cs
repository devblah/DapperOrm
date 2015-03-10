using System.Collections.Generic;
using DevBlah.SqlExpressionBuilder;

namespace DevBlah.DapperOrm.Query
{
    /// <summary>
    /// Base class for the queries
    /// </summary>
    public abstract class QueryBase
    {
        private IList<int> _ids = new List<int>();

        /// <summary>
        /// Filtering by Ids
        /// </summary>
        public IList<int> Ids
        {
            get { return _ids; }
            set { _ids = value; }
        }

        /// <summary>
        /// delegate for the AfterSqlBuilderInitialized event
        /// </summary>
        /// <param name="dbBuilder">current instance of the sql connection string builder</param>
        public delegate void AfterSqlBuilderInitializedEventHandler(ISqlExpressionBuilder dbBuilder);

        /// <summary>
        /// Raised before the query string is built out of the connection string builder
        /// </summary>
        public event AfterSqlBuilderInitializedEventHandler AfterSqlBuilderInitialized;

        /// <summary>
        /// Raises the "AfterSqlBuilderInitialized" event
        /// </summary>
        /// <param name="dbBuilder">current instance of the sql connection string builder</param>
        public void RaiseAfterSqlBuilderInitialized(ISqlExpressionBuilder dbBuilder)
        {
            if (AfterSqlBuilderInitialized != null)
            {
                AfterSqlBuilderInitialized(dbBuilder);
            }
        }

        /// <summary>
        /// String which specifies the logical connection in the where clause
        /// </summary>
        public string WhereLogicalConnectionString { get; set; }
    }
}