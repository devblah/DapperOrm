using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DevBlah.SqlExpressionBuilder;

namespace DevBlah.DapperOrm.Helper
{
    /// <summary>
    /// Adds strongly typed methods to the sql expression builder
    /// </summary>
    public static class SqlExpressionBuilderSelectExtensions
    {
        /// <summary>
        /// adds the complete list of properties of an entity to the select string
        /// </summary>
        /// <typeparam name="TEntity">type of the entity</typeparam>
        /// <param name="builder">current instance of the sql expression builder</param>
        /// <param name="table">dependant table</param>
        /// <param name="declaredOnly">specifies if only the members of the child entity should be selected</param>
        public static void Select<TEntity>(this ISqlExpressionBuilder builder, Table table, bool declaredOnly = false)
        {
            Type type = typeof(TEntity);
            builder.Select(table, type, declaredOnly);
        }

        /// <summary>
        /// adds the complete list of properties of an entity to the select string
        /// </summary>
        /// <param name="builder">current instance of the sql expression builder</param>
        /// <param name="table">dependant table</param>
        /// <param name="type">type of the entity</param>
        /// <param name="declaredOnly">specifies if only the members of the child entity should be selected</param>
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

        /// <summary>
        /// Adds a comparation to the sql where string
        /// </summary>
        /// <typeparam name="TEntity">type of the entity</typeparam>
        /// <param name="builder">current instance of the sql expression builder</param>
        /// <param name="table">dependant table</param>
        /// <param name="expression">expression which property of the entity should be compared</param>
        /// <param name="value">value to compare to</param>
        /// <param name="compare">operation to compare to</param>
        public static void Where<TEntity>(
            this ISqlExpressionBuilder builder,
            Table table,
            Expression<Func<TEntity, object>> expression,
            object value,
            CompareOperations compare = CompareOperations.Equals)
        {
            string memberName = expression.GetFullPropertyName();

            string name = string.Join(".", new[] { table.Alias, memberName });

            builder.Where(new Compare<string, IDbDataParameter>(
                compare,
                name,
                new SqlParameter(string.Join(string.Empty, new[] { "@", table.Alias, memberName }), value)));
        }
    }
}