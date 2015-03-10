using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DevBlah.DapperOrm.Helper.Attributes;

namespace DevBlah.DapperOrm.Helper
{
    /// <summary>
    /// Helps to create an Logical Connection String to adjust the where clause in the generated query
    /// </summary>
    public class LogicalConnectionStringBuilder
    {
        /// <summary>
        /// types of connection logic
        /// </summary>
        private enum ConnectionType
        {
            And,
            Or
        }

        /// <summary>
        /// list of connection types to where clauses
        /// </summary>
        private List<Tuple<ConnectionType, string>> _items = new List<Tuple<ConnectionType, string>>();

        /// <summary>
        /// Adds an AND connection to the where clause
        /// </summary>
        /// <typeparam name="TEntity">connected entity type</typeparam>
        /// <param name="expression">name of the property</param>
        /// <returns></returns>
        public LogicalConnectionStringBuilder And<TEntity>(Expression<Func<TEntity, object>> expression)
        {
            _items.Add(new Tuple<ConnectionType, string>(ConnectionType.And, GetMemberNameOfExpression(expression)));

            return this;
        }

        /// <summary>
        /// Adds an AND connection for another bracketed logical connection string builder
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public LogicalConnectionStringBuilder And(LogicalConnectionStringBuilder connection)
        {
            _items.Add(new Tuple<ConnectionType, string>(ConnectionType.And, connection.ToString()));

            return this;
        }

        /// <summary>
        /// Adds an OR connection to the where clause
        /// </summary>
        /// <typeparam name="TEntity">connected entity type</typeparam>
        /// <param name="expression">name of the property</param>
        /// <returns></returns>
        public LogicalConnectionStringBuilder Or<TEntity>(Expression<Func<TEntity, object>> expression)
        {
            _items.Add(new Tuple<ConnectionType, string>(ConnectionType.Or, GetMemberNameOfExpression(expression)));

            return this;
        }

        /// <summary>
        /// Adds an OR connection for another bracketed logical connection string builder
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public LogicalConnectionStringBuilder Or(LogicalConnectionStringBuilder connection)
        {
            _items.Add(new Tuple<ConnectionType, string>(ConnectionType.Or, connection.ToString()));

            return this;
        }

        private string GetMemberNameOfExpression<TEntity>(Expression<Func<TEntity, object>> expression)
        {
            Type entityType = typeof(TEntity);

            var tableAttribute = entityType.GetCustomAttributes(typeof(TableAttribute), false)
                .FirstOrDefault() as TableAttribute;

            if (tableAttribute == null)
            {
                throw new Exception("The given object doesn't implement the 'TableAttribute'.");
            }

            string name = expression.GetFullPropertyName();

            return string.Join(string.Empty, new[] { "@", tableAttribute.Alias, name });
        }

        /// <summary>
        /// builds the string out of the connection string builder
        /// can be set into the sql query builder
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder("(");

            foreach (Tuple<ConnectionType, string> tuple in _items)
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (tuple == _items.First())
                {
                    sb.Append(tuple.Item2);
                }
                else
                {
                    sb.Append(" ").Append(tuple.Item1.ToString().ToUpper()).Append(" ").Append(tuple.Item2);
                }
            }

            sb.Append(")");

            return sb.ToString();
        }
    }
}