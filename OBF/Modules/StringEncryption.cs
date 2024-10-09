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
    private static MethodDefinition getMethod;
    private static FieldDefinition initializedField;
    private static FieldDefinition listField;
    private static MethodDefinition initializeMethod;
    private static TypeDefinition type;
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
        type = new TypeDefinition("Embed", "StringEncryption", TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.Abstract, assembly.MainModule.ImportReference(typeof(object)));

        initializedField = new FieldDefinition("initialized", FieldAttributes.Private | FieldAttributes.Static, assembly.MainModule.ImportReference(typeof(bool)));
        listField = new FieldDefinition("list", FieldAttributes.Private | FieldAttributes.Static, assembly.MainModule.ImportReference(typeof(List<string>)));
        initializeMethod = new MethodDefinition("Initialize", MethodAttributes.Private | MethodAttributes.Static, assembly.MainModule.ImportReference(typeof(void)));
        type.Methods.Add(initializeMethod);
        type.Fields.Add(initializedField);
        type.Fields.Add(listField);

        getMethod = new MethodDefinition("Get", MethodAttributes.Public | MethodAttributes.Static, assembly.MainModule.ImportReference(typeof(string)));
        getMethod.Parameters.Add(new ParameterDefinition(assembly.MainModule.ImportReference(typeof(int))));

        type.Methods.Add(getMethod);

        {
            var processor = getMethod.Body.GetILProcessor();
            processor.Body.InitLocals = true;
            processor.Body.Variables.Add(new VariableDefinition(assembly.MainModule.ImportReference(typeof(string))));
            processor.Body.Variables.Add(new VariableDefinition(assembly.MainModule.ImportReference(typeof(bool))));

            var instructions = processor.Body.Instructions;

            // Add instructions
            processor.Emit(OpCodes.Ldsfld, initializedField);
            processor.Emit(OpCodes.Stloc_1);
            processor.Emit(OpCodes.Ldloc_1);

            // Placeholder for Brtrue_S
            var brtrueInstruction = processor.Create(OpCodes.Nop);
            processor.Append(brtrueInstruction);

            processor.Emit(OpCodes.Call, initializeMethod);
            processor.Emit(OpCodes.Ldsfld, listField);
            processor.Emit(OpCodes.Ldarg_0);
            processor.Emit(OpCodes.Callvirt, assembly.MainModule.ImportReference(typeof(List<string>).GetMethod("get_Item")));
            processor.Emit(OpCodes.Stloc_0);
            processor.Emit(OpCodes.Ldloc_0);
            processor.Emit(OpCodes.Ret);

            // Replace placeholder with Brtrue_S
            processor.Replace(brtrueInstruction, processor.Create(OpCodes.Brtrue_S, instructions[4]));
        }
    }
    private static void FinalizeHandler(AssemblyDefinition assembly)
    {
        var processor = initializeMethod.Body.GetILProcessor();
        processor.Emit(OpCodes.Ldc_I4, FoundStrings.Count);
        processor.Emit(OpCodes.Newobj, assembly.MainModule.ImportReference(typeof(List<string>).GetConstructors()[1]));
        processor.Emit(OpCodes.Stsfld, listField);
        foreach (var str in FoundStrings)
        {
            var (encryptedString, keyString, ivString) = EncryptString(str);
            processor.Emit(OpCodes.Ldsfld, listField);
            processor.Emit(OpCodes.Ldstr, encryptedString);
            processor.Emit(OpCodes.Ldstr, keyString);
            processor.Emit(OpCodes.Ldstr, ivString);
            processor.Emit(OpCodes.Callvirt, assembly.MainModule.ImportReference(typeof(List<string>).GetMethod("Add")));
        }
        processor.Emit(OpCodes.Ldc_I4_1);
        processor.Emit(OpCodes.Stsfld, initializedField);
        processor.Emit(OpCodes.Ret);
        assembly.MainModule.Types.Add(type);
    }
    private static void ProcessType(TypeDefinition type)
    {
        if (!type.HasMethods)
            return;

        foreach (var method in type.Methods)
            ProcessMethod(method);

        foreach (var nestedType in type.NestedTypes)
            ProcessType(nestedType);
    }
    private static void ProcessMethod(MethodDefinition method)
    {
        if (!method.HasBody)
            return;

        var processor = method.Body.GetILProcessor();
        var instructionsToReplace = new Dictionary<Instruction, int>();
        foreach (var instruction in method.Body.Instructions)
        {
            if (instruction.OpCode.Code != Code.Ldstr || instruction.Operand is not string)
                continue;

            FoundStrings.Add(instruction.Operand as string);
            instructionsToReplace.Add(instruction, FoundStrings.Count - 1);
        }

        foreach (var kvp in instructionsToReplace)
        {
            // Insert the call to getMethod
            var callInstruction = processor.Create(OpCodes.Call, getMethod);
            processor.InsertAfter(kvp.Key, callInstruction);

            // Replace the original instruction with ldc.i4
            var ldcInstruction = processor.Create(OpCodes.Ldc_I4, kvp.Value);
            processor.Replace(kvp.Key, ldcInstruction);
        }
    }
    private static (string encryptedString, string keyString, string ivString) EncryptString(string originalString)
    {
        byte[] originalBytes = Encoding.UTF8.GetBytes(originalString);
        using Aes aes = Aes.Create();
        aes.GenerateKey();
        aes.GenerateIV();
        using MemoryStream ms = new();
        using (CryptoStream cs = new(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
        {
            cs.Write(originalBytes, 0, originalBytes.Length);
            cs.FlushFinalBlock();
        }
        string encryptedString = Convert.ToBase64String(ms.ToArray());
        string keyString = Convert.ToBase64String(aes.Key);
        string ivString = Convert.ToBase64String(aes.IV);
        return (encryptedString, keyString, ivString);
    }
}