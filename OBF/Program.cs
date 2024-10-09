using Mono.Cecil;
using Mono.Cecil.Cil;
using OBF.Modules;
using OBF.ILProcessing;
using OBF.Modules;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;

namespace OBF;

internal class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Enter the name of the DLL:");
        var dll = Console.ReadLine();

        // Ensure the DLL name ends with .dll
        if (!dll.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            dll += ".dll";

        var obfDirectory = Path.Combine(Directory.GetParent(Environment.CurrentDirectory)?.FullName, "OBF");
        var dllFilePath = Directory.GetFiles(obfDirectory, dll).FirstOrDefault();

        if (dllFilePath != null)
        {
            Console.WriteLine($"Found DLL: {dllFilePath}");
            Obfuscate(dllFilePath);
        }
        else
            Console.WriteLine("No DLL file found in the OBF directory with the given name.");
        
    }
    public static void Obfuscate(string dllPath)
    {
        Console.WriteLine($"Obfuscating DLL: {dllPath}");

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(Path.GetDirectoryName(dllPath));
        var readerParameters = new ReaderParameters { AssemblyResolver = resolver };
        AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(dllPath, readerParameters);
        Renaming.OriginalAssembly = assembly;

        //AntiDebug.InitAntiDebug(assembly);

        StringEncryption.AddDecryptionMethod(assembly);  // kinda works
        StringEncryption.EncryptStrings(assembly); // maybe?

        //CodeInjection.InjectCode(assembly); // works, want to add class injection
        //ControlFlow.CtrlFlow(assembly); // need work

        //Renaming.RenameAssembly(assembly); // works, not sure i can do much more

        string newDllPath = Path.Combine(Path.Combine(Directory.GetParent(Environment.CurrentDirectory).FullName, Path.GetFileNameWithoutExtension(dllPath) + "_OBF" + Path.GetExtension(dllPath)));
        string additionalPath = @"D:\SteamLibrary\steamapps\common\VRChat\Hexed\Settings\UnityLoader\VRChat\Cheats\" + Path.GetFileName(newDllPath);

        assembly.Write(newDllPath);
        Console.WriteLine($"Obfuscated DLL written to: {newDllPath}");

        File.Copy(newDllPath, additionalPath, true);
        Console.WriteLine($"Obfuscated DLL copied to: {additionalPath}");

        Console.Write("\nPress any key to close this window . . .");
        Console.ReadKey();
    }
}

