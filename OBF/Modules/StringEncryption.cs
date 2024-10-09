using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace OBF.Modules;

public static class StringEncryption
{
    public static void AddDecryptionMethod(AssemblyDefinition assembly)
    {
        try
        {
            var module = assembly.MainModule;
            var encryptionClass = CreateEncryptionClass(module);

            var onDecryptMethodDef = CreateOnDecryptMethod(module);
            encryptionClass.Methods.Add(onDecryptMethodDef);

            var decryptMethod = CreateDecryptStringMethod(module, onDecryptMethodDef);
            encryptionClass.Methods.Add(decryptMethod);

            module.Types.Add(encryptionClass);

            Console.WriteLine("Decryption methods added successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding decryption methods: {ex.Message}");
            throw;
        }
    }
    private static TypeDefinition CreateEncryptionClass(ModuleDefinition module)
    {
        return new TypeDefinition("Embed", "EmbeddedStringEncryption",
            TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed,
            module.TypeSystem.Object);
    }
    private static MethodDefinition CreateOnDecryptMethod(ModuleDefinition module)
    {
        var method = new MethodDefinition("OnDecrypt",
            MethodAttributes.Public | MethodAttributes.Static,
            module.TypeSystem.Byte.MakeArrayType());

        method.Parameters.Add(new ParameterDefinition("bytes", ParameterAttributes.None, module.TypeSystem.Byte.MakeArrayType()));
        method.Parameters.Add(new ParameterDefinition("key", ParameterAttributes.None, module.TypeSystem.Byte.MakeArrayType()));
        method.Parameters.Add(new ParameterDefinition("iv", ParameterAttributes.None, module.TypeSystem.Byte.MakeArrayType()));

        var ilProcessor = method.Body.GetILProcessor();
        ilProcessor.Body.Variables.Add(new VariableDefinition(module.ImportReference(typeof(Aes))));
        ilProcessor.Body.Variables.Add(new VariableDefinition(module.ImportReference(typeof(MemoryStream))));
        ilProcessor.Body.Variables.Add(new VariableDefinition(module.ImportReference(typeof(CryptoStream))));

        ilProcessor.Append(ilProcessor.Create(OpCodes.Call, module.ImportReference(typeof(Aes).GetMethod("Create", Type.EmptyTypes))));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Stloc_0));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldloc_0));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_1));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Callvirt, module.ImportReference(typeof(Aes).GetProperty("Key").GetSetMethod())));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldloc_0));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_2));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Callvirt, module.ImportReference(typeof(Aes).GetProperty("IV").GetSetMethod())));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Newobj, module.ImportReference(typeof(MemoryStream).GetConstructor(Type.EmptyTypes))));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Stloc_1));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldloc_1));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldloc_0));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Callvirt, module.ImportReference(typeof(Aes).GetMethod("CreateDecryptor", Type.EmptyTypes))));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_I4_1));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Newobj, module.ImportReference(typeof(CryptoStream).GetConstructor([typeof(Stream), typeof(ICryptoTransform), typeof(CryptoStreamMode)]))));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Stloc_2));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldloc_2));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_0));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_I4_0));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_0));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldlen));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Conv_I4));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Callvirt, module.ImportReference(typeof(CryptoStream).GetMethod("Write", [typeof(byte[]), typeof(int), typeof(int)]))));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldloc_2));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Callvirt, module.ImportReference(typeof(CryptoStream).GetMethod("FlushFinalBlock", Type.EmptyTypes))));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldloc_1));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Callvirt, module.ImportReference(typeof(MemoryStream).GetMethod("ToArray", Type.EmptyTypes))));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));

        return method;
    }
    private static MethodDefinition CreateDecryptStringMethod(ModuleDefinition module, MethodReference onDecryptMethod)
    {
        var method = new MethodDefinition("DecryptString",
            MethodAttributes.Public | MethodAttributes.Static,
            module.TypeSystem.String);

        method.Parameters.Add(new ParameterDefinition("encryptedText", ParameterAttributes.None, module.TypeSystem.String));
        method.Parameters.Add(new ParameterDefinition("keyString", ParameterAttributes.None, module.TypeSystem.String));
        method.Parameters.Add(new ParameterDefinition("ivString", ParameterAttributes.None, module.TypeSystem.String));

        var ilProcessor = method.Body.GetILProcessor();
        ilProcessor.Body.Variables.Add(new VariableDefinition(module.TypeSystem.Byte.MakeArrayType()));
        ilProcessor.Body.Variables.Add(new VariableDefinition(module.TypeSystem.Byte.MakeArrayType()));
        ilProcessor.Body.Variables.Add(new VariableDefinition(module.TypeSystem.Byte.MakeArrayType()));
        ilProcessor.Body.Variables.Add(new VariableDefinition(module.TypeSystem.Byte.MakeArrayType()));

        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_0));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Call, module.ImportReference(typeof(Convert).GetMethod("FromBase64String", [typeof(string)]))));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Stloc_0));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_1));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Call, module.ImportReference(typeof(Convert).GetMethod("FromBase64String", [typeof(string)]))));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Stloc_1));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_2));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Call, module.ImportReference(typeof(Convert).GetMethod("FromBase64String", [typeof(string)]))));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Stloc_2));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldloc_0));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldloc_1));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldloc_2));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Call, onDecryptMethod));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Stloc_3));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Call, module.ImportReference(typeof(Encoding).GetProperty("UTF8").GetGetMethod())));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldloc_3));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Callvirt, module.ImportReference(typeof(Encoding).GetMethod("GetString", [typeof(byte[])]))));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));

        return method;
    }

    public static List<string> FoundStrings { get; private set; }
    private static MethodDefinition _getMethod;
    private static FieldDefinition _initializedField;
    private static FieldDefinition _listField;
    private static FieldDefinition _keyField;
    private static FieldDefinition _ivField;
    private static MethodDefinition _initializeMethod;
    private static TypeDefinition _type;

    public static bool EncryptStrings(AssemblyDefinition assembly)
    {
        var types = assembly.MainModule.Types;

        FoundStrings = [];
        AddHandler(assembly);

        foreach (var type in types)
            ProcessType(type);

        FinalizeHandler(assembly);
        return true;
    }

    private static void AddHandler(AssemblyDefinition assembly)
    {
        _type = new TypeDefinition("obfuscatus", "StringHandler",
                                   TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AutoClass |
                                   TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit |
                                   TypeAttributes.Abstract, assembly.MainModule.Import(typeof(object)));

        _initializedField = new FieldDefinition("_initialized", FieldAttributes.Private | FieldAttributes.Static,
                                                assembly.MainModule.Import(typeof(bool)));
        _listField = new FieldDefinition("_list", FieldAttributes.Private | FieldAttributes.Static,
                                         assembly.MainModule.Import(typeof(List<string>)));
        _keyField = new FieldDefinition("_key", FieldAttributes.Private | FieldAttributes.Static,
                                        assembly.MainModule.Import(typeof(string)));
        _ivField = new FieldDefinition("_iv", FieldAttributes.Private | FieldAttributes.Static,
                                       assembly.MainModule.Import(typeof(string)));
        _initializeMethod = new MethodDefinition("Initialize", MethodAttributes.Private | MethodAttributes.Static,
                                                 assembly.MainModule.Import(typeof(void)));
        _type.Methods.Add(_initializeMethod);
        _type.Fields.Add(_initializedField);
        _type.Fields.Add(_listField);
        _type.Fields.Add(_keyField);
        _type.Fields.Add(_ivField);

        _getMethod = new MethodDefinition("Get", MethodAttributes.Public | MethodAttributes.Static,
                                          assembly.MainModule.Import(typeof(string)));
        _getMethod.Parameters.Add(new ParameterDefinition(assembly.MainModule.Import(typeof(int))));

        _type.Methods.Add(_getMethod);

        {
            var processor = _getMethod.Body.GetILProcessor();
            processor.Body.InitLocals = true;
            processor.Body.Variables.Add(new VariableDefinition(assembly.MainModule.Import(typeof(string))));
            processor.Body.Variables.Add(new VariableDefinition(assembly.MainModule.Import(typeof(bool))));
            processor.Emit(OpCodes.Ldsfld, _initializedField);
            processor.Emit(OpCodes.Stloc_1);
            processor.Emit(OpCodes.Ldloc_1);
            processor.Emit(OpCodes.Nop);
            processor.Emit(OpCodes.Call, _initializeMethod);
            processor.Emit(OpCodes.Ldsfld, _listField);
            processor.Emit(OpCodes.Ldarg_0);
            processor.Emit(OpCodes.Callvirt, assembly.MainModule.Import(typeof(List<string>).GetMethod("get_Item")));
            processor.Emit(OpCodes.Stloc_0);
            processor.Emit(OpCodes.Ldloc_0);
            processor.Emit(OpCodes.Ret);
            // fix up initialization check
            processor.Replace(processor.Body.Instructions[3],
                              processor.Create(OpCodes.Brtrue_S, processor.Body.Instructions[5]));
        }
    }

    private static void FinalizeHandler(AssemblyDefinition assembly)
    {
        var processor = _initializeMethod.Body.GetILProcessor();
        var (encryptedStrings, key, iv) = EncryptStrings(FoundStrings);
        processor.Emit(OpCodes.Ldc_I4, encryptedStrings.Count);
        processor.Emit(OpCodes.Newobj, assembly.MainModule.Import(typeof(List<string>).GetConstructors()[1]));
        processor.Emit(OpCodes.Stsfld, _listField);
        foreach (var str in encryptedStrings)
        {
            processor.Emit(OpCodes.Ldsfld, _listField);
            processor.Emit(OpCodes.Ldstr, str);
            processor.Emit(OpCodes.Callvirt, assembly.MainModule.Import(typeof(List<string>).GetMethod("Add")));
        }
        processor.Emit(OpCodes.Ldstr, Convert.ToBase64String(key));
        processor.Emit(OpCodes.Stsfld, _keyField);
        processor.Emit(OpCodes.Ldstr, Convert.ToBase64String(iv));
        processor.Emit(OpCodes.Stsfld, _ivField);
        processor.Emit(OpCodes.Ldc_I4_1);
        processor.Emit(OpCodes.Stsfld, _initializedField);
        processor.Emit(OpCodes.Ret);
        assembly.MainModule.Types.Add(_type);
    }

    private static void ProcessType(TypeDefinition type)
    {
        if (!type.HasMethods)
            return;

        foreach (var method in type.Methods)
            ProcessMethod(method);
    }

    private static void ProcessMethod(MethodDefinition method)
    {
        if (!method.HasBody)
            return;

        var processor = method.Body.GetILProcessor();
        var instructionsToReplace = new Dictionary<Instruction, int>();
        foreach (var instruction in method.Body.Instructions)
        {
            if (instruction.OpCode.Code != Code.Ldstr)
                continue;

            if (!(instruction.Operand is string))
                continue;

            FoundStrings.Add(instruction.Operand as string);
            instructionsToReplace.Add(instruction, FoundStrings.Count - 1);
        }

        foreach (var kvp in instructionsToReplace)
        {
            processor.InsertAfter(kvp.Key, processor.Create(OpCodes.Call, _getMethod));
            processor.Replace(kvp.Key, processor.Create(OpCodes.Ldc_I4, kvp.Value));
        }
    }

    private static (List<string>, byte[], byte[]) EncryptStrings(List<string> strings)
    {
        var encryptedStrings = new List<string>();
        byte[] key = null;
        byte[] iv = null;
        foreach (var str in strings)
        {
            var (encrypted, k, i) = OnEncrypt(Encoding.UTF8.GetBytes(str));
            encryptedStrings.Add(Convert.ToBase64String(encrypted));
            key = k;
            iv = i;
        }
        return (encryptedStrings, key, iv);
    }

    public static (byte[], byte[], byte[]) OnEncrypt(byte[] by)
    {
        using Aes aes = Aes.Create();
        aes.GenerateKey();
        aes.GenerateIV();
        using MemoryStream mrms = new();
        using (CryptoStream cryste = new(mrms, aes.CreateEncryptor(), CryptoStreamMode.Write))
        {
            cryste.Write(by, 0, by.Length);
            cryste.FlushFinalBlock();
        }
        return (mrms.ToArray(), aes.Key, aes.IV);
    }
    /*public static void EncryptStrings(AssemblyDefinition assembly)
    {
        Console.WriteLine("Starting string encryption...");

        foreach (TypeDefinition type in assembly.MainModule.Types)
            if (type.Name != "EmbeddedStringEncryption")
                ProcessType(type);

        Console.WriteLine("String encryption completed.");
    }

    private static void ProcessType(TypeDefinition type)
    {
        Console.WriteLine($"Processing type: {type.Name}");

        foreach (MethodDefinition method in type.Methods)
        {
            if (method.Body == null)
                continue;

            Console.WriteLine($"Processing method: {method.Name}");

            var ilProcessor = method.Body.GetILProcessor();
            var decryptMethod = type.Module.Types.First(t => t.Name == "EmbeddedStringEncryption").Methods.First(m => m.Name == "DecryptString");

            var instructionsToReplace = new List<(Instruction, List<Instruction>)>();

            for (int i = 0; i < method.Body.Instructions.Count; i++)
            {
                var instruction = method.Body.Instructions[i];

                if (instruction.OpCode == OpCodes.Ldstr)
                {
                    string originalString = (string)instruction.Operand;

                    Console.WriteLine($"Encrypting string: {originalString}");

                    byte[] originalBytes = Encoding.UTF8.GetBytes(originalString);
                    var (encryptedBytes, key, iv) = OnEncrypt(originalBytes);
                    string encryptedString = Convert.ToBase64String(encryptedBytes);
                    string keyString = Convert.ToBase64String(key);
                    string ivString = Convert.ToBase64String(iv);

                    // Validate the encrypted string, key, and IV
                    if (!string.IsNullOrEmpty(encryptedString) && !string.IsNullOrEmpty(keyString) && !string.IsNullOrEmpty(ivString))
                    {
                        // Create new instructions
                        var newInstructions = new List<Instruction>
                        {
                            ilProcessor.Create(OpCodes.Ldstr, encryptedString),
                            ilProcessor.Create(OpCodes.Ldstr, keyString),
                            ilProcessor.Create(OpCodes.Ldstr, ivString),
                            ilProcessor.Create(OpCodes.Call, type.Module.ImportReference(decryptMethod))
                        };

                        // Validate all new instructions
                        if (newInstructions.All(instr => instr != null && instr.Operand != null))                        
                            instructionsToReplace.Add((instruction, newInstructions));                        
                        else                        
                            Console.WriteLine($"\n\nError: One or more new instructions are invalid. Skipping replacement for: {originalString}\n\n");
                    }
                    else
                        Console.WriteLine($"\n\nError: Invalid encrypted string, key, or IV. Skipping replacement for: {originalString}\n\n");
                }
            }

            // Perform the replacements after collecting all instructions to replace
            foreach (var (originalInstruction, newInstructions) in instructionsToReplace)
            {
                ilProcessor.Replace(originalInstruction, newInstructions[0]);
                for (int j = 1; j < newInstructions.Count; j++)
                    ilProcessor.InsertAfter(newInstructions[j - 1], newInstructions[j]);
            }

            Console.WriteLine($"String encrypted and decryption call inserted in {type.Name} | {method.Name}");
        }

        foreach (var nestedType in type.NestedTypes)
            ProcessType(nestedType);
    }

    public static (byte[], byte[], byte[]) OnEncrypt(byte[] by)
    {
        using Aes aes = Aes.Create();
        aes.GenerateKey();
        aes.GenerateIV();
        using MemoryStream mrms = new();
        using (CryptoStream cryste = new(mrms, aes.CreateEncryptor(), CryptoStreamMode.Write))
        {
            cryste.Write(by, 0, by.Length);
            cryste.FlushFinalBlock();
        }
        return (mrms.ToArray(), aes.Key, aes.IV);
    }*/
}