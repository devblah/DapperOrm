using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DevBlah.DapperOrm.Helper.Attributes;
using DevBlah.DapperOrm.Query;
using DevBlah.SqlExpressionBuilder;

namespace DevBlah.DapperOrm.Mapper
{
    /// <summary>
    /// base mapper class for mapping entities, which has several subentities included
    /// </summary>
    /// <typeparam name="TQuery">type of the query</typeparam>
    /// <typeparam name="TResult">type of the entity</typeparam>
    public abstract class TableWithReferencesMapperBase<TQuery, TResult> : SqlMapperBase<TQuery, TResult>
        where TQuery : QueryBase
        where TResult : class
    {
        /// <summary>
        /// list of referenced tables
        /// </summary>
        private List<ReferenceTable> _referenceTables = new List<ReferenceTable>();

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="connectionString"></param>
        protected TableWithReferencesMapperBase(string connectionString)
            : base(connectionString)
        {
            var resultType = typeof(TResult);
            _DetermineTable(resultType);
        }

        /// <summary>
        /// Builds the select string for the entity and all references
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        protected override SqlExpressionBuilderSelect BuildSelect(TQuery query)
        {
            var type = typeof(TResult);
            ReferenceTable first = _referenceTables.First();
            var builder = new SqlExpressionBuilderSelect();
            builder.From(first.Table);
            SqlBuilderSelectByType(builder, first.Table, type, query);

            foreach (ReferenceTable referenceTable in _referenceTables)
            {
                string onClause = string.Format("{0}.{1} = {2}.{3}", referenceTable.Table.Alias, referenceTable.ColumnName,
                    referenceTable.TargetTable.Alias, referenceTable.TargetColumnName);
                builder.JoinLeft(referenceTable.TargetTable, onClause);
                SqlBuilderSelectByType(builder, referenceTable.TargetTable, referenceTable.PropertyInfo.PropertyType,
                    query);
            }

            return builder;
        }

        /// <summary>
        /// Method to create the select string according to the used types of the current entity and all references.
        /// Is called while the sql query is builded.
        /// </summary>
        /// <param name="builder">current instance of the sql expression builder</param>
        /// <param name="table">table where selectable fields should be determined</param>
        /// <param name="type">type of the entity where selectable fields should be determined</param>
        /// <param name="query">type of the dependant query</param>
        protected virtual void SqlBuilderSelectByType(ISqlExpressionBuilder builder, Table table, Type type,
            TQuery query)
        { }

        /// <summary>
        /// gets the references out of the decorating attributes
        /// </summary>
        /// <param name="entityType">type of the entity</param>
        /// <param name="table">table which belongs to the current entity</param>
        private void _DetermineReferences(Type entityType, Table table)
        {
            IEnumerable<PropertyInfo> propertyInfos = entityType.GetProperties()
                .Where(x => x.CustomAttributes.Any(y => y.AttributeType == typeof(ReferencedEntityAttribute)));

            foreach (PropertyInfo info in propertyInfos)
            {
                var referenceAttribute = (ReferencedEntityAttribute)Attribute.GetCustomAttribute(
                    info, typeof(ReferencedEntityAttribute));
                var tableAttribute =
                    Attribute.GetCustomAttribute(info.PropertyType, typeof(TableAttribute)) as TableAttribute;

                if (tableAttribute == null)
                {
                    throw new ArgumentException(string.Format(
                        "The referenced type '{0}' does not implement the TableAttribute",
                        info.PropertyType.FullName));
                }

                var reference = new ReferenceTable
                {
                    PropertyInfo = info,
                    Table = table,
                    ColumnName = referenceAttribute.ColumnName,
                    TargetTable = new Table(tableAttribute.Name, tableAttribute.Alias),
                    TargetColumnName = referenceAttribute.TargetColumnName
                };

                _referenceTables.Add(reference);

                _DetermineReferences(info.PropertyType, reference.TargetTable);
            }
        }

        /// <summary>
        /// Gets the table from the TableWithReferencesAttribute decorating the current entitiy
        /// </summary>
        /// <param name="entityType"></param>
        private void _DetermineTable(Type entityType)
        {
            if (entityType.CustomAttributes.All(x => x.AttributeType != typeof(TableWithReferencesAttribute)))
            {
                throw new ArgumentException(string.Format(
                    "The result type '{0}' does not implement the TableWithReferencesAttribute", entityType.FullName));
            }

            var tableAttribute = Attribute.GetCustomAttribute(entityType, typeof(TableAttribute)) as TableAttribute;
            if (tableAttribute == null)
            {
                throw new ArgumentException(string.Format(
                    "The result type '{0}' does not implement the TableAttribute", entityType.FullName));
            }

            _DetermineReferences(entityType, new Table(tableAttribute.Name, tableAttribute.Alias));
        }

        /// <summary>
        /// Class, which represents all attributes of a reference
        /// Only used internally
        /// </summary>
        internal class ReferenceTable
        {
            public Table Table { get; set; }

            public string ColumnName { get; set; }

            public PropertyInfo PropertyInfo { get; set; }

            public Table TargetTable { get; set; }

            public string TargetColumnName { get; set; }
        }
    }
}