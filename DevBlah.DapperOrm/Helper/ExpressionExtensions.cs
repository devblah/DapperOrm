using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DevBlah.DapperOrm.Helper
{
    internal static class ExpressionExtensions
    {
        /// <summary>
        /// returns the property name including the object path
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="exp"></param>
        /// <returns></returns>
        public static string GetFullPropertyName<T, TProperty>(this Expression<Func<T, TProperty>> exp)
        {
            MemberExpression memberExp;
            if (!TryFindMemberExpression(exp.Body, out memberExp))
                return string.Empty;

            var memberNames = new Stack<string>();
            do
            {
                memberNames.Push(memberExp.Member.Name);
            }
            while (TryFindMemberExpression(memberExp.Expression, out memberExp));

            return string.Join(".", memberNames.ToArray());
        }

        /// <summary>
        /// tries to find the member expression regardless of conversions
        /// </summary>
        /// <param name="exp"></param>
        /// <param name="memberExp"></param>
        /// <returns></returns>
        private static bool TryFindMemberExpression(Expression exp, out MemberExpression memberExp)
        {
            memberExp = exp as MemberExpression;
            if (memberExp != null)
            {
                return true;
            }

            // if the compiler created an automatic conversion,
            // it'll look something like...
            // obj => Convert(obj.Property) [e.g., int -> object]
            // OR:
            // obj => ConvertChecked(obj.Property) [e.g., int -> long]
            // ...which are the cases checked in IsConversion
            if (IsConversion(exp) && exp is UnaryExpression)
            {
                memberExp = ((UnaryExpression)exp).Operand as MemberExpression;
                if (memberExp != null)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if the current expression is a convert expression
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        private static bool IsConversion(Expression exp)
        {
            return (
                exp.NodeType == ExpressionType.Convert ||
                exp.NodeType == ExpressionType.ConvertChecked
            );
        }
    }
}
