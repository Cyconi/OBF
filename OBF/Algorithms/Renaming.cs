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
            type.Name = GenerateUniqueName();

            if (type.IsPublic)
                continue;

            foreach (MethodDefinition method in type.Methods)
            {
                if (method.HasOverrides || !method.HasBody || method.IsVirtual || method.IsSpecialName)
                    continue;

                method.Name = GenerateUniqueName();

                // still testing v

                /*foreach (ParameterDefinition parameter in method.Parameters)
                    parameter.Name = GenerateUniqueName();

                foreach (VariableDefinition var in method.Body.Variables)
                {
                    var.VariableType = new TypeReference(
                        var.VariableType.Namespace,
                        GenerateUniqueName(),
                        var.VariableType.Module,
                        var.VariableType.Scope
                    );
                }
                foreach (var instruction in method.Body.Instructions)
                {
                    if (instruction.Operand is VariableDefinition variable)
                    {
                        variable.VariableType = new TypeReference(
                            variable.VariableType.Namespace,
                            GenerateUniqueName(),
                            variable.VariableType.Module,
                            variable.VariableType.Scope
                        );
                    }
                    else if (instruction.Operand is ParameterDefinition parameter)
                        parameter.Name = GenerateUniqueName(); 
                }

                foreach (var gen in method.GenericParameters)
                    gen.Name = GenerateUniqueName();*/
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

