using System;

namespace DevBlah.DapperOrm.Helper.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class TableWithReferencesAttribute : Attribute
    {
    }
}