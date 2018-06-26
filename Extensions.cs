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
    }
}
