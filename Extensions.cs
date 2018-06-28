using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace UsageRateTool
{
    static class Extensions
    {
        public static string GetFullName(this MemberInfo mi)
        {
            return $"{mi?.DeclaringType?.Namespace}.{mi.DeclaringType?.Name}.{mi.Name}";
        }

        public static string ToByteString(this byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var b in bytes)
            {
                sb.Append($"{b:x}:");
            }
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }

        public static MethodBase ResolveMethod(this IEnumerable<Module> modules, int metadataToken)
        {
            foreach (var module in modules)
            {
                try
                {
                    return module.ResolveMethod(metadataToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            return null;
        }
    }
}
