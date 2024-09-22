using Mono.Cecil;
using Mono.Cecil.Cil;
using OBF.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBF;
public enum SystemType
{
    Void,
    Int,
    Float,
    Double,
    String,
    Bool,
    Byte,
    Char,
    IntPtr,
    Int16,
    Int32,
    Int64,
    Object,
    SByte,
    UInt16,
    UInt32,
    UInt64,
    UIntPtr,
    TypeReference
}
public static class Extensions
{
    public static TypeReference GetSystemType(this ModuleDefinition module, SystemType field)
    {
        return field switch
        {
            SystemType.Int => module.TypeSystem.Int32,
            SystemType.Float => module.TypeSystem.Single,
            SystemType.String => module.TypeSystem.String,
            SystemType.Bool => module.TypeSystem.Boolean,
            SystemType.Byte => module.TypeSystem.Byte,
            SystemType.Double => module.TypeSystem.Double,
            SystemType.Void => module.TypeSystem.Void,
            SystemType.Char => module.TypeSystem.Char,
            SystemType.Int16 => module.TypeSystem.Int16,
            SystemType.Int32 => module.TypeSystem.Int32,
            SystemType.Int64 => module.TypeSystem.Int64,
            SystemType.UInt16 => module.TypeSystem.UInt16,
            SystemType.UInt32 => module.TypeSystem.UInt32,
            SystemType.UInt64 => module.TypeSystem.UInt64,
            SystemType.SByte => module.TypeSystem.SByte,
            SystemType.UIntPtr => module.TypeSystem.UIntPtr,
            SystemType.IntPtr => module.TypeSystem.IntPtr,
            SystemType.Object => module.TypeSystem.Object,
            SystemType.TypeReference => module.TypeSystem.TypedReference,
            _ => module.TypeSystem.Object,
        };
    }
    public static void GetReturnType(this ILProcessor ilProcessor, SystemType field)
    {
        Random random = new Random();
        switch (field)
        {
            case SystemType.Void:
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));
                break;
            case SystemType.Int:
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_I4, random.Next()));
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));
                break;
            case SystemType.Float:
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_R4, (float)random.NextDouble() * 100));
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));
                break;
            case SystemType.Double:
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_R8, random.NextDouble() * 100));
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));
                break;
            case SystemType.String:
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ldstr, Guid.NewGuid().ToString()));
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));
                break;
            case SystemType.Bool:
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_I4, random.Next(2))); // true or false
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));
                break;
            case SystemType.Byte:
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_I4_S, (sbyte)random.Next(256)));
                ilProcessor.Append(ilProcessor.Create(OpCodes.Conv_U1)); // Convert to byte
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));
                break;
            case SystemType.Char:
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_I4, random.Next(65, 91))); // A-Z
                ilProcessor.Append(ilProcessor.Create(OpCodes.Conv_U2)); // Convert to char
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));
                break;
            case SystemType.IntPtr:
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_I4, random.Next()));
                ilProcessor.Append(ilProcessor.Create(OpCodes.Conv_I)); // Convert to IntPtr
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));
                break;
            case SystemType.Int16:
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_I4, random.Next(short.MinValue, short.MaxValue)));
                ilProcessor.Append(ilProcessor.Create(OpCodes.Conv_I2)); // Convert to Int16
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));
                break;
            case SystemType.Int32:
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_I4, random.Next()));
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));
                break;
            case SystemType.Int64:
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_I8, (long)random.Next() << 32 | (long)random.Next()));
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));
                break;
            case SystemType.Object:
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ldnull));
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));
                break;
            case SystemType.SByte:
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_I4_S, (sbyte)random.Next(sbyte.MinValue, sbyte.MaxValue)));
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));
                break;
            case SystemType.UInt16:
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_I4, random.Next(ushort.MaxValue)));
                ilProcessor.Append(ilProcessor.Create(OpCodes.Conv_U2)); // Convert to UInt16
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));
                break;
            case SystemType.UInt32:
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_I4, random.Next()));
                ilProcessor.Append(ilProcessor.Create(OpCodes.Conv_U4)); // Convert to UInt32
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));
                break;
            case SystemType.UInt64:
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_I8, (ulong)random.Next() << 32 | (ulong)random.Next()));
                ilProcessor.Append(ilProcessor.Create(OpCodes.Conv_U8)); // Convert to UInt64
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));
                break;
            case SystemType.UIntPtr:
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_I4, random.Next()));
                ilProcessor.Append(ilProcessor.Create(OpCodes.Conv_U)); // Convert to UIntPtr
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));
                break;
            case SystemType.TypeReference:
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ldnull)); // Return null for TypeReference
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));
                break;
            default:
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));
                break;
        }
    }
    internal static SystemType SysRng(int startRange = 0, int endRange = 7)
    {
        Random random = new();
        return (SystemType)random.Next(startRange, endRange);
    }
    internal static int IntRng(int startRange = 0, int endRange = 4)
    {
        Random random = new();
        return random.Next(startRange, endRange);
    }
    public static bool IsDelegate(this TypeDefinition type)
    {
        return type.BaseType != null && type.BaseType.FullName.Contains("MulticastDelegate");
    }
    public static bool IsCompilerGenerated(this TypeDefinition type)
    {
        return type.Name.Contains('<') || type.Name.Contains('>');
    }
}
