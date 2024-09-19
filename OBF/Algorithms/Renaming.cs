using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OBF.Algorithms;

public class Renaming
{
    private static readonly Random random = new();
    internal static void RenameAssembly(AssemblyDefinition assembly)
    {
        foreach (var module in assembly.Modules)
            foreach (var type in module.Types)
                if (!string.IsNullOrEmpty(type.Namespace))
                    type.Namespace = GenerateUniqueName(type.Namespace);

        foreach (TypeDefinition type in assembly.MainModule.Types)
        {
            if ((type.Name.Equals("AppStart") || type.Name.Equals("Client")) && type.IsPublic)
                type.Name = GenerateUniqueName(type.Name);

            if (type.IsEnum || type.IsPublic)
                continue;

            type.Name = GenerateUniqueName(type.Name);

            foreach (MethodDefinition method in type.Methods)
            {
                if (!method.HasBody || method.IsVirtual || method.DeclaringType != type)
                    continue;

                if (!method.IsConstructor && !method.IsSpecialName)
                    method.Name = GenerateUniqueName(method.Name);

                foreach (var gen in method.GenericParameters)
                    gen.Name = GenerateUniqueName(gen.Name);

                foreach (ParameterDefinition parameter in method.Parameters)
                    parameter.Name = GenerateUniqueName(parameter.Name);
            }

            foreach (PropertyDefinition property in type.Properties)
            {
                if (property.IsSpecialName || property.DeclaringType != type)
                    continue;

                property.Name = GenerateUniqueName(property.Name);

                if (property.GetMethod != null && property.GetMethod.DeclaringType == type)
                    property.GetMethod.Name = GenerateUniqueName(property.GetMethod.Name);

                if (property.SetMethod != null && property.SetMethod.DeclaringType == type)
                    property.SetMethod.Name = GenerateUniqueName(property.SetMethod.Name);
            }

            foreach (FieldDefinition field in type.Fields)
                if (!field.HasCustomAttributes && field.DeclaringType == type)
                    field.Name = GenerateUniqueName(field.Name);
        }
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
    public static string GenerateUniqueName(string originalName)
    {
        string newName = Guid.NewGuid().ToString("N");
        Console.WriteLine($"Renaming {originalName}...");
        return newName;
    }

}

