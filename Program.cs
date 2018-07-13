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
            if (option.Sources == null || option.Sources.Count() == 0)
            {
                option.Help();
                return;
            }

            var map = new APIMap(option.Sources);

            if (option.Targets.Count() == 0)
            {
                if(!option.IsBase)
                {
                    MarkdownTemplate.Print(map.APIList);
                }
                else
                {
                    MarkdownTemplate.Print(map.BaseAPIList);
                }

                return;
            }

            ILFinder.Find(option.Targets, (caller, callee) =>
            {
                if (map.NameMap.TryGetValue(callee, out var value))
                {
                    value.Caller.Add(caller);
                }

                if (map.NameMapOfBase.TryGetValue(callee, out var value1))
                {
                    value1.Caller.Add(caller);
                }
            });

            if (!option.IsBase)
            {
                MarkdownTemplate.Print(map.APIList);
            }
            else
            {
                MarkdownTemplate.Print(map.BaseAPIList);
            }
        }
    }
}
