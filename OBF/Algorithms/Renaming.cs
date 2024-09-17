using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBF.Algorithms;

public class Renaming
{
    private static readonly Random random = new();
    internal static void RenameAssembly(AssemblyDefinition assembly)
    {
        foreach (TypeDefinition type in assembly.MainModule.Types)
        {
            type.Name = GenerateUniqueName();

            if (type.IsPublic)
                continue;

            foreach (MethodDefinition method in type.Methods)
            {
                if (method.HasOverrides || !method.HasBody || method.IsVirtual || method.IsSpecialName)
                    continue;

                method.Name = GenerateUniqueName();
            }

            foreach (PropertyDefinition property in type.Properties)
            {
                if (property.IsSpecialName)
                    continue;

                property.Name = GenerateUniqueName();

                if (property.GetMethod != null)
                    property.GetMethod.Name = GenerateUniqueName();

                if (property.SetMethod != null)
                    property.SetMethod.Name = GenerateUniqueName();
            }

            foreach (FieldDefinition field in type.Fields)
            {
                if (field.HasCustomAttributes)
                    continue;

                field.Name = GenerateUniqueName();
            }
        }

        foreach (var module in assembly.Modules)
            foreach (var type in module.Types)
                if (!string.IsNullOrEmpty(type.Namespace))
                    type.Namespace = GenerateUniqueName();
    }
    public static string GenerateUniqueName(int length = 20)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        StringBuilder result = new StringBuilder(length);

        for (int i = 0; i < length; i++)
            result.Append(chars[random.Next(chars.Length)]);

        return result.ToString();
    }
    //public static string GenerateUniqueName() { return Guid.NewGuid().ToString(); }

}

