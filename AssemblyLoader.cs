using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace UsageRateTool
{
    class AssemblyLoader
    {

        static AssemblyLoader()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += MyResolveEventHandler;
        }

        public static Assembly GetAssemblyByName(AssemblyName name)
        {
            string cwd = Directory.GetCurrentDirectory();
            return FindDll(cwd, name.FullName);
        }

        public static Assembly GetAssembly(string path)
        {
            string cwd = Directory.GetCurrentDirectory();
            string source = Path.Combine(cwd, path);
            try
            {
                return Assembly.LoadFrom(source);
            }
            catch (ReflectionTypeLoadException ex)
            {
                StringBuilder sb = new StringBuilder();
                foreach (Exception exSub in ex.LoaderExceptions)
                {
                    sb.AppendLine(exSub.Message);
                    FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
                    if (exFileNotFound != null)
                    {
                        if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                        {
                            sb.AppendLine("Fusion Log:");
                            sb.AppendLine(exFileNotFound.FusionLog);
                        }
                    }
                    sb.AppendLine();
                }
                string errorMessage = sb.ToString();
                Console.WriteLine(errorMessage);
            }
            return null;
        }

        static Assembly MyResolveEventHandler(object sender, ResolveEventArgs args)
        {
            var cwd = Directory.GetCurrentDirectory();
            return FindDll(cwd, args.Name);
        }

        static Assembly FindDll(string dir, string fullName)
        {
            var files = Directory.GetFiles(dir, "*.dll", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                var asm = Assembly.LoadFile(file);
                if (fullName == asm.FullName)
                {
                    return asm;
                }
            }

            return null;
        }
    }
}
