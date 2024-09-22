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
