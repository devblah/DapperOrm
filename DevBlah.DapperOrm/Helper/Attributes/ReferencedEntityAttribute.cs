using System;

namespace DevBlah.DapperOrm.Helper.Attributes
{
    /// <summary>
    /// Attribute to specify a referenced entity (1:1)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ReferencedEntityAttribute : Attribute
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="columnName">foreign key column name</param>
        /// <param name="targetColumnName">primary key column name of target entity</param>
        public ReferencedEntityAttribute(string columnName, string targetColumnName = "Id")
        {
            ColumnName = columnName;
            TargetColumnName = targetColumnName;
        }

        /// <summary>
        /// the name of the column which represents the foreign key
        /// </summary>
        public string ColumnName { get; private set; }

        /// <summary>
        /// the name of referenced column in the target table (mostly the primary key)
        /// </summary>
        public string TargetColumnName { get; set; }
    }
}