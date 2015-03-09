using System;

namespace DevBlah.DapperOrm.Helper.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ReferencedEntityAttribute : Attribute
    {
        public ReferencedEntityAttribute(string columnName, string targetColumnName = "Nr")
        {
            ColumnName = columnName;
            TargetColumnName = targetColumnName;
        }

        public string ColumnName { get; private set; }

        public string TargetColumnName { get; set; }
    }
}