using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace UsageRateTool
{
    class MarkdownTemplate
    {
        public static void Print(IEnumerable<API> apis)
        {
            var apiProperty = typeof(API).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(p => Attribute.IsDefined(p, typeof(PrintableAttribute)))
                .OrderBy(p => (p?.GetCustomAttribute(typeof(PrintableAttribute), false) as PrintableAttribute)?.Order);
            Dictionary<string, int> maxSize = new Dictionary<string, int>();
            foreach (var property in apiProperty)
            {
                maxSize[property.Name] = property.Name.Length;
            }

            foreach (var api in apis)
            {
                foreach (var property in apiProperty)
                {
                    object v = property.GetValue(api);
                    string vstr = v == null ? "" : v.ToString();
                    if (maxSize[property.Name] < vstr.Length)
                    {
                        maxSize[property.Name] = vstr.Length;
                    }
                }
            }

            var types = apis.Where(m => m.Category == Category.Type);
            foreach (var type in types)
            {
                Console.WriteLine();
                Console.WriteLine($"## {type.Name}");
                Console.WriteLine();

                var members = apis.Where(m => m.Parent == type);

                if (members.Count() == 0)
                {
                    Console.WriteLine("There are no newly defined members in this type.");
                    continue;
                }

                Console.Write("|");
                foreach (var property in apiProperty)
                {
                    Console.Write(String.Format($" {{0, -{maxSize[property.Name]}}} |", property.Name));
                }
                Console.WriteLine();
                Console.Write("|");
                foreach (var property in apiProperty)
                {
                    Console.Write(String.Format($" {{0, -{maxSize[property.Name]}}} |", "".PadLeft(maxSize[property.Name], '-')));
                }
                Console.WriteLine();

                foreach (var m in members)
                {
                    Console.Write("|");
                    foreach (var property in apiProperty)
                    {
                        var v = property.GetValue(m);
                        string vstr = v == null ? "" : v.ToString();
                        Console.Write(String.Format($" {{0, -{maxSize[property.Name]}}} |", vstr));
                    }
                    Console.WriteLine();
                }
            }

            Console.WriteLine("----");

            Console.WriteLine();
            Console.WriteLine("## API Cross references");
            Console.WriteLine();

            foreach (var api in apis)
            {
                if (api.Caller.Count() < 1)
                {
                    continue;
                }
                Console.WriteLine($"### {api.DeclaredType}.{api.Name}");

                foreach (var reference in api.Caller)
                {
                    Console.WriteLine($" - {reference.DeclaringType.Namespace}.{reference.DeclaringType.Name} / {reference}");
                }

                Console.WriteLine();
            }

            Console.WriteLine();
            Console.WriteLine("----");
            Console.WriteLine();
            Console.WriteLine("This document was created at https://github.com/idkiller/UsageRateTool.git");
            Console.WriteLine();
        }
    }
}
