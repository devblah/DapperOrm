using System.Collections.Generic;
using DevBlah.SqlExpressionBuilder;

namespace DevBlah.DapperOrm.Query
{
    public abstract class QueryBase
    {
        private IList<int> _ids = new List<int>();

        public IList<int> Ids
        {
            get { return _ids; }
            set { _ids = value; }
        }

        public delegate void AfterSqlBuilderInitializedEventHandler(ISqlExpressionBuilder dbBuilder);

        public event AfterSqlBuilderInitializedEventHandler AfterSqlBuilderInitialized;

        public void RaiseAfterSqlBuilderInitialized(ISqlExpressionBuilder dbBuilder)
        {
            if (AfterSqlBuilderInitialized != null)
            {
                AfterSqlBuilderInitialized(dbBuilder);
            }
        }

        public string WhereLogicalConnectionString { get; set; }
    }
}