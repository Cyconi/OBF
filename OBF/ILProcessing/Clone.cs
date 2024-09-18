using Mono.Cecil.Cil;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBF.ILProcessing
{
    internal class Clone
    {
        public static void MethodbyName(ModuleDefinition module, string className, string methodName)
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
        public static void RandomMethod(ModuleDefinition module)
        {
            Random random = new Random();

            // Select a random class
            var types = module.Types
                .Where(t => t.HasMethods && t.Methods.Any(m => m.Body != null /*&& !m.IsVirtual && !m.HasOverrides*/))
                .ToList();

            if (types.Count == 0)
            {
                Console.WriteLine("No classes with suitable methods found in the module.");
                throw new ArgumentException("No classes with methods found in the module.");
            }

            MethodDefinition? randomMethod = null;

            foreach (var randomType in types.OrderBy(t => random.Next())) // Shuffle the types list
            {
                // Select a random method from the selected class
                var methods = randomType.Methods
                    .Where(m => m.Body != null && !m.IsConstructor && !m.Name.Contains('.'))
                    .OrderBy(m => random.Next()) // Shuffle the methods list
                    .ToList();

                if (methods.Count > 0)
                {
                    randomMethod = methods.First();
                    break;
                }
            }

            if (randomMethod == null)
            {
                Console.WriteLine("Failed to find a suitable method to clone.");
                throw new InvalidOperationException("Failed to find a suitable method to clone.");
            }

            // Select a random target class to clone the method to
            var targetTypes = types.Where(t => t.IsClass && !t.IsEnum && !t.IsInterface && !t.IsCompilerGenerated() && !t.IsDelegate()).ToList();
            if (targetTypes.Count == 0)
            {
                Console.WriteLine("No suitable target classes found.");
                throw new InvalidOperationException("No suitable target classes found.");
            }

            var targetType = targetTypes[random.Next(targetTypes.Count)];

            // Create a new method definition
            var clonedMethod = new MethodDefinition(randomMethod.Name + "_Clone",
                randomMethod.Attributes, randomMethod.ReturnType);

            // Copy parameters
            foreach (var parameter in randomMethod.Parameters)
                clonedMethod.Parameters.Add(new ParameterDefinition(parameter.Name, parameter.Attributes, parameter.ParameterType));

            // Copy method body
            var ilProcessor = clonedMethod.Body.GetILProcessor();
            var body = randomMethod.Body;

            // Copy variables if they exist
            if (body.Variables != null)
                foreach (var variable in body.Variables)
                    clonedMethod.Body.Variables.Add(new VariableDefinition(variable.VariableType));

            // Copy instructions
            foreach (var instruction in body.Instructions)
                ilProcessor.Append(instruction);

            // Copy exception handlers if they exist
            if (body.ExceptionHandlers != null)
                foreach (var handler in body.ExceptionHandlers)
                    clonedMethod.Body.ExceptionHandlers.Add(handler);

            // Add the cloned method to the target class
            targetType.Methods.Add(clonedMethod);

            Console.WriteLine($"Cloned method {randomMethod.Name} to {targetType.Name}");
        }
    }
}
