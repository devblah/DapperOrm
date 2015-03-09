using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DevBlah.DapperOrm.Helper.Attributes;

namespace DevBlah.DapperOrm.Helper
{
    public class LogicalConnectionStringBuilder
    {
        private enum ConnectionType
        {
            And,
            Or
        }

        private List<Tuple<ConnectionType, string>> _items = new List<Tuple<ConnectionType, string>>();

        public LogicalConnectionStringBuilder And<TEntity>(Expression<Func<TEntity, object>> expression)
        {
            _items.Add(new Tuple<ConnectionType, string>(ConnectionType.And, GetMemberNameOfExpression(expression)));

            return this;
        }

        public LogicalConnectionStringBuilder And(LogicalConnectionStringBuilder connection)
        {
            _items.Add(new Tuple<ConnectionType, string>(ConnectionType.And, connection.ToString()));

            return this;
        }

        public LogicalConnectionStringBuilder Or<TEntity>(Expression<Func<TEntity, object>> expression)
        {
            _items.Add(new Tuple<ConnectionType, string>(ConnectionType.Or, GetMemberNameOfExpression(expression)));

            return this;
        }

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

            MemberExpression member = (expression.Body.NodeType == ExpressionType.Convert) ?
               (MemberExpression)((UnaryExpression)expression.Body).Operand :
               (MemberExpression)expression.Body;

            return string.Join(string.Empty, new[] { "@", tableAttribute.Alias, member.Member.Name });
        }

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