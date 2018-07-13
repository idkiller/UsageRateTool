using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace UsageRateTool
{
    class OptionParser
    {
        IEnumerable<string> _sources;
        IEnumerable<string> _targets;
        bool _isbase;

        const string SourceOption = "-s";
        const string TargetOption = "-t";
        const string BaseOption = "-b";
        const string HelpOption = "-h";

        string option = null;

        public OptionParser(string[] args)
        {
            List<string> sources = new List<string>();
            List<string> targets = new List<string>();
            List<string> others = new List<string>();
            bool isbase = false;

            foreach (var arg in args)
            {
                if (arg == SourceOption)
                {
                    option = SourceOption;
                }
                else if (arg == TargetOption)
                {
                    option = TargetOption;
                }
                else if (arg == BaseOption)
                {
                    isbase = true;
                }
                else if (arg == HelpOption)
                {
                    return;
                }
                else
                {
                    if (option == SourceOption)
                    {
                        sources.Add(arg);
                    }
                    else if (option == TargetOption)
                    {
                        targets.Add(arg);
                    }
                    else
                    {
                        others.Add(arg);
                    }
                }
            }

            if (sources.Count == 0)
            {
                sources.AddRange(others);
            }

            _sources = sources;
            _targets = targets;
            _isbase = isbase;
        }

        public void Help()
        {
            var name = this.GetType().Assembly.GetName().Name;
            Console.WriteLine($"Usage: {name} <source dll>");
            Console.WriteLine($"Usage: {name} [{SourceOption}] <source dlls> [{TargetOption}] <target dlls>");
            Console.WriteLine($"Usage: {name} [{BaseOption}] [{SourceOption}] <source dlls> [{TargetOption}] <target dlls>");
            Console.WriteLine($"Usage: {name} [{HelpOption}]");
        }

        public IEnumerable<string> Sources => _sources;
        public IEnumerable<string> Targets => _targets;
        public bool IsBase => _isbase;
    }
}
