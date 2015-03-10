using System;

namespace DevBlah.DapperOrm.Helper.Attributes
{
    /// <summary>
    /// Specifies an entity, which has references to other entites
    /// Used to start the resolution of the join queries
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class TableWithReferencesAttribute : Attribute
    {
    }
}