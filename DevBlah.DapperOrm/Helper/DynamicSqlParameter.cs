using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;

namespace DevBlah.DapperOrm.Helper
{
    internal class DynamicSqlParameter : DynamicObject
    {
        private readonly IEnumerable<SqlParameter> _parameters;
        private readonly Dictionary<string, object> _dictionary;

        public DynamicSqlParameter(IEnumerable<SqlParameter> parameters)
        {
            _parameters = parameters;
            _dictionary = _ConvertToDictionary();
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return _dictionary.TryGetValue("@" + binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _dictionary["@" + binder.Name] = value;

            return true;
        }

        private Dictionary<string, object> _ConvertToDictionary()
        {
            var collection = new Dictionary<string, object>();
            foreach (var sqlParameter in _parameters)
            {
                collection.Add(sqlParameter.ParameterName, sqlParameter.Value);
            }
            return collection;
        }
    }
}