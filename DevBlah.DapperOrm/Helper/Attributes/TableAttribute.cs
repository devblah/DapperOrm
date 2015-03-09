using System;

namespace DevBlah.DapperOrm.Helper.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class TableAttribute : Attribute
    {
        public TableAttribute(string name, string @alias)
        {
            Alias = alias;
            Name = name;
        }

        public string Name { get; private set; }

        public string Alias { get; private set; }
    }
}