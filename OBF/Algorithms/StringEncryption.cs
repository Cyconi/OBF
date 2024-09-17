using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace OBF.Algorithms;

public static class StringEncryption
{
    public static void InjectClass(AssemblyDefinition assembly)
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
        ilProcessorOnDecrypt.Append(ilProcessorOnDecrypt.Create(OpCodes.Newobj, module.ImportReference(typeof(CryptoStream).GetConstructor(new[] { typeof(Stream), typeof(ICryptoTransform), typeof(CryptoStreamMode) }))));
        ilProcessorOnDecrypt.Append(ilProcessorOnDecrypt.Create(OpCodes.Stloc_2));
        ilProcessorOnDecrypt.Append(ilProcessorOnDecrypt.Create(OpCodes.Ldloc_2));
        ilProcessorOnDecrypt.Append(ilProcessorOnDecrypt.Create(OpCodes.Ldarg_0));
        ilProcessorOnDecrypt.Append(ilProcessorOnDecrypt.Create(OpCodes.Ldc_I4_0));
        ilProcessorOnDecrypt.Append(ilProcessorOnDecrypt.Create(OpCodes.Ldarg_0));
        ilProcessorOnDecrypt.Append(ilProcessorOnDecrypt.Create(OpCodes.Ldlen));
        ilProcessorOnDecrypt.Append(ilProcessorOnDecrypt.Create(OpCodes.Conv_I4));
        ilProcessorOnDecrypt.Append(ilProcessorOnDecrypt.Create(OpCodes.Callvirt, module.ImportReference(typeof(CryptoStream).GetMethod("Write", new[] { typeof(byte[]), typeof(int), typeof(int) }))));
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
        ilProcessor.Append(ilProcessor.Create(OpCodes.Call, module.ImportReference(typeof(Convert).GetMethod("FromBase64String", new[] { typeof(string) }))));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Stloc_0));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_1));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Call, module.ImportReference(typeof(Convert).GetMethod("FromBase64String", new[] { typeof(string) }))));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Stloc_1));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_2));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Call, module.ImportReference(typeof(Convert).GetMethod("FromBase64String", new[] { typeof(string) }))));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Stloc_2));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldloc_0));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldloc_1));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldloc_2));
        encryptionClass.Methods.Add(decryptMethod);
        module.Types.Add(encryptionClass);
        var onDecryptMethod = module.ImportReference(onDecryptMethodDef);
        ilProcessor.Append(ilProcessor.Create(OpCodes.Call, onDecryptMethod));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Call, module.ImportReference(typeof(Encoding).GetProperty("UTF8").GetGetMethod())));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Callvirt, module.ImportReference(typeof(Encoding).GetMethod("GetString", new[] { typeof(byte[]) }))));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));
        
    }

    /*public static void InjectClass1(AssemblyDefinition assembly)
    {
        var module = assembly.MainModule;
        var encryptionClass = new TypeDefinition("Embed", "EmbeddedStringEncryption",
            TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed,
            module.TypeSystem.Object);

        // Define the OnDecrypt method
        var onDecryptMethodDef = new MethodDefinition("OnDecrypt", MethodAttributes.Public | MethodAttributes.Static, module.TypeSystem.Byte.MakeArrayType());
        onDecryptMethodDef.Parameters.Add(new ParameterDefinition("bytes", ParameterAttributes.None, module.TypeSystem.Byte.MakeArrayType()));
        onDecryptMethodDef.Parameters.Add(new ParameterDefinition("key", ParameterAttributes.None, module.TypeSystem.Byte.MakeArrayType()));
        onDecryptMethodDef.Parameters.Add(new ParameterDefinition("iv", ParameterAttributes.None, module.TypeSystem.Byte.MakeArrayType()));

        var ilProcessorOnDecrypt = onDecryptMethodDef.Body.GetILProcessor();
        ilProcessorOnDecrypt.Append(ilProcessorOnDecrypt.Create(OpCodes.Ret));

        encryptionClass.Methods.Add(onDecryptMethodDef);

        // Define the DecryptString method
        var decryptMethod = new MethodDefinition("DecryptString", MethodAttributes.Public | MethodAttributes.Static, module.TypeSystem.String);
        decryptMethod.Parameters.Add(new ParameterDefinition("encryptedText", ParameterAttributes.None, module.TypeSystem.String));
        decryptMethod.Parameters.Add(new ParameterDefinition("keyString", ParameterAttributes.None, module.TypeSystem.String));
        decryptMethod.Parameters.Add(new ParameterDefinition("ivString", ParameterAttributes.None, module.TypeSystem.String));

        var ilProcessor = decryptMethod.Body.GetILProcessor();
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));

        encryptionClass.Methods.Add(decryptMethod);
        module.Types.Add(encryptionClass);

        // Import the OnDecrypt method reference
        var onDecryptMethod = module.ImportReference(onDecryptMethodDef);

        // Add a simple call to the OnDecrypt method in the DecryptString method
        ilProcessor.InsertBefore(decryptMethod.Body.Instructions[0], ilProcessor.Create(OpCodes.Call, onDecryptMethod));
    }*/

    public static void EncryptStrings(AssemblyDefinition assembly)
    {
        Console.WriteLine("Starting string encryption...");

        foreach (TypeDefinition type in assembly.MainModule.Types)
        {
            Console.WriteLine($"Processing type: {type.Name}");

            foreach (MethodDefinition method in type.Methods)
            {
                if (method.Body == null)
                    continue;

                Console.WriteLine($"Processing method: {method.Name}");

                var ilProcessor = method.Body.GetILProcessor();
                var decryptMethod = assembly.MainModule.Types
                    .First(t => t.Name == "EmbeddedStringEncryption")
                    .Methods.First(m => m.Name == "DecryptString");

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

                        // Create new instructions
                        var newInstructions = new List<Instruction>
                        {
                            ilProcessor.Create(OpCodes.Ldstr, encryptedString),
                            ilProcessor.Create(OpCodes.Ldstr, keyString),
                            ilProcessor.Create(OpCodes.Ldstr, ivString),
                            ilProcessor.Create(OpCodes.Call, assembly.MainModule.ImportReference(decryptMethod))
                        };

                        // Replace the original instruction with the new instructions
                        ilProcessor.Replace(instruction, newInstructions[0]);
                        for (int j = 1; j < newInstructions.Count; j++)
                        {
                            ilProcessor.InsertAfter(newInstructions[j - 1], newInstructions[j]);
                        }

                        Console.WriteLine("String encrypted and decryption call inserted.");
                        break; // Exit the loop after processing the string
                    }
                }
            }
        }

        Console.WriteLine("String encryption completed.");
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