using Microsoft.VisualBasic.FileIO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using OBF.ILProcessing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBF.Modules;

internal class CodeInjection
{
    public static void InjectCode(AssemblyDefinition assembly)
    {
        foreach (TypeDefinition type in assembly.MainModule.Types)
        {
            var methods = new List<MethodDefinition>(type.Methods);

            foreach (var method in methods)
            {
                if (!method.HasBody)
                    continue;

                Statements.AddIfWithMethod(method, type);
                Statements.AddSwitchWithMethod(method, type);
            }
        }

        foreach (TypeDefinition type in assembly.MainModule.Types)
        {
            var methods = new List<MethodDefinition>(type.Methods);

            foreach (var method in methods)
            {
                if (!method.HasBody)
                    continue;

                var ilProcessor = method.Body.GetILProcessor();
                var firstInstruction = method.Body.Instructions.FirstOrDefault();
                if (firstInstruction == null)
                    continue;

                // Create dummy branching logic
                var nopInstruction = ilProcessor.Create(OpCodes.Nop);
                var branchInstruction = ilProcessor.Create(OpCodes.Br_S, firstInstruction);

                // Add dummy instructions
                ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldc_I4, 0));
                ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Brfalse_S, nopInstruction));
                ilProcessor.InsertBefore(firstInstruction, branchInstruction);
                ilProcessor.InsertBefore(firstInstruction, nopInstruction);

                // Log to the console
                Console.WriteLine($"Injected control flow obfuscation into method: {type.Name}.{method.Name}");
            }
        }



        /*var types = new List<TypeDefinition>(assembly.MainModule.Types);*/

        /*for (int i = 0; i < 10; i++)
        {
            var encryptionClass = new TypeDefinition("Class"+i, Renaming.GenerateUniqueName() + i, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed, assembly.MainModule.TypeSystem.Object);
            Generate.JunkClass(assembly, encryptionClass, Extensions.IntRng(3, i + 10), Extensions.IntRng(0, i + 6));            
        }*/

        /*foreach (TypeDefinition type in types)
        {
            Generate.JunkMethods(type); // Fixed :>
            Generate.JunkMethods(type);
            Generate.JunkVirtualMethods(type);
            Generate.JunkVirtualMethods(type);
        }

        foreach (TypeDefinition type in types)
        {
            Clone.RandomMethod(assembly.MainModule); // Fixed :>
            Clone.RandomMethod(assembly.MainModule);
            Clone.RandomMethod(assembly.MainModule);
            Clone.RandomMethod(assembly.MainModule);
            Clone.RandomMethod(assembly.MainModule);
            Clone.RandomMethod(assembly.MainModule);
        }*/
    }
}