using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace UsageRateTool
{
    delegate void FoundDelegater(MethodBase caller, MemberInfo callee);
    class ILFinder
    {
        static IEnumerable<OpCode> opCodes;
        static ILFinder()
        {
            opCodes = typeof(OpCodes)
                .GetFields()
                .Select(fi => (OpCode)fi.GetValue(null));
        }
        public static void Find(IEnumerable<string> paths, FoundDelegater onFound)
        {
            if (onFound == null) return;
            foreach (var path in paths)
            {
                var asm = AssemblyLoader.GetAssembly(path);

                var types = asm.GetTypes();

                foreach (var type in types)
                {
                    var methods = type.GetMethods(
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance |
                        BindingFlags.CreateInstance | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.InvokeMethod |
                        BindingFlags.SetField | BindingFlags.GetField | BindingFlags.FlattenHierarchy | BindingFlags.DeclaredOnly);

                    foreach (var method in methods)
                    {
                        Find(method, onFound);
                    }

                    var ctors = type.GetConstructors(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    foreach (var ctor in ctors)
                    {
                        Find(ctor, onFound);
                    }
                }
            }
        }

        static void Find(MethodBase methodInfo, FoundDelegater onFound)
        {
            var body = methodInfo.GetMethodBody();
            if (body == null) return;

            byte[] il = body.GetILAsByteArray();

            var walker = il.GetEnumerator();
            var module = methodInfo.Module;
            while (walker.MoveNext())
            {
                long b = (byte)walker.Current;

                if (b == OpCodes.Prefix1.Value)
                {
                    walker.MoveNext();
                    b |= (long)((byte)walker.Current) << 8;
                }

                var op = opCodes.FirstOrDefault(opCode => opCode.Value == b);

                int byteCount = 4;
                long operand = 0;
                Action<int> resolver = null;
                try
                {
                    switch (op.OperandType)
                    {
                        case OperandType.InlineNone:
                            byteCount = 0;
                            break;
                        case OperandType.InlineI8:
                        case OperandType.InlineR:
                            byteCount = 8;
                            break;
                        case OperandType.ShortInlineBrTarget:
                        case OperandType.ShortInlineI:
                        case OperandType.ShortInlineVar:
                            byteCount = 1;
                            break;
                        case OperandType.InlineVar:
                            byteCount = 2;
                            break;
                        case OperandType.InlineMethod:
                            resolver = md => onFound(methodInfo, module.ResolveMethod(md));
                            break;
                        case OperandType.InlineField:
                            resolver = md => onFound(methodInfo, module.ResolveField(md));
                            break;
                        case OperandType.InlineSig:
                            resolver = md => module.ResolveSignature(md);
                            break;
                        case OperandType.InlineString:
                            resolver = md => module.ResolveString(md);
                            break;
                        case OperandType.InlineType:
                            resolver = md => module.ResolveType(md);
                            break;
                    }
                    for (int i = 0; i < byteCount; i++)
                    {
                        walker.MoveNext();
                        b = (byte)walker.Current;
                        operand |= ((long)b) << (8 * i);
                    }
                    resolver?.Invoke((int)operand);
                }
                catch (Exception ex)
                {
                }
            }
        }
    }
}
