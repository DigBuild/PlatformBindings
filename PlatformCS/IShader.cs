using System;
using System.Runtime.CompilerServices;

namespace DigBuildPlatformCS
{
    public interface IShader { }

    public sealed class VertexShader<TUniform> : IShader { }

    public sealed class FragmentShader<TUniform> : IShader { }

    public sealed class Uniform : Attribute
    {
        internal readonly int Order;

        public Uniform([CallerLineNumber] int order = 0)
        {
            Order = order;
        }
    }

    public readonly ref struct ShaderBuilder<TShader> where TShader : IShader
    {
        public static implicit operator TShader(ShaderBuilder<TShader> builder) => throw new NotImplementedException();
    }
}