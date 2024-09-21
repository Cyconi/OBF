using Mono.Cecil.Cil;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil.Rocks;

public static class EmbeddedStringEncryption
{
    public static void InjectClass(AssemblyDefinition assembly)
    {
        var module = assembly.MainModule;
        var encryptionClass = new TypeDefinition("Embed", "EmbeddedStringEncryption",
            TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed,
            module.TypeSystem.Object);

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

        var onDecryptMethod = typeof(EmbeddedStringEncryption).GetMethod("OnDecrypt", new[] { typeof(byte[]), typeof(byte[]), typeof(byte[]) });
        ilProcessor.Append(ilProcessor.Create(OpCodes.Call, module.ImportReference(onDecryptMethod)));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Call, module.ImportReference(typeof(Encoding).GetProperty("UTF8").GetGetMethod())));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Callvirt, module.ImportReference(typeof(Encoding).GetMethod("GetString", new[] { typeof(byte[]) }))));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));

        encryptionClass.Methods.Add(decryptMethod);

        var onDecryptMethodDef = new MethodDefinition("OnDecrypt", MethodAttributes.Public | MethodAttributes.Static, module.TypeSystem.Byte.MakeArrayType());
        onDecryptMethodDef.Parameters.Add(new ParameterDefinition("bytes", ParameterAttributes.None, module.TypeSystem.Byte.MakeArrayType()));
        onDecryptMethodDef.Parameters.Add(new ParameterDefinition("key", ParameterAttributes.None, module.TypeSystem.Byte.MakeArrayType()));
        onDecryptMethodDef.Parameters.Add(new ParameterDefinition("iv", ParameterAttributes.None, module.TypeSystem.Byte.MakeArrayType()));

        ilProcessor = onDecryptMethodDef.Body.GetILProcessor();
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
        ilProcessor.Append(ilProcessor.Create(OpCodes.Newobj, module.ImportReference(typeof(CryptoStream).GetConstructor(new[] { typeof(Stream), typeof(ICryptoTransform), typeof(CryptoStreamMode) }))));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Stloc_2));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldloc_2));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_0));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_I4_0));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_0));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldlen));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Conv_I4));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Callvirt, module.ImportReference(typeof(CryptoStream).GetMethod("Write", new[] { typeof(byte[]), typeof(int), typeof(int) }))));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldloc_2));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Callvirt, module.ImportReference(typeof(CryptoStream).GetMethod("FlushFinalBlock", Type.EmptyTypes))));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldloc_1));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Callvirt, module.ImportReference(typeof(MemoryStream).GetMethod("ToArray", Type.EmptyTypes))));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));

        encryptionClass.Methods.Add(onDecryptMethodDef);
        module.Types.Add(encryptionClass);
    }

    public static string DecryptString(string encryptedText, string keyString, string ivString)
    {
        byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
        byte[] key = Convert.FromBase64String(keyString);
        byte[] iv = Convert.FromBase64String(ivString);
        byte[] decryptedBytes = OnDecrypt(encryptedBytes, key, iv);
        return Encoding.UTF8.GetString(decryptedBytes);
    }

    public static byte[] OnDecrypt(byte[] bytes, byte[] key, byte[] iv)
    {
        using Aes aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        using MemoryStream memory = new();
        using (CryptoStream crypto = new(memory, aes.CreateDecryptor(), CryptoStreamMode.Write))
        {
            crypto.Write(bytes, 0, bytes.Length);
            crypto.FlushFinalBlock();
        }
        return memory.ToArray();
    }

}
