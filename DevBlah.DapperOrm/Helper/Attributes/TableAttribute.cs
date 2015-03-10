using System;

namespace DevBlah.DapperOrm.Helper.Attributes
{
    /// <summary>
    /// Attribute to decorate the entity
    /// Specifies different properties for the mapping to the database table
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class TableAttribute : Attribute
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="name">full name of the db table</param>
        /// <param name="alias">alias used in sql query</param>
        public TableAttribute(string name, string @alias)
        {
            Alias = alias;
            Name = name;
        }

        /// <summary>
        /// full name of the database table
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// alias for the database table, which is used in the sql queries
        /// </summary>
        public string Alias { get; private set; }
    }
}