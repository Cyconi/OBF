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
        var types = new List<TypeDefinition>(assembly.MainModule.Types);

        /*for (int i = 0; i < 10; i++)
        {
            var encryptionClass = new TypeDefinition("Class"+i, Renaming.GenerateUniqueName() + i, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed, assembly.MainModule.TypeSystem.Object);
            Generate.JunkClass(assembly, encryptionClass, Extensions.IntRng(3, i + 10), Extensions.IntRng(0, i + 6));            
        }*/

        foreach (TypeDefinition type in types)
        {
            Generate.JunkMethods(type); // Fixed :>
            Generate.JunkVirtualMethods(type);
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
        }
    }
}