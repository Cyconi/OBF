using Microsoft.VisualBasic.FileIO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using OBF.ILProcessing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBF.Algorithms;

internal class CodeInjection
{
    public static void InjectCode(AssemblyDefinition assembly)
    {
        /*foreach (TypeDefinition type in assembly.MainModule.Types)
        {
            var methods = new List<MethodDefinition>(type.Methods);

            foreach (MethodDefinition method in methods)
            {
                Statements.AddSwitch(method);
                Statements.AddIfElse(method);
            }
        }*/

        var types = new List<TypeDefinition>(assembly.MainModule.Types);

        /*for (int i = 0; i < types.Count; i++)
        {
            var encryptionClass = new TypeDefinition(types[i].Namespace, "ClassName" + i, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed, assembly.MainModule.TypeSystem.Object);
            JunkClass(assembly, encryptionClass, VarRng(3, 10), VarRng(0, 6));
            //JunkMethods(types[i]);
            //RandomMethod(types[i].Module);
        }*/
        /*for (int i = 0; i < types.Count / 4; i++)
        {
            var encryptionClass = new TypeDefinition("NameSpace", "ClassName" + i, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed, assembly.MainModule.TypeSystem.Object);
            Generate.JunkClass(assembly, encryptionClass, Extensions.VarRng(3, i + 10), Extensions.VarRng(0, i + 6));
            //JunkMethods(types[i]);
            //RandomMethod(types[i].Module);
        }*/

        foreach (TypeDefinition type in types)
        {
            Generate.JunkMethods(type); // Fixed :>
            Generate.JunkMethods(type);
            Generate.JunkMethods(type);
        }

        foreach (TypeDefinition type in types)
        {
            Clone.RandomMethod(assembly.MainModule); // Fixed :>
            Clone.RandomMethod(assembly.MainModule);
            Clone.RandomMethod(assembly.MainModule);
            Clone.RandomMethod(assembly.MainModule);
            Clone.RandomMethod(assembly.MainModule);
            Clone.RandomMethod(assembly.MainModule);
        }
    }
}