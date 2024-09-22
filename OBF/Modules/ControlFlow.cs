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
    internal class ControlFlow
    {
        public static void CtrlFlow(AssemblyDefinition assembly)
        {
            foreach (TypeDefinition type in assembly.MainModule.Types)
            {
                var methods = new List<MethodDefinition>(type.Methods);

                foreach (var method in methods)
                {
                    //AddNestedIfOpaquePredicates(method, type);
                    //Statements.AddDoWhileOpaquePredicates(method, type);
                    //Statements.AddIfOpaquePredicates(method, type);
                    //AddSwitchWithNestedIf(method, type);
                    RandomizeMethodCalls(method, type);
                }
            }
        }
        public static void RandomizeMethodCalls(MethodDefinition method, TypeDefinition type)
        {
            Random random = new();
            var actions = new List<Action>
            {
                () => AddNestedIfOpaquePredicates(method, type),
                () => Statements.AddDoWhileOpaquePredicates(method, type),
                () => Statements.AddIfOpaquePredicates(method, type),
                () => AddSwitchWithNestedIf(method, type)
            };

            // Randomize the number of actions to call (up to 4)
            int numberOfActions = random.Next(1, actions.Count + 1);

            // Call the actions
            for (int i = 0; i < numberOfActions; i++)
            {
                // Select a random action and execute it
                var randomAction = actions[random.Next(actions.Count)];
                randomAction();
            }
        }
        public static void AddNestedIfOpaquePredicates(MethodDefinition method, TypeDefinition type)
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
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Nop)); // Placeholder for additional logic

            ilProcessor.InsertBefore(firstInstruction, endLabel);
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Nop)); // End of method

            Console.WriteLine($"Injected nested if obfuscation into method: {type.Name}.{method.Name}");
        }
        public static void AddSwitchWithNestedIf(MethodDefinition method, TypeDefinition type)
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

            Console.WriteLine($"Injected switch with nested if statements into method: {type.Name}.{method.Name}");
        }
        private static MethodDefinition CreateJunkMethod(TypeDefinition type, ModuleDefinition module)
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

            AddNestedIfOpaquePredicates(junkMethod, type);
            Statements.AddIfOpaquePredicates(junkMethod, type);

            return junkMethod;
        }
    }
}
