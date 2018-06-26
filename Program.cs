using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;

namespace UsageRateTool
{
    class Program
    {
        static int CountReference(byte[] needle, byte[] haystack)
        {
            int result = 0;

            return result;
        }

        static void Main(string[] args)
        {
            var option = new OptionParser(args);
            if (option.Sources.Count() == 0)
            {
                option.Help();
                return;
            }

            var map = new APIMap(option.Sources);

            if (option.Targets.Count() == 0)
            {
                MarkdownTemplate.Print(map.APIList);
                return;
            }

            ILFinder.Find(option.Targets, (caller, callee) =>
            {
                if (map.NameMap.TryGetValue(callee, out var value))
                {
                    value.Caller.Add(caller);
                }
            });

            MarkdownTemplate.Print(map.APIList);
        }
    }
}
