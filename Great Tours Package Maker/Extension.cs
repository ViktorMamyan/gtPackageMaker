using System.Collections.Generic;
using System.Text;

namespace Great_Tours_Package_Maker
{
    public static class Extension
    {
        public static string ForEachToString<T>(this IEnumerable<T> q)
        {
            StringBuilder sb = new StringBuilder();
            foreach (T value in q)
            {
                sb.Append(value);
            }
            return sb.ToString();
        }

    }
}