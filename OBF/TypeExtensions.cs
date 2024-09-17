using Mono.Cecil;
using OBF.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBF;
public enum SystemType
{
    Int,
    Float,
    Double,
    String,
    Bool,
    Void,
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
public static class TypeExtensions
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
}
