using System;
using System.Reflection;

namespace DevBlah.DapperOrm.Helper
{
    internal static class ReflectionExtensions
    {
        public static object GetValueFromPropertyPath(this Type originalType, object obj, string propertyPath)
        {
            string[] path = propertyPath.Split('.');

            object result = obj;

            foreach (string pathStep in path)
            {
                Type type = result.GetType();
                PropertyInfo property = type.GetProperty(pathStep);
                if (property == null)
                {
                    return null;
                }

                result = property.GetValue(result, null);
            }

            return result;
        }

        public static bool TryGetValueFromPropertyPath<T>(this Type originalType, string propertyPath, out T res,
            object obj) where T : class
        {
            string[] path = propertyPath.Split('.');

            object result = obj;

            foreach (string pathStep in path)
            {
                Type type = result.GetType();
                PropertyInfo property = type.GetProperty(pathStep);
                if (property == null)
                {
                    res = default(T);
                    return false;
                }

                result = property.GetValue(result, null);
            }

            res = result as T;

            return res != null;
        }

        public static bool TrySetValueFromPropertyPath(this Type originalType, string propertyPath, object obj,
            object value)
        {
            string[] path = propertyPath.Split('.');

            object destination = obj;

            for (int i = 0; i < path.Length; i++)
            {
                string pathStep = path[i];
                Type type = destination.GetType();
                PropertyInfo property = type.GetProperty(pathStep);
                if (property == null)
                {
                    return false;
                }

                if (i < path.Length - 1)
                {
                    destination = property.GetValue(obj, null);
                }
                else
                {
                    destination.GetType().GetProperty(path[i]).SetValue(destination, value, null);
                }
            }

            return true;
        }
    }
}
