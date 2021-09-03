using System;
using System.Numerics;

namespace DigBuild.Platform.Util
{
    /// <summary>
    /// A numeric type.
    /// </summary>
    public enum NumericType : byte
    {
        Byte, UByte,
        Short, UShort,
        Int, UInt,
        Long, ULong,
        Float, Double,
        Float2, Float3, Float4,
        Float4x4
    }

    /// <summary>
    /// Helper to convert C# types into numeric types.
    /// </summary>
    public static class NumericTypeHelper
    {
        /// <summary>
        /// Gets the numeric type of the type argument.
        /// </summary>
        /// <typeparam name="T">The type</typeparam>
        /// <returns>The numeric type</returns>
        public static NumericType GetType<T>() where T : unmanaged => GetType(typeof(T));

        /// <summary>
        /// Gets the numeric type of the C# type.
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns>The numeric type</returns>
        public static NumericType GetType(Type type)
        {
            if (type == typeof(sbyte))
                return NumericType.Byte;
            if (type == typeof(byte))
                return NumericType.UByte;
            if (type == typeof(short))
                return NumericType.Short;
            if (type == typeof(ushort))
                return NumericType.UShort;
            if (type == typeof(int))
                return NumericType.Int;
            if (type == typeof(uint))
                return NumericType.UInt;
            if (type == typeof(long))
                return NumericType.Long;
            if (type == typeof(ulong))
                return NumericType.ULong;
            if (type == typeof(float))
                return NumericType.Float;
            if (type == typeof(double))
                return NumericType.Double;
            if (type == typeof(Vector2))
                return NumericType.Float2;
            if (type == typeof(Vector3))
                return NumericType.Float3;
            if (type == typeof(Vector4))
                return NumericType.Float4;
            if (type == typeof(Matrix4x4))
                return NumericType.Float4x4;
            throw new ArgumentException($"The type must be a numeric type. Got: {type.Name}", nameof(type));
        }
    }
}