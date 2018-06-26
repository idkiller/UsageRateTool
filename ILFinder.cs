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
                        BindingFlags.SetField | BindingFlags.GetField | BindingFlags.FlattenHierarchy);

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

        public static void Find(MethodBase methodInfo, FoundDelegater onFound)
        {
            var body = methodInfo.GetMethodBody();
            if (body == null) return;

            byte[] il = body.GetILAsByteArray();

            var opCodes = typeof(System.Reflection.Emit.OpCodes)
                .GetFields()
                .Select(fi => (System.Reflection.Emit.OpCode)fi.GetValue(null));

            var mappedIL = il.Select(op =>
                opCodes.FirstOrDefault(opCode => opCode.Value == op));

            var ilWalker = mappedIL.GetEnumerator();
            while (ilWalker.MoveNext())
            {
                var mappedOp = ilWalker.Current;
                if (mappedOp.OperandType != OperandType.InlineNone)
                {
                    var byteCount = 4;
                    long operand = 0;
                    string token = string.Empty;

                    var module = methodInfo.Module;
                    Func<int, string> tokenResolver = tkn => string.Empty;
                    switch (mappedOp.OperandType)
                    {
                        case OperandType.InlineI8:
                        case OperandType.InlineR:
                            byteCount = 8;
                            break;
                        case OperandType.ShortInlineBrTarget:
                        case OperandType.ShortInlineI:
                        case OperandType.ShortInlineVar:
                            byteCount = 1;
                            break;
                    }
                    for (int i = 0; i < byteCount; i++)
                    {
                        ilWalker.MoveNext();
                        operand |= ((long)ilWalker.Current.Value) << (8 * i);
                    }

                    try
                    {
                        switch (mappedOp.OperandType)
                        {
                            case OperandType.InlineMethod:
                                var m = module.ResolveMethod((int)operand);
                                if (m != null)
                                    onFound(methodInfo, m);
                                break;
                            case OperandType.InlineField:
                                var f = module.ResolveField((int)operand);
                                if (f != null)
                                    onFound(methodInfo, f);
                                break;
                            case OperandType.InlineSig:
                                var sig = module.ResolveSignature((int)operand);
                                break;
                            case OperandType.InlineString:
                                var str = module.ResolveString((int)operand);
                                break;
                            case OperandType.InlineType:
                                var t = module.ResolveType((int)operand);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }
    }
}
