using Mono.Cecil;
using Mono.Cecil.Cil;
using OBF.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBF.ILProcessing
{
    internal class Statements
    {
        public static void AddIfElse(MethodDefinition method)
        {
            var ilProcessor = method.Body.GetILProcessor();
            var endIf = ilProcessor.Create(OpCodes.Nop);
            var rng = Extensions.IntRng(-100, 100);

            ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_0)); // Load the parameter
            ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_I4, rng)); // Load constant 0
            ilProcessor.Append(ilProcessor.Create(OpCodes.Bne_Un_S, endIf)); // Branch to endIf if not equal

            // If block
            ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_0)); // Load the parameter
            ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_I4, -rng)); // Load constant 1
            ilProcessor.Append(ilProcessor.Create(OpCodes.Add)); // Add the parameter and constant
            ilProcessor.Append(ilProcessor.Create(OpCodes.Pop)); // Pop the result

            ilProcessor.Append(endIf); // End if
        }
        public static void AddSwitch(MethodDefinition method)
        {
            var ilProcessor = method.Body.GetILProcessor();
            var switchEnd = ilProcessor.Create(OpCodes.Nop);
            var case1 = ilProcessor.Create(OpCodes.Nop);
            var case2 = ilProcessor.Create(OpCodes.Nop);
            var rng = Extensions.IntRng(-100, 100);

            ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_0)); // Load the parameter
            ilProcessor.Append(ilProcessor.Create(OpCodes.Switch, new Instruction[] { case1, case2 })); // Switch statement

            // Default case
            ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_0)); // Load the parameter
            ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_I4, rng%5)); // Load constant 1
            ilProcessor.Append(ilProcessor.Create(OpCodes.Add)); // Add the parameter and constant
            ilProcessor.Append(ilProcessor.Create(OpCodes.Pop)); // Pop the result
            ilProcessor.Append(ilProcessor.Create(OpCodes.Br, switchEnd));

            // Case 1
            ilProcessor.Append(case1);
            ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_0)); // Load the parameter
            ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_I4, rng)); // Load constant 1
            ilProcessor.Append(ilProcessor.Create(OpCodes.Add)); // Add the parameter and constant
            ilProcessor.Append(ilProcessor.Create(OpCodes.Pop)); // Pop the result
            ilProcessor.Append(ilProcessor.Create(OpCodes.Br, switchEnd)); // Break

            // Case 2
            ilProcessor.Append(case2);
            ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_0)); // Load the parameter
            ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_I4, -rng)); // Load constant 2
            ilProcessor.Append(ilProcessor.Create(OpCodes.Add)); // Add the parameter and constant
            ilProcessor.Append(ilProcessor.Create(OpCodes.Pop)); // Pop the result
            ilProcessor.Append(ilProcessor.Create(OpCodes.Br, switchEnd)); // Break

            ilProcessor.Append(switchEnd); // End switch
        }
        public static void AddIfWithMethod(MethodDefinition method, TypeDefinition type)
        {
            if (!method.HasBody)
                return;

            var ilProcessor = method.Body.GetILProcessor();
            var firstInstruction = method.Body.Instructions.FirstOrDefault();
            if (firstInstruction == null)
                return;

            var endIf1 = ilProcessor.Create(OpCodes.Nop);
            var endIf2 = ilProcessor.Create(OpCodes.Nop);
            var endIf3 = ilProcessor.Create(OpCodes.Nop);

            // Create junk methods
            var junkMethod1 = CodeInjection.CreateJunkMethod(method.Module, false);
            var junkMethod2 = CodeInjection.CreateJunkMethod(method.Module, false);
            var junkMethod3 = CodeInjection.CreateJunkMethod(method.Module, false);

            // Insert dummy if-else logic with conditions
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldc_I4_1)); // Load constant 1
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Brtrue_S, endIf1)); // Branch to endIf1 if true

            // If block 1
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Call, junkMethod1)); // Call junk method 1
            ilProcessor.InsertBefore(firstInstruction, endIf1); // End if 1

            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldc_I4_1)); // Load constant 1
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Brtrue_S, endIf2)); // Branch to endIf2 if true

            // If block 2
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Call, junkMethod2)); // Call junk method 2
            ilProcessor.InsertBefore(firstInstruction, endIf2); // End if 2

            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldc_I4_1)); // Load constant 1
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Brtrue_S, endIf3)); // Branch to endIf3 if true

            // If block 3
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Call, junkMethod3)); // Call junk method 3
            ilProcessor.InsertBefore(firstInstruction, endIf3); // End if 3

            // Log to the console
            Console.WriteLine($"Injected nested if-else obfuscation into method: {type.Name}.{method.Name}");
        }
        /*private static MethodDefinition CreateJunkMethod(ModuleDefinition module, bool calledByJunk)
        {
            // Get a list of all types in the module
            var allTypes = module.Types.ToList();
            if (allTypes.Count == 0)
                throw new InvalidOperationException("No types found in the module.");

            // Select a random type
            var random = new Random();
            var randomType = allTypes[random.Next(allTypes.Count)];

            // Create the junk method
            var junkMethod = new MethodDefinition(Renaming.GenerateUniqueName(), MethodAttributes.Private | MethodAttributes.Static, module.TypeSystem.Void);
            randomType.Methods.Add(junkMethod);
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

            return junkMethod;
        }*/

        public static void AddSwitchWithMethod(MethodDefinition method, TypeDefinition type)
        {
            if (!method.HasBody)
                return;

            var ilProcessor = method.Body.GetILProcessor();
            var firstInstruction = method.Body.Instructions.FirstOrDefault();
            if (firstInstruction == null)
                return;

            var switchEnd = ilProcessor.Create(OpCodes.Nop);
            var case1 = ilProcessor.Create(OpCodes.Nop);
            var case2 = ilProcessor.Create(OpCodes.Nop);
            var case3 = ilProcessor.Create(OpCodes.Nop);

            // Create a junk method
            var junkMethod = new MethodDefinition(Renaming.GenerateUniqueName(), MethodAttributes.Private | MethodAttributes.Static, method.Module.TypeSystem.Void);
            type.Methods.Add(junkMethod);
            var junkIlProcessor = junkMethod.Body.GetILProcessor();

            // Fill the junk method with random junk code
            var intType = method.Module.TypeSystem.Int32;
            var resultVariable = new VariableDefinition(intType);
            junkMethod.Body.Variables.Add(resultVariable);

            junkIlProcessor.Append(junkIlProcessor.Create(OpCodes.Ldc_I4, Extensions.IntRng(-1000, 1000))); // Load constant
            junkIlProcessor.Append(junkIlProcessor.Create(OpCodes.Ldc_I4, Extensions.IntRng(-1000, 1000))); // Load constant
            junkIlProcessor.Append(junkIlProcessor.Create(OpCodes.Add)); // Add the two constants
            junkIlProcessor.Append(junkIlProcessor.Create(OpCodes.Stloc, resultVariable)); // Store the result in a local variable
            junkIlProcessor.Append(junkIlProcessor.Create(OpCodes.Ldloc, resultVariable)); // Load the local variable onto the stack
            junkIlProcessor.Append(junkIlProcessor.Create(OpCodes.Ret)); // Return the value

            // Insert dummy switch logic with a condition that is always false
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldc_I4, -1)); // Load constant -1 (an invalid case)
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Switch, new Instruction[] { case1, case2, case3 })); // Switch statement

            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Br, switchEnd));

            // Case 1
            ilProcessor.InsertBefore(firstInstruction, case1);
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Call, junkMethod)); // Call junk method
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Br, switchEnd)); // Break

            // Case 2
            ilProcessor.InsertBefore(firstInstruction, case2);
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Call, junkMethod)); // Call junk method
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Br, switchEnd)); // Break

            // Case 3
            ilProcessor.InsertBefore(firstInstruction, case3);
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Call, junkMethod)); // Call junk method
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Br, switchEnd));

            ilProcessor.InsertBefore(firstInstruction, switchEnd); // End switch

            // Log to the console
            Console.WriteLine($"Injected switch obfuscation into method: {type.Name}.{method.Name}");
        }
        
        /*public static void AddDoWhileOpaquePredicates(MethodDefinition method, TypeDefinition type)
        {
            if (!method.HasBody)
                return;

            var ilProcessor = method.Body.GetILProcessor();
            var instructions = method.Body.Instructions;
            var random = new Random();

            // Find a random position within the method's instructions
            int randomIndex = random.Next(instructions.Count);

            var startLabel = ilProcessor.Create(OpCodes.Nop);
            var endLabel = ilProcessor.Create(OpCodes.Nop);

            // Define a local variable
            var intType = method.Module.TypeSystem.Int32;
            var localVariable = new VariableDefinition(intType);
            method.Body.Variables.Add(localVariable);

            // Insert the do-while loop at the random position
            ilProcessor.InsertBefore(instructions[randomIndex], ilProcessor.Create(OpCodes.Br, startLabel));

            ilProcessor.InsertBefore(instructions[randomIndex], startLabel);
            ilProcessor.InsertBefore(instructions[randomIndex], ilProcessor.Create(OpCodes.Ldc_I4, 100));
            ilProcessor.InsertBefore(instructions[randomIndex], ilProcessor.Create(OpCodes.Stloc, localVariable));

            // Do-while loop start
            ilProcessor.InsertBefore(instructions[randomIndex], ilProcessor.Create(OpCodes.Ldloc, localVariable));
            ilProcessor.InsertBefore(instructions[randomIndex], ilProcessor.Create(OpCodes.Ldc_I4, 50));
            ilProcessor.InsertBefore(instructions[randomIndex], ilProcessor.Create(OpCodes.Blt, endLabel));

            // Placeholder for additional logic inside the loop
            ilProcessor.InsertBefore(instructions[randomIndex], ilProcessor.Create(OpCodes.Nop));

            // Loop back to start
            ilProcessor.InsertBefore(instructions[randomIndex], ilProcessor.Create(OpCodes.Br, startLabel));

            ilProcessor.InsertBefore(instructions[randomIndex], endLabel);
            ilProcessor.InsertBefore(instructions[randomIndex], ilProcessor.Create(OpCodes.Nop)); // End of method

            Console.WriteLine($"Injected Do While obfuscation into method: {type.Name}.{method.Name}");
        }*/

        public static void AddDoWhile(MethodDefinition method, TypeDefinition type)
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
            var endIf = ilProcessor.Create(OpCodes.Nop);

            // Create a junk method
            var junkMethod = new MethodDefinition(Renaming.GenerateUniqueName(), MethodAttributes.Private | MethodAttributes.Static, method.Module.TypeSystem.Void);
            type.Methods.Add(junkMethod);
            var junkIlProcessor = junkMethod.Body.GetILProcessor();

            // Fill the junk method with random junk code
            var intType = method.Module.TypeSystem.Int32;
            var resultVariable = new VariableDefinition(intType);
            junkMethod.Body.Variables.Add(resultVariable);

            junkIlProcessor.Append(junkIlProcessor.Create(OpCodes.Ldc_I4, Extensions.IntRng(-1000, 1000))); // Load constant
            junkIlProcessor.Append(junkIlProcessor.Create(OpCodes.Ldc_I4, Extensions.IntRng(-1000, 1000))); // Load constant
            junkIlProcessor.Append(junkIlProcessor.Create(OpCodes.Add)); // Add the two constants
            junkIlProcessor.Append(junkIlProcessor.Create(OpCodes.Stloc, resultVariable)); // Store the result in a local variable
            junkIlProcessor.Append(junkIlProcessor.Create(OpCodes.Ldloc, resultVariable)); // Load the local variable onto the stack
            junkIlProcessor.Append(junkIlProcessor.Create(OpCodes.Ret)); // Return the value

            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Br, label1));

            ilProcessor.InsertBefore(firstInstruction, label1);
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldc_I4, 100));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Stloc_0));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Br, label2));

            ilProcessor.InsertBefore(firstInstruction, label2);
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldloc_0));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldc_I4, 50));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Blt, label3));

            // If block
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Call, junkMethod)); // Call junk method

            ilProcessor.InsertBefore(firstInstruction, endIf); // End if

            // Log to the console
            Console.WriteLine($"Injected do-while obfuscation into method: {type.Name}.{method.Name}");
        }
        public static void AddMethodCall(MethodDefinition method, TypeDefinition type)
        {
            if (!method.HasBody)
                return;

            var ilProcessor = method.Body.GetILProcessor();
            var firstInstruction = method.Body.Instructions.FirstOrDefault();
            if (firstInstruction == null)
                return;

            // Create a new method with obfuscation logic
            var newMethod = new MethodDefinition("Obfuscated_" + method.Name, MethodAttributes.Private | MethodAttributes.Static, method.ReturnType);
            type.Methods.Add(newMethod);

            var newIlProcessor = newMethod.Body.GetILProcessor();
            var nopInstruction = newIlProcessor.Create(OpCodes.Nop);
            var branchInstruction = newIlProcessor.Create(OpCodes.Br_S, nopInstruction);
            var retInstruction = newIlProcessor.Create(OpCodes.Ret);

            // Add dummy instructions
            newIlProcessor.Append(newIlProcessor.Create(OpCodes.Ldc_I4, 0));
            newIlProcessor.Append(newIlProcessor.Create(OpCodes.Brfalse_S, nopInstruction));
            newIlProcessor.Append(branchInstruction);
            newIlProcessor.Append(nopInstruction);
            newIlProcessor.Append(retInstruction);

            // Call the new method from the original method
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Call, newMethod));

            // Log to the console
            Console.WriteLine($"Injected control flow obfuscation into method: {type.Name}.{method.Name}");
        }
    }
}
