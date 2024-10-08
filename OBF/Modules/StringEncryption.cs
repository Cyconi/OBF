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
    public static void ing(AssemblyDefinition assembly)
    {
        var module = assembly.MainModule;
        var encryptionClass = new TypeDefinition("Embed", "EmbeddedStringEncryption", TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed, module.TypeSystem.Object);
        var onDecryptMethodDef = new MethodDefinition("OnDecrypt", MethodAttributes.Public | MethodAttributes.Static, module.TypeSystem.Byte.MakeArrayType());
        onDecryptMethodDef.Parameters.Add(new ParameterDefinition("bytes", ParameterAttributes.None, module.TypeSystem.Byte.MakeArrayType()));
        onDecryptMethodDef.Parameters.Add(new ParameterDefinition("key", ParameterAttributes.None, module.TypeSystem.Byte.MakeArrayType()));
        onDecryptMethodDef.Parameters.Add(new ParameterDefinition("iv", ParameterAttributes.None, module.TypeSystem.Byte.MakeArrayType()));
        var ilProcessorOnDecrypt = onDecryptMethodDef.Body.GetILProcessor();
        ilProcessorOnDecrypt.Body.Variables.Add(new VariableDefinition(module.ImportReference(typeof(Aes))));
        ilProcessorOnDecrypt.Body.Variables.Add(new VariableDefinition(module.ImportReference(typeof(MemoryStream))));
        ilProcessorOnDecrypt.Body.Variables.Add(new VariableDefinition(module.ImportReference(typeof(CryptoStream))));
        ilProcessorOnDecrypt.Append(ilProcessorOnDecrypt.Create(OpCodes.Call, module.ImportReference(typeof(Aes).GetMethod("Create", Type.EmptyTypes))));
        ilProcessorOnDecrypt.Append(ilProcessorOnDecrypt.Create(OpCodes.Stloc_0));
        ilProcessorOnDecrypt.Append(ilProcessorOnDecrypt.Create(OpCodes.Ldloc_0));
        ilProcessorOnDecrypt.Append(ilProcessorOnDecrypt.Create(OpCodes.Ldarg_1));
        ilProcessorOnDecrypt.Append(ilProcessorOnDecrypt.Create(OpCodes.Callvirt, module.ImportReference(typeof(Aes).GetProperty("Key").GetSetMethod())));
        ilProcessorOnDecrypt.Append(ilProcessorOnDecrypt.Create(OpCodes.Ldloc_0));
        ilProcessorOnDecrypt.Append(ilProcessorOnDecrypt.Create(OpCodes.Ldarg_2));
        ilProcessorOnDecrypt.Append(ilProcessorOnDecrypt.Create(OpCodes.Callvirt, module.ImportReference(typeof(Aes).GetProperty("IV").GetSetMethod())));
        ilProcessorOnDecrypt.Append(ilProcessorOnDecrypt.Create(OpCodes.Newobj, module.ImportReference(typeof(MemoryStream).GetConstructor(Type.EmptyTypes))));
        ilProcessorOnDecrypt.Append(ilProcessorOnDecrypt.Create(OpCodes.Stloc_1));
        ilProcessorOnDecrypt.Append(ilProcessorOnDecrypt.Create(OpCodes.Ldloc_1));
        ilProcessorOnDecrypt.Append(ilProcessorOnDecrypt.Create(OpCodes.Ldloc_0));
        ilProcessorOnDecrypt.Append(ilProcessorOnDecrypt.Create(OpCodes.Callvirt, module.ImportReference(typeof(Aes).GetMethod("CreateDecryptor", Type.EmptyTypes))));
        ilProcessorOnDecrypt.Append(ilProcessorOnDecrypt.Create(OpCodes.Ldc_I4_1));
        ilProcessorOnDecrypt.Append(ilProcessorOnDecrypt.Create(OpCodes.Newobj, module.ImportReference(typeof(CryptoStream).GetConstructor([typeof(Stream), typeof(ICryptoTransform), typeof(CryptoStreamMode)]))));
        ilProcessorOnDecrypt.Append(ilProcessorOnDecrypt.Create(OpCodes.Stloc_2));
        ilProcessorOnDecrypt.Append(ilProcessorOnDecrypt.Create(OpCodes.Ldloc_2));
        ilProcessorOnDecrypt.Append(ilProcessorOnDecrypt.Create(OpCodes.Ldarg_0));
        ilProcessorOnDecrypt.Append(ilProcessorOnDecrypt.Create(OpCodes.Ldc_I4_0));
        ilProcessorOnDecrypt.Append(ilProcessorOnDecrypt.Create(OpCodes.Ldarg_0));
        ilProcessorOnDecrypt.Append(ilProcessorOnDecrypt.Create(OpCodes.Ldlen));
        ilProcessorOnDecrypt.Append(ilProcessorOnDecrypt.Create(OpCodes.Conv_I4));
        ilProcessorOnDecrypt.Append(ilProcessorOnDecrypt.Create(OpCodes.Callvirt, module.ImportReference(typeof(CryptoStream).GetMethod("Write", [typeof(byte[]), typeof(int), typeof(int)]))));
        ilProcessorOnDecrypt.Append(ilProcessorOnDecrypt.Create(OpCodes.Ldloc_2));
        ilProcessorOnDecrypt.Append(ilProcessorOnDecrypt.Create(OpCodes.Callvirt, module.ImportReference(typeof(CryptoStream).GetMethod("FlushFinalBlock", Type.EmptyTypes))));
        ilProcessorOnDecrypt.Append(ilProcessorOnDecrypt.Create(OpCodes.Ldloc_1));
        ilProcessorOnDecrypt.Append(ilProcessorOnDecrypt.Create(OpCodes.Callvirt, module.ImportReference(typeof(MemoryStream).GetMethod("ToArray", Type.EmptyTypes))));
        ilProcessorOnDecrypt.Append(ilProcessorOnDecrypt.Create(OpCodes.Ret));
        encryptionClass.Methods.Add(onDecryptMethodDef);
        var decryptMethod = new MethodDefinition("DecryptString", MethodAttributes.Public | MethodAttributes.Static, module.TypeSystem.String);
        decryptMethod.Parameters.Add(new ParameterDefinition("encryptedText", ParameterAttributes.None, module.TypeSystem.String));
        decryptMethod.Parameters.Add(new ParameterDefinition("keyString", ParameterAttributes.None, module.TypeSystem.String));
        decryptMethod.Parameters.Add(new ParameterDefinition("ivString", ParameterAttributes.None, module.TypeSystem.String));
        var ilProcessor = decryptMethod.Body.GetILProcessor();
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
        encryptionClass.Methods.Add(decryptMethod);
        module.Types.Add(encryptionClass);
        var onDecryptMethod = module.ImportReference(onDecryptMethodDef);
        ilProcessor.Append(ilProcessor.Create(OpCodes.Call, onDecryptMethod));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Call, module.ImportReference(typeof(Encoding).GetProperty("UTF8").GetGetMethod())));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Callvirt, module.ImportReference(typeof(Encoding).GetMethod("GetString", [typeof(byte[])]))));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));
        
    }
    public static void AddDecryptionMethod(AssemblyDefinition assembly)
    {
        var module = assembly.MainModule;
        var encryptionClass = CreateEncryptionClass(module);

        var onDecryptMethodDef = CreateOnDecryptMethod(module);
        encryptionClass.Methods.Add(onDecryptMethodDef);

        var decryptMethod = CreateDecryptStringMethod(module, onDecryptMethodDef);
        encryptionClass.Methods.Add(decryptMethod);

        module.Types.Add(encryptionClass);
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
    public static void EncryptStrings(AssemblyDefinition assembly)
    {
        Console.WriteLine("Starting string encryption...");

        foreach (TypeDefinition type in assembly.MainModule.Types)
        {
            ProcessType(type);
        }

        Console.WriteLine("String encryption completed.");
    }

    private static void ProcessType(TypeDefinition type)
    {
        Console.WriteLine($"Processing type: {type.Name}");

        foreach (MethodDefinition method in type.Methods)
        {
            if (method.Body == null)
            {
                Console.WriteLine($"Skipping method with no body: {method.Name}");
                continue;
            }

            Console.WriteLine($"Processing method: {method.Name}");

            var ilProcessor = method.Body.GetILProcessor();
            var decryptMethod = type.Module.Types
                .First(t => t.Name == "EmbeddedStringEncryption")
                .Methods.First(m => m.Name == "DecryptString");

            var processedInstructions = new HashSet<Instruction>();

            for (int i = 0; i < method.Body.Instructions.Count; i++)
            {
                var instruction = method.Body.Instructions[i];
                Console.WriteLine($"{instruction.OpCode.Name} {instruction.Operand}");

                if (instruction.OpCode == OpCodes.Ldstr && !processedInstructions.Contains(instruction))
                {
                    string originalString = (string)instruction.Operand;

                    // Skip interpolated strings
                    if (originalString.Contains('{') || originalString.Contains('}') || string.IsNullOrWhiteSpace(originalString))
                    {
                        Console.WriteLine($"Skipping string: {originalString}");
                        continue;
                    }

                    Console.WriteLine($"Encrypting string: {originalString}");

                    byte[] originalBytes = Encoding.UTF8.GetBytes(originalString);
                    var (encryptedBytes, key, iv) = OnEncrypt(originalBytes);
                    string encryptedString = Convert.ToBase64String(encryptedBytes);
                    string keyString = Convert.ToBase64String(key);
                    string ivString = Convert.ToBase64String(iv);

                    // Create new instructions
                    var newInstructions = new List<Instruction>
                    {
                        ilProcessor.Create(OpCodes.Ldstr, encryptedString),
                        ilProcessor.Create(OpCodes.Ldstr, keyString),
                        ilProcessor.Create(OpCodes.Ldstr, ivString),
                        ilProcessor.Create(OpCodes.Call, type.Module.ImportReference(decryptMethod))
                    };

                    // Replace the original instruction with the new instructions
                    if (encryptedString != null && keyString != null && ivString != null)
                    {
                        ilProcessor.Replace(instruction, newInstructions[0]);
                        for (int j = 1; j < newInstructions.Count; j++)
                            ilProcessor.InsertAfter(newInstructions[j - 1], newInstructions[j]);

                        foreach (var instr in newInstructions)
                            processedInstructions.Add(instr);

                        Console.WriteLine($"String encrypted and decryption call inserted in {type.Name} | {method.Name}");
                    }
                    else
                        Console.WriteLine($"\n\nError replacing instructions: encryptedString: {encryptedString} keyString: {keyString} ivString: {ivString}\n\n");
                }
            }
        }

        // Process nested types (state machines for async methods)
        foreach (var nestedType in type.NestedTypes)
        {
            ProcessType(nestedType);
        }
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
}