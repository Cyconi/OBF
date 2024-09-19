using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OBF.Algorithms;

public class Renaming
{
    private static readonly Random random = new();
    internal static void RenameAssembly(AssemblyDefinition assembly)
    {
        foreach (TypeDefinition type in assembly.MainModule.Types)
        {
            if (type.IsEnum || type.IsPublic)
                continue;

            type.Name = GenerateUniqueName();

            foreach (MethodDefinition method in type.Methods)
            {
                if (method.HasOverrides || !method.HasBody || method.IsVirtual || method.DeclaringType != type)
                    continue;

                if (!method.IsConstructor && !method.HasOverrides && !method.IsSpecialName && (type.IsNotPublic || method.IsPrivate || method.IsAssembly))
                    method.Name = GenerateUniqueName();

                foreach (var gen in method.GenericParameters)
                    if (!method.HasOverrides)
                        gen.Name = GenerateUniqueName();

                foreach (ParameterDefinition parameter in method.Parameters)
                    if (!method.HasOverrides)
                        parameter.Name = GenerateUniqueName();
            }

            foreach (PropertyDefinition property in type.Properties)
            {
                if (property.IsSpecialName || property.DeclaringType != type || (type.IsPublic && (property.GetMethod?.IsPublic == true || property.SetMethod?.IsPublic == true)))
                    continue;

                property.Name = GenerateUniqueName();

                if (property.GetMethod != null && property.GetMethod.DeclaringType == type && (type.IsNotPublic || property.GetMethod.IsPrivate || property.GetMethod.IsAssembly))
                    property.GetMethod.Name = GenerateUniqueName();

                if (property.SetMethod != null && property.SetMethod.DeclaringType == type && (type.IsNotPublic || property.SetMethod.IsPrivate || property.SetMethod.IsAssembly))
                    property.SetMethod.Name = GenerateUniqueName();
            }

            foreach (FieldDefinition field in type.Fields)
                if (!field.HasCustomAttributes && field.DeclaringType == type && (type.IsNotPublic || field.IsPrivate || field.IsAssembly))
                    field.Name = GenerateUniqueName();
        }

        foreach (var module in assembly.Modules)
            foreach (var type in module.Types)
                if (!string.IsNullOrEmpty(type.Namespace))
                    type.Namespace = GenerateUniqueName();
    }

    /*public static string GenerateUniqueName(int length = 20)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        StringBuilder result = new StringBuilder(length);

        for (int i = 0; i < length; i++)
            result.Append(chars[random.Next(chars.Length)]);

        return result.ToString();
    }*/
    public static string GenerateUniqueName() { return Guid.NewGuid().ToString("N"); }
}

