using Mono.Cecil;
using Mono.Cecil.Cil;
using OBF.Modules;
using OBF.ILProcessing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Runtime.CompilerServices;
using CustomAttributeNamedArgument = Mono.Cecil.CustomAttributeNamedArgument;

namespace OBF;

internal class Program
{
    public static void Main(string[] args)
    {
        string dll;

        if (args.Length > 0 && File.Exists(args[0]))
            dll = args[0];
        else
        {
            Console.WriteLine("Enter the name of the DLL:");
            dll = Console.ReadLine();
            if (dll == null)
                return;

            // Ensure the DLL name ends with .dll
            if (!dll.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                dll += ".dll";

            var obfDirectory = Path.Combine(Directory.GetParent(Environment.CurrentDirectory)?.FullName, "OBF");
            var dllFilePath = Directory.GetFiles(obfDirectory, dll).FirstOrDefault();
            if (dllFilePath != null)
            {
                Console.WriteLine($"Found DLL: {dllFilePath}");
                ProcessDll(dllFilePath);
            }
            else
                Console.WriteLine("No DLL file found in the OBF directory with the given name.");
        }

        if (dll != null && File.Exists(dll))
        {
            Console.WriteLine($"Found DLL: {dll}");
            ProcessDll(dll);
        }
    }

    public static void ProcessDll(string dllPath)
    {
        Console.WriteLine($"Processing DLL: {dllPath}");

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(Path.Combine(Path.GetDirectoryName(dllPath), "..", "deps"));
        resolver.AddSearchDirectory(Path.GetDirectoryName(dllPath));
        var readerParameters = new ReaderParameters { AssemblyResolver = resolver };
        AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(dllPath, readerParameters);

        // Obfuscate
        Obfuscate(assembly);
        
        // Write the modified and obfuscated assembly to a new file
        string newDllPath = Path.Combine(Path.GetDirectoryName(dllPath), Path.GetFileNameWithoutExtension(dllPath) + "_OBF" + Path.GetExtension(dllPath));
        assembly.Write(newDllPath);
        Console.WriteLine($"Processed DLL written to: {newDllPath}");

        // Optionally copy to an additional path
        string additionalPath = @"D:\SteamLibrary\steamapps\common\VRChat\Mods\" + Path.GetFileName(newDllPath);
        try
        {
            File.Copy(newDllPath, additionalPath, true);
            Console.WriteLine($"Processed DLL copied to: {additionalPath}");
        }
        catch { Console.WriteLine($"Path does not exist: {additionalPath}"); }

        if (Debugger.IsAttached)
            return;

        Console.Write("\nPress any key to close this window . . .");
        Console.ReadLine();
    }

    public static void ModifyAttributes(AssemblyDefinition assembly)
    {
        Console.WriteLine("Modifying Attributes in Assembly");

        // Essential attributes to keep
        var attributesToKeep = new[]
        {
            // Assembly metadata
            "AssemblyTitleAttribute",
            "AssemblyVersionAttribute",
            "AssemblyFileVersionAttribute",
            "AssemblyCompanyAttribute",
            "AssemblyProductAttribute",
            "AssemblyCopyrightAttribute",
            "AssemblyDescriptionAttribute",
            "AssemblyConfigurationAttribute",

            // Runtime + Compatibility
            "TargetFrameworkAttribute",
            "RuntimeCompatibilityAttribute",
            "AssemblyMetadataAttribute",
            "CompilationRelaxationsAttribute",

            // Security and IL behavior
            "SuppressIldasmAttribute",
            "SecurityPermissionAttribute",
            "MethodImplAttribute",

            // Obfuscation marker
            "ObfuscationAttribute"
        };

        // Remove non-essential attributes
        var attributesToRemove = assembly.CustomAttributes
            .Where(attr => !attributesToKeep.Contains(attr.AttributeType.Name))
            .ToList();

        foreach (var attr in attributesToRemove)
        {
            assembly.CustomAttributes.Remove(attr);
            Console.WriteLine($"Removed Attribute: {attr.AttributeType.Name}");
        }

        // Modify AssemblyTitle for obfuscation branding
        foreach (var attr in assembly.CustomAttributes)
        {
            if (attr.AttributeType.Name == "AssemblyTitleAttribute")
            {
                attr.ConstructorArguments.Clear();
                attr.ConstructorArguments.Add(new CustomAttributeArgument(
                    assembly.MainModule.TypeSystem.String, "EXO"));
                Console.WriteLine("Modified AssemblyTitleAttribute to: EXO");
            }
        }

        // Add SuppressIldasmAttribute to discourage IL inspection
        var suppressIldasmCtor = typeof(SuppressIldasmAttribute).GetConstructor(Type.EmptyTypes);
        if (suppressIldasmCtor != null)
        {
            var suppressIldasmAttr = new CustomAttribute(assembly.MainModule.ImportReference(suppressIldasmCtor));
            assembly.CustomAttributes.Add(suppressIldasmAttr);
            Console.WriteLine("Added SuppressIldasmAttribute");
        }

        // Add generic obfuscation marker
        var obfuscationCtor = typeof(ObfuscationAttribute).GetConstructor(Type.EmptyTypes);
        if (obfuscationCtor != null)
        {
            var obfuscationAttr = new CustomAttribute(assembly.MainModule.ImportReference(obfuscationCtor));
            obfuscationAttr.Properties.Add(new CustomAttributeNamedArgument("Exclude", new CustomAttributeArgument(
                assembly.MainModule.TypeSystem.Boolean, false)));
            obfuscationAttr.Properties.Add(new CustomAttributeNamedArgument("ApplyToMembers", new CustomAttributeArgument(
                assembly.MainModule.TypeSystem.Boolean, true)));
            obfuscationAttr.Properties.Add(new CustomAttributeNamedArgument("Feature", new CustomAttributeArgument(
                assembly.MainModule.TypeSystem.String, "all")));
            assembly.CustomAttributes.Add(obfuscationAttr);
            Console.WriteLine("Added ObfuscationAttribute");
        }

        // Add MethodImplAttribute with NoOptimization
        var methodImplCtor = typeof(MethodImplAttribute).GetConstructor(new[] { typeof(MethodImplOptions) });
        if (methodImplCtor != null)
        {
            var methodImplAttr = new CustomAttribute(assembly.MainModule.ImportReference(methodImplCtor));
            methodImplAttr.ConstructorArguments.Add(new CustomAttributeArgument(
                assembly.MainModule.TypeSystem.Int32, (int)MethodImplOptions.NoOptimization));
            assembly.CustomAttributes.Add(methodImplAttr);
            Console.WriteLine("Added MethodImplOptions.NoOptimization");
        }
    }

    public static void Obfuscate(AssemblyDefinition assembly)
    {
        Console.WriteLine("Obfuscating Assembly");

        // Modify Attributes
        //ModifyAttributes(assembly);

        //AntiDebug.InitAntiDebug(assembly);

        //StringEncryption.EncryptStrings(assembly); // has issues with some methods (nested?)

        CodeInjection.InjectMethods(assembly); // works, want to add class injection
        CodeInjection.JunkWithMethodCalls(assembly); // need work

        Renaming.RenameAssembly(assembly); // works, not sure I can do much more

        Renaming.UpdateMelonInfoAttribute(assembly);
    }
}
