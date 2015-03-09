using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DevBlah.DapperOrm.Helper.Attributes;
using DevBlah.DapperOrm.Query;
using DevBlah.SqlExpressionBuilder;

namespace DevBlah.DapperOrm.Mapper
{
    public abstract class TableWithReferencesMapperBase<TQuery, TResult> : SqlMapperBase<TQuery, TResult>
        where TQuery : QueryBase
        where TResult : class
    {
        private List<ReferenceTable> _referenceTables = new List<ReferenceTable>();

        protected TableWithReferencesMapperBase(string connectionString)
            : base(connectionString)
        {
            var resultType = typeof(TResult);
            _DetermineTable(resultType);
        }

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

        protected virtual void SqlBuilderSelectByType(ISqlExpressionBuilder builder, Table table, Type type, TQuery query)
        {
        }

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