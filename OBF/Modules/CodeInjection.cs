using Mono.Cecil;
using Mono.Cecil.Cil;
using OBF.Modules;
using OBF.ILProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBF.Modules
{
    internal class CodeInjection
    {
        public static void InjectMethods(AssemblyDefinition assembly)
        {
            var types = new List<TypeDefinition>(assembly.MainModule.Types);

            foreach (TypeDefinition type in types)
            {
                Generate.JunkMethods(type);
                Generate.JunkVirtualMethods(type);
                Generate.JunkVirtualMethods(type);
                Clone.RandomMethod(assembly.MainModule);
                Clone.RandomMethod(assembly.MainModule);
            }
        }

        public static void JunkWithMethodCalls(AssemblyDefinition assembly)
        {
            foreach (TypeDefinition type in assembly.MainModule.Types)
            {
                var methods = new List<MethodDefinition>(type.Methods);

                foreach (var method in methods)
                {
                    if (method.Name.StartsWith("get_") || method.Name.StartsWith("set_"))
                        continue;

                    RandomizeMethodCalls(method, type);
                }
            }
        }
        public static void RandomizeMethodCalls(MethodDefinition method, TypeDefinition type)
        {
            Random random = new();
            var actions = new List<Action>
            {
                () => AddNestedIf(method, type, true),
                () => AddNestedIf(method, type, true),
                () => AddIfOpaquePredicates(method, type, true),
                () => AddIfOpaquePredicates(method, type, true),
                () => AddSwitchWithNestedIf(method, type, true)
            };

            int numberOfActions = random.Next(1, 3);

            // Call the actions
            for (int i = 0; i < numberOfActions; i++)
            {
                // Select a random action and execute it
                var randomAction = actions[random.Next(actions.Count)];
                randomAction();
            }
        }
        public static void AddIfOpaquePredicates(MethodDefinition method, TypeDefinition type, bool calledByJunk = false)
        {
            if (!method.HasBody)
                return;

            var ilProcessor = method.Body.GetILProcessor();
            var firstInstruction = method.Body.Instructions.FirstOrDefault();
            if (firstInstruction == null)
                return;

            var label1 = ilProcessor.Create(OpCodes.Nop);
            var label2 = ilProcessor.Create(OpCodes.Nop);
            var endLabel = ilProcessor.Create(OpCodes.Nop);

            var junkMethod1 = CreateJunkMethod(method.Module, calledByJunk);

            // Define a local variable
            var intType = method.Module.TypeSystem.Int32;
            var localVariable = new VariableDefinition(intType);
            method.Body.Variables.Add(localVariable);

            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Br, label1));

            ilProcessor.InsertBefore(firstInstruction, label1);
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldc_I4, 110));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Stloc, localVariable));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldloc, localVariable));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldc_I4, 60));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Blt, label2));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Br, endLabel));

            ilProcessor.InsertBefore(firstInstruction, label2);
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Call, junkMethod1)); // Call junk method 1

            ilProcessor.InsertBefore(firstInstruction, endLabel);
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Nop)); // End of method

            Console.WriteLine($"[ControlFlow] Injected if obfuscation into method: {type.Name}.{method.Name}");
        }
        public static void AddNestedIf(MethodDefinition method, TypeDefinition type, bool calledByJunk = false)
        {
            if (!method.HasBody)
                return;

            var ilProcessor = method.Body.GetILProcessor();
            var firstInstruction = method.Body.Instructions.FirstOrDefault();
            if (firstInstruction == null)
                return;

            var label1 = ilProcessor.Create(OpCodes.Nop);
            var label2 = ilProcessor.Create(OpCodes.Nop);
            var label3 = ilProcessor.Create(OpCodes.Nop);
            var endLabel = ilProcessor.Create(OpCodes.Nop);

            var junkMethod1 = CreateJunkMethod(method.Module, calledByJunk);

            // Define a local variable
            var intType = method.Module.TypeSystem.Int32;
            var localVariable = new VariableDefinition(intType);
            method.Body.Variables.Add(localVariable);

            var caseA = Extensions.IntRng(-100, 100);
            var caseB = Extensions.IntRng(caseA, caseA + 50);
            var caseC = Extensions.IntRng(caseB, caseB + 50);


            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Br, label1));

            ilProcessor.InsertBefore(firstInstruction, label1);
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldc_I4, caseC));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Stloc, localVariable));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldloc, localVariable));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldc_I4, caseB));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Blt, label2));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Br, endLabel));

            ilProcessor.InsertBefore(firstInstruction, label2);
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldloc, localVariable));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldc_I4, caseA));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Blt, label3));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Br, endLabel));

            ilProcessor.InsertBefore(firstInstruction, label3);
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Call, junkMethod1)); // Call junk method 1
            //ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Nop)); // Placeholder for additional logic

            ilProcessor.InsertBefore(firstInstruction, endLabel);
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Nop)); // End of method

            Console.WriteLine($"[ControlFlow] Injected nested if obfuscation into method: {type.Name}.{method.Name}");
        }
        /*public static void AddNestedIfAfter(MethodDefinition method, TypeDefinition type)
        {
            if (!method.HasBody || method.ReturnType.IsValueType || method.Name.StartsWith("get_") || method.Name.StartsWith("set_"))
                return;

                var ilProcessor = method.Body.GetILProcessor();
            var lastInstruction = method.Body.Instructions.LastOrDefault();
            if (lastInstruction == null)
                return;

            var label1 = ilProcessor.Create(OpCodes.Nop);
            var label2 = ilProcessor.Create(OpCodes.Nop);
            var label3 = ilProcessor.Create(OpCodes.Nop);
            var endLabel = ilProcessor.Create(OpCodes.Nop);

            // Define a local variable
            var intType = method.Module.TypeSystem.Int32;
            var localVariable = new VariableDefinition(intType);
            method.Body.Variables.Add(localVariable);

            var caseA = Extensions.IntRng(-100, 100);
            var caseB = Extensions.IntRng(caseA, caseA + 50);
            var caseC = Extensions.IntRng(caseB, caseB + 50);

            // Insert at the end of the method
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Br, label1));

            ilProcessor.InsertBefore(lastInstruction, label1);
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Ldc_I4, caseC));
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Stloc, localVariable));
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Ldloc, localVariable));
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Ldc_I4, caseB));
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Blt, label2));
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Br, endLabel));

            ilProcessor.InsertBefore(lastInstruction, label2);
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Ldloc, localVariable));
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Ldc_I4, caseA));
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Blt, label3));
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Br, endLabel));

            ilProcessor.InsertBefore(lastInstruction, label3);
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Nop)); // Placeholder for additional logic

            ilProcessor.InsertBefore(lastInstruction, endLabel);
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Nop)); // End of method

            Console.WriteLine($"Injected nested if obfuscation into method: {type.Name}.{method.Name}");
        }*/
        public static void AddSwitchWithNestedIf(MethodDefinition method, TypeDefinition type, bool calledByJunk = false)
        {
            if (!method.HasBody)
                return;

            var ilProcessor = method.Body.GetILProcessor();
            var firstInstruction = method.Body.Instructions.FirstOrDefault();
            if (firstInstruction == null)
                return;

            var switchLabels = new Instruction[]
            {
        ilProcessor.Create(OpCodes.Nop), // Case 0
        ilProcessor.Create(OpCodes.Nop), // Case 1
        ilProcessor.Create(OpCodes.Nop), // Case 2
            };
            var endLabel = ilProcessor.Create(OpCodes.Nop);

            // Define a local variable for the switch
            var intType = method.Module.TypeSystem.Int32;
            var switchVariable = new VariableDefinition(intType);
            method.Body.Variables.Add(switchVariable);

            // Create junk methods
            var junkMethod1 = CreateJunkMethod(method.Module, calledByJunk);
            var junkMethod2 = CreateJunkMethod(method.Module, calledByJunk);
            var junkMethod3 = CreateJunkMethod(method.Module, calledByJunk);

            var switchA = Extensions.IntRng(0, 10);
            var switchB = Extensions.IntRng(3, 10);

            var caseA = Extensions.IntRng(-100, 100);
            var caseB = Extensions.IntRng(caseA, caseA + 100);
            var case1A = Extensions.IntRng(-100, 100);
            var case1B = Extensions.IntRng(caseA, caseA + 100);
            var case2A = Extensions.IntRng(-100, 100);
            var case2B = Extensions.IntRng(caseA, caseA + 100);

            // Load a value into the switch variable
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldc_I4, switchA)); // Example value
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Stloc, switchVariable));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldloc, switchVariable));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldc_I4, switchB));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Add_Ovf));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Switch, switchLabels));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Br, endLabel));

            // Case 0
            ilProcessor.InsertBefore(firstInstruction, switchLabels[0]);
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldc_I4, case1A));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Stloc, switchVariable));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldloc, switchVariable));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldc_I4, case1B));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Blt, endLabel));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Call, junkMethod1)); // Call junk method 1
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Br, endLabel));

            // Case 1
            ilProcessor.InsertBefore(firstInstruction, switchLabels[1]);
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldc_I4, case2A));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Stloc, switchVariable));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldloc, switchVariable));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldc_I4, case2B));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Blt, endLabel));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Call, junkMethod2)); // Call junk method 2
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Br, endLabel));

            // Case 2
            ilProcessor.InsertBefore(firstInstruction, switchLabels[2]);
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldc_I4, caseA));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Stloc, switchVariable));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldloc, switchVariable));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldc_I4, caseB));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Blt, endLabel));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Call, junkMethod3)); // Call junk method 3
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Br, endLabel));

            ilProcessor.InsertBefore(firstInstruction, endLabel);
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Nop)); // End of method

            Console.WriteLine($"[ControlFlow] Injected switch with nested if statements into method: {type.Name}.{method.Name}");
        }
        /*public static void AddSwitchWithNestedIfAfter(MethodDefinition method, TypeDefinition type)
        {
            if (!method.HasBody || method.ReturnType.IsValueType || method.Name.StartsWith("get_") || method.Name.StartsWith("set_"))
                return;

            var ilProcessor = method.Body.GetILProcessor();
            var lastInstruction = method.Body.Instructions.LastOrDefault();
            if (lastInstruction == null)
                return;

            var switchLabels = new Instruction[]
            {
        ilProcessor.Create(OpCodes.Nop), // Case 0
        ilProcessor.Create(OpCodes.Nop), // Case 1
        ilProcessor.Create(OpCodes.Nop), // Case 2
            };
            var endLabel = ilProcessor.Create(OpCodes.Nop);

            // Define a local variable for the switch
            var intType = method.Module.TypeSystem.Int32;
            var switchVariable = new VariableDefinition(intType);
            method.Body.Variables.Add(switchVariable);

            // Create junk methods
            var junkMethod1 = CreateJunkMethod(type, method.Module);
            var junkMethod2 = CreateJunkMethod(type, method.Module);
            var junkMethod3 = CreateJunkMethod(type, method.Module);

            var switchA = Extensions.IntRng(-10, 10);
            var switchB = Extensions.IntRng(0, 3);
            if (switchA >= 0 && switchA <= 3)
                switchB = Extensions.IntRng(3, 10);

            var caseA = Extensions.IntRng(-100, 100);
            var caseB = Extensions.IntRng(caseA, caseA + 100);
            var case1A = Extensions.IntRng(-100, 100);
            var case1B = Extensions.IntRng(caseA, caseA + 100);
            var case2A = Extensions.IntRng(-100, 100);
            var case2B = Extensions.IntRng(caseA, caseA + 100);

            // Load a value into the switch variable
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Ldc_I4, switchA)); // Example value
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Stloc, switchVariable));
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Ldloc, switchVariable));
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Ldc_I4, switchB));
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Add_Ovf));
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Switch, switchLabels));
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Br, endLabel));

            // Case 0
            ilProcessor.InsertBefore(lastInstruction, switchLabels[0]);
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Ldc_I4, case1A));
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Stloc, switchVariable));
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Ldloc, switchVariable));
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Ldc_I4, case1B));
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Blt, endLabel));
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Call, junkMethod1)); // Call junk method 1
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Br, endLabel));

            // Case 1
            ilProcessor.InsertBefore(lastInstruction, switchLabels[1]);
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Ldc_I4, case2A));
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Stloc, switchVariable));
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Ldloc, switchVariable));
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Ldc_I4, case2B));
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Blt, endLabel));
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Call, junkMethod2)); // Call junk method 2
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Br, endLabel));

            // Case 2
            ilProcessor.InsertBefore(lastInstruction, switchLabels[2]);
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Ldc_I4, caseA));
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Stloc, switchVariable));
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Ldloc, switchVariable));
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Ldc_I4, caseB));
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Blt, endLabel));
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Call, junkMethod3)); // Call junk method 3
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Br, endLabel));

            ilProcessor.InsertBefore(lastInstruction, endLabel);
            ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Nop)); // End of method

            Console.WriteLine($"Injected switch with nested if statements into method: {type.Name}.{method.Name}");
        }*/
        internal static MethodDefinition CreateJunkMethod(ModuleDefinition module, bool calledByJunk)
        {
            var existingClasses = module.Types.Where(t => t.IsClass && !t.IsEnum && !t.IsInterface && !t.IsCompilerGenerated() && !t.IsDelegate()).ToList();
            // Get a random class from the module
            var randomClass = existingClasses[Extensions.IntRng(0, existingClasses.Count)];

            var junkMethod = new MethodDefinition(Renaming.GenerateUniqueName(), MethodAttributes.Private | MethodAttributes.Static, module.TypeSystem.Void);
            randomClass.Methods.Add(junkMethod);
            var junkIlProcessor = junkMethod.Body.GetILProcessor();

            // Fill the junk method with random junk code
            var intType = module.TypeSystem.Int32;
            var resultVariable = new VariableDefinition(intType);
            junkMethod.Body.Variables.Add(resultVariable);

            junkIlProcessor.Append(junkIlProcessor.Create(OpCodes.Ldc_I4, Extensions.IntRng(-1000, 1000))); // Load constant
            junkIlProcessor.Append(junkIlProcessor.Create(OpCodes.Ldc_I4, Extensions.IntRng(-1000, 1000))); // Load constant
            junkIlProcessor.Append(junkIlProcessor.Create(OpCodes.Add)); // Add the two constants
            junkIlProcessor.Append(junkIlProcessor.Create(OpCodes.Stloc, resultVariable)); // Store the result in a local variable
            junkIlProcessor.Append(junkIlProcessor.Create(OpCodes.Ldloc, resultVariable)); // Load the local variable onto the stack
            junkIlProcessor.Append(junkIlProcessor.Create(OpCodes.Ret)); // Return the value

            if (calledByJunk)
            {
                if (Extensions.IntRng(0, 100) == 50)
                    AddSwitchWithNestedIf(junkMethod, randomClass, true);

                if (Extensions.IntRng(0, 50) == 25)
                    AddNestedIf(junkMethod, randomClass, true);

                if (Extensions.IntRng(0, 50) == 25)
                    AddIfOpaquePredicates(junkMethod, randomClass, true);
            }
            else
            {
                if (Extensions.IntRng(0, 50) == 25)
                    AddSwitchWithNestedIf(junkMethod, randomClass, true);

                if (Extensions.IntRng(0, 5) == 5)
                    AddNestedIf(junkMethod, randomClass, true);

                if (Extensions.IntRng(0, 5) == 5)
                    AddIfOpaquePredicates(junkMethod, randomClass, true);
            }
            
            return junkMethod;
        }

        /*private static MethodDefinition CreateJunkMethod(TypeDefinition type, ModuleDefinition module)
        {
            var junkMethod = new MethodDefinition(Renaming.GenerateUniqueName(), MethodAttributes.Private | MethodAttributes.Static, module.TypeSystem.Void);
            type.Methods.Add(junkMethod);
            var junkIlProcessor = junkMethod.Body.GetILProcessor();

            // Fill the junk method with random junk code
            var intType = module.TypeSystem.Int32;
            var resultVariable = new VariableDefinition(intType);
            junkMethod.Body.Variables.Add(resultVariable);

            junkIlProcessor.Append(junkIlProcessor.Create(OpCodes.Ldc_I4, Extensions.IntRng(-1000, 1000))); // Load constant
            junkIlProcessor.Append(junkIlProcessor.Create(OpCodes.Ldc_I4, Extensions.IntRng(-1000, 1000))); // Load constant
            junkIlProcessor.Append(junkIlProcessor.Create(OpCodes.Add)); // Add the two constants
            junkIlProcessor.Append(junkIlProcessor.Create(OpCodes.Stloc, resultVariable)); // Store the result in a local variable
            junkIlProcessor.Append(junkIlProcessor.Create(OpCodes.Ldloc, resultVariable)); // Load the local variable onto the stack
            junkIlProcessor.Append(junkIlProcessor.Create(OpCodes.Ret)); // Return the value


            if (Extensions.IntRng() == 2)
            {
                AddNestedIf(junkMethod, type);
                Statements.AddIfOpaquePredicates(junkMethod, type);
            }
            else if (Extensions.IntRng() == 0)
            {
                AddSwitchWithNestedIf(junkMethod, type);
            }
            return junkMethod;
        }*/
    }
}
