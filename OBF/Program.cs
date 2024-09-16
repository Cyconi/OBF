using Mono.Cecil;
using Mono.Cecil.Cil;
using OBF.Algorithms;

namespace OBF;

internal class Program
{
    public static void Main(string[] args)
    {
        var dllFilePath = Directory.GetFiles(Directory.GetParent(Environment.CurrentDirectory)?.FullName, "*.dll").FirstOrDefault();

        if (dllFilePath != null)
        {
            Console.WriteLine($"Found DLL: {dllFilePath}");
            Obfuscate(dllFilePath);
        }
        else
            Console.WriteLine("No DLL file found in the parent directory.");
        
    }
    public static void ObfuscateControlFlow(MethodDefinition method)
    {
        var ilProcessor = method.Body.GetILProcessor();
        var instructions = method.Body.Instructions;

        // Example: Adding a simple jump to obfuscate control flow
        var firstInstruction = instructions.First();
        var lastInstruction = instructions.Last();

        // Create a new instruction to jump to the first instruction
        var jumpToFirst = ilProcessor.Create(OpCodes.Br, firstInstruction);
        ilProcessor.InsertBefore(lastInstruction, jumpToFirst);

        // Create a new instruction to jump to the last instruction
        var jumpToLast = ilProcessor.Create(OpCodes.Br, lastInstruction);
        ilProcessor.InsertBefore(firstInstruction, jumpToLast);
    }

    public static void Obfuscate(string dllPath)
    {
        Console.WriteLine($"Obfuscating DLL: {dllPath}");

        AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(dllPath);
        foreach (TypeDefinition type in assembly.MainModule.Types)
        {
            type.Name = Renaming.GenerateUniqueName();

            foreach (MethodDefinition method in type.Methods)
            {
                method.Name = Renaming.GenerateUniqueName();
                ObfuscateControlFlow(method); // Apply control flow obfuscation
            }

            foreach (PropertyDefinition property in type.Properties)
            {
                property.Name = Renaming.GenerateUniqueName();
            }

            foreach (FieldDefinition field in type.Fields)
            {
                field.Name = Renaming.GenerateUniqueName();
            }
        }

        string newDllPath = Path.Combine(Path.GetDirectoryName(dllPath), Path.GetFileNameWithoutExtension(dllPath) + "_OBF" + Path.GetExtension(dllPath));

        assembly.Write(newDllPath);
        Console.WriteLine($"Obfuscated DLL written to: {newDllPath}");
    }

    /*public static void Obfuscate(string dllPath)
    {
        Console.WriteLine($"Obfuscating DLL: {dllPath}");

        AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(dllPath);
        foreach (TypeDefinition type in assembly.MainModule.Types)
        {
            type.Name = Algorithms.Renaming.GenerateUniqueName();

            foreach (MethodDefinition method in type.Methods)
            {
                method.Name = Algorithms.Renaming.GenerateUniqueName();
            }

            foreach (PropertyDefinition property in type.Properties)
            {
                property.Name = Algorithms.Renaming.GenerateUniqueName();
            }

            foreach (FieldDefinition field in type.Fields)
            {
                field.Name = Algorithms.Renaming.GenerateUniqueName();
            }
        }

        string newDllPath = Path.Combine(Path.GetDirectoryName(dllPath), Path.GetFileNameWithoutExtension(dllPath) + "_OBF" + Path.GetExtension(dllPath));

        assembly.Write(newDllPath);
        Console.WriteLine($"Obfuscated DLL written to: {newDllPath}");
    }*/
}
