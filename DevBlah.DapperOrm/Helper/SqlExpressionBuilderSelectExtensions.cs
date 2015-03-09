using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DevBlah.SqlExpressionBuilder;

namespace DevBlah.DapperOrm.Helper
{
    public static class SqlExpressionBuilderSelectExtensions
    {
        public static void Select<TEntity>(this ISqlExpressionBuilder builder, Table table, bool declaredOnly = false)
        {
            Type type = typeof(TEntity);
            builder.Select(table, type, declaredOnly);
        }

        public static void Select(this ISqlExpressionBuilder builder, Table table, Type type,
            bool declaredOnly = false)
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            if (declaredOnly)
            {
                flags = flags | BindingFlags.DeclaredOnly;
            }
            PropertyInfo[] propertyInfos = type.GetProperties(flags);
            builder.Select(propertyInfos
                .Where(x => x.PropertyType.IsValueType
                    || x.PropertyType.IsPrimitive
                    || x.PropertyType == typeof(Decimal)
                    || x.PropertyType == typeof(DateTime)
                    || x.PropertyType == typeof(String)
                    || x.PropertyType == typeof(byte[]))
                .Select(x => x.Name), table);
        }

        public static void Where<TEntity>(
            this ISqlExpressionBuilder builder,
            Table table,
            Expression<Func<TEntity, object>> expression,
            object value,
            CompareOperations compare = CompareOperations.Equals)
        {
            MemberExpression member = (expression.Body.NodeType == ExpressionType.Convert) ?
                (MemberExpression)((UnaryExpression)expression.Body).Operand :
                (MemberExpression)expression.Body;

            string name = string.Join(".", new[] { table.Alias, member.Member.Name });

            builder.Where(new Compare<string, IDbDataParameter>(
                compare,
                name,
                new SqlParameter(string.Join(string.Empty, new[] { "@", table.Alias, member.Member.Name }), value)));
        }
    }
}