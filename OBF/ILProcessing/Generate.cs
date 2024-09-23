using Mono.Cecil.Cil;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OBF.Modules;

namespace OBF.ILProcessing
{
    internal class Generate
    {
        public static void JunkMethods(TypeDefinition type)
        {
            if (!type.IsClass || type.IsEnum || type.IsInterface || type.IsDelegate() || type.IsCompilerGenerated())
                return;

            var module = type.Module;
            var paramType = module.GetSystemType((SystemType)Extensions.IntRng());
            var methodType = module.GetSystemType(Extensions.SysRng());

            // Create a new junk method
            var junkMethod = new MethodDefinition(Renaming.GenerateUniqueName(), MethodAttributes.Private | MethodAttributes.Static, methodType);
            junkMethod.Parameters.Add(new ParameterDefinition("param", ParameterAttributes.None, paramType));

            // Create the method body
            var ilProcessor = junkMethod.Body.GetILProcessor();
            var resultVariable = new VariableDefinition(paramType);
            junkMethod.Body.Variables.Add(resultVariable);

            // Generate random method calls
            var randomMethodCount = Extensions.IntRng(1, 4); // Random number of method calls

            var methods = new List<MethodDefinition>();
            var existingClasses = module.Types.Where(t => t.IsClass && !t.IsEnum && !t.IsInterface && !t.IsCompilerGenerated() && !t.IsDelegate() && t != type).ToList();

            for (int i = 0; i < randomMethodCount; i++)
            {
                if (existingClasses.Count == 0)
                {
                    Console.WriteLine("[CodeInjection] No suitable classes found for injecting junk methods.");
                    return;
                }

                // Select a random existing class
                var randomClass = existingClasses[Extensions.IntRng(0, existingClasses.Count)];

                // Create a new method in the selected class
                var randomMethod = new MethodDefinition(Renaming.GenerateUniqueName(), MethodAttributes.Private | MethodAttributes.Static, methodType);
                randomMethod.Parameters.Add(new ParameterDefinition("param" + i, ParameterAttributes.None, paramType));

                var randomIlProcessor = randomMethod.Body.GetILProcessor();
                randomIlProcessor.Append(randomIlProcessor.Create(OpCodes.Ldarg_0)); // Load the parameter

                // Randomly choose to add if-else or switch statement
                var action = Extensions.IntRng(0, 6);
                switch (action)
                {
                    case 0:
                        Statements.AddIfElse(randomMethod);
                        break;
                    case 1:
                        Statements.AddSwitch(randomMethod);
                        break;
                    case 2:
                        Statements.AddIfElse(randomMethod);
                        Statements.AddIfElse(randomMethod);
                        break;
                    case 3:
                        Statements.AddSwitch(randomMethod);
                        Statements.AddSwitch(randomMethod);
                        break;
                    default:
                        Statements.AddIfElse(randomMethod);                        
                        Statements.AddSwitch(randomMethod);
                        break;
                }

                randomIlProcessor.Append(randomIlProcessor.Create(OpCodes.Ret)); // Return

                randomClass.Methods.Add(randomMethod);
                methods.Add(randomMethod);
            }

            // Create tree-like structure
            for (int i = 0; i < methods.Count; i++)
            {
                var method = methods[i];
                var methodIlProcessor = method.Body.GetILProcessor();

                if (2 * i + 1 < methods.Count)
                {
                    methodIlProcessor.InsertBefore(methodIlProcessor.Body.Instructions.Last(), methodIlProcessor.Create(OpCodes.Ldarg_0)); // Load the parameter
                    methodIlProcessor.InsertBefore(methodIlProcessor.Body.Instructions.Last(), methodIlProcessor.Create(OpCodes.Call, methods[2 * i + 1])); // Call left child
                }

                if (2 * i + 2 < methods.Count)
                {
                    methodIlProcessor.InsertBefore(methodIlProcessor.Body.Instructions.Last(), methodIlProcessor.Create(OpCodes.Ldarg_0)); // Load the parameter
                    methodIlProcessor.InsertBefore(methodIlProcessor.Body.Instructions.Last(), methodIlProcessor.Create(OpCodes.Call, methods[2 * i + 2])); // Call right child
                }
            }

            if (methods.Count > 0)
            {
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_0)); // Load the parameter
                ilProcessor.Append(ilProcessor.Create(OpCodes.Call, methods[0])); // Call the root method
            }

            ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_I4, Extensions.IntRng(-1000, 1000))); // Load constant
            ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_I4, Extensions.IntRng(-1000, 1000))); // Load constant
            ilProcessor.Append(ilProcessor.Create(OpCodes.Add)); // Add the two constants
            ilProcessor.Append(ilProcessor.Create(OpCodes.Stloc, resultVariable)); // Store the result in a local variable
            ilProcessor.Append(ilProcessor.Create(OpCodes.Ldloc, resultVariable)); // Load the local variable onto the stack
            ilProcessor.Append(ilProcessor.Create(OpCodes.Ret)); // Return the value

            // Add the junk method to the type
            type.Methods.Add(junkMethod);
            Console.WriteLine($"[CodeInjection] Added junk method {junkMethod.Name} to class {type.Name}");
        }
        public static void JunkVirtualMethods(TypeDefinition type)
        {
            if (!type.IsClass || type.IsEnum || type.IsInterface || type.IsDelegate() || type.IsCompilerGenerated())
                return;

            var module = type.Module;
            var stringType = module.ImportReference(typeof(string));
            var voidType = module.ImportReference(typeof(void));

            // Create a new junk method
            var junkMethod = new MethodDefinition(Renaming.GetRealisticName(), MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig, voidType);
            junkMethod.Parameters.Add(new ParameterDefinition("param", ParameterAttributes.None, stringType));

            // Create the method body
            var ilProcessor = junkMethod.Body.GetILProcessor();
            var resultVariable = new VariableDefinition(stringType);
            junkMethod.Body.Variables.Add(resultVariable);

            // Generate random method calls
            var randomMethodCount = Extensions.IntRng(1, 4); // Random number of method calls

            var methods = new List<MethodDefinition>();
            var existingClasses = module.Types.Where(t => t.IsClass && !t.IsEnum && !t.IsInterface && !t.IsCompilerGenerated() && !t.IsDelegate() && t != type).ToList();

            for (int i = 0; i < randomMethodCount; i++)
            {
                if (existingClasses.Count == 0)
                {
                    Console.WriteLine("[CodeInjection] No suitable classes found for injecting junk methods.");
                    return;
                }

                // Select a random existing class
                var randomClass = existingClasses[Extensions.IntRng(0, existingClasses.Count)];

                // Create a new method in the selected class
                var randomMethod = new MethodDefinition(Renaming.GenerateUniqueName(), MethodAttributes.Private | MethodAttributes.Virtual | MethodAttributes.HideBySig, voidType);
                randomMethod.Parameters.Add(new ParameterDefinition("param" + i, ParameterAttributes.None, stringType));

                var randomIlProcessor = randomMethod.Body.GetILProcessor();
                randomIlProcessor.Append(randomIlProcessor.Create(OpCodes.Ldarg_0)); // Load the parameter

                // Randomly choose to add if-else or switch statement
                var action = Extensions.IntRng(0, 6);
                switch (action)
                {
                    case 0:
                        Statements.AddIfElse(randomMethod);
                        Statements.AddIfElse(randomMethod);
                        break;
                    case 1:
                        Statements.AddSwitch(randomMethod);
                        Statements.AddSwitch(randomMethod);
                        break;
                    case 2:
                        Statements.AddIfElse(randomMethod);
                        Statements.AddIfElse(randomMethod);
                        Statements.AddSwitch(randomMethod);
                        break;
                    case 3:
                        Statements.AddSwitch(randomMethod);
                        Statements.AddSwitch(randomMethod);
                        Statements.AddIfElse(randomMethod);
                        break;
                    case 4:
                        Statements.AddSwitch(randomMethod);
                        Statements.AddIfElse(randomMethod);
                        break;
                    case 5:
                        Statements.AddIfElse(randomMethod);
                        Statements.AddSwitch(randomMethod);
                        break;
                    default:
                        Statements.AddIfElse(randomMethod);
                        Statements.AddSwitch(randomMethod);
                        Statements.AddIfElse(randomMethod);
                        break;
                }

                randomIlProcessor.Append(randomIlProcessor.Create(OpCodes.Ret)); // Return

                randomClass.Methods.Add(randomMethod);
                methods.Add(randomMethod);
            }

            // Create tree-like structure
            for (int i = 0; i < methods.Count; i++)
            {
                var method = methods[i];
                var methodIlProcessor = method.Body.GetILProcessor();

                if (2 * i + 1 < methods.Count)
                {
                    methodIlProcessor.InsertBefore(methodIlProcessor.Body.Instructions.Last(), methodIlProcessor.Create(OpCodes.Ldarg_0)); // Load the parameter
                    methodIlProcessor.InsertBefore(methodIlProcessor.Body.Instructions.Last(), methodIlProcessor.Create(OpCodes.Call, methods[2 * i + 1])); // Call left child
                }

                if (2 * i + 2 < methods.Count)
                {
                    methodIlProcessor.InsertBefore(methodIlProcessor.Body.Instructions.Last(), methodIlProcessor.Create(OpCodes.Ldarg_0)); // Load the parameter
                    methodIlProcessor.InsertBefore(methodIlProcessor.Body.Instructions.Last(), methodIlProcessor.Create(OpCodes.Call, methods[2 * i + 2])); // Call right child
                }
            }

            if (methods.Count > 0)
            {
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_0)); // Load the parameter
                ilProcessor.Append(ilProcessor.Create(OpCodes.Call, methods[0])); // Call the root method
            }

            ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_I4, Extensions.IntRng(-1000, 1000))); // Load constant
            ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_I4, Extensions.IntRng(-1000, 1000))); // Load constant
            ilProcessor.Append(ilProcessor.Create(OpCodes.Add)); // Add the two constants
            ilProcessor.Append(ilProcessor.Create(OpCodes.Stloc, resultVariable)); // Store the result in a local variable
            ilProcessor.Append(ilProcessor.Create(OpCodes.Ldloc, resultVariable)); // Load the local variable onto the stack
            ilProcessor.Append(ilProcessor.Create(OpCodes.Ret)); // Return the value

            // Add the junk method to the type
            type.Methods.Add(junkMethod);
            Console.WriteLine($"[CodeInjection] Added junk method {junkMethod.Name} to class {type.Name}");
        }

        public static void JunkClass(AssemblyDefinition assembly, TypeDefinition type, int methods = 5, int fields = 3)
        {
            var module = assembly.MainModule;

            // Add fields to the class
            for (int i = 0; i < fields; i++)
            {
                var field = CreateJunkField(module, Renaming.GenerateUniqueName(), Extensions.SysRng());
                type.Fields.Add(field);
            }

            // Add junk methods to the class
            for (int i = 0; i < methods; i++)
            {
                var junkMethod = CreateJunkMethod(module, Renaming.GenerateUniqueName(), Extensions.IntRng(), Extensions.IntRng(), 0, Extensions.SysRng());
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
                junkMethod.Parameters.Add(new ParameterDefinition(Renaming.GenerateUniqueName(), ParameterAttributes.None, pType));


            // Create the method body
            var ilProcessor = junkMethod.Body.GetILProcessor();

            // Add variables to the method body
            for (int i = 0; i < variables; i++)
                junkMethod.Body.Variables.Add(new VariableDefinition(pType));


            if (parameters > 0)
            {
                // Initialize variables with parameter values or constants
                for (int i = 0; i < variables; i++)
                {
                    ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg, i % parameters)); // Load the parameter
                    ilProcessor.Append(ilProcessor.Create(OpCodes.Stloc, i)); // Store it in the variable
                }
                // Perform some operations using the variables and parameters
                for (int i = 0; i < variables; i++)
                {
                    ilProcessor.Append(ilProcessor.Create(OpCodes.Ldloc, i)); // Load the variable
                    ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg, i % parameters)); // Load the parameter
                    ilProcessor.Append(ilProcessor.Create(OpCodes.Add)); // Add the parameter to the variable
                    ilProcessor.Append(ilProcessor.Create(OpCodes.Stloc, i)); // Store the result back in the variable
                }
            }
            var resultVariable = new VariableDefinition(pType);

            ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_I4, Extensions.IntRng(-1000, 100))); // Load constant
            ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_I4, Extensions.IntRng(-100, 1000))); // Load constant
            ilProcessor.Append(ilProcessor.Create(OpCodes.Add)); // Add the two constants
            ilProcessor.Append(ilProcessor.Create(OpCodes.Stloc, resultVariable)); // Store the result in a local variable
            ilProcessor.Append(ilProcessor.Create(OpCodes.Ldloc, resultVariable)); // Load the local variable onto the stack
            ilProcessor.Append(ilProcessor.Create(OpCodes.Ret)); // Return the value

            // Add some junk instructions
            ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_I4, Extensions.IntRng(-100, 1000))); // Load constant
            ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_I4, Extensions.IntRng(-1000, 100))); // Load constant 
            ilProcessor.Append(ilProcessor.Create(OpCodes.Rem)); // Remainder of the two constants
            ilProcessor.Append(ilProcessor.Create(OpCodes.Pop)); // Pop the result from the stack
            ilProcessor.GetReturnType(methodType);

            junkMethod.Body.Variables.Add(resultVariable);

            return junkMethod;
        }
    }
}
