using Microsoft.VisualBasic.FileIO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBF.Algorithms;

internal class CodeInjection
{
    public static void InjectCode(AssemblyDefinition assembly)
    {
        for (int i = 0; i < 10/*assembly.Modules.Count*/; i++)
        {
            var encryptionClass = new TypeDefinition("NameSpace", "ClassName" + i, TypeAttributes.Public | TypeAttributes.Class, assembly.MainModule.TypeSystem.Object);
            JunkClass(assembly, encryptionClass);
        }
        foreach (TypeDefinition type in assembly.MainModule.Types)
            JunkMethods(type);

    }
    public static void CloneMethod(ModuleDefinition module,string className, string methodName)
    {
        var type = module.Types.FirstOrDefault(t => t.Name.Contains(className)) ?? throw new ArgumentException($"Class {className} not found in type {module.Name}");
        // Find the method to clone
        var methodToClone = type.Methods.FirstOrDefault(m => m.Name.Contains(methodName)) ?? throw new ArgumentException($"Method {methodName} not found in type {type.Name}");

        // Create a new method definition
        var clonedMethod = new MethodDefinition(methodToClone.Name + "_Clone",
            methodToClone.Attributes, methodToClone.ReturnType);

        // Copy parameters
        foreach (var parameter in methodToClone.Parameters)
            clonedMethod.Parameters.Add(new ParameterDefinition(parameter.Name, parameter.Attributes, parameter.ParameterType));        

        // Copy method body
        var ilProcessor = clonedMethod.Body.GetILProcessor();
        var body = methodToClone.Body;

        // Copy variables
        foreach (var variable in body.Variables)
            clonedMethod.Body.Variables.Add(new VariableDefinition(variable.VariableType));        

        // Copy instructions
        foreach (var instruction in body.Instructions)
            ilProcessor.Append(instruction);        

        // Copy exception handlers
        foreach (var handler in body.ExceptionHandlers)
            clonedMethod.Body.ExceptionHandlers.Add(handler);        

        // Add the cloned method to the type
        type.Methods.Add(clonedMethod);
    }
    public static void JunkCode(MethodDefinition method)
    {
        if (method.Body == null)
            return;

        var ilProcessor = method.Body.GetILProcessor();
        var instructions = method.Body.Instructions;

        // Example: Injecting junk code
        var junkInstruction1 = ilProcessor.Create(OpCodes.Ldc_I4, VarRng(-1000, 1000)); // Load constant 1234
        var junkInstruction2 = ilProcessor.Create(OpCodes.Pop); // Pop the value from the stack

        // Insert junk instructions at the beginning of the method
        ilProcessor.InsertBefore(instructions.First(), junkInstruction1);
        ilProcessor.InsertAfter(junkInstruction1, junkInstruction2);
    }
    public static void JunkMethods(TypeDefinition type)
    {
        var module = type.Module;
        var voidType = module.ImportReference(typeof(void));
        var intType = module.ImportReference(typeof(int));

        // Create a new junk method
        var junkMethod = new MethodDefinition("JunkMethod" + Guid.NewGuid().ToString("N"), MethodAttributes.Private | MethodAttributes.Static, voidType);

        // Add parameters to the junk method
        junkMethod.Parameters.Add(new ParameterDefinition("param1", ParameterAttributes.None, intType));

        // Create the method body
        var ilProcessor = junkMethod.Body.GetILProcessor();
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_I4, VarRng(-1000, 1000))); // Load constant
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_I4, VarRng(-1000, 1000))); // Load constant
        ilProcessor.Append(ilProcessor.Create(OpCodes.Add)); // Add the two constants
        ilProcessor.Append(ilProcessor.Create(OpCodes.Pop)); // Pop the result from the stack
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ret)); // Return

        // Add the junk method to the type
        type.Methods.Add(junkMethod);
    }

    public static void JunkClass(AssemblyDefinition assembly, TypeDefinition type, int methods = 5, int fields = 3)
    {
        var module = assembly.MainModule;

        // Add fields to the class
        for (int i = 0; i < fields; i++)
        {
            var field = CreateJunkField(module, module.Name + i, SysRng());
            type.Fields.Add(field);
        }
        
        // Add junk methods to the class
        for (int i = 0; i < methods; i++)
        {
            var junkMethod = CreateJunkMethod(module, module.Name + i, VarRng(), VarRng(), SysRng(), SysRng());
            type.Methods.Add(junkMethod);
        }

        // Add the class to the module if it's not already added
        if (!module.Types.Contains(type))
            module.Types.Add(type);
    }

    private static FieldDefinition CreateJunkField(ModuleDefinition module, string fieldName, SystemType fieldType = SystemType.Int, FieldAttributes attributes = FieldAttributes.Private | FieldAttributes.Static) { return new FieldDefinition(fieldName + fieldType, attributes, module.GetSystemType(fieldType)); }

    private static MethodDefinition CreateJunkMethod(ModuleDefinition module, string methodName, int variables, int parameters, SystemType methodType, SystemType paramType, MethodAttributes attributes = MethodAttributes.Private | MethodAttributes.Static)
    {
        var mType = module.GetSystemType(methodType);
        var pType = module.GetSystemType(paramType);

        // Create a new junk method
        var junkMethod = new MethodDefinition(methodName + methodType, attributes, mType);

        // Add parameters to the junk method (optional)
        for (int i = 0; i < parameters; i++)
        {
            junkMethod.Parameters.Add(new ParameterDefinition("param" + i, ParameterAttributes.None, pType));
        }

        // Create the method body
        var ilProcessor = junkMethod.Body.GetILProcessor();

        // Add variables to the method body
        for (int i = 0; i < variables; i++)
        {
            junkMethod.Body.Variables.Add(new VariableDefinition(pType));
        }

        // Initialize variables with parameter values or constants
        /*if (parameters > 0 && variables > 0)
        {
            for (int i = 0; i < variables; i++)
            {
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_0)); // Load the first parameter
                ilProcessor.Append(ilProcessor.Create(OpCodes.Stloc, i)); // Store it in the variable
            }
        }

        // Perform some operations using the variables and parameters
        if (parameters > 0 && variables > 0)
        {
            for (int i = 0; i < variables; i++)
            {
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ldloc, i)); // Load the variable
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_0)); // Load the first parameter
                ilProcessor.Append(ilProcessor.Create(OpCodes.Add)); // Add the parameter to the variable
                ilProcessor.Append(ilProcessor.Create(OpCodes.Stloc, i)); // Store the result back in the variable
            }
        }*/

        // Add some junk instructions
        //ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_I4, VarRng(-1000, 1000))); // Load constant
        //ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_I4, VarRng(-1000, 1000))); // Load constant 
        //ilProcessor.Append(ilProcessor.Create(OpCodes.Rem)); // Remainder of the two constants
        //ilProcessor.Append(ilProcessor.Create(OpCodes.Pop)); // Pop the result from the stack
        //ilProcessor.Append(ilProcessor.Create(OpCodes.Ret)); // Return

        return junkMethod;
    }


    private static SystemType SysRng(int startRange = 0, int endRange = 7)
    {
        Random random = new();
        return (SystemType)random.Next(startRange, endRange);
    }
    private static int VarRng(int startRange = 0, int endRange = 4)
    {
        Random random = new();
        return random.Next(startRange, endRange);
    }
}