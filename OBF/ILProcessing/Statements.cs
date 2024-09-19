using Mono.Cecil;
using Mono.Cecil.Cil;
using OBF.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBF.ILProcessing
{
    internal class Statements
    {
        public static void AddIfElse1(MethodDefinition method)
        {
            if (method.Body == null)
                return;

            var ilProcessor = method.Body.GetILProcessor();
            var instructions = method.Body.Instructions;

            // Create labels for branching
            var elseLabel = ilProcessor.Create(OpCodes.Nop);
            var endIfLabel = ilProcessor.Create(OpCodes.Nop);

            // Insert the condition at the beginning of the method
            var firstInstruction = instructions.First();

            // Load the condition onto the stack (e.g., load a constant value)
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldc_I4, 1)); // Load constant 1 (true)

            // Branch to the else block if the condition is false
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Brfalse, elseLabel));

            // Generate the code for the if block
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldstr, "If block executed"));
            //ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Call, method.Module.ImportReference(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }))));

            // Branch to the end of the if-else statement
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Br, endIfLabel));

            // Generate the code for the else block
            ilProcessor.InsertBefore(firstInstruction, elseLabel);
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldstr, "Else block executed"));
            //ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Call, method.Module.ImportReference(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }))));

            // Mark the end of the if-else statement
            ilProcessor.InsertBefore(firstInstruction, endIfLabel);
        }

        public static void AddIfElse(MethodDefinition method)
        {
            var ilProcessor = method.Body.GetILProcessor();
            var endIf = ilProcessor.Create(OpCodes.Nop);
            var rng = Extensions.VarRng(-100, 100);

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
            if (method.Body == null)
                return;

            var ilProcessor = method.Body.GetILProcessor();
            var instructions = method.Body.Instructions;

            // Create labels for each case and the default case
            var case0Label = ilProcessor.Create(OpCodes.Nop);
            var case1Label = ilProcessor.Create(OpCodes.Nop);
            var case2Label = ilProcessor.Create(OpCodes.Nop);
            var defaultLabel = ilProcessor.Create(OpCodes.Nop);
            var endSwitchLabel = ilProcessor.Create(OpCodes.Nop);

            // Insert the switch value at the beginning of the method
            var firstInstruction = instructions.First();

            // Load the switch value onto the stack (e.g., load a constant value)
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldc_I4, Extensions.VarRng(4, 100))); // Load a random value between 0 and 2

            // Generate the switch instruction
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Switch, new Instruction[] { case0Label, case1Label, case2Label }));

            // Generate the code for case 0
            ilProcessor.InsertBefore(firstInstruction, case0Label);
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldstr, "Case 0 executed"));
            //ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Call, method.Module.ImportReference(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }))));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Br, endSwitchLabel));

            // Generate the code for case 1
            ilProcessor.InsertBefore(firstInstruction, case1Label);
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldstr, "Case 1 executed"));
            //ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Call, method.Module.ImportReference(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }))));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Br, endSwitchLabel));

            // Generate the code for case 2
            ilProcessor.InsertBefore(firstInstruction, case2Label);
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldstr, "Case 2 executed"));
            //ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Call, method.Module.ImportReference(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }))));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Br, endSwitchLabel));

            // Generate the code for the default case
            ilProcessor.InsertBefore(firstInstruction, defaultLabel);
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldstr, "Default case executed"));
            //ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Call, method.Module.ImportReference(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }))));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Br, endSwitchLabel));

            // Mark the end of the switch statement
            ilProcessor.InsertBefore(firstInstruction, endSwitchLabel);
        }
        public static void AddSwitch0(MethodDefinition method)
        {
            var ilProcessor = method.Body.GetILProcessor();
            var switchEnd = ilProcessor.Create(OpCodes.Nop);
            var case1 = ilProcessor.Create(OpCodes.Nop);
            var case2 = ilProcessor.Create(OpCodes.Nop);
            var rng = Extensions.VarRng(-100, 100);

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

    }
}
