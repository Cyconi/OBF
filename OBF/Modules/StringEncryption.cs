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
    #region AES Decryption 
    public static void AddDecryptionMethod(AssemblyDefinition assembly)
    {
        try
        {
            // Get the main module of the assembly
            var module = assembly.MainModule;

            // Create the encryption class
            var encryptionClass = CreateEncryptionClass(module);

            // Create the OnDecrypt method and add it to the encryption class
            var onDecryptMethodDef = CreateOnDecryptMethod(module);
            encryptionClass.Methods.Add(onDecryptMethodDef);

            // Create the DecryptString method and add it to the encryption class
            var decryptMethod = CreateDecryptStringMethod(module, onDecryptMethodDef);
            encryptionClass.Methods.Add(decryptMethod);

            // Add the encryption class to the module's types
            module.Types.Add(encryptionClass);

            // Print a success message
            Console.WriteLine("Decryption methods added successfully.");
        }
        catch (Exception ex)
        {
            // Print an error message and rethrow the exception
            Console.WriteLine($"Error adding decryption methods: {ex.Message}");
            throw;
        }
    }
    private static TypeDefinition CreateEncryptionClass(ModuleDefinition module)
    {
        // Create a new type definition for the encryption class
        return new TypeDefinition("Embed", "EmbeddedStringEncryption",
            TypeAttributes.NotPublic | TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed,
            module.TypeSystem.Object);
    }
    private static MethodDefinition CreateOnDecryptMethod(ModuleDefinition module)
    {
        // Create a new method definition for the OnDecrypt method
        var method = new MethodDefinition("OnDecrypt",
            MethodAttributes.Public | MethodAttributes.Static,
            module.TypeSystem.Byte.MakeArrayType());

        // Add parameters to the OnDecrypt method
        method.Parameters.Add(new ParameterDefinition("bytes", ParameterAttributes.None, module.TypeSystem.Byte.MakeArrayType()));
        method.Parameters.Add(new ParameterDefinition("key", ParameterAttributes.None, module.TypeSystem.Byte.MakeArrayType()));
        method.Parameters.Add(new ParameterDefinition("iv", ParameterAttributes.None, module.TypeSystem.Byte.MakeArrayType()));

        // Get the IL processor for the method body
        var ilProcessor = method.Body.GetILProcessor();

        // Add local variables to the method body
        ilProcessor.Body.Variables.Add(new VariableDefinition(module.ImportReference(typeof(Aes))));
        ilProcessor.Body.Variables.Add(new VariableDefinition(module.ImportReference(typeof(MemoryStream))));
        ilProcessor.Body.Variables.Add(new VariableDefinition(module.ImportReference(typeof(CryptoStream))));

        // Create and initialize the Aes object
        ilProcessor.Append(ilProcessor.Create(OpCodes.Call, module.ImportReference(typeof(Aes).GetMethod("Create", Type.EmptyTypes))));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Stloc_0));

        // Set the key for the Aes object
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldloc_0));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_1));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Callvirt, module.ImportReference(typeof(Aes).GetProperty("Key")?.GetSetMethod())));

        // Set the IV for the Aes object
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldloc_0));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_2));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Callvirt, module.ImportReference(typeof(Aes).GetProperty("IV")?.GetSetMethod())));

        // Create and initialize the MemoryStream object
        ilProcessor.Append(ilProcessor.Create(OpCodes.Newobj, module.ImportReference(typeof(MemoryStream).GetConstructor(Type.EmptyTypes))));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Stloc_1));

        // Create and initialize the CryptoStream object
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldloc_1));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldloc_0));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Callvirt, module.ImportReference(typeof(Aes).GetMethod("CreateDecryptor", Type.EmptyTypes))));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_I4_1));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Newobj, module.ImportReference(typeof(CryptoStream).GetConstructor([typeof(Stream), typeof(ICryptoTransform), typeof(CryptoStreamMode)]))));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Stloc_2));

        // Write the encrypted data to the CryptoStream
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldloc_2));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_0));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_I4_0));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_0));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldlen));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Conv_I4));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Callvirt, module.ImportReference(typeof(CryptoStream).GetMethod("Write", new[] { typeof(byte[]), typeof(int), typeof(int) }))));

        // Flush the final block of the CryptoStream
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldloc_2));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Callvirt, module.ImportReference(typeof(CryptoStream).GetMethod("FlushFinalBlock", Type.EmptyTypes))));

        // Convert the decrypted data in the MemoryStream to a byte array
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldloc_1));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Callvirt, module.ImportReference(typeof(MemoryStream).GetMethod("ToArray", Type.EmptyTypes))));

        // Return the decrypted byte array
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));

        return method;
    }
    private static MethodDefinition CreateDecryptStringMethod(ModuleDefinition module, MethodReference onDecryptMethod)
    {
        // Create a new method definition for the DecryptString method
        var method = new MethodDefinition("DecryptString",
            MethodAttributes.Public | MethodAttributes.Static,
            module.TypeSystem.String);

        // Add parameters to the DecryptString method
        method.Parameters.Add(new ParameterDefinition("encryptedText", ParameterAttributes.None, module.TypeSystem.String));
        method.Parameters.Add(new ParameterDefinition("keyString", ParameterAttributes.None, module.TypeSystem.String));
        method.Parameters.Add(new ParameterDefinition("ivString", ParameterAttributes.None, module.TypeSystem.String));

        // Get the IL processor for the method body
        var ilProcessor = method.Body.GetILProcessor();

        // Add local variables to the method body
        ilProcessor.Body.Variables.Add(new VariableDefinition(module.TypeSystem.Byte.MakeArrayType()));
        ilProcessor.Body.Variables.Add(new VariableDefinition(module.TypeSystem.Byte.MakeArrayType()));
        ilProcessor.Body.Variables.Add(new VariableDefinition(module.TypeSystem.Byte.MakeArrayType()));
        ilProcessor.Body.Variables.Add(new VariableDefinition(module.TypeSystem.Byte.MakeArrayType()));

        // Convert the encrypted text from Base64 to a byte array
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_0));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Call, module.ImportReference(typeof(Convert).GetMethod("FromBase64String", [typeof(string)]))));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Stloc_0));

        // Convert the key string from Base64 to a byte array
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_1));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Call, module.ImportReference(typeof(Convert).GetMethod("FromBase64String", [typeof(string)]))));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Stloc_1));

        // Convert the IV string from Base64 to a byte array
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_2));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Call, module.ImportReference(typeof(Convert).GetMethod("FromBase64String", [typeof(string)]))));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Stloc_2));

        // Call the OnDecrypt method with the byte arrays
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldloc_0));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldloc_1));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldloc_2));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Call, onDecryptMethod));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Stloc_3));

        // Convert the decrypted byte array to a string using UTF8 encoding
        ilProcessor.Append(ilProcessor.Create(OpCodes.Call, module.ImportReference(typeof(Encoding).GetProperty("UTF8")?.GetGetMethod())));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldloc_3));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Callvirt, module.ImportReference(typeof(Encoding).GetMethod("GetString", [typeof(byte[])]))));

        // Return the decrypted string
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));

        return method;
    }
    #endregion
    #region Handler
    public static List<string> FoundStrings { get; private set; } = [];
    private static MethodDefinition? getMethod;
    private static FieldDefinition? initField;
    private static FieldDefinition? listField;
    private static MethodDefinition? initMethod;
    private static TypeDefinition? typeHandle;
    private static void AddHandler(AssemblyDefinition assembly)
    {
        // Define a new type called "StringEncryption" with various attributes
        typeHandle = new TypeDefinition("Embed", "StringEncryption", TypeAttributes.NotPublic | TypeAttributes.Sealed | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.Abstract, assembly.MainModule.ImportReference(typeof(object)));

        // Define a private static boolean field named "initialized"
        initField = new FieldDefinition("initialized", FieldAttributes.Private | FieldAttributes.Static, assembly.MainModule.ImportReference(typeof(bool)));

        // Define a private static field named "dictionary" of type Dictionary<int, (string, string, string)>
        var dictionaryType = assembly.MainModule.ImportReference(typeof(Dictionary<,>))
            .MakeGenericInstanceType(assembly.MainModule.TypeSystem.Int32,
                assembly.MainModule.ImportReference(typeof(ValueTuple<,,>))
            .MakeGenericInstanceType(assembly.MainModule.TypeSystem.String, 
                assembly.MainModule.TypeSystem.String,
                assembly.MainModule.TypeSystem.String));

        var dictionaryField = new FieldDefinition("dictionary", FieldAttributes.Private | FieldAttributes.Static, dictionaryType);

        listField = new FieldDefinition("dictionary", FieldAttributes.Private | FieldAttributes.Static, dictionaryType);

        // Define a private static method named "Initialize"
        initMethod = new MethodDefinition("Initialize", MethodAttributes.Private | MethodAttributes.Static, assembly.MainModule.ImportReference(typeof(void)));

        // Add the "Initialize" method to the type
        typeHandle.Methods.Add(initMethod);

        // Add the "initialized" and "dictionary" fields to the type
        typeHandle.Fields.Add(initField);
        typeHandle.Fields.Add(listField);

        // Define a public static method named "Get" that takes an integer parameter and returns a string
        getMethod = new MethodDefinition("Get", MethodAttributes.Public | MethodAttributes.Static, assembly.MainModule.ImportReference(typeof(string)));
        getMethod.Parameters.Add(new ParameterDefinition(assembly.MainModule.ImportReference(typeof(int))));

        // Add the "Get" method to the type
        typeHandle.Methods.Add(getMethod);

        // Get the IL processor for the "Get" method
        var processor = getMethod.Body.GetILProcessor();
        processor.Body.InitLocals = true;

        // Define local variables for the method
        processor.Body.Variables.Add(new VariableDefinition(assembly.MainModule.ImportReference(typeof(string))));
        processor.Body.Variables.Add(new VariableDefinition(assembly.MainModule.ImportReference(typeof(bool))));
        processor.Body.Variables.Add(new VariableDefinition(assembly.MainModule.ImportReference(typeof(ValueTuple<string, string, string>))));

        var instructions = processor.Body.Instructions;

        // Check if the dictionary is initialized
        processor.Emit(OpCodes.Ldsfld, initField); // Load the value of the "initialized" field onto the stack
        processor.Emit(OpCodes.Stloc_1); // Store the value in the local variable at index 1 (bool)
        processor.Emit(OpCodes.Ldloc_1); // Load the value of the local variable at index 1 onto the stack

        // Placeholder for Brtrue_S
        var brtrueInstruction = processor.Create(OpCodes.Nop); // Create a placeholder instruction
        processor.Append(brtrueInstruction); // Append the placeholder instruction

        // Call Initialize method if not initialized
        processor.Emit(OpCodes.Call, initMethod); // Call the "Initialize" method

        // Retrieve the tuple from the dictionary
        processor.Emit(OpCodes.Ldsfld, listField); // Load the value of the "dictionary" field onto the stack
        processor.Emit(OpCodes.Ldarg_0); // Load the first argument (index) onto the stack
        processor.Emit(OpCodes.Callvirt, assembly.MainModule.ImportReference(typeof(Dictionary<int, (string, string, string)>).GetMethod("get_Item"))); // Call the "get_Item" method of the dictionary
        processor.Emit(OpCodes.Stloc_2); // Store the retrieved tuple in the local variable at index 2

        // Decrypt the string
        var decryptStringType = assembly.MainModule.ImportReference(Type.GetType("Embed.EmbeddedStringEncryption"));
        var decryptStringMethod = decryptStringType.Resolve().Methods.First(m => m.Name == "DecryptString" && m.Parameters.Count == 3);
        processor.Emit(OpCodes.Ldloca, 2); // Load the address of the tuple onto the stack
        processor.Emit(OpCodes.Ldfld, assembly.MainModule.ImportReference(typeof(ValueTuple<string, string, string>).GetField("Item1"))); // Load the encrypted string from the tuple
        processor.Emit(OpCodes.Ldloca, 2); // Load the address of the tuple onto the stack
        processor.Emit(OpCodes.Ldfld, assembly.MainModule.ImportReference(typeof(ValueTuple<string, string, string>).GetField("Item2"))); // Load the key string from the tuple
        processor.Emit(OpCodes.Ldloca, 2); // Load the address of the tuple onto the stack
        processor.Emit(OpCodes.Ldfld, assembly.MainModule.ImportReference(typeof(ValueTuple<string, string, string>).GetField("Item3"))); // Load the IV string from the tuple
        processor.Emit(OpCodes.Call, assembly.MainModule.ImportReference(decryptStringMethod)); // Call the "DecryptString" method
        processor.Emit(OpCodes.Stloc_0); // Store the decrypted string in the local variable at index 0 (string)

        // Return the decrypted string
        processor.Emit(OpCodes.Ldloc_0); // Load the decrypted string onto the stack
        processor.Emit(OpCodes.Ret); // Return the value on the stack

        // Replace placeholder with Brtrue_S
        processor.Replace(brtrueInstruction, processor.Create(OpCodes.Brtrue_S, instructions[5])); // Replace the placeholder with a conditional branch instruction (places StringEncryption.Initialize(); inside the if statement)
    }
    private static void FinalizeHandler(AssemblyDefinition assembly)
    {
        // Get the IL processor for the "Initialize" method's body
        var processor = initMethod?.Body.GetILProcessor();
        if (processor == null)
            return; // Return if the processor is null

        // Load the count of FoundStrings onto the stack
        processor.Emit(OpCodes.Ldc_I4, FoundStrings.Count);

        // Create a new Dictionary<int, (string, string, string)> instance with the specified capacity
        var dictionaryCtor = typeof(Dictionary<int, (string, string, string)>).GetConstructor(new[] { typeof(int) });
        processor.Emit(OpCodes.Newobj, assembly.MainModule.ImportReference(dictionaryCtor));

        // Store the new Dictionary<int, (string, string, string)> instance in the static field "dictionary"
        processor.Emit(OpCodes.Stsfld, listField);

        // Iterate over each string in FoundStrings
        for (int i = 0; i < FoundStrings.Count; i++)
        {
            var str = FoundStrings[i];

            // Encrypt the string and get the encrypted string, key string, and IV string
            var (encryptedString, keyString, ivString) = EncryptString(str);

            // Add the encrypted string, key string, and IV string to the dictionary
            processor.Emit(OpCodes.Ldsfld, listField); // Load the dictionary field onto the stack
            processor.Emit(OpCodes.Ldc_I4, i); // Load the index onto the stack
            processor.Emit(OpCodes.Ldstr, encryptedString); // Load the encrypted string onto the stack
            processor.Emit(OpCodes.Ldstr, keyString); // Load the key string onto the stack
            processor.Emit(OpCodes.Ldstr, ivString); // Load the IV string onto the stack
            var tupleCtor = typeof(ValueTuple<string, string, string>).GetConstructor(new[] { typeof(string), typeof(string), typeof(string) });
            processor.Emit(OpCodes.Newobj, assembly.MainModule.ImportReference(tupleCtor)); // Create a new tuple with the encrypted string, key string, and IV string
            processor.Emit(OpCodes.Callvirt, assembly.MainModule.ImportReference(typeof(Dictionary<int, (string, string, string)>).GetMethod("Add"))); // Call the "Add" method of the dictionary
        }

        // Set the "initialized" field to true
        processor.Emit(OpCodes.Ldc_I4_1); // Load the constant value 1 onto the stack
        processor.Emit(OpCodes.Stsfld, initField); // Store the value in the "initialized" field

        // Return from the method
        processor.Emit(OpCodes.Ret); // Return from the method

        // Add the type to the assembly's module types
        assembly.MainModule.Types.Add(typeHandle);
    }
    #endregion
    public static void EncryptStrings(AssemblyDefinition assembly)
    {
        var types = assembly.MainModule.Types;

        AddDecryptionMethod(assembly); // works
        AddHandler(assembly); // works

        foreach (var type in types)        
            ProcessType(type);        

        FinalizeHandler(assembly);
    }
    private static void ProcessType(TypeDefinition type)
    {
        if (!type.HasMethods)
            return;

        foreach (var method in type.Methods)
            ProcessMethod(method);

        foreach (var nestedType in type.NestedTypes)
            ProcessType(nestedType); // Process nested types recursively
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