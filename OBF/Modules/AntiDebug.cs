using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OBF.Modules
{
    internal class AntiDebug
    {
        public static void InitAntiDebug(AssemblyDefinition assembly)
        {
            foreach (var module in assembly.Modules)
                foreach (var type in module.Types)
                    foreach (var method in type.Methods)
                        if (method.HasBody)
                        {
                            CheckIsAttached(module, method);
                            CheckEnvironment(module, method);
                            CheckTiming(module, method);
                            CheckDebuggerAPI(module, method);
                        }
        }
        public static void CheckIsAttached(ModuleDefinition module, MethodDefinition method)
        {
            var ilProcessor = method.Body.GetILProcessor();
            var firstInstruction = method.Body.Instructions[0];
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Call, module.ImportReference(typeof(System.Diagnostics.Debugger).GetMethod("IsAttached"))));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Brfalse_S, firstInstruction));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Newobj, module.ImportReference(typeof(System.Exception).GetConstructor(Type.EmptyTypes))));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Throw));
        }
        public static void CheckEnvironment(ModuleDefinition module, MethodDefinition method)
        {
            var ilProcessor = method.Body.GetILProcessor();
            var firstInstruction = method.Body.Instructions[0];
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Call, module.ImportReference(typeof(System.Diagnostics.Debugger).GetMethod("IsLogging"))));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Brfalse_S, firstInstruction));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Newobj, module.ImportReference(typeof(System.Exception).GetConstructor(Type.EmptyTypes))));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Throw));
        }
        public static void CheckTiming(ModuleDefinition module, MethodDefinition method)
        {
            var ilProcessor = method.Body.GetILProcessor();
            var firstInstruction = method.Body.Instructions[0];
            var stopwatchType = module.ImportReference(typeof(System.Diagnostics.Stopwatch));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Call, stopwatchType.Resolve().Methods.First(m => m.Name == "StartNew")));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Stloc_0));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldloc_0));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Call, stopwatchType.Resolve().Methods.First(m => m.Name == "Stop")));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldloc_0));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Call, stopwatchType.Resolve().Methods.First(m => m.Name == "get_ElapsedMilliseconds")));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldc_I4, 100)); // expected time in ms
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ble_S, firstInstruction));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Newobj, module.ImportReference(typeof(System.Exception).GetConstructor(Type.EmptyTypes))));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Throw));
        }
        [DllImport("kernel32.dll")]
        private static extern bool IsDebuggerPresent();
        public static void CheckDebuggerAPI(ModuleDefinition module, MethodDefinition method)
        {
            var ilProcessor = method.Body.GetILProcessor();
            var firstInstruction = method.Body.Instructions[0];
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Call, module.ImportReference(typeof(AntiDebug).GetMethod("IsDebuggerPresent", BindingFlags.NonPublic | BindingFlags.Static))));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Brfalse_S, firstInstruction));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Newobj, module.ImportReference(typeof(System.Exception).GetConstructor(Type.EmptyTypes))));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Throw));
        }
    }
}
