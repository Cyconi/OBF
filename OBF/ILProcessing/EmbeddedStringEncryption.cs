using Mono.Cecil.Cil;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil.Rocks;


namespace Embed;

public static class EmbeddedStringEncryptionOG
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


// Token: 0x0200000D RID: 13
public static class EmbeddedStringEncryption
{
    // Token: 0x0600002D RID: 45 RVA: 0x000037E8 File Offset: 0x000019E8
    public static byte[] OnDecrypt(byte[] bytes, byte[] key, byte[] iv)
    {
        Aes aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        MemoryStream memoryStream = new MemoryStream();
        CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Write);
        cryptoStream.Write(bytes, 0, bytes.Length);
        cryptoStream.FlushFinalBlock();
        return memoryStream.ToArray();
    }

    // Token: 0x0600002E RID: 46 RVA: 0x00003834 File Offset: 0x00001A34
    public static string DecryptString(string encryptedText, string keyString, string ivString)
    {
        byte[] array = Convert.FromBase64String(encryptedText);
        byte[] array2 = Convert.FromBase64String(keyString);
        byte[] array3 = Convert.FromBase64String(ivString);
        byte[] array4 = EmbeddedStringEncryption.OnDecrypt(array, array2, array3);
        return Encoding.UTF8.GetString(array4);
    }
}
